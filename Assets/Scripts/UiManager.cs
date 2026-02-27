using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Tek merkezden UI panel geçişlerini yönetir.
/// - Her buton tek bir panele bağlıdır
/// - Aynı anda sadece bir panel açıktır
/// - Başlangıçta Leaderboard paneli açıktır
/// - Geçişlerde smooth fade animasyonu uygulanır
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button infoButton;
    public Button optionsButton;
    public Button leaderboardButton;

    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject weaponPanel;
    public GameObject leaderboardPanel;
    public GameObject countdownPanel;
    [Tooltip("Harita seçim paneli (Tools > Map Selection > Otomatik Kurulum ile oluşturulur)")]
    public GameObject mapSelectionPanel;

    [Header("Harita Seçimi")]
    [Tooltip("Varsa Play tıklanınca önce harita seçim paneli açılır")]
    public MapSelectionManager mapSelectionManager;

    [Header("Local Canvas")]
    [Tooltip("Play'e basınca bu canvas'ın countdown dışındaki tüm alt ögeleri kapanır")]
    public Transform localCanvas;

    [Header("Animations")]
    [Tooltip("Panel geçişlerinde fade süresi (0 = animasyonsuz)")]
    [Range(0f, 0.5f)]
    public float panelTransitionDuration = 0.15f;

    [Tooltip("Geçiş animasyonu kullanılsın mı?")]
    public bool useSmoothTransitions = true;

    // Tüm içerik panelleri (countdown hariç - o özel)
    private List<GameObject> _contentPanels;
    private GameObject _currentPanel;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        EnsureVRUIConfiguration();
        ResolveMissingReferences();
        _contentPanels = new List<GameObject>();
        if (optionsPanel != null) _contentPanels.Add(optionsPanel);
        if (weaponPanel != null) _contentPanels.Add(weaponPanel);
        if (leaderboardPanel != null) _contentPanels.Add(leaderboardPanel);
        if (mapSelectionPanel != null) _contentPanels.Add(mapSelectionPanel);
    }

    private void Start()
    {
        WireButtons();
        // Başlangıçta Leaderboard paneli açık
        ShowLeaderboard();
    }

    /// <summary>
    /// Quest build'de EventSystem/OVRInputModule ve Canvas.worldCamera ayarlarını düzeltir.
    /// Editor'da Link ile çalışır, standalone Quest'te UI'ın çalışması için kritik.
    /// </summary>
    private void EnsureVRUIConfiguration()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem != null)
        {
            var ovrModule = eventSystem.GetComponent<OVRInputModule>();
            if (ovrModule != null)
                ovrModule.allowActivationOnMobileDevice = true;
        }
#endif
        var canvases = FindObjectsOfType<Canvas>(true);
        var mainCam = Camera.main;
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null && mainCam != null)
                c.worldCamera = mainCam;
        }
    }

    /// <summary>
    /// Build'de serialized referanslar kaybolursa fallback ile bul.
    /// </summary>
    private void ResolveMissingReferences()
    {
        if (localCanvas == null && countdownPanel != null)
            localCanvas = countdownPanel.transform.parent;
        if (leaderboardPanel == null)
            leaderboardPanel = GameObject.Find("LeaderboardPanel") ?? GameObject.Find("Leaderboard");
    }

    private void WireButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (infoButton != null) infoButton.onClick.AddListener(OnInfoClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (leaderboardButton != null) leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
    }

    private void OnPlayClicked()
    {
        // Geri sayım sadece harita seçildiğinde başlasın; Play'de sadece harita paneli açılır
        // Panel başlangıçta kapalı olduğu için includeInactive: true gerekli
        MapSelectionManager manager = mapSelectionManager != null ? mapSelectionManager : FindObjectOfType<MapSelectionManager>(true);
        if (manager != null)
        {
            manager.OpenMapSelection();
            return;
        }
        StartGameCountdown();
    }

    private void OnInfoClicked()
    {
        LoadTutorial();
    }

    private void OnOptionsClicked()
    {
        ShowOptions();
    }

    private void OnLeaderboardClicked()
    {
        ShowLeaderboard();
    }

    /// <summary>
    /// Sadece Leaderboard panelini açar, diğerlerini kapatır.
    /// </summary>
    public void ShowLeaderboard()
    {
        ShowPanel(leaderboardPanel);
    }

    /// <summary>
    /// Sadece Options panelini açar, diğerlerini kapatır.
    /// </summary>
    public void ShowOptions()
    {
        ShowPanel(optionsPanel);
    }

    /// <summary>
    /// 5-4-3-2-1 geri sayımı ile Tutorial sahnesini yükler.
    /// </summary>
    public void LoadTutorial()
    {
        StartCoroutine(LoadTutorialCountdownCoroutine());
    }

    private IEnumerator LoadTutorialCountdownCoroutine()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        // Tüm panelleri kapat, sadece countdown paneli göster
        foreach (var p in _contentPanels)
            if (p != null) SetPanelActiveImmediate(p, false);

        Transform canvas = localCanvas != null ? localCanvas : (countdownPanel != null ? countdownPanel.transform.parent : null);
        if (canvas != null)
        {
            foreach (Transform child in canvas)
            {
                bool keepActive = (countdownPanel != null && child.gameObject == countdownPanel);
                child.gameObject.SetActive(keepActive);
            }
        }

        if (countdownPanel != null) countdownPanel.SetActive(true);
        _currentPanel = null;

        TextMeshProUGUI countdownText = CountdownManager.Instance != null ? CountdownManager.Instance.countdownText : null;
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        for (int i = 5; i >= 1; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null) countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        // Info butonuyla yüklendi - tutorial atlanmasın, tekrar izlensin
        TutorialIntroController.ForceShowTutorialThisLoad = true;
        SceneManager.LoadScene("Tutorial");
    }

    /// <summary>
    /// Tüm panelleri kapatır. Geri butonlarından çağrılabilir.
    /// Leaderboard'a döner (varsayılan panel).
    /// </summary>
    public void CloseAllPanels()
    {
        ShowLeaderboard();
    }

    /// <summary>
    /// Geri sayımı başlatır, sonra seçilen sahneye geçer.
    /// Harita seçim paneli varsa sadece MapSelectionManager.SelectMap() bu metodu çağırmalı;
    /// Play butonuna doğrudan bağlanmamalı (yoksa hem panel hem sayım başlar).
    /// </summary>
    /// <param name="fromMapSelection">true = harita seçildi, sayım başlasın; false = Play'den geliyorsa sayım başlamasın</param>
    public void StartGameCountdown(bool fromMapSelection = false)
    {
        // Harita seçimi kurulmuşsa geri sayım sadece harita seçildiğinde başlasın (Inspector'daki Play->StartGameCountdown bağlantısını etkisiz kılar)
        if (!fromMapSelection && (mapSelectionManager != null || FindObjectOfType<MapSelectionManager>(true) != null))
            return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        // Tüm panelleri hemen kapat
        foreach (var p in _contentPanels)
            if (p != null) SetPanelActiveImmediate(p, false);
        if (countdownPanel != null) SetPanelActiveImmediate(countdownPanel, false);

        // Canvas altındaki her şeyi kapat, sadece countdown kalsın
        Transform canvas = localCanvas != null ? localCanvas : (countdownPanel != null ? countdownPanel.transform.parent : null);
        if (canvas != null)
        {
            foreach (Transform child in canvas)
            {
                bool keepActive = (countdownPanel != null && child.gameObject == countdownPanel);
                child.gameObject.SetActive(keepActive);
            }
        }

        if (countdownPanel != null) countdownPanel.SetActive(true);
        _currentPanel = null;

        if (CountdownManager.Instance != null)
            CountdownManager.Instance.StartCountdown();
    }

    /// <summary>
    /// Belirtilen paneli açar, diğer tüm içerik panellerini kapatır.
    /// Aynı panele tekrar basılırsa hiçbir şey yapmaz (zaten açık).
    /// </summary>
    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        if (panel == _currentPanel) return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        if (useSmoothTransitions && panelTransitionDuration > 0.001f)
        {
            _transitionCoroutine = StartCoroutine(TransitionToPanelCoroutine(panel));
        }
        else
        {
            ApplyPanelSwitch(panel);
        }
    }

    private IEnumerator TransitionToPanelCoroutine(GameObject targetPanel)
    {
        GameObject previousPanel = _currentPanel;

        // Önce kapanacak paneli fade out
        if (previousPanel != null)
        {
            yield return FadePanel(previousPanel, false);
            SetPanelActiveImmediate(previousPanel, false);
        }

        // Yeni paneli aç ve fade in
        SetPanelActiveImmediate(targetPanel, true);
        if (targetPanel != countdownPanel)
        {
            yield return FadePanel(targetPanel, true);
        }

        _currentPanel = targetPanel;
        _transitionCoroutine = null;
    }

    private IEnumerator FadePanel(GameObject panel, bool fadeIn)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        cg.alpha = startAlpha;
        cg.blocksRaycasts = fadeIn;
        cg.interactable = fadeIn;

        while (elapsed < panelTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelTransitionDuration);
            // EaseOutQuad - daha doğal his
            t = 1f - (1f - t) * (1f - t);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        cg.alpha = endAlpha;
    }

    private void ApplyPanelSwitch(GameObject panel)
    {
        foreach (var p in _contentPanels)
            if (p != null) SetPanelActiveImmediate(p, false);

        if (countdownPanel != null && countdownPanel != panel)
            SetPanelActiveImmediate(countdownPanel, false);

        SetPanelActiveImmediate(panel, true);
        _currentPanel = panel;
    }

    private void SetPanelActiveImmediate(GameObject panel, bool active)
    {
        if (panel == null) return;
        panel.SetActive(active);
        // CanvasGroup varsa alpha'yı sıfırlama - animasyon kullanıyorsak zaten ayarlanıyor
        if (active && !useSmoothTransitions)
        {
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }
}
