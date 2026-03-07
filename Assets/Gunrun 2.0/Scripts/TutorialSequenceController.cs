using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [Tooltip("Metin yazma hızı (sn/karakter). 0 = anında")]
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

    [Header("VFX / Silah Geçişi")]
    [Tooltip("Silah değişim süreleri: WeaponManager > Tutorial - Silah Değişim Süreleri")]
    public float weaponChangeVFXDelay = 0.5f;

    private int _currentPhase = -1;
    private int _phaseKillCount;
    private bool _portalsOpened;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial")
        {
            enabled = false;
            return;
        }

        ResolveReferences();
        Time.timeScale = 0f;
        if (weaponManager != null)
            weaponManager.weaponSwitchEnabled = false;
        if (holographicWeaponHUD != null)
            holographicWeaponHUD.EnsureHidden();
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
    }

    private void ResolveReferences()
    {
        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
        if (portalSpawner == null) portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();
        if (holographicWeaponHUD == null) holographicWeaponHUD = FindObjectOfType<HolographicWeaponHUD>();
        if (audioSource == null && dialoguePanel != null) audioSource = dialoguePanel.GetComponentInChildren<AudioSource>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnemyKilled()
    {
        _phaseKillCount++;
    }

    private IEnumerator RunTutorialSequence()
    {
        SetDialogueVisible(true);

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

            // Faz 9: HolographicWeaponHUD
            if (phase == 8)
            {
                if (holographicWeaponHUD != null)
                    holographicWeaponHUD.EnsureVisible();
                if (weaponManager != null)
                    weaponManager.SetJoystickWeaponSwitchEnabled(true);

                if (!string.IsNullOrEmpty(holographicHUDText) || holographicHUDSFX != null)
                {
                    yield return ShowDialogueRealtime(holographicHUDText, null, holographicHUDSFX);
                    yield return new WaitForSecondsRealtime(holographicHUDDuration);
                }
            }
        }

        _currentPhase = -1;
        SetDialogueVisible(false);
        // Tutorial bitti
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

        if (dialogueTextUI != null)
        {
            dialogueTextUI.text = text ?? "";
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
        }

        if (sfx != null && audioSource != null)
            audioSource.PlayOneShot(sfx);

        float duration = !string.IsNullOrEmpty(text) ? text.Length * Mathf.Max(0.02f, typingSpeedPerChar) + dialogueEndBuffer : 0.5f;
        yield return new WaitForSecondsRealtime(duration);
    }

    private IEnumerator ShowDialogueRealtime(string text, AudioClip voiceClip, AudioClip sfxClip)
    {
        if (dialogueTextUI != null)
            dialogueTextUI.text = "";

        if (sfxClip != null && audioSource != null)
            audioSource.PlayOneShot(sfxClip);
        if (voiceClip != null && audioSource != null)
        {
            audioSource.clip = voiceClip;
            audioSource.Play();
        }

        if (!string.IsNullOrEmpty(text) && dialogueTextUI != null)
        {
            if (typingSpeedPerChar > 0f)
            {
                foreach (char c in text)
                {
                    dialogueTextUI.text += c;
                    yield return new WaitForSecondsRealtime(typingSpeedPerChar);
                }
            }
            else
            {
                dialogueTextUI.text = text;
            }
        }

        float audioLen = (voiceClip != null && audioSource != null) ? voiceClip.length : 0f;
        yield return new WaitForSecondsRealtime(Mathf.Max(audioLen, 0.5f) + dialogueEndBuffer);
    }
}
