using System.Collections;
using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    public GameObject portalPrefabA;
    public GameObject portalPrefabB;
    public GameObject portalPrefabC;

    [Header("Enemy Prefabs for Portals")]
    public GameObject jamEnemy;
    public GameObject flyingEnemy;
    public GameObject tallEnemy;
    public GameObject middleEnemy;

    [Header("Enemy Spawn Settings")]
    public float enemySpawnInterval = 2f;
    public float delayBeforeFirstWave = 10f;
    public float postWaveSpawnDelay = 15f;

    [Header("Portal Spawn Points")]
    public Vector3[] spawnPointsA;
    public Vector3[] spawnPointsB;
    public Vector3[] spawnPointsC;

    private int currentWave = 1;
    private bool isSpawning = false;
    private bool isGameOver = false;

    void Start()
    {
        StartCoroutine(SpawnPortals());
    }

    IEnumerator SpawnPortals()
    {
        SpawnPortalWithRotation(portalPrefabA, new Vector3(5.2f, -4.3f, -87.1f), Quaternion.Euler(0, 90, 0));
        SpawnPortalWithRotation(portalPrefabB, new Vector3(5.2f, -4.5f, 105f), Quaternion.Euler(0, 90, 0));
        SpawnPortal(portalPrefabC, new Vector3(-116.2f, -4.6f, 1.8f));

        yield return new WaitForSeconds(delayBeforeFirstWave);

        while (!isGameOver)
        {
            isSpawning = true;
            float enemySpeed = GetEnemySpeedForWave(currentWave);
            EnemyWaveData waveData = GetWaveData(currentWave);
            yield return StartCoroutine(SpawnWave(waveData, enemySpeed));
            isSpawning = false;
            yield return new WaitForSeconds(postWaveSpawnDelay);
            currentWave++;
        }
    }

    void SpawnPortal(GameObject portalPrefab, Vector3 position)
    {
        Instantiate(portalPrefab, position, Quaternion.identity);
    }

    void SpawnPortalWithRotation(GameObject portalPrefab, Vector3 position, Quaternion rotation)
    {
        Instantiate(portalPrefab, position, rotation);
    }

    IEnumerator SpawnWave(EnemyWaveData waveData, float enemySpeed)
    {
        if (!isSpawning || isGameOver) yield break;

        for (int i = 0; i < waveData.spawnSequence.Length; i++)
        {
            yield return SpawnEnemyAtPortal(spawnPointsA, waveData.spawnSequence[i][0], enemySpeed);
            yield return SpawnEnemyAtPortal(spawnPointsB, waveData.spawnSequence[i][1], enemySpeed);
            yield return SpawnEnemyAtPortal(spawnPointsC, waveData.spawnSequence[i][2], enemySpeed);
        }
    }

    IEnumerator SpawnEnemyAtPortal(Vector3[] spawnPoints, GameObject enemyType, float enemySpeed)
    {
        if (!isSpawning || enemyType == null) yield break;

        Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyType, spawnPos, Quaternion.identity);

        EnemyBehavior enemyBehavior = enemy.GetComponent<EnemyBehavior>();
        if (enemyBehavior != null)
        {
            enemyBehavior.speed = enemySpeed;
        }

        yield return new WaitForSeconds(enemySpawnInterval);
    }

    float GetEnemySpeedForWave(int waveNumber)
    {
        return 7f + (waveNumber - 1) * 2f;
    }

    public void StopSpawning()
    {
        isGameOver = true; 
        isSpawning = false; 
        StopAllCoroutines(); 
    }
    EnemyWaveData GetWaveData(int wave)
    {
        switch (wave)
        {
            case 1:
                return new EnemyWaveData(new GameObject[][]
                {
                    new GameObject[] { jamEnemy, jamEnemy, middleEnemy }, 
                    new GameObject[] { jamEnemy, jamEnemy, jamEnemy }, 
                    new GameObject[] { jamEnemy, jamEnemy, middleEnemy }
                    
                });
            case 2:
                return new EnemyWaveData(new GameObject[][]
                {
                    new GameObject[] { jamEnemy, jamEnemy, jamEnemy },
                    new GameObject[] { flyingEnemy, flyingEnemy, middleEnemy },
                    new GameObject[] { flyingEnemy, flyingEnemy, flyingEnemy },
                    new GameObject[] { flyingEnemy, flyingEnemy, middleEnemy }
                });
            case 3:
                return new EnemyWaveData(new GameObject[][]
                {
                    new GameObject[] {flyingEnemy, flyingEnemy, middleEnemy },
                    new GameObject[] { middleEnemy, middleEnemy, middleEnemy },
                    new GameObject[] { jamEnemy, jamEnemy, jamEnemy },
                    new GameObject[] { middleEnemy, middleEnemy, middleEnemy },
                    new GameObject[] { middleEnemy, middleEnemy, middleEnemy }

                });
            case 4:
                return new EnemyWaveData(new GameObject[][]
                {
                    new GameObject[] {middleEnemy, middleEnemy, middleEnemy},
                    new GameObject[] {middleEnemy, middleEnemy, middleEnemy},
                    new GameObject[] { tallEnemy, tallEnemy, flyingEnemy },
                    new GameObject[] { tallEnemy, jamEnemy,middleEnemy },
                    new GameObject[] { tallEnemy, tallEnemy, middleEnemy }
                });
            default:
                return new EnemyWaveData(new GameObject[][]
                {
                    new GameObject[] { jamEnemy, flyingEnemy, middleEnemy },
                    new GameObject[] { middleEnemy, jamEnemy, middleEnemy },
                    new GameObject[] { tallEnemy, middleEnemy, jamEnemy },
                    new GameObject[] { middleEnemy, jamEnemy, flyingEnemy },
                    new GameObject[] { middleEnemy, jamEnemy,middleEnemy},
                      new GameObject[] { tallEnemy, middleEnemy, middleEnemy }
                });
        }
    }
}

public class EnemyWaveData
{
    public GameObject[][] spawnSequence;

    public EnemyWaveData(GameObject[][] sequence)
    {
        spawnSequence = sequence;
    }
}
