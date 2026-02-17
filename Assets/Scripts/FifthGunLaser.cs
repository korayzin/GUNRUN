using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// FifthGun silahı için YILDIRIM sistemi.
/// A tuşu ile charge başlar, bırakınca PATLAMA!
/// </summary>
public class FifthGunLaser : MonoBehaviour
{
    [Header("=== YILDIRIM AYARLARI ===")]
    [Tooltip("Yıldırımın çıkış noktası")]
    public Transform laserSpawnPoint;
    
    [Tooltip("Maksimum menzil (metre)")]
    public float laserRange = 100f;
    
    [Tooltip("Hasar (9999 = instant kill)")]
    public float laserDamage = 9999f;
    
    [Tooltip("Kaç kez ateşlenebilir (0 = sınırsız)")]
    public int maxLaserShots = 2;
    
    [Header("=== CHARGE SİSTEMİ ===")]
    [Tooltip("Charge süresi (saniye)")]
    public float chargeDuration = 1.5f;
    
    [Tooltip("Minimum charge oranı (0-1) - bu kadar charge olmadan ateşlenmez")]
    [Range(0f, 1f)]
    public float minChargeToFire = 0.3f;
    
    [Tooltip("Charge sırasında titreşim")]
    public bool chargeVibration = true;
    
    [Header("=== YILDIRIM GÖRSELİ ===")]
    [Tooltip("Ana yıldırım rengi")]
    public Color lightningColor = new Color(0.4f, 0.6f, 1f, 1f); // Elektrik mavisi
    
    [Tooltip("Çekirdek rengi (parlak beyaz)")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Dış glow rengi")]
    public Color outerGlowColor = new Color(0.3f, 0.5f, 1f, 0.15f);
    
    [Tooltip("İç glow rengi")]
    public Color innerGlowColor = new Color(0.6f, 0.8f, 1f, 0.4f);
    
    [Tooltip("Ana yıldırım kalınlığı")]
    public float mainBoltWidth = 0.08f;
    
    [Tooltip("Çekirdek kalınlığı")]
    public float coreWidth = 0.025f;
    
    [Tooltip("Dış glow kalınlığı")]
    public float outerGlowWidth = 0.4f;
    
    [Tooltip("İç glow kalınlığı")]
    public float innerGlowWidth = 0.18f;
    
    [Header("=== YILDIRIM DETAYLARI ===")]
    [Tooltip("Segment sayısı (zigzag detayı)")]
    public int segments = 40;
    
    [Tooltip("Sapma miktarı")]
    public float jaggedAmount = 0.12f;
    
    [Tooltip("Sapma yumuşaklığı (0=keskin, 1=yumuşak)")]
    [Range(0f, 1f)]
    public float jaggedSmoothness = 0.4f;
    
    [Tooltip("Titreşim hızı")]
    public float flickerSpeed = 45f;
    
    [Tooltip("Ana dal sayısı")]
    public int mainBoltCount = 2;
    
    [Tooltip("Yan dal sayısı")]
    public int branchCount = 4;
    
    [Tooltip("Yan dal uzunluğu")]
    public float branchLength = 0.3f;
    
    [Header("=== KIVILCIM SİSTEMİ ===")]
    [Tooltip("Kıvılcım sayısı (charge sırasında)")]
    public int chargeSparks = 20;
    
    [Tooltip("Kıvılcım sayısı (ateşleme sırasında)")]
    public int fireSparks = 50;
    
    [Tooltip("Kıvılcım hızı")]
    public float sparkSpeed = 5f;
    
    [Tooltip("Kıvılcım ömrü")]
    public float sparkLifetime = 0.3f;
    
    [Header("=== PATLAMA ===")]
    [Tooltip("Patlama ışık şiddeti")]
    public float explosionIntensity = 10f;
    
    [Tooltip("Patlama ışık menzili")]
    public float explosionRange = 15f;
    
    [Tooltip("Bitiş noktasında patlama")]
    public bool endExplosion = true;
    
    [Header("=== IŞIK SİSTEMİ ===")]
    [Tooltip("Işık şiddeti")]
    public float lightIntensity = 5f;
    
    [Tooltip("Işık menzili")]
    public float lightRange = 8f;
    
    [Tooltip("Işık sayısı")]
    public int lightCount = 8;
    
    [Header("=== SES ===")]
    [Tooltip("Charge sesi")]
    public AudioClip chargeSound;
    
    [Tooltip("Ateşleme sesi")]
    public AudioClip fireSound;
    
    [Tooltip("Ses kaynağı")]
    public AudioSource audioSource;
    
    [Header("=== HAPTİK ===")]
    [Tooltip("Charge titreşim şiddeti")]
    public float chargeHaptic = 0.3f;
    
    [Tooltip("Ateşleme titreşim şiddeti")]
    public float fireHaptic = 1f;
    
    [Header("=== CHARGE UI ===")]
    [Tooltip("Charge UI'ı göster")]
    public bool showChargeUI = true;
    
    [Tooltip("UI boyutu")]
    public float uiSize = 0.06f;
    
    [Tooltip("UI offset (X=sağ, Y=yukarı, Z=ileri)")]
    public Vector3 uiOffset = new Vector3(0f, 0.08f, -0.25f);
    
    [Tooltip("Ana renk")]
    public Color uiColor = new Color(0.4f, 0.7f, 1f, 1f);
    
    [Tooltip("Arka plan rengi")]
    public Color uiBgColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    
    [Tooltip("Arc kalınlığı (0-1)")]
    [Range(0.05f, 0.5f)]
    public float uiArcThickness = 0.15f;
    
    [Header("=== LAYER ===")]
    public LayerMask raycastLayerMask = ~0;
    
    [Header("=== LASER UI ===")]
    [Tooltip("Laser kullanım sayısını gösteren text (örn: 2/2)")]
    public TextMeshProUGUI laserCountText;
    
    [Tooltip("Text'in normal rengi")]
    public Color textNormalColor = Color.white;
    
    [Tooltip("Text'in kırmızı rengi (0/2 olduğunda)")]
    public Color textEmptyColor = Color.red;
    
    // Private değişkenler
    private int remainingShots;
    private GunFire gunFire;
    private bool isCharging = false;
    private float currentCharge = 0f;
    private GameObject chargeEffectContainer;
    private List<LineRenderer> chargeBolts = new List<LineRenderer>();
    private Light chargeLight;
    private Coroutine chargeCoroutine;
    
    // UI değişkenleri
    private GameObject chargeUIContainer;
    private Canvas chargeCanvas;
    private Image backgroundImage;
    private Image fillImage;
    private Image glowImage;
    private Image outerHaloImage;
    private Image progressDotImage;
    private RectTransform progressDotRect;
    private Image minChargeMarkerImage;
    
    private bool _tutorialUnlimitedApplied;

    void Start()
    {
        gunFire = GetComponent<GunFire>();
        if (maxLaserShots != 2) maxLaserShots = 2;
        remainingShots = maxLaserShots;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (laserSpawnPoint == null)
        {
            Transform laserChild = transform.Find("Laser");
            if (laserChild != null)
                laserSpawnPoint = laserChild;
        }
        UpdateLaserCountUI();
    }
    
    void Update()
    {
        if (TutorialIntroController.TutorialFifthWeaponPhase && !_tutorialUnlimitedApplied)
        {
            _tutorialUnlimitedApplied = true;
            maxLaserShots = 0;
            remainingShots = 999;
            showChargeUI = false;
            if (laserCountText != null) laserCountText.gameObject.SetActive(false);
        }
        if (maxLaserShots > 0 && remainingShots <= 0) return;
        
        if (!TutorialIntroController.TutorialCompleteFreehand && TutorialIntroController.TutorialFifthWeaponPhase && !TutorialIntroController.TutorialFifthWeaponSecondaryEnabled)
            return; // 13. diyalog bitmeden ikincil (yıldırım) kapalı
        
        // A tuşu basılı tutulunca CHARGE
        bool buttonHeld = OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three);
        bool buttonUp = OVRInput.GetUp(OVRInput.Button.One) || OVRInput.GetUp(OVRInput.Button.Three);
        
        if (buttonHeld && !isCharging)
        {
            StartCharging();
        }
        else if (buttonUp && isCharging)
        {
            ReleaseCharge();
        }
    }
    
    void StartCharging()
    {
        // Laser bitti mi kontrol et
        if (maxLaserShots > 0 && remainingShots <= 0)
        {
            return; // Laser bitti, charge başlatma
        }
        
        isCharging = true;
        currentCharge = 0f;
        
        // Charge efekt container
        chargeEffectContainer = new GameObject("ChargeEffect");
        chargeEffectContainer.transform.position = laserSpawnPoint.position;
        
        // Charge ışığı
        GameObject lightObj = new GameObject("ChargeLight");
        lightObj.transform.SetParent(chargeEffectContainer.transform);
        lightObj.transform.position = laserSpawnPoint.position;
        chargeLight = lightObj.AddComponent<Light>();
        chargeLight.type = LightType.Point;
        chargeLight.color = lightningColor;
        chargeLight.intensity = 0f;
        chargeLight.range = lightRange;

        UICameraStackSetup.SetLayerRecursivelyToUI(chargeEffectContainer);
        
        // Charge UI oluştur
        if (showChargeUI)
        {
            CreateChargeUI();
        }
        
        // Charge coroutine başlat
        chargeCoroutine = StartCoroutine(ChargeRoutine());
        
        // Charge sesi
        if (audioSource != null && chargeSound != null)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    IEnumerator ChargeRoutine()
    {
        float flickerTimer = 0f;
        
        while (isCharging && currentCharge < 1f)
        {
            currentCharge += Time.deltaTime / chargeDuration;
            currentCharge = Mathf.Clamp01(currentCharge);
            flickerTimer += Time.deltaTime;
            
            // Charge efektlerini güncelle
            UpdateChargeEffect(flickerTimer);
            
            // Haptic feedback
            if (chargeVibration)
            {
                OVRInput.Controller controller = (gunFire != null && gunFire.isLeftHanded) 
                    ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                OVRInput.SetControllerVibration(1f, chargeHaptic * currentCharge, controller);
            }
            
            yield return null;
        }
        
        // Tam charge oldu - sürekli titreşim
        while (isCharging)
        {
            flickerTimer += Time.deltaTime;
            UpdateChargeEffect(flickerTimer);
            
            // Güçlü haptic
            if (chargeVibration)
            {
                OVRInput.Controller controller = (gunFire != null && gunFire.isLeftHanded) 
                    ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                float pulse = 0.5f + Mathf.Sin(Time.time * 20f) * 0.5f;
                OVRInput.SetControllerVibration(1f, chargeHaptic * pulse, controller);
            }
            
            yield return null;
        }
    }
    
    void UpdateChargeEffect(float timer)
    {
        if (chargeEffectContainer == null) return;
        
        Vector3 spawnPos = laserSpawnPoint.position;
        Vector3 forward = laserSpawnPoint.forward;
        
        // Işık güncelle
        if (chargeLight != null)
        {
            float flicker = 0.8f + Random.Range(0f, 0.4f);
            chargeLight.intensity = lightIntensity * currentCharge * 2f * flicker;
            chargeLight.range = lightRange * (0.5f + currentCharge * 0.5f);
            chargeLight.transform.position = spawnPos;
        }
        
        // UI güncelle
        if (showChargeUI)
        {
            UpdateChargeUI();
        }
        
        // Kıvılcımlar oluştur
        if (timer % 0.05f < Time.deltaTime && currentCharge > 0.1f)
        {
            int sparkCount = Mathf.RoundToInt(chargeSparks * currentCharge);
            for (int i = 0; i < Mathf.Min(3, sparkCount); i++)
            {
                CreateSpark(spawnPos, currentCharge * 0.5f);
            }
        }
        
        // Mini yıldırımlar (charge sırasında silah etrafında)
        if (timer % (1f / (flickerSpeed * 0.5f)) < Time.deltaTime)
        {
            UpdateChargeBolts(spawnPos, forward, currentCharge);
        }
    }
    
    void UpdateChargeBolts(Vector3 origin, Vector3 forward, float charge)
    {
        // Eski boltları temizle
        foreach (var bolt in chargeBolts)
        {
            if (bolt != null) Destroy(bolt.gameObject);
        }
        chargeBolts.Clear();
        
        if (charge < 0.15f) return;
        
        // Yeni mini boltlar oluştur
        int boltCount = Mathf.RoundToInt(4 * charge);
        float boltLength = 0.2f + charge * 0.5f;
        
        for (int i = 0; i < boltCount; i++)
        {
            Vector3 randomDir = Random.onUnitSphere;
            // Forward yönüne doğru bias (charge arttıkça daha fazla)
            randomDir = Vector3.Lerp(randomDir, forward, 0.2f + charge * 0.3f).normalized;
            
            Vector3 endPos = origin + randomDir * boltLength;

            // Saçaklar eski tarz: (Glow + Core)
            Vector3[] points = GenerateSmoothLightningPoints(origin, endPos, 6, jaggedAmount * 0.4f);

            // Glow layer (inner glow rengi)
            LineRenderer glowBolt = CreateSmoothBoltLine(chargeEffectContainer.transform, coreWidth * 4f, innerGlowColor, 3);
            glowBolt.positionCount = points.Length;
            glowBolt.SetPositions(points);
            chargeBolts.Add(glowBolt);

            // Core layer (beyaz çekirdek)
            LineRenderer coreBolt = CreateSmoothBoltLine(chargeEffectContainer.transform, coreWidth * 1.5f, coreColor, 3);
            coreBolt.positionCount = points.Length;
            coreBolt.SetPositions(points);
            chargeBolts.Add(coreBolt);
        }
    }
    
    void ReleaseCharge()
    {
        isCharging = false;
        
        // Haptic durdur
        OVRInput.Controller controller = (gunFire != null && gunFire.isLeftHanded) 
            ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(0f, 0f, controller);
        
        // Ses durdur
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        
        // Yeterli charge var mı?
        if (currentCharge >= minChargeToFire)
        {
            FireLightning(currentCharge);
        }
        
        // Charge efektlerini temizle
        CleanupChargeEffects();
    }
    
    void CleanupChargeEffects()
    {
        if (chargeEffectContainer != null)
        {
            Destroy(chargeEffectContainer);
        }
        chargeBolts.Clear();
        chargeLight = null;
        
        // UI temizle
        DestroyChargeUI();
        
        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }
    
    void FireLightning(float chargeAmount)
    {
        if (laserSpawnPoint == null) return;
        
        if (maxLaserShots > 0)
        {
            remainingShots--;
            Debug.Log($"⚡ YILDIRIM! Charge: {chargeAmount:P0} | Kalan: {remainingShots}");
            UpdateLaserCountUI();
        }
        
        // BÜYÜK HAPTİK
        StartCoroutine(FireHapticFeedback());
        
        // Ateşleme sesi
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // Raycast - düşmanlara hasar
        Vector3 origin = laserSpawnPoint.position;
        Vector3 direction = laserSpawnPoint.forward;
        float damage = laserDamage * chargeAmount; // Charge'a göre hasar
        
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, laserRange, raycastLayerMask);
        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
        
        foreach (RaycastHit hit in hits)
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                enemy.TakeDamage(damage, hit.collider, fromFlameSpray: false, fromSecondary: true, tutorialWeaponIndex: 4);
            }
        }
        
        // YILDIRIM GÖRSELİ
        StartCoroutine(LightningVisualRoutine(origin, direction, laserRange, chargeAmount));
    }
    
    IEnumerator FireHapticFeedback()
    {
        OVRInput.Controller controller = (gunFire != null && gunFire.isLeftHanded) 
            ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        
        // PATLAMA hissi
        OVRInput.SetControllerVibration(1f, fireHaptic, controller);
        yield return new WaitForSeconds(0.15f);
        OVRInput.SetControllerVibration(0.5f, fireHaptic * 0.7f, controller);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0.3f, fireHaptic * 0.4f, controller);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }
    
    IEnumerator LightningVisualRoutine(Vector3 origin, Vector3 direction, float distance, float power)
    {
        GameObject container = new GameObject("LightningBlast");
        Vector3 endPoint = origin + direction * distance;
        
        // ==================== SAÇAK STİLİ YILDIRIM (CHARGE İLE AYNI) ====================
        // Yani: Glow(innerGlowColor) + Core(coreColor)
        List<LineRenderer> mainGlowBolts = new List<LineRenderer>();
        List<LineRenderer> mainCoreBolts = new List<LineRenderer>();
        List<LineRenderer> branchGlowBolts = new List<LineRenderer>();
        List<LineRenderer> branchCoreBolts = new List<LineRenderer>();

        for (int i = 0; i < mainBoltCount; i++)
        {
            LineRenderer glow = CreateSmoothBoltLine(container.transform, coreWidth * 4f, innerGlowColor, 4);
            LineRenderer core = CreateSmoothBoltLine(container.transform, coreWidth * 1.6f, coreColor, 3);
            mainGlowBolts.Add(glow);
            mainCoreBolts.Add(core);
        }

        for (int i = 0; i < branchCount * mainBoltCount; i++)
        {
            LineRenderer glow = CreateSmoothBoltLine(container.transform, coreWidth * 2.5f, innerGlowColor, 3);
            LineRenderer core = CreateSmoothBoltLine(container.transform, coreWidth * 1.0f, coreColor, 3);
            branchGlowBolts.Add(glow);
            branchCoreBolts.Add(core);
        }
        
        // ==================== IŞIKLAR ====================
        List<Light> lights = new List<Light>();
        
        // Başlangıç patlaması
        Light startFlash = CreateLight(container.transform, origin, explosionIntensity * power, explosionRange);
        lights.Add(startFlash);
        
        // Yol boyunca ışıklar
        for (int i = 0; i < lightCount; i++)
        {
            float t = (float)(i + 1) / (lightCount + 1);
            Vector3 pos = Vector3.Lerp(origin, endPoint, t);
            Light light = CreateLight(container.transform, pos, lightIntensity * power, lightRange);
            lights.Add(light);
        }
        
        // Bitiş patlaması
        if (endExplosion)
        {
            Light endFlash = CreateLight(container.transform, endPoint, explosionIntensity * power * 1.5f, explosionRange * 1.5f);
            lights.Add(endFlash);
        }
        
        // ==================== KIVILCIMLAR ====================
        // Başlangıç kıvılcımları
        for (int i = 0; i < fireSparks; i++)
        {
            CreateSpark(origin, power, container.transform);
        }

        UICameraStackSetup.SetLayerRecursivelyToUI(container);
        
        // ==================== ANİMASYON ====================
        float duration = 0.5f + power * 0.5f; // Charge'a göre süre
        float elapsed = 0f;
        float flickerTimer = 0f;
        
        // İLK PATLAMA - çok hızlı uzama
        float extendTime = 0.05f;
        while (elapsed < extendTime)
        {
            elapsed += Time.deltaTime;
            flickerTimer += Time.deltaTime;
            
            float t = elapsed / extendTime;
            float easedT = 1f - Mathf.Pow(1f - t, 4f); // Çok hızlı ease out
            
            Vector3 currentEnd = Vector3.Lerp(origin, endPoint, easedT);
            float currentDist = distance * easedT;
            
            // Yıldırımları güncelle (saçak stili)
            UpdateAllBoltsFringeStyle(mainGlowBolts, mainCoreBolts, branchGlowBolts, branchCoreBolts, origin, currentEnd, currentDist, power);
            
            // Işık titreşimi
            foreach (var light in lights)
            {
                if (light != null)
                {
                    float baseIntensity = light == startFlash || light == lights[lights.Count - 1] 
                        ? explosionIntensity : lightIntensity;
                    light.intensity = baseIntensity * power * (0.8f + Random.Range(0f, 0.4f)) * t;
                }
            }
            
            yield return null;
        }
        
        // Bitiş kıvılcımları
        if (endExplosion)
        {
            for (int i = 0; i < fireSparks * 2; i++)
            {
                CreateSpark(endPoint, power * 1.5f, container.transform);
            }
        }
        
        // YILDIRIM TİTREŞİMİ
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flickerTimer += Time.deltaTime;
            
            float fadeT = elapsed / duration;
            float intensityMult = 1f - (fadeT * fadeT); // Yavaş sönen
            
            // Sürekli titreşim
            if (flickerTimer >= 1f / flickerSpeed)
            {
                flickerTimer = 0f;
                UpdateAllBoltsFringeStyle(mainGlowBolts, mainCoreBolts, branchGlowBolts, branchCoreBolts, origin, endPoint, distance, power * intensityMult);
                
                // Rastgele kıvılcımlar
                if (Random.value < 0.3f * intensityMult)
                {
                    Vector3 sparkPos = Vector3.Lerp(origin, endPoint, Random.value);
                    CreateSpark(sparkPos, power * 0.5f * intensityMult, container.transform);
                }
            }
            
            // Işık titreşimi
            foreach (var light in lights)
            {
                if (light != null)
                {
                    float baseIntensity = (light == startFlash || (endExplosion && light == lights[lights.Count - 1]))
                        ? explosionIntensity : lightIntensity;
                    light.intensity = baseIntensity * power * intensityMult * (0.7f + Random.Range(0f, 0.6f));
                }
            }
            
            // Kalınlık smooth azalma (saçak stili)
            float widthMult = Mathf.SmoothStep(1f, 0f, fadeT);
            foreach (var g in mainGlowBolts) if (g != null) SetLineWidth(g, coreWidth * 4f * widthMult);
            foreach (var c in mainCoreBolts) if (c != null) SetLineWidth(c, coreWidth * 1.6f * widthMult);
            foreach (var g in branchGlowBolts) if (g != null) SetLineWidth(g, coreWidth * 2.5f * widthMult);
            foreach (var c in branchCoreBolts) if (c != null) SetLineWidth(c, coreWidth * 1.0f * widthMult);
            
            yield return null;
        }
        
        // Temizlik
        Destroy(container);
    }
    
    void SetLineWidth(LineRenderer line, float width)
    {
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(0.5f, 1.1f); // Ortada biraz şişkin
        curve.AddKey(1f, 0.7f);   // Uçta incelme
        line.widthCurve = curve;
        line.widthMultiplier = width;
    }
    
    // İleri giden yıldırım: charge saçak stili (Glow + Core)
    void UpdateAllBoltsFringeStyle(
        List<LineRenderer> mainGlows,
        List<LineRenderer> mainCores,
        List<LineRenderer> branchGlows,
        List<LineRenderer> branchCores,
        Vector3 start,
        Vector3 end,
        float dist,
        float power)
    {
        if (dist < 0.1f) return;

        Vector3 dir = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.up).normalized;
        if (perpendicular.magnitude < 0.1f) perpendicular = Vector3.Cross(dir, Vector3.right).normalized;

        // Ana boltlar
        for (int i = 0; i < mainCores.Count; i++)
        {
            float offsetAmount = (i - (mainCores.Count - 1) / 2f) * jaggedAmount * 0.25f;
            Vector3 offsetStart = start + perpendicular * offsetAmount;

            Vector3[] points = GenerateSmoothLightningPoints(offsetStart, end, segments, jaggedAmount * power);

            if (i < mainGlows.Count && mainGlows[i] != null)
            {
                mainGlows[i].positionCount = points.Length;
                mainGlows[i].SetPositions(points);
            }
            if (i < mainCores.Count && mainCores[i] != null)
            {
                mainCores[i].positionCount = points.Length;
                mainCores[i].SetPositions(points);
            }

            // Yan dallar (glow+core)
            int branchesPerMain = branchCount;
            for (int b = 0; b < branchesPerMain; b++)
            {
                int idx = i * branchesPerMain + b;
                if (idx >= branchCores.Count || idx >= branchGlows.Count) continue;

                float branchT = 0.25f + (float)b / Mathf.Max(1, branchesPerMain - 1) * 0.55f;
                int branchPoint = Mathf.Clamp(Mathf.RoundToInt(branchT * (points.Length - 1)), 2, points.Length - 2);
                Vector3 branchStart = points[branchPoint];

                float angle = (b % 2 == 0 ? 1f : -1f) * (35f + Random.Range(0f, 35f));
                Vector3 branchDir = Quaternion.AngleAxis(angle, Vector3.up) *
                                    Quaternion.AngleAxis(Random.Range(-25f, 25f), perpendicular) * dir;

                float branchDist = dist * branchLength * (0.55f + Random.value * 0.45f);
                Vector3 branchEnd = branchStart + branchDir * branchDist;

                Vector3[] branchPoints = GenerateSmoothLightningPoints(branchStart, branchEnd, 8, jaggedAmount * 0.55f);

                if (branchGlows[idx] != null)
                {
                    branchGlows[idx].positionCount = branchPoints.Length;
                    branchGlows[idx].SetPositions(branchPoints);
                }
                if (branchCores[idx] != null)
                {
                    branchCores[idx].positionCount = branchPoints.Length;
                    branchCores[idx].SetPositions(branchPoints);
                }
            }
        }
    }
    
    LineRenderer CreateSmoothBoltLine(Transform parent, float width, Color color, int quality)
    {
        GameObject obj = new GameObject("Bolt");
        obj.transform.SetParent(parent);
        
        LineRenderer line = obj.AddComponent<LineRenderer>();
        
        // Additive shader için daha iyi görünüm
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;
        
        // Smooth width curve
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);
        widthCurve.AddKey(0.1f, 1f);
        widthCurve.AddKey(0.5f, 1.05f);
        widthCurve.AddKey(0.9f, 0.9f);
        widthCurve.AddKey(1f, 0.5f);
        line.widthCurve = widthCurve;
        line.widthMultiplier = width;
        
        line.positionCount = 0;
        line.useWorldSpace = true;
        line.numCapVertices = quality;
        line.numCornerVertices = quality;
        line.textureMode = LineTextureMode.Stretch;
        
        // Smooth gradient
        Gradient grad = new Gradient();
        Color brightColor = Color.Lerp(color, Color.white, 0.5f);
        Color midColor = color;
        
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),
                new GradientColorKey(midColor, 0.15f),
                new GradientColorKey(midColor, 0.85f),
                new GradientColorKey(brightColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(color.a * 0.9f, 0f),
                new GradientAlphaKey(color.a, 0.2f),
                new GradientAlphaKey(color.a, 0.8f),
                new GradientAlphaKey(color.a * 0.7f, 1f)
            }
        );
        line.colorGradient = grad;
        
        return line;
    }
    
    Light CreateLight(Transform parent, Vector3 position, float intensity, float range)
    {
        GameObject obj = new GameObject("Light");
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = lightningColor;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
        
        return light;
    }
    
    void CreateSpark(Vector3 position, float power, Transform parent = null)
    {
        GameObject spark = new GameObject("Spark");
        if (parent != null) spark.transform.SetParent(parent);
        spark.transform.position = position;
        
        // Mini zigzag kıvılcım
        LineRenderer line = spark.AddComponent<LineRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = coreColor;
        line.material = mat;
        
        // Smooth width
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(0.3f, 0.7f);
        widthCurve.AddKey(1f, 0f);
        line.widthCurve = widthCurve;
        line.widthMultiplier = 0.015f * power;
        
        line.numCapVertices = 3;
        line.numCornerVertices = 2;
        line.useWorldSpace = true;
        
        // Rastgele yön
        Vector3 dir = Random.onUnitSphere;
        float length = 0.08f + Random.value * 0.15f * power;
        
        // Mini zigzag (3-5 nokta)
        int pointCount = Random.Range(3, 6);
        line.positionCount = pointCount;
        
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        if (perp.magnitude < 0.1f) perp = Vector3.Cross(dir, Vector3.right).normalized;
        
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            Vector3 basePos = position + dir * length * t;
            
            // Küçük zigzag
            if (i > 0 && i < pointCount - 1)
            {
                float offset = Random.Range(-0.02f, 0.02f) * power;
                basePos += perp * offset;
            }
            
            line.SetPosition(i, basePos);
        }
        
        // Gradient
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(coreColor, 0f),
                new GradientColorKey(lightningColor, 0.5f),
                new GradientColorKey(innerGlowColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        line.colorGradient = grad;
        
        // Hareket ve yok olma
        StartCoroutine(AnimateSpark(spark, dir, power, length));
    }
    
    IEnumerator AnimateSpark(GameObject spark, Vector3 direction, float power, float length)
    {
        float life = sparkLifetime * (0.7f + Random.value * 0.6f);
        float elapsed = 0f;
        Vector3 velocity = direction * sparkSpeed * power * 0.7f;
        float gravity = 1.5f;
        
        LineRenderer line = spark.GetComponent<LineRenderer>();
        Vector3[] originalPositions = new Vector3[line.positionCount];
        line.GetPositions(originalPositions);
        
        while (elapsed < life && spark != null && line != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / life;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Hareket (tüm noktalar birlikte)
            Vector3 movement = velocity * elapsed + Vector3.down * gravity * elapsed * elapsed;
            
            for (int i = 0; i < line.positionCount; i++)
            {
                line.SetPosition(i, originalPositions[i] + movement);
            }
            
            // Smooth solma
            float alpha = 1f - smoothT;
            Color c = line.material.color;
            c.a = alpha;
            line.material.color = c;
            
            // Width küçülme
            line.widthMultiplier = 0.015f * power * (1f - smoothT * 0.7f);
            
            yield return null;
        }
        
        if (spark != null) Destroy(spark);
    }
    
    Vector3[] GenerateLightningPoints(Vector3 start, Vector3 end, int segmentCount, float jagged)
    {
        return GenerateSmoothLightningPoints(start, end, segmentCount, jagged);
    }
    
    Vector3[] GenerateSmoothLightningPoints(Vector3 start, Vector3 end, int segmentCount, float jagged)
    {
        Vector3[] rawPoints = new Vector3[segmentCount + 1];
        rawPoints[0] = start;
        rawPoints[segmentCount] = end;
        
        Vector3 dir = (end - start).normalized;
        float totalDist = Vector3.Distance(start, end);
        
        // Perpendicular vektörler
        Vector3 perp1 = Vector3.Cross(dir, Vector3.up).normalized;
        if (perp1.magnitude < 0.1f) perp1 = Vector3.Cross(dir, Vector3.right).normalized;
        Vector3 perp2 = Vector3.Cross(dir, perp1).normalized;
        
        // Displacement midpoint subdivision benzeri yaklaşım
        float displacement = jagged;
        
        for (int i = 1; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 basePoint = Vector3.Lerp(start, end, t);
            
            // Uçlara yaklaştıkça sapma azalsın (smooth falloff)
            float edgeFalloff = Mathf.Sin(t * Mathf.PI); // 0'dan 1'e, tekrar 0'a
            edgeFalloff = Mathf.Pow(edgeFalloff, 0.5f); // Daha geniş orta bölge
            
            // Perlin noise benzeri smooth random
            float noiseX = (Mathf.PerlinNoise(i * 0.3f, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, i * 0.3f) - 0.5f) * 2f;
            
            // Biraz gerçek random da ekle
            noiseX = Mathf.Lerp(noiseX, Random.Range(-1f, 1f), 0.4f);
            noiseY = Mathf.Lerp(noiseY, Random.Range(-1f, 1f), 0.4f);
            
            float offsetX = noiseX * displacement * edgeFalloff;
            float offsetY = noiseY * displacement * edgeFalloff;
            
            rawPoints[i] = basePoint + perp1 * offsetX + perp2 * offsetY;
        }
        
        // Smoothing pass (jaggedSmoothness'a göre)
        if (jaggedSmoothness > 0.01f)
        {
            Vector3[] smoothedPoints = new Vector3[segmentCount + 1];
            smoothedPoints[0] = rawPoints[0];
            smoothedPoints[segmentCount] = rawPoints[segmentCount];
            
            for (int i = 1; i < segmentCount; i++)
            {
                // Komşu noktalarla ortalama al
                Vector3 prev = rawPoints[Mathf.Max(0, i - 1)];
                Vector3 curr = rawPoints[i];
                Vector3 next = rawPoints[Mathf.Min(segmentCount, i + 1)];
                
                Vector3 smoothed = (prev + curr * 2f + next) / 4f;
                smoothedPoints[i] = Vector3.Lerp(curr, smoothed, jaggedSmoothness);
            }
            
            return smoothedPoints;
        }
        
        return rawPoints;
    }
    
    // ==================== PUBLIC METODLAR ====================
    
    public void ResetLaser()
    {
        remainingShots = maxLaserShots;
        UpdateLaserCountUI();
    }
    
    public bool HasLaserBeenFired()
    {
        return maxLaserShots > 0 && remainingShots <= 0;
    }
    
    public int GetRemainingShots()
    {
        return maxLaserShots == 0 ? -1 : remainingShots;
    }
    
    public void AddLaserShots(int amount)
    {
        remainingShots += amount;
        UpdateLaserCountUI();
    }
    
    /// <summary>
    /// Laser kullanım sayısını UI'da günceller ve renk değiştirir
    /// </summary>
    void UpdateLaserCountUI()
    {
        if (laserCountText == null) return;
        
        // maxLaserShots'i 2 olarak garanti et
        int displayMaxShots = maxLaserShots > 0 ? maxLaserShots : 2;
        if (displayMaxShots != 2) displayMaxShots = 2;
        
        // remainingShots'i sınırla
        int displayRemainingShots = Mathf.Clamp(remainingShots, 0, displayMaxShots);
        
        // Text'i güncelle: remainingShots/maxLaserShots (her zaman 2/2 formatında)
        laserCountText.text = $"{displayRemainingShots}/{displayMaxShots}";
        
        // 0/2 olduğunda kırmızı, diğer durumlarda normal renk
        if (displayRemainingShots <= 0)
        {
            laserCountText.color = textEmptyColor;
        }
        else
        {
            laserCountText.color = textNormalColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (laserSpawnPoint != null)
        {
            Gizmos.color = lightningColor;
            Gizmos.DrawRay(laserSpawnPoint.position, laserSpawnPoint.forward * laserRange);
            Gizmos.DrawWireSphere(laserSpawnPoint.position, 0.1f);
        }
    }
    
    // ==================== CHARGE UI (MİNİMAL) ====================
    
    void CreateChargeUI()
    {
        // Ana container - silaha parent'la
        chargeUIContainer = new GameObject("ChargeUI");
        chargeUIContainer.transform.SetParent(transform); // Silahın child'ı
        chargeUIContainer.transform.localPosition = uiOffset;
        chargeUIContainer.transform.localRotation = Quaternion.identity;
        
        // World Space Canvas
        chargeCanvas = chargeUIContainer.AddComponent<Canvas>();
        chargeCanvas.renderMode = RenderMode.WorldSpace;
        UICameraStackSetup.Instance?.RegisterWorldSpaceCanvas(chargeCanvas);
        
        RectTransform canvasRect = chargeCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 2);
        canvasRect.localScale = Vector3.one * uiSize * 0.01f;
        
        // Minimal tasarım - sadece ince arc
        
        // 1. Arka plan arc (koyu, tam daire)
        GameObject bgArc = CreateArcElement("BgArc", chargeUIContainer.transform, 100);
        backgroundImage = bgArc.GetComponent<Image>();
        backgroundImage.color = uiBgColor;
        backgroundImage.fillAmount = 1f;
        
        // 2. Progress arc (parlak, dolan)
        GameObject progressArc = CreateArcElement("ProgressArc", chargeUIContainer.transform, 100);
        fillImage = progressArc.GetComponent<Image>();
        fillImage.color = uiColor;
        fillImage.fillAmount = 0f;
        
        // 3. Glow (daha cafcaflı)
        GameObject glowArc = CreateArcElement("GlowArc", chargeUIContainer.transform, 118);
        glowImage = glowArc.GetComponent<Image>();
        glowImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0f);
        glowImage.fillAmount = 0f;
        RectTransform glowRect = glowArc.GetComponent<RectTransform>();
        glowRect.SetAsFirstSibling();

        // 4. Outer halo (tam daire, çok yumuşak)
        GameObject halo = CreateArcElement("OuterHalo", chargeUIContainer.transform, 130);
        outerHaloImage = halo.GetComponent<Image>();
        outerHaloImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.08f);
        outerHaloImage.fillAmount = 1f;
        RectTransform haloRect = halo.GetComponent<RectTransform>();
        haloRect.SetAsFirstSibling();

        // 5. Minimum charge marker (ince işaret)
        GameObject marker = CreateArcElement("MinChargeMarker", chargeUIContainer.transform, 106);
        minChargeMarkerImage = marker.GetComponent<Image>();
        minChargeMarkerImage.color = new Color(1f, 1f, 1f, 0.25f);
        minChargeMarkerImage.fillAmount = Mathf.Clamp01(minChargeToFire);
        // marker sadece küçük bir yay olsun: fillAmount'ı küçük tutup, rotasyonla konumlandır
        // Bunu UpdateChargeUI'da angle ile yapacağız; şimdilik 0.02 gibi küçük bir arc
        minChargeMarkerImage.fillAmount = 0.02f;

        // 6. Progress dot (dolum ucunda parlayan nokta)
        GameObject dot = new GameObject("ProgressDot");
        dot.transform.SetParent(chargeUIContainer.transform, false);
        progressDotImage = dot.AddComponent<Image>();
        progressDotImage.sprite = CreateDotSprite(64);
        progressDotImage.color = new Color(1f, 1f, 1f, 0f);
        progressDotImage.raycastTarget = false;
        progressDotRect = dot.GetComponent<RectTransform>();
        progressDotRect.sizeDelta = new Vector2(10, 10);
        progressDotRect.anchoredPosition = Vector2.zero;
    }
    
    GameObject CreateArcElement(string name, Transform parent, float size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        Image image = obj.AddComponent<Image>();
        image.sprite = CreateRingSprite(128, uiArcThickness);
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = (int)Image.Origin360.Top;
        image.fillClockwise = true;
        image.raycastTarget = false;
        image.preserveAspect = true;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = Vector2.zero;
        
        return obj;
    }
    
    Sprite CreateRingSprite(int resolution, float thickness)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        
        float center = resolution / 2f;
        float outerRadius = resolution / 2f - 2f;
        float innerRadius = outerRadius * (1f - thickness);
        
        Color transparent = new Color(0, 0, 0, 0);
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                // Ring alanında mı?
                if (dist >= innerRadius && dist <= outerRadius)
                {
                    // Kenar yumuşatma
                    float alpha = 1f;
                    
                    // Dış kenar AA
                    if (dist > outerRadius - 1.5f)
                    {
                        alpha = Mathf.Clamp01((outerRadius - dist) / 1.5f);
                    }
                    // İç kenar AA
                    else if (dist < innerRadius + 1.5f)
                    {
                        alpha = Mathf.Clamp01((dist - innerRadius) / 1.5f);
                    }
                    
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
        }
        
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
    }
    
    void UpdateChargeUI()
    {
        if (chargeUIContainer == null || fillImage == null) return;
        
        // Billboard - her zaman kameraya baksın
        if (Camera.main != null)
        {
            Vector3 lookDir = chargeUIContainer.transform.position - Camera.main.transform.position;
            if (lookDir != Vector3.zero)
            {
                chargeUIContainer.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        
        // Fill amount - smooth
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, currentCharge, Time.deltaTime * 15f);
        
        // Renk - charge arttıkça parlak
        float intensity = 0.8f + currentCharge * 0.4f;
        Color currentColor = new Color(
            uiColor.r * intensity,
            uiColor.g * intensity,
            uiColor.b * intensity,
            uiColor.a
        );
        fillImage.color = currentColor;
        
        // Glow - sadece yüksek charge'da
        if (glowImage != null)
        {
            glowImage.fillAmount = fillImage.fillAmount;
            
            float glowAlpha = 0f;
            if (currentCharge > 0.5f)
            {
                glowAlpha = (currentCharge - 0.5f) * 0.6f;
            }
            
            // Full charge pulse
            if (currentCharge >= 0.99f)
            {
                glowAlpha = 0.25f + Mathf.Sin(Time.time * 12f) * 0.15f;
                
                // Boyut pulse
                float scale = 1.1f + Mathf.Sin(Time.time * 12f) * 0.05f;
                glowImage.rectTransform.localScale = Vector3.one * scale;
            }
            else
            {
                glowImage.rectTransform.localScale = Vector3.one;
            }
            
            glowImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, glowAlpha);
        }

        // Outer halo - charge ile hafif güçlensin
        if (outerHaloImage != null)
        {
            float haloPulse = 0.06f + currentCharge * 0.14f;
            if (currentCharge >= 0.99f)
            {
                haloPulse = 0.15f + Mathf.Sin(Time.time * 10f) * 0.05f;
            }
            outerHaloImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, haloPulse);
        }

        // Progress dot - dolum ucunda parlayan ve minik pulse
        if (progressDotImage != null && progressDotRect != null)
        {
            float fill = Mathf.Clamp01(fillImage.fillAmount);
            float angleDeg = 90f - fill * 360f; // Top origin + clockwise
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // Radius (UI elemanının yarıçapı)
            float radius = 50f; // 100 size / 2
            Vector2 pos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            progressDotRect.anchoredPosition = pos;

            float dotAlpha = Mathf.Clamp01(currentCharge) * 0.9f;
            if (currentCharge >= 0.99f) dotAlpha = 0.8f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotImage.color = new Color(1f, 1f, 1f, dotAlpha);

            float dotScale = 1f + currentCharge * 0.6f;
            if (currentCharge >= 0.99f) dotScale = 1.6f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotRect.localScale = Vector3.one * dotScale;
        }

        // Min charge marker - sabit konum (arc olarak değil, çizgi gibi)
        if (minChargeMarkerImage != null)
        {
            // marker görüntüsünü küçük arc olarak kullanıp doğru açıya yerleştiriyoruz
            float markerAngleDeg = 90f - Mathf.Clamp01(minChargeToFire) * 360f;
            minChargeMarkerImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, markerAngleDeg);
            float markerAlpha = currentCharge >= minChargeToFire ? 0.55f : 0.25f;
            minChargeMarkerImage.color = new Color(1f, 1f, 1f, markerAlpha);
        }
        
        // Background - min charge geçince hafif parla
        if (backgroundImage != null)
        {
            Color bgCol = uiBgColor;
            if (currentCharge >= minChargeToFire)
            {
                float t = (currentCharge - minChargeToFire) / (1f - minChargeToFire);
                bgCol = Color.Lerp(uiBgColor, new Color(uiColor.r * 0.3f, uiColor.g * 0.3f, uiColor.b * 0.3f, uiBgColor.a), t * 0.5f);
            }
            backgroundImage.color = bgCol;
        }
    }
    
    void DestroyChargeUI()
    {
        if (chargeUIContainer != null)
        {
            Destroy(chargeUIContainer);
            chargeUIContainer = null;
        }
        chargeCanvas = null;
        backgroundImage = null;
        fillImage = null;
        glowImage = null;
        outerHaloImage = null;
        progressDotImage = null;
        progressDotRect = null;
        minChargeMarkerImage = null;
    }

    Sprite CreateDotSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        float center = resolution / 2f;
        float radius = resolution / 2f - 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float a = Mathf.Clamp01((radius - dist) / 2.0f);
                if (dist <= radius)
                {
                    // yumuşak, parlak çekirdek
                    float core = Mathf.Clamp01((radius * 0.35f - dist) / (radius * 0.35f));
                    float alpha = Mathf.Clamp01(a + core * 0.6f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
    }
}
