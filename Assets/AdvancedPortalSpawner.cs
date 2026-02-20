
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdvancedPortalSpawner : MonoBehaviour
{
    [Header("Portal Prefabs")]
    public GameObject portalPrefabA, portalPrefabB, portalPrefabC;

    [Header("Portal Spawn Locations")]
    public GameObject portalLocationA;
    public GameObject portalLocationB;
    public GameObject portalLocationC;

    private Vector3 portalAPosition;
    private Vector3 portalBPosition;
    private Vector3 portalCPosition;
    private Quaternion portalARotation;
    private Quaternion portalBRotation;
    private Quaternion portalCRotation;

    [Header("Enemy Prefabs")]
    public GameObject tur1Enemy;
    public GameObject tur2Enemy;
    public GameObject tur3Enemy;
    [Tooltip("4. düşman - Stage 2'den itibaren nadiren spawn olur. Prefab'ı Inspector'dan atayın.")]
    public GameObject tur4Enemy;

    [Header("Damage Feedback Prefabs")]
    [Tooltip("Tek collider sistemli yeni düşmanlar için genel floating text")]
    public GameObject floatingTextPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Portallar açıldıktan sonra ilk düşman spawn'ı için bekleme süresi")]
    public float delayBeforeFirstSpawn = 2f;
    [Tooltip("Düşmanların portalın ne kadar yukarısından spawn olacağı (portalın ortasına hizalamak için)")]
    public float spawnHeightOffset = 1.5f;

    [Header("Stage (from GameBalanceManager when present; else fallbacks)")]
    [Tooltip("Stage 2 başlangıcı - fallback when no balance")]
    public int stage2KillThreshold = 36;
    [Tooltip("Stage 3 başlangıcı - fallback when no balance")]
    public int stage3KillThreshold = 90;

    [Header("Spawn Intervals per Stage (fallback when no balance)")]
    public float stage1SpawnInterval = 2.5f;
    public float stage2SpawnInterval = 2.0f;
    public float stage3SpawnInterval = 1.4f;

    [Header("Enemy Speed per Stage (fallback when no balance)")]
    public float stage1EnemySpeed = 3.0f;
    public float stage2EnemySpeed = 3.5f;
    public float stage3EnemySpeed = 4.0f;

    [Header("Enemy HP fallback (when no GameBalanceManager)")]
    public float tur1HP = 100f;
    public float tur2HP = 150f;
    public float tur3HP = 200f;
    public float tur4HP = 0f;

    [Header("Enemy Score fallback (when no balance)")]
    public int tur1Score = 25;
    public int tur2Score = 50;
    public int tur3Score = 100;
    public int tur4Score = 150;

    [Header("Tutorial Modu")]
    [Tooltip("Açıksa normal akış başlamaz - portallar kapalı kalır. TutorialIntroController OpenPortalA() ile açar")]
    public bool tutorialMode = false;
    [Tooltip("B ve C portallarından spawn: interval çarpanı (2 = normalin 2 katı yavaş)")]
    public float tutorialBCSpawnIntervalMultiplier = 2.5f;

    [Header("Portal Spawn Animation")]
    [Tooltip("Oyun başladıktan kaç saniye sonra portallar açılsın")]
    public float portalSpawnStartDelay = 7.5f;
    [Tooltip("Portal açılma animasyon süresi")]
    public float portalSpawnDuration = 1.5f;
    [Tooltip("Portallar arası açılma gecikmesi")]
    public float portalSpawnDelay = 0.3f;
    [Tooltip("Açılırken dönme miktarı (derece)")]
    public float portalSpawnRotation = 360f;
    [Tooltip("Overshoot (bounce) efekti için")]
    public float portalOvershoot = 1.1f;

    private List<string> portals = new List<string> { "A", "B", "C" };
    private Dictionary<string, GameObject> portalPrefabs;
    private Dictionary<string, int> consecutivePortalCounts = new();
    private Dictionary<string, GameObject> lastEnemyPerPortal = new();

    private bool isGameOver = false;
    private int currentStage = 1;
    private int totalKillCount = 0;

    [Header("Bag system (12 slots, refill when < 1)")]
    private List<GameObject> spawnBag = new List<GameObject>();

    void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += OnEnemyKilledForStage;
    }

    void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilledForStage;
    }

    void Start()
    {
        // GameObject referanslarından pozisyon ve rotation değerlerini al
        portalAPosition = portalLocationA != null ? portalLocationA.transform.position : new Vector3(5.2f, -4.3f, -87.1f);
        portalARotation = portalLocationA != null ? portalLocationA.transform.rotation : Quaternion.Euler(0, 90, 0);

        portalBPosition = portalLocationB != null ? portalLocationB.transform.position : new Vector3(5.2f, -4.5f, 105f);
        portalBRotation = portalLocationB != null ? portalLocationB.transform.rotation : Quaternion.Euler(0, 90, 0);

        portalCPosition = portalLocationC != null ? portalLocationC.transform.position : new Vector3(-116.2f, -4.6f, 1.8f);
        portalCRotation = portalLocationC != null ? portalLocationC.transform.rotation : Quaternion.identity;

        portalPrefabs = new Dictionary<string, GameObject>
        {
            { "A", portalPrefabA },
            { "B", portalPrefabB },
            { "C", portalPrefabC }
        };

        // Tutorial modunda otomatik başlatma - TutorialIntroController OpenPortalA() çağıracak
        if (!tutorialMode)
            StartCoroutine(StartGameSequence());
    }

    /// <summary>
    /// Tutorial için sadece Portal A'yı aç. Pause (timeScale=0) sırasında da çalışır.
    /// TutorialIntroController 2. diyalogdan 3 sn sonra çağırır.
    /// </summary>
    public void OpenPortalA()
    {
        StartCoroutine(OpenPortalACoroutine());
    }

    /// <summary>Tutorial fallback: Portal A açılana kadar bekle (yield return ile kullanılır).</summary>
    public IEnumerator OpenPortalAAndWait()
    {
        yield return OpenPortalACoroutine();
    }

    /// <summary>Tutorial: Portal A açıldığında WayManager vb. dinleyebilir.</summary>
    public static System.Action OnPortalAOpened;

    private IEnumerator OpenPortalACoroutine()
    {
        if (portalPrefabA == null) yield break;

        GameObject portalA = Instantiate(portalPrefabA, portalAPosition, portalARotation);
        yield return AnimatePortalSpawnUnscaled(portalA, portalARotation);
        _portalAOpened = true;
        OnPortalAOpened?.Invoke();
    }

    /// <summary>
    /// Tutorial: Sadece Portal A'dan düşman spawn başlat. Unpause sonrası TutorialIntroController çağırır.
    /// </summary>
    public void StartTutorialEnemySpawn()
    {
        if (tutorialMode)
            StartCoroutine(SpawnEnemiesFromPortalAOnly());
    }

    /// <summary>
    /// Tutorial: Portal A'dan tek düşman spawn et.
    /// addTutorialController=true ise TutorialFirstEnemyController eklenir (stop/resume, invulnerability).
    /// </summary>
    public GameObject SpawnSingleEnemyAtPortalA(bool addTutorialController = false)
    {
        GameObject enemyPrefab = tur1Enemy != null ? tur1Enemy : tur2Enemy;
        if (enemyPrefab == null) return null;

        Vector3 spawnPos = portalAPosition;
        spawnPos.y += spawnHeightOffset;

        Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
        directionToPlayer.y = 0;
        Quaternion spawnRotation = directionToPlayer != Vector3.zero ? Quaternion.LookRotation(directionToPlayer) : Quaternion.identity;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
        enemy.tag = "Enemy";
        SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

        if (addTutorialController && enemy.GetComponent<TutorialFirstEnemyController>() == null)
            enemy.AddComponent<TutorialFirstEnemyController>();

        return enemy;
    }

    /// <summary>
    /// Tutorial: Portal B'yi aç. Pause sırasında da çalışır.
    /// </summary>
    public void OpenPortalB()
    {
        StartCoroutine(OpenPortalCoroutine("B", portalBPosition, portalBRotation));
    }

    /// <summary>
    /// Tutorial: Portal C'yi aç. Pause sırasında da çalışır.
    /// </summary>
    public void OpenPortalC()
    {
        StartCoroutine(OpenPortalCoroutine("C", portalCPosition, portalCRotation));
    }

    /// <summary>
    /// Tutorial: Portal B ve C'yi sırayla aç. Coroutine olarak yield return edilebilir.
    /// </summary>
    public IEnumerator OpenPortalBAndC()
    {
        yield return OpenPortalBAndCCoroutine();
    }

    private IEnumerator OpenPortalCoroutine(string portalId, Vector3 pos, Quaternion rot)
    {
        GameObject prefab = portalId == "B" ? portalPrefabB : portalPrefabC;
        if (prefab == null) yield break;

        GameObject portal = Instantiate(prefab, pos, rot);
        yield return AnimatePortalSpawnUnscaled(portal, rot);
    }

    /// <summary>Tutorial: Portal B ve C açıldığında WayManager vb. dinleyebilir.</summary>
    public static System.Action OnPortalBAndCOpened;

    private IEnumerator OpenPortalBAndCCoroutine()
    {
        if (portalPrefabB != null)
        {
            GameObject portalB = Instantiate(portalPrefabB, portalBPosition, portalBRotation);
            StartCoroutine(AnimatePortalSpawnUnscaled(portalB, portalBRotation));
        }
        yield return new WaitForSecondsRealtime(portalSpawnDelay);
        if (portalPrefabC != null)
        {
            GameObject portalC = Instantiate(portalPrefabC, portalCPosition, portalCRotation);
            yield return AnimatePortalSpawnUnscaled(portalC, portalCRotation);
        }
        _portalBAndCOpened = true;
        OnPortalBAndCOpened?.Invoke();
    }

    private bool _stopTutorialPhase2Spawning;
    private bool _stopTutorialPhase2aSpawning;

    private bool _portalAOpened;
    private bool _portalBAndCOpened;

    /// <summary>
    /// Tutorial: Sadece B ve C portallarından düşman spawn et (normalden daha az - yavaş interval).
    /// Dialogue 7 öncesi düşmanların ilerlemesi için.
    /// </summary>
    public void StartTutorialPhase2aFromBAndC()
    {
        _stopTutorialPhase2aSpawning = false;
        StartCoroutine(SpawnTutorialPhase2aFromBAndCCoroutine());
    }

    public void StopTutorialPhase2aSpawning()
    {
        _stopTutorialPhase2aSpawning = true;
    }

    private IEnumerator SpawnTutorialPhase2aFromBAndCCoroutine()
    {
        yield return new WaitForSecondsRealtime(delayBeforeFirstSpawn);

        var bcPortals = new List<string> { "B", "C" };
        float interval = stage1SpawnInterval * Mathf.Max(1f, tutorialBCSpawnIntervalMultiplier);

        while (!_stopTutorialPhase2aSpawning && !isGameOver)
        {
            string portal = bcPortals[Random.Range(0, bcPortals.Count)];
            Vector3 spawnPos = GetPortalPosition(portal);
            spawnPos.y += spawnHeightOffset;

            GameObject enemyPrefab = tur1Enemy != null ? tur1Enemy : tur2Enemy;
            if (enemyPrefab == null) break;

            Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
            directionToPlayer.y = 0;
            Quaternion spawnRotation = directionToPlayer != Vector3.zero ? Quaternion.LookRotation(directionToPlayer) : Quaternion.identity;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
            enemy.tag = "Enemy";
            SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

            yield return new WaitForSecondsRealtime(interval);
        }
    }

    /// <summary>
    /// Tutorial: 3 portaldan düşman spawn et. targetKillCount'a ulaşılınca durur (TutorialIntroController OnEnemyKilled ile sayar).
    /// </summary>
    public void StartTutorialPhase2Spawning(int targetKillCount, System.Func<int> getCurrentTutorialKillCount)
    {
        _stopTutorialPhase2Spawning = false;
        StartCoroutine(SpawnTutorialPhase2Coroutine(targetKillCount, getCurrentTutorialKillCount));
    }

    public void StopTutorialPhase2Spawning()
    {
        _stopTutorialPhase2Spawning = true;
    }

    /// <summary>Tutorial 4-kill fazı bittikten sonra normal oyun spawn'ına geç (süre dolana kadar devam).</summary>
    public void StartNormalSpawningFromTutorial()
    {
        StartCoroutine(SpawnEnemiesContinuously());
    }

    /// <summary>Sahnedeki tüm düşmanları kaldır. 5. silah vb. geçişlerde temiz başlangıç için.</summary>
    public void ClearAllEnemiesInScene()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            if (e != null) Object.Destroy(e);
        }
    }

    /// <summary>Tutorial retry: düşmanları temizleyip sadece açık portallardan hemen spawn et. Diğer aşamaya kadar normal spawn devam eder.</summary>
    public void ClearAndSpawnFromPortalsForTutorialRetry()
    {
        ClearAllEnemiesInScene();
        var openPortals = new List<string>();
        if (_portalAOpened) openPortals.Add("A");
        if (_portalBAndCOpened) { openPortals.Add("B"); openPortals.Add("C"); }
        if (openPortals.Count == 0) openPortals.Add("A");
        for (int i = 0; i < openPortals.Count; i++)
        {
            string portal = openPortals[i];
            Vector3 spawnPos = GetPortalPosition(portal);
            spawnPos.y += spawnHeightOffset;

            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            if (enemyPrefab == null) continue;

            Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
            directionToPlayer.y = 0;
            Quaternion spawnRotation = directionToPlayer != Vector3.zero ? Quaternion.LookRotation(directionToPlayer) : Quaternion.identity;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
            enemy.tag = "Enemy";
            SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

            if (!consecutivePortalCounts.ContainsKey(portal)) consecutivePortalCounts[portal] = 0;
            consecutivePortalCounts[portal]++;
            foreach (var p in portals.Where(p => p != portal))
                consecutivePortalCounts[p] = 0;
            lastEnemyPerPortal[portal] = enemyPrefab;
        }
    }

    private IEnumerator SpawnTutorialPhase2Coroutine(int targetKillCount, System.Func<int> getCurrentTutorialKillCount)
    {
        yield return new WaitForSeconds(delayBeforeFirstSpawn);

        while (!_stopTutorialPhase2Spawning && !isGameOver)
        {
            if (getCurrentTutorialKillCount != null && getCurrentTutorialKillCount() >= targetKillCount)
                break;

            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            string portal = GetPreferredPortalForEnemy(enemyPrefab);
            Vector3 spawnPos = GetPortalPosition(portal);
            spawnPos.y += spawnHeightOffset;

            if (enemyPrefab == null) break;

            Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
            directionToPlayer.y = 0;
            Quaternion spawnRotation = directionToPlayer != Vector3.zero ? Quaternion.LookRotation(directionToPlayer) : Quaternion.identity;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
            enemy.tag = "Enemy";
            SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

            if (!consecutivePortalCounts.ContainsKey(portal))
                consecutivePortalCounts[portal] = 0;
            consecutivePortalCounts[portal]++;
            foreach (var p in portals.Where(p => p != portal))
                consecutivePortalCounts[p] = 0;
            lastEnemyPerPortal[portal] = enemyPrefab;

            yield return new WaitForSeconds(GetSpawnInterval());
        }
    }

    private IEnumerator AnimatePortalSpawnUnscaled(GameObject portal, Quaternion targetRotation)
    {
        if (portal == null) yield break;

        Transform portalTransform = portal.transform;
        Vector3 targetScale = portalTransform.localScale;
        portalTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        float startRotationY = targetRotation.eulerAngles.y - portalSpawnRotation;

        while (elapsed < portalSpawnDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / portalSpawnDuration;
            float easedT = EaseOutBack(t, portalOvershoot);
            portalTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easedT);
            float currentRotationY = Mathf.Lerp(startRotationY, targetRotation.eulerAngles.y, EaseOutCubic(t));
            portalTransform.rotation = Quaternion.Euler(targetRotation.eulerAngles.x, currentRotationY, targetRotation.eulerAngles.z);
            yield return null;
        }

        portalTransform.localScale = targetScale;
        portalTransform.rotation = targetRotation;
        yield return StartCoroutine(PulseEffectUnscaled(portalTransform, targetScale));

        if (GameManager.Instance != null)
            GameManager.Instance.PlayPortalOpenSound();
    }

    private IEnumerator PulseEffectUnscaled(Transform portalTransform, Vector3 baseScale)
    {
        float pulseDuration = 0.2f;
        float pulseAmount = 1.05f;
        float elapsed = 0f;
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (pulseDuration / 2f);
            portalTransform.localScale = Vector3.Lerp(baseScale, baseScale * pulseAmount, t);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (pulseDuration / 2f);
            portalTransform.localScale = Vector3.Lerp(baseScale * pulseAmount, baseScale, t);
            yield return null;
        }
        portalTransform.localScale = baseScale;
    }

    private IEnumerator SpawnEnemiesFromPortalAOnly()
    {
        yield return new WaitForSecondsRealtime(delayBeforeFirstSpawn);

        while (!isGameOver && !TutorialIntroController.TutorialCompleteFreehand)
        {
            GameObject enemyPrefab = tur1Enemy != null ? tur1Enemy : tur2Enemy;
            if (enemyPrefab == null) break;

            Vector3 spawnPos = portalAPosition;
            spawnPos.y += spawnHeightOffset;

            Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
            directionToPlayer.y = 0;
            Quaternion spawnRotation = directionToPlayer != Vector3.zero ? Quaternion.LookRotation(directionToPlayer) : Quaternion.identity;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
            enemy.tag = "Enemy";
            SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

            if (!consecutivePortalCounts.ContainsKey("A")) consecutivePortalCounts["A"] = 0;
            consecutivePortalCounts["A"]++;
            lastEnemyPerPortal["A"] = enemyPrefab;

            yield return new WaitForSecondsRealtime(stage1SpawnInterval);
        }
    }

    IEnumerator StartGameSequence()
    {
        // Portallar için bekle
        yield return new WaitForSeconds(portalSpawnStartDelay);
        
        // Portalları animasyonla spawn et
        yield return StartCoroutine(SpawnPortalsWithAnimation());
        
        // Portallar açıldıktan sonra düşman spawn'ı başlat
        StartCoroutine(SpawnEnemiesContinuously());
    }

    void OnEnemyKilledForStage()
    {
        totalKillCount++;
        UpdateStage();
    }

    /// <summary>Skor veya kill'e göre stage günceller. AddScore'tan (skor modunda) veya OnEnemyKilled'den çağrılır.</summary>
    public void RefreshStage()
    {
        UpdateStage();
    }

    void UpdateStage()
    {
        int newStage = currentStage;
        if (GameBalanceManager.Instance != null)
        {
            if (GameBalanceManager.Instance.UseScoreForStage && GameManager.Instance != null)
                newStage = GameBalanceManager.Instance.GetStageFromScore(GameManager.Instance.score);
            else
                newStage = GameBalanceManager.Instance.GetStageFromKill(totalKillCount);
        }
        else
        {
            if (currentStage == 1 && totalKillCount >= stage2KillThreshold) newStage = 2;
            else if (currentStage == 2 && totalKillCount >= stage3KillThreshold) newStage = 3;
        }
        if (newStage != currentStage)
        {
            currentStage = newStage;
            RefillSpawnBag();
            if (GameManager.Instance != null)
                GameManager.Instance.PlayStageSound(currentStage);
            Debug.Log($"[STAGE] Stage {currentStage}'e geçildi! Kill: {totalKillCount}, Spawn Interval: {GetSpawnInterval()}s, Enemy Speed: {GetEnemySpeed()}");
        }
    }

    float GetSpawnInterval()
    {
        if (TutorialIntroController.TutorialCompleteFreehand)
        {
            if (GameBalanceManager.Instance != null)
                return GameBalanceManager.Instance.GetSpawnInterval(currentStage);
            return currentStage switch { 1 => stage1SpawnInterval, 2 => stage2SpawnInterval, _ => stage3SpawnInterval };
        }
        if (TutorialIntroController.TutorialNinthWeaponPhase)
            return 1.2f; // 9. silah: flame + fireball denemesi
        if (TutorialIntroController.TutorialEighthWeaponPhase)
            return 2f; // 8. silah: daha fazla düşman (toy denemesi)
        if (TutorialIntroController.TutorialSeventhWeaponPhase)
            return 3.5f; // 7. silah (kırbaç): 6. silah gibi (uzaklaştırma denemesi)
        if (TutorialIntroController.TutorialSixthWeaponPhase)
            return 3.5f; // 6. silah: az az (slow beam denemesi)
        if (TutorialIntroController.TutorialFifthWeaponPhase)
            return 1f; // 5. silah: 1 sn aralık (yıldırım 3 kere denemesi)
        if (GameBalanceManager.Instance != null)
            return GameBalanceManager.Instance.GetSpawnInterval(currentStage);
        return currentStage switch { 1 => stage1SpawnInterval, 2 => stage2SpawnInterval, _ => stage3SpawnInterval };
    }

    float GetEnemySpeed()
    {
        if (TutorialIntroController.TutorialCompleteFreehand)
        {
            if (GameBalanceManager.Instance != null)
                return GameBalanceManager.Instance.GetEnemySpeed(currentStage);
            return currentStage switch { 1 => stage1EnemySpeed, 2 => stage2EnemySpeed, _ => stage3EnemySpeed };
        }
        if (TutorialIntroController.TutorialEighthWeaponPhase)
            return 1.8f; // 8. silah: 19. diyalog sonrası biraz daha hızlı (sayı aynı)
        if (TutorialIntroController.TutorialFifthWeaponPhase || TutorialIntroController.TutorialSixthWeaponPhase || TutorialIntroController.TutorialSeventhWeaponPhase)
            return 1.2f; // 5./6./7. silah tutorial: yavaş düşmanlar
        if (GameBalanceManager.Instance != null)
            return GameBalanceManager.Instance.GetEnemySpeed(currentStage);
        return currentStage switch { 1 => stage1EnemySpeed, 2 => stage2EnemySpeed, _ => stage3EnemySpeed };
    }

    IEnumerator SpawnPortalsWithAnimation()
    {
        // Portal A
        if (portalPrefabA != null)
        {
            GameObject portalA = Instantiate(portalPrefabA, portalAPosition, portalARotation);
            StartCoroutine(AnimatePortalSpawn(portalA, portalARotation));
        }
        
        yield return new WaitForSeconds(portalSpawnDelay);
        
        // Portal B
        if (portalPrefabB != null)
        {
            GameObject portalB = Instantiate(portalPrefabB, portalBPosition, portalBRotation);
            StartCoroutine(AnimatePortalSpawn(portalB, portalBRotation));
        }
        
        yield return new WaitForSeconds(portalSpawnDelay);
        
        // Portal C
        if (portalPrefabC != null)
        {
            GameObject portalC = Instantiate(portalPrefabC, portalCPosition, portalCRotation);
            StartCoroutine(AnimatePortalSpawn(portalC, portalCRotation));
        }
    }

    IEnumerator AnimatePortalSpawn(GameObject portal, Quaternion targetRotation)
    {
        if (portal == null) yield break;

        Transform portalTransform = portal.transform;
        Vector3 targetScale = portalTransform.localScale;
        
        // Başlangıç: sıfır scale
        portalTransform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        float startRotationY = targetRotation.eulerAngles.y - portalSpawnRotation;
        
        while (elapsed < portalSpawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / portalSpawnDuration;
            
            // Elastic/Bounce easing fonksiyonu
            float easedT = EaseOutBack(t, portalOvershoot);
            
            // Scale animasyonu (sıfırdan hedefe)
            portalTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easedT);
            
            // Dönme animasyonu
            float currentRotationY = Mathf.Lerp(startRotationY, targetRotation.eulerAngles.y, EaseOutCubic(t));
            portalTransform.rotation = Quaternion.Euler(
                targetRotation.eulerAngles.x,
                currentRotationY,
                targetRotation.eulerAngles.z
            );
            
            yield return null;
        }
        
        // Son değerleri garanti et
        portalTransform.localScale = targetScale;
        portalTransform.rotation = targetRotation;
        
        // Pulse efekti (tatlı bir son dokunuş)
        yield return StartCoroutine(PulseEffect(portalTransform, targetScale));

        if (GameManager.Instance != null)
            GameManager.Instance.PlayPortalOpenSound();
    }

    IEnumerator PulseEffect(Transform portalTransform, Vector3 baseScale)
    {
        float pulseDuration = 0.2f;
        float pulseAmount = 1.05f;
        
        // Büyü
        float elapsed = 0f;
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pulseDuration / 2f);
            portalTransform.localScale = Vector3.Lerp(baseScale, baseScale * pulseAmount, t);
            yield return null;
        }
        
        // Küçül
        elapsed = 0f;
        while (elapsed < pulseDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (pulseDuration / 2f);
            portalTransform.localScale = Vector3.Lerp(baseScale * pulseAmount, baseScale, t);
            yield return null;
        }
        
        portalTransform.localScale = baseScale;
    }

    // Easing fonksiyonları
    float EaseOutBack(float t, float overshoot = 1.70158f)
    {
        float c1 = overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    IEnumerator SpawnEnemiesContinuously()
    {
        yield return new WaitForSeconds(delayBeforeFirstSpawn);

        bool firstSpawn = true;
        while (!isGameOver)
        {
            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            string portal = GetPreferredPortalForEnemy(enemyPrefab);
            Vector3 spawnPos = GetPortalPosition(portal);
            
            // Spawn yüksekliğini ayarla (portalın ortasına hizalama)
            spawnPos.y += spawnHeightOffset;

            // Düşmanı oyuncuya bakacak şekilde spawn et (portaldan dönerek çıkmasını engeller)
            Vector3 directionToPlayer = Camera.main != null ? Camera.main.transform.position - spawnPos : Vector3.forward;
            directionToPlayer.y = 0; // Y eksenini sabitle, sadece yatay düzlemde dönsün
            Quaternion spawnRotation = directionToPlayer != Vector3.zero 
                ? Quaternion.LookRotation(directionToPlayer) 
                : Quaternion.identity;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, spawnRotation);
            enemy.tag = "Enemy";

            // Enemy'ye gerekli componentleri ekle (eğer yoksa) - prefab referansıyla birlikte
            SetupEnemyComponents(enemy, spawnPos, enemyPrefab);

            if (!consecutivePortalCounts.ContainsKey(portal))
                consecutivePortalCounts[portal] = 0;

            consecutivePortalCounts[portal]++;
            foreach (var p in portals.Where(p => p != portal))
                consecutivePortalCounts[p] = 0;

            lastEnemyPerPortal[portal] = enemyPrefab;

            if (firstSpawn && GameManager.Instance != null)
            {
                GameManager.Instance.PlayStageSound(1);
                firstSpawn = false;
            }

            Debug.Log($"[SPAWN] Stage {currentStage} | {enemyPrefab.name} | Portal {portal} | Speed: {GetEnemySpeed()} | Kill: {totalKillCount}");

            // DİNAMİK spawn interval - stage'e göre değişir
            yield return new WaitForSeconds(GetSpawnInterval());
        }
    }

    void RefillSpawnBag()
    {
        spawnBag.Clear();
        int bagSize = 12;
        int w = 70, m = 25, s = 5, t = 0;
        if (GameBalanceManager.Instance != null)
        {
            bagSize = GameBalanceManager.Instance.BagSize;
            GameBalanceManager.Instance.GetEnemySpawnWeights(currentStage, out w, out m, out s, out t);
        }
        else
        {
            if (currentStage == 2) { w = 50; m = 30; s = 15; t = 5; }
            else if (currentStage == 3) { w = 10; m = 20; s = 45; t = 25; }
        }
        int total = w + m + s + t;
        if (total <= 0) total = 100;
        int weak = Mathf.Max(0, (w * bagSize) / total);
        int medium = Mathf.Max(0, (m * bagSize) / total);
        int strong = Mathf.Max(0, (s * bagSize) / total);
        int tank = Mathf.Max(0, (t * bagSize) / total);
        int filled = weak + medium + strong + tank;
        while (filled < bagSize && (weak + medium + strong + tank) > 0)
        {
            if (weak > 0) { weak++; filled++; } else if (medium > 0) { medium++; filled++; } else if (strong > 0) { strong++; filled++; } else if (tank > 0) { tank++; filled++; }
        }

        if (tur1Enemy != null) spawnBag.AddRange(Enumerable.Repeat(tur1Enemy, weak));
        if (tur2Enemy != null) spawnBag.AddRange(Enumerable.Repeat(tur2Enemy, medium));
        if (tur3Enemy != null) spawnBag.AddRange(Enumerable.Repeat(tur3Enemy, strong));
        if (tur4Enemy != null) spawnBag.AddRange(Enumerable.Repeat(tur4Enemy, tank));

        for (int i = spawnBag.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = spawnBag[i];
            spawnBag[i] = spawnBag[j];
            spawnBag[j] = tmp;
        }
    }

    GameObject GetRandomEnemyForStage(int stage)
    {
        int refillThreshold = 1;
        if (GameBalanceManager.Instance != null)
            refillThreshold = GameBalanceManager.Instance.BagRefillWhenSlotsBelow;
        if (spawnBag.Count < refillThreshold)
            RefillSpawnBag();

        if (spawnBag.Count > 0)
        {
            int idx = Random.Range(0, spawnBag.Count);
            GameObject chosen = spawnBag[idx];
            spawnBag.RemoveAt(idx);
            return chosen;
        }

        List<GameObject> pool = new List<GameObject>();
        if (GameBalanceManager.Instance != null)
        {
            GameBalanceManager.Instance.GetEnemySpawnWeights(stage, out int w, out int m, out int s, out int t);
            if (tur1Enemy != null) pool.AddRange(Enumerable.Repeat(tur1Enemy, w));
            if (tur2Enemy != null) pool.AddRange(Enumerable.Repeat(tur2Enemy, m));
            if (tur3Enemy != null) pool.AddRange(Enumerable.Repeat(tur3Enemy, s));
            if (tur4Enemy != null) pool.AddRange(Enumerable.Repeat(tur4Enemy, t));
        }
        else
        {
            if (stage == 1) { pool.AddRange(Enumerable.Repeat(tur1Enemy, 8)); pool.AddRange(Enumerable.Repeat(tur2Enemy, 2)); }
            else if (stage == 2) { pool.AddRange(Enumerable.Repeat(tur1Enemy, 5)); pool.AddRange(Enumerable.Repeat(tur2Enemy, 4)); pool.AddRange(Enumerable.Repeat(tur3Enemy, 1)); if (tur4Enemy != null) pool.Add(tur4Enemy); }
            else { pool.AddRange(Enumerable.Repeat(tur1Enemy, 3)); pool.AddRange(Enumerable.Repeat(tur2Enemy, 5)); pool.AddRange(Enumerable.Repeat(tur3Enemy, 2)); if (tur4Enemy != null) { pool.Add(tur4Enemy); pool.Add(tur4Enemy); } }
        }
        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : (tur1Enemy != null ? tur1Enemy : tur2Enemy);
    }

    private void SetupEnemyComponents(GameObject enemy, Vector3 spawnPos, GameObject prefab)
    {
        Debug.Log($"[SETUP] Component setup başlıyor: {enemy.name}");

        // 1. NavMeshAgent ekle/kontrol et
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = 0.04f;
            agent.height = 0f;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
            agent.stoppingDistance = 2f;
            agent.autoBraking = true;
            agent.autoRepath = true;
        }
        else
        {
            agent.radius = 0.04f;
            agent.height = 0f;
        }
        
        // Stage bazlı hız ataması
        agent.speed = GetEnemySpeed();

        // NavMeshAgent'ı doğru pozisyona warp et
        agent.enabled = true;
        agent.Warp(spawnPos);

        // 2. EnemyBehavior script'i ekle/kontrol et
        EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
        if (enemyBehavior == null)
        {
            enemyBehavior = enemy.AddComponent<EnemyBehavior>();
            enemyBehavior.agent = agent;
        }
        else
        {
            enemyBehavior.agent = agent;
        }
        // Stage bazlı hız ataması
        enemyBehavior.speed = GetEnemySpeed();

        // 3. EnemyHealth script'i ekle/kontrol et - TİP BAZLI HP VE SKOR
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = enemy.AddComponent<EnemyHealth>();
            enemyHealth.headshotMultiplier = 2f;
            enemyHealth.bodyMultiplier = 1f;
            enemyHealth.legsMultiplier = 0.7f;
        }
        
        // Düşman tipine göre HP ve skor: GameBalanceManager'dan (stage + type) veya fallback
        int enemyType = GetEnemyTypeIndex(prefab);
        if (GameBalanceManager.Instance != null)
        {
            enemyHealth.totalHealth = GameBalanceManager.Instance.GetEnemyHP(currentStage, enemyType);
            enemyHealth.scoreValue = GameBalanceManager.Instance.GetEnemyScore(currentStage, enemyType);
        }
        else
        {
            if (prefab == tur1Enemy) { enemyHealth.totalHealth = tur1HP; enemyHealth.scoreValue = tur1Score; }
            else if (prefab == tur2Enemy) { enemyHealth.totalHealth = tur2HP; enemyHealth.scoreValue = tur2Score; }
            else if (prefab == tur3Enemy) { enemyHealth.totalHealth = tur3HP; enemyHealth.scoreValue = tur3Score; }
            else if (prefab == tur4Enemy) { enemyHealth.totalHealth = tur4HP; enemyHealth.scoreValue = tur4Score; }
            else { enemyHealth.totalHealth = tur3HP; enemyHealth.scoreValue = tur3Score; }
        }
        
        // Floating text prefab'ını ata
        if (floatingTextPrefab != null && enemyHealth.floatingTextPrefab == null)
            enemyHealth.floatingTextPrefab = floatingTextPrefab;

        // 4. Rigidbody ekle/kontrol et (kinematic olarak)
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = enemy.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 5. Collider kontrolü - Sadece trigger ayarı yap
        Collider col = enemy.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 6. Animasyon başlat ve loop yap
        Animator animator = enemy.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.enabled = true;
            animator.Play("mixamo_com", 0, 0f);
        }

        Debug.Log($"[SETUP] Enemy hazır: {enemy.name} | HP: {enemyHealth.totalHealth} | Speed: {enemyBehavior.speed} | Score: {enemyHealth.scoreValue}");
    }

    int GetEnemyTypeIndex(GameObject prefab)
    {
        if (prefab == tur1Enemy) return 0;
        if (prefab == tur2Enemy) return 1;
        if (prefab == tur3Enemy) return 2;
        if (prefab == tur4Enemy) return 3;
        return 2;
    }

    string GetPreferredPortalForEnemy(GameObject enemy)
    {
        List<string> validPortals = new();

        foreach (string portal in portals)
        {
            bool tooManyConsecutive = consecutivePortalCounts.TryGetValue(portal, out int count) && count >= 2;
            bool sameEnemyLast = lastEnemyPerPortal.TryGetValue(portal, out GameObject lastEnemy) && lastEnemy == enemy;

            if (!tooManyConsecutive && !sameEnemyLast)
                validPortals.Add(portal);
        }

        int sameTypeCount = lastEnemyPerPortal.Values.Count(e => e == enemy);
        if (sameTypeCount >= 3)
        {
            validPortals = validPortals.Where(p => lastEnemyPerPortal.TryGetValue(p, out var e) && e != enemy).ToList();
        }

        if (validPortals.Count > 0)
        {
            Vector3 weights = GameBalanceManager.Instance != null ? GameBalanceManager.Instance.GetPortalWeights() : new Vector3(33f, 33f, 33f);
            float totalWeight = 0f;
            foreach (string p in validPortals)
            {
                if (p == "A") totalWeight += weights.x;
                else if (p == "B") totalWeight += weights.y;
                else if (p == "C") totalWeight += weights.z;
            }
            if (totalWeight <= 0f) return validPortals[Random.Range(0, validPortals.Count)];
            float r = Random.Range(0f, totalWeight);
            foreach (string p in validPortals)
            {
                float w = p == "A" ? weights.x : (p == "B" ? weights.y : weights.z);
                if (r < w) return p;
                r -= w;
            }
            return validPortals[validPortals.Count - 1];
        }

        Debug.LogWarning($"[PORTAL OVERRIDE] {enemy.name} i�in kural d��� portal se�imi yap�l�yor.");
        return portals[Random.Range(0, portals.Count)];
    }

    public Vector3 GetPortalPosition(string portal)
    {
        switch (portal)
        {
            case "A":
                return portalAPosition;
            case "B":
                return portalBPosition;
            case "C":
                return portalCPosition;
            default:
                return portalAPosition;
        }
    }

    public void StopSpawning()
    {
        isGameOver = true;
        StopAllCoroutines();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(false);
        }
    }
}
