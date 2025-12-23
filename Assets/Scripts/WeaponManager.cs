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
    private bool isSwitchingWeapon = false;

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
        InitializeWeapons();
        UpdateWeaponUI();
    }

    private void InitializeWeapons()
    {
        if (weaponA != null) weaponA.SetActive(true);
        if (weaponB != null) weaponB.SetActive(false);
        if (weaponC != null) weaponC.SetActive(false);
        if (weaponD != null) weaponD.SetActive(false);

        currentWeapon = 0;
        enemyKillCount = 0;
        isSwitchingWeapon = false;
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
        if (isSwitchingWeapon)
        {
            return;
        }

        enemyKillCount++;

        CheckWeaponSwitch();
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

    private void DisableAllBarettaWeapons()
    {
        GunFire[] allGuns = FindObjectsOfType<GunFire>();
        foreach (GunFire gun in allGuns)
        {
            if (gun.isBaretta)
            {
                gun.gameObject.SetActive(false);
                gun.enabled = false;
                Debug.Log($"{gun.gameObject.name} baretta olduğu için kapatıldı (sol el: {gun.isLeftHanded}).");
            }
        }
    }

    public void CheckWeaponSwitch()
    {
        if (isSwitchingWeapon)
        {
            return;
        }

        if (currentWeapon == 0 && enemyKillCount >= killsToWeaponB)
        {
            isSwitchingWeapon = true;
            // Tüm baretta silahlarını kapat (hem sol hem sağ el)
            DisableAllBarettaWeapons();
            // WeaponA'yı da kapat
            if (weaponA != null)
            {
                weaponA.SetActive(false);
                GunFire gunFireA = weaponA.GetComponent<GunFire>();
                if (gunFireA != null) gunFireA.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(weaponA, weaponB, vfxA, vfxB));
            currentWeapon = 1;
            HandleBarettaSwitch(weaponB);
            Debug.Log($"Tüm baretta silahları kapatıldı, WeaponB açıldı ({enemyKillCount} kill).");
        }
        else if (currentWeapon == 1 && enemyKillCount >= killsToWeaponC)
        {
            isSwitchingWeapon = true;
            // WeaponB'yi hemen kapat
            if (weaponB != null)
            {
                weaponB.SetActive(false);
                GunFire gunFireB = weaponB.GetComponent<GunFire>();
                if (gunFireB != null) gunFireB.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(weaponB, weaponC, vfxB, vfxC));
            currentWeapon = 2;
            HandleBarettaSwitch(weaponC);
            Debug.Log($"WeaponB kapatıldı, WeaponC açıldı ({enemyKillCount} kill).");
        }
        else if (currentWeapon == 2 && enemyKillCount >= killsToWeaponD)
        {
            // isSwitchingWeapon = true;
            // // WeaponC'yi hemen kapat
            // if (weaponC != null)
            // {
            //     weaponC.SetActive(false);
            //     GunFire gunFireC = weaponC.GetComponent<GunFire>();
            //     if (gunFireC != null) gunFireC.enabled = false;
            // }
            // StartCoroutine(SwitchWeaponWithVFX(weaponC, weaponD, vfxC, vfxD));
            // currentWeapon = 3;
            // HandleBarettaSwitch(weaponD);
            // Debug.Log($"WeaponC kapatıldı, WeaponD açıldı ({enemyKillCount} kill).");
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
        // Önceki silah zaten CheckWeaponSwitch'te kapatıldı, burada sadece VFX'i kapat
        if (currentWeaponVFX != null)
        {
            if (currentWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
            {
                ps.Stop();
            }
            currentWeaponVFX.SetActive(false);
        }

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
        isSwitchingWeapon = false;
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