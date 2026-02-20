using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SixthGun için şık ray laser efekti.
/// B tuşu ile açılıp kapanır, düşmanları yavaşlatır ve buzlandırır.
/// </summary>
public class SixthGunLaser : MonoBehaviour
{
    [Header("=== LASER AYARLARI ===")]
    [Tooltip("Laser çıkış noktası (sağ el / ana silah)")]
    public Transform laserSpawnPoint;
    
    [Tooltip("İkinci laser çıkış noktası (sol el silahı). Boşsa tek laser, doluysa iki laser karşıya doğru gider.")]
    public Transform laserSpawnPoint2;
    
    
    [Tooltip("Laser menzili")]
    public float laserRange = 100f;
    
    [Header("=== SLOW AYARLARI ===")]
    [Tooltip("Slow multiplier (0.5 = %50 yavaş)")]
    [Range(0f, 1f)]
    public float slowMultiplier = 0.4f; // %60 yavaşlatma
    
    [Tooltip("Slow tick rate (saniyede kaç kez kontrol)")]
    public float slowTickRate = 10f; // Saniyede 10 kez
    
    [Header("=== CAN AZALMA (HASAR) ===")]
    [Tooltip("Laser düşmana değerken her tick'te verilen hasar (tur1-tur4 tüm düşmanlara uygulanır)")]
    public float damagePerTick = 3f;
    
    [Tooltip("Raycast layer mask")]
    public LayerMask raycastLayerMask = -1; // Tüm layer'lar
    
    [Header("=== LASER GÖRSELİ ===")]
    [Tooltip("Ana laser rengi")]
    public Color laserColor = new Color(0f, 0.7f, 1f, 1f); // Parlak mavi
    
    [Tooltip("Glow rengi")]
    public Color glowColor = new Color(0f, 0.4f, 1f, 0.4f); // Koyu mavi glow
    
    [Tooltip("Core rengi (parlak çekirdek)")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f); // Beyaz
    
    [Tooltip("Ana laser kalınlığı")]
    public float laserWidth = 0.02f;
    
    [Tooltip("Glow kalınlığı")]
    public float glowWidth = 0.08f;
    
    [Tooltip("Core kalınlığı")]
    public float coreWidth = 0.008f;
    
    [Header("=== IŞIK EFEKTİ ===")]
    [Tooltip("Işık efekti aktif mi?")]
    public bool enableLight = true;
    
    [Tooltip("Işık şiddeti")]
    public float lightIntensity = 3f;
    
    [Tooltip("Işık menzili")]
    public float lightRange = 5f;
    
    [Tooltip("Işık rengi")]
    public Color lightColor = new Color(0f, 0.98f, 1f, 1f);
    
    [Header("=== ENERJİ BAR SİSTEMİ ===")]
    [Tooltip("Enerji bar göster")]
    public bool showEnergyBar = true;
    
    [Tooltip("Maksimum enerji (saniye cinsinden)")]
    public float maxEnergy = 10f;
    
    [Tooltip("Laser açıkken azalma hızı (saniyede)")]
    public float drainRate = 1f; // Saniyede 1 birim azalır
    
    [Tooltip("Düşmana değdirince ek azalma hızı (saniyede)")]
    public float drainRateOnHit = 2f; // Düşmana değdirince ekstra 2 birim/saniye
    
    [Tooltip("Laser kapalıyken dolma hızı (saniyede)")]
    public float rechargeRate = 2f; // Saniyede 2 birim dolar
    
    [Tooltip("UI boyutu")]
    public float uiSize = 0.06f;
    
    [Tooltip("UI offset (X=sağ, Y=yukarı, Z=ileri)")]
    public Vector3 uiOffset = new Vector3(0f, 0.08f, -0.25f);
    
    [Tooltip("Ana renk")]
    public Color uiColor = new Color(0f, 0.7f, 1f, 1f); // Mavi
    
    [Tooltip("Arka plan rengi")]
    public Color uiBgColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    
    [Tooltip("Arc kalınlığı (0-1)")]
    [Range(0.05f, 0.5f)]
    public float uiArcThickness = 0.15f;
    
    // Private değişkenler
    private LineRenderer laserMain;
    private LineRenderer laserGlow;
    private LineRenderer laserCore;
    private LineRenderer laserLeftGlow, laserLeftMain, laserLeftCore;
    private LineRenderer laserRightGlow, laserRightMain, laserRightCore;
    private Light laserLight;
    private bool isLaserActive = false;
    private GameObject laserContainer;
    private bool useDualLaser = false;
    
    // Slow sistemi
    private HashSet<EnemyBehavior> slowedEnemies = new HashSet<EnemyBehavior>();
    private float slowTickTimer = 0f;
    
    // Enerji bar sistemi
    private float currentEnergy;
    private GameObject energyBarContainer;
    private Canvas energyBarCanvas;
    private Image backgroundImage;
    private Image fillImage;
    private Image glowImage;
    private Image outerHaloImage;
    private Image progressDotImage;
    private RectTransform progressDotRect;
    private bool isHittingEnemy = false;

    private void Awake()
    {
        // Laser spawn point'i bul
        if (laserSpawnPoint == null)
        {
            Transform laserChild = transform.Find("Laser");
            if (laserChild != null)
                laserSpawnPoint = laserChild;
        }
        
        // İkinci spawn point: Inspector'da atanmamışsa WeaponManager'dan sol el silahını dene
        if (laserSpawnPoint2 == null && WeaponManager.Instance != null)
        {
            GameObject leftHand = WeaponManager.Instance.GetSixthWeaponLeftHand();
            if (leftHand != null)
            {
                Transform t = leftHand.transform.Find("Laser");
                if (t == null) t = leftHand.transform.Find("Laser (1)");
                if (t != null) laserSpawnPoint2 = t;
            }
        }
        useDualLaser = (laserSpawnPoint2 != null);
        
        // Laser container oluştur
        laserContainer = new GameObject("LaserContainer");
        laserContainer.transform.SetParent(transform);
        laserContainer.transform.localPosition = Vector3.zero;
        laserContainer.transform.localRotation = Quaternion.identity;
        
        // Laser'ları oluştur
        CreateLaser();
        
        // Enerji bar'ı oluştur
        if (showEnergyBar)
        {
            CreateEnergyBar();
        }
        
        // Enerjiyi başlangıçta full yap
        currentEnergy = maxEnergy;
        
        // Başlangıçta kapalı
        SetLaserActive(false);
    }

    private void Start()
    {
        if (!useDualLaser && laserSpawnPoint2 == null && WeaponManager.Instance != null)
        {
            GameObject leftHand = WeaponManager.Instance.GetSixthWeaponLeftHand();
            if (leftHand != null)
            {
                Transform t = leftHand.transform.Find("Laser");
                if (t == null) t = leftHand.transform.Find("Laser (1)");
                if (t != null)
                {
                    laserSpawnPoint2 = t;
                    useDualLaser = true;
                    CreateDualLaserBeams();
                }
            }
        }
    }

    private void Update()
    {
        if (GameManager.IsRetryScreenActive) return; // Retry ekranında sadece HandRayUIInteractor ile butonlara tıklanabilir
        if (!TutorialIntroController.TutorialCompleteFreehand && TutorialIntroController.TutorialSixthWeaponPhase && !TutorialIntroController.TutorialSixthWeaponSecondaryEnabled)
            return; // 15. diyalog bitmeden ikincil (yavaşlatma) kapalı
        
        // B/Y tuşları veya sol kontrolcü trigger ile toggle (SlowedGun sol elde tutulurken)
        if (OVRInput.GetDown(OVRInput.Button.Two) || OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            ToggleLaser();
        }
        
        // Laser aktifse güncelle
        if (isLaserActive)
        {
            UpdateLaser();
            UpdateSlowEffect();
            UpdateEnergyBar();
            
            // Enerji kontrolü
            if (currentEnergy <= 0f)
            {
                SetLaserActive(false); // Enerji bitince kapat
            }
        }
        else
        {
            // Laser kapalıyken enerjiyi doldur
            if (currentEnergy < maxEnergy)
            {
                currentEnergy += rechargeRate * Time.deltaTime;
                currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
                UpdateEnergyBar();
            }
        }
    }

    private void CreateLaser()
    {
        // 1. GLOW LASER (Dış glow - en geniş)
        GameObject glowObj = new GameObject("LaserGlow");
        glowObj.transform.SetParent(laserContainer.transform);
        laserGlow = glowObj.AddComponent<LineRenderer>();
        SetupLaser(laserGlow, glowWidth, glowColor, 0);
        
        // 2. MAIN LASER (Ana laser)
        GameObject mainObj = new GameObject("LaserMain");
        mainObj.transform.SetParent(laserContainer.transform);
        laserMain = mainObj.AddComponent<LineRenderer>();
        SetupLaser(laserMain, laserWidth, laserColor, 1);
        
        // 3. CORE LASER (Parlak çekirdek - en dar)
        GameObject coreObj = new GameObject("LaserCore");
        coreObj.transform.SetParent(laserContainer.transform);
        laserCore = coreObj.AddComponent<LineRenderer>();
        SetupLaser(laserCore, coreWidth, coreColor, 2);
        
        // Işık efekti
        if (enableLight)
        {
            GameObject lightObj = new GameObject("LaserLight");
            lightObj.transform.SetParent(laserContainer.transform);
            laserLight = lightObj.AddComponent<Light>();
            laserLight.type = LightType.Point;
            laserLight.color = lightColor;
            laserLight.intensity = lightIntensity;
            laserLight.range = lightRange;
            laserLight.shadows = LightShadows.None;
        }

        if (useDualLaser)
            CreateDualLaserBeams();

        UICameraStackSetup.SetLayerRecursivelyToUI(laserContainer);
    }

    private void CreateDualLaserBeams()
    {
        if (laserContainer == null) return;
        // Sol el beam (spawn1 -> convergence)
        GameObject leftGlowObj = new GameObject("LaserLeftGlow");
        leftGlowObj.transform.SetParent(laserContainer.transform);
        laserLeftGlow = leftGlowObj.AddComponent<LineRenderer>();
        SetupLaser(laserLeftGlow, glowWidth, glowColor, 0);
        GameObject leftMainObj = new GameObject("LaserLeftMain");
        leftMainObj.transform.SetParent(laserContainer.transform);
        laserLeftMain = leftMainObj.AddComponent<LineRenderer>();
        SetupLaser(laserLeftMain, laserWidth, laserColor, 1);
        GameObject leftCoreObj = new GameObject("LaserLeftCore");
        leftCoreObj.transform.SetParent(laserContainer.transform);
        laserLeftCore = leftCoreObj.AddComponent<LineRenderer>();
        SetupLaser(laserLeftCore, coreWidth, coreColor, 2);
        // Sağ el beam (spawn2 -> convergence)
        GameObject rightGlowObj = new GameObject("LaserRightGlow");
        rightGlowObj.transform.SetParent(laserContainer.transform);
        laserRightGlow = rightGlowObj.AddComponent<LineRenderer>();
        SetupLaser(laserRightGlow, glowWidth, glowColor, 0);
        GameObject rightMainObj = new GameObject("LaserRightMain");
        rightMainObj.transform.SetParent(laserContainer.transform);
        laserRightMain = rightMainObj.AddComponent<LineRenderer>();
        SetupLaser(laserRightMain, laserWidth, laserColor, 1);
        GameObject rightCoreObj = new GameObject("LaserRightCore");
        rightCoreObj.transform.SetParent(laserContainer.transform);
        laserRightCore = rightCoreObj.AddComponent<LineRenderer>();
        SetupLaser(laserRightCore, coreWidth, coreColor, 2);
    }

    private void SetupLaser(LineRenderer line, float width, Color color, int sortingOrder)
    {
        line.material = CreateLaserMaterial(color);
        line.startWidth = width;
        line.endWidth = width;
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sortingOrder = sortingOrder;
        line.positionCount = 2; // Başlangıç ve bitiş
        line.enabled = false;
        
        // Smooth görünüm
        line.numCapVertices = 5;
        line.numCornerVertices = 5;
    }

    private Material CreateLaserMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            mat = new Material(Shader.Find("Sprites/Default"));
        }
        mat.SetColor("_Color", color);
        mat.color = color;
        return mat;
    }

    private void UpdateLaser()
    {
        if (laserSpawnPoint == null) return;
        
        if (useDualLaser && laserSpawnPoint2 != null)
        {
            UpdateDualLaser();
            return;
        }
        
        Vector3 startPos = laserSpawnPoint.position;
        Vector3 direction = laserSpawnPoint.forward;
        Vector3 endPos = startPos + direction * laserRange;
        
        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, laserRange))
        {
            endPos = hit.point;
        }
        
        laserGlow.SetPosition(0, startPos);
        laserGlow.SetPosition(1, endPos);
        laserMain.SetPosition(0, startPos);
        laserMain.SetPosition(1, endPos);
        laserCore.SetPosition(0, startPos);
        laserCore.SetPosition(1, endPos);
        
        if (laserLight != null)
            laserLight.transform.position = startPos;
    }

    private void UpdateDualLaser()
    {
        Vector3 p1 = laserSpawnPoint.position;
        Vector3 p2 = laserSpawnPoint2.position;
        Vector3 dir1 = laserSpawnPoint.forward;
        Vector3 dir2 = laserSpawnPoint2.forward;
        
        Vector3 end1 = p1 + dir1 * laserRange;
        Vector3 end2 = p2 + dir2 * laserRange;
        RaycastHit hit;
        if (Physics.Raycast(p1, dir1, out hit, laserRange))
            end1 = hit.point;
        if (Physics.Raycast(p2, dir2, out hit, laserRange))
            end2 = hit.point;
        
        if (laserLeftGlow != null) { laserLeftGlow.SetPosition(0, p1); laserLeftGlow.SetPosition(1, end1); }
        if (laserLeftMain != null) { laserLeftMain.SetPosition(0, p1); laserLeftMain.SetPosition(1, end1); }
        if (laserLeftCore != null) { laserLeftCore.SetPosition(0, p1); laserLeftCore.SetPosition(1, end1); }
        if (laserRightGlow != null) { laserRightGlow.SetPosition(0, p2); laserRightGlow.SetPosition(1, end2); }
        if (laserRightMain != null) { laserRightMain.SetPosition(0, p2); laserRightMain.SetPosition(1, end2); }
        if (laserRightCore != null) { laserRightCore.SetPosition(0, p2); laserRightCore.SetPosition(1, end2); }
        
        laserGlow.SetPosition(0, p1); laserGlow.SetPosition(1, p1);
        laserMain.SetPosition(0, p1); laserMain.SetPosition(1, p1);
        laserCore.SetPosition(0, p1); laserCore.SetPosition(1, p1);
        if (laserLight != null) laserLight.transform.position = p1;
    }

    private void ToggleLaser()
    {
        SetLaserActive(!isLaserActive);
    }

    private void SetLaserActive(bool active)
    {
        // Enerji yoksa açma
        if (active && currentEnergy <= 0f)
        {
            return;
        }
        
        isLaserActive = active;
        
        if (laserGlow != null) laserGlow.enabled = active;
        if (laserMain != null) laserMain.enabled = active;
        if (laserCore != null) laserCore.enabled = active;
        if (laserLight != null) laserLight.enabled = active;
        if (useDualLaser)
        {
            if (laserLeftGlow != null) laserLeftGlow.enabled = active;
            if (laserLeftMain != null) laserLeftMain.enabled = active;
            if (laserLeftCore != null) laserLeftCore.enabled = active;
            if (laserRightGlow != null) laserRightGlow.enabled = active;
            if (laserRightMain != null) laserRightMain.enabled = active;
            if (laserRightCore != null) laserRightCore.enabled = active;
        }
        
        // Laser kapandığında tüm slow'ları kaldır
        if (!active)
        {
            RemoveAllSlows();
            isHittingEnemy = false;
        }
    }
    
    private void UpdateSlowEffect()
    {
        if (laserSpawnPoint == null) return;
        
        slowTickTimer += Time.deltaTime;
        float tickInterval = 1f / slowTickRate;
        
        if (slowTickTimer >= tickInterval)
        {
            slowTickTimer = 0f;
            
            HashSet<EnemyBehavior> currentHitEnemies = new HashSet<EnemyBehavior>();
            bool hittingAnyEnemy = false;
            
            if (useDualLaser && laserSpawnPoint2 != null)
            {
                Vector3 dir1 = laserSpawnPoint.forward;
                Vector3 dir2 = laserSpawnPoint2.forward;
                RaycastHit[] hits1 = Physics.RaycastAll(laserSpawnPoint.position, dir1, laserRange, raycastLayerMask);
                RaycastHit[] hits2 = Physics.RaycastAll(laserSpawnPoint2.position, dir2, laserRange, raycastLayerMask);
                foreach (RaycastHit hit in hits1) CollectHitEnemy(hit, currentHitEnemies, ref hittingAnyEnemy);
                foreach (RaycastHit hit in hits2) CollectHitEnemy(hit, currentHitEnemies, ref hittingAnyEnemy);
            }
            else
            {
                Vector3 startPos = laserSpawnPoint.position;
                Vector3 direction = laserSpawnPoint.forward;
                RaycastHit[] hits = Physics.RaycastAll(startPos, direction, laserRange, raycastLayerMask);
                foreach (RaycastHit hit in hits) CollectHitEnemy(hit, currentHitEnemies, ref hittingAnyEnemy);
            }
            
            isHittingEnemy = hittingAnyEnemy;
            
            // Artık laser'da olmayan düşmanların slow'unu kaldır
            HashSet<EnemyBehavior> enemiesToRemove = new HashSet<EnemyBehavior>();
            foreach (EnemyBehavior enemy in slowedEnemies)
            {
                if (enemy == null || !currentHitEnemies.Contains(enemy))
                {
                    enemiesToRemove.Add(enemy);
                }
            }
            
            foreach (EnemyBehavior enemy in enemiesToRemove)
            {
                RemoveSlowFromEnemy(enemy);
            }
        }
    }
    
    private static bool _tutorialSixthSecondaryUsed;

    private void CollectHitEnemy(RaycastHit hit, HashSet<EnemyBehavior> currentHitEnemies, ref bool hittingAnyEnemy)
    {
        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            EnemyBehavior enemyBehavior = enemyHealth.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null && !currentHitEnemies.Contains(enemyBehavior))
            {
                currentHitEnemies.Add(enemyBehavior);
                ApplySlowToEnemy(enemyBehavior, enemyHealth, hit.collider);
                hittingAnyEnemy = true;
            }
        }
    }
    
    private void ApplySlowToEnemy(EnemyBehavior enemyBehavior, EnemyHealth enemyHealth, Collider hitCollider)
    {
        if (enemyBehavior == null) return;
        
        // Slow uygula
        enemyBehavior.ApplySlow(slowMultiplier);
        slowedEnemies.Add(enemyBehavior);
        
        // Buzlanma görsel efekti uygula
        if (enemyHealth != null)
        {
            enemyHealth.ApplyFreezeEffect();
            // Can azalma efekti - tüm düşman tiplerine (tur1, tur2, tur3, tur4) uygulanır
            if (damagePerTick > 0f && hitCollider != null)
            {
                enemyHealth.TakeDamage(damagePerTick, hitCollider, fromFlameSpray: false, fromSecondary: true, tutorialWeaponIndex: 5);
            }
        }
        
        if (TutorialIntroController.TutorialSixthWeaponPhase && TutorialIntroController.TutorialSixthWeaponSecondaryEnabled && !_tutorialSixthSecondaryUsed)
        {
            _tutorialSixthSecondaryUsed = true;
            TutorialIntroController.NotifyTutorialSecondaryUsed(5);
        }
    }
    
    private void RemoveSlowFromEnemy(EnemyBehavior enemyBehavior)
    {
        if (enemyBehavior == null) return;
        
        // Slow'u kaldır
        enemyBehavior.RemoveSlow();
        slowedEnemies.Remove(enemyBehavior);
        
        // Buzlanma efektini kaldır
        EnemyHealth enemyHealth = enemyBehavior.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.RemoveFreezeEffect();
        }
    }
    
    private void RemoveAllSlows()
    {
        // Tüm slow'ları kaldır
        foreach (EnemyBehavior enemy in slowedEnemies)
        {
            if (enemy != null)
            {
                RemoveSlowFromEnemy(enemy);
            }
        }
        slowedEnemies.Clear();
    }

    private void UpdateEnergyBar()
    {
        if (!showEnergyBar || energyBarContainer == null || fillImage == null) return;
        
        // Enerjiyi azalt (laser açıkken)
        if (isLaserActive)
        {
            float totalDrainRate = drainRate;
            if (isHittingEnemy)
            {
                totalDrainRate += drainRateOnHit;
            }
            
            currentEnergy -= totalDrainRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        }
        
        // Billboard - her zaman kameraya baksın
        if (Camera.main != null)
        {
            Vector3 lookDir = energyBarContainer.transform.position - Camera.main.transform.position;
            if (lookDir != Vector3.zero)
            {
                energyBarContainer.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        
        // Fill amount - smooth
        float energyPercent = currentEnergy / maxEnergy;
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, energyPercent, Time.deltaTime * 15f);
        
        // Renk - enerji azaldıkça kırmızıya dönüş
        float intensity = 0.8f + energyPercent * 0.4f;
        Color currentColor = new Color(
            Mathf.Lerp(1f, uiColor.r, energyPercent) * intensity,
            Mathf.Lerp(0f, uiColor.g, energyPercent) * intensity,
            Mathf.Lerp(0f, uiColor.b, energyPercent) * intensity,
            uiColor.a
        );
        fillImage.color = currentColor;
        
        // Glow - düşük enerjide parlak
        if (glowImage != null)
        {
            glowImage.fillAmount = fillImage.fillAmount;
            
            float glowAlpha = 0f;
            if (energyPercent < 0.3f)
            {
                glowAlpha = (0.3f - energyPercent) / 0.3f * 0.6f;
            }
            
            // Düşük enerji pulse
            if (energyPercent < 0.2f)
            {
                glowAlpha = 0.3f + Mathf.Sin(Time.time * 12f) * 0.2f;
                
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

        // Outer halo - enerji ile hafif güçlensin
        if (outerHaloImage != null)
        {
            float haloPulse = 0.06f + energyPercent * 0.14f;
            if (energyPercent < 0.2f)
            {
                haloPulse = 0.15f + Mathf.Sin(Time.time * 10f) * 0.05f;
            }
            outerHaloImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, haloPulse);
        }

        // Progress dot - dolum ucunda parlayan
        if (progressDotImage != null && progressDotRect != null)
        {
            float fill = Mathf.Clamp01(fillImage.fillAmount);
            float angleDeg = 90f - fill * 360f; // Top origin + clockwise
            float angleRad = angleDeg * Mathf.Deg2Rad;

            // Radius (UI elemanının yarıçapı)
            float radius = 50f; // 100 size / 2
            Vector2 pos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            progressDotRect.anchoredPosition = pos;

            float dotAlpha = Mathf.Clamp01(energyPercent) * 0.9f;
            if (energyPercent < 0.2f) dotAlpha = 0.8f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotImage.color = new Color(1f, 1f, 1f, dotAlpha);

            float dotScale = 1f + energyPercent * 0.6f;
            if (energyPercent < 0.2f) dotScale = 1.6f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotRect.localScale = Vector3.one * dotScale;
        }
        
        // Background - düşük enerjide hafif parla
        if (backgroundImage != null)
        {
            Color bgCol = uiBgColor;
            if (energyPercent < 0.3f)
            {
                float t = (0.3f - energyPercent) / 0.3f;
                bgCol = Color.Lerp(uiBgColor, new Color(1f * 0.3f, 0f, 0f, uiBgColor.a), t * 0.5f);
            }
            backgroundImage.color = bgCol;
        }
    }
    
    private void CreateEnergyBar()
    {
        // Ana container - silaha parent'la
        energyBarContainer = new GameObject("EnergyBarUI");
        energyBarContainer.transform.SetParent(transform); // Silahın child'ı
        energyBarContainer.transform.localPosition = uiOffset;
        energyBarContainer.transform.localRotation = Quaternion.identity;
        
        // World Space Canvas
        energyBarCanvas = energyBarContainer.AddComponent<Canvas>();
        energyBarCanvas.renderMode = RenderMode.WorldSpace;
        UICameraStackSetup.Instance?.RegisterWorldSpaceCanvas(energyBarCanvas);
        
        RectTransform canvasRect = energyBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 2);
        canvasRect.localScale = Vector3.one * uiSize * 0.01f;
        
        // 1. Arka plan arc (koyu, tam daire)
        GameObject bgArc = CreateArcElement("BgArc", energyBarContainer.transform, 100);
        backgroundImage = bgArc.GetComponent<Image>();
        backgroundImage.color = uiBgColor;
        backgroundImage.fillAmount = 1f;
        
        // 2. Progress arc (parlak, dolan)
        GameObject progressArc = CreateArcElement("ProgressArc", energyBarContainer.transform, 100);
        fillImage = progressArc.GetComponent<Image>();
        fillImage.color = uiColor;
        fillImage.fillAmount = 1f; // Başlangıçta full
        
        // 3. Glow
        GameObject glowArc = CreateArcElement("GlowArc", energyBarContainer.transform, 118);
        glowImage = glowArc.GetComponent<Image>();
        glowImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0f);
        glowImage.fillAmount = 1f;
        RectTransform glowRect = glowArc.GetComponent<RectTransform>();
        glowRect.SetAsFirstSibling();

        // 4. Outer halo (tam daire, çok yumuşak)
        GameObject halo = CreateArcElement("OuterHalo", energyBarContainer.transform, 130);
        outerHaloImage = halo.GetComponent<Image>();
        outerHaloImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.08f);
        outerHaloImage.fillAmount = 1f;
        RectTransform haloRect = halo.GetComponent<RectTransform>();
        haloRect.SetAsFirstSibling();

        // 5. Progress dot (dolum ucunda parlayan nokta)
        GameObject dot = new GameObject("ProgressDot");
        dot.transform.SetParent(energyBarContainer.transform, false);
        progressDotImage = dot.AddComponent<Image>();
        progressDotImage.sprite = CreateDotSprite(64);
        progressDotImage.color = new Color(1f, 1f, 1f, 0.9f);
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
    
    private void OnDestroy()
    {
        // Tüm slow'ları temizle
        RemoveAllSlows();
        
        // UI'ı temizle
        if (energyBarContainer != null)
        {
            Destroy(energyBarContainer);
        }
        
        if (laserContainer != null)
        {
            Destroy(laserContainer);
        }
    }
}
