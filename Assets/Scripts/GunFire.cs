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

    //[Header("Magic System")]
    //public bool isMagicalGun = false; 
    //private bool isMagicTouching = false;
    //public float tiltAngle = -15f; 
    //public float rotationSpeed = 5f; 

    private Quaternion originalRotation;
    private bool isOutOfAmmo = false;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
        // Artik dusman oldugunde otomatik reload yok - mermi bitince oyun biter

        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (!canFire) return;

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
            if (OVRInput.Get(fireButton) && currentAmmo > 0 && !isFiring)
            {
                isFiring = true;
                StartCoroutine(AutoFire(fireButton));
            }
            else if (!OVRInput.Get(fireButton) || currentAmmo <= 0)
            {
                isFiring = false;
                StopFireSound();
            }
        }
        else
        {
            if (OVRInput.GetDown(fireButton) && currentAmmo > 0 && canFire)
            {
                StartCoroutine(FireWithCooldown());
            }
        }

        if (currentAmmo <= 0 && !isOutOfAmmo)
        {
            // Mermi bitince oyun biter
            isOutOfAmmo = true;
            if (GameManager.Instance != null)
            {
                Debug.Log("Mermi bitti! Oyun bitiyor...");
                GameManager.Instance.GameOver(null);
            }
        }
    }

    private IEnumerator FireWithCooldown()
    {
        canFire = false;
        Fire();
        StartCoroutine(HapticFeedback());
        currentAmmo -= 1;
        UpdateAmmoDisplay();

        yield return new WaitForSeconds(fireCooldown);

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

        while (OVRInput.Get(fireButton) && currentAmmo > 0)
        {
            Fire();
            StartCoroutine(HapticFeedback());
            currentAmmo -= 1;
            UpdateAmmoDisplay();

            yield return new WaitForSeconds(fireCooldown);
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
        FireFromBarrel(barrel1, targetDirection1);

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

        // WeaponManager üzerinden ateş sesini çal
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.PlayFireSound();
        }
    }

    private void FireFromBarrel(Transform barrel, Transform target)
    {
        if (barrel == null || target == null) return; 

        GameObject spawnedBullet = Instantiate(bulletPrefab, barrel.position, Quaternion.LookRotation(target.position - barrel.position));
        spawnedBullet.GetComponent<Rigidbody>().velocity = velocity * (target.position - barrel.position).normalized;

        Bullet bulletScript = spawnedBullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.hitSound = bulletHitSound;
            bulletScript.damageEffectPrefab = damageEffectPrefab;
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

    public void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
    }

    public void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
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
