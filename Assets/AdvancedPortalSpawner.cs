
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

    [Header("Damage Feedback Prefabs")]
    [Tooltip("Tek collider sistemli yeni düşmanlar için genel floating text")]
    public GameObject floatingTextPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Portallar açıldıktan sonra ilk düşman spawn'ı için bekleme süresi")]
    public float delayBeforeFirstSpawn = 2f;
    [Tooltip("Düşmanların portalın ne kadar yukarısından spawn olacağı (portalın ortasına hizalamak için)")]
    public float spawnHeightOffset = 1.5f;

    [Header("Stage Kill Thresholds (Silah değişimiyle senkron)")]
    [Tooltip("Stage 2 başlangıcı - Silah 4 ile senkron")]
    public int stage2KillThreshold = 24;
    [Tooltip("Stage 3 başlangıcı - Silah 7 ile senkron")]
    public int stage3KillThreshold = 54;

    [Header("Spawn Intervals per Stage")]
    public float stage1SpawnInterval = 2.5f;
    public float stage2SpawnInterval = 2.0f;
    public float stage3SpawnInterval = 1.5f;

    [Header("Enemy Speed per Stage")]
    public float stage1EnemySpeed = 3.0f;
    public float stage2EnemySpeed = 3.5f;
    public float stage3EnemySpeed = 4.0f;

    [Header("Enemy HP (Constant per Type)")]
    public float tur1HP = 40f;
    public float tur2HP = 80f;
    public float tur3HP = 150f;

    [Header("Enemy Score Values")]
    public int tur1Score = 25;
    public int tur2Score = 50;
    public int tur3Score = 100;

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

        // Portalları ve düşmanları gecikmeli başlat
        StartCoroutine(StartGameSequence());
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

    void UpdateStage()
    {
        if (currentStage == 1 && totalKillCount >= stage2KillThreshold)
        {
            currentStage = 2;
            Debug.Log($"[STAGE] Stage 2'ye geçildi! Kill: {totalKillCount}, Spawn Interval: {GetSpawnInterval()}s, Enemy Speed: {GetEnemySpeed()}");
        }
        else if (currentStage == 2 && totalKillCount >= stage3KillThreshold)
        {
            currentStage = 3;
            Debug.Log($"[STAGE] Stage 3'e geçildi! Kill: {totalKillCount}, Spawn Interval: {GetSpawnInterval()}s, Enemy Speed: {GetEnemySpeed()}");
        }
    }

    float GetSpawnInterval() => currentStage switch
    {
        1 => stage1SpawnInterval,
        2 => stage2SpawnInterval,
        _ => stage3SpawnInterval
    };

    float GetEnemySpeed() => currentStage switch
    {
        1 => stage1EnemySpeed,
        2 => stage2EnemySpeed,
        _ => stage3EnemySpeed
    };

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

        while (!isGameOver)
        {
            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            string portal = GetPreferredPortalForEnemy(enemyPrefab);
            Vector3 spawnPos = GetPortalPosition(portal);
            
            // Spawn yüksekliğini ayarla (portalın ortasına hizalama)
            spawnPos.y += spawnHeightOffset;

            // Düşmanı oyuncuya bakacak şekilde spawn et (portaldan dönerek çıkmasını engeller)
            Vector3 directionToPlayer = Camera.main.transform.position - spawnPos;
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

            Debug.Log($"[SPAWN] Stage {currentStage} | {enemyPrefab.name} | Portal {portal} | Speed: {GetEnemySpeed()} | Kill: {totalKillCount}");

            // DİNAMİK spawn interval - stage'e göre değişir
            yield return new WaitForSeconds(GetSpawnInterval());
        }
    }

    GameObject GetRandomEnemyForStage(int stage)
    {
        List<GameObject> pool = new List<GameObject>();

        if (stage == 1)
        {
            // Stage 1: %80 tur1, %20 tur2, %0 tur3 (Öğrenme fazı)
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 8));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 2));
            // tur3 yok - tank düşmanlar henüz çıkmaz
        }
        else if (stage == 2)
        {
            // Stage 2: %50 tur1, %40 tur2, %10 tur3 (Baskı fazı)
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 5));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 4));
            pool.AddRange(Enumerable.Repeat(tur3Enemy, 1));
        }
        else
        {
            // Stage 3: %30 tur1, %50 tur2, %20 tur3 (Hayatta kalma)
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 3));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 5));
            pool.AddRange(Enumerable.Repeat(tur3Enemy, 2));
        }

        return pool[Random.Range(0, pool.Count)];
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
        
        // Düşman tipine göre HP ve skor ataması (SABİT değerler)
        if (prefab == tur1Enemy)
        {
            enemyHealth.totalHealth = tur1HP;
            enemyHealth.scoreValue = tur1Score;
        }
        else if (prefab == tur2Enemy)
        {
            enemyHealth.totalHealth = tur2HP;
            enemyHealth.scoreValue = tur2Score;
        }
        else // tur3Enemy
        {
            enemyHealth.totalHealth = tur3HP;
            enemyHealth.scoreValue = tur3Score;
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
            return validPortals[Random.Range(0, validPortals.Count)];
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
