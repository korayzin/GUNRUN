using UnityEngine;

/// <summary>
/// Bullet.cs ile uyumlu, modern çizgisel efektlere sahip Fireball scripti.
/// </summary>
public class FireballEffect : MonoBehaviour
{
    [Tooltip("Bullet.cs bu değişkeni kontrol eder.")]
    public bool enableHitEffect = true;

    [Header("=== TRAIL – Glow & Line ===")]
    [ColorUsage(true, true)] public Color trailCoreColor = new Color(1.6f, 0.95f, 0.4f, 1f);   // Parlak sarı çekirdek (HDR)
    [ColorUsage(true, true)] public Color trailMidColor = new Color(1f, 0.45f, 0.05f, 0.95f);  // Turuncu
    [ColorUsage(true, true)] public Color trailEdgeColor = new Color(0.9f, 0.15f, 0f, 0.5f);   // Kırmızı kenar
    [ColorUsage(true, true)] public Color trailGlowColor = new Color(1f, 0.35f, 0f, 0.4f);    // Dış glow (HDR)
    [Tooltip("Tüm trail boyutlarını çarpan (çizgi kalınlığı + glow + uzunluk). 1 = varsayılan.")]
    [Range(0.25f, 3f)]
    public float trailSizeScale = 1f;
    [Tooltip("İnce çizgi kalınlığı (trailSizeScale ile çarpılır)")]
    [Range(0.02f, 0.2f)]
    public float lineThickness = 0.06f;
    [Tooltip("Glow halo genişliği (trailSizeScale ile çarpılır)")]
    [Range(0.08f, 0.6f)]
    public float glowWidth = 0.22f;
    [Tooltip("Çizgilerin yayıldığı küre yarıçapı")]
    [Range(0.2f, 1f)]
    public float sphereRadius = 0.4f;
    public float rotationSpeed = 280f;
    [Tooltip("Çizgi uzunluğu – trail ne kadar uzun (trailSizeScale ile çarpılır)")]
    [Range(2f, 16f)]
    public float trailLengthScale = 8f;
    [Tooltip("Hareket yönünde uzama miktarı")]
    [Range(0.1f, 0.8f)]
    public float trailVelocityScale = 0.35f;

    [Header("=== HIT EFFECT – Ateş patlaması ===")]
    [Tooltip("Ana patlama parçacık sayısı")]
    public int explosionCount = 100;
    public Vector2 explosionSpeed = new Vector2(6f, 18f);
    public Vector2 explosionSize = new Vector2(0.3f, 0.7f);
    public Vector2 explosionLifetime = new Vector2(0.3f, 0.65f);

    [Header("Kıvılcım / kor parçacıkları")]
    public int sparkCount = 45;
    public Vector2 sparkSpeed = new Vector2(10f, 24f);
    public Vector2 sparkSize = new Vector2(0.05f, 0.14f);

    [Header("Duman")]
    public int smokeCount = 35;
    public Vector2 smokeSpeed = new Vector2(1.5f, 5f);
    public Vector2 smokeSize = new Vector2(0.35f, 0.7f);

    [Header("Ateş renk paleti (HDR)")]
    [ColorUsage(true, true)] public Color fireCore = new Color(1.4f, 0.9f, 0.2f, 1f);   // Sarı çekirdek
    [ColorUsage(true, true)] public Color fireMid = new Color(1f, 0.45f, 0.05f, 1f);   // Turuncu
    [ColorUsage(true, true)] public Color fireOuter = new Color(0.9f, 0.15f, 0f, 0.8f); // Koyu kırmızı
    [ColorUsage(true, true)] public Color smokeColor = new Color(0.25f, 0.08f, 0.02f, 0.6f);

    [Header("Şok dalgası")]
    public bool enableShockwave = true;
    public float shockwaveStartSize = 0.05f;
    public float shockwaveMaxSize = 1.6f;
    public float shockwaveDuration = 0.35f;
    [ColorUsage(true, true)] public Color shockwaveColor = new Color(1f, 0.35f, 0f, 0.75f);

    [Header("Flaş ışığı")]
    public bool enableFlash = true;
    public float flashIntensity = 20f;
    public float flashRange = 5f;
    public float flashDuration = 0.15f;

    public float hitEffectDuration = 1.2f;

    private ParticleSystem _lineParticles;
    private ParticleSystem _glowParticles;
    private static Material _lineMaterial;
    private static Material _glowMaterial;
    private static Material _hitStreakMat;
    private static Material _hitSoftMat;

    private void Start()
    {
        InitializeModernLines();
    }

    /// <summary>Trail objesini UI layer'a alır; passthrough'ta overlay kamerada görünür.</summary>
    private static void SetTrailLayerForPassthrough(GameObject trailRoot)
    {
        if (trailRoot == null) return;
        UICameraStackSetup.SetLayerRecursivelyToUI(trailRoot);
    }

    private void InitializeModernLines()
    {
        // 1) Çekirdek çizgiler – ince, parlak, line hissi
        GameObject lineObj = new GameObject("Fireball_Core_Lines");
        lineObj.transform.SetParent(transform, false);
        _lineParticles = lineObj.AddComponent<ParticleSystem>();
        SetTrailLayerForPassthrough(lineObj);

        var main = _lineParticles.main;
        main.startLifetime = 0.4f;
        main.startSpeed = 0.08f;
        main.startSize = lineThickness * trailSizeScale;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1200;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var emission = _lineParticles.emission;
        emission.rateOverTime = 260;

        var shape = _lineParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = sphereRadius;
        shape.radiusThickness = 0.15f;

        var renderer = lineObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.velocityScale = trailVelocityScale;
        renderer.lengthScale = trailLengthScale * trailSizeScale;
        renderer.material = GetLineMaterial();

        var colorOL = _lineParticles.colorOverLifetime;
        colorOL.enabled = true;
        var lineGrad = new Gradient();
        lineGrad.SetKeys(
            new[] {
                new GradientColorKey(trailCoreColor, 0f),
                new GradientColorKey(trailMidColor, 0.35f),
                new GradientColorKey(trailEdgeColor, 0.85f),
                new GradientColorKey(trailEdgeColor, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.2f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = lineGrad;

        var sizeOL = _lineParticles.sizeOverLifetime;
        sizeOL.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1.1f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(0.7f, 0.6f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var velocity = _lineParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.orbitalZ = 2.2f;

        // 2) Glow katmanı – yumuşak halo, aynı hareket
        GameObject glowObj = new GameObject("Fireball_Glow_Layer");
        glowObj.transform.SetParent(transform, false);
        _glowParticles = glowObj.AddComponent<ParticleSystem>();
        SetTrailLayerForPassthrough(glowObj);

        var mainG = _glowParticles.main;
        mainG.startLifetime = 0.5f;
        mainG.startSpeed = 0.06f;
        mainG.startSize = glowWidth * trailSizeScale;
        mainG.simulationSpace = ParticleSystemSimulationSpace.World;
        mainG.maxParticles = 800;
        mainG.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var emissionG = _glowParticles.emission;
        emissionG.rateOverTime = 180;

        var shapeG = _glowParticles.shape;
        shapeG.shapeType = ParticleSystemShapeType.Sphere;
        shapeG.radius = sphereRadius * 0.9f;
        shapeG.radiusThickness = 0.25f;

        var rendererG = glowObj.GetComponent<ParticleSystemRenderer>();
        rendererG.renderMode = ParticleSystemRenderMode.Stretch;
        rendererG.velocityScale = trailVelocityScale * 0.9f;
        rendererG.lengthScale = (trailLengthScale * 0.85f) * trailSizeScale;
        rendererG.material = GetGlowMaterial();
        rendererG.sortingOrder = -1;

        var colorOLG = _glowParticles.colorOverLifetime;
        colorOLG.enabled = true;
        var glowGrad = new Gradient();
        glowGrad.SetKeys(
            new[] {
                new GradientColorKey(trailGlowColor, 0f),
                new GradientColorKey(trailMidColor, 0.4f),
                new GradientColorKey(trailEdgeColor, 1f)
            },
            new[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0.45f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOLG.color = glowGrad;

        var sizeOLG = _glowParticles.sizeOverLifetime;
        sizeOLG.enabled = true;
        var glowSizeCurve = new AnimationCurve();
        glowSizeCurve.AddKey(0f, 1f);
        glowSizeCurve.AddKey(0.6f, 0.5f);
        glowSizeCurve.AddKey(1f, 0f);
        sizeOLG.size = new ParticleSystem.MinMaxCurve(1f, glowSizeCurve);

        var velocityG = _glowParticles.velocityOverLifetime;
        velocityG.enabled = true;
        velocityG.orbitalZ = 1.8f;
    }

    /// <summary>İnce çizgi: ortada parlak çizgi, kenarlara yumuşak geçiş (glow).</summary>
    private Material GetLineMaterial()
    {
        if (_lineMaterial != null) return _lineMaterial;

        // Doku: kısa eksen (kalınlık) = ortada parlak çizgi + yumuşak kenar; uzun eksen = baştan sona hafif gradient
        int w = 64, h = 128;
        Texture2D tex = new Texture2D(w, h);
        float cx = w * 0.5f, cy = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            float along = (float)y / (h - 1);
            float alongFade = along <= 0.15f ? along / 0.15f : (along >= 0.85f ? (1f - along) / 0.15f : 1f);
            for (int x = 0; x < w; x++)
            {
                float across = Mathf.Abs((x - cx) / cx);
                float core = across <= 0.25f ? 1f : 0f;
                float soft = across <= 0.5f ? 1f : across <= 1f ? Mathf.Lerp(1f, 0f, (across - 0.5f) / 0.5f) : 0f;
                float alpha = Mathf.Lerp(soft * 0.6f, core, 0.7f) * alongFade;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        _lineMaterial = new Material(sh ?? Shader.Find("Sprites/Default"));
        _lineMaterial.mainTexture = tex;
        _lineMaterial.SetInt("_ZWrite", 0);
        if (sh != null && sh.name.Contains("Default"))
        {
            _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
        _lineMaterial.renderQueue = 3000; // Transparent – passthrough overlay'de görünsün
        return _lineMaterial;
    }

    /// <summary>Glow katmanı: tamamen yumuşak, geniş halo.</summary>
    private Material GetGlowMaterial()
    {
        if (_glowMaterial != null) return _glowMaterial;

        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        float c = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / c;
                float dy = (y - c) / c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = d <= 0.3f ? 0.9f : d <= 0.85f ? Mathf.Lerp(0.9f, 0.12f, (d - 0.3f) / 0.55f) : Mathf.Lerp(0.12f, 0f, (d - 0.85f) / 0.15f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        _glowMaterial = new Material(sh ?? Shader.Find("Sprites/Default"));
        _glowMaterial.mainTexture = tex;
        _glowMaterial.SetInt("_ZWrite", 0);
        if (sh != null && sh.name.Contains("Default"))
        {
            _glowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _glowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }
        _glowMaterial.renderQueue = 3000; // Transparent – passthrough overlay'de görünsün
        return _glowMaterial;
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Trail boyutlarını Inspector'dan güncel tut (çizgi kalınlığı, glow, uzunluk)
        if (_lineParticles != null)
        {
            var main = _lineParticles.main;
            main.startSize = lineThickness * trailSizeScale;
            var rend = _lineParticles.GetComponent<ParticleSystemRenderer>();
            rend.lengthScale = trailLengthScale * trailSizeScale;
            rend.velocityScale = trailVelocityScale;
        }
        if (_glowParticles != null)
        {
            var mainG = _glowParticles.main;
            mainG.startSize = glowWidth * trailSizeScale;
            var rendG = _glowParticles.GetComponent<ParticleSystemRenderer>();
            rendG.lengthScale = (trailLengthScale * 0.85f) * trailSizeScale;
            rendG.velocityScale = trailVelocityScale * 0.9f;
        }
    }

    /// <summary>Bullet.cs tarafından çağrılan ateş patlaması hit efekti.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        GameObject root = new GameObject("Fireball_Hit_Explosion");
        root.transform.position = position;

        Material streakMat = CreateHitStreakMaterial();
        Material softMat = CreateHitSoftMaterial();

        CreateFireExplosionBurst(root.transform, streakMat);
        CreateFireSparks(root.transform, streakMat);
        CreateFireSmoke(root.transform, softMat);
        if (enableShockwave)
            CreateFireShockwave(root.transform, softMat);
        if (enableFlash)
            CreateFireFlash(root.transform);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Light flash = root.GetComponentInChildren<Light>();
        if (flash != null)
            root.AddComponent<FireballLightFade>().Setup(flash, flashDuration, flashIntensity);

        Destroy(root, hitEffectDuration);
    }

    private void CreateFireExplosionBurst(Transform parent, Material mat)
    {
        GameObject go = new GameObject("FireBurst");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.05f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(explosionLifetime.x, explosionLifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(explosionSpeed.x, explosionSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(explosionSize.x, explosionSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(fireCore, fireMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = explosionCount;
        main.gravityModifier = 0.04f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, explosionCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.12f, 1.25f);
        sizeCurve.AddKey(0.5f, 0.7f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new[] {
                new GradientColorKey(fireCore, 0f),
                new GradientColorKey(fireMid, 0.25f),
                new GradientColorKey(fireOuter, 0.7f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.95f, 0.25f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = colGrad;

        var velOL = ps.velocityOverLifetime;
        velOL.enabled = true;
        velOL.radial = new ParticleSystem.MinMaxCurve(0.4f, 1.1f);

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 3.5f;
        rend.velocityScale = 0.22f;
    }

    private void CreateFireSparks(Transform parent, Material mat)
    {
        GameObject go = new GameObject("FireSparks");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(sparkSpeed.x, sparkSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(sparkSize.x, sparkSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(fireCore, fireMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = sparkCount;
        main.gravityModifier = 0.12f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.01f, sparkCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sparkCurve = new AnimationCurve();
        sparkCurve.AddKey(0f, 1f);
        sparkCurve.AddKey(0.55f, 0.45f);
        sparkCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sparkCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var sparkGrad = new Gradient();
        sparkGrad.SetKeys(
            new[] {
                new GradientColorKey(fireCore, 0f),
                new GradientColorKey(fireMid, 0.35f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.75f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = sparkGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 2.5f;
        rend.velocityScale = 0.18f;
    }

    private void CreateFireSmoke(Transform parent, Material mat)
    {
        GameObject go = new GameObject("FireSmoke");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.06f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(smokeSpeed.x, smokeSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(smokeSize.x, smokeSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(smokeColor, fireOuter);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = smokeCount;
        main.gravityModifier = -0.025f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.02f, smokeCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.04f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var smokeCurve = new AnimationCurve();
        smokeCurve.AddKey(0f, 0.5f);
        smokeCurve.AddKey(0.2f, 1.35f);
        smokeCurve.AddKey(0.65f, 1.1f);
        smokeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var smokeGrad = new Gradient();
        smokeGrad.SetKeys(
            new[] {
                new GradientColorKey(smokeColor, 0f),
                new GradientColorKey(fireOuter, 0.45f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.65f, 0f),
                new GradientAlphaKey(0.45f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = smokeGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void CreateFireShockwave(Transform parent, Material mat)
    {
        GameObject go = new GameObject("FireShockwave");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = shockwaveDuration;
        main.startSpeed = 0f;
        main.startSize = shockwaveStartSize;
        main.startColor = shockwaveColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var ringCurve = new AnimationCurve();
        ringCurve.AddKey(0f, 1f);
        ringCurve.AddKey(0.05f, 1.35f);
        ringCurve.AddKey(1f, shockwaveMaxSize / shockwaveStartSize);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, ringCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var ringGrad = new Gradient();
        ringGrad.SetKeys(
            new[] {
                new GradientColorKey(shockwaveColor, 0f),
                new GradientColorKey(fireMid, 0.25f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.5f, 0.12f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = ringGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void CreateFireFlash(Transform parent)
    {
        GameObject go = new GameObject("FireFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.5f, 0.15f);
        light.intensity = flashIntensity;
        light.range = flashRange;
    }

    private static Material CreateHitStreakMaterial()
    {
        if (_hitStreakMat != null) return _hitStreakMat;

        int w = 128, h = 32;
        Texture2D tex = new Texture2D(w, h);
        float cx = w * 0.5f, cy = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy * 4f);
                float alpha = d <= 0.4f ? 1f : d <= 0.9f ? Mathf.Lerp(1f, 0.1f, (d - 0.4f) / 0.5f) : Mathf.Lerp(0.1f, 0f, (d - 0.9f) / 0.1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        _hitStreakMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _hitStreakMat.mainTexture = tex;
        _hitStreakMat.SetInt("_ZWrite", 0);
        return _hitStreakMat;
    }

    private static Material CreateHitSoftMaterial()
    {
        if (_hitSoftMat != null) return _hitSoftMat;

        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        float cx = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cx) / cx;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float hash = ((x * 7 + y * 13) % 100) / 100f - 0.5f;
                float edge = 0.88f + hash * 0.12f;
                float alpha = d <= 0.5f ? 0.98f : d <= edge ? Mathf.Lerp(0.98f, 0.15f, (d - 0.5f) / (edge - 0.5f)) : Mathf.Lerp(0.15f, 0f, (d - edge) / (1f - edge));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha * alpha)));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        _hitSoftMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _hitSoftMat.mainTexture = tex;
        _hitSoftMat.SetInt("_ZWrite", 0);
        return _hitSoftMat;
    }
}

/// <summary>Fireball hit efektindeki flaş ışığının kısa sürede sönmesi için.</summary>
public class FireballLightFade : MonoBehaviour
{
    private Light _light;
    private float _duration;
    private float _initialIntensity;
    private float _elapsed;

    public void Setup(Light l, float dur, float initialIntensity)
    {
        _light = l;
        _duration = dur;
        _initialIntensity = initialIntensity;
        _elapsed = 0f;
    }

    private void Update()
    {
        if (_light == null) return;
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        _light.intensity = _initialIntensity * Mathf.Max(0f, 1f - t);
    }
}