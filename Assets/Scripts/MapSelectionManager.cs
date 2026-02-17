using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Play'e basıldığında açılan harita seçim panelini yönetir.
/// Oyuncu hangi haritayı seçerse o sahneye gider.
/// </summary>
public class MapSelectionManager : MonoBehaviour
{
    [Serializable]
    public struct MapEntry
    {
        [Tooltip("UI'da görünen isim")]
        public string displayName;
        [Tooltip("Build Settings'teki sahne adı")]
        public string sceneName;
    }

    [Header("Panel")]
    [SerializeField] private GameObject mapSelectionPanel;

    [Header("Arka plan karartma")]
    [Tooltip("Panel açıkken arkadaki öğelerin alpha değeri (0–1)")]
    [Range(0f, 1f)]
    [SerializeField] private float backgroundDimAlpha = 0.5f;

    [Header("Referanslar")]
    [SerializeField] private CountdownManager countdownManager;
    [SerializeField] private UIManager uiManager;
    [Tooltip("Physics ray başlangıcı (manuel collider'lar için). Boşsa Camera.main kullanılır.")]
    [SerializeField] private Transform rayOrigin;

    [Header("Mapler (Inspector'dan ekleyebilirsin)")]
    [SerializeField] private MapEntry[] maps = new MapEntry[]
    {
        new MapEntry { displayName = "Koray", sceneName = "Koray" }
    };

    private readonly List<CanvasGroup> _dimmedGroups = new List<CanvasGroup>();
    private readonly List<float> _originalAlphas = new List<float>();

    private Button _currentHoveredButton;

    private void Awake()
    {
        if (countdownManager == null) countdownManager = FindObjectOfType<CountdownManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (rayOrigin == null && Camera.main != null) rayOrigin = Camera.main.transform;
    }

    private void Update()
    {
        if (mapSelectionPanel == null || !mapSelectionPanel.activeInHierarchy) return;

        Transform origin = rayOrigin != null ? rayOrigin : (Camera.main != null ? Camera.main.transform : null);
        if (origin == null) return;

        Ray ray = new Ray(origin.position, origin.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            ClearHover();
            return;
        }

        Button button = hit.collider.GetComponent<Button>();
        if (button == null) button = hit.collider.GetComponentInParent<Button>();
        if (button != null && button.interactable)
        {
            if (_currentHoveredButton != button)
            {
                var hover = _currentHoveredButton != null ? _currentHoveredButton.GetComponent<MapSelectionButtonHover>() : null;
                if (hover != null) hover.OnHoverExit();
                _currentHoveredButton = button;
                hover = button.GetComponent<MapSelectionButtonHover>();
                if (hover != null) hover.OnHoverEnter();
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                button.onClick.Invoke();
        }
        else
            ClearHover();
    }

    private void ClearHover()
    {
        if (_currentHoveredButton != null)
        {
            var hover = _currentHoveredButton.GetComponent<MapSelectionButtonHover>();
            if (hover != null) hover.OnHoverExit();
            _currentHoveredButton = null;
        }
    }

    private void OnDisable()
    {
        ClearHover();
    }

    /// <summary>
    /// Harita seçim panelini açar. Play butonuna basıldığında çağrılır.
    /// Arkadaki öğelerin alpha'sı yarıya iner, odak panelde olur.
    /// </summary>
    public void OpenMapSelection()
    {
        if (mapSelectionPanel == null) return;
        ClearHover();
        if (uiManager != null)
            uiManager.ShowPanel(mapSelectionPanel);
        else
            mapSelectionPanel.SetActive(true);

        DimBackground();
    }

    private void DimBackground()
    {
        RestoreBackground();
        Transform parent = mapSelectionPanel.transform.parent;
        if (parent == null) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.gameObject == mapSelectionPanel) continue;

            CanvasGroup cg = child.GetComponent<CanvasGroup>();
            if (cg == null) cg = child.gameObject.AddComponent<CanvasGroup>();

            _originalAlphas.Add(cg.alpha);
            _dimmedGroups.Add(cg);
            cg.alpha = backgroundDimAlpha;
        }
    }

    private void RestoreBackground()
    {
        for (int i = 0; i < _dimmedGroups.Count; i++)
        {
            if (_dimmedGroups[i] != null && i < _originalAlphas.Count)
                _dimmedGroups[i].alpha = _originalAlphas[i];
        }
        _dimmedGroups.Clear();
        _originalAlphas.Clear();
    }

    /// <summary>
    /// Seçilen haritanın sahnesine gidecek şekilde countdown'u başlatır.
    /// Harita butonlarından çağrılır (örn. SelectMap("Koray")).
    /// </summary>
    public void SelectMap(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[MapSelectionManager] sceneName boş!");
            return;
        }
        if (countdownManager != null)
            countdownManager.sceneToLoad = sceneName;
        RestoreBackground();
        if (mapSelectionPanel != null)
            mapSelectionPanel.SetActive(false);
        if (uiManager != null)
            uiManager.StartGameCountdown(fromMapSelection: true);
        else if (countdownManager != null)
            countdownManager.StartCountdown();
    }

    /// <summary>
    /// Paneli kapatıp ana menüye (Leaderboard) döner.
    /// </summary>
    public void CloseMapSelection()
    {
        RestoreBackground();
        if (mapSelectionPanel != null)
            mapSelectionPanel.SetActive(false);
        if (uiManager != null)
            uiManager.ShowLeaderboard();
    }

    /// <summary>
    /// Inspector'daki map listesini döndürür (ileride dinamik buton üretimi için).
    /// </summary>
    public MapEntry[] GetMaps() => maps;
}

/// <summary>
/// Harita seçim paneli butonları için sadece hafif büyüme hover efekti.
/// Glitch yapan letter-spacing / renk / bounce yok; sadece scale.
/// HandRayUIInteractor bu component varsa ButtonRayAnimator yerine bunu kullanır.
/// </summary>
[RequireComponent(typeof(Button))]
public class MapSelectionButtonHover : MonoBehaviour
{
    [Tooltip("Hover'da büyüme çarpanı (1 = yok, 1.05 = %5 büyüme)")]
    [Range(1f, 1.15f)]
    public float hoverScale = 1.05f;

    private RectTransform _rect;
    private Vector3 _originalScale;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect != null) _originalScale = _rect.localScale;
    }

    private void OnEnable()
    {
        if (_rect != null) _originalScale = _rect.localScale;
    }

    public void OnHoverEnter()
    {
        if (_rect != null)
            _rect.localScale = _originalScale * hoverScale;
    }

    public void OnHoverExit()
    {
        if (_rect != null)
            _rect.localScale = _originalScale;
    }
}

/// <summary>
/// Harita butonuna tıklandığında MapSelectionManager.SelectMap(sceneName) çağrılır.
/// Inspector'da sceneName atanır; Editor kurulumunda otomatik bağlanır.
/// </summary>
public class MapSelectionButton : MonoBehaviour
{
    [Tooltip("Build Settings'teki sahne adı")]
    public string sceneName = "Koray";

    public void OnMapSelected()
    {
        var manager = FindObjectOfType<MapSelectionManager>();
        if (manager != null)
            manager.SelectMap(sceneName);
    }
}
