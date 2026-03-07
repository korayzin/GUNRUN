using UnityEngine;

/// <summary>
/// EightAmmo mermisi için cartoon toz/duman puff hit efekti.
/// Yuvarlak veya çizgisel değil: yumuşak, düzensiz kenarlı "toz bulutu" – noise'lı blob hissi.
/// EightAmmoAnimation (turuncu/sarı cartoon trail) ile uyumlu palet.
/// Bullet.cs düşmana çarptığında SpawnHitEffect çağırır.
/// </summary>
public class EightAmmoHitEffect : MonoBehaviour
{
    [Tooltip("Düşmana değince cartoon puff hit efekti aktif mi?")]
    public bool enableHitEffect = true;

    [Tooltip("Ardışık vuruşlarda efekt spawn cooldown (sn)")]
    public float hitEffectCooldown = 0.4f;

    [Header("Ana Toz Puff")]
    [Tooltip("Ana puff parçacık sayısı (düzensiz bulut)")]
    public int puffCount = 45;
    [Tooltip("Parçacık hızı min-max – yumuşak yayılma")]
    public Vector2 puffSpeed = new Vector2(1.5f, 4f);
    [Tooltip("Parçacık boyutu min-max")]
    public Vector2 puffSize = new Vector2(0.35f, 0.7f);
    [Tooltip("Parçacık ömür süresi")]
    public Vector2 puffLifetime = new Vector2(0.4f, 0.75f);

    [Header("İkincil Puff (derinlik)")]
    public int secondaryPuffCount = 25;
    public Vector2 secondaryPuffSpeed = new Vector2(0.8f, 2.5f);
    public Vector2 secondaryPuffSize = new Vector2(0.2f, 0.45f);

    [Header("Renk Paleti (EightAmmo – cartoon turuncu/sarı)")]
    [Tooltip("Açık toz / kremsi")]
    public Color puffLight = new Color(1f, 0.92f, 0.75f, 0.9f);
    [Tooltip("Orta ton – turuncu vurgu")]
    public Color puffMid = new Color(1f, 0.7f, 0.4f, 0.85f);
    [Tooltip("Koyu / gölge")]
    public Color puffDark = new Color(0.75f, 0.5f, 0.25f, 0.5f);

    [Header("Flaş (isteğe bağlı)")]
    public bool enableFlash = true;
    public float flashIntensity = 8f;
    public float flashRange = 3f;
    public float flashDuration = 0.08f;

    [Tooltip("Efekt root yok edilme süresi (sn)")]
    public float effectDuration = 1f;

    private static float _lastHitEffectTime = float.MinValue;

    /// <summary>Bullet.cs düşmana çarptığında bu metodu çağırır.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        float now = Time.time;
        if (now - _lastHitEffectTime < hitEffectCooldown) return;
        _lastHitEffectTime = now;

        GameObject root = new GameObject("EightAmmo_PuffHitEffect");
        root.transform.position = position;

        Material puffMat = CreatePuffMaterial();

        CreateDustPuff(root.transform, puffMat, puffCount, puffSpeed, puffSize, puffLifetime, puffLight, puffMid);
        CreateDustPuff(root.transform, puffMat, secondaryPuffCount, secondaryPuffSpeed, secondaryPuffSize,
            new Vector2(puffLifetime.x * 0.8f, puffLifetime.y * 0.9f), puffMid, puffDark);

        if (enableFlash)
            CreateFlashLight(root.transform);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Light flash = root.GetComponentInChildren<Light>();
        if (flash != null)
            root.AddComponent<EightAmmoLightFade>().Setup(flash, flashDuration, flashIntensity);

        Destroy(root, effectDuration);
    }

    private void CreateDustPuff(Transform parent, Material mat, int count, Vector2 speed, Vector2 size, Vector2 lifetime, Color colorStart, Color colorEnd)
    {
        GameObject go = new GameObject("DustPuff");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.06f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
        main.startColor = new ParticleSystem.MinMaxGradient(colorStart, colorEnd);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startRotation3D = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = count;
        main.gravityModifier = -0.02f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

        // Hemisphere – yarı küre; tek yöne yayılma hissi, tam yuvarlak değil
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.04f;
        shape.radiusThickness = 0f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var curve = new AnimationCurve();
        curve.AddKey(0f, 0.4f);
        curve.AddKey(0.15f, 1.1f);
        curve.AddKey(0.5f, 1.05f);
        curve.AddKey(0.85f, 0.5f);
        curve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, curve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(colorStart, 0f),
                new GradientColorKey(colorEnd, 0.5f),
                new GradientColorKey(puffDark, 1f)
            },
            new[] {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = grad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void CreateFlashLight(Transform parent)
    {
        GameObject go = new GameObject("PuffFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.75f, 0.45f);
        light.intensity = flashIntensity;
        light.range = flashRange;
    }

    /// <summary>Düzensiz kenarlı blob – yuvarlak değil, noise ile asymetrik.</summary>
    private static Material _puffMat;

    private static Material CreatePuffMaterial()
    {
        if (_puffMat != null) return _puffMat;

        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        float cx = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - cx) / cx;
                float ny = (y - cx) / cx;
                // Düzensiz "mesafe": açıya göre değişen yarıçap (wobbly blob)
                float angle = Mathf.Atan2(ny, nx);
                float wobble = 0.85f + 0.2f * Mathf.Sin(angle * 5f) + 0.1f * Mathf.Sin(angle * 11f);
                float d = Mathf.Sqrt(nx * nx + ny * ny) / wobble;
                // Kenar noise – pürüzlü, yumuşak geçiş
                int ix = x % 16; int iy = y % 16;
                float noise = ((ix * 7 + iy * 13) % 100) / 100f;
                float edge = 0.7f + noise * 0.25f;
                float alpha = d <= 0.4f ? 0.95f : d <= edge ? Mathf.Lerp(0.95f, 0.12f, (d - 0.4f) / (edge - 0.4f)) : Mathf.Lerp(0.12f, 0f, (d - edge) / (1.1f - edge));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Alpha Blended") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
        _puffMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _puffMat.mainTexture = tex;
        if (_puffMat.HasProperty("_ZWrite")) _puffMat.SetInt("_ZWrite", 0);
        return _puffMat;
    }
}

/// <summary>Flaş ışığının hızlıca sönmesi için yardımcı.</summary>
public class EightAmmoLightFade : MonoBehaviour
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
