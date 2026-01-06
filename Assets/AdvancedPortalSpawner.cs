
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

    [Header("Spawn Settings")]
    public float enemySpawnInterval = 2f;
    public float delayBeforeFirstSpawn = 5f;

    [Header("Stage Score Thresholds")]
    public int stage2ScoreThreshold = 200;
    public int stage3ScoreThreshold = 500;

    private List<string> portals = new List<string> { "A", "B", "C" };
    private Dictionary<string, GameObject> portalPrefabs;
    private Dictionary<string, int> consecutivePortalCounts = new();
    private Dictionary<string, GameObject> lastEnemyPerPortal = new();

    private bool isGameOver = false;
    private int currentStage = 1;

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

        SpawnPortals();
        StartCoroutine(SpawnEnemiesContinuously());
    }

    void Update()
    {
        if (isGameOver) return;

        int score = GameManager.Instance.score;

        if (currentStage == 1 && score >= stage2ScoreThreshold)
        {
            // Debug.LogError("[STAGE] Stage 2'ye ge�ildi!");
            currentStage = 2;
        }
        else if (currentStage == 2 && score >= stage3ScoreThreshold)
        {
            // Debug.LogError("[STAGE] Stage 3'e ge�ildi!");
            currentStage = 3;
        }
    }

    void SpawnPortals()
    {
        if (portalPrefabA != null)
        {
            Instantiate(portalPrefabA, portalAPosition, portalARotation);
        }
        
        if (portalPrefabB != null)
        {
            Instantiate(portalPrefabB, portalBPosition, portalBRotation);
        }
        
        if (portalPrefabC != null)
        {
            Instantiate(portalPrefabC, portalCPosition, portalCRotation);
        }
    }

    IEnumerator SpawnEnemiesContinuously()
    {
        yield return new WaitForSeconds(delayBeforeFirstSpawn);

        while (!isGameOver)
        {
            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            string portal = GetPreferredPortalForEnemy(enemyPrefab);
            Vector3 spawnPos = GetPortalPosition(portal);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";

            // Enemy'ye gerekli componentleri ekle (eğer yoksa)
            SetupEnemyComponents(enemy, spawnPos);

            if (!consecutivePortalCounts.ContainsKey(portal))
                consecutivePortalCounts[portal] = 0;

            consecutivePortalCounts[portal]++;
            foreach (var p in portals.Where(p => p != portal))
                consecutivePortalCounts[p] = 0;

            lastEnemyPerPortal[portal] = enemyPrefab;

            // Debug.LogError($"[SPAWN] {enemyPrefab.name} t�r� {portal} portal�ndan spawn oldu. Pozisyon: {spawnPos}");

            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }

    GameObject GetRandomEnemyForStage(int stage)
    {
        List<GameObject> pool = new List<GameObject>();

        if (stage == 1)
        {
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 5));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 5));
        }
        else if (stage == 2)
        {
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 4));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 4));
            pool.AddRange(Enumerable.Repeat(tur3Enemy, 2));
        }
        else
        {
            pool.AddRange(Enumerable.Repeat(tur1Enemy, 3));
            pool.AddRange(Enumerable.Repeat(tur2Enemy, 3));
            pool.AddRange(Enumerable.Repeat(tur3Enemy, 4));
        }

        return pool[Random.Range(0, pool.Count)];
    }

    private void SetupEnemyComponents(GameObject enemy, Vector3 spawnPos)
    {
        // Debug.Log($"[SETUP] 🔧 Component setup başlıyor: {enemy.name}");

        // 1. NavMeshAgent ekle/kontrol et
        UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = 0.04f;
            agent.height = 0f;
            agent.speed = 3.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
            agent.stoppingDistance = 2f;
            agent.autoBraking = true;
            agent.autoRepath = true;
            // Debug.Log($"[SETUP] ✅ NavMeshAgent eklendi: {enemy.name} (Radius: {agent.radius}, Height: {agent.height}, Speed: {agent.speed})");
        }
        else
        {
            // Var olan agent'in ayarlarını da güncelle
            agent.radius = 0.04f;
            agent.height = 0f;
            // Debug.Log($"[SETUP] ℹ️ NavMeshAgent ayarları güncellendi: {enemy.name} (Radius: {agent.radius}, Height: {agent.height})");
        }

        // NavMeshAgent'ı doğru pozisyona warp et
        agent.enabled = true;
        agent.Warp(spawnPos);

        // 2. EnemyBehavior script'i ekle/kontrol et
        EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
        if (enemyBehavior == null)
        {
            enemyBehavior = enemy.AddComponent<EnemyBehavior>();
            enemyBehavior.speed = 3.5f;
            enemyBehavior.agent = agent; // Agent referansını set et
            // Debug.Log($"[SETUP] ✅ EnemyBehavior eklendi: {enemy.name}");
        }
        else
        {
            enemyBehavior.agent = agent; // Var olan script'e agent referansı ver
            // Debug.Log($"[SETUP] ℹ️ EnemyBehavior zaten var: {enemy.name}");
        }

        // 3. EnemyHealth script'i ekle/kontrol et
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = enemy.AddComponent<EnemyHealth>();
            enemyHealth.totalHealth = 100f;
            enemyHealth.headshotMultiplier = 2f;
            enemyHealth.bodyMultiplier = 1f;
            enemyHealth.legsMultiplier = 0.7f;
            enemyHealth.scoreValue = 50;
            // Debug.Log($"[SETUP] ✅ EnemyHealth eklendi: {enemy.name}");
        }
        else
        {
            // Debug.Log($"[SETUP] ℹ️ EnemyHealth zaten var: {enemy.name}");
        }

        // 4. Rigidbody ekle/kontrol et (kinematic olarak)
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = enemy.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            // Debug.Log($"[SETUP] ✅ Rigidbody eklendi: {enemy.name}");
        }

        // 5. Collider kontrolü - Sadece trigger ayarı yap (kullanıcı manuel ekleyecek)
        Collider col = enemy.GetComponent<Collider>();
        if (col != null)
        {
            // Var olan collider'ı trigger yap
            col.isTrigger = true;
            // Debug.Log($"[SETUP] ℹ️ Collider trigger yapıldı: {enemy.name}");
        }
        else
        {
            // Debug.LogWarning($"[SETUP] ⚠️ {enemy.name}'de collider bulunamadı! Prefab'a collider ekleyin!");
        }

        // 6. Animasyon başlat ve loop yap
        Animator animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            if (animator.runtimeAnimatorController != null)
            {
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.enabled = true;

                // State adını kullanarak animasyonu başlat
                string stateName = "mixamo_com";
                animator.Play(stateName, 0, 0f);
                
                // Debug.Log($"[SETUP] ✅ Animasyon başlatıldı: {enemy.name} - State: {stateName}, Controller: {animator.runtimeAnimatorController.name}");
            }
            else
            {
                // Debug.LogWarning($"[SETUP] ⚠️ AnimatorController atanmamış: {enemy.name}");
            }
        }
        else
        {
            // Debug.LogWarning($"[SETUP] ⚠️ Animator component bulunamadı: {enemy.name}");
        }

        // Debug.Log($"[SETUP] 🎉 Enemy tamamen hazır: {enemy.name} at {spawnPos}");
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

        // Debug.LogWarning($"[PORTAL OVERRIDE] {enemy.name} i�in kural d��� portal se�imi yap�l�yor.");
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
