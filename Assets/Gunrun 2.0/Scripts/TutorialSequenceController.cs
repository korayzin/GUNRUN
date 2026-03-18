using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// newtutorial sahnesi için modüler tutorial sekansı. Eski tutorial'dan tamamen bağımsız.
/// Duvar kırma ile başlar, 9 silahı sırayla (her silahta 3 düşman) tanıtır, 9. silahta HolographicWeaponHUD aktif olur.
/// </summary>
public class TutorialSequenceController : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Boşsa sahnede aranır")]
    public WeaponManager weaponManager;
    [Tooltip("Boşsa sahnede aranır")]
    public AdvancedPortalSpawner portalSpawner;
    [Tooltip("Boşsa sahnede aranır. Başta gizli, 9. fazda açılır")]
    public HolographicWeaponHUD holographicWeaponHUD;
    [Tooltip("Diyalog/metin gösterimi için. Boşsa metin gösterilmez, sesler çalınır.")]
    public TextMeshProUGUI dialogueTextUI;
    [Tooltip("Metin paneli - göster/gizle")]
    public GameObject dialoguePanel;
    [Tooltip("Ses çalmak için")]
    public AudioSource audioSource;

    [Header("Tutorial Bot")]
    [Tooltip("Bot prefabı - prefab içindeki düzen ve canvas aynen korunur")]
    public GameObject tutorialBotPrefab;
    [Tooltip("Botun spawn olacağı nokta - zorunlu. Bot prefab'daki tüm transform değerleri bu noktaya göre uygulanır")]
    public Transform botSpawnPoint;
    [Header("Bot Dissolve VFX")]
    [Tooltip("Dissolve ile geliş süresi (saniye). Prefab'da WeaponDissolveEffect yoksa CanvasGroup fade kullanılır")]
    public float botDissolveSpawnDuration = 0.5f;
    [Tooltip("Dissolve ile gidiş süresi (saniye)")]
    public float botDissolveDespawnDuration = 0.4f;

    private Transform _tutorialBot;
    private WeaponDissolveEffect _botDissolveEffect;
    private CanvasGroup _botFadeCanvasGroup;
    private bool _botUseDissolve;

    [Header("Faz 0 - Destructible (Duvar Kırma)")]
    [Tooltip("Duvar kırma anlatım süresi (saniye)")]
    public float destructiblePhaseDuration = 5f;
    [TextArea(2, 5)]
    public string destructibleText = "Duvarları mermilerinle kırabilirsin. Deneyebilirsin!";
    public AudioClip destructibleVoiceClip;
    public AudioClip destructibleSFX;

    [Header("Silah Fazları - Ortak Ayarlar")]
    [Tooltip("Düşmanlar spawn olduktan kaç saniye sonra duracak")]
    public float enemyStopAfterSeconds = 5f;
    [Tooltip("Düşman hızı (0 ise spawner varsayılanı)")]
    public float enemySpeed = 3f;
    [Tooltip("Diyalog yazma hızı (saniye/karakter). 0 = anında göster, slider ile ayarlayabilirsin")]
    [Range(0f, 0.2f)]
    public float typingSpeedPerChar = 0.02f;
    [Tooltip("Ses/metin bitince ekstra bekleme (saniye)")]
    public float dialogueEndBuffer = 1f;

    [Header("Silah Fazları - Her Silah İçin (9 silah, silah elimize geldiği an oynar)")]
    [Tooltip("Silah elimize geldiğinde gösterilecek metin")]
    public string[] weaponIntroText = new string[9];
    [Tooltip("Silah elimize geldiğinde çalacak SFX")]
    public AudioClip[] weaponIntroSFX = new AudioClip[9];

    [Header("Faz 9 - Holographic HUD")]
    [TextArea(2, 5)]
    public string holographicHUDText = "Sol joystick ile silahlar arasında geçiş yapabilirsin.";
    public AudioClip holographicHUDSFX;
    [Tooltip("HUD açıklama süresi (saniye)")]
    public float holographicHUDDuration = 5f;

    [Header("Faz 10 - Son 3 Düşman (HUD sonrası)")]
    [Tooltip("3 düşman öldürüldükten sonra gösterilecek metin")]
    public string finalPhaseText = "bb";
    [Tooltip("3 düşman öldürüldükten sonra çalacak ses")]
    public AudioClip finalPhaseSFX;
    [Tooltip("Ses/metin bittikten kaç saniye sonra fade out başlasın")]
    public float finalPhaseDelayBeforeFade = 3f;
    [Tooltip("Fade out süresi (saniye)")]
    public float finalPhaseFadeOutDuration = 1.5f;

    [Header("VFX / Silah Geçişi")]
    [Tooltip("Silah değişim süreleri: WeaponManager > Tutorial - Silah Değişim Süreleri")]
    public float weaponChangeVFXDelay = 0.5f;

    private int _currentPhase = -1;
    private int _phaseKillCount;
    private bool _portalsOpened;

    /// <summary>Tutorial sırasında düşmanlar oyuncuya değse bile ölüm olmaz.</summary>
    public static bool IsPlayerInvulnerable { get; private set; }

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial")
        {
            IsPlayerInvulnerable = false;
            enabled = false;
            return;
        }

        IsPlayerInvulnerable = true;
        ResolveReferences();
        Time.timeScale = 0f;
        if (weaponManager != null)
            weaponManager.weaponSwitchEnabled = false;
        if (holographicWeaponHUD != null)
            holographicWeaponHUD.SetCanvasVisible(false);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial") return;
        StartCoroutine(RunTutorialSequence());
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial") return;
        EnemyHealth.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled;
        if (SceneManager.GetActiveScene().name == "newtutorial")
            IsPlayerInvulnerable = false;
    }

    private void ResolveReferences()
    {
        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
        if (portalSpawner == null) portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();
        if (holographicWeaponHUD == null) holographicWeaponHUD = FindObjectOfType<HolographicWeaponHUD>(true);
        if (audioSource == null && dialoguePanel != null) audioSource = dialoguePanel.GetComponentInChildren<AudioSource>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (tutorialBotPrefab != null && botSpawnPoint != null)
        {
            GameObject instance = Instantiate(tutorialBotPrefab, botSpawnPoint);
            _tutorialBot = instance.transform;
            _tutorialBot.localPosition = tutorialBotPrefab.transform.localPosition;
            _tutorialBot.localRotation = tutorialBotPrefab.transform.localRotation;
            _tutorialBot.localScale = tutorialBotPrefab.transform.localScale;
            SetupBotReferences();
            _tutorialBot.gameObject.SetActive(false);
        }
    }

    /// <summary>Bot spawn edildikten sonra dialogueTextUI, dialoguePanel, audioSource ve dissolve/fade referansları alınır.</summary>
    private void SetupBotReferences()
    {
        if (_tutorialBot == null) return;
        _botDissolveEffect = _tutorialBot.GetComponentInChildren<WeaponDissolveEffect>(true);
        if (_botDissolveEffect == null) _botDissolveEffect = _tutorialBot.GetComponent<WeaponDissolveEffect>();
        _botFadeCanvasGroup = _tutorialBot.GetComponentInChildren<CanvasGroup>(true);
        if (_botFadeCanvasGroup == null) _botFadeCanvasGroup = _tutorialBot.GetComponent<CanvasGroup>();
        _botUseDissolve = (_botDissolveEffect != null);

        if (dialogueTextUI == null)
            dialogueTextUI = _tutorialBot.GetComponentInChildren<TextMeshProUGUI>(true);
        if (dialoguePanel == null)
        {
            var canvas = _tutorialBot.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    Transform c = canvas.transform.GetChild(i);
                    if (c.name.Contains("Panel"))
                    {
                        dialoguePanel = c.gameObject;
                        break;
                    }
                }
                if (dialoguePanel == null)
                    dialoguePanel = canvas.gameObject;
            }
        }
        if (audioSource == null)
            audioSource = _tutorialBot.GetComponentInChildren<AudioSource>(true);
        if (dialogueTextUI == null)
            dialogueTextUI = GetComponentInChildren<TextMeshProUGUI>(true);
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnemyKilled()
    {
        _phaseKillCount++;
    }

    private IEnumerator RunTutorialSequence()
    {
        SetDialogueVisible(false);

        // Faz 0: Destructible
        yield return ShowDialogueRealtime(destructibleText, destructibleVoiceClip, destructibleSFX);
        yield return new WaitForSecondsRealtime(Mathf.Max(0.5f, destructiblePhaseDuration));

        // Silah 1 elimizde - text + SFX (silah geldiği an)
        yield return ShowWeaponIntroRealtime(0);

        Time.timeScale = 1f;
        yield return new WaitForSeconds(2f); // Oyuncu duvara ateş edebilsin

        // Portalları aç
        if (portalSpawner != null && !_portalsOpened)
        {
            yield return portalSpawner.StartCoroutine(portalSpawner.OpenPortalAAndWait());
            yield return portalSpawner.StartCoroutine(portalSpawner.OpenPortalBAndC());
            _portalsOpened = true;
        }

        // Faz 1-9: Her silah için 3 düşman
        for (int phase = 0; phase < 9; phase++)
        {
            _currentPhase = phase;
            _phaseKillCount = 0;

            float stopDuration = enemyStopAfterSeconds;
            float speed = enemySpeed > 0f ? enemySpeed : 3f;

            // 3 portaldan 1'er düşman spawn
            if (portalSpawner != null)
            {
                portalSpawner.SpawnSingleEnemyAtPortal("A", stopDuration, 0f, speed);
                portalSpawner.SpawnSingleEnemyAtPortal("B", stopDuration, 0f, speed);
                portalSpawner.SpawnSingleEnemyAtPortal("C", stopDuration, 0f, speed);
            }

            // Düşmanların durmasını bekle
            yield return new WaitForSeconds(stopDuration);

            // 3 kill bekle
            yield return new WaitUntil(() => _phaseKillCount >= 3);

            // Silah değiştir (1->2, 2->3, ... 8->9)
            int nextWeaponIndex = phase + 1;
            if (nextWeaponIndex <= 8 && weaponManager != null)
            {
                weaponManager.TutorialOnly_SwitchToWeaponWithVFX(nextWeaponIndex);
                yield return new WaitForSeconds(weaponChangeVFXDelay + 1f); // VFX + kısa bekleme
                // Yeni silah elimize geldi - text + SFX
                yield return ShowWeaponIntroRealtime(nextWeaponIndex);
            }

            // Faz 9: HolographicWeaponHUD (sahne konumunda açılır)
            if (phase == 8)
            {
                if (holographicWeaponHUD != null)
                    holographicWeaponHUD.SetCanvasVisible(true);
                if (weaponManager != null)
                    weaponManager.SetJoystickWeaponSwitchEnabled(true);

                if (!string.IsNullOrEmpty(holographicHUDText) || holographicHUDSFX != null)
                {
                    yield return ShowDialogueRealtime(holographicHUDText, null, holographicHUDSFX);
                    yield return new WaitForSecondsRealtime(holographicHUDDuration);
                }
            }
        }

        // Faz 10: HUD sonrası 3 düşman - öldürünce "bb" metni + ses
        _phaseKillCount = 0;
        if (portalSpawner != null)
        {
            float stopDuration = enemyStopAfterSeconds;
            float speed = enemySpeed > 0f ? enemySpeed : 3f;
            portalSpawner.SpawnSingleEnemyAtPortal("A", stopDuration, 0f, speed);
            portalSpawner.SpawnSingleEnemyAtPortal("B", stopDuration, 0f, speed);
            portalSpawner.SpawnSingleEnemyAtPortal("C", stopDuration, 0f, speed);
        }
        yield return new WaitForSeconds(enemyStopAfterSeconds);
        yield return new WaitUntil(() => _phaseKillCount >= 3);
        if (!string.IsNullOrEmpty(finalPhaseText) || finalPhaseSFX != null)
            yield return ShowDialogueRealtime(finalPhaseText ?? "bb", null, finalPhaseSFX);

        _currentPhase = -1;
        SetDialogueVisible(false);

        // Ses bittikten 3 saniye sonra fade out, build settings'teki sıradaki sahneye geç
        yield return new WaitForSecondsRealtime(finalPhaseDelayBeforeFade);
        yield return FadeOutAndLoadNextScene();
    }

    private IEnumerator FadeOutAndLoadNextScene()
    {
        // VR'da ScreenSpaceOverlay/ScreenSpaceCamera bazen gözlükte görünmez - World Space + CenterEyeAnchor child kullan
        Transform fadeParent = null;
        Camera vrCamera = null;
        var ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null && ovrRig.centerEyeAnchor != null)
        {
            fadeParent = ovrRig.centerEyeAnchor;
            vrCamera = ovrRig.centerEyeAnchor.GetComponentInChildren<Camera>(true);
        }
        if (vrCamera == null) vrCamera = Camera.main;
        if (fadeParent == null) fadeParent = vrCamera != null ? vrCamera.transform : null;

        var canvasObj = new GameObject("TutorialFadeOverlay");
        if (fadeParent != null)
            canvasObj.transform.SetParent(fadeParent, false);
        canvasObj.transform.localPosition = new Vector3(0f, 0f, 1f); // Kamera önünde 1m
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = vrCamera;
        canvas.sortingOrder = 32767;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        scaler.referencePixelsPerUnit = 100f;
        canvasObj.AddComponent<GraphicRaycaster>();

        var rect = canvasObj.GetComponent<RectTransform>();
        if (rect == null) rect = canvasObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2000f, 2000f); // Büyük alan - görüşü kaplasın

        var imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        var imgRect = imageObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;

        var image = imageObj.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        if (UICameraStackSetup.Instance != null)
            UICameraStackSetup.SetLayerRecursivelyToUI(canvasObj);

        float elapsed = 0f;
        float dur = Mathf.Max(0.1f, finalPhaseFadeOutDuration);
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            t = 1f - (1f - t) * (1f - t);
            image.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        image.color = new Color(0f, 0f, 0f, 1f);

        Time.timeScale = 1f;
        IsPlayerInvulnerable = false; // Geçiş öncesi oyuncu dokunulmazlığını kapat
        TutorialFirstPlayBootstrap.MarkTutorialCompleted(); // Bir sonraki açılışta newtutorial atlanacak
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        if (nextIndex >= sceneCount) nextIndex = 0;
        SceneManager.LoadScene(nextIndex);
    }

    private void SetDialogueVisible(bool visible)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(visible);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(visible);
    }

    private IEnumerator ShowWeaponIntroRealtime(int weaponIndex)
    {
        string text = weaponIndex < weaponIntroText.Length ? weaponIntroText[weaponIndex] : "";
        AudioClip sfx = weaponIndex < weaponIntroSFX.Length ? weaponIntroSFX[weaponIndex] : null;
        yield return ShowDialogueRealtime(text, null, sfx);
    }

    private IEnumerator ShowDialogueRealtime(string text, AudioClip voiceClip, AudioClip sfxClip)
    {
        if (_tutorialBot != null)
        {
            _tutorialBot.gameObject.SetActive(true);
            if (_botUseDissolve && _botDissolveEffect != null)
            {
                yield return _botDissolveEffect.StartCoroutine(_botDissolveEffect.PlaySpawn(botDissolveSpawnDuration));
            }
            else if (_botFadeCanvasGroup != null)
            {
                _botFadeCanvasGroup.alpha = 0f;
                float d = botDissolveSpawnDuration;
                for (float t = 0; t < d; t += Time.unscaledDeltaTime)
                {
                    _botFadeCanvasGroup.alpha = Mathf.Clamp01(t / d);
                    yield return null;
                }
                _botFadeCanvasGroup.alpha = 1f;
            }
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);

        if (dialogueTextUI != null) dialogueTextUI.text = "";
        if (sfxClip != null && audioSource != null) audioSource.PlayOneShot(sfxClip);
        if (voiceClip != null && audioSource != null)
        {
            audioSource.clip = voiceClip;
            audioSource.Play();
        }

        float typingDuration = 0f;
        if (dialogueTextUI != null && !string.IsNullOrEmpty(text))
        {
            if (typingSpeedPerChar > 0f)
            {
                yield return TypewriterCoroutine(dialogueTextUI, text, typingSpeedPerChar);
                typingDuration = text.Length * typingSpeedPerChar;
            }
            else
            {
                dialogueTextUI.text = text;
            }
        }

        float audioLen = (voiceClip != null && audioSource != null) ? voiceClip.length : 0f;
        float audioRemaining = Mathf.Max(0f, audioLen - typingDuration);
        yield return new WaitForSecondsRealtime(audioRemaining + dialogueEndBuffer);

        if (_tutorialBot != null)
        {
            if (_botUseDissolve && _botDissolveEffect != null)
            {
                yield return _botDissolveEffect.StartCoroutine(_botDissolveEffect.PlayDespawn(botDissolveDespawnDuration));
            }
            else if (_botFadeCanvasGroup != null)
            {
                float d = botDissolveDespawnDuration;
                for (float t = 0; t < d; t += Time.unscaledDeltaTime)
                {
                    _botFadeCanvasGroup.alpha = 1f - Mathf.Clamp01(t / d);
                    yield return null;
                }
                _botFadeCanvasGroup.alpha = 0f;
            }
            _tutorialBot.gameObject.SetActive(false);
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
    }

    private IEnumerator TypewriterCoroutine(TextMeshProUGUI textUI, string fullText, float secondsPerChar)
    {
        if (textUI != null) textUI.text = "";
        foreach (char c in fullText)
        {
            if (textUI != null) textUI.text += c;
            yield return new WaitForSecondsRealtime(secondsPerChar);
        }
    }
}
