using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Meta.XR.MRUtilityKit;

/// <summary>
/// LastGun için ateş püskürtme (flame thrower) mekanizması.
/// VFX_Fire ile ileri doğru püskürtme, püskürtme enerjisi ve SeventhGunWhip tarzı enerji bar UI.
/// </summary>
public class LastGunFlameSpray : MonoBehaviour
{
    [Header("=== VFX AYARLARI ===")]
    [Tooltip("Ateş VFX prefab (VFX_Fire)")]
    public GameObject flameVfxPrefab;

    [Tooltip("Püskürtme çıkış noktası (namlu - barrel1)")]
    public Transform sprayOrigin;

    [Tooltip("İleri yön hedefi (targetDirection1) - boşsa sprayOrigin.forward kullanılır")]
    public Transform sprayForwardTarget;

    [Tooltip("Barrel'dan ne kadar ileriye VFX (forward offset). Çizgiler (forward offset - 1) kadar uzanır.")]
    public float vfxForwardOffset = 0.15f;

    [Tooltip("VFX instance scale")]
    public Vector3 vfxScale = new Vector3(1f, 1f, 1f);

    [Tooltip("Ateş partiküllerinin ileri hızı (velocity) - yüksek değer = uzağa püskürtme")]
    [Range(2f, 30f)]
    public float sprayForwardSpeed = 12f;

    [Tooltip("Püskürtme koni açısı (derece) - düşük = ileri odaklı, yüksek = etrafa yayılır")]
    [Range(2f, 20f)]
    public float sprayConeAngle = 6f;

    [Tooltip("Yön rastgeleliği (0=sadece ileri, ~0.1=hafif alev titremesi, yakma hissi)")]
    [Range(0f, 0.25f)]
    public float sprayDirectionRandomness = 0.06f;

    [Header("=== CARTOON FIRE LINES (Forward Offset Kadar İleri) ===")]
    [Tooltip("Çizgi segment sayısı (dalgalı alev)")]
    [Range(8, 36)]
    public int flameLineSegments = 22;

    [Tooltip("Çizgi kalınlığı (barrel tarafı)")]
    public float flameLineWidthStart = 0.03f;

    [Tooltip("Çizgi kalınlığı (uç taraf, VFX'e geçiş)")]
    public float flameLineWidthEnd = 0.09f;

    [Tooltip("Alev dalga miktarı (titreme)")]
    public float flameLineWaveAmount = 0.028f;

    [Tooltip("Dalga hızı")]
    public float flameLineWaveSpeed = 30f;

    [Tooltip("Dış glow (alev dışı)")]
    public Color flameLineGlowColor = new Color(1f, 0.35f, 0.05f, 0.55f);

    [Tooltip("İç alev (sarı–turuncu)")]
    public Color flameLineCoreColor = new Color(1f, 0.9f, 0.25f, 1f);

    [Header("=== ENERJİ AYARLARI ===")]
    [Tooltip("Maksimum püskürtme enerjisi")]
    public float maxSprayEnergy = 100f;

    [Tooltip("Tetik basılıyken saniyede tüketilen enerji")]
    public float drainRate = 25f;

    [Tooltip("Tetik bırakıldığında saniyede yenilenen enerji")]
    public float regenRate = 15f;

    [Header("=== HASAR AYARLARI ===")]
    [Tooltip("Saniyede verilen hasar (sürekli püskürtme)")]
    public float damagePerSecond = 12f;

    [Tooltip("Püskürtme menzili")]
    public float sprayRange = 8f;

    [Tooltip("Hasar kontrol aralığı (saniye)")]
    public float damageCheckInterval = 0.08f;

    [Tooltip("Düşman layer mask")]
    public LayerMask enemyLayerMask = -1;

    [Header("=== SES / HAPTİK ===")]
    [Tooltip("Haptic şiddeti")]
    public float hapticStrength = 0.6f;

    [Tooltip("Sol el mi (GunFire ile aynı)")]
    public bool isLeftHanded = false;

    [Header("=== ENERJİ CIRCLE UI (FifthGun gibi dairesel) ===")]
    [Tooltip("Prefab içinden atanacak Transform: UI'ın konumu bu objenin pozisyonudur. Boş bırakılırsa prefab içinde 'SprayEnergyUIPosition' aranır veya oluşturulur.")]
    public Transform uiPosition;

    [Tooltip("UI offset (uiPosition'a göre local X,Y,Z)")]
    public Vector3 uiOffset = Vector3.zero;

    [Tooltip("UI boyutu")]
    public float uiSize = 0.06f;

    [Tooltip("Ana renk (ateş)")]
    public Color uiColor = new Color(1f, 0.4f, 0.1f, 1f);

    [Tooltip("Arka plan rengi")]
    public Color uiBgColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);

    [Tooltip("Daire arc kalınlığı (0-1)")]
    [Range(0.05f, 0.5f)]
    public float uiArcThickness = 0.15f;

    // Private
    private float currentSprayEnergy;
    private GameObject vfxInstance;
    private ParticleSystem vfxParticles;
    private bool isSpraying;
    private GameObject flameLineContainer;
    private LineRenderer flameLineGlow;
    private LineRenderer flameLineCore;
    private float nextDamageTime;
    private HashSet<EnemyHealth> hitEnemiesThisSpray = new HashSet<EnemyHealth>();
    private GunFire gunFire;
    private Coroutine hapticCoroutine;
    private bool sprayEnergyGameOverTriggered;

    // UI (FifthGun tarzı circle/arc)
    private GameObject uiContainer;
    private Canvas sprayCanvas;
    private Image backgroundImage;
    private Image fillImage;
    private Image glowImage;
    private Image outerHaloImage;
    private Image progressDotImage;
    private RectTransform progressDotRect;

    void Start()
    {
        currentSprayEnergy = maxSprayEnergy;
        gunFire = GetComponent<GunFire>();
        if (gunFire != null)
            isLeftHanded = gunFire.isLeftHanded;

        if (sprayOrigin == null)
        {
            if (gunFire != null && gunFire.barrel1 != null)
                sprayOrigin = gunFire.barrel1;
            else
                Debug.LogWarning("LastGunFlameSpray: sprayOrigin atanmamış!");
        }

        if (sprayForwardTarget == null && gunFire != null && gunFire.targetDirection1 != null)
            sprayForwardTarget = gunFire.targetDirection1;

        ResolveUIPosition();
        CreateFlameVFXInstance();
        CreateFlameLines();
        CreateEnergyUI();
    }

    /// <summary>
    /// uiPosition prefab içinden atanmamışsa, child'da SprayEnergyUIPosition ara veya oluştur.
    /// Böylece UI konumu her zaman prefab içindeki bir Transform ile tanımlanır.
    /// </summary>
    private void ResolveUIPosition()
    {
        if (uiPosition != null) return;

        Transform found = transform.Find("SprayEnergyUIPosition");
        if (found != null)
        {
            uiPosition = found;
            return;
        }
        found = transform.Find("EnergyUIPosition");
        if (found != null)
        {
            uiPosition = found;
            return;
        }

        GameObject anchor = new GameObject("SprayEnergyUIPosition");
        anchor.transform.SetParent(transform);
        anchor.transform.localPosition = new Vector3(0.05f, 0.2f, 0f);
        anchor.transform.localRotation = Quaternion.identity;
        anchor.transform.localScale = Vector3.one;
        uiPosition = anchor.transform;
        UICameraStackSetup.SetLayerRecursivelyToUI(anchor);
    }

    void Update()
    {
        OVRInput.Button trigger = isLeftHanded ? OVRInput.Button.PrimaryIndexTrigger : OVRInput.Button.SecondaryIndexTrigger;
        bool triggerHeld = OVRInput.Get(trigger);

        if (!TutorialIntroController.TutorialCompleteFreehand && TutorialIntroController.TutorialNinthWeaponPhase && !TutorialIntroController.TutorialNinthWeaponPrimaryEnabled)
            triggerHeld = false; // 21. diyalog bitmeden birincil (flame spray) kapalı

        bool flameSprayUnlimited = TutorialIntroController.TutorialNinthWeaponPhase && !TutorialIntroController.TutorialNinthWeaponDialogue22Complete;
        if (triggerHeld && (flameSprayUnlimited || currentSprayEnergy > 0f))
        {
            if (!isSpraying)
                StartSpray();
            if (!flameSprayUnlimited)
            {
                currentSprayEnergy -= drainRate * Time.deltaTime;
                if (currentSprayEnergy < 0f) currentSprayEnergy = 0f;
            }
        }
        else
        {
            if (isSpraying)
                StopSpray();
            currentSprayEnergy += regenRate * Time.deltaTime;
            if (currentSprayEnergy > maxSprayEnergy) currentSprayEnergy = maxSprayEnergy;
        }

        // Enerji sıfırlanınca bir kez game over
        if (currentSprayEnergy <= 0f && !sprayEnergyGameOverTriggered)
        {
            sprayEnergyGameOverTriggered = true;
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver(null);
        }

        if (isSpraying && currentSprayEnergy > 0f)
        {
            DoDamageTick();
            if (TutorialIntroController.TutorialNinthWeaponPhase && !TutorialIntroController.TutorialNinthWeaponDialogue22Complete)
                TutorialIntroController.NotifyTutorialNinthWeaponFlameSprayTime(Time.deltaTime);
        }

        // VR kontrolcüsü A butonu: Toy kullanılabilirse ToyHelper tüketir; Toy aktifken ateş engellenir; değilse tek atış (Fireball vb.)
        if (gunFire != null && OVRInput.GetDown(OVRInput.Button.One))
        {
            if (!TutorialIntroController.TutorialCompleteFreehand && TutorialIntroController.TutorialNinthWeaponPhase && !TutorialIntroController.TutorialNinthWeaponSecondaryEnabled)
                return; // 22. diyalog bitmeden fireball kapalı
            ToyHelper toyHelper = GetComponent<ToyHelper>();
            if (toyHelper == null || (toyHelper.IsIdle() && !toyHelper.WouldConsumeA()))
                gunFire.TryFire(isSecondary: true);
        }

        UpdateVFXPosition();
        if (isSpraying)
            UpdateFlameLines();
        else
            SetFlameLinesVisible(false);
        UpdateEnergyUI();
    }

    /// <summary>Tutorial retry: enerjiyi yenile. Death/energy retry sonrası çağrılır.</summary>
    public void RefillSprayEnergy()
    {
        currentSprayEnergy = maxSprayEnergy;
        sprayEnergyGameOverTriggered = false;
        UpdateEnergyUI();
    }

    private Vector3 GetSprayForward()
    {
        if (sprayOrigin == null) return transform.forward;
        if (sprayForwardTarget != null)
            return (sprayForwardTarget.position - sprayOrigin.position).normalized;
        return sprayOrigin.forward;
    }

    private void CreateFlameVFXInstance()
    {
        if (flameVfxPrefab == null || sprayOrigin == null) return;

        vfxInstance = Instantiate(flameVfxPrefab, sprayOrigin.position, Quaternion.identity);
        vfxInstance.transform.SetParent(sprayOrigin);
        vfxInstance.transform.localPosition = Vector3.forward * vfxForwardOffset;
        vfxInstance.transform.localRotation = Quaternion.identity;
        vfxInstance.transform.localScale = vfxScale;

        Vector3 forward = GetSprayForward();
        vfxInstance.transform.rotation = Quaternion.LookRotation(forward);

        vfxParticles = vfxInstance.GetComponentInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in vfxInstance.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(sprayForwardSpeed * 0.7f, sprayForwardSpeed);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = ps.shape;
            if (shape.enabled)
            {
                shape.angle = sprayConeAngle * 0.5f;
                shape.randomDirectionAmount = sprayDirectionRandomness;
            }
        }
        if (vfxParticles != null)
            vfxParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        UICameraStackSetup.SetLayerRecursivelyToUI(vfxInstance);
        vfxInstance.SetActive(false);
    }

    private void UpdateVFXPosition()
    {
        if (vfxInstance == null || sprayOrigin == null) return;
        Vector3 forward = GetSprayForward();
        vfxInstance.transform.rotation = Quaternion.LookRotation(forward);
    }

    private void CreateFlameLines()
    {
        if (sprayOrigin == null) return;

        flameLineContainer = new GameObject("FlameLines");
        flameLineContainer.transform.SetParent(sprayOrigin);
        flameLineContainer.transform.localPosition = Vector3.zero;
        flameLineContainer.transform.localRotation = Quaternion.identity;
        flameLineContainer.transform.localScale = Vector3.one;

        var glowObj = new GameObject("FlameGlow");
        glowObj.transform.SetParent(flameLineContainer.transform);
        flameLineGlow = glowObj.AddComponent<LineRenderer>();
        SetupFlameLine(flameLineGlow, flameLineWidthStart * 2f, flameLineWidthEnd * 1.6f, flameLineGlowColor, true);

        var coreObj = new GameObject("FlameCore");
        coreObj.transform.SetParent(flameLineContainer.transform);
        flameLineCore = coreObj.AddComponent<LineRenderer>();
        SetupFlameLine(flameLineCore, flameLineWidthStart, flameLineWidthEnd, flameLineCoreColor, false);

        UICameraStackSetup.SetLayerRecursivelyToUI(flameLineContainer);
        SetFlameLinesVisible(false);
    }

    private void SetupFlameLine(LineRenderer line, float widthStart, float widthEnd, Color color, bool isGlow)
    {
        line.useWorldSpace = true;
        line.positionCount = flameLineSegments + 1;
        line.startWidth = widthStart;
        line.endWidth = widthEnd;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.material.color = color;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = 6;
        line.numCornerVertices = 6;
        if (isGlow)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0f), new GradientColorKey(new Color(1f, 0.25f, 0f), 0.6f), new GradientColorKey(new Color(0.8f, 0.15f, 0f), 1f) },
                new[] { new GradientAlphaKey(0.6f, 0f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0.15f, 1f) }
            );
            line.colorGradient = g;
        }
        else
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.95f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.7f, 0.2f), 0.35f), new GradientColorKey(new Color(1f, 0.4f, 0.1f), 0.7f), new GradientColorKey(new Color(1f, 0.25f, 0f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.95f, 0.3f), new GradientAlphaKey(0.6f, 0.7f), new GradientAlphaKey(0.25f, 1f) }
            );
            line.colorGradient = g;
        }
        line.enabled = false;
    }

    private float GetFlameLineLength()
    {
        return Mathf.Max(vfxForwardOffset - 1f, 0.05f);
    }

    private void UpdateFlameLines()
    {
        if (sprayOrigin == null || flameLineGlow == null || flameLineCore == null) return;

        Vector3 start = sprayOrigin.position;
        Vector3 forward = GetSprayForward();
        float length = GetFlameLineLength();
        Vector3 end = start + forward * length;
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(forward, Vector3.right).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;

        float t = Time.time * flameLineWaveSpeed;
        for (int i = 0; i <= flameLineSegments; i++)
        {
            float segT = (float)i / flameLineSegments;
            Vector3 basePos = Vector3.Lerp(start, end, segT);
            float flicker = 1f - segT * 0.4f;
            float wave = Mathf.Sin(t + segT * Mathf.PI * 5f) * flameLineWaveAmount * flicker;
            float wave2 = (Mathf.PerlinNoise(t * 0.4f + segT * 10f, 0f) - 0.5f) * flameLineWaveAmount * 1.2f * flicker;
            float waveUp = Mathf.Sin(t * 1.2f + segT * Mathf.PI * 3f) * flameLineWaveAmount * 0.6f * flicker;
            Vector3 offset = right * (wave + wave2) + up * waveUp;
            Vector3 pos = basePos + offset;
            if (flameLineGlow.enabled) flameLineGlow.SetPosition(i, pos);
            if (flameLineCore.enabled) flameLineCore.SetPosition(i, pos);
        }
    }

    private void SetFlameLinesVisible(bool visible)
    {
        if (flameLineGlow != null) flameLineGlow.enabled = visible;
        if (flameLineCore != null) flameLineCore.enabled = visible;
    }

    private void StartSpray()
    {
        isSpraying = true;
        hitEnemiesThisSpray.Clear();
        nextDamageTime = Time.time;

        if (vfxInstance != null)
        {
            vfxInstance.SetActive(true);
            if (vfxParticles != null)
                vfxParticles.Play(true);
        }
        SetFlameLinesVisible(true);
        UpdateFlameLines();

        if (WeaponManager.Instance != null)
            WeaponManager.Instance.PlayFireSound();

        if (hapticCoroutine != null) StopCoroutine(hapticCoroutine);
        hapticCoroutine = StartCoroutine(HapticLoop());
    }

    private void StopSpray()
    {
        isSpraying = false;
        if (vfxInstance != null)
        {
            if (vfxParticles != null)
                vfxParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        SetFlameLinesVisible(false);

        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
            hapticCoroutine = null;
        }
        OVRInput.Controller c = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(0f, 0f, c);
    }

    private IEnumerator HapticLoop()
    {
        OVRInput.Controller controller = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        while (isSpraying)
        {
            OVRInput.SetControllerVibration(0.5f, hapticStrength, controller);
            yield return new WaitForSeconds(0.05f);
        }
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    private void DoDamageTick()
    {
        if (Time.time < nextDamageTime || sprayOrigin == null) return;
        nextDamageTime = Time.time + damageCheckInterval;

        Vector3 origin = sprayOrigin.position;
        Vector3 forward = GetSprayForward();
        float damageThisTick = damagePerSecond * damageCheckInterval;

        RaycastHit[] hits = Physics.RaycastAll(origin, forward, sprayRange, enemyLayerMask);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider) continue;
            EnemyHealth eh = hit.collider.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(damageThisTick, hit.collider, fromFlameSpray: true, fromSecondary: false, tutorialWeaponIndex: 8);
                hitEnemiesThisSpray.Add(eh);
            }
        }

        // SphereCast for wider cone feel
        RaycastHit[] sphereHits = Physics.SphereCastAll(origin, 0.4f, forward, sprayRange, enemyLayerMask);
        foreach (RaycastHit hit in sphereHits)
        {
            if (!hit.collider) continue;
            EnemyHealth eh = hit.collider.GetComponentInParent<EnemyHealth>();
            if (eh != null && !hitEnemiesThisSpray.Contains(eh))
            {
                eh.TakeDamage(damageThisTick * 0.5f, hit.collider, fromFlameSpray: true, fromSecondary: false, tutorialWeaponIndex: 8);
                hitEnemiesThisSpray.Add(eh);
            }
        }

        // Duvar (destructible mesh) – tetik basılı tutulurken de kırılsın (Raycast + SphereCast ile koni)
        float coneRadius = 0.4f;
        RaycastHit[] wallSphereHits = Physics.SphereCastAll(origin, coneRadius, forward, sprayRange);
        foreach (RaycastHit hit in wallSphereHits)
        {
            if (!hit.collider) continue;
            if (sprayOrigin != null && hit.collider.transform.IsChildOf(sprayOrigin.root))
                continue;
            DestructibleMeshComponent destructibleMesh = hit.collider.GetComponentInParent<DestructibleMeshComponent>();
            if (destructibleMesh != null && hit.collider.gameObject != destructibleMesh.ReservedSegment)
            {
                destructibleMesh.DestroySegment(hit.collider.gameObject);
                DestructibleMeshHint.NotifyWallDestroyed(); // Duvar ipuçlarını ilk kırılmada kaldır
                if (WeaponManager.Instance != null)
                {
                    WeaponManager.Instance.PlayHitSound();
                    WeaponManager.Instance.TriggerHitHaptic();
                }
                break;
            }
        }
    }

    private void CreateEnergyUI()
    {
        if (uiPosition == null)
        {
            Debug.LogWarning("LastGunFlameSpray: uiPosition çözülemedi. Enerji circle UI oluşturulamıyor.");
            return;
        }

        uiContainer = new GameObject("SprayEnergyCircleUI");
        uiContainer.transform.SetParent(uiPosition, false);
        uiContainer.transform.localPosition = uiOffset;
        uiContainer.transform.localRotation = Quaternion.identity;
        uiContainer.transform.localScale = Vector3.one;

        sprayCanvas = uiContainer.AddComponent<Canvas>();
        sprayCanvas.renderMode = RenderMode.WorldSpace;
        UICameraStackSetup.Instance?.RegisterWorldSpaceCanvas(sprayCanvas);

        RectTransform canvasRect = uiContainer.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 2);
        canvasRect.localScale = Vector3.one * uiSize * 0.01f;

        // 1. Arka plan arc (tam daire)
        GameObject bgArc = CreateArcElement("BgArc", uiContainer.transform, 100);
        backgroundImage = bgArc.GetComponent<Image>();
        backgroundImage.color = uiBgColor;
        backgroundImage.fillAmount = 1f;

        // 2. Progress arc (enerji dolumu)
        GameObject progressArc = CreateArcElement("ProgressArc", uiContainer.transform, 100);
        fillImage = progressArc.GetComponent<Image>();
        fillImage.color = uiColor;
        fillImage.fillAmount = 1f;

        // 3. Glow
        GameObject glowArc = CreateArcElement("GlowArc", uiContainer.transform, 118);
        glowImage = glowArc.GetComponent<Image>();
        glowImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0f);
        glowImage.fillAmount = 1f;
        glowArc.GetComponent<RectTransform>().SetAsFirstSibling();

        // 4. Outer halo
        GameObject halo = CreateArcElement("OuterHalo", uiContainer.transform, 130);
        outerHaloImage = halo.GetComponent<Image>();
        outerHaloImage.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.08f);
        outerHaloImage.fillAmount = 1f;
        halo.GetComponent<RectTransform>().SetAsFirstSibling();

        // 5. Progress dot (dolum ucunda)
        GameObject dot = new GameObject("ProgressDot");
        dot.transform.SetParent(uiContainer.transform, false);
        progressDotImage = dot.AddComponent<Image>();
        progressDotImage.sprite = CreateDotSprite(64);
        progressDotImage.color = new Color(1f, 1f, 1f, 1f);
        progressDotImage.raycastTarget = false;
        progressDotRect = dot.GetComponent<RectTransform>();
        progressDotRect.sizeDelta = new Vector2(10, 10);
        progressDotRect.anchoredPosition = Vector2.zero;
    }

    private GameObject CreateArcElement(string name, Transform parent, float size)
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

    private Sprite CreateRingSprite(int resolution, float thickness)
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
                if (dist >= innerRadius && dist <= outerRadius)
                {
                    float alpha = 1f;
                    if (dist > outerRadius - 1.5f) alpha = Mathf.Clamp01((outerRadius - dist) / 1.5f);
                    else if (dist < innerRadius + 1.5f) alpha = Mathf.Clamp01((dist - innerRadius) / 1.5f);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else texture.SetPixel(x, y, transparent);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
    }

    private Sprite CreateDotSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        float center = resolution / 2f;
        float radius = center - 2f;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = dist <= radius ? 1f : (dist < radius + 2f ? Mathf.Clamp01((radius + 2f - dist) / 2f) : 0f);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
    }

    private void UpdateEnergyUI()
    {
        if (uiContainer == null || fillImage == null) return;

        bool flameSprayUnlimited = TutorialIntroController.TutorialNinthWeaponPhase && !TutorialIntroController.TutorialNinthWeaponDialogue22Complete;
        uiContainer.SetActive(!flameSprayUnlimited);
        if (flameSprayUnlimited) return;

        float percent = maxSprayEnergy > 0f ? currentSprayEnergy / maxSprayEnergy : 1f;

        if (Camera.main != null)
        {
            Vector3 lookDir = uiContainer.transform.position - Camera.main.transform.position;
            if (lookDir.sqrMagnitude > 0.01f)
                uiContainer.transform.rotation = Quaternion.LookRotation(lookDir);
        }

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, percent, Time.deltaTime * 15f);

        float intensity = 0.8f + percent * 0.4f;
        Color currentColor = new Color(uiColor.r * intensity, uiColor.g * intensity, uiColor.b * intensity, uiColor.a);
        fillImage.color = currentColor;

        if (glowImage != null)
        {
            glowImage.fillAmount = fillImage.fillAmount;
            float glowAlpha = percent > 0.5f ? (percent - 0.5f) * 0.6f : 0f;
            if (percent >= 0.99f) glowAlpha = 0.25f + Mathf.Sin(Time.time * 12f) * 0.15f;
            glowImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, glowAlpha);
            glowImage.rectTransform.localScale = percent >= 0.99f ? Vector3.one * (1.1f + Mathf.Sin(Time.time * 12f) * 0.05f) : Vector3.one;
        }

        if (outerHaloImage != null)
        {
            float haloAlpha = 0.06f + percent * 0.14f;
            if (percent >= 0.99f) haloAlpha = 0.15f + Mathf.Sin(Time.time * 10f) * 0.05f;
            outerHaloImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, haloAlpha);
        }

        if (progressDotImage != null && progressDotRect != null)
        {
            float fill = Mathf.Clamp01(fillImage.fillAmount);
            float angleDeg = 90f - fill * 360f;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            float radius = 50f;
            progressDotRect.anchoredPosition = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
            float dotAlpha = Mathf.Clamp01(percent) * 0.9f;
            if (percent >= 0.99f) dotAlpha = 0.8f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotImage.color = new Color(1f, 1f, 1f, dotAlpha);
            float dotScale = 1f + percent * 0.6f;
            if (percent >= 0.99f) dotScale = 1.6f + Mathf.Sin(Time.time * 18f) * 0.2f;
            progressDotRect.localScale = Vector3.one * dotScale;
        }

        if (backgroundImage != null)
            backgroundImage.color = uiBgColor;
    }

    void OnDestroy()
    {
        if (vfxInstance != null)
            Destroy(vfxInstance);
        if (flameLineContainer != null)
            Destroy(flameLineContainer);
        if (uiContainer != null)
            Destroy(uiContainer);
        if (hapticCoroutine != null)
            StopCoroutine(hapticCoroutine);
        OVRInput.Controller c = isLeftHanded ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(0f, 0f, c);
    }

    void OnDisable()
    {
        if (isSpraying)
            StopSpray();
    }
}
