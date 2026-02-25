using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UserName sahnesi: Sadece VR tetikleyici ile Physics.Raycast + tek ray. Butonlar runtime'da BoxCollider + UserNameButtonHit ile kurulur.
/// </summary>
public class UserNameController : MonoBehaviour
{
    public const string UserNameSetKey = "UserNameSet";
    public const string PlayerNameKey = "PlayerName";
    private const int MaxNameLength = 20;
    private const float RayDistance = 50f;

    [Header("Sonraki sahne")]
    [SerializeField] private string nextSceneName = "UI";

    [Header("Cihaz başına bir kez")]
    [Tooltip("Açık: Daha önce isim kaydedildiyse bu sahne atlanır (cihaz başına 1 kez). Kapalı: Bu sahne her seferinde gösterilir.")]
    [SerializeField] private bool showOnlyOncePerDevice = true;

    [Header("İsim girişi")]
    [SerializeField] private TMP_InputField nameInputTMP;
    [SerializeField] private InputField nameInputLegacy;

    [Header("Butonlar")]
    [Tooltip("Üzerinde 'Random' yazan buton - basınca rastgele harf/sayı ismi üretir (4-6 karakter).")]
    [SerializeField] private Button randomButton;
    [SerializeField] private Button confirmButton;
    [Tooltip("Kapalı: OVRInputModule/GraphicRaycaster kullanıyorsanız collider eklenmez. Açık: Physics.Raycast için BoxCollider eklenir.")]
    [SerializeField] private bool setupButtonCollidersAtRuntime = true;

    [Header("Raycast - Elinden çıkan ray")]
    [Tooltip("Elimden çıkan rayin başlangıç noktası. OVR'da laser/pointer kullanan objeyi buraya sürükle. Boşsa otomatik RightHandAnchor aranır.")]
    [SerializeField] private Transform rayOrigin;
    [Tooltip("Ray atılacak katman (varsayılan = tümü)")]
    [SerializeField] private LayerMask raycastLayers = -1;
    [Tooltip("Ray görsel çizgisi (VR'da nereye baktığını gösterir)")]
    [SerializeField] private LineRenderer rayLine;

    [Header("Ray Görsel (HandRayUIInteractor ile aynı)")]
    [Tooltip("Ray rengi (normal)")]
    [SerializeField] private Color rayColor = new Color(0f, 1f, 1f, 0.8f);
    [Tooltip("Ray rengi (hover durumunda)")]
    [SerializeField] private Color rayHoverColor = new Color(0f, 1f, 0f, 1f);
    [Tooltip("Ray kalınlığı")]
    [SerializeField] private float rayWidth = 0.01f;

    private Camera _mainCam;
    private Button _currentHoveredButton;

    private void Start()
    {
        if (showOnlyOncePerDevice && PlayerPrefs.GetInt(UserNameSetKey, 0) == 1)
        {
            LoadNextScene();
            return;
        }

        randomButton?.onClick.AddListener(OnRandomClick);
        confirmButton?.onClick.AddListener(OnConfirmClick);

        _mainCam = Camera.main;
        if (rayOrigin == null)
            rayOrigin = FindHandRayOrigin();

        Canvas can = GetComponentInParent<Canvas>();
        if (can != null && can.renderMode == RenderMode.WorldSpace && _mainCam != null)
            can.worldCamera = _mainCam;

        if (setupButtonCollidersAtRuntime)
        {
            SetupButtonColliders();
            SetupInputFieldCollider();
        }
        if (rayOrigin != null)
        {
            SetupRayLine();
            DisableOtherRays();
        }
    }

    private void SetupRayLine()
    {
        if (rayLine == null)
        {
            rayLine = GetComponentInChildren<LineRenderer>();
            if (rayLine == null && rayOrigin != null)
            {
                var go = new GameObject("UserNameRayLine");
                go.transform.SetParent(rayOrigin);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                rayLine = go.AddComponent<LineRenderer>();
            }
        }
        if (rayLine == null) return;

        rayLine.positionCount = 2;
        rayLine.useWorldSpace = true;
        rayLine.startWidth = rayWidth;
        rayLine.endWidth = rayWidth * 0.5f;

        var shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        rayLine.material = new Material(shader);
        rayLine.material.color = rayColor;
        rayLine.startColor = rayColor;
        rayLine.endColor = rayColor;
    }

    /// <summary>
    /// Sahnede bizim ray dışındaki tüm LineRenderer'ları kapat (tek ray kalsın).
    /// </summary>
    private void DisableOtherRays()
    {
        var all = Object.FindObjectsOfType<LineRenderer>(true);
        foreach (var lr in all)
        {
            if (lr == rayLine) continue;
            lr.enabled = false;
        }
    }

    /// <summary>
    /// Her butona BoxCollider + UserNameButtonHit ekler - ray'in çarpabileceği 3D hedef oluşturur.
    /// </summary>
    private void SetupButtonColliders()
    {
        SetupButton(randomButton, UserNameButtonHit.Action.Random);
        SetupButton(confirmButton, UserNameButtonHit.Action.Confirm);
    }

    private void SetupButton(Button btn, UserNameButtonHit.Action action)
    {
        if (btn == null) return;
        var go = btn.gameObject;

        var hit = go.GetComponent<UserNameButtonHit>();
        if (hit == null)
        {
            hit = go.AddComponent<UserNameButtonHit>();
            hit.action = action;
        }

        var col = go.GetComponent<BoxCollider>();
        if (col == null)
            col = go.AddComponent<BoxCollider>();

        // Collider boyutu/merkezi sahnedeki sizin ayarlarınız kullanılır; kod ile üzerine yazılmaz.
        col.isTrigger = true;
    }

    /// <summary>
    /// İsim input alanına BoxCollider ekler - ray ile tıklanınca klavye açılır.
    /// </summary>
    private void SetupInputFieldCollider()
    {
        var go = nameInputTMP != null ? nameInputTMP.gameObject : (nameInputLegacy != null ? nameInputLegacy.gameObject : null);
        if (go == null) return;

        var col = go.GetComponent<BoxCollider>();
        if (col == null)
            col = go.AddComponent<BoxCollider>();

        // Collider boyutu/merkezi sahnedeki sizin ayarlarınız kullanılır; kod ile üzerine yazılmaz.
        col.isTrigger = true;
    }

    private void FocusNameInputAndShowKeyboard()
    {
        if (nameInputTMP != null)
        {
            nameInputTMP.ActivateInputField();
            nameInputTMP.Select();
        }
        else if (nameInputLegacy != null)
        {
            nameInputLegacy.ActivateInputField();
            nameInputLegacy.Select();
        }
    }

    private bool IsNameInputHit(Collider col)
    {
        if (col == null) return false;
        var go = col.gameObject;
        if (nameInputTMP != null && (go == nameInputTMP.gameObject || go.transform.IsChildOf(nameInputTMP.transform)))
            return true;
        if (nameInputLegacy != null && (go == nameInputLegacy.gameObject || go.transform.IsChildOf(nameInputLegacy.transform)))
            return true;
        return false;
    }

    private Transform FindHandRayOrigin()
    {
        try
        {
            var ovrRig = Object.FindObjectOfType<OVRCameraRig>();
            if (ovrRig == null) return null;
            if (ovrRig.rightHandAnchor != null) return ovrRig.rightHandAnchor;
            if (ovrRig.rightControllerAnchor != null) return ovrRig.rightControllerAnchor;
            Transform t = ovrRig.transform.Find("TrackingSpace/RightHandAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("RightHandAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("TrackingSpace/RightControllerAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("RightControllerAnchor");
            if (t != null) return t;
            foreach (Transform child in ovrRig.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains("RightHand") || child.name.Contains("RightController"))
                    return child;
            }
        }
        catch (System.Exception) { }
        return null;
    }

    public void OnRandomClick()
    {
        string name = GenerateRandomName();
        if (nameInputTMP != null)
            nameInputTMP.text = name;
        else if (nameInputLegacy != null)
            nameInputLegacy.text = name;
    }

    public void OnConfirmClick() => OnConfirm();

    /// <summary>
    /// Anlamsız rastgele isim: 4-6 karakter, sadece harf ve rakam.
    /// </summary>
    private static string GenerateRandomName()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        int length = Random.Range(4, 7);
        var sb = new System.Text.StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(chars[Random.Range(0, chars.Length)]);
        return sb.ToString();
    }

    private void Update()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        float hitDistance = RayDistance;
        Button hitButton = null;
        Collider hitCollider = null;

        if (Physics.Raycast(ray, out RaycastHit previewHit, RayDistance, raycastLayers))
        {
            hitDistance = previewHit.distance;
            hitCollider = previewHit.collider;
            hitButton = GetButtonFromCollider(hitCollider);
        }

        UpdateRayVisual(ray, hitDistance);
        UpdateHoverState(hitButton);

        bool trigger = false;
        try
        {
            trigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        }
        catch (System.Exception) { }

        if (!trigger) return;
        if (hitCollider == null) return;

        var target = hitCollider.GetComponent<UserNameButtonHit>();
        if (target == null)
            target = hitCollider.GetComponentInParent<UserNameButtonHit>();

        if (target != null)
        {
            if (hitButton != null)
            {
                var animator = hitButton.GetComponent<ButtonRayAnimator>();
                if (animator != null) animator.OnPressed();
            }
            switch (target.action)
            {
                case UserNameButtonHit.Action.Random:  OnRandomClick(); break;
                case UserNameButtonHit.Action.Confirm: OnConfirm(); break;
            }
            return;
        }

        if (IsNameInputHit(hitCollider))
            FocusNameInputAndShowKeyboard();
    }

    private Button GetButtonFromCollider(Collider col)
    {
        if (col == null) return null;
        var btn = col.GetComponent<Button>();
        if (btn != null) return btn;
        return col.GetComponentInParent<Button>();
    }

    private void UpdateHoverState(Button hitButton)
    {
        if (hitButton == _currentHoveredButton) return;

        ClearHoverEffect();
        _currentHoveredButton = hitButton;

        if (_currentHoveredButton != null)
        {
            var animator = _currentHoveredButton.GetComponent<ButtonRayAnimator>();
            if (animator != null)
                animator.OnHoverEnter();
            SetRayColor(rayHoverColor);
        }
        else
        {
            SetRayColor(rayColor);
        }
    }

    private void ClearHoverEffect()
    {
        if (_currentHoveredButton != null)
        {
            var animator = _currentHoveredButton.GetComponent<ButtonRayAnimator>();
            if (animator != null)
                animator.OnHoverExit();
            _currentHoveredButton = null;
        }
        SetRayColor(rayColor);
    }

    private void SetRayColor(Color color)
    {
        if (rayLine != null)
        {
            rayLine.startColor = color;
            rayLine.endColor = color;
            if (rayLine.material != null)
                rayLine.material.color = color;
        }
    }

    private void UpdateRayVisual(Ray ray, float length)
    {
        if (rayLine == null) return;
        rayLine.enabled = true;
        rayLine.SetPosition(0, ray.origin);
        rayLine.SetPosition(1, ray.origin + ray.direction * length);
    }

    private string GetInputText()
    {
        if (nameInputTMP != null && nameInputTMP.gameObject.activeInHierarchy)
            return nameInputTMP.text ?? "";
        if (nameInputLegacy != null && nameInputLegacy.gameObject.activeInHierarchy)
            return nameInputLegacy.text ?? "";
        return "";
    }

    private void OnConfirm()
    {
        string name = GetInputText().Trim();
        if (string.IsNullOrEmpty(name))
            name = GenerateRandomName();
        if (name.Length > MaxNameLength)
            name = name.Substring(0, MaxNameLength);

        PlayerPrefs.SetString(PlayerNameKey, name);
        PlayerPrefs.SetInt(UserNameSetKey, 1);
        PlayerPrefs.Save();

        if (FirebaseLeaderboardManager.Instance != null)
            FirebaseLeaderboardManager.Instance.SetPlayerName(name);

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        string sceneName = string.IsNullOrEmpty(nextSceneName) ? "UI" : nextSceneName.Trim();
        // Build'de "MainMenu" yok; yanlış ayarlanmışsa UI kullan (quit/crash önlemi)
        if (string.IsNullOrEmpty(sceneName) || sceneName.Equals("MainMenu", System.StringComparison.OrdinalIgnoreCase))
            sceneName = "UI";
        SceneManager.LoadScene(sceneName);
    }
}
