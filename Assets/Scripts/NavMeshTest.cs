using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== NAVMESH SİSTEMİ KAPSAMLI TEST BAŞLATILIYOR ===");

        // 1. NavMesh temel kontrolü
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Debug.Log($"📐 NavMesh Durumu: {triangulation.vertices.Length} vertex, {triangulation.indices.Length / 3} üçgen");

        if (triangulation.vertices.Length == 0)
        {
            Debug.LogError("❌ KRİTİK: NavMesh bake edilmemiş! Unity'de Navigation penceresinden Bake yapın!");
            Debug.LogError("📋 TALİMAT: Window > AI > Navigation > Bake tab > Bake butonu");
            return;
        }

        // 2. SamplePosition testi
        Vector3[] testPositions = {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(-10, 0, 0),
            new Vector3(0, 0, 10),
            new Vector3(0, 0, -10)
        };

        bool sampleSuccess = false;
        foreach (Vector3 testPos in testPositions)
        {
            if (NavMesh.SamplePosition(testPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Debug.Log($"✅ NavMesh sample başarılı: {testPos} -> {hit.position}");
                sampleSuccess = true;
                break;
            }
        }

        if (!sampleSuccess)
        {
            Debug.LogError("❌ KRİTİK: NavMesh sample başarısız! Bake işlemi eksik veya hatalı!");
        }

        // 3. Enemy objelerini bul ve analiz et
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"👹 Tag'li Enemy sayısı: {enemies.Length}");

        // 4. NavMeshAgent testi
        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();
        Debug.Log($"🤖 Aktif NavMeshAgent sayısı: {agents.Length}");

        foreach (GameObject enemy in enemies)
        {
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            EnemyBehavior behavior = enemy.GetComponent<EnemyBehavior>();
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            Debug.Log($"📊 Enemy: {enemy.name}");
            Debug.Log($"   - NavMeshAgent: {(agent != null ? "✅ Var" : "❌ Yok")}");
            Debug.Log($"   - EnemyBehavior: {(behavior != null ? "✅ Var" : "❌ Yok")}");
            Debug.Log($"   - EnemyHealth: {(health != null ? "✅ Var" : "❌ Yok")}");

            if (agent != null)
            {
                Debug.Log($"   - Agent Enabled: {agent.enabled}, Speed: {agent.speed}");
                if (!agent.enabled)
                {
                    Debug.LogError($"❌ {enemy.name} agent'ı devre dışı!");
                }
            }

            // Collider kontrolü
            Collider enemyCollider = enemy.GetComponent<Collider>();
            Debug.Log($"   - Collider: {(enemyCollider != null ? "✅ Var" : "❌ Yok")}");
            if (enemyCollider != null)
            {
                Debug.Log($"   - IsTrigger: {(enemyCollider.isTrigger ? "✅ Trigger" : "❌ Solid")}");
            }
        }

        // 4. Enemy component testi
        EnemyBehavior[] enemyBehaviors = FindObjectsOfType<EnemyBehavior>();
        EnemyHealth[] enemyHealths = FindObjectsOfType<EnemyHealth>();

        Debug.Log($"👹 EnemyBehavior sayısı: {enemyBehaviors.Length}");
        Debug.Log($"❤️ EnemyHealth sayısı: {enemyHealths.Length}");

        if (enemyBehaviors.Length == 0)
        {
            Debug.LogWarning("⚠️ Uyarı: Sahnedeki enemy'lerde EnemyBehavior yok!");
            Debug.LogWarning("💡 Enemy prefab'larına EnemyBehavior script'i eklendiğini kontrol edin!");
        }

        foreach (EnemyBehavior eb in enemyBehaviors)
        {
            Debug.Log($"  - EnemyBehavior: {eb.gameObject.name}, Agent: {(eb.agent != null ? "Var" : "Yok")}, Speed: {eb.speed}");
            if (eb.agent == null)
            {
                Debug.LogError($"❌ {eb.gameObject.name}'de EnemyBehavior.agent referansı yok!");
            }
        }

        // 5. PortalSpawner testi
        AdvancedPortalSpawner spawner = FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner != null)
        {
            Debug.Log($"🏠 PortalSpawner bulundu: {spawner.gameObject.name}");
            int portalLocationCount = 0;
            if (spawner.portalLocationA != null) portalLocationCount++;
            if (spawner.portalLocationB != null) portalLocationCount++;
            if (spawner.portalLocationC != null) portalLocationCount++;
            Debug.Log($"📍 Portal Locations: {portalLocationCount}/3 ayarlanmış");
        }
        else
        {
            Debug.LogError("❌ PortalSpawner bulunamadı!");
        }

        // 6. Player objesi kontrolü
        OVRCameraRig ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            Debug.Log($"🎮 Player bulundu: OVRCameraRig - Tag: {ovrRig.tag}");
            Collider playerCollider = ovrRig.GetComponent<Collider>();
            if (playerCollider != null)
            {
                Debug.Log($"   - Player Collider: ✅ Var, IsTrigger: {(playerCollider.isTrigger ? "✅ Trigger" : "❌ Solid")}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Player'da collider yok! Trigger sistemi çalışmayacak.");
                Debug.LogWarning($"💡 Player objesine CapsuleCollider ekleyin ve IsTrigger = false yapın.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ OVRCameraRig bulunamadı! Player kontrol edilemiyor.");
        }

        // 7. Özet
        if (triangulation.vertices.Length > 0 && sampleSuccess && agents.Length > 0 && enemyBehaviors.Length > 0)
        {
            Debug.Log("🎉 NavMesh sistemi ÇALIŞIYOR! Enemy'ler spawn olup size doğru gelecek.");
        }
        else
        {
            Debug.LogError("❌ NavMesh sistemi ÇALIŞMIYOR! Yukarıdaki hataları düzeltin.");
        }

        Debug.Log("=== NAVMESH SİSTEMİ KAPSAMLI TEST TAMAMLANDI ===");
    }

    void Update()
    {
        // F1 ile test tekrar başlat
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Start();
        }

        // F2 ile enemy spawn testi
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TestEnemySpawn();
        }
    }

    void TestEnemySpawn()
    {
        Debug.Log("=== ENEMY SPAWN TESTİ ===");

        // Rastgele bir spawn noktası seç
        AdvancedPortalSpawner spawner = FindObjectOfType<AdvancedPortalSpawner>();
        if (spawner != null)
        {
            // Portal A'dan spawn testi
            Vector3 spawnPos = spawner.GetPortalPosition("A");
            Debug.Log($"Portal A spawn noktası: {spawnPos}");

            // NavMesh üzerinde mi kontrol et
            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 1f, NavMesh.AllAreas))
            {
                Debug.Log($"✅ Spawn noktası NavMesh üzerinde: {hit.position}");
            }
            else
            {
                Debug.LogError($"❌ Spawn noktası NavMesh dışında: {spawnPos}");
            }
        }
        else
        {
            Debug.LogError("❌ AdvancedPortalSpawner bulunamadı!");
        }
    }
}
