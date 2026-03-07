using UnityEngine;

/// <summary>
/// SixthAmmo roket mermisi için patlama temalı hit efekti.
/// Roket çarpması: enerji patlaması, kıvılcımlar, duman, güçlü şok dalgası.
/// Renk paleti SharkBulletAnimation ile uyumlu (parlak mavi/cyan).
/// 1 sn hit effect cooldown – ardışık vuruşlarda efekt spawn edilmez.
/// </summary>
public class SixthAmmoHitEffect : MonoBehaviour
{
    [Tooltip("Düşmana değince roket hit efekti aktif mi?")]
    public bool enableHitEffect = true;

    [Tooltip("Hit effect cooldown (sn) – aynı silahtan ardışık vuruşlarda efekt spawn edilmez")]
    public float hitEffectCooldown = 1f;

    [Header("Ana Patlama")]
    [Tooltip("Ana patlama parçacık sayısı")]
    public int explosionCount = 120;
    [Tooltip("Parçacık hızı min-max (sert, hızlı)")]
    public Vector2 explosionSpeed = new Vector2(8f, 22f);
    [Tooltip("Parçacık boyutu min-max")]
    public Vector2 explosionSize = new Vector2(0.25f, 0.6f);
    [Tooltip("Parçacık ömür süresi")]
    public Vector2 explosionLifetime = new Vector2(0.35f, 0.7f);

    [Header("Kıvılcımlar")]
    public int sparkCount = 50;
    public Vector2 sparkSpeed = new Vector2(12f, 28f);
    public Vector2 sparkSize = new Vector2(0.04f, 0.12f);

    [Header("Duman")]
    public int smokeCount = 40;
    public Vector2 smokeSpeed = new Vector2(2f, 6f);
    public Vector2 smokeSize = new Vector2(0.3f, 0.65f);

    [Header("Renk Paleti (SharkBulletAnimation ile uyumlu)")]
    [Tooltip("Parlak çekirdek – trailCoreColor (0.6, 0.9, 1)")]
    public Color fireCore = new Color(0.6f, 0.9f, 1f, 1f);
    [Tooltip("Orta ton – particleColor (0.5, 0.85, 1)")]
    public Color fireMid = new Color(0.5f, 0.85f, 1f, 0.95f);
    [Tooltip("Enerji vurgusu – auraColor/glowColor (0.4, 0.8, 1)")]
    public Color energyAccent = new Color(0.4f, 0.8f, 1f, 0.9f);
    [Tooltip("Koyu dış ton – trailMainColor (0.2, 0.6, 1)")]
    public Color fireOuter = new Color(0.2f, 0.6f, 1f, 0.7f);
    [Tooltip("Duman rengi – trailGlowColor benzeri koyu mavi")]
    public Color smokeColor = new Color(0.1f, 0.35f, 0.85f, 0.5f);

    [Header("Şok Dalgası")]
    public bool enableShockwave = true;
    public float shockwaveStartSize = 0.05f;
    public float shockwaveMaxSize = 1.8f;
    public float shockwaveDuration = 0.4f;
    public Color shockwaveColor = new Color(0.4f, 0.8f, 1f, 0.7f);

    [Header("Flaş Işığı")]
    public bool enableFlash = true;
    public float flashIntensity = 25f;
    public float flashRange = 5f;
    public float flashDuration = 0.12f;

    [Tooltip("Efekt nesnesinin yok edilme süresi (sn)")]
    public float effectDuration = 1.5f;

    private static float _lastHitEffectTime = float.MinValue;

    /// <summary>Bullet.cs düşmana çarptığında bu metodu çağırır.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        float now = Time.time;
        if (now - _lastHitEffectTime < hitEffectCooldown) return;
        _lastHitEffectTime = now;

        GameObject root = new GameObject("SixthAmmo_RocketHitEffect");
        root.transform.position = position;

        Material streakMat = CreateStreakMaterial();
        Material softMat = CreateSoftMaterial();

        // 1) Ana patlama – streak parçacıklar, sert ve hızlı
        CreateExplosionBurst(root.transform, streakMat);

        // 2) Kıvılcımlar – hızlı uçan parçacıklar
        CreateSparks(root.transform, streakMat);

        // 3) Duman – derinlik ve patlama hissi
        CreateSmoke(root.transform, softMat);

        // 4) Şok dalgası – güçlü genişleyen halka
        if (enableShockwave)
            CreateShockwave(root.transform, softMat);

        // 5) Flaş ışığı
        if (enableFlash)
            CreateFlashLight(root.transform);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Light flash = root.GetComponentInChildren<Light>();
        if (flash != null)
            root.AddComponent<SixthAmmoLightFade>().Setup(flash, flashDuration, flashIntensity);

        Destroy(root, effectDuration);
    }

    private void CreateExplosionBurst(Transform parent, Material mat)
    {
        GameObject go = new GameObject("ExplosionBurst");
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
        main.gravityModifier = 0.05f;
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
        sizeCurve.AddKey(0.15f, 1.2f);
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
                new GradientColorKey(energyAccent, 0.5f),
                new GradientColorKey(fireOuter, 0.9f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.95f, 0.3f),
                new GradientAlphaKey(0.7f, 0.6f),
                new GradientAlphaKey(0.2f, 0.9f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = colGrad;

        var velOL = ps.velocityOverLifetime;
        velOL.enabled = true;
        velOL.radial = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 3.5f;
        rend.velocityScale = 0.2f;
    }

    private void CreateSparks(Transform parent, Material mat)
    {
        GameObject go = new GameObject("Sparks");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(sparkSpeed.x, sparkSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(sparkSize.x, sparkSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(fireCore, fireMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = sparkCount;
        main.gravityModifier = 0.15f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.01f, sparkCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sparkCurve = new AnimationCurve();
        sparkCurve.AddKey(0f, 1f);
        sparkCurve.AddKey(0.6f, 0.4f);
        sparkCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sparkCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var sparkGrad = new Gradient();
        sparkGrad.SetKeys(
            new[] {
                new GradientColorKey(fireCore, 0f),
                new GradientColorKey(fireMid, 0.4f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
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

    private void CreateSmoke(Transform parent, Material mat)
    {
        GameObject go = new GameObject("Smoke");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.08f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(smokeSpeed.x, smokeSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(smokeSize.x, smokeSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(smokeColor, fireOuter);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = smokeCount;
        main.gravityModifier = -0.03f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.02f, smokeCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var smokeCurve = new AnimationCurve();
        smokeCurve.AddKey(0f, 0.5f);
        smokeCurve.AddKey(0.2f, 1.3f);
        smokeCurve.AddKey(0.7f, 1.1f);
        smokeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, smokeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var smokeGrad = new Gradient();
        smokeGrad.SetKeys(
            new[] {
                new GradientColorKey(smokeColor, 0f),
                new GradientColorKey(fireOuter, 0.5f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0.5f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = smokeGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void CreateShockwave(Transform parent, Material mat)
    {
        GameObject go = new GameObject("Shockwave");
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
        ringCurve.AddKey(0.05f, 1.4f);
        ringCurve.AddKey(1f, shockwaveMaxSize / shockwaveStartSize);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, ringCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var ringGrad = new Gradient();
        ringGrad.SetKeys(
            new[] {
                new GradientColorKey(shockwaveColor, 0f),
                new GradientColorKey(energyAccent, 0.3f),
                new GradientColorKey(fireOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0.6f, 0.15f),
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
        GameObject go = new GameObject("ExplosionFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.4f, 0.8f, 1f); // SharkBulletAnimation glowColor
        light.intensity = flashIntensity;
        light.range = flashRange;
    }

    private static Material _streakMat;
    private static Material _softMat;

    private static Material CreateStreakMaterial()
    {
        if (_streakMat != null) return _streakMat;

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
        _streakMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _streakMat.mainTexture = tex;
        _streakMat.SetInt("_ZWrite", 0);
        return _streakMat;
    }

    private static Material CreateSoftMaterial()
    {
        if (_softMat != null) return _softMat;

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
        _softMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _softMat.mainTexture = tex;
        _softMat.SetInt("_ZWrite", 0);
        return _softMat;
    }
}

/// <summary>Flaş ışığının hızlıca sönmesi için yardımcı.</summary>
public class SixthAmmoLightFade : MonoBehaviour
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
