using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// OctopusAmmo hit efekti – klasik particle değil: streak mürekkep, LineRenderer halka ve ink jet çizgileri.
/// OctopusBulletAnimation ile uyumlu mor palet.
/// </summary>
public class OctopusAmmoHitEffect : MonoBehaviour
{
    [Tooltip("Düşmana değince ahtapot hit efekti aktif mi?")]
    public bool enableHitEffect = true;

    [Tooltip("Ardışık vuruşlarda efekt spawn cooldown (sn)")]
    public float hitEffectCooldown = 0.5f;

    [Header("Mürekkep Streak'leri (çizgi formunda parçacıklar)")]
    public int inkStreakCount = 50;
    public Vector2 inkSpeed = new Vector2(6f, 16f);
    public Vector2 inkStreakLength = new Vector2(0.35f, 0.7f);
    public Vector2 inkLifetime = new Vector2(0.3f, 0.55f);

    [Header("Ink Jet Çizgileri (merkezden dışa çizgiler)")]
    public bool enableInkJetLines = true;
    public int inkJetCount = 8;
    public float inkJetLength = 0.5f;
    public float inkJetGrowTime = 0.06f;
    public float inkJetWidth = 0.04f;

    [Header("Genişleyen Halka (LineRenderer – tek parçacık değil)")]
    public bool enableInkRing = true;
    public float inkRingMaxRadius = 1.2f;
    public float inkRingDuration = 0.4f;
    public int inkRingSegments = 64;
    public float inkRingLineWidth = 0.03f;

    [Header("Renk Paleti")]
    public Color inkCore = new Color(0.95f, 0.75f, 1f, 1f);
    public Color inkMid = new Color(0.55f, 0.25f, 0.9f, 0.95f);
    public Color inkOuter = new Color(0.35f, 0.12f, 0.55f, 0.6f);
    public Color inkRingColor = new Color(0.5f, 0.2f, 0.85f, 0.9f);

    [Header("Flaş")]
    public bool enableFlash = true;
    public float flashIntensity = 14f;
    public float flashRange = 4f;
    public float flashDuration = 0.1f;

    [Tooltip("Efekt root yok edilme süresi (sn)")]
    public float effectDuration = 1.1f;

    private static float _lastHitEffectTime = float.MinValue;

    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        float now = Time.time;
        if (now - _lastHitEffectTime < hitEffectCooldown) return;
        _lastHitEffectTime = now;

        GameObject root = new GameObject("OctopusAmmo_HitEffect");
        root.transform.position = position;

        Material streakMat = CreateStreakMaterial();

        CreateInkStreaks(root.transform, streakMat);

        if (enableInkJetLines)
            CreateInkJetLines(root.transform);

        if (enableInkRing)
            CreateInkRing(root.transform);

        if (enableFlash)
            CreateFlashLight(root.transform);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();

        Light flash = root.GetComponentInChildren<Light>();
        if (flash != null)
            root.AddComponent<OctopusAmmoLightFade>().Setup(flash, flashDuration, flashIntensity);

        Destroy(root, effectDuration);
    }

    private void CreateInkStreaks(Transform parent, Material mat)
    {
        GameObject go = new GameObject("InkStreaks");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(inkLifetime.x, inkLifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(inkSpeed.x, inkSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(inkStreakLength.x, inkStreakLength.y);
        main.startColor = new ParticleSystem.MinMaxGradient(inkCore, inkMid);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = inkStreakCount;
        main.gravityModifier = -0.06f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, inkStreakCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.15f, 1.1f);
        sizeCurve.AddKey(0.6f, 0.5f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        Gradient colGrad = new Gradient();
        colGrad.SetKeys(
            new[] {
                new GradientColorKey(inkCore, 0f),
                new GradientColorKey(inkMid, 0.3f),
                new GradientColorKey(inkOuter, 0.85f),
                new GradientColorKey(inkOuter, 1f)
            },
            new[] {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0.8f, 0.4f),
                new GradientAlphaKey(0.15f, 0.85f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = colGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Stretch;
        rend.lengthScale = 2.8f;
        rend.velocityScale = 0.25f;
    }

    private void CreateInkJetLines(Transform parent)
    {
        GameObject container = new GameObject("InkJetLines");
        container.transform.SetParent(parent);
        container.transform.localPosition = Vector3.zero;

        OctopusInkJetAnimator anim = container.AddComponent<OctopusInkJetAnimator>();
        anim.Setup(inkJetCount, inkJetLength, inkJetGrowTime, inkJetWidth, inkCore, inkMid, effectDuration);
    }

    private void CreateInkRing(Transform parent)
    {
        GameObject go = new GameObject("InkRing");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        OctopusInkRingAnimator anim = go.AddComponent<OctopusInkRingAnimator>();
        anim.Setup(inkRingMaxRadius, inkRingDuration, inkRingSegments, inkRingLineWidth, inkRingColor, inkOuter);
    }

    private void CreateFlashLight(Transform parent)
    {
        GameObject go = new GameObject("OctopusFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        Light light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.6f, 0.3f, 1f);
        light.intensity = flashIntensity;
        light.range = flashRange;
    }

    private static Material _streakMat;

    private static Material CreateStreakMaterial()
    {
        if (_streakMat != null) return _streakMat;

        int w = 128, h = 28;
        Texture2D tex = new Texture2D(w, h);
        float cx = w * 0.5f, cy = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy * 5f);
                float alpha = d <= 0.35f ? 1f : d <= 0.85f ? Mathf.Lerp(1f, 0.08f, (d - 0.35f) / 0.5f) : Mathf.Lerp(0.08f, 0f, (d - 0.85f) / 0.15f);
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
}

/// <summary>Merkezden dışa büyüyen ink jet çizgileri – LineRenderer ile.</summary>
public class OctopusInkJetAnimator : MonoBehaviour
{
    private List<LineRenderer> _lines = new List<LineRenderer>();
    private List<Vector3> _directions = new List<Vector3>();
    private float _length;
    private float _growTime;
    private float _width;
    private Color _colorStart;
    private Color _colorEnd;
    private float _duration;
    private float _elapsed;

    public void Setup(int count, float length, float growTime, float width, Color colorStart, Color colorEnd, float duration)
    {
        _length = length;
        _growTime = growTime;
        _width = width;
        _colorStart = colorStart;
        _colorEnd = colorEnd;
        _duration = duration;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = colorStart;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i + Random.Range(-12f, 12f);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            _directions.Add(dir);

            GameObject lineObj = new GameObject("Jet");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position);
            lr.material = new Material(mat);
            lr.startWidth = width;
            lr.endWidth = width * 0.3f;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;

            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(colorStart, 0f), new GradientColorKey(colorEnd, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.4f, 1f) }
            );
            lr.colorGradient = g;

            _lines.Add(lr);
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        Vector3 center = transform.position;

        float growT = Mathf.Clamp01(_elapsed / _growTime);
        float lengthNow = _length * growT;
        float fadeT = _elapsed / _duration;
        float alpha = Mathf.Clamp01(1f - fadeT * 1.8f);

        for (int i = 0; i < _lines.Count; i++)
        {
            LineRenderer lr = _lines[i];
            if (lr == null) continue;

            Vector3 dir = i < _directions.Count ? _directions[i] : Vector3.right;
            lr.SetPosition(0, center);
            lr.SetPosition(1, center + dir * lengthNow);

            Gradient g = lr.colorGradient;
            GradientAlphaKey[] aKeys = g.alphaKeys;
            for (int k = 0; k < aKeys.Length; k++)
                aKeys[k] = new GradientAlphaKey(aKeys[k].alpha * alpha, aKeys[k].time);
            lr.colorGradient = new Gradient { colorKeys = g.colorKeys, alphaKeys = aKeys };
        }
    }
}

/// <summary>LineRenderer ile çizilen genişleyen halka – particle değil.</summary>
public class OctopusInkRingAnimator : MonoBehaviour
{
    private LineRenderer _line;
    private float _maxRadius;
    private float _duration;
    private int _segments;
    private float _width;
    private Color _colorStart;
    private Color _colorEnd;
    private float _elapsed;
    private Vector3[] _positions;

    public void Setup(float maxRadius, float duration, int segments, float lineWidth, Color colorStart, Color colorEnd)
    {
        _maxRadius = maxRadius;
        _duration = duration;
        _segments = segments;
        _width = lineWidth;

        _positions = new Vector3[segments + 1];

        GameObject lineObj = new GameObject("Ring");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;

        _line = lineObj.AddComponent<LineRenderer>();
        _line.useWorldSpace = true;
        _line.loop = true;
        _line.positionCount = segments + 1;
        _line.startWidth = lineWidth;
        _line.endWidth = lineWidth * 0.6f;
        _line.numCapVertices = 4;
        _line.numCornerVertices = 4;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = colorStart;
        _line.material = mat;

        _colorStart = colorStart;
        _colorEnd = colorEnd;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        float radius = _maxRadius * EaseOutQuad(t);
        float alpha = 1f - t * 1.2f;
        if (alpha <= 0f) return;

        Vector3 center = transform.position;
        for (int i = 0; i <= _segments; i++)
        {
            float angle = (float)i / _segments * Mathf.PI * 2f;
            _positions[i] = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }
        _line.SetPositions(_positions);

        Color c = Color.Lerp(_colorStart, _colorEnd, t);
        c.a *= alpha;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
            new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha * 0.5f, 1f) }
        );
        _line.colorGradient = g;
    }

    private static float EaseOutQuad(float t) { return 1f - (1f - t) * (1f - t); }
}

/// <summary>Flaş ışığının hızlıca sönmesi.</summary>
public class OctopusAmmoLightFade : MonoBehaviour
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
