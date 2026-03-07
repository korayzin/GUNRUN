using UnityEngine;

/// <summary>
/// FifthAmmo mermisi için elektrik temalı hit efekti.
/// Diğer hit effectlerden farklı: parlak elektrik arkı, mavi/cyan ışık, yıldırım benzeri parçacıklar.
/// Bullet.cs düşmana çarptığında SpawnHitEffect çağırır.
/// </summary>
public class FifthAmmoHitEffect : MonoBehaviour
{
    [Tooltip("Düşmana değince elektrik hit efekti aktif mi?")]
    public bool enableHitEffect = true;

    [Header("Elektrik Patlama")]
    [Tooltip("Ana elektrik parçacık sayısı")]
    public int explosionParticleCount = 100;
    [Tooltip("Parçacık hızı min-max")]
    public Vector2 explosionSpeed = new Vector2(8f, 18f);
    [Tooltip("Parçacık boyutu min-max")]
    public Vector2 explosionSize = new Vector2(0.25f, 0.55f);
    [Tooltip("Parçacık ömür süresi min-max")]
    public Vector2 explosionLifetime = new Vector2(0.4f, 0.7f);

    [Header("Elektrik Renk Paleti")]
    [Tooltip("Çekirdek (parlak beyaz)")]
    public Color fireCore = new Color(1f, 1f, 1f, 1f);
    [Tooltip("Orta ton (elektrik mavisi/cyan)")]
    public Color fireMid = new Color(0.4f, 0.75f, 1f, 0.95f);
    [Tooltip("Dış ton (koyu mavi glow)")]
    public Color fireOuter = new Color(0.15f, 0.35f, 0.9f, 0.8f);
    [Tooltip("İyonize hava / discharge tonu")]
    public Color smokeColor = new Color(0.5f, 0.7f, 1f, 0.4f);

    [Header("Elektrik Nabız Halkası")]
    public bool enableShockwaveRing = true;
    public float shockwaveStartSize = 0.1f;
    public float shockwaveMaxSize = 1.2f;
    public float shockwaveDuration = 0.5f;
    public Color shockwaveColor = new Color(0.3f, 0.7f, 1f, 0.6f);

    [Header("Elektrik Flaş Işığı")]
    public bool enableFlashLight = true;
    public float flashIntensity = 15f;
    public float flashRange = 4f;
    public float flashDuration = 0.15f;

    [Header("Ek Parçacıklar")]
    [Tooltip("İyonize hava parçacıkları")]
    public int smokeParticleCount = 35;
    [Tooltip("Elektrik kıvılcımları")]
    public int sparkParticleCount = 30;

    [Tooltip("Efekt nesnesinin yok edilme süresi (sn)")]
    public float effectDuration = 1.2f;

    /// <summary>Bullet.cs düşmana çarptığında bu metodu çağırır.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        GameObject root = new GameObject("FifthAmmo_ElectricHitEffect");
        root.transform.position = position;

        Material streakMat = CreateStreakMaterial();
        Material ringMat = CreateElectricMaterial();

        // 1) Ana elektrik patlaması – çizgisel/streak parçacıklar, parlak
        CreateElectricBurst(root.transform, streakMat);

        // 2) Elektrik kıvılcımları – çizgisel kıvılcımlar
        CreateElectricSparks(root.transform, streakMat);

        // 3) İyonize hava / discharge katmanı – çizgisel
        CreateDischargeLayer(root.transform, streakMat);

        // 4) Elektrik nabız halkası (yuvarlak kalır)
        if (enableShockwaveRing)
            CreateElectricPulseRing(root.transform, ringMat);

        // 5) Flaş ışığı
        if (enableFlashLight)
            CreateFlashLight(root.transform);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Light flash = root.GetComponentInChildren<Light>();
        if (flash != null)
            root.AddComponent<FifthAmmoFlashFade>().Setup(flash, flashDuration, flashIntensity);

        Destroy(root, effectDuration);
    }

    private void CreateElectricBurst(Transform parent, Material mat)
    {
        GameObject go = new GameObject("ElectricBurst");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.06f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(explosionLifetime.x, explosionLifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(explosionSpeed.x, explosionSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(explosionSize.x, explosionSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(fireCore, fireMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = explosionParticleCount;
        main.gravityModifier = -0.02f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, explosionParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.04f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.2f, 1.1f);
        sizeCurve.AddKey(0.6f, 0.6f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new[] {
                new GradientColorKey(fireCore, 0f),
                new GradientColorKey(fireMid, 0.35f),
                new GradientColorKey(fireOuter, 0.8f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.95f, 0.4f),
                new GradientAlphaKey(0.7f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = colGrad;

        var velOL = ps.velocityOverLifetime;
        velOL.enabled = true;
        velOL.radial = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 3f;
        rend.velocityScale = 0.15f;
    }

    private void CreateElectricSparks(Transform parent, Material mat)
    {
        GameObject go = new GameObject("ElectricSparks");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(fireCore, fireMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = sparkParticleCount;
        main.gravityModifier = 0.1f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.02f, sparkParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sparkCurve = new AnimationCurve();
        sparkCurve.AddKey(0f, 1f);
        sparkCurve.AddKey(0.5f, 0.5f);
        sparkCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sparkCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var sparkGrad = new Gradient();
        sparkGrad.SetKeys(
            new[] {
                new GradientColorKey(fireCore, 0f),
                new GradientColorKey(fireMid, 0.5f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = sparkGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 2.5f;
        rend.velocityScale = 0.15f;
    }

    private void CreateDischargeLayer(Transform parent, Material mat)
    {
        GameObject go = new GameObject("DischargeLayer");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.08f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.65f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startColor = new ParticleSystem.MinMaxGradient(smokeColor, fireOuter);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = smokeParticleCount;
        main.gravityModifier = -0.08f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.03f, smokeParticleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var dischargeCurve = new AnimationCurve();
        dischargeCurve.AddKey(0f, 0.5f);
        dischargeCurve.AddKey(0.2f, 1.2f);
        dischargeCurve.AddKey(0.7f, 1f);
        dischargeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, dischargeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var dischargeGrad = new Gradient();
        dischargeGrad.SetKeys(
            new[] {
                new GradientColorKey(smokeColor, 0f),
                new GradientColorKey(fireOuter, 0.6f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.75f, 0f),
                new GradientAlphaKey(0.55f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = dischargeGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 2f;
        rend.velocityScale = 0.1f;
    }

    private void CreateElectricPulseRing(Transform parent, Material mat)
    {
        GameObject go = new GameObject("ElectricPulseRing");
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
        ringCurve.AddKey(0.1f, 1.3f);
        ringCurve.AddKey(1f, shockwaveMaxSize / shockwaveStartSize);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, ringCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var ringGrad = new Gradient();
        ringGrad.SetKeys(
            new[] {
                new GradientColorKey(shockwaveColor, 0f),
                new GradientColorKey(fireMid, 0.4f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0.5f, 0.25f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = ringGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void CreateFlashLight(Transform parent)
    {
        GameObject go = new GameObject("ElectricFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.5f, 0.8f, 1f);
        light.intensity = flashIntensity;
        light.range = flashRange;
    }

    private static Material _electricMat;
    private static Material _streakMat;

    /// <summary>Çizgisel/streak parçacıklar için uzun, ince texture – parlak merkez, yumuşak kenar.</summary>
    private static Material CreateStreakMaterial()
    {
        if (_streakMat != null) return _streakMat;

        int w = 128;
        int h = 32;
        Texture2D tex = new Texture2D(w, h);
        float cx = w * 0.5f;
        float cy = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy * 4f);
                float alpha;
                if (d <= 0.4f)
                    alpha = 1f;
                else if (d <= 0.9f)
                    alpha = Mathf.Lerp(1f, 0.1f, (d - 0.4f) / 0.5f);
                else
                    alpha = Mathf.Lerp(0.1f, 0f, (d - 0.9f) / 0.1f);
                alpha = Mathf.Clamp01(alpha);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
            ?? Shader.Find("Particles/Additive")
            ?? Shader.Find("Sprites/Default");
        _streakMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _streakMat.mainTexture = tex;
        _streakMat.SetInt("_ZWrite", 0);
        return _streakMat;
    }

    private static Material CreateElectricMaterial()
    {
        if (_electricMat != null) return _electricMat;

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
                float noise = hash * 0.12f;
                float edge = 0.88f + noise;
                float alpha;
                if (d <= 0.5f)
                    alpha = 0.98f;
                else if (d <= edge)
                    alpha = Mathf.Lerp(0.98f, 0.15f, (d - 0.5f) / (edge - 0.5f));
                else
                    alpha = Mathf.Lerp(0.15f, 0f, (d - edge) / (1f - edge));
                alpha = Mathf.Clamp01(alpha);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
            ?? Shader.Find("Particles/Additive")
            ?? Shader.Find("Sprites/Default");
        _electricMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _electricMat.mainTexture = tex;
        _electricMat.SetInt("_ZWrite", 0);
        return _electricMat;
    }
}

/// <summary>Flaş ışığının hızlıca sönmesi için yardımcı.</summary>
public class FifthAmmoFlashFade : MonoBehaviour
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
