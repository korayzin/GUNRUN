using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        _contentPanels = new List<GameObject>();
        if (optionsPanel != null) _contentPanels.Add(optionsPanel);
        if (weaponPanel != null) _contentPanels.Add(weaponPanel);
        if (leaderboardPanel != null) _contentPanels.Add(leaderboardPanel);
    }

    private void Start()
    {
        WireButtons();
        // Başlangıçta Leaderboard paneli açık
        ShowLeaderboard();
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
        StartGameCountdown();
    }

    private void OnInfoClicked()
    {
        ShowWeapons();
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
    /// Sadece Weapon (Info) panelini açar, diğerlerini kapatır.
    /// </summary>
    public void ShowWeapons()
    {
        ShowPanel(weaponPanel);
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
    /// Play butonuna basıldığında: Sadece countdown 5'ten geriye sayar, sonra Koray sahnesine geçer.
    /// </summary>
    public void StartGameCountdown()
    {
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
