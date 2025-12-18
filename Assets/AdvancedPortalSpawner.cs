
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdvancedPortalSpawner : MonoBehaviour
{
    [Header("Portal Prefabs")]
    public GameObject portalPrefabA, portalPrefabB, portalPrefabC;

    [Header("Portal Positions")]
    public Vector3 portalAPosition = new Vector3(5.2f, -4.3f, -87.1f);
    public Vector3 portalBPosition = new Vector3(5.2f, -4.5f, 105f);
    public Vector3 portalCPosition = new Vector3(-116.2f, -4.6f, 1.8f);

    [Header("Portal Rotations")]
    public Vector3 portalARotation = new Vector3(0, 90, 0);
    public Vector3 portalBRotation = new Vector3(0, 90, 0);
    public Vector3 portalCRotation = Vector3.zero;

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

    [Header("Spawn Points")]
    public Vector3[] spawnPointsA, spawnPointsB, spawnPointsC;

    private List<string> portals = new List<string> { "A", "B", "C" };
    private Dictionary<string, Vector3[]> portalSpawnPoints;
    private Dictionary<string, GameObject> portalPrefabs;
    private Dictionary<string, int> consecutivePortalCounts = new();
    private Dictionary<string, GameObject> lastEnemyPerPortal = new();

    private bool isGameOver = false;
    private int currentStage = 1;

    void Start()
    {
        portalSpawnPoints = new Dictionary<string, Vector3[]>
        {
            { "A", spawnPointsA },
            { "B", spawnPointsB },
            { "C", spawnPointsC }
        };

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
            Debug.LogError("[STAGE] Stage 2'ye ge�ildi!");
            currentStage = 2;
        }
        else if (currentStage == 2 && score >= stage3ScoreThreshold)
        {
            Debug.LogError("[STAGE] Stage 3'e ge�ildi!");
            currentStage = 3;
        }
    }

    void SpawnPortals()
    {
        if (portalPrefabA != null)
        {
            Instantiate(portalPrefabA, portalAPosition, Quaternion.Euler(portalARotation));
        }
        
        if (portalPrefabB != null)
        {
            Instantiate(portalPrefabB, portalBPosition, Quaternion.Euler(portalBRotation));
        }
        
        if (portalPrefabC != null)
        {
            Instantiate(portalPrefabC, portalCPosition, Quaternion.Euler(portalCRotation));
        }
    }

    IEnumerator SpawnEnemiesContinuously()
    {
        yield return new WaitForSeconds(delayBeforeFirstSpawn);

        while (!isGameOver)
        {
            GameObject enemyPrefab = GetRandomEnemyForStage(currentStage);
            string portal = GetPreferredPortalForEnemy(enemyPrefab);
            Vector3 spawnPos = GetRandomSpawnPoint(portal);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.tag = "Enemy";

            if (!consecutivePortalCounts.ContainsKey(portal))
                consecutivePortalCounts[portal] = 0;

            consecutivePortalCounts[portal]++;
            foreach (var p in portals.Where(p => p != portal))
                consecutivePortalCounts[p] = 0;

            lastEnemyPerPortal[portal] = enemyPrefab;

            Debug.LogError($"[SPAWN] {enemyPrefab.name} t�r� {portal} portal�ndan spawn oldu. Pozisyon: {spawnPos}");

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

    Vector3 GetRandomSpawnPoint(string portal)
    {
        Vector3[] points = portalSpawnPoints[portal];
        return points[Random.Range(0, points.Length)];
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
