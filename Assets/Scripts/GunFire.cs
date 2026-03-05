using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using Meta.XR.MRUtilityKit;

public class GunFire : MonoBehaviour
{
    public float velocity;
    public GameObject bulletPrefab;

    public Transform barrel1, barrel2, barrel3, barrel4, barrel5;
    public Transform targetDirection1, targetDirection2, targetDirection3, targetDirection4, targetDirection5;

    public AudioSource audioSource;
    public ParticleSystem ps;
    public Animator gunAnimator;
    public GameObject muzzleFlashPrefab;

    [Header("Haptic Feedback Settings")]
    public float hapticStrength = 0.5f;

    [Header("Bullet Settings")]
    public AudioClip bulletHitSound;
    public GameObject damageEffectPrefab;

    [Header("Ammo Settings")]
    public int maxAmmo = 20;
    private int currentAmmo;
    public TextMeshProUGUI ammoText;
    public GameObject ammoUI;

    [Header("Weapon Settings")]
    public bool useDualBarrel = false;
    public bool isAutomatic = false;
    private bool isFiring = false;

    [Header("Fire Settings")]
    public float fireCooldown = 0.5f;
    public bool canFire = true;

    [Header("Weapon Type")]
    public bool isBaretta = false;
    public bool isLeftHanded = false;

    [Header("Balance (per-weapon from GameBalanceManager)")]
    [Tooltip("0=FirstGun, 8=LastGun. Set by WeaponManager or Inspector.")]
    [SerializeField] private int weaponBalanceIndex = 0;
    /// <summary>Sol el baretta eşleşmesi için WeaponManager tarafından kullanılır.</summary>
    public int WeaponBalanceIndex => weaponBalanceIndex;

    [Header("Dual Shot (altlı üstlü 2 mermi)")]
    [Tooltip("Açıkken her atışta 2 mermi atar (üst + alt), yine 1 mermi harcanır.")]
    public bool useDualShotVertical = false;
    [Tooltip("İki mermi arasındaki dikey mesafe (barrel.up yönünde).")]
    public float dualShotSpacing = 0.04f;

    //[Header("Magic System")]
    //public bool isMagicalGun = false; 
    //private bool isMagicTouching = false;
    //public float tiltAngle = -15f; 
    //public float rotationSpeed = 5f; 

    private Quaternion originalRotation;
    private Quaternion bulletPrefabRotation;
    private bool isOutOfAmmo = false;

    public void SetWeaponBalanceIndex(int index)
    {
        weaponBalanceIndex = Mathf.Clamp(index, 0, 8);
        ApplyBalanceFromManager();
    }

    private void ApplyBalanceFromManager()
    {
        if (GameBalanceManager.Instance == null) return;
        var data = GameBalanceManager.Instance.GetWeaponData(weaponBalanceIndex);
        velocity = data.bulletVelocity;
        maxAmmo = data.maxAmmo;
        fireCooldown = data.fireCooldown;
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
    }

    /// <summary>Silah her etkinleştirildiğinde (9. silahtan sonra joystick ile geçiş) ateş hazır olsun.
    /// SetWeaponByIndex ile silah değişince FireWithCooldown coroutine kesilir, canFire false kalır - bu düzeltir.</summary>
    private void OnEnable()
    {
        canFire = true;
        isFiring = false;
    }

    void Start()
    {
        ApplyBalanceFromManager();
        if (GameBalanceManager.Instance == null)
            currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
        originalRotation = transform.localRotation;

        if (bulletPrefab != null)
        {
            GameObject tempPrefab = Instantiate(bulletPrefab);
            bulletPrefabRotation = tempPrefab.transform.rotation;
            Destroy(tempPrefab);
        }
        else
            bulletPrefabRotation = Quaternion.identity;
    }

    void Update()
    {
        // Mermi/enerji bitti mi kontrolü (Tutorial sınırsız mermi modunda atla)
        if (!TutorialIntroController.TutorialUnlimitedAmmo && currentAmmo <= 0 && !isOutOfAmmo)
        {
            bool partnerHasAmmo = isBaretta && FindPartnerBaretta() != null && FindPartnerBaretta().GetCurrentAmmo() > 0;
            if (!partnerHasAmmo)
            {
                bool inTutorial = FindObjectOfType<TutorialIntroController>() != null;
                if (inTutorial && TutorialIntroController.TutorialActive)
                {
                    var ctrl = FindObjectOfType<TutorialIntroController>();
                    if (ctrl != null)
                    {
                        isOutOfAmmo = true;
                        ctrl.HandleTutorialOutOfAmmo();
                        return;
                    }
                }
                bool isLastTwoWeapons = weaponBalanceIndex >= 7;
                if (inTutorial && !isLastTwoWeapons)
                    return;
                isOutOfAmmo = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver(null);
            }
        }

        if (!canFire) return;
        if (GameManager.IsRetryScreenActive) return; // Retry ekranında sadece HandRayUIInteractor ile butonlara tıklanabilir
        if (!TutorialIntroController.TutorialFiringEnabled) return;

        // LastGun: ateş püskürtme bu silahta LastGunFlameSpray tarafından yönetilir, mermi atma.
        var flameSpray = GetComponent<LastGunFlameSpray>();
        if (flameSpray != null && flameSpray.enabled)
            return;

        if (ammoUI != null)
        {
            ammoUI.transform.rotation = Quaternion.LookRotation(ammoUI.transform.position - Camera.main.transform.position);
        }

        //if (isMagicalGun)
        //{
            //Quaternion targetRotation = isMagicTouching
            //    ? originalRotation 
            //    : Quaternion.AngleAxis(tiltAngle, transform.up) * originalRotation; 

            //transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);

            //Debug.LogError("isMagicTouching: " + isMagicTouching);
        //}

        //transform.Rotate(Vector3.right, 10f * Time.deltaTime);
        //Debug.Log("Rotation: " + transform.localEulerAngles);

        OVRInput.Button fireButton = isLeftHanded ? OVRInput.Button.PrimaryIndexTrigger : OVRInput.Button.SecondaryIndexTrigger;

        if (isAutomatic)
        {
            if (OVRInput.Get(fireButton) && (TutorialIntroController.TutorialUnlimitedAmmo || currentAmmo > 0) && !isFiring)
            {
                isFiring = true;
                StartCoroutine(AutoFire(fireButton));
            }
            else if (!OVRInput.Get(fireButton) || (!TutorialIntroController.TutorialUnlimitedAmmo && currentAmmo <= 0))
            {
                isFiring = false;
                StopFireSound();
            }
        }
        else
        {
            if (OVRInput.GetDown(fireButton) && (TutorialIntroController.TutorialUnlimitedAmmo || currentAmmo > 0) && canFire)
            {
                StartCoroutine(FireWithCooldown());
            }
        }

    }

    // Diğer Baretta silahını bul (sol ise sağı, sağ ise solu)
    private GunFire FindPartnerBaretta()
    {
        GunFire[] allGuns = FindObjectsOfType<GunFire>();
        foreach (GunFire gun in allGuns)
        {
            // Aktif, Baretta ve farklı el (partner)
            if (gun != this && gun.isBaretta && gun.gameObject.activeInHierarchy && gun.isLeftHanded != this.isLeftHanded)
            {
                return gun;
            }
        }
        return null;
    }

    // Mevcut mermi sayısını döndür
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    /// <summary>
    /// Alternatif giriş (örn. LastGun A tuşu) ile tek atış. Bu silahın bulletPrefab'ını kullanır.
    /// </summary>
    /// <param name="isSecondary">true = A tuşu (fireball), Tutorial ikincil kill takibi için</param>
    public bool TryFire(bool isSecondary = false)
    {
        if (!canFire || (!TutorialIntroController.TutorialUnlimitedAmmo && !TutorialIntroController.TutorialNinthWeaponFireballUnlimited && currentAmmo <= 0)) return false;
        _nextShotIsFromSecondary = isSecondary;
        StartCoroutine(FireWithCooldown());
        return true;
    }

    private bool _nextShotIsFromSecondary = false;
    private bool _canRestoreAmmoThisShot = false; // İlk 2 silah: destructible mesh'e çarpınca en fazla 1 iade (dual shot için)

    private IEnumerator FireWithCooldown()
    {
        canFire = false;
        _canRestoreAmmoThisShot = (weaponBalanceIndex == 0 || weaponBalanceIndex == 1);
        Fire();
        StartCoroutine(HapticFeedback());
        if (!TutorialIntroController.TutorialUnlimitedAmmo && !TutorialIntroController.TutorialNinthWeaponFireballUnlimited)
        {
            currentAmmo -= 1;
            UpdateAmmoDisplay();
        }

        yield return new WaitForSecondsRealtime(fireCooldown);

        canFire = true;
    }

    private IEnumerator AutoFire(OVRInput.Button fireButton)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = true;
            audioSource.Play();
        }

        while (OVRInput.Get(fireButton) && (TutorialIntroController.TutorialUnlimitedAmmo || currentAmmo > 0))
        {
            _canRestoreAmmoThisShot = (weaponBalanceIndex == 0 || weaponBalanceIndex == 1);
            Fire();
            StartCoroutine(HapticFeedback());
            if (!TutorialIntroController.TutorialUnlimitedAmmo)
            {
                currentAmmo -= 1;
                UpdateAmmoDisplay();
            }

            yield return new WaitForSecondsRealtime(fireCooldown);
        }

        StopFireSound();
    }

    private void StopFireSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    public void Fire()
    {
        if (useDualShotVertical && barrel1 != null && targetDirection1 != null)
        {
            float half = dualShotSpacing * 0.5f;
            FireFromBarrel(barrel1, targetDirection1, half);
            FireFromBarrel(barrel1, targetDirection1, -half);
        }
        else
        {
            FireFromBarrel(barrel1, targetDirection1);
        }

        if (useDualBarrel)
        {
            FireFromBarrel(barrel2, targetDirection2);
            FireFromBarrel(barrel3, targetDirection3);
            FireFromBarrel(barrel4, targetDirection4);
            FireFromBarrel(barrel5, targetDirection5);
        }

        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Shoot");
        }

        if (ps != null)
        {
            ps.Play();
        }

        // WeaponManager üzerinden ateş sesini çal ve hangi el ile ateş edildiğini kaydet (isabet haptic için)
        if (WeaponManager.Instance != null)
        {
            if (weaponBalanceIndex == 8 && _nextShotIsFromSecondary)
                WeaponManager.Instance.PlayWeapon9FireballSFX();
            else
                WeaponManager.Instance.PlayFireSound(weaponBalanceIndex);
            WeaponManager.Instance.SetLastFiringController((int)(isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch));
        }
    }

    private void FireFromBarrel(Transform barrel, Transform target)
    {
        FireFromBarrel(barrel, target, 0f);
    }

    private void FireFromBarrel(Transform barrel, Transform target, float verticalOffset)
    {
        if (barrel == null || target == null) return;

        Vector3 spawnPos = barrel.position + (verticalOffset != 0f ? barrel.up * verticalOffset : Vector3.zero);

        // Prefab'ın rotasyonunu baz alarak hesapla
        Quaternion lookRotation = Quaternion.LookRotation(target.position - barrel.position);
        Quaternion finalRotation = lookRotation * bulletPrefabRotation;

        GameObject spawnedBullet = Instantiate(bulletPrefab, spawnPos, finalRotation);
        Vector3 targetDirection = (target.position - barrel.position).normalized;
        spawnedBullet.GetComponent<Rigidbody>().velocity = velocity * targetDirection;

        Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // Fireball (weapon 8) dahil tüm silahlar: Tutorial'da da normal oyunla aynı hasar (GameBalanceManager)
            if (GameBalanceManager.Instance != null)
                bulletScript.damage = GameBalanceManager.Instance.GetWeaponData(weaponBalanceIndex).damage;
            else if (weaponBalanceIndex == 8)
                bulletScript.damage = 12f; // LastGun fireball default (GameBalanceManager.GetDefaultWeaponData(8))
            bulletScript.hitSound = bulletHitSound;
            bulletScript.damageEffectPrefab = damageEffectPrefab;
            bulletScript.isFromSecondary = _nextShotIsFromSecondary;
            bulletScript.weaponIndex = weaponBalanceIndex;
            bulletScript.sourceGunFire = this; // İlk 2 silah destructible mesh'e çarpınca mermi iade için
            _nextShotIsFromSecondary = false;
            bulletScript.SetMovementDirection(targetDirection);
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, barrel.position, barrel.rotation);
            Destroy(flash, 0.2f);
        }

        Destroy(spawnedBullet, 2f);
    }

    private IEnumerator HapticFeedback()
    {
        OVRInput.Controller controller = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(1, hapticStrength, controller);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, controller);
    }

    /// <summary>İlk 2 silahın mermisi destructible mesh'e çarptığında mermi iade için. Atış başına en fazla 1 iade (dual shot için).</summary>
    public void RestoreAmmo(int amount)
    {
        if (!_canRestoreAmmoThisShot) return;
        _canRestoreAmmoThisShot = false;
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoDisplay();
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
        isOutOfAmmo = false; // Yenilendiğinde flag'i sıfırla
        
        // Debug log ekle
        Debug.Log($"[Reload] {gameObject.name}: Mermi {currentAmmo}/{maxAmmo} olarak yenilendi. ammoText null mu? {ammoText == null}");
        
        UpdateAmmoDisplay();
    }

    public void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            if (TutorialIntroController.TutorialUnlimitedAmmo)
            {
                ammoText.gameObject.SetActive(false);
                return;
            }
            ammoText.gameObject.SetActive(true);
            ammoText.text = currentAmmo.ToString();
            
            // Son 3 mermide kırmızı, son 5 mermide turuncu, diğer durumlarda beyaz
            if (currentAmmo <= 3)
            {
                ammoText.color = Color.red;
            }
            else if (currentAmmo <= 5)
            {
                ammoText.color = new Color(1f, 0.5f, 0f); // Turuncu
            }
            else
            {
                ammoText.color = Color.white;
            }
        }
        else
        {
            Debug.LogWarning($"[UpdateAmmoDisplay] {gameObject.name}: ammoText null! Mermi sayısı güncellenemedi.");
        }
    }

    public void SetWeapon(bool isBaretta, bool isLeftHanded)
    {
        this.isBaretta = isBaretta;
        this.isLeftHanded = isLeftHanded;
    }

    //private void OnTriggerEnter(Collider other)
    //{
        //if (isMagicalGun && other.CompareTag("Magic"))
     //   {
            //Debug.LogError("Magic temas etti!");
     //       isMagicTouching = true;
    //    }
   // }

    //private void OnTriggerExit(Collider other)
   // {
     //   if (isMagicalGun && other.CompareTag("Magic"))
     //   {
            //Debug.LogError("Magic temas kayboldu!");
     //       isMagicTouching = false;
      //  }

}
