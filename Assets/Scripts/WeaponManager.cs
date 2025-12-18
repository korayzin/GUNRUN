using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject weaponA;
    public GameObject weaponB;
    public GameObject weaponC;
    public GameObject weaponD;

    [Header("Weapon UI Images")]
    public Image weaponAImage;
    public Image weaponBImage;
    public Image weaponCImage;
    public Image weaponDImage;

    [Header("Weapon VFX")]
    public GameObject vfxA;
    public GameObject vfxB;
    public GameObject vfxC;
    public GameObject vfxD;

    private int currentWeapon = 0;
    private int enemyKillCount = 0;

    [Header("Weapon Switch Settings")]
    public int killsToWeaponB = 10;
    public int killsToWeaponC = 22;
    public int killsToWeaponD = 36;
    public float vfxDelay = 0.5f;

    [Header("Weapon Scale Settings")]
    public float activeGlobalScale = 1.2f;
    public float inactiveGlobalScale = 1.0f;

    [Header("UI Image Scale Settings")]
    public float activeUIImageScale = 0.8f;
    public float inactiveUIImageScale = 0.6f;

    private void Start()
    {
        UpdateWeaponUI();
    }

    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled;
    }

    private void OnEnemyKilled()
    {
        int currentScore = GameManager.Instance.score;

        if (currentScore < killsToWeaponB)
        {
            return; 
        }

        CheckWeaponSwitch(currentScore);
    }


    private void DisableLeftHandWeapons()
    {
        GunFire[] allGuns = FindObjectsOfType<GunFire>();
        foreach (GunFire gun in allGuns)
        {
            if (gun.isBaretta && gun.isLeftHanded)
            {
                gun.gameObject.SetActive(false);
                Debug.Log($"{gun.gameObject.name} sol elde olduğu için kapatıldı.");
            }
        }
    }

    public void CheckWeaponSwitch(int score)
    {
        if (currentWeapon == 0 && score >= killsToWeaponB)
        {
            StartCoroutine(SwitchWeaponWithVFX(weaponA, weaponB, vfxA, vfxB));
            currentWeapon = 1;
            HandleBarettaSwitch(weaponB);
            DisableLeftHandWeapons();
            Debug.Log("USP açıldı, sol el silahı kapatıldı.");
        }
        else if (currentWeapon == 1 && score >= killsToWeaponC)
        {
            StartCoroutine(SwitchWeaponWithVFX(weaponB, weaponC, vfxB, vfxC));
            currentWeapon = 2;
            HandleBarettaSwitch(weaponC);
        }
        else if (currentWeapon == 2 && score >= killsToWeaponD)
        {
            // StartCoroutine(SwitchWeaponWithVFX(weaponC, weaponD, vfxC, vfxD));
            // currentWeapon = 3;
            // HandleBarettaSwitch(weaponD);
        }

        UpdateWeaponUI();
    }

    private void HandleBarettaSwitch(GameObject newWeapon)
    {
        GunFire gunFire = newWeapon.GetComponent<GunFire>();
        if (gunFire != null && gunFire.isBaretta)
        {
            gunFire.isLeftHanded = false;

            if (!gunFire.isAutomatic)
            {
                // Ateş etmesini engelle
                gunFire.canFire = false;

                // Tüm Renderer bileşenlerini kapat (silahın görünürlüğünü kaldır)
                Renderer[] renderers = newWeapon.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = false;
                }

                // Tüm Collider bileşenlerini devre dışı bırak (silahla etkileşim olmasın)
                Collider[] colliders = newWeapon.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }

                // Eğer silah sol eldeyse, GameObject'i tamamen kapat
                if (gunFire.isLeftHanded)
                {
                    newWeapon.SetActive(false);
                    Debug.Log($"{newWeapon.name} sol eldeydi, tamamen devre dışı bırakıldı.");
                }
            }
        }
    }







    private IEnumerator SwitchWeaponWithVFX(GameObject currentWeaponObj, GameObject nextWeaponObj, GameObject currentWeaponVFX, GameObject nextWeaponVFX)
    {
        if (currentWeaponVFX != null)
        {
            if (currentWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                ps.Stop();
            }
            currentWeaponVFX.SetActive(false);
        }

        GunFire currentGunFire = currentWeaponObj.GetComponent<GunFire>();
        if (currentGunFire != null)
        {
            currentGunFire.enabled = false;
            Debug.Log($"{currentWeaponObj.name} silahı kapatıldı.");
        }

        currentWeaponObj.SetActive(false);
        yield return new WaitForSeconds(vfxDelay); 

        if (nextWeaponVFX != null)
        {
            nextWeaponVFX.SetActive(true); // **Yeni VFX açılıyor**

            if (nextWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem psNext))
            {
                psNext.Play();
                Debug.Log("Silah değiştirme VFX oynatılıyor...");

                yield return new WaitForSeconds(psNext.main.duration);

                psNext.Stop(); 
            }
        }

        nextWeaponObj.SetActive(true);
        Debug.Log("Yeni silaha geçildi: " + nextWeaponObj.name);

        GunFire nextGunFire = nextWeaponObj.GetComponent<GunFire>();
        if (nextGunFire != null)
        {
            nextGunFire.enabled = true;
            Debug.Log($"{nextWeaponObj.name} silahı açıldı.");
        }

        UpdateWeaponUI();
    }


    private void UpdateWeaponUI()
    {
        SetWeaponUIImageAlphaAndScale(weaponAImage, currentWeapon == 0);
        SetWeaponUIImageAlphaAndScale(weaponBImage, currentWeapon == 1);
        SetWeaponUIImageAlphaAndScale(weaponCImage, currentWeapon == 2);
        SetWeaponUIImageAlphaAndScale(weaponDImage, currentWeapon == 3);
    }

    private void SetWeaponUIImageAlphaAndScale(Image image, bool isActive)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = isActive ? 1f : 0.5f;
            image.color = color;

            image.rectTransform.localScale = Vector3.one * (isActive ? activeUIImageScale : inactiveUIImageScale);
        }
    }

}