using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtlantisTestRunner : MonoBehaviour
{
    [Header("Test Ayarları")]
    public bool runTestsOnStart = false;
    public float testDelay = 2f;

    void Start()
    {
        if (runTestsOnStart)
        {
            Invoke("RunAllTests", testDelay);
        }
    }

    void Update()
    {
        // F12 ile test başlat
        if (Input.GetKeyDown(KeyCode.F12))
        {
            RunAllTests();
        }

        // F11 ile sistem durumunu yazdır
        if (Input.GetKeyDown(KeyCode.F11))
        {
            PrintSystemStatus();
        }
    }

    public void RunAllTests()
    {
        Debug.Log("🎯 ARTLANTIS SAHNESİ TESTLERİ BAŞLATILIYOR...");
        Debug.Log("==========================================");

        TestVRSytem();
        TestGameManager();
        TestPortalSpawner();
        TestWeaponManager();
        TestUI();
        TestPrefabs();

        Debug.Log("==========================================");
        Debug.Log("🎯 TESTLER TAMAMLANDI!");
    }

    private void TestVRSytem()
    {
        Debug.Log("🔍 VR Sistemi Testi:");

        OVRCameraRig ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            Debug.Log("✅ OVRCameraRig bulundu");
        }
        else
        {
            Debug.LogError("❌ OVRCameraRig bulunamadı!");
        }
    }

    private void TestGameManager()
    {
        Debug.Log("🔍 GameManager Testi:");

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            Debug.Log("✅ GameManager bulundu");

            if (gm.restartCanvas != null)
                Debug.Log("✅ Restart Canvas bağlı");
            else
                Debug.LogWarning("⚠️ Restart Canvas bağlı değil");

            if (gm.timeAndScorePanel != null)
                Debug.Log("✅ Time & Score Panel bağlı");
            else
                Debug.LogWarning("⚠️ Time & Score Panel bağlı değil");
        }
        else
        {
            Debug.LogError("❌ GameManager bulunamadı!");
        }
    }

    private void TestPortalSpawner()
    {
        Debug.Log("🔍 PortalSpawner Testi:");

        AdvancedPortalSpawner ps = FindObjectOfType<AdvancedPortalSpawner>();
        if (ps != null)
        {
            Debug.Log("✅ AdvancedPortalSpawner bulundu");

            // Prefab kontrolleri
            int prefabCount = 0;
            if (ps.portalPrefabA != null) prefabCount++;
            if (ps.portalPrefabB != null) prefabCount++;
            if (ps.portalPrefabC != null) prefabCount++;

            Debug.Log($"📦 Portal Prefab'leri: {prefabCount}/3 bağlı");

            // Enemy prefab kontrolleri
            int enemyCount = 0;
            if (ps.tur1Enemy != null) enemyCount++;
            if (ps.tur2Enemy != null) enemyCount++;
            if (ps.tur3Enemy != null) enemyCount++;

            Debug.Log($"👹 Enemy Prefab'leri: {enemyCount}/3 bağlı");

            // Portal location kontrolü
            int portalLocationCount = 0;
            if (ps.portalLocationA != null) portalLocationCount++;
            if (ps.portalLocationB != null) portalLocationCount++;
            if (ps.portalLocationC != null) portalLocationCount++;

            Debug.Log($"📍 Portal Locations: {portalLocationCount}/3 ayarlanmış");
        }
        else
        {
            Debug.LogError("❌ AdvancedPortalSpawner bulunamadı!");
        }
    }

    private void TestWeaponManager()
    {
        Debug.Log("🔍 WeaponManager Testi:");

        WeaponManager wm = FindObjectOfType<WeaponManager>();
        if (wm != null)
        {
            Debug.Log("✅ WeaponManager bulundu");

            // Weapon prefab kontrolleri
            int weaponCount = 0;
            if (wm.firstWeapon != null) weaponCount++;
            if (wm.secondWeapon != null) weaponCount++;
            if (wm.thirdWeapon != null) weaponCount++;
            if (wm.fourthWeapon != null) weaponCount++;
            if (wm.fifthWeapon != null) weaponCount++;
            if (wm.sixthWeapon != null) weaponCount++;
            if (wm.seventhWeapon != null) weaponCount++;
            if (wm.eighthWeapon != null) weaponCount++;
            if (wm.ninthWeapon != null) weaponCount++;

            Debug.Log($"🔫 Weapon Prefab'leri: {weaponCount}/9 bağlı");
        }
        else
        {
            Debug.LogError("❌ WeaponManager bulunamadı!");
        }
    }

    private void TestUI()
    {
        Debug.Log("🔍 UI Sistemi Testi:");

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"📱 Canvas sayısı: {canvases.Length}");

        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"  - {canvas.name} ({(canvas.gameObject.activeSelf ? "Aktif" : "Pasif")})");
        }
    }

    private void TestPrefabs()
    {
        Debug.Log("🔍 Prefab Testi:");

        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>();
        Debug.Log($"👹 Aktif Enemy sayısı: {enemies.Length}");

        GunFire[] guns = FindObjectsOfType<GunFire>();
        Debug.Log($"🔫 Aktif Gun sayısı: {guns.Length}");
    }

    public void PrintSystemStatus()
    {
        Debug.Log("📊 SİSTEM DURUMU:");
        Debug.Log("================");

        Debug.Log($"OVRCameraRig: {FindObjectOfType<OVRCameraRig>() != null}");
        Debug.Log($"GameManager: {FindObjectOfType<GameManager>() != null}");
        Debug.Log($"PortalSpawner: {FindObjectOfType<AdvancedPortalSpawner>() != null}");
        Debug.Log($"WeaponManager: {FindObjectOfType<WeaponManager>() != null}");
        Debug.Log($"Canvas: {FindObjectsOfType<Canvas>().Length} adet");
        Debug.Log($"Enemy: {FindObjectsOfType<EnemyHealth>().Length} adet");
        Debug.Log($"Gun: {FindObjectsOfType<GunFire>().Length} adet");

        Debug.Log("================");
    }
}
