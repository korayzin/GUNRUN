using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HolographicWeaponHUD : MonoBehaviour
{
    [Header("Anchoring")]
    [SerializeField] private Transform leftHandAnchor;
    [Tooltip("Sahnede verdiğin Transform (position/rotation/scale) oyunda da kullanılsın. Açıksa aşağıdaki offset/rotation/scale yok sayılır.")]
    [SerializeField] private bool useSceneTransform = true;
    [Tooltip("World Space canvas scale. Keep small (0.005-0.02) so HUD stays wrist-sized. Sadece Use Scene Transform kapalıyken kullanılır.")]
    [SerializeField] private float hudScale = 0.01f;
    [Tooltip("Local position offset from hand anchor (bilek üzerinde konum). Sadece Use Scene Transform kapalıyken kullanılır.")]
    [SerializeField] private Vector3 localPositionOffset = new Vector3(0.02f, 0f, 0.06f);
    [Tooltip("Local rotation: bileği saran eğimli ekran. Sadece Use Scene Transform kapalıyken kullanılır.")]
    [SerializeField] private Vector3 localRotationEuler = new Vector3(-50f, 12f, -55f);

    [Header("Weapon Manager & Data")]
    [SerializeField] private WeaponManager weaponManager;
    [Tooltip("Optional. If empty, sprites are taken from WeaponManager's UI Images.")]
    [SerializeField] private Sprite[] weaponIcons = new Sprite[9];

    [Header("Center (Current Weapon)")]
    [SerializeField] private Image centerWeaponImage;
    [SerializeField] private TextMeshProUGUI centerWeaponName;

    [Header("Left Slot (Previous Weapon)")]
    [SerializeField] private Image leftSlotImage;
    [SerializeField] private Button leftButton;

    [Header("Right Slot (Next Weapon)")]
    [SerializeField] private Image rightSlotImage;
    [SerializeField] private Button rightButton;

    [Header("Warning (Top-Left)")]
    [SerializeField] private TextMeshProUGUI warningLabel;
    [Tooltip("Tüm silahlar açıldığında (9/9) gösterilir. Aksi halde 'GUN REMAIN X' kullanılır.")]
    [SerializeField] private string allUnlockedText = "YOU CAN SCROLL THE GUNS";

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sol Controller Joystick")]
    [Tooltip("Sol joystick ile silah değiştirme (sadece tüm silahlar açıksa).")]
    [SerializeField] private bool useLeftJoystick = true;
    [Tooltip("Joystick bu eşiği geçince tetiklenir (0.3–0.8).")]
    [SerializeField] private float joystickThreshold = 0.5f;
    [Tooltip("Tekrar tetik için joystick bu değere dönmeli (neutral).")]
    [SerializeField] private float joystickNeutralDeadzone = 0.2f;

    [Header("Sol Kontrolcü X ile Aç/Kapa (Pop)")]
    [Tooltip("Sol VR kontrolcüsü X tuşu ile HUD açılıp kapanır.")]
    [SerializeField] private bool toggleWithXButton = true;
    [Tooltip("Pop animasyon süresi (saniye).")]
    [SerializeField] private float popDuration = 0.2f;
    [Tooltip("Açılışta hafif overshoot (pop) için eğri. 1 üzeri değer = hafif büyüyüp geri gelir.")]
    [SerializeField] private AnimationCurve popOpenCurve = new AnimationCurve(
        new Keyframe(0f, 0f), new Keyframe(0.6f, 1.12f), new Keyframe(1f, 1f));
    [Tooltip("Kapanış animasyon eğrisi.")]
    [SerializeField] private AnimationCurve popCloseCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private int _lastIndex = -1;
    private bool _isAnimating;
    private bool _joystickNeutral = true;
    private bool _isHudVisible = true;
    private bool _isPopAnimating;
    private Vector3 _baseScale;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    /// <summary>Sahnedeki local position/rotation/scale - her zaman bu değerler korunur.</summary>
    private Vector3 _sceneLocalPosition;
    private Quaternion _sceneLocalRotation;
    private Vector3 _sceneLocalScale;

    private void Awake()
    {
        _sceneLocalPosition = transform.localPosition;
        _sceneLocalRotation = transform.localRotation;
        _sceneLocalScale = transform.localScale;
    }

    /// <summary>Sahnedeki konum ve boyutu uygular - her zaman aynı kalır.</summary>
    private void ApplySceneTransform()
    {
        transform.localPosition = _sceneLocalPosition;
        transform.localRotation = _sceneLocalRotation;
        transform.localScale = _sceneLocalScale;
    }

    /// <summary>newtutorial sahnesinde HUD sahne konumunda kalır, sol ele taşınmaz.</summary>
    private bool IsNewTutorialScene => SceneManager.GetActiveScene().name == "newtutorial";

    /// <summary>Sol el anchor'ı OVRCameraRig veya sahneden bulur, HUD'u ona parent eder. newtutorial'da atlanır.</summary>
    private void ResolveAndParentToLeftHand()
    {
        if (IsNewTutorialScene) return;
        if (leftHandAnchor != null)
        {
            transform.SetParent(leftHandAnchor, true);
            if (useSceneTransform)
            {
                transform.localPosition = _sceneLocalPosition;
                transform.localRotation = _sceneLocalRotation;
                transform.localScale = _sceneLocalScale;
            }
            else
            {
                transform.localPosition = localPositionOffset;
                transform.localRotation = Quaternion.Euler(localRotationEuler);
                transform.localScale = Vector3.one * hudScale;
            }
            return;
        }
        var ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            if (ovrRig.leftHandOnControllerAnchor != null)
            {
                leftHandAnchor = ovrRig.leftHandOnControllerAnchor;
                transform.SetParent(leftHandAnchor, true);
                if (useSceneTransform)
                {
                    transform.localPosition = _sceneLocalPosition;
                    transform.localRotation = _sceneLocalRotation;
                    transform.localScale = _sceneLocalScale;
                }
                else
                {
                    transform.localPosition = localPositionOffset;
                    transform.localRotation = Quaternion.Euler(localRotationEuler);
                    transform.localScale = Vector3.one * hudScale;
                }
                return;
            }
            var ts = ovrRig.transform.Find("TrackingSpace");
            if (ts != null)
            {
                var anchor = ts.Find("LeftHandAnchor");
                if (anchor != null)
                {
                    leftHandAnchor = anchor;
                    transform.SetParent(leftHandAnchor, true);
                    if (useSceneTransform) ApplySceneTransform();
                    return;
                }
            }
            var direct = ovrRig.transform.Find("LeftHandAnchor");
            if (direct != null)
            {
                leftHandAnchor = direct;
                transform.SetParent(leftHandAnchor, true);
                if (useSceneTransform) ApplySceneTransform();
                return;
            }
        }
        var go = GameObject.Find("LeftHandAnchor");
        if (go != null)
        {
            leftHandAnchor = go.transform;
            transform.SetParent(leftHandAnchor, true);
            if (useSceneTransform) ApplySceneTransform();
            return;
        }
        go = GameObject.Find("LeftHand");
        if (go != null)
        {
            leftHandAnchor = go.transform;
            transform.SetParent(leftHandAnchor, true);
            if (useSceneTransform) ApplySceneTransform();
            return;
        }
        if (leftHandAnchor != null && !useSceneTransform)
        {
            transform.localPosition = localPositionOffset;
            transform.localRotation = Quaternion.Euler(localRotationEuler);
            transform.localScale = Vector3.one * hudScale;
        }
    }

    private void Start()
    {
        if (!IsNewTutorialScene) ResolveAndParentToLeftHand();
        _baseScale = useSceneTransform ? _sceneLocalScale : transform.localScale;
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        EnsureSpritesFromWeaponManager();
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += OnWeaponChanged;
            _lastIndex = weaponManager.CurrentWeaponIndex;
        }

        if (leftButton != null) leftButton.onClick.AddListener(OnLeftClick);
        if (rightButton != null) rightButton.onClick.AddListener(OnRightClick);

        RefreshDisplay(instant: true);
        UpdateButtonInteractable();
        UpdateWarningText();
    }

    private void LateUpdate()
    {
        if (_isPopAnimating) return;
        if (useSceneTransform)
        {
            transform.localPosition = _sceneLocalPosition;
            transform.localRotation = _sceneLocalRotation;
            transform.localScale = _isHudVisible ? _sceneLocalScale : Vector3.zero;
            return;
        }
        if (leftHandAnchor == null) return;
        if (transform.parent != leftHandAnchor) return;
        transform.localPosition = localPositionOffset;
        transform.localRotation = Quaternion.Euler(localRotationEuler);
        transform.localScale = _isHudVisible ? Vector3.one * hudScale : Vector3.zero;
    }

    private void OnDestroy()
    {
        if (weaponManager != null)
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
    }

    private void OnWeaponChanged(int newIndex)
    {
        if (_lastIndex == newIndex) return;
        int prevIndex = _lastIndex;
        _lastIndex = newIndex;
        RefreshDisplay(instant: false, fromIndex: prevIndex, toIndex: newIndex);
        UpdateButtonInteractable();
        UpdateWarningText();
    }

    private void Update()
    {
        if (GameManager.IsRetryScreenActive) return; // Retry ekranında sadece HandRayUIInteractor ile butonlara tıklanabilir
        if (toggleWithXButton && !_isPopAnimating && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
            ToggleHUD();

        if (weaponManager == null) return;
        int idx = weaponManager.CurrentWeaponIndex;
        if (_lastIndex != idx && !_isAnimating)
        {
            int prev = _lastIndex;
            _lastIndex = idx;
            RefreshDisplay(instant: false, fromIndex: prev, toIndex: idx);
            UpdateButtonInteractable();
            UpdateWarningText();
        }

        if (useLeftJoystick && weaponManager.AllWeaponsUnlocked)
            PollLeftJoystickWeaponSwitch();
    }

    /// <summary>Sol kontrolcü X ile çağrılır; HUD açık/kapalı durumunu pop animasyonu ile değiştirir.</summary>
    public void ToggleHUD()
    {
        if (_isPopAnimating) return;
        _isHudVisible = !_isHudVisible;
        StartCoroutine(PopAnimation());
    }

    /// <summary>Tutorial 12. diyalogda HUD'u açmak için. Kapalıysa açar ve pop animasyonu oynatır.</summary>
    public void EnsureVisible()
    {
        ResolveAndParentToLeftHand();
        gameObject.SetActive(true);
        StartCoroutine(EnsureVisibleDelayed());
    }

    private IEnumerator EnsureVisibleDelayed()
    {
        yield return null; // Bir frame bekle - Start() tamamlansın, _baseScale set edilsin
        if (_canvas == null) _canvas = GetComponent<Canvas>();
        if (_canvas != null)
        {
            _canvas.enabled = true;
            _canvas.sortingOrder = 100;
            if (_canvas.renderMode == RenderMode.WorldSpace && _canvas.worldCamera == null)
            {
                _canvas.worldCamera = Camera.main;
                if (_canvas.worldCamera == null)
                {
                    var ovr = FindObjectOfType<OVRCameraRig>();
                    if (ovr != null && ovr.centerEyeAnchor != null)
                    {
                        var cam = ovr.centerEyeAnchor.GetComponentInChildren<Camera>(true);
                        if (cam != null) _canvas.worldCamera = cam;
                    }
                }
            }
        }
        if (transform.parent != null) transform.SetAsLastSibling();
        if (_baseScale == Vector3.zero) _baseScale = useSceneTransform ? _sceneLocalScale : transform.localScale;
        if (_baseScale == Vector3.zero) _baseScale = useSceneTransform ? _sceneLocalScale : Vector3.one * hudScale;
        if (transform.localScale.sqrMagnitude < 0.0001f) transform.localScale = _baseScale;
        if (_isHudVisible) yield break;
        _isHudVisible = true;
        if (!_isPopAnimating) yield return StartCoroutine(PopAnimation());
    }

    /// <summary>Tutorial başında HUD'u gizlemek için.</summary>
    public void EnsureHidden()
    {
        _isHudVisible = false;
        gameObject.SetActive(false);
    }

    /// <summary>Tutorial: Sadece Canvas'ı aç/kapat. GameObject aktif kalır, HUD bileşeni çalışır. VR'da worldCamera ayarlanır.</summary>
    public void SetCanvasVisible(bool visible)
    {
        if (visible) gameObject.SetActive(true);
        var c = _canvas != null ? _canvas : GetComponent<Canvas>();
        if (c == null) return;
        c.enabled = visible;
        if (visible && c.renderMode == RenderMode.WorldSpace)
        {
            // VR'da World Space canvas için kamera gerekli - yoksa gözlükte görünmez (her açılışta ayarla)
            if (UICameraStackSetup.Instance != null && UICameraStackSetup.Instance.UIOverlayCamera != null)
                c.worldCamera = UICameraStackSetup.Instance.UIOverlayCamera;
            else
            {
                var ovr = FindObjectOfType<OVRCameraRig>();
                if (ovr != null && ovr.centerEyeAnchor != null)
                {
                    var cam = ovr.centerEyeAnchor.GetComponentInChildren<Camera>(true);
                    if (cam != null) c.worldCamera = cam;
                }
                if (c.worldCamera == null) c.worldCamera = Camera.main;
            }
            if (UICameraStackSetup.Instance != null)
                UICameraStackSetup.Instance.RegisterWorldSpaceCanvas(c);
            else
                UICameraStackSetup.SetLayerRecursivelyToUI(gameObject);
        }
    }

    private IEnumerator PopAnimation()
    {
        _isPopAnimating = true;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = _isHudVisible ? _baseScale : Vector3.zero;

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = _isHudVisible;
            _canvasGroup.interactable = _isHudVisible;
        }

        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            float curveT = _isHudVisible ? popOpenCurve.Evaluate(t) : popCloseCurve.Evaluate(t);
            transform.localScale = Vector3.LerpUnclamped(startScale, endScale, curveT);
            yield return null;
        }

        transform.localScale = endScale;
        _isPopAnimating = false;
    }

    private void PollLeftJoystickWeaponSwitch()
    {
        float x = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;

        if (Mathf.Abs(x) < joystickNeutralDeadzone)
            _joystickNeutral = true;

        if (!_joystickNeutral) return;

        if (x > joystickThreshold)
        {
            weaponManager.NextWeaponManual();
            _joystickNeutral = false;
        }
        else if (x < -joystickThreshold)
        {
            weaponManager.PreviousWeaponManual();
            _joystickNeutral = false;
        }
    }

    private void EnsureSpritesFromWeaponManager()
    {
        if (weaponManager == null) return;
        bool anyMissing = false;
        for (int i = 0; i < 9; i++)
        {
            if (weaponIcons == null || weaponIcons.Length <= i || weaponIcons[i] == null)
            {
                anyMissing = true;
                break;
            }
        }
        if (!anyMissing) return;

        if (weaponIcons == null || weaponIcons.Length != 9)
            weaponIcons = new Sprite[9];

        Image[] imgs = new Image[]
        {
            weaponManager.firstWeaponImage, weaponManager.secondWeaponImage, weaponManager.thirdWeaponImage,
            weaponManager.fourthWeaponImage, weaponManager.fifthWeaponImage, weaponManager.sixthWeaponImage,
            weaponManager.seventhWeaponImage, weaponManager.eighthWeaponImage, weaponManager.ninthWeaponImage
        };
        for (int i = 0; i < 9 && i < imgs.Length; i++)
        {
            if (imgs[i] != null && imgs[i].sprite != null && (weaponIcons[i] == null))
                weaponIcons[i] = imgs[i].sprite;
        }
    }

    private void OnLeftClick()
    {
        if (weaponManager != null && weaponManager.AllWeaponsUnlocked)
            weaponManager.PreviousWeaponManual();
    }

    private void OnRightClick()
    {
        if (weaponManager != null && weaponManager.AllWeaponsUnlocked)
            weaponManager.NextWeaponManual();
    }

    private void UpdateButtonInteractable()
    {
        bool canManual = weaponManager != null && weaponManager.AllWeaponsUnlocked;
        if (leftButton != null) leftButton.interactable = canManual;
        if (rightButton != null) rightButton.interactable = canManual;
    }

    private void UpdateWarningText()
    {
        if (warningLabel == null) return;
        if (weaponManager == null)
        {
            warningLabel.text = "GUN REMAIN 9";
            return;
        }
        if (weaponManager.AllWeaponsUnlocked)
        {
            warningLabel.text = allUnlockedText;
            return;
        }
        int remaining = 8 - weaponManager.CurrentWeaponIndex;
        warningLabel.text = "GUN REMAIN " + remaining;
    }

    private void RefreshDisplay(bool instant, int fromIndex = -1, int toIndex = -1)
    {
        if (weaponManager == null) return;
        int current = weaponManager.CurrentWeaponIndex;
        int prevSlot = (current - 1 + 9) % 9;
        int nextSlot = (current + 1) % 9;

        Sprite prevSprite = GetSprite(prevSlot);
        Sprite currSprite = GetSprite(current);
        Sprite nextSprite = GetSprite(nextSlot);
        string currName = GetWeaponName(current);

        if (leftSlotImage != null) leftSlotImage.sprite = prevSprite;
        if (rightSlotImage != null) rightSlotImage.sprite = nextSprite;

        if (instant)
        {
            if (centerWeaponImage != null) centerWeaponImage.sprite = currSprite;
            if (centerWeaponName != null) centerWeaponName.text = currName ?? "";
            return;
        }

        StartCoroutine(TransitionCenter(currSprite, currName));
    }

    private IEnumerator TransitionCenter(Sprite newSprite, string newName)
    {
        _isAnimating = true;
        float elapsed = 0f;

        if (centerWeaponImage != null)
        {
            Image im = centerWeaponImage;
            Vector3 startScale = im.rectTransform.localScale;
            Color startColor = im.color;

            while (elapsed < transitionDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.5f);
                float curveT = scaleCurve.Evaluate(t);
                im.rectTransform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, curveT);
                im.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), curveT);
                yield return null;
            }

            if (newSprite != null) im.sprite = newSprite;
            im.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
            im.rectTransform.localScale = startScale * 0.3f;

            elapsed = 0f;
            while (elapsed < transitionDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (transitionDuration * 0.5f);
                float curveT = scaleCurve.Evaluate(t);
                im.rectTransform.localScale = Vector3.Lerp(startScale * 0.3f, startScale, curveT);
                im.color = Color.Lerp(new Color(startColor.r, startColor.g, startColor.b, 0f), startColor, curveT);
                yield return null;
            }

            im.rectTransform.localScale = startScale;
            im.color = startColor;
        }

        if (centerWeaponName != null)
            centerWeaponName.text = newName ?? "";

        _isAnimating = false;
    }

    private Sprite GetSprite(int index)
    {
        if (weaponIcons == null || index < 0 || index >= weaponIcons.Length) return null;
        return weaponIcons[index];
    }

    private string GetWeaponName(int index)
    {
        if (index < 0 || index > 8) return "";
        if (GameBalanceManager.Instance != null)
        {
            var data = GameBalanceManager.Instance.GetWeaponData(index);
            if (!string.IsNullOrEmpty(data.weaponName))
                return data.weaponName;
        }
        if (weaponManager != null && weaponManager.weaponDisplayNames != null && index < weaponManager.weaponDisplayNames.Length)
        {
            string s = weaponManager.weaponDisplayNames[index];
            if (!string.IsNullOrEmpty(s)) return s;
        }
        return $"Weapon {index + 1}";
    }
}
