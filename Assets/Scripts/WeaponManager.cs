using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

    [Header("Weapon SFX")]
    [Tooltip("Her tetik basıldığında çalacak mermi sesleri (1-9. silah)")]
    public AudioClip[] weaponFireSFX = new AudioClip[9];
    [Tooltip("5. silah: Charge tamamlandığında patlama sesi")]
    public AudioClip weapon5ChargeReleaseSFX;
    [Tooltip("6. silah: Slow laser açıkken loop çalacak ses")]
    public AudioClip weapon6SlowLaserSFX;
    [Tooltip("7. silah: Kırbaç her çıktığında ses")]
    public AudioClip weapon7WhipSFX;
    [Tooltip("8. silah: ToyHelper her ateş ettiğinde ses")]
    public AudioClip weapon8ToyFireSFX;
    [Tooltip("9. silah: Flame spray basılı tutarken loop ses")]
    public AudioClip weapon9FlameSprayLoopSFX;
    [Tooltip("9. silah: Fireball tetiklendiğinde ses")]
    public AudioClip weapon9FireballSFX;
    [Tooltip("SFX çalmak için AudioSource (boşsa otomatik eklenir)")]
    public AudioSource sfxAudioSource;

    [Header("Haptic - İsabet (düşmana vurulduğunda)")]
    [Tooltip("İsabet haptic şiddeti (0-1)")]
    [Range(0f, 1f)]
    public float hitHapticStrength = 0.4f;
    [Tooltip("İsabet haptic süresi (saniye)")]
    public float hitHapticDuration = 0.06f;

    [Header("Weapons")]
    public GameObject firstWeapon;
    public GameObject secondWeapon;
    public GameObject thirdWeapon;
    public GameObject fourthWeapon;
    public GameObject fifthWeapon;
    public GameObject sixthWeapon;
    [Tooltip("6. silahtayken sol elde açılacak ekstra silah (Baretta değil). Laser spawn point içermeli.")]
    public GameObject sixthWeaponLeftHand;
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

    [Header("Tutorial - VFX Pozisyonu (Sağ/Sol El)")]
    [Tooltip("Atanırsa silah değişim VFX bu Transform'un child'ı olarak oynatılır (sağ el)")]
    public Transform tutorialWeaponChangeVFXParentRight;
    [Tooltip("Atanırsa sol elde silah varsa VFX bu Transform'un child'ı olarak oynatılır")]
    public Transform tutorialWeaponChangeVFXParentLeft;

    [Header("Tutorial - Silah Değişim Süreleri (saniye)")]
    [Tooltip("Hangi silah index'i çift elde (Baretta). Sadece bu indeks için el VFX her iki elde oynar.")]
    public int tutorialBarettaWeaponIndex = 1;
    [Tooltip("Mevcut silahın elinden kaybolma süresi")]
    public float tutorialDespawnDuration = 0.5f;
    [Tooltip("Despawn ile spawn arasındaki bekleme")]
    public float tutorialDelayBetweenWeapons = 0.3f;
    [Tooltip("Yeni silahın elde belirme süresi")]
    public float tutorialSpawnDuration = 0.7f;
    public GameObject vfxThird;
    public GameObject vfxFourth;
    public GameObject vfxFifth;
    public GameObject vfxSixth;
    public GameObject vfxSeventh;
    public GameObject vfxEighth;
    public GameObject vfxNinth;

    private int currentWeapon = 0;
    private int enemyKillCount = 0;
    private bool isSwitchingWeapon = false;
    private AudioSource _loopSfxSource; // Slow laser ve flame spray loop için
    private int _lastFiringController = -1; // 0 = sol, 1 = sağ, -1 = bilinmiyor (OVRInput.Controller enum değeri)
    /// <summary>Bir kez 9. silaha ulaşıldığında true olur; joystick ile tüm silahlar arasında gezinmeye izin verir.</summary>
    private bool _allWeaponsUnlockedPermanent = false;

    [Header("Weapon Switch Settings")]
    [Tooltip("false ise kill ile silah değişimi çalışmaz (Tutorial'da dialogue 8'e kadar kapalı)")]
    public bool weaponSwitchEnabled = true;
    [Tooltip("Fallback when GameBalanceManager is not present. GameBalanceManager.GetKillToUnlock tek kaynaktır.")]
    public int killsToSecond = 4;
    public int killsToThird = 12;
    public int killsToFourth = 20;
    public int killsToFifth = 32;
    public int killsToSixth = 48;
    public int killsToSeventh = 68;
    public int killsToEighth = 82;
    public int killsToNinth = 102;
    public float vfxDelay = 0.5f;

    [Header("TEST - Sixth Left Hand on First Weapon")]
    [Tooltip("true = SixthWeaponLeftHand sadece 1. silahta açılır (test). false = normal (6. silahta açılır).")]
    public bool testSixthLeftHandOnSecondWeapon = true;

    [Header("Weapon Scale Settings")]
    public float activeGlobalScale = 1.2f;
    public float inactiveGlobalScale = 1.0f;

    [Header("UI Image Scale Settings")]
    public float activeUIImageScale = 0.8f;
    public float inactiveUIImageScale = 0.6f;

    [Header("Holographic HUD - Weapon Display Names")]
    public string[] weaponDisplayNames = new string[9];

    public int CurrentWeaponIndex => currentWeapon;
    /// <summary>9. silah açıldıysa true; joystick ile 1-9 arası tüm silahlara kaydırma açılır.</summary>
    public bool AllWeaponsUnlocked => _allWeaponsUnlockedPermanent;
    public event System.Action<int> OnWeaponChanged;

    public int GetCurrentWeaponIndex() => currentWeapon;
    public int GetEnemyKillCount() => enemyKillCount;
    /// <summary>6. silahtayken sol elde açılan ekstra silah. SixthGunLaser ikinci spawn point için kullanır.</summary>
    public GameObject GetSixthWeaponLeftHand() => sixthWeaponLeftHand;

    /// <summary>Oyuncu oyun başladıktan sonra en az bir kez ateş ettiyse true (DestructibleMeshHint ipucu için kullanılır).</summary>
    public static bool HasPlayerFiredSinceLevelLoad { get; private set; }

    /// <summary>Her tetik basıldığında çağrılır (1-9. silah mermi sesi). weaponIndex 0-8.</summary>
    public void PlayFireSound(int weaponIndex = -1)
    {
        HasPlayerFiredSinceLevelLoad = true;
        if (weaponIndex < 0 || weaponIndex > 8) weaponIndex = currentWeapon;
        PlaySFX(GetFireClip(weaponIndex));
    }

    /// <summary>5. silah: Charge tamamlandığında patlama sesi.</summary>
    public void PlayWeapon5ChargeReleaseSFX()
    {
        PlaySFX(weapon5ChargeReleaseSFX);
    }

    /// <summary>6. silah: Slow laser açıkken loop başlat, kapalıyken durdur.</summary>
    public void PlayWeapon6SlowLaserSFX(bool startLoop)
    {
        if (startLoop && weapon6SlowLaserSFX != null)
        {
            EnsureLoopSource();
            if (!_loopSfxSource.isPlaying || _loopSfxSource.clip != weapon6SlowLaserSFX)
            {
                _loopSfxSource.volume = MusicManager.Instance != null ? MusicManager.Instance.GetSfxVolume() : 1f;
                _loopSfxSource.clip = weapon6SlowLaserSFX;
                _loopSfxSource.loop = true;
                _loopSfxSource.Play();
            }
        }
        else
        {
            StopLoopSFX();
        }
    }

    /// <summary>7. silah: Kırbaç her çıktığında ses.</summary>
    public void PlayWeapon7WhipSFX()
    {
        PlaySFX(weapon7WhipSFX);
    }

    /// <summary>8. silah: ToyHelper her ateş ettiğinde ses.</summary>
    public void PlayWeapon8ToyFireSFX()
    {
        PlaySFX(weapon8ToyFireSFX);
    }

    /// <summary>9. silah: Flame spray basılı tutarken loop başlat/durdur.</summary>
    public void PlayWeapon9FlameSprayLoopSFX(bool startLoop)
    {
        if (startLoop && weapon9FlameSprayLoopSFX != null)
        {
            EnsureLoopSource();
            if (!_loopSfxSource.isPlaying || _loopSfxSource.clip != weapon9FlameSprayLoopSFX)
            {
                _loopSfxSource.volume = MusicManager.Instance != null ? MusicManager.Instance.GetSfxVolume() : 1f;
                _loopSfxSource.clip = weapon9FlameSprayLoopSFX;
                _loopSfxSource.loop = true;
                _loopSfxSource.Play();
            }
        }
        else
        {
            StopLoopSFX();
        }
    }

    /// <summary>9. silah: Fireball tetiklendiğinde ses.</summary>
    public void PlayWeapon9FireballSFX()
    {
        PlaySFX(weapon9FireballSFX);
    }

    private AudioClip GetFireClip(int weaponIndex)
    {
        if (weaponFireSFX == null || weaponIndex < 0 || weaponIndex >= weaponFireSFX.Length) return null;
        return weaponFireSFX[weaponIndex];
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySfx(clip);
            return;
        }
        AudioSource src = sfxAudioSource != null ? sfxAudioSource : GetOrCreateSFXSource();
        if (src != null)
            src.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
    }

    private void EnsureLoopSource()
    {
        if (_loopSfxSource == null)
        {
            GameObject loopObj = new GameObject("WeaponManager_LoopSFX");
            loopObj.transform.SetParent(transform);
            _loopSfxSource = loopObj.AddComponent<AudioSource>();
        }
    }

    private void StopLoopSFX()
    {
        if (_loopSfxSource != null && _loopSfxSource.isPlaying)
        {
            _loopSfxSource.Stop();
            _loopSfxSource.clip = null;
        }
    }

    private AudioSource GetOrCreateSFXSource()
    {
        if (sfxAudioSource != null) return sfxAudioSource;
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();
        sfxAudioSource = src;
        return src;
    }

    public void PlayHitSound()
    {
        // Ses çalma mantığını buraya ekleyebilirsin
    }

    /// <summary>Hangi kontrolcüyle ateş edildiğini kaydeder (isabet haptic için).</summary>
    public void SetLastFiringController(int controllerMaskOrIndex)
    {
        _lastFiringController = controllerMaskOrIndex;
    }

    /// <summary>Düşmana isabet ettiğinde kontrolcülere haptic verir. Son ateş eden el biliniyorsa sadece o, değilse her iki el.</summary>
    public void TriggerHitHaptic()
    {
        StartCoroutine(HitHapticRoutine());
    }

    private IEnumerator HitHapticRoutine()
    {
        float freq = 1f;
        float amp = Mathf.Clamp01(hitHapticStrength);
        float dur = Mathf.Max(0.02f, hitHapticDuration);
        bool left = (_lastFiringController == (int)OVRInput.Controller.LTouch);
        bool right = (_lastFiringController == (int)OVRInput.Controller.RTouch);
        if (_lastFiringController < 0)
        {
            left = true;
            right = true;
        }
        if (left)
            OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.LTouch);
        if (right)
            OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(dur);
        if (left)
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        if (right)
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        if (sixthWeaponLeftHand != null)
        {
            if (testSixthLeftHandOnSecondWeapon)
            {
                sixthWeaponLeftHand.SetActive(true);
                var laserOnLeft = sixthWeaponLeftHand.GetComponent<SixthGunLaser>();
                if (laserOnLeft != null) laserOnLeft.enabled = true;
            }
            else
                sixthWeaponLeftHand.SetActive(false);
        }
        if (seventhWeapon != null) seventhWeapon.SetActive(false);
        if (eighthWeapon != null) eighthWeapon.SetActive(false);
        if (ninthWeapon != null) ninthWeapon.SetActive(false);

        currentWeapon = 0;
        enemyKillCount = 0;
        isSwitchingWeapon = false;
        _allWeaponsUnlockedPermanent = false;

        SetWeaponBalanceIndices();

        // 1. silah sırasında sol el baretta açık olsun (1-2-3-7. silahlar sol elde baretta kullanır)
        for (int i = 1; i <= 8; i++)
            SetLeftHandBarettaActiveForWeaponIndex(i, false);
        if (HasLeftHandBarettaForWeaponIndex(0))
            SetLeftHandBarettaActiveForWeaponIndex(0, true);
    }

    private void SetWeaponBalanceIndices()
    {
        if (firstWeapon != null)  SetGunFireBalanceIndex(firstWeapon, 0);
        if (secondWeapon != null) SetGunFireBalanceIndex(secondWeapon, 1);
        if (thirdWeapon != null)  SetGunFireBalanceIndex(thirdWeapon, 2);
        if (fourthWeapon != null) SetGunFireBalanceIndex(fourthWeapon, 3);
        if (fifthWeapon != null) SetGunFireBalanceIndex(fifthWeapon, 4);
        if (sixthWeapon != null) SetGunFireBalanceIndex(sixthWeapon, 5);
        if (seventhWeapon != null) SetGunFireBalanceIndex(seventhWeapon, 6);
        if (eighthWeapon != null) SetGunFireBalanceIndex(eighthWeapon, 7);
        if (ninthWeapon != null) SetGunFireBalanceIndex(ninthWeapon, 8);
    }

    private void SetGunFireBalanceIndex(GameObject weaponObj, int index)
    {
        var gunFire = weaponObj.GetComponent<GunFire>();
        if (gunFire != null)
            gunFire.SetWeaponBalanceIndex(index);
    }

    /// <summary>Bu silah indeksinde sol elde baretta (LeftHandAnchor, isBaretta+isLeftHanded) varsa true. 1., 2., 3. ve 7. silahlar (indeks 0,1,2,6).</summary>
    private static bool HasLeftHandBarettaForWeaponIndex(int weaponIndex)
    {
        return weaponIndex == 0 || weaponIndex == 1 || weaponIndex == 2 || weaponIndex == 6;
    }

    /// <summary>Verilen silah indeksine ait sol el baretta'yı açar veya kapatır. isBaretta ve isLeftHanded olan, aynı weaponBalanceIndex'e sahip silah bulunur.</summary>
    private void SetLeftHandBarettaActiveForWeaponIndex(int weaponIndex, bool active)
    {
        GunFire[] all = FindObjectsOfType<GunFire>(true);
        foreach (GunFire g in all)
        {
            if (g.isBaretta && g.isLeftHanded && g.WeaponBalanceIndex == weaponIndex)
            {
                g.gameObject.SetActive(active);
                g.enabled = active;
                return;
            }
        }
    }

    /// <summary>Verilen silah indeksine ait sol el baretta GameObject'ini döner. Tutorial dissolve VFX için.</summary>
    private GameObject GetLeftHandBarettaGameObject(int weaponIndex)
    {
        GunFire[] all = FindObjectsOfType<GunFire>(true);
        foreach (GunFire g in all)
        {
            if (g.isBaretta && g.isLeftHanded && g.WeaponBalanceIndex == weaponIndex)
                return g.gameObject;
        }
        return null;
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
            return;

        if (TutorialIntroController.TutorialFifthWeaponPhase && !TutorialIntroController.TutorialFifthWeaponGunChangeAfterEnabled
            && EnemyHealth.LastKillWeaponIndex == 4 && EnemyHealth.LastKillWasFromSecondary)
            return;

        if (TutorialIntroController.TutorialSeventhWeaponPhase && EnemyHealth.LastKillWeaponIndex == 6 && EnemyHealth.LastKillWasFromSecondary)
            return;

        if (TutorialIntroController.TutorialEighthWeaponPhase && EnemyHealth.LastKillWeaponIndex == 7 && EnemyHealth.LastKillWasFromSecondary)
            return;

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
        // newtutorial'da dissolve VFX coroutine silahları kapatacak - burada erken kapatma
        if (SceneManager.GetActiveScene().name == "newtutorial") return;
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

    /// <summary>Kill count required to unlock the next weapon (index 1..8). From GameBalanceManager or fallback fields.</summary>
    public int GetKillsRequiredForNextWeapon(int nextWeaponIndex)
    {
        if (GameBalanceManager.Instance != null && nextWeaponIndex >= 1 && nextWeaponIndex <= 8)
            return GameBalanceManager.Instance.GetKillToUnlock(nextWeaponIndex);
        switch (nextWeaponIndex) { case 1: return killsToSecond; case 2: return killsToThird; case 3: return killsToFourth; case 4: return killsToFifth; case 5: return killsToSixth; case 6: return killsToSeventh; case 7: return killsToEighth; case 8: return killsToNinth; default: return 999; }
    }

    public void CheckWeaponSwitch()
    {
        if (!weaponSwitchEnabled || isSwitchingWeapon)
            return;

        // 9 silah tamamlandıysa kill ile otomatik silah değişimi çalışmasın; sadece joystick ile seçilen silah kullanılsın
        if (_allWeaponsUnlockedPermanent)
            return;

        int killsForSecond = GetKillsRequiredForNextWeapon(1);
        // First -> Second
        if (currentWeapon == 0 && enemyKillCount >= killsForSecond)
        {
            isSwitchingWeapon = true;
            DisableAllBarettaWeapons();
            if (firstWeapon != null)
            {
                GunFire gunFire = firstWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(firstWeapon, secondWeapon, vfxFirst, vfxSecond));
            currentWeapon = 1;
            HandleBarettaSwitch(secondWeapon);
            // TEST: 1. silahtan 2'ye geçince SlowedGun kapat (testte sadece 1. silahta açık)
            if (testSixthLeftHandOnSecondWeapon && sixthWeaponLeftHand != null) sixthWeaponLeftHand.SetActive(false);
            Debug.Log($"First weapon kapatıldı, Second weapon açıldı ({enemyKillCount} kill).");
        }
        // Second -> Third
        else if (currentWeapon == 1 && enemyKillCount >= GetKillsRequiredForNextWeapon(2))
        {
            isSwitchingWeapon = true;
            if (testSixthLeftHandOnSecondWeapon && sixthWeaponLeftHand != null) sixthWeaponLeftHand.SetActive(false);
            if (secondWeapon != null)
            {
                GunFire gunFire = secondWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(secondWeapon, thirdWeapon, vfxSecond, vfxThird));
            currentWeapon = 2;
            HandleBarettaSwitch(thirdWeapon);
            Debug.Log($"Second weapon kapatıldı, Third weapon açıldı ({enemyKillCount} kill).");
        }
        // Third -> Fourth
        else if (currentWeapon == 2 && enemyKillCount >= GetKillsRequiredForNextWeapon(3))
        {
            isSwitchingWeapon = true;
            if (thirdWeapon != null)
            {
                GunFire gunFire = thirdWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(thirdWeapon, fourthWeapon, vfxThird, vfxFourth));
            currentWeapon = 3;
            HandleBarettaSwitch(fourthWeapon);
            Debug.Log($"Third weapon kapatıldı, Fourth weapon açıldı ({enemyKillCount} kill).");
        }
        // Fourth -> Fifth
        else if (currentWeapon == 3 && enemyKillCount >= GetKillsRequiredForNextWeapon(4))
        {
            isSwitchingWeapon = true;
            if (fourthWeapon != null)
            {
                GunFire gunFire = fourthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(fourthWeapon, fifthWeapon, vfxFourth, vfxFifth));
            currentWeapon = 4;
            HandleBarettaSwitch(fifthWeapon);
            Debug.Log($"Fourth weapon kapatıldı, Fifth weapon açıldı ({enemyKillCount} kill).");
        }
        // Fifth -> Sixth (Tutorial: 5. silahta 3 kill sonrası serbest)
        else if (currentWeapon == 4 && enemyKillCount >= GetKillsRequiredForNextWeapon(5))
        {
            if (TutorialIntroController.TutorialFifthWeaponPhase && !TutorialIntroController.TutorialFifthWeaponGunChangeAfterEnabled)
                return;
            isSwitchingWeapon = true;
            if (fifthWeapon != null)
            {
                GunFire gunFire = fifthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(fifthWeapon, sixthWeapon, vfxFifth, vfxSixth));
            currentWeapon = 5;
            HandleBarettaSwitch(sixthWeapon);
            // TEST kapalıyken normal: 6. silahta sol el açılır
            if (!testSixthLeftHandOnSecondWeapon && sixthWeaponLeftHand != null)
            {
                sixthWeaponLeftHand.SetActive(true);
                var laserOnLeft = sixthWeaponLeftHand.GetComponent<SixthGunLaser>();
                if (laserOnLeft != null) laserOnLeft.enabled = true; // Sol trigger ile slow laser
            }
            Debug.Log($"Fifth weapon kapatıldı, Sixth weapon açıldı ({enemyKillCount} kill).");
        }
        // Sixth -> Seventh (Tutorial: 16. diyalog sonrası serbest)
        else if (currentWeapon == 5 && enemyKillCount >= GetKillsRequiredForNextWeapon(6))
        {
            if (TutorialIntroController.TutorialSixthWeaponPhase && !TutorialIntroController.TutorialSixthWeaponGunChangeAfterEnabled)
                return;
            if (seventhWeapon == null)
            {
                Debug.LogError("WeaponManager: Seventh weapon atanmamış! Inspector'da WeaponManager > Seventh Weapon slot'una silahı atayın.");
                return;
            }
            isSwitchingWeapon = true;
            if (sixthWeapon != null)
            {
                GunFire gunFire = sixthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(sixthWeapon, seventhWeapon, vfxSixth, vfxSeventh));
            currentWeapon = 6;
            HandleBarettaSwitch(seventhWeapon);
            Debug.Log($"Sixth weapon kapatıldı, Seventh weapon açıldı ({enemyKillCount} kill).");
        }
        // Seventh -> Eighth (Tutorial: 18. diyalog sonrası serbest)
        else if (currentWeapon == 6 && enemyKillCount >= GetKillsRequiredForNextWeapon(7))
        {
            if (TutorialIntroController.TutorialSeventhWeaponPhase && !TutorialIntroController.TutorialSeventhWeaponGunChangeAfterEnabled)
                return;
            isSwitchingWeapon = true;
            if (seventhWeapon != null)
            {
                GunFire gunFire = seventhWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(seventhWeapon, eighthWeapon, vfxSeventh, vfxEighth));
            currentWeapon = 7;
            HandleBarettaSwitch(eighthWeapon);
            Debug.Log($"Seventh weapon kapatıldı, Eighth weapon açıldı ({enemyKillCount} kill).");
        }
        // Eighth -> Ninth (Tutorial: 20. diyalog sonrası serbest)
        else if (currentWeapon == 7 && enemyKillCount >= GetKillsRequiredForNextWeapon(8))
        {
            if (TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialEighthWeaponGunChangeAfterEnabled)
                return;
            isSwitchingWeapon = true;
            if (eighthWeapon != null)
            {
                GunFire gunFire = eighthWeapon.GetComponent<GunFire>();
                if (gunFire != null) gunFire.enabled = false;
            }
            StartCoroutine(SwitchWeaponWithVFX(eighthWeapon, ninthWeapon, vfxEighth, vfxNinth));
            currentWeapon = 8;
            bool inTutorial = FindObjectOfType<TutorialIntroController>() != null;
            _allWeaponsUnlockedPermanent = !inTutorial; // Tutorial'da 23. diyalogda açılacak
            HandleBarettaSwitch(ninthWeapon);
            Debug.Log($"Eighth weapon kapatıldı, Ninth weapon açıldı ({enemyKillCount} kill).");
        }

        UpdateWeaponUI();
    }

    private void HandleBarettaSwitch(GameObject newWeapon)
    {
        if (newWeapon == null) return;

        GunFire gunFire = newWeapon.GetComponent<GunFire>();
        if (gunFire != null && gunFire.isBaretta)
        {
            gunFire.isLeftHanded = false;

            // Sadece sol el kopyası kapatılsın; oyuncuya verdiğimiz silah (yeni geçilen) görünür ve kullanılabilir kalsın.
            // Önceki kod tüm Renderer/Collider kapatıyordu → seventh gun gibi silahlar "spawn olmuyor" gibi görünüyordu.
            if (gunFire.isLeftHanded)
            {
                newWeapon.SetActive(false);
                Debug.Log($"{newWeapon.name} sol eldeydi, tamamen devre dışı bırakıldı.");
            }
        }
    }







    private IEnumerator SwitchWeaponWithVFX(GameObject currentWeaponObj, GameObject nextWeaponObj, GameObject currentWeaponVFX, GameObject nextWeaponVFX)
    {
        StopLoopSFX();
        // Yeni silah null ise coroutine'i güvenli şekilde bitir
        if (nextWeaponObj == null)
        {
            Debug.LogError("WeaponManager: Geçilecek silah (nextWeaponObj) atanmamış! Inspector'da ilgili weapon slot'unu kontrol edin.");
            UpdateWeaponUI();
            isSwitchingWeapon = false;
            OnWeaponChanged?.Invoke(currentWeapon);
            yield break;
        }

        var prevGun = currentWeaponObj != null ? currentWeaponObj.GetComponent<GunFire>() : null;
        int prevWeaponIndex = prevGun != null ? prevGun.WeaponBalanceIndex : -1;
        GameObject leftHandBaretta = (prevGun != null && HasLeftHandBarettaForWeaponIndex(prevWeaponIndex)) ? GetLeftHandBarettaGameObject(prevWeaponIndex) : null;

        if (currentWeaponVFX != null)
        {
            if (currentWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
                ps.Stop();
            currentWeaponVFX.SetActive(false);
        }

        // Dissolve: Mevcut silahı yukarıdan aşağıya siler
        var dissolveCurrent = currentWeaponObj != null ? (currentWeaponObj.GetComponent<WeaponDissolveEffect>() ?? currentWeaponObj.AddComponent<WeaponDissolveEffect>()) : null;
        if (dissolveCurrent != null)
            yield return dissolveCurrent.PlayDespawn(tutorialDespawnDuration);
        else if (currentWeaponObj != null)
            yield return new WaitForSeconds(tutorialDespawnDuration);

        // Sol el despawn
        if (leftHandBaretta != null && leftHandBaretta.activeSelf)
        {
            var dissolveLeft = leftHandBaretta.GetComponent<WeaponDissolveEffect>() ?? leftHandBaretta.AddComponent<WeaponDissolveEffect>();
            yield return dissolveLeft.PlayDespawn(tutorialDespawnDuration);
        }

        if (currentWeaponObj != null)
            currentWeaponObj.SetActive(false);
        if (prevGun != null)
            SetLeftHandBarettaActiveForWeaponIndex(prevGun.WeaponBalanceIndex, false);
        if (dissolveCurrent != null) dissolveCurrent.EnsureVisible();
        if (leftHandBaretta != null)
        {
            var dl = leftHandBaretta.GetComponent<WeaponDissolveEffect>();
            if (dl != null) dl.EnsureVisible();
        }

        // 6. silahtan çıkarken sol eli de despawn et
        GameObject sixthLeft = (currentWeaponObj == sixthWeapon && sixthWeaponLeftHand != null && sixthWeaponLeftHand.activeSelf) ? sixthWeaponLeftHand : null;
        if (sixthLeft != null)
        {
            var dissolveSixthLeft = sixthLeft.GetComponent<WeaponDissolveEffect>() ?? sixthLeft.AddComponent<WeaponDissolveEffect>();
            yield return dissolveSixthLeft.PlayDespawn(tutorialDespawnDuration);
            sixthLeft.SetActive(false);
            dissolveSixthLeft.EnsureVisible();
        }

        yield return new WaitForSeconds(tutorialDelayBetweenWeapons);

        nextWeaponObj.SetActive(true);
        var dissolveNext = nextWeaponObj.GetComponent<WeaponDissolveEffect>() ?? nextWeaponObj.AddComponent<WeaponDissolveEffect>();
        var gunFire = nextWeaponObj.GetComponent<GunFire>();
        if (gunFire != null) gunFire.enabled = false;

        if (nextWeaponVFX != null)
        {
            nextWeaponVFX.SetActive(true);
            if (nextWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem psNext))
                psNext.Play();
        }

        yield return dissolveNext.PlaySpawn(tutorialSpawnDuration);

        if (nextWeaponVFX != null && nextWeaponVFX.TryGetComponent<ParticleSystem>(out ParticleSystem psVfx))
        {
            yield return new WaitForSeconds(psVfx.main.duration);
            psVfx.Stop();
        }

        if (gunFire != null)
        {
            gunFire.enabled = true;
            if (gunFire.isBaretta || HasLeftHandBarettaForWeaponIndex(currentWeapon))
            {
                GameObject leftHandNew = GetLeftHandBarettaGameObject(currentWeapon);
                if (leftHandNew != null)
                {
                    leftHandNew.SetActive(true);
                    var dissolveLeftNew = leftHandNew.GetComponent<WeaponDissolveEffect>() ?? leftHandNew.AddComponent<WeaponDissolveEffect>();
                    yield return dissolveLeftNew.PlaySpawn(tutorialSpawnDuration);
                    SetLeftHandBarettaActiveForWeaponIndex(currentWeapon, true);
                }
                else
                {
                    SetLeftHandBarettaActiveForWeaponIndex(currentWeapon, true);
                }
            }
        }

        if (currentWeapon == 5 && !testSixthLeftHandOnSecondWeapon && sixthWeaponLeftHand != null)
        {
            sixthWeaponLeftHand.SetActive(true);
            var dissolveSixthLeft = sixthWeaponLeftHand.GetComponent<WeaponDissolveEffect>() ?? sixthWeaponLeftHand.AddComponent<WeaponDissolveEffect>();
            yield return dissolveSixthLeft.PlaySpawn(tutorialSpawnDuration);
            var laserOnLeft = sixthWeaponLeftHand.GetComponent<SixthGunLaser>();
            if (laserOnLeft != null) laserOnLeft.enabled = true;
        }

        UpdateWeaponUI();
        isSwitchingWeapon = false;
        OnWeaponChanged?.Invoke(currentWeapon);
        Debug.Log("Yeni silaha geçildi: " + nextWeaponObj.name);
    }

    public void NextWeaponManual()
    {
        if (!_allWeaponsUnlockedPermanent) return;
        SelectWeaponByIndex((currentWeapon + 1) % 9);
    }

    public void PreviousWeaponManual()
    {
        if (!_allWeaponsUnlockedPermanent) return;
        SelectWeaponByIndex((currentWeapon - 1 + 9) % 9);
    }

    /// <summary>Tutorial 23. diyalogda joystick ile silah geçişini açar.</summary>
    public void SetJoystickWeaponSwitchEnabled(bool enabled)
    {
        _allWeaponsUnlockedPermanent = enabled;
    }

    /// <summary>
    /// Sadece newtutorial sahnesi için. Kill şartı olmadan silaha geçiş + VFX.
    /// Ana oyun bu metodu çağırmaz - sahne adı kontrolü ile korunur.
    /// </summary>
    public void TutorialOnly_SwitchToWeaponWithVFX(int targetIndex)
    {
        if (SceneManager.GetActiveScene().name != "newtutorial") return;
        if (targetIndex < 0 || targetIndex > 8) return;
        if (targetIndex == currentWeapon) return;
        if (isSwitchingWeapon) return;

        GameObject currentObj = GetWeaponAt(currentWeapon);
        GameObject nextObj = GetWeaponAt(targetIndex);
        if (nextObj == null) return;

        isSwitchingWeapon = true;

        if (currentObj != null)
        {
            var gf = currentObj.GetComponent<GunFire>();
            if (gf != null) gf.enabled = false;
        }

        // 2. silahtan (Baretta) çıkarken DisableAllBarettaWeapons ÇAĞIRMA - dissolve VFX coroutine içinde oynatılacak
        if (testSixthLeftHandOnSecondWeapon && sixthWeaponLeftHand != null && currentWeapon == 0)
            sixthWeaponLeftHand.SetActive(false);

        HandleBarettaSwitch(nextObj);

        GameObject currentVFX = GetVFXAt(currentWeapon);
        GameObject nextVFX = GetVFXAt(targetIndex);
        currentWeapon = targetIndex;
        StartCoroutine(SwitchWeaponWithVFX(currentObj, nextObj, currentVFX, nextVFX));
    }

    private GameObject GetVFXAt(int index)
    {
        switch (index)
        {
            case 0: return vfxFirst; case 1: return vfxSecond; case 2: return vfxThird;
            case 3: return vfxFourth; case 4: return vfxFifth; case 5: return vfxSixth;
            case 6: return vfxSeventh; case 7: return vfxEighth; case 8: return vfxNinth;
            default: return null;
        }
    }

    /// <summary>Mevcut silahı yenile. Baretta ise partner da yenilenir. Tutorial mermi bitti retry için.</summary>
    public void ReloadCurrentWeapon()
    {
        GameObject w = GetWeaponAt(currentWeapon);
        if (w == null) return;
        GunFire gf = w.GetComponent<GunFire>();
        if (gf != null)
        {
            gf.Reload();
            if (gf.isBaretta)
            {
                GunFire[] allGuns = FindObjectsOfType<GunFire>();
                foreach (GunFire g in allGuns)
                {
                    if (g.isBaretta && g != gf) { g.Reload(); break; }
                }
            }
        }
    }

    public void SelectWeaponByIndex(int index)
    {
        if (!_allWeaponsUnlockedPermanent) return;
        if (index < 0 || index > 8) return;
        SetWeaponByIndex(index);
    }

    private GameObject GetWeaponAt(int index)
    {
        switch (index)
        {
            case 0: return firstWeapon;
            case 1: return secondWeapon;
            case 2: return thirdWeapon;
            case 3: return fourthWeapon;
            case 4: return fifthWeapon;
            case 5: return sixthWeapon;
            case 6: return seventhWeapon;
            case 7: return eighthWeapon;
            case 8: return ninthWeapon;
            default: return null;
        }
    }

    private void SetWeaponByIndex(int index)
    {
        if (currentWeapon == 5 || currentWeapon == 8) StopLoopSFX();
        // Tüm sol el barettaları kapat (sadece mevcut silahın sol eli sonra açılacak)
        for (int i = 0; i < 9; i++)
            SetLeftHandBarettaActiveForWeaponIndex(i, false);
        // TEST: sol el sadece 1. silahta (index 0); normal modda sadece 6. silahta (index 5) açık
        bool showSixthLeft = testSixthLeftHandOnSecondWeapon ? (index == 0) : (index == 5);
        if (sixthWeaponLeftHand != null)
        {
            if (!showSixthLeft) sixthWeaponLeftHand.SetActive(false);
        }
        for (int i = 0; i < 9; i++)
        {
            GameObject w = GetWeaponAt(i);
            if (w != null)
            {
                w.SetActive(false);
                GunFire gf = w.GetComponent<GunFire>();
                if (gf != null) gf.enabled = false;
            }
        }
        GameObject next = GetWeaponAt(index);
        if (next != null)
        {
            next.SetActive(true);
            var dissolve = next.GetComponent<WeaponDissolveEffect>();
            if (dissolve != null) dissolve.EnsureVisible();
            GunFire nextGun = next.GetComponent<GunFire>();
            if (nextGun != null)
            {
                nextGun.enabled = true;
                if (nextGun.isBaretta || HasLeftHandBarettaForWeaponIndex(index))
                    SetLeftHandBarettaActiveForWeaponIndex(index, true);
            }
        }
        if (showSixthLeft && sixthWeaponLeftHand != null)
        {
            sixthWeaponLeftHand.SetActive(true);
            var dissolve = sixthWeaponLeftHand.GetComponent<WeaponDissolveEffect>();
            if (dissolve != null) dissolve.EnsureVisible();
            var laserOnLeft = sixthWeaponLeftHand.GetComponent<SixthGunLaser>();
            if (laserOnLeft != null) laserOnLeft.enabled = true;
        }
        currentWeapon = index;
        UpdateWeaponUI();
        OnWeaponChanged?.Invoke(currentWeapon);
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

}