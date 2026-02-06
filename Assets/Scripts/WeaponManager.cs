using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }

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

    private int currentWeapon = 0;
    private int enemyKillCount = 0;
    private bool isSwitchingWeapon = false;
    private int _lastFiringController = -1; // 0 = sol, 1 = sağ, -1 = bilinmiyor (OVRInput.Controller enum değeri)
    /// <summary>Bir kez 9. silaha ulaşıldığında true olur; joystick ile tüm silahlar arasında gezinmeye izin verir.</summary>
    private bool _allWeaponsUnlockedPermanent = false;

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

    [Header("Holographic HUD - Weapon Display Names")]
    public string[] weaponDisplayNames = new string[9];

    public int CurrentWeaponIndex => currentWeapon;
    /// <summary>9. silah açıldıysa true; joystick ile 1-9 arası tüm silahlara kaydırma açılır.</summary>
    public bool AllWeaponsUnlocked => _allWeaponsUnlockedPermanent;
    public event System.Action<int> OnWeaponChanged;

    public int GetCurrentWeaponIndex() => currentWeapon;
    public int GetEnemyKillCount() => enemyKillCount;

    public void PlayFireSound()
    {
        // Ses çalma mantığını buraya ekleyebilirsin (örn. AudioSource.PlayClipAtPoint)
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
        if (seventhWeapon != null) seventhWeapon.SetActive(false);
        if (eighthWeapon != null) eighthWeapon.SetActive(false);
        if (ninthWeapon != null) ninthWeapon.SetActive(false);

        currentWeapon = 0;
        enemyKillCount = 0;
        isSwitchingWeapon = false;
        _allWeaponsUnlockedPermanent = false;
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
            return;

        // 9 silah tamamlandıysa kill ile otomatik silah değişimi çalışmasın; sadece joystick ile seçilen silah kullanılsın
        if (_allWeaponsUnlockedPermanent)
            return;

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
            if (seventhWeapon == null)
            {
                Debug.LogError("WeaponManager: Seventh weapon atanmamış! Inspector'da WeaponManager > Seventh Weapon slot'una silahı atayın.");
                return;
            }
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
            _allWeaponsUnlockedPermanent = true; // Joystick ile tüm silahlar arasında gezinme açıldı
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
        // Yeni silah null ise coroutine'i güvenli şekilde bitir (seventh weapon atanmamış olabilir)
        if (nextWeaponObj == null)
        {
            Debug.LogError("WeaponManager: Geçilecek silah (nextWeaponObj) atanmamış! Inspector'da ilgili weapon slot'unu kontrol edin.");
            UpdateWeaponUI();
            isSwitchingWeapon = false;
            OnWeaponChanged?.Invoke(currentWeapon);
            yield break;
        }

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
        OnWeaponChanged?.Invoke(currentWeapon);
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
            GunFire nextGun = next.GetComponent<GunFire>();
            if (nextGun != null) nextGun.enabled = true;
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