using UnityEngine;

/// <summary>
/// Fireball mermisi için ateş temasına uygun trail efekti.
/// Prefab'a bu component'ı ekleyin; trail runtime'da otomatik oluşturulur.
/// Ateş renkleri: sarı/beyaz (merkez) -> turuncu -> kırmızı -> koyu kırmızı (kuyruk).
/// </summary>
[DisallowMultipleComponent]
public class FireballTrail : MonoBehaviour
{
    [Header("Trail Ayarı")]
    [Tooltip("Trail efekti açık mı?")]
    public bool enableTrail = true;

    [Tooltip("Trail süresi (saniye)")]
    [Range(0.1f, 1.5f)]
    public float trailTime = 0.45f;

    [Tooltip("Trail minimum vertex mesafesi (küçük = daha pürüzsüz)")]
    [Range(0.02f, 0.2f)]
    public float minVertexDistance = 0.06f;

    [Header("Ateş Renkleri (isteğe bağlı override)")]
    [Tooltip("Çekirdek rengi (parlak sarı/beyaz - ateşin merkezi)")]
    public Color coreColor = new Color(1f, 0.95f, 0.7f, 1f);

    [Tooltip("Ana trail rengi (turuncu)")]
    public Color mainColor = new Color(1f, 0.45f, 0.1f, 0.9f);

    [Tooltip("Dış glow rengi (koyu kırmızı/turuncu)")]
    public Color glowColor = new Color(0.8f, 0.2f, 0.05f, 0.4f);

    [Header("Genişlik")]
    [Tooltip("Glow trail başlangıç genişliği")]
    public float glowStartWidth = 0.5f;

    [Tooltip("Ana trail başlangıç genişliği")]
    public float mainStartWidth = 0.28f;

    [Tooltip("Çekirdek trail başlangıç genişliği")]
    public float coreStartWidth = 0.12f;

    [Header("Topun Etrafında Sarsıntı")]
    [Tooltip("Trail çıkış noktasının top merkezi etrafında dönme/sarsıntı yarıçapı")]
    [Range(0f, 0.5f)]
    public float wobbleRadius = 0.22f;

    [Tooltip("Sarsıntı hızı (ne kadar hızlı etrafında döner)")]
    [Range(2f, 20f)]
    public float wobbleSpeed = 8f;

    private TrailRenderer trailGlow;
    private TrailRenderer trailMain;
    private TrailRenderer trailCore;
    private GameObject trailRoot;
    private bool trailsCreated;

    private void Start()
    {
        if (enableTrail)
            CreateFireTrail();
    }

    private void Update()
    {
        if (!enableTrail || trailRoot == null) return;
        // Trail çıkış noktasını topun etrafında hafifçe döndür/sars; böylece trail topu sarar
        float t = Time.time * wobbleSpeed;
        float r = wobbleRadius;
        trailRoot.transform.localPosition = new Vector3(
            r * Mathf.Sin(t),
            r * Mathf.Cos(t * 1.2f),
            r * 0.6f * Mathf.Sin(t * 0.7f)
        );
    }

    private void CreateFireTrail()
    {
        if (trailsCreated) return;
        trailsCreated = true;

        trailRoot = new GameObject("FireballTrail");
        trailRoot.transform.SetParent(transform);
        trailRoot.transform.localPosition = Vector3.zero;
        trailRoot.transform.localRotation = Quaternion.identity;
        trailRoot.transform.localScale = Vector3.one;

        // 1. GLOW (dış ateş - en geniş, koyu kırmızı/turuncu)
        GameObject glowObj = new GameObject("TrailGlow");
        glowObj.transform.SetParent(trailRoot.transform);
        glowObj.transform.localPosition = Vector3.zero;
        trailGlow = glowObj.AddComponent<TrailRenderer>();
        SetupFireTrail(trailGlow, glowStartWidth, glowColor, trailTime * 0.85f, 0);

        // 2. MAIN (ana ateş - turuncu)
        GameObject mainObj = new GameObject("TrailMain");
        mainObj.transform.SetParent(trailRoot.transform);
        mainObj.transform.localPosition = Vector3.zero;
        trailMain = mainObj.AddComponent<TrailRenderer>();
        SetupFireTrail(trailMain, mainStartWidth, mainColor, trailTime, 1);

        // 3. CORE (çekirdek - parlak sarı/beyaz)
        GameObject coreObj = new GameObject("TrailCore");
        coreObj.transform.SetParent(trailRoot.transform);
        coreObj.transform.localPosition = Vector3.zero;
        trailCore = coreObj.AddComponent<TrailRenderer>();
        SetupFireTrail(trailCore, coreStartWidth, coreColor, trailTime * 0.9f, 2);
    }

    private void SetupFireTrail(TrailRenderer trail, float startWidth, Color baseColor, float time, int sortingOrder)
    {
        trail.time = time;
        trail.minVertexDistance = minVertexDistance;
        trail.textureMode = LineTextureMode.Stretch;
        trail.material = CreateFireTrailMaterial(baseColor);
        trail.sortingOrder = sortingOrder;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;

        // Ateş gibi genişlik: topa yakın geniş, kuyrukta yumuşak daralma
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(0.2f, 0.95f);
        widthCurve.AddKey(0.5f, 0.7f);
        widthCurve.AddKey(0.85f, 0.25f);
        widthCurve.AddKey(1f, 0.05f);
        trail.widthCurve = widthCurve;
        trail.widthMultiplier = startWidth;

        // Ateş gradient: başta parlak sarı/beyaz, kuyrukta koyu kırmızı ve saydam
        Gradient gradient = new Gradient();
        Color bright = Color.Lerp(baseColor, Color.white, 0.5f);
        Color mid = baseColor;
        Color dark = new Color(baseColor.r * 0.4f, baseColor.g * 0.15f, baseColor.b * 0.05f, 0f);

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(bright, 0f),
                new GradientColorKey(mid, 0.25f),
                new GradientColorKey(mid, 0.6f),
                new GradientColorKey(dark, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(Mathf.Clamp01(baseColor.a * 1.2f), 0f),
                new GradientAlphaKey(baseColor.a, 0.2f),
                new GradientAlphaKey(baseColor.a * 0.7f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;

        trail.numCapVertices = 5;
        trail.numCornerVertices = 5;
    }

    private static Material CreateFireTrailMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        mat.SetColor("_Color", color);
        return mat;
    }

    private void OnDestroy()
    {
        if (!enableTrail || trailRoot == null) return;

        // Trail'leri parent'tan ayır ki mermi yok olunca kendi sürelerince fade-out olsun
        trailRoot.transform.SetParent(null);
        float maxTime = Mathf.Max(
            trailGlow != null ? trailGlow.time : 0f,
            trailMain != null ? trailMain.time : 0f,
            trailCore != null ? trailCore.time : 0f
        );
        Destroy(trailRoot, maxTime + 0.1f);
    }
}
