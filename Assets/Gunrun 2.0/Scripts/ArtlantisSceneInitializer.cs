using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtlantisSceneInitializer : MonoBehaviour
{
    [Header("Sahne Başlatma Ayarları")]
    public string artlantisSceneName = "Artlantis";
    public bool initializeOnStart = true;

    void Start()
    {
        if (initializeOnStart)
        {
            InitializeArtlantisScene();
        }
    }

    public void InitializeArtlantisScene()
    {
        Debug.Log("Artlantis sahnesi başlatılıyor...");

        // Mevcut sahnedeki tüm gerekli sistemleri kontrol et ve başlat
        CheckAndInitializeSystems();
    }

    private void CheckAndInitializeSystems()
    {
        // 1. GameManager kontrolü
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager bulunamadı! Artlantis sahnesine GameManager ekleyin.");
        }
        else
        {
            Debug.Log("GameManager bulundu ve çalışıyor.");
        }

        // 2. PortalSpawner kontrolü
        AdvancedPortalSpawner portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();
        if (portalSpawner == null)
        {
            Debug.LogError("AdvancedPortalSpawner bulunamadı! Artlantis sahnesine AdvancedPortalSpawner ekleyin.");
        }
        else
        {
            Debug.Log("AdvancedPortalSpawner bulundu ve çalışıyor.");
        }

        // 3. WeaponManager kontrolü
        WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager bulunamadı! Artlantis sahnesine WeaponManager ekleyin.");
        }
        else
        {
            Debug.Log("WeaponManager bulundu ve çalışıyor.");
        }

        // 4. VR sistemi kontrolü
        OVRCameraRig ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig bulunamadı! Artlantis sahnesine OVRCameraRig ekleyin.");
        }
        else
        {
            Debug.Log("OVRCameraRig bulundu ve çalışıyor.");
        }

        // 5. UI Canvas kontrolü
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases.Length == 0)
        {
            Debug.LogWarning("Hiç Canvas bulunamadı. UI sistemleri eksik olabilir.");
        }
        else
        {
            Debug.Log($"{canvases.Length} adet Canvas bulundu.");
        }
    }

    // Sahne geçişi için yardımcı metod
    public void LoadArtlantisScene()
    {
        SceneManager.LoadScene(artlantisSceneName);
    }

    // Debug için sistem durumunu yazdır
    public void PrintSystemStatus()
    {
        Debug.Log("=== ARTLANTIS SAHNESİ SİSTEM DURUMU ===");

        Debug.Log($"GameManager: {FindObjectOfType<GameManager>() != null}");
        Debug.Log($"PortalSpawner: {FindObjectOfType<AdvancedPortalSpawner>() != null}");
        Debug.Log($"WeaponManager: {FindObjectOfType<WeaponManager>() != null}");
        Debug.Log($"OVRCameraRig: {FindObjectOfType<OVRCameraRig>() != null}");
        Debug.Log($"Canvas sayısı: {FindObjectsOfType<Canvas>().Length}");
        Debug.Log($"Enemy sayısı: {FindObjectsOfType<EnemyHealth>().Length}");

        Debug.Log("=====================================");
    }
}
