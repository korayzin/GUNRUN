using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tutorial sahnesi giriş akışı. Ana oyun mantığından bağımsız çalışır.
/// 1. Sahne açılınca oyun pause (Time.timeScale = 0)
/// 2. X saniye sonra bot spawn olur
/// 3. Y saniye sonra bot konuşmaya başlar (metin + ses)
/// 4. 2. diyalog bitince Portal A açılır, tek düşman spawn (ilerleyip durur)
/// 5. 3. diyalog: "İşte anomaliler! Harekete geçtiler..."
/// 6. 3-2-1-GO, unpause
/// 7. İlk düşman öldürülünce pause, 4. diyalog, Portal B ve C açılır
/// 8. 5. diyalog: "3 portaldan gelen anomalilerden 4 tane indir"
/// 9. 4 düşman öldürülünce devam
/// </summary>
public class TutorialIntroController : MonoBehaviour
{
    private const string TutorialCompletedKey = "TutorialCompleted";

    /// <summary>Info butonuyla yüklendiyse true - tutorial atlanmaz, tekrar izlenebilir.</summary>
    public static bool ForceShowTutorialThisLoad = false;

    /// <summary>Tutorial başında false, ilk diyalog bitip 1 sn sonra true. GunFire bu flag'i kontrol eder.</summary>
    public static bool TutorialFiringEnabled = true;

    /// <summary>Dialogue 6'dan önce mermi sınırsız; GunFire bu flag true iken currentAmmo azaltmaz.</summary>
    public static bool TutorialUnlimitedAmmo = false;

    /// <summary>Dialogue 8 sırası geldiğinde true; Gun Change After UI baştan kapalı.</summary>
    public static bool TutorialGunChangeAfterVisible = false;

    /// <summary>5. silaha geçildiğinde true; düşman sayısı artar, hız azalır, ikincil özellik sınırsız.</summary>
    public static bool TutorialFifthWeaponPhase = false;
    /// <summary>13. diyalog bitince true; 5. silah ikincil (yıldırım) aktif.</summary>
    public static bool TutorialFifthWeaponSecondaryEnabled = false;
    /// <summary>5. yıldırım kill sonrası (Dialogue 14) true; Gun Change After açılır, 5->6 geçişi serbest.</summary>
    public static bool TutorialFifthWeaponGunChangeAfterEnabled = false;

    /// <summary>6. silaha geçildiğinde true; düşman sayısı artar, hız azalır.</summary>
    public static bool TutorialSixthWeaponPhase = false;
    /// <summary>15. diyalog bitince true; 6. silah ikincil (yavaşlatma) aktif.</summary>
    public static bool TutorialSixthWeaponSecondaryEnabled = false;
    /// <summary>16. diyalogda true; 6. silah Gun Change After açılır, 6->7 geçişi serbest.</summary>
    public static bool TutorialSixthWeaponGunChangeAfterEnabled = false;

    /// <summary>7. silaha geçildiğinde true; düşman sayısı artar, hız azalır.</summary>
    public static bool TutorialSeventhWeaponPhase = false;
    /// <summary>17. diyalog bitince true; 7. silah ikincil (kırbaç) aktif.</summary>
    public static bool TutorialSeventhWeaponSecondaryEnabled = false;
    /// <summary>5 kırbaç kullanım sonrası (Dialogue 18) true; 7. silah Gun Change After + energy bar aktif.</summary>
    public static bool TutorialSeventhWeaponGunChangeAfterEnabled = false;

    /// <summary>8. silaha geçildiğinde true; düşman sayısı artar, hız azalır.</summary>
    public static bool TutorialEighthWeaponPhase = false;
    /// <summary>19. diyalog bitince true; 8. silah ikincil aktif (varsa).</summary>
    public static bool TutorialEighthWeaponSecondaryEnabled = false;
    /// <summary>1 toy kullanım sonrası (Dialogue 20) true; 8. silah Gun Change After + cooldown aktif.</summary>
    public static bool TutorialEighthWeaponGunChangeAfterEnabled = false;

    /// <summary>6. silah: slow beam bir düşmana değdiğinde tetiklenir (ikincil kullanıldı).</summary>
    public static event System.Action<int> OnTutorialSecondaryUsed;

    /// <summary>Dış scriptlerden ikincil kullanım bildirimi (örn. SixthGunLaser slow hit).</summary>
    public static void NotifyTutorialSecondaryUsed(int weaponIndex) => OnTutorialSecondaryUsed?.Invoke(weaponIndex);

    private static int _fifthWeaponLightningKillCount;
    private static int _seventhWeaponWhipUseCount;
    private static int _eighthWeaponToyUseCount;
    private static int _ninthWeaponFlameSprayKillCount;
    private static float _ninthWeaponFlameSprayCumulativeTime;

    /// <summary>9. silah flame spray kullanım süresi. ~5 sn sonra Dialogue 22.</summary>
    public static void NotifyTutorialNinthWeaponFlameSprayTime(float deltaTime)
    {
        if (!TutorialNinthWeaponPhase || !TutorialNinthWeaponPrimaryEnabled || TutorialNinthWeaponDialogue22Complete)
            return;
        _ninthWeaponFlameSprayCumulativeTime += deltaTime;
        if (_ninthWeaponFlameSprayCumulativeTime >= 5f)
        {
            var ctrl = Object.FindObjectOfType<TutorialIntroController>();
            if (ctrl != null && !ctrl._dialogue22Shown)
            {
                ctrl._dialogue22Shown = true;
                ctrl.StartCoroutine(ctrl.ShowDialogue22Coroutine());
            }
        }
    }

    public void TriggerShowDialogue14()
    {
        if (!_dialogue14Shown) StartCoroutine(ShowDialogue14Coroutine());
    }

    /// <summary>7. silah kırbaç her kullanımda çağrılır. 5. kullanımda Dialogue 18.</summary>
    public static void NotifyTutorialSeventhWeaponWhipUsed()
    {
        if (!TutorialSeventhWeaponPhase || !TutorialSeventhWeaponSecondaryEnabled)
            return;
        _seventhWeaponWhipUseCount++;
        if (_seventhWeaponWhipUseCount >= 5)
        {
            var ctrl = Object.FindObjectOfType<TutorialIntroController>();
            if (ctrl != null) ctrl.TriggerShowDialogue18();
        }
    }

    public void TriggerShowDialogue18()
    {
        if (!_dialogue18Shown) StartCoroutine(ShowDialogue18Coroutine());
    }

    /// <summary>8. silah toy her kullanımda çağrılır. 1. kullanımda Dialogue 20.</summary>
    public static void NotifyTutorialEighthWeaponToyUsed()
    {
        if (!TutorialEighthWeaponPhase || !TutorialEighthWeaponSecondaryEnabled)
            return;
        _eighthWeaponToyUseCount++;
        if (_eighthWeaponToyUseCount >= 1)
        {
            var ctrl = Object.FindObjectOfType<TutorialIntroController>();
            if (ctrl != null) ctrl.TriggerShowDialogue20();
        }
    }

    public void TriggerShowDialogue20()
    {
        if (!_dialogue20Shown) StartCoroutine(ShowDialogue20Coroutine());
    }

    public void TriggerShowDialogue23()
    {
        if (!_dialogue23Shown) StartCoroutine(ShowDialogue23Coroutine());
    }

    /// <summary>9. silaha geçildiğinde true; 3 diyalog (21→1kill→22→1kill→23). Gun Change After çalışmaz.</summary>
    public static bool TutorialNinthWeaponPhase = false;
    /// <summary>21. diyalog bitince true; birincil (flame spray) aktif.</summary>
    public static bool TutorialNinthWeaponPrimaryEnabled = false;
    /// <summary>22. diyalog bitince true; ikincil (fireball) aktif.</summary>
    public static bool TutorialNinthWeaponSecondaryEnabled = false;
    /// <summary>23. diyalog bitince true; joystick ile silah geçişi aktif.</summary>
    public static bool TutorialNinthWeaponJoystickEnabled = false;
    /// <summary>9. silah: 23. diyaloga kadar fireball sınırsız (ammo tüketilmez).</summary>
    public static bool TutorialNinthWeaponFireballUnlimited = false;
    /// <summary>22. diyalog bitti; fireball kullanım sayacı aktif.</summary>
    public static bool TutorialNinthWeaponDialogue22Complete = false;
    /// <summary>23. diyalog bitti; serbest oyun: düşmanlar temizlenir, normal spawn, tüm silahlar tam kullanılabilir.</summary>
    public static bool TutorialCompleteFreehand = false;

    [Header("Tutorial Bot")]
    [Tooltip("Bot prefabı - prefab içindeki düzen ve canvas aynen korunur")]
    public GameObject tutorialBotPrefab;

    [Tooltip("Botun spawn olacağı nokta - zorunlu. Bot prefab'daki tüm transform değerleri bu noktaya göre uygulanır")]
    public Transform botSpawnPoint;

    [Tooltip("Oyun başladıktan kaç saniye sonra bot spawn olsun ve tutorial başlasın")]
    public float delayBeforeSpawn = 5f;

    [Tooltip("Bot spawn olduktan kaç saniye sonra konuşmaya (metin + ses) başlasın. 0 = spawn ile birlikte")]
    public float delayBeforeDialogue = 0f;

    [Tooltip("Bot hafif salınım genliği (metre) - 0 = kapalı")]
    [Range(0f, 0.1f)]
    public float botSwayAmount = 0.03f;

    [Tooltip("Bot kendi ekseninde minik rotasyon (derece) - 10 ile -5 arası hafif canlılık")]
    public Vector2 botRotationSway = new Vector2(10f, -5f);

    [Header("Bot Konuşma")]
    [Tooltip("Botun söyleyeceği metin - ekranda gösterilecek")]
    [TextArea(2, 5)]
    public string dialogueText = "Merhaba! Ben senin eğitmen botunum. Hazır mısın?";

    [Tooltip("Bot sesi - Inspector'dan atanacak")]
    public AudioClip botVoiceClip;

    [Tooltip("Konuşma metninin gösterileceği TextMeshPro")]
    public TextMeshProUGUI dialogueTextUI;

    [Tooltip("Opsiyonel: Konuşma paneli (içinde dialogueTextUI varsa, panel açılıp kapanır)")]
    public GameObject dialoguePanel;

    [Tooltip("Ses çalmak için AudioSource (atanmazsa otomatik eklenir)")]
    public AudioSource audioSource;

    [Tooltip("Yazma animasyonu hızı (saniye/karakter). 0 = animasyonsuz, anında göster")]
    [Range(0f, 0.2f)]
    public float typingSpeedPerChar = 0.04f;

    [Header("İkinci Konuşma")]
    [Header("Dialogue 1 → Duvar kırma → Dialogue 2")]
    [Tooltip("1. diyalog bitince oyun devam eder, mermiler aktif. Bu süre (sn) + en az bu kadar duvar kırımı sonrası 2. diyalog başlar")]
    public float delayAfterDialogue1BeforeDialogue2 = 5f;
    [Tooltip("2. diyaloga geçmeden önce en az kaç duvar segmenti kırılmalı (0 = sadece süre yeterli)")]
    public int minWallBreakCountBeforeDialogue2 = 1;
    [Tooltip("1. diyalog bitince bot hareket edip 2. cümleye geçmeden önce bekleme (sn)")]
    public float delayBetweenDialogues = 2f;

    [Tooltip("İkinci konuşma için bot pozisyon offset'i (metre)")]
    public Vector3 botSecondPositionOffset = new Vector3(0.2f, 0f, 0.15f);

    [Tooltip("Bot ikinci pozisyona geçiş süresi (saniye)")]
    public float botPositionMoveDuration = 0.5f;

    [Tooltip("İkinci konuşma metni - boşsa atlanır")]
    [TextArea(2, 5)]
    public string dialogueText2 = "";

    [Tooltip("İkinci konuşma sesi")]
    public AudioClip botVoiceClip2;

    [Tooltip("2. diyalog başladıktan kaç saniye sonra Portal A açılsın")]
    public float delayAfterDialogue2StartBeforePortalA = 1f;

    [Tooltip("Düşman spawn olduktan kaç saniye sonra durup dialogue 4 çalacak")]
    public float enemyMoveDurationBeforeDialogue4 = 4f;

    [Tooltip("Portal spawner - boşsa sahnede aranır")]
    public AdvancedPortalSpawner portalSpawner;

    [Header("3. Diyalog - Anomaliler")]
    [TextArea(2, 5)]
    public string dialogueText3 = "İşte anomaliler! Harekete geçtiler, onları indir! Dikkat et eğer sana değerse ölürsün!";
    public AudioClip botVoiceClip3;

    [Header("4. Diyalog - Düşman durdu, vurma talimatı")]
    [TextArea(2, 5)]
    public string dialogueText4 = "Şimdi onu indir!";
    public AudioClip botVoiceClip4;

    [Header("5. Diyalog - İlk düşman öldürüldü")]
    [TextArea(2, 5)]
    public string dialogueText5 = "Well done! İlk düşmanı indirdin, sırada diğerleri var.";
    public AudioClip botVoiceClip5;

    [Header("6. Diyalog - 4 düşman görevi")]
    [TextArea(2, 5)]
    public string dialogueText6 = "3 portaldan gelen anomalilerden 4 tane indir.";
    public AudioClip botVoiceClip6;

    [Header("7. Diyalog - B/C portalları açıldıktan sonra")]
    [Tooltip("Portallar açılıp düşmanlar biraz ilerledikten sonra oyun durur, bu diyalog oynar")]
    [TextArea(2, 5)]
    public string dialogueText7 = "Bak, B ve C portallarından da anomaliler geliyor! Hazır mısın?";
    public AudioClip botVoiceClip7;
    [Tooltip("B ve C açıldıktan, düşmanlar spawn olduktan sonra kaç saniye bekleyip pause + dialogue 7")]
    public float delayAfterPortalsBCBeforeDialogue7 = 5f;

    [Header("8. Diyalog - Silah değişimi anlatımı")]
    [Tooltip("Oyuncu 7. diyalogtan sonra gelen düşmanları vurduktan sonra oyun durur, bu diyalog silah değişimini anlatır")]
    [TextArea(2, 5)]
    public string dialogueText8 = "Belirli sayıda düşman öldürdüğünde silahın otomatik değişir. Ekrandaki 'Gun Change After' sayacına bak!";
    public AudioClip botVoiceClip8;
    [Tooltip("Tüm portallar açıldıktan sonra kaç düşman öldürünce pause + dialogue 8")]
    public int killsAfterDialogue7BeforeDialogue8 = 2;

    [Header("9. Diyalog - Silah değiştikten sonra")]
    [Tooltip("8. diyalog bittikten, silah değişimi gerçekleştikten sonra oyun durur, bu diyalog oynar")]
    [TextArea(2, 5)]
    public string dialogueText9 = "Harika! Yeni silahınla devam et.";
    public AudioClip botVoiceClip9;

    [Header("10. Diyalog - 3. silaha geçildiğinde")]
    [Tooltip("Oyuncu 3. silaha geçtiğinde oyun bir kez durur, bu diyalog oynar")]
    [TextArea(2, 5)]
    public string dialogueText10 = "";
    public AudioClip botVoiceClip10;

    [Header("11. Diyalog - 4. silaha geçildiğinde")]
    [Tooltip("Oyuncu 4. silaha geçtiğinde oyun durur, bu silah hakkında bilgi verilir")]
    [TextArea(2, 5)]
    public string dialogueText11 = "";
    public AudioClip botVoiceClip11;

    [Header("12. Diyalog - Holographic Weapon HUD")]
    [Tooltip("Sol el haptic + HUD açılır, diyalogda anlatılır")]
    [TextArea(2, 5)]
    public string dialogueText12 = "";
    public AudioClip botVoiceClip12;

    [Header("13. Diyalog - 5. silah ikincil özellik (yıldırım)")]
    [Tooltip("5. silaha geçilince oyun durur, ikincil özellik anlatılır. Diyalog bitince oyun devam eder.")]
    [TextArea(2, 5)]
    public string dialogueText13 = "";
    public AudioClip botVoiceClip13;

    [Header("14. Diyalog - Normal trigger ile ateş")]
    [Tooltip("Normal trigger ile de ateş edilebildiği anlatılır")]
    [TextArea(2, 5)]
    public string dialogueText14 = "";
    public AudioClip botVoiceClip14;

    [Header("15. Diyalog - 6. silah ikincil özellik (yavaşlatma)")]
    [Tooltip("6. silaha geçilince. Sol kontrolcü trigger ile düşmanları yavaşlatır, açıp kapatabilirsin.")]
    [TextArea(2, 5)]
    public string dialogueText15 = "";
    public AudioClip botVoiceClip15;

    [Header("16. Diyalog - 6. silah normal mermi + Gun Change After aç")]
    [Tooltip("Normal mermisini anlatır, Gun Change After açılır, kalan killeri tamamla")]
    [TextArea(2, 5)]
    public string dialogueText16 = "";
    public AudioClip botVoiceClip16;

    [Header("17. Diyalog - 7. silah ikincil özellik")]
    [Tooltip("7. silaha geçilince. İkincil özelliği anlatır.")]
    [TextArea(2, 5)]
    public string dialogueText17 = "";
    public AudioClip botVoiceClip17;

    [Header("18. Diyalog - 7. silah normal mermi + Gun Change After aç")]
    [Tooltip("Normal mermisini anlatır, Gun Change After açılır, kalan killeri tamamla")]
    [TextArea(2, 5)]
    public string dialogueText18 = "";
    public AudioClip botVoiceClip18;

    [Header("19. Diyalog - 8. silah ikincil özellik")]
    [Tooltip("8. silaha geçilince. İkincil özelliği anlatır.")]
    [TextArea(2, 5)]
    public string dialogueText19 = "";
    public AudioClip botVoiceClip19;

    [Header("20. Diyalog - 8. silah normal mermi + Gun Change After aç")]
    [Tooltip("Normal mermisini anlatır, Gun Change After açılır, kalan killeri tamamla")]
    [TextArea(2, 5)]
    public string dialogueText20 = "";
    public AudioClip botVoiceClip20;

    [Header("21. Diyalog - 9. silah birincil özellik")]
    [Tooltip("9. silaha geçilince. Birincil özelliği anlatır. 1 kill sonra 22. diyalog")]
    [TextArea(2, 5)]
    public string dialogueText21 = "";
    public AudioClip botVoiceClip21;

    [Header("22. Diyalog - 9. silah ikincil özellik")]
    [Tooltip("1 kill sonra. İkincil özelliği anlatır. 1 kill sonra 23. diyalog")]
    [TextArea(2, 5)]
    public string dialogueText22 = "";
    public AudioClip botVoiceClip22;

    [Header("23. Diyalog - Joystick ile silah geçişi")]
    [Tooltip("Tüm silahlar aktif, sol joystick ile geçiş, freehand oynanabilir. Gun Change After yok")]
    [TextArea(2, 5)]
    public string dialogueText23 = "";
    public AudioClip botVoiceClip23;

    [Header("24. Diyalog - 45 sn sonra ana menüye dönüş")]
    [Tooltip("45 sn bittiğinde gösterilir. Sol kontrolcü Y tuşuna basılı tutarak ana menüye dön")]
    [TextArea(2, 5)]
    public string dialogueText24 = "Hold the Y button on the left controller to return to the main menu.";
    public AudioClip botVoiceClip24;

    [Header("Tutorial Retry - Oyuncu öldüğünde")]
    [Tooltip("Tutorial sırasında ölünce gösterilecek retry diyaloğu")]
    [TextArea(2, 5)]
    public string tutorialRetryDialogueText = "No problem, lets start where we stay.";
    public AudioClip tutorialRetryVoiceClip;

    [Header("Tutorial Retry - Mermi bittiğinde")]
    [Tooltip("Tutorial sırasında mermi bitince gösterilecek retry diyaloğu")]
    [TextArea(2, 5)]
    public string tutorialOutOfAmmoRetryDialogueText = "No worries, I'll refill your ammo. Lets try again.";
    public AudioClip tutorialOutOfAmmoRetryVoiceClip;

    [Header("Tutorial Retry - Enerji/süre bittiğinde")]
    [Tooltip("Tutorial sırasında 9. silah enerjisi veya süre bitince gösterilecek retry diyaloğu")]
    [TextArea(2, 5)]
    public string tutorialEnergyOrTimeRetryDialogueText = "No worries, I'll refill your energy and time. Lets try again.";
    public AudioClip tutorialEnergyOrTimeRetryVoiceClip;

    [Header("Gun Change After UI")]
    [Tooltip("8. diyalogla beraber gösterilecek. Boşsa sahnede aranır.")]
    public WeaponSwitchCountdownUI gunChangeAfterUI;

    [Header("Holographic Weapon HUD")]
    [Tooltip("Başta kapalı, 11. diyalogda açılır. Inspector'dan ata.")]
    public HolographicWeaponHUD holographicWeaponHUD;

    [Header("Geri Sayım")]
    [Tooltip("3-2-1 sayısının gösterileceği TextMeshPro")]
    public TextMeshProUGUI countdownTextUI;

    [Tooltip("Konuşma bitince ekstra bekleme süresi (saniye) - okuma için")]
    public float dialogueEndBuffer = 1f;

    [Header("Tutorial / Username akışı")]
    [Tooltip("Açık: Tutorial tamamlandıysa ve username set ise UI'ya yönlendir; değilse Tutorial → UserName → UI. Kapalı: Build sırasına göre aç, yönlendirme yapma.")]
    [SerializeField] private bool useTutorialAndUserNameFlow = true;
    [Tooltip("UserName sahnesindeki 'Cihaz başına bir kez' ile aynı tutun. Kapalı: Username set olsa bile uygulama UserName sahnesi ile açılır.")]
    [SerializeField] private bool showUserNameOnlyOncePerDevice = true;

    [Header("Ana Menü Geçişi")]
    [Tooltip("TEST: true ise 2. diyalog bitince Y tuşu ile ana menüye dönüş aktif olur (test için)")]
    public bool testMainMenuReturnAfterDialogue2 = true;
    [Tooltip("İlk kez tutorial bitince (username henüz set değilse) yüklenecek sahne adı")]
    public string userNameSceneName = "UserName";
    [Tooltip("Y tuşuna basılı tutunca yüklenecek ana menü sahnesi (Build Settings'te ekli olmalı)")]
    public string mainMenuSceneName = "UI";
    [Tooltip("Y tuşuna basılı tutunca fade out süresi (sn)")]
    public float mainMenuFadeOutDuration = 1.5f;

    [Header("Test")]
    [Tooltip("Açıksa sağ el grip ile son diyaloğa atla, Y ile ana menüye dön")]
    [SerializeField] private bool skipTutorialEnabled = true;

    private Transform _tutorialBot;
    private Vector3 _botBasePosition;
    private Quaternion _botBaseRotation;

    private int _tutorialKillCount;
    private bool _waitingForFirstKill;
    private bool _waitingForFourKills;
    private bool _waitingForPostDialogue7Kills;
    private TutorialFirstEnemyController _firstEnemyController;
    private GameObject _firstEnemyGameObject;
    private int _wallBreakCount;
    private bool _dialogue10Shown;
    private bool _dialogue11Shown;
    private bool _dialogue12Shown;
    private bool _dialogue13Shown;
    private bool _dialogue14Shown;
    private bool _dialogue15Shown;
    private bool _dialogue16Shown;
    private bool _dialogue17Shown;
    private bool _dialogue18Shown;
    private bool _dialogue19Shown;
    private bool _dialogue20Shown;
    private bool _dialogue21Shown;
    private bool _dialogue22Shown;
    private bool _dialogue22Complete; // 22 bittiğinde true; 23 için gerekli
    private bool _dialogue23Shown;
    private bool _tutorialFinalPhaseEnded; // 24. diyalog bitti, Y ile ana menüye dönüş bekleniyor
    private float _mainMenuHoldTime;
    private bool _tutorialRetryInProgress;

    private void Awake()
    {
        // Info butonuyla yüklendiyse tutorial göster (tekrar izleme)
        if (ForceShowTutorialThisLoad)
        {
            ForceShowTutorialThisLoad = false;
        }
        // Tik açıksa: tutorial tamamlandıysa ve username set ise; "cihaz başına 1 kez" açıksa UI'ya, kapalıysa UserName ile aç
        if (useTutorialAndUserNameFlow && PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1 && PlayerPrefs.GetInt(UserNameController.UserNameSetKey, 0) == 1)
        {
            if (!showUserNameOnlyOncePerDevice)
                UserNameController.ForceShowFormThisLoad = true; // UserName sahnesinde kurulum atlanmasın, butonlar çalışsın
            string target = showUserNameOnlyOncePerDevice
                ? (string.IsNullOrEmpty(mainMenuSceneName) ? "UI" : mainMenuSceneName)
                : (string.IsNullOrEmpty(userNameSceneName) ? "UserName" : userNameSceneName);
            StartCoroutine(DelayedRedirectToScene(target));
            return;
        }
        // Tik açıksa ve tutorial tamamlandıysa ama username henüz set değilse UserName'e gönder
        if (useTutorialAndUserNameFlow && PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1)
        {
            string target = string.IsNullOrEmpty(userNameSceneName) ? "UserName" : userNameSceneName;
            StartCoroutine(DelayedRedirectToScene(target));
            return;
        }

        Time.timeScale = 0f;
        TutorialActive = true;
    }

    /// <summary>XR/OVR hazır olsun diye yönlendirmeyi 2 kare geciktirir (UserName sahnesinde ray/butonların tetiklenmesi için).</summary>
    private IEnumerator DelayedRedirectToScene(string sceneName)
    {
        yield return null;
        yield return null;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>Tutorial sahnesinde miyiz (WeaponSwitchCountdownUI için).</summary>
    public static bool TutorialActive { get; private set; }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += OnTutorialEnemyKilled;
        OnTutorialSecondaryUsed += OnTutorialSecondaryUsedHandler;
        DestructibleMeshHint.OnWallDestroyed += OnTutorialWallDestroyed;
        var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
        if (wm != null)
            wm.OnWeaponChanged += OnWeaponChangedForDialogue10;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnTutorialEnemyKilled;
        OnTutorialSecondaryUsed -= OnTutorialSecondaryUsedHandler;
        DestructibleMeshHint.OnWallDestroyed -= OnTutorialWallDestroyed;
        var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
        if (wm != null)
            wm.OnWeaponChanged -= OnWeaponChangedForDialogue10;
    }

    private void OnWeaponChangedForDialogue10(int newWeaponIndex)
    {
        if (newWeaponIndex == 2 && !_dialogue10Shown && !string.IsNullOrEmpty(dialogueText10))
        {
            _dialogue10Shown = true;
            StartCoroutine(ShowDialogue10Coroutine());
            return;
        }
        if (newWeaponIndex == 3 && _dialogue10Shown && !_dialogue11Shown)
        {
            _dialogue11Shown = true;
            StartCoroutine(ShowDialogue11And12Coroutine());
            return;
        }
        if (newWeaponIndex == 4 && _dialogue11Shown && !_dialogue13Shown)
        {
            _dialogue13Shown = true;
            StartCoroutine(ShowDialogue13And14Coroutine());
            return;
        }
        if (newWeaponIndex == 5 && _dialogue14Shown && !_dialogue15Shown)
        {
            _dialogue15Shown = true;
            StartCoroutine(ShowDialogue15And16Coroutine());
            return;
        }
        if (newWeaponIndex == 6 && _dialogue16Shown && !_dialogue17Shown)
        {
            _dialogue17Shown = true;
            StartCoroutine(ShowDialogue17And18Coroutine());
            return;
        }
        if (newWeaponIndex == 7 && _dialogue18Shown && !_dialogue19Shown)
        {
            _dialogue19Shown = true;
            StartCoroutine(ShowDialogue19And20Coroutine());
            return;
        }
        if (newWeaponIndex == 8 && _dialogue20Shown && !_dialogue21Shown)
        {
            _dialogue21Shown = true;
            StartCoroutine(ShowDialogue21Coroutine());
        }
    }

    private IEnumerator ShowDialogue10Coroutine()
    {
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        yield return ShowDialogueCoroutine(dialogueText10, botVoiceClip10);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator ShowDialogue11And12Coroutine()
    {
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(dialogueText11))
            yield return ShowDialogueCoroutine(dialogueText11, botVoiceClip11);
        if (!_dialogue12Shown && !string.IsNullOrEmpty(dialogueText12))
        {
            _dialogue12Shown = true;
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
            yield return new WaitForSecondsRealtime(0.15f);
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            var hud12 = holographicWeaponHUD != null ? holographicWeaponHUD : FindObjectOfType<HolographicWeaponHUD>(true);
            if (hud12 != null) hud12.SetCanvasVisible(true); // 12. diyalogda Canvas aç
            yield return ShowDialogueCoroutine(dialogueText12, botVoiceClip12);
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator ShowDialogue13And14Coroutine()
    {
        Time.timeScale = 0f;
        TutorialFifthWeaponPhase = true;
        var spawner = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner != null) spawner.ClearAllEnemiesInScene();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText13))
            yield return ShowDialogueCoroutine(dialogueText13, botVoiceClip13);
        TutorialFifthWeaponSecondaryEnabled = true; // İkincil (yıldırım) aktif
        yield return StartCoroutine(TriggerHapticForWeaponIndex(4));
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        // 1 kill (yıldırım) sonra OnTutorialEnemyKilled → ShowDialogue14Coroutine
    }

    private IEnumerator ShowDialogue15And16Coroutine()
    {
        Time.timeScale = 0f;
        TutorialSixthWeaponPhase = true;
        var spawner6 = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner6 != null) spawner6.ClearAllEnemiesInScene();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText15))
            yield return ShowDialogueCoroutine(dialogueText15, botVoiceClip15);
        TutorialSixthWeaponSecondaryEnabled = true; // İkincil (yavaşlatma) aktif
        yield return StartCoroutine(TriggerHapticForWeaponIndex(5));
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        // 1 düşmana slow değince OnTutorialSecondaryUsed(5) → ShowDialogue16Coroutine
    }

    private IEnumerator ShowDialogue17And18Coroutine()
    {
        Time.timeScale = 0f;
        TutorialSeventhWeaponPhase = true;
        var spawner7 = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner7 != null) spawner7.ClearAllEnemiesInScene();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText17))
            yield return ShowDialogueCoroutine(dialogueText17, botVoiceClip17);
        TutorialSeventhWeaponSecondaryEnabled = true; // İkincil (kırbaç) aktif
        yield return StartCoroutine(TriggerHapticForWeaponIndex(6));
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        // 5 kırbaç kullanım sonra NotifyTutorialSeventhWeaponWhipUsed → TriggerShowDialogue18
    }

    private IEnumerator ShowDialogue19And20Coroutine()
    {
        Time.timeScale = 0f;
        TutorialEighthWeaponPhase = true;
        var spawner8 = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner8 != null) spawner8.ClearAllEnemiesInScene();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText19))
            yield return ShowDialogueCoroutine(dialogueText19, botVoiceClip19);
        TutorialEighthWeaponSecondaryEnabled = true; // 8. silah ikincil (varsa) aktif
        yield return StartCoroutine(TriggerHapticForWeaponIndex(7));
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        // 5 toy kullanım sonra NotifyTutorialEighthWeaponToyUsed → TriggerShowDialogue20
    }

    private IEnumerator ShowDialogue14Coroutine()
    {
        _dialogue14Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText14))
        {
            yield return ShowDialogueCoroutine(dialogueText14, botVoiceClip14);
            TutorialFifthWeaponGunChangeAfterEnabled = true;
            EnsureGunChangeAfterVisible();
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator ShowDialogue16Coroutine()
    {
        _dialogue16Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText16))
        {
            yield return ShowDialogueCoroutine(dialogueText16, botVoiceClip16);
            TutorialSixthWeaponGunChangeAfterEnabled = true;
            EnsureGunChangeAfterVisible();
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator ShowDialogue18Coroutine()
    {
        _dialogue18Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText18))
        {
            yield return ShowDialogueCoroutine(dialogueText18, botVoiceClip18);
            TutorialSeventhWeaponGunChangeAfterEnabled = true;
            EnsureGunChangeAfterVisible();
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator ShowDialogue20Coroutine()
    {
        _dialogue20Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText20))
        {
            yield return ShowDialogueCoroutine(dialogueText20, botVoiceClip20);
            TutorialEighthWeaponGunChangeAfterEnabled = true;
            EnsureGunChangeAfterVisible();
        }
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void EnsureGunChangeAfterVisible()
    {
        var ui = gunChangeAfterUI != null ? gunChangeAfterUI : FindObjectOfType<WeaponSwitchCountdownUI>();
        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            if (ui.countdownText != null) ui.countdownText.gameObject.SetActive(true);
        }
    }

    private IEnumerator ShowDialogue21Coroutine()
    {
        Time.timeScale = 0f;
        TutorialNinthWeaponPhase = true;
        TutorialNinthWeaponFireballUnlimited = true;
        var spawner9 = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner9 != null) spawner9.ClearAllEnemiesInScene();
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText21))
            yield return ShowDialogueCoroutine(dialogueText21, botVoiceClip21);
        TutorialNinthWeaponPrimaryEnabled = true; // Birincil (flame spray) aktif
        yield return StartCoroutine(TriggerHapticForNinthWeaponHand(isJoystick: false)); // Tetik eli haptic
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        // 1 kill (flame spray) sonra OnTutorialEnemyKilled → ShowDialogue22Coroutine
    }

    private IEnumerator ShowDialogue22Coroutine()
    {
        _dialogue22Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText22))
            yield return ShowDialogueCoroutine(dialogueText22, botVoiceClip22);
        TutorialNinthWeaponSecondaryEnabled = true; // İkincil (fireball) aktif
        TutorialNinthWeaponDialogue22Complete = true;
        yield return StartCoroutine(TriggerHapticForNinthWeaponHand(isJoystick: false)); // A tuşu eli haptic
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        _dialogue22Complete = true; // 22 bitti; 1 kill (fire/fireball) sonra 23 gösterilecek
    }

    private IEnumerator ShowDialogue23Coroutine()
    {
        _dialogue23Shown = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText23))
            yield return ShowDialogueCoroutine(dialogueText23, botVoiceClip23);
        TutorialNinthWeaponJoystickEnabled = true; // Joystick ile silah geçişi aktif
        TutorialNinthWeaponFireballUnlimited = false; // Fireball artık sınırlı
        yield return StartCoroutine(TriggerHapticForNinthWeaponHand(isJoystick: true)); // Sol joystick haptic
        var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
        if (wm != null) wm.SetJoystickWeaponSwitchEnabled(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        Time.timeScale = 1f;

        // 23. diyalog bitti: düşmanları temizle, normal spawn, tüm silahlar tam kullanılabilir
        TutorialCompleteFreehand = true;
        TutorialFifthWeaponSecondaryEnabled = true;
        TutorialFifthWeaponGunChangeAfterEnabled = true;
        TutorialSixthWeaponSecondaryEnabled = true;
        TutorialSixthWeaponGunChangeAfterEnabled = true;
        TutorialSeventhWeaponSecondaryEnabled = true;
        TutorialSeventhWeaponGunChangeAfterEnabled = true;
        TutorialEighthWeaponSecondaryEnabled = true;
        TutorialEighthWeaponGunChangeAfterEnabled = true;

        var spawner = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner != null)
        {
            spawner.ClearAllEnemiesInScene();
            spawner.StopTutorialPhase2aSpawning();
        }

        // 23. diyalog bitti: 45 sn geri sayım başlat (sonsuzdan 45 sn'e geçiş)
        if (GameManager.Instance != null)
            GameManager.Instance.StartTutorialFinalPhaseTimer();
    }

    /// <summary>23. diyalog sonrası 45 sn bittiğinde: oyun durur, 24. diyalog (Y ile ana menü) gösterilir.</summary>
    public void HandleTutorialFinalPhaseComplete()
    {
        if (_tutorialRetryInProgress) return;
        StartCoroutine(TutorialFinalPhaseCompleteCoroutine());
    }

    private IEnumerator TutorialFinalPhaseCompleteCoroutine()
    {
        _tutorialRetryInProgress = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(dialogueText24))
            yield return ShowDialogueCoroutine(dialogueText24, botVoiceClip24);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
        _tutorialRetryInProgress = false;
        _tutorialFinalPhaseEnded = true; // Y tuşu ile ana menüye dönüş aktif
    }

    /// <summary>Tutorial sırasında oyuncu öldüğünde: retry diyaloğu göster, düşmanları temizle, enerji/süre yenile, devam et.</summary>
    public void HandleTutorialDeath(Collider hitCollider)
    {
        if (_tutorialRetryInProgress) return;
        StartCoroutine(TutorialRetryCoroutine(retryType: TutorialRetryType.Death, hitCollider: hitCollider));
    }

    /// <summary>Tutorial sırasında mermi bitince: retry diyaloğu göster, düşmanları temizle, silahı yenile, devam et.</summary>
    public void HandleTutorialOutOfAmmo()
    {
        if (_tutorialRetryInProgress) return;
        StartCoroutine(TutorialRetryCoroutine(retryType: TutorialRetryType.OutOfAmmo, hitCollider: null));
    }

    private enum TutorialRetryType { Death, OutOfAmmo }

    private IEnumerator TutorialRetryCoroutine(TutorialRetryType retryType, Collider hitCollider)
    {
        _tutorialRetryInProgress = true;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
        string retryText;
        AudioClip voiceClip;
        if (retryType == TutorialRetryType.OutOfAmmo)
        {
            retryText = !string.IsNullOrEmpty(tutorialOutOfAmmoRetryDialogueText) ? tutorialOutOfAmmoRetryDialogueText : "No worries, I'll refill your ammo. Lets try again.";
            voiceClip = tutorialOutOfAmmoRetryVoiceClip;
        }
        else if (hitCollider == null)
        {
            retryText = !string.IsNullOrEmpty(tutorialEnergyOrTimeRetryDialogueText) ? tutorialEnergyOrTimeRetryDialogueText : "No worries, I'll refill your energy and time. Lets try again.";
            voiceClip = tutorialEnergyOrTimeRetryVoiceClip;
        }
        else
        {
            retryText = !string.IsNullOrEmpty(tutorialRetryDialogueText) ? tutorialRetryDialogueText : "No problem, lets start where we stay.";
            voiceClip = tutorialRetryVoiceClip;
        }
        yield return ShowDialogueCoroutine(retryText, voiceClip);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);

        if (retryType == TutorialRetryType.OutOfAmmo)
        {
            var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
            if (wm != null) wm.ReloadCurrentWeapon();
        }

        if (retryType == TutorialRetryType.Death)
        {
            var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
            if (wm != null && wm.GetCurrentWeaponIndex() == 8)
            {
                var ninth = wm.ninthWeapon;
                if (ninth != null)
                {
                    var spray = ninth.GetComponent<LastGunFlameSpray>();
                    if (spray != null) spray.RefillSprayEnergy();
                }
            }
            if (GameManager.Instance != null)
                GameManager.Instance.RefillTutorialTimer();
        }

        var spawner = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner != null)
            spawner.ClearAndSpawnFromPortalsForTutorialRetry();

        Time.timeScale = 1f;
        _tutorialRetryInProgress = false;
    }

    /// <summary>Silah index (0-8) için tetik eli haptic. 5-8. silahlar ikincil anlatımı sonrası.</summary>
    private IEnumerator TriggerHapticForWeaponIndex(int weaponIndex)
    {
        var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
        if (wm == null) yield break;
        GameObject weapon = weaponIndex switch
        {
            0 => wm.firstWeapon, 1 => wm.secondWeapon, 2 => wm.thirdWeapon, 3 => wm.fourthWeapon,
            4 => wm.fifthWeapon, 5 => wm.sixthWeapon, 6 => wm.seventhWeapon, 7 => wm.eighthWeapon,
            8 => wm.ninthWeapon, _ => null
        };
        bool isLeftHanded = false;
        if (weapon != null)
        {
            var gf = weapon.GetComponent<GunFire>();
            if (gf != null) isLeftHanded = gf.isLeftHanded;
            else
            {
                var spray = weapon.GetComponent<LastGunFlameSpray>();
                if (spray != null) isLeftHanded = spray.isLeftHanded;
            }
        }
        var ctrl = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(1f, 1f, ctrl);
        yield return new WaitForSecondsRealtime(0.15f);
        OVRInput.SetControllerVibration(0f, 0f, ctrl);
    }

    /// <summary>9. silah için haptic: birincil/ikincil = silah eli, joystick = sol el</summary>
    private IEnumerator TriggerHapticForNinthWeaponHand(bool isJoystick)
    {
        OVRInput.Controller ctrl = isJoystick ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        if (!isJoystick)
        {
            var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
            var ninth = wm != null ? wm.ninthWeapon : null;
            bool isLeftHanded = false;
            if (ninth != null)
            {
                var spray = ninth.GetComponent<LastGunFlameSpray>();
                if (spray != null) isLeftHanded = spray.isLeftHanded;
            }
            ctrl = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        }
        OVRInput.SetControllerVibration(1f, 1f, ctrl);
        yield return new WaitForSecondsRealtime(0.15f);
        OVRInput.SetControllerVibration(0f, 0f, ctrl);
    }

    private void OnDestroy()
    {
        TutorialActive = false;
        TutorialFifthWeaponPhase = false;
        TutorialFifthWeaponSecondaryEnabled = false;
        TutorialFifthWeaponGunChangeAfterEnabled = false;
        TutorialSixthWeaponPhase = false;
        TutorialSixthWeaponSecondaryEnabled = false;
        TutorialSixthWeaponGunChangeAfterEnabled = false;
        TutorialSeventhWeaponPhase = false;
        TutorialSeventhWeaponSecondaryEnabled = false;
        TutorialSeventhWeaponGunChangeAfterEnabled = false;
        TutorialEighthWeaponPhase = false;
        TutorialEighthWeaponSecondaryEnabled = false;
        TutorialEighthWeaponGunChangeAfterEnabled = false;
        TutorialNinthWeaponPhase = false;
        TutorialNinthWeaponPrimaryEnabled = false;
        TutorialNinthWeaponSecondaryEnabled = false;
        TutorialNinthWeaponJoystickEnabled = false;
        TutorialNinthWeaponFireballUnlimited = false;
        TutorialNinthWeaponDialogue22Complete = false;
        TutorialCompleteFreehand = false;
        TutorialFiringEnabled = true;
        TutorialUnlimitedAmmo = false;
    }

    private void OnTutorialWallDestroyed()
    {
        _wallBreakCount++;
    }

    private void OnTutorialEnemyKilled()
    {
        if (_waitingForFirstKill || _waitingForFourKills || _waitingForPostDialogue7Kills)
            _tutorialKillCount++;

        if (TutorialNinthWeaponPhase && _dialogue22Complete && !_dialogue23Shown &&
            (EnemyHealth.LastKillWasFromFlameSpray || (EnemyHealth.LastKillWeaponIndex == 8 && EnemyHealth.LastKillWasFromSecondary)))
        {
            TriggerShowDialogue23();
        }

        if (TutorialFifthWeaponPhase && !_dialogue14Shown && EnemyHealth.LastKillWeaponIndex == 4 && EnemyHealth.LastKillWasFromSecondary)
        {
            _fifthWeaponLightningKillCount++;
            if (_fifthWeaponLightningKillCount >= 5)
                TriggerShowDialogue14();
        }

    }

    private void OnTutorialSecondaryUsedHandler(int weaponIndex)
    {
        if (weaponIndex == 5 && TutorialSixthWeaponPhase && !_dialogue16Shown)
            StartCoroutine(ShowDialogue16Coroutine());
    }

    private void Start()
    {
        Time.timeScale = 0f;
        TutorialFiringEnabled = false; // Oyun başında mermi kapalı
        TutorialUnlimitedAmmo = true; // Ammo UI baştan kapalı, dialogue 6'dan önce sınırsız
        TutorialGunChangeAfterVisible = false; // Gun Change After dialogue 8'de açılacak
        foreach (var gun in FindObjectsOfType<GunFire>(true))
            gun.UpdateAmmoDisplay();
        var hud = holographicWeaponHUD != null ? holographicWeaponHUD : FindObjectOfType<HolographicWeaponHUD>(true);
        if (hud != null) hud.SetCanvasVisible(false); // Başta Canvas kapalı
        DestructibleMeshExperience.allowTriggerToBreakWalls = false; // Sadece mermi duvar kırsın
        var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
        if (wm != null)
            wm.weaponSwitchEnabled = false;
        StartCoroutine(TutorialFlowCoroutine());
    }

    private void SetupBotReferences()
    {
        if (dialogueTextUI == null)
            dialogueTextUI = _tutorialBot.GetComponentInChildren<TextMeshProUGUI>(true);
        if (dialoguePanel == null)
        {
            var canvas = _tutorialBot.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                dialoguePanel = canvas.gameObject;
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

    private IEnumerator TutorialFlowCoroutine()
    {
        // 1. Oyun başladıktan X saniye bekle (realtime - pause'da da çalışır)
        yield return new WaitForSecondsRealtime(delayBeforeSpawn);

        // 2. Bot spawn et
        if (tutorialBotPrefab != null && botSpawnPoint != null)
        {
            GameObject instance = Instantiate(tutorialBotPrefab, botSpawnPoint);
            _tutorialBot = instance.transform;
            _tutorialBot.localPosition = tutorialBotPrefab.transform.localPosition;
            _tutorialBot.localRotation = tutorialBotPrefab.transform.localRotation;
            _tutorialBot.localScale = tutorialBotPrefab.transform.localScale;
            _botBasePosition = _tutorialBot.position;
            _botBaseRotation = _tutorialBot.rotation;
            SetupBotReferences();
        }

        yield return IntroSequenceCoroutine();
    }

    private IEnumerator IntroSequenceCoroutine()
    {
        if (countdownTextUI != null)
            countdownTextUI.gameObject.SetActive(false);

        // Bot spawn olduktan sonra X saniye bekle, sonra konuş
        yield return new WaitForSecondsRealtime(delayBeforeDialogue);

        // 3. Konuşma panelini göster
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        if (dialogueTextUI != null)
            dialogueTextUI.gameObject.SetActive(true);

        // 4. Bot sesini çal
        if (botVoiceClip != null && audioSource != null)
        {
            audioSource.clip = botVoiceClip;
            audioSource.Play();
        }

        // 5. Yazma animasyonu veya anında göster
        float typingDuration = 0f;
        if (dialogueTextUI != null)
        {
            if (typingSpeedPerChar > 0f && !string.IsNullOrEmpty(dialogueText))
            {
                yield return TypewriterCoroutine(dialogueTextUI, dialogueText, typingSpeedPerChar);
                typingDuration = dialogueText.Length * typingSpeedPerChar;
            }
            else
            {
                dialogueTextUI.text = dialogueText;
            }
        }

        // 6. 1. diyalog bitti: 1 sn bekle, güçlü haptic, mermileri aç ve unpause
        yield return new WaitForSecondsRealtime(1f);
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        yield return new WaitForSecondsRealtime(0.15f);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        TutorialFiringEnabled = true;
        TutorialUnlimitedAmmo = true;
        foreach (var gun in FindObjectsOfType<GunFire>(true))
            gun.UpdateAmmoDisplay();
        Time.timeScale = 1f;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (dialogueTextUI != null)
            dialogueTextUI.gameObject.SetActive(false);

        _wallBreakCount = 0;
        float wallPhaseStart = Time.realtimeSinceStartup;
        float maxWait = Mathf.Max(delayAfterDialogue1BeforeDialogue2, 15f);
        while (Time.realtimeSinceStartup - wallPhaseStart < maxWait)
        {
            float elapsed = Time.realtimeSinceStartup - wallPhaseStart;
            if (elapsed >= delayAfterDialogue1BeforeDialogue2 && _wallBreakCount >= minWallBreakCountBeforeDialogue2)
                break;
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // 2. diyalog başlasın (pause yok - oyuncu hareket edip ateş edebilir)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        if (dialogueTextUI != null)
        {
            dialogueTextUI.gameObject.SetActive(true);
            dialogueTextUI.text = ""; // 1. diyalog metnini hemen temizle
        }

        // 7. İkinci konuşma varsa: bekle -> bot hareket et -> yeni metin söyle
        var spawner = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner == null)
        {
            Debug.LogError("TutorialIntroController: AdvancedPortalSpawner bulunamadı! Fallback akışa geçiliyor.");
        }

        if (!string.IsNullOrEmpty(dialogueText2) && _tutorialBot != null && spawner != null)
        {
            yield return new WaitForSecondsRealtime(delayBetweenDialogues);

            // Bot pozisyonunu değiştir
            Vector3 targetPos = _botBasePosition + _botBaseRotation * botSecondPositionOffset;
            yield return MoveBotToPositionCoroutine(targetPos, botPositionMoveDuration);
            _botBasePosition = targetPos;

            // 2. diyalog başladıktan 1 sn sonra Portal A açılsın (paralel)
            if (spawner != null)
                StartCoroutine(OpenPortalAAfterDelay(spawner, delayAfterDialogue2StartBeforePortalA));

            // İkinci metni göster ve ses çal (2. diyalog)
            if (dialogueTextUI != null)
                dialogueTextUI.text = "";
            if (botVoiceClip2 != null && audioSource != null)
            {
                audioSource.clip = botVoiceClip2;
                audioSource.Play();
            }
            float typingDuration2 = 0f;
            if (dialogueTextUI != null)
            {
                if (typingSpeedPerChar > 0f)
                {
                    yield return TypewriterCoroutine(dialogueTextUI, dialogueText2, typingSpeedPerChar);
                    typingDuration2 = dialogueText2.Length * typingSpeedPerChar;
                }
                else
                {
                    dialogueTextUI.text = dialogueText2;
                }
            }
            float audioRemaining2 = 0f;
            if (botVoiceClip2 != null && audioSource != null)
                audioRemaining2 = Mathf.Max(0f, botVoiceClip2.length - typingDuration2);
            yield return new WaitForSecondsRealtime(audioRemaining2 + dialogueEndBuffer);

            // TEST: 2. diyalog bitince Y ile ana menüye dönüşü test et
            if (testMainMenuReturnAfterDialogue2)
            {
                if (dialoguePanel != null) dialoguePanel.SetActive(false);
                if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(false);
                _tutorialFinalPhaseEnded = true;
                Time.timeScale = 0f;
                yield break; // Tutorial akışını durdur, Y tuşu ile ana menüye dön
            }

            // 2. diyalog bitti, 3. diyaloga geç: düşman spawn, unpause, dialogue 3 göster
            if (spawner != null)
            {
                GameObject firstEnemy = spawner.SpawnSingleEnemyAtPortalA(addTutorialController: true);
                _firstEnemyGameObject = firstEnemy;
                _firstEnemyController = firstEnemy != null ? firstEnemy.GetComponent<TutorialFirstEnemyController>() : null;
            }
            _tutorialKillCount = 0;
            _waitingForFirstKill = true;
            Time.timeScale = 1f; // Unpause - düşman gelmeye başlasın
            yield return ShowDialogueCoroutine(dialogueText3, botVoiceClip3); // Dialogue 3 (oyun çalışıyor)

            // Kill VEYA 4 sn race: hangisi önce gelirse
            float elapsed = 0f;
            while (elapsed < enemyMoveDurationBeforeDialogue4 && _tutorialKillCount < 1)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_tutorialKillCount >= 1)
            {
                // Erken öldürdü, Dialogue 4 atla
            }
            else
            {
                // 4 sn doldu, düşmanı durdur, Dialogue 4 (ipucu) - PAUSE YOK, oyun devam eder
                if (_firstEnemyController != null) _firstEnemyController.Stop();
                if (dialoguePanel != null) dialoguePanel.SetActive(true);
                if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
                yield return ShowDialogueCoroutine(dialogueText4, botVoiceClip4);
                if (_firstEnemyController != null) _firstEnemyController.Resume();
                yield return new WaitUntil(() => _tutorialKillCount >= 1);
            }
            _waitingForFirstKill = false;

            // Düşman Destroy olana kadar bekle (ses/VFX bitene kadar)
            if (_firstEnemyGameObject != null)
                yield return new WaitUntil(() => _firstEnemyGameObject == null);

            // Düşman öldürüldü - dialogue 5
            Time.timeScale = 0f;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
            yield return ShowDialogueCoroutine(dialogueText5, botVoiceClip5);

            // 5. diyalog bitti: Portal B ve C aç, bekle, sonra dialogue 7
            if (spawner != null)
                yield return spawner.StartCoroutine(spawner.OpenPortalBAndC());

            Time.timeScale = 1f;
            yield return new WaitForSeconds(delayAfterPortalsBCBeforeDialogue7);

            // Pause, dialogue 7 başlasın
            Time.timeScale = 0f;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
            // Dialogue 7 başlat - hemen unpause + düşman spawn, diyalog devam ederken düşmanları görsün
            if (!string.IsNullOrEmpty(dialogueText7))
            {
                if (dialogueTextUI != null) dialogueTextUI.text = "";
                if (botVoiceClip7 != null && audioSource != null) { audioSource.clip = botVoiceClip7; audioSource.Play(); }
                var typing7 = StartCoroutine(typingSpeedPerChar > 0f
                    ? TypewriterCoroutine(dialogueTextUI, dialogueText7, typingSpeedPerChar)
                    : InstantSetTextCoroutine(dialogueTextUI, dialogueText7));
                Time.timeScale = 1f;
                if (spawner != null && spawner.tutorialMode)
                    spawner.StartTutorialPhase2aFromBAndC();
                yield return typing7;
                float audioRemaining7 = botVoiceClip7 != null && audioSource != null
                    ? Mathf.Max(0f, botVoiceClip7.length - dialogueText7.Length * (typingSpeedPerChar > 0f ? typingSpeedPerChar : 0f))
                    : 0f;
                yield return new WaitForSecondsRealtime(audioRemaining7 + dialogueEndBuffer);
            }
            else
            {
                Time.timeScale = 1f;
                if (spawner != null && spawner.tutorialMode)
                    spawner.StartTutorialPhase2aFromBAndC();
            }

            _tutorialKillCount = 0;
            _waitingForPostDialogue7Kills = true;
            yield return new WaitUntil(() => _tutorialKillCount >= killsAfterDialogue7BeforeDialogue8);
            _waitingForPostDialogue7Kills = false;

            if (spawner != null)
                spawner.StopTutorialPhase2aSpawning();
            Time.timeScale = 0f;

            // Gun change after fonksiyonunu aktif et (dialogue 8'de anlatılacak)
            var wm = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
            if (wm != null)
            {
                wm.weaponSwitchEnabled = true;
                wm.CheckWeaponSwitch(); // Eşik aşılmışsa hemen silah değişimi tetikle
            }

            // Dialogue 8: Silah değişimi anlatımı - Gun Change After UI diyalogla beraber açılsın
            TutorialGunChangeAfterVisible = true;
            var gunChangeUI = gunChangeAfterUI != null ? gunChangeAfterUI : FindObjectOfType<WeaponSwitchCountdownUI>();
            if (gunChangeUI != null && gunChangeUI.countdownText != null)
                gunChangeUI.countdownText.gameObject.SetActive(true);
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(dialogueText8))
                yield return ShowDialogueCoroutine(dialogueText8, botVoiceClip8);

            // 8. diyalog bitti: unpause, spawn devam etsin, silah değişene kadar bekle
            Time.timeScale = 1f;
            if (spawner != null && spawner.tutorialMode)
                spawner.StartTutorialPhase2aFromBAndC();

            // Silah değişimini bekle (max 60 sn timeout)
            float weaponSwitchTimeout = 60f;
            float weaponSwitchElapsed = 0f;
            yield return new WaitUntil(() =>
            {
                weaponSwitchElapsed += Time.deltaTime;
                if (weaponSwitchElapsed >= weaponSwitchTimeout) return true;
                var w = WeaponManager.Instance != null ? WeaponManager.Instance : FindObjectOfType<WeaponManager>();
                return w != null && w.GetCurrentWeaponIndex() >= 1;
            });

            if (spawner != null)
                spawner.StopTutorialPhase2aSpawning();
            Time.timeScale = 0f;

            // Dialogue 9: Silah değiştikten sonra
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (dialogueTextUI != null) dialogueTextUI.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(dialogueText9))
                yield return ShowDialogueCoroutine(dialogueText9, botVoiceClip9);

            // Dialogue 6 öncesi: sınırsız mermiyi kapat, ammo UI göster
            TutorialUnlimitedAmmo = false;
            foreach (var gun in FindObjectsOfType<GunFire>(true))
                gun.UpdateAmmoDisplay();

            // Dialogue 6: "3 portaldan gelen anomalilerden 4 tane indir"
            yield return ShowDialogueCoroutine(dialogueText6, botVoiceClip6);

            // Konuşma panelini gizle, 3-2-1-GO countdown
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (dialogueTextUI != null)
                dialogueTextUI.gameObject.SetActive(false);
            if (countdownTextUI != null)
            {
                countdownTextUI.gameObject.SetActive(true);
                for (int i = 3; i >= 1; i--)
                {
                    countdownTextUI.text = i.ToString();
                    yield return new WaitForSecondsRealtime(1f);
                }
                countdownTextUI.text = "GO!";
                yield return new WaitForSecondsRealtime(0.5f);
                countdownTextUI.gameObject.SetActive(false);
            }

            // Unpause - süre 23. diyaloga kadar sonsuz, 23. diyalog sonrası 45 sn geri sayım başlar
            Time.timeScale = 1f;

            _tutorialKillCount = 0;
            _waitingForFourKills = true;
            if (spawner != null && spawner.tutorialMode)
                spawner.StartTutorialPhase2Spawning(4, () => _tutorialKillCount);

            yield return new WaitUntil(() => _tutorialKillCount >= 4);
            _waitingForFourKills = false;
            if (spawner != null)
            {
                spawner.StopTutorialPhase2Spawning();
                spawner.StartNormalSpawningFromTutorial();
            }
        }
        else
        {
            // dialogueText2 boşsa veya spawner yoksa fallback: Portal A aç, countdown, unpause
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (dialogueTextUI != null)
                dialogueTextUI.gameObject.SetActive(false);
            var spawnerFallback = portalSpawner != null ? portalSpawner : FindObjectOfType<AdvancedPortalSpawner>();
            if (spawnerFallback != null)
            {
                yield return spawnerFallback.StartCoroutine(spawnerFallback.OpenPortalAAndWait());
                if (countdownTextUI != null)
                {
                    countdownTextUI.gameObject.SetActive(true);
                    for (int i = 3; i >= 1; i--)
                    {
                        countdownTextUI.text = i.ToString();
                        yield return new WaitForSecondsRealtime(1f);
                    }
                    countdownTextUI.text = "GO!";
                    yield return new WaitForSecondsRealtime(0.5f);
                    countdownTextUI.gameObject.SetActive(false);
                }
                Time.timeScale = 1f;
                if (spawnerFallback.tutorialMode)
                    spawnerFallback.StartTutorialEnemySpawn();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        Debug.Log("Tutorial intro tamamlandı - oyun başladı!");
    }

    private IEnumerator OpenPortalAAfterDelay(AdvancedPortalSpawner spawner, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (spawner != null)
            spawner.OpenPortalA();
    }

    private IEnumerator ShowDialogueCoroutine(string text, AudioClip clip)
    {
        if (dialogueTextUI != null)
            dialogueTextUI.text = "";
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
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

        float audioRemaining = 0f;
        if (clip != null && audioSource != null)
            audioRemaining = Mathf.Max(0f, clip.length - typingDuration);
        yield return new WaitForSecondsRealtime(audioRemaining + dialogueEndBuffer);
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

    private IEnumerator InstantSetTextCoroutine(TextMeshProUGUI textUI, string text)
    {
        if (textUI != null) textUI.text = text;
        yield break;
    }

    private IEnumerator FadeOutAndLoadMainMenu()
    {
        // Tam ekran siyah overlay oluştur
        var canvasObj = new GameObject("TutorialFadeOverlay");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        var imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        var rect = imageObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageObj.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < mainMenuFadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / mainMenuFadeOutDuration);
            t = 1f - (1f - t) * (1f - t); // EaseOutQuad
            image.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        image.color = new Color(0f, 0f, 0f, 1f);

        // Tutorial tamamlandı - bir daha oyun tutorial ile başlamasın
        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        string targetScene;
        if (!useTutorialAndUserNameFlow)
            targetScene = string.IsNullOrEmpty(userNameSceneName) ? "UserName" : userNameSceneName;
        else
            targetScene = PlayerPrefs.GetInt(UserNameController.UserNameSetKey, 0) != 1
                ? (string.IsNullOrEmpty(userNameSceneName) ? "UserName" : userNameSceneName)
                : (string.IsNullOrEmpty(mainMenuSceneName) ? "UI" : mainMenuSceneName);
        SceneManager.LoadScene(targetScene);
    }

    private IEnumerator MoveBotToPositionCoroutine(Vector3 targetPos, float duration)
    {
        if (_tutorialBot == null || duration <= 0f)
        {
            if (_tutorialBot != null)
                _tutorialBot.position = targetPos;
            yield break;
        }
        Vector3 startPos = _tutorialBot.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t); // SmoothStep
            _tutorialBot.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        _tutorialBot.position = targetPos;
    }

    /// <summary>Test: Son diyaloğa atla; 24. diyalog gösterilir, Y ile ana menüye dönüş aktif olur.</summary>
    private void SkipTutorialToEnd()
    {
        if (_tutorialFinalPhaseEnded) return;
        Time.timeScale = 0f;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (dialogueTextUI != null)
        {
            dialogueTextUI.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(dialogueText24)) dialogueTextUI.text = dialogueText24;
        }
        _tutorialFinalPhaseEnded = true;
    }

    private void Update()
    {
        // Test: sağ el grip ile son diyaloğa atla
        if (skipTutorialEnabled && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
            SkipTutorialToEnd();

        // 24. diyalog bittikten sonra: sol kontrolcü Y tuşuna basılı tutarak ana menüye dön
        if (_tutorialFinalPhaseEnded && OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            // Sol kontrolcü titreşim (basılı tutarken)
            OVRInput.SetControllerVibration(0.5f, 0.7f, OVRInput.Controller.LTouch);
            _mainMenuHoldTime += Time.unscaledDeltaTime;
            if (_mainMenuHoldTime >= 2f)
            {
                OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
                StartCoroutine(FadeOutAndLoadMainMenu());
                return;
            }
        }
        else
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
            _mainMenuHoldTime = 0f;
        }

        if (_tutorialBot == null) return;

        float t = Time.unscaledTime * 1.2f;

        // Kendi ekseninde minik rotasyon (Y: 10° ile -5° arası) - önce rotasyon
        if (botRotationSway.x != botRotationSway.y)
        {
            float rotT = (Mathf.Sin(t * 0.8f) + 1f) * 0.5f;
            float angleY = Mathf.Lerp(botRotationSway.y, botRotationSway.x, rotT);
            _tutorialBot.rotation = _botBaseRotation * Quaternion.Euler(0f, angleY, 0f);
        }

        // Pozisyon salınımı
        if (botSwayAmount > 0f)
        {
            float vertical = Mathf.Sin(t) * botSwayAmount;
            float horizontal = Mathf.Sin(t + 2.5f) * botSwayAmount * 0.7f;
            Vector3 rightDir = _botBaseRotation * Vector3.right;
            _tutorialBot.position = _botBasePosition + Vector3.up * vertical + rightDir * horizontal;
        }
    }

}
