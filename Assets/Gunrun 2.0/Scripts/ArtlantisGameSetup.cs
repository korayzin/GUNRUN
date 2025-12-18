using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ArtlantisGameSetup : MonoBehaviour
{
    [Header("VR Player Setup")]
    public OVRCameraRig ovrCameraRig;

    [Header("Game Managers")]
    public GameManager gameManager;
    public AdvancedPortalSpawner portalSpawner;
    public WeaponManager weaponManager;
    public SoundManager soundManager;

    [Header("UI Canvases")]
    public GameObject gameUICanvas;
    public GameObject weaponUICanvas;
    public GameObject restartCanvas;

    [Header("Portal Prefabs")]
    public GameObject portalPrefabA;
    public GameObject portalPrefabB;
    public GameObject portalPrefabC;

    [Header("Enemy Prefabs")]
    public GameObject tur1Enemy;
    public GameObject tur2Enemy;
    public GameObject tur3Enemy;

    [Header("Weapon Prefabs")]
    public GameObject weaponA;
    public GameObject weaponB;
    public GameObject weaponC;

    void Start()
    {
        StartCoroutine(InitializeGameSystems());
    }

    private IEnumerator InitializeGameSystems()
    {
        Debug.Log("Artlantis sahnesi başlatılıyor...");

        // 1. VR sistemi kontrolü
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig atanmamış!");
            yield break;
        }

        // 2. GameManager kurulumu
        if (gameManager != null)
        {
            gameManager.restartCanvas = restartCanvas;
            gameManager.timeAndScorePanel = gameUICanvas;
            Debug.Log("GameManager kuruldu");
        }

        // 3. PortalSpawner kurulumu
        if (portalSpawner != null)
        {
            portalSpawner.portalPrefabA = portalPrefabA;
            portalSpawner.portalPrefabB = portalPrefabB;
            portalSpawner.portalPrefabC = portalPrefabC;

            portalSpawner.tur1Enemy = tur1Enemy;
            portalSpawner.tur2Enemy = tur2Enemy;
            portalSpawner.tur3Enemy = tur3Enemy;

            // Spawn points sadece AdvancedPortalSpawner.cs içinde kontrol edilir
            Debug.Log("PortalSpawner kuruldu");
        }

        // 4. WeaponManager kurulumu
        if (weaponManager != null)
        {
            weaponManager.weaponA = weaponA;
            weaponManager.weaponB = weaponB;
            weaponManager.weaponC = weaponC;

            // VFX referansları (varsa ekle)
            // weaponManager.vfxA = vfxA;
            // weaponManager.vfxB = vfxB;
            // weaponManager.vfxC = vfxC;

            Debug.Log("WeaponManager kuruldu");
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("Artlantis sahnesi hazır!");
    }
}
