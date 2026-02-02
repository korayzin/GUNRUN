using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    // Singleton instance
    public static WeaponManager Instance { get; private set; }

    [Header("Weapons")]
    public GameObject firstWeapon;
    public GameObject secondWeapon;
    public GameObject thirdWeapon;
    public GameObject fourthWeapon;
    public GameObject fifthWeapon;
    public GameObject sixthWeapon;
    public GameObject seventhWeapon;
    public GameObject eighthWeapon;
    public GameObject ninthWeapon;

    [Header("Weapon UI Images")]
    public Image firstWeaponImage;
    public Image secondWeaponImage;
    public Image thirdWeaponImage;
    public Image fourthWeaponImage;
    public Image fifthWeaponImage;
    public Image sixthWeaponImage;
    public Image seventhWeaponImage;
    public Image eighthWeaponImage;
    public Image ninthWeaponImage;

    [Header("Weapon VFX")]
    public GameObject vfxFirst;
    public GameObject vfxSecond;
    public GameObject vfxThird;
    public GameObject vfxFourth;
    public GameObject vfxFifth;
    public GameObject vfxSixth;
    public GameObject vfxSeventh;
    public GameObject vfxEighth;
    public GameObject vfxNinth;

    [Header("Weapon Sound Effects")]
    [Tooltip("Her silah için ateş sesi (1-9 sırasıyla)")]
    public AudioClip[] fireSounds = new AudioClip[9];
    [Tooltip("Her silah için hit sesi (1-9 sırasıyla)")]
    public AudioClip[] hitSounds = new AudioClip[9];
    [Tooltip("Ateş sesi çalma süresi")]
    public float fireSoundDuration = 1f;
    [Tooltip("Hit haptic gücü (0-1)")]
    public float hitHapticStrength = 0.7f;
    [Tooltip("Hit haptic süresi")]
    public float hitHapticDuration = 0.15f;

    [Header("Audio Sources")]
    [Tooltip("Ateş sesleri için AudioSource")]
    public AudioSource fireAudioSource;
    [Tooltip("Hit sesleri için AudioSource")]
    public AudioSource hitAudioSource;

    private int currentWeapon = 0;
    private int enemyKillCount = 0;
    private bool isSwitchingWeapon = false;
    private Coroutine fireSoundCoroutine;

    [Header("Weapon Switch Settings")]
    public int killsToSecond = 10;
    public int killsToThird = 22;
    public int killsToFourth = 36;
    public int killsToFifth = 52;
    public int killsToSixth = 70;
    public int killsToSeventh = 90;
    public int killsToEighth = 112;
    public int killsToNinth = 136;
    public float vfxDelay = 0.5f;

    [Header("Weapon Scale Settings")]
    public float activeGlobalScale = 1.2f;
    public float inactiveGlobalScale = 1.0f;

    [Header("UI Image Scale Settings")]
    public float activeUIImageScale = 0.8f;
    public float inactiveUIImageScale = 0.6f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource'ları otomatik oluştur (atanmamışsa)
        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.playOnAwake = false;
        }
        if (hitAudioSource == null)
        {
            hitAudioSource = gameObject.AddComponent<AudioSource>();
            hitAudioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        InitializeWeapons();
        UpdateWeaponUI();
    }

    private void InitializeWeapons()
    {
        if (firstWeapon != null) firstWeapon.SetActive(true);
        if (secondWeapon != null) secondWeapon.SetActive(false);
        if (thirdWeapon != null) thirdWeapon.SetActive(false);
        if (fourthWeapon != null) fourthWeapon.SetActive(false);
        if (fifthWeapon != null) fifthWeapon.SetActive(false);
        if (sixthWeapon != null) sixthWeapon.SetActive(false);
        if (seventhWeapon != null) seventhWeapon.SetActive(false);
        if (eighthWeapon != null) eighthWeapon.SetActive(false);
        if (ninthWeapon != null) ninthWeapon.SetActive(false);

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

        // First -> Second
        if (currentWeapon == 0 && enemyKillCount >= killsToSecond)
        {
            isSwitchingWeapon = true;
            DisableAllBarettaWeapons();
            if (firstWeapon != null)
            {
                firstWeapon.SetActive(false);
                GunFire gunFire = firstWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(firstWeapon, secondWeapon, vfxFirst, vfxSecond));
            currentWeapon = 1;
            HandleBarettaSwitch(secondWeapon);
            Debug.Log($"First weapon kapatıldı, Second weapon açıldı ({enemyKillCount} kill).");
        }
        // Second -> Third
        else if (currentWeapon == 1 && enemyKillCount >= killsToThird)
        {
            isSwitchingWeapon = true;
            if (secondWeapon != null)
            {
                secondWeapon.SetActive(false);
                GunFire gunFire = secondWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(secondWeapon, thirdWeapon, vfxSecond, vfxThird));
            currentWeapon = 2;
            HandleBarettaSwitch(thirdWeapon);
            Debug.Log($"Second weapon kapatıldı, Third weapon açıldı ({enemyKillCount} kill).");
        }
        // Third -> Fourth
        else if (currentWeapon == 2 && enemyKillCount >= killsToFourth)
        {
            isSwitchingWeapon = true;
            if (thirdWeapon != null)
            {
                thirdWeapon.SetActive(false);
                GunFire gunFire = thirdWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(thirdWeapon, fourthWeapon, vfxThird, vfxFourth));
            currentWeapon = 3;
            HandleBarettaSwitch(fourthWeapon);
            Debug.Log($"Third weapon kapatıldı, Fourth weapon açıldı ({enemyKillCount} kill).");
        }
        // Fourth -> Fifth
        else if (currentWeapon == 3 && enemyKillCount >= killsToFifth)
        {
            isSwitchingWeapon = true;
            if (fourthWeapon != null)
            {
                fourthWeapon.SetActive(false);
                GunFire gunFire = fourthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(fourthWeapon, fifthWeapon, vfxFourth, vfxFifth));
            currentWeapon = 4;
            HandleBarettaSwitch(fifthWeapon);
            Debug.Log($"Fourth weapon kapatıldı, Fifth weapon açıldı ({enemyKillCount} kill).");
        }
        // Fifth -> Sixth
        else if (currentWeapon == 4 && enemyKillCount >= killsToSixth)
        {
            isSwitchingWeapon = true;
            if (fifthWeapon != null)
            {
                fifthWeapon.SetActive(false);
                GunFire gunFire = fifthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(fifthWeapon, sixthWeapon, vfxFifth, vfxSixth));
            currentWeapon = 5;
            HandleBarettaSwitch(sixthWeapon);
            Debug.Log($"Fifth weapon kapatıldı, Sixth weapon açıldı ({enemyKillCount} kill).");
        }
        // Sixth -> Seventh
        else if (currentWeapon == 5 && enemyKillCount >= killsToSeventh)
        {
            isSwitchingWeapon = true;
            if (sixthWeapon != null)
            {
                sixthWeapon.SetActive(false);
                GunFire gunFire = sixthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(sixthWeapon, seventhWeapon, vfxSixth, vfxSeventh));
            currentWeapon = 6;
            HandleBarettaSwitch(seventhWeapon);
            Debug.Log($"Sixth weapon kapatıldı, Seventh weapon açıldı ({enemyKillCount} kill).");
        }
        // Seventh -> Eighth
        else if (currentWeapon == 6 && enemyKillCount >= killsToEighth)
        {
            isSwitchingWeapon = true;
            if (seventhWeapon != null)
            {
                seventhWeapon.SetActive(false);
                GunFire gunFire = seventhWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(seventhWeapon, eighthWeapon, vfxSeventh, vfxEighth));
            currentWeapon = 7;
            HandleBarettaSwitch(eighthWeapon);
            Debug.Log($"Seventh weapon kapatıldı, Eighth weapon açıldı ({enemyKillCount} kill).");
        }
        // Eighth -> Ninth
        else if (currentWeapon == 7 && enemyKillCount >= killsToNinth)
        {
            isSwitchingWeapon = true;
            if (eighthWeapon != null)
            {
                eighthWeapon.SetActive(false);
                GunFire gunFire = eighthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(eighthWeapon, ninthWeapon, vfxEighth, vfxNinth));
            currentWeapon = 8;
            HandleBarettaSwitch(ninthWeapon);
            Debug.Log($"Eighth weapon kapatıldı, Ninth weapon açıldı ({enemyKillCount} kill).");
        }

        UpdateWeaponUI();
    }

    private void HandleBarettaSwitch(GameObject newWeapon)
    {
        // GunFire component'ini bul (prefab instance içinde de olabilir)
        GunFire gunFire = newWeapon.GetComponentInChildren<GunFire>(true);
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

        // Prefab instance child'larını da aktif et (mesh yükleme sorunları için)
        Transform[] allChildren = nextWeaponObj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.gameObject != nextWeaponObj && !child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
        }

        // Mesh renderer'ları açıkça etkinleştir (prefab instance sorunları için)
        Renderer[] allRenderers = nextWeaponObj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            renderer.enabled = true;
        }
        Debug.Log($"{nextWeaponObj.name} için {allRenderers.Length} renderer etkinleştirildi.");

        // GunFire component'ini bul (prefab instance içinde de olabilir)
        GunFire nextGunFire = nextWeaponObj.GetComponentInChildren<GunFire>(true);
        if (nextGunFire != null)
        {
            nextGunFire.enabled = true;
            nextGunFire.canFire = true; // Ateş edebilmesi için canFire'ı true yap
            Debug.Log($"{nextWeaponObj.name} silahı açıldı ve ateş edebilir durumda.");
        }
        else
        {
            Debug.LogWarning($"{nextWeaponObj.name} için GunFire component'i bulunamadı!");
        }

        UpdateWeaponUI();
        isSwitchingWeapon = false;
    }


    private void UpdateWeaponUI()
    {
        SetWeaponUIImageAlphaAndScale(firstWeaponImage, currentWeapon == 0);
        SetWeaponUIImageAlphaAndScale(secondWeaponImage, currentWeapon == 1);
        SetWeaponUIImageAlphaAndScale(thirdWeaponImage, currentWeapon == 2);
        SetWeaponUIImageAlphaAndScale(fourthWeaponImage, currentWeapon == 3);
        SetWeaponUIImageAlphaAndScale(fifthWeaponImage, currentWeapon == 4);
        SetWeaponUIImageAlphaAndScale(sixthWeaponImage, currentWeapon == 5);
        SetWeaponUIImageAlphaAndScale(seventhWeaponImage, currentWeapon == 6);
        SetWeaponUIImageAlphaAndScale(eighthWeaponImage, currentWeapon == 7);
        SetWeaponUIImageAlphaAndScale(ninthWeaponImage, currentWeapon == 8);
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

    #region Sound System

    /// <summary>
    /// Mevcut silahın ateş sesini çalar (1 saniye süreyle)
    /// </summary>
    public void PlayFireSound()
    {
        if (fireAudioSource == null) return;
        
        if (currentWeapon >= 0 && currentWeapon < fireSounds.Length && fireSounds[currentWeapon] != null)
        {
            // Önceki ses coroutine'ini durdur
            if (fireSoundCoroutine != null)
            {
                StopCoroutine(fireSoundCoroutine);
            }
            
            fireAudioSource.clip = fireSounds[currentWeapon];
            fireAudioSource.Play();
            fireSoundCoroutine = StartCoroutine(StopFireSoundAfterDuration());
        }
    }

    private IEnumerator StopFireSoundAfterDuration()
    {
        yield return new WaitForSeconds(fireSoundDuration);
        if (fireAudioSource != null && fireAudioSource.isPlaying)
        {
            fireAudioSource.Stop();
        }
    }

    /// <summary>
    /// Mevcut silahın hit sesini çalar ve haptic gönderir
    /// </summary>
    public void PlayHitSound()
    {
        // Hit sesini çal
        if (hitAudioSource != null && currentWeapon >= 0 && currentWeapon < hitSounds.Length && hitSounds[currentWeapon] != null)
        {
            hitAudioSource.PlayOneShot(hitSounds[currentWeapon]);
        }
        
        // Hit haptic gönder
        StartCoroutine(HitHapticFeedback());
    }

    private IEnumerator HitHapticFeedback()
    {
        // Her iki controller'a da haptic gönder (hangi elde silah tutuluyorsa hissetsin)
        OVRInput.SetControllerVibration(1, hitHapticStrength, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(1, hitHapticStrength, OVRInput.Controller.LTouch);
        
        yield return new WaitForSeconds(hitHapticDuration);
        
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    /// <summary>
    /// Mevcut aktif silah indeksini döndürür (0-8)
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return currentWeapon;
    }

    #endregion

}