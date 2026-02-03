using UnityEngine;

/// <summary>
/// Eight Gun mermisi görsel animasyonu. Ammo prefab'ının root'una component olarak ekle.
/// Cartoon trail (dolgulu + konturlu) ekler. Hareket/hasar Bullet.cs tarafından yönetilir.
/// </summary>
public class EightAmmoAnimation : MonoBehaviour
{
    [Header("Cartoon Trail (dolgulu + konturlu)")]
    [Tooltip("Cartoon trail açık mı?")]
    public bool enableTrail = true;

    [Tooltip("Trail süresi (saniye)")]
    public float trailTime = 0.2f;

    [Tooltip("Kontur (dış çizgi) kalınlığı - başlangıç")]
    public float outlineStartWidth = 0.032f;

    [Tooltip("Kontur kalınlığı - bitiş")]
    public float outlineEndWidth = 0.006f;

    [Tooltip("Kontur rengi (çizgi film çerçevesi)")]
    public Color outlineColor = new Color(0.12f, 0.1f, 0.08f, 0.95f);

    [Tooltip("Dolgu kalınlığı - başlangıç (konturun içinde, ince)")]
    public float fillStartWidth = 0.02f;

    [Tooltip("Dolgu kalınlığı - bitiş")]
    public float fillEndWidth = 0.002f;

    [Tooltip("Dolgu rengi (parlak şerit)")]
    public Color fillColor = new Color(1f, 0.92f, 0.6f, 0.9f);

    private TrailRenderer _trailOutline;
    private TrailRenderer _trailFill;

    private void Start()
    {
        if (enableTrail)
            CreateCartoonTrail();
    }

    private void CreateCartoonTrail()
    {
        Material outlineMat = new Material(Shader.Find("Sprites/Default"));
        if (outlineMat.shader.name == "Hidden/InternalErrorShader")
            outlineMat = new Material(Shader.Find("Unlit/Color"));

        Material fillMat = new Material(Shader.Find("Sprites/Default"));
        if (fillMat.shader.name == "Hidden/InternalErrorShader")
            fillMat = new Material(Shader.Find("Unlit/Color"));

        // 1) Kontur (dış çizgi) – biraz daha geniş, arkada
        GameObject outlineObj = new GameObject("TrailOutline");
        outlineObj.transform.SetParent(transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one;
        _trailOutline = outlineObj.AddComponent<TrailRenderer>();
        SetupTrail(_trailOutline, outlineStartWidth, outlineEndWidth, outlineColor, outlineMat, 0);

        // 2) Dolgu (iç parlak şerit) – ince, üstte
        GameObject fillObj = new GameObject("TrailFill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localRotation = Quaternion.identity;
        fillObj.transform.localScale = Vector3.one;
        _trailFill = fillObj.AddComponent<TrailRenderer>();
        SetupTrail(_trailFill, fillStartWidth, fillEndWidth, fillColor, fillMat, 1);
    }

    private void SetupTrail(TrailRenderer trail, float startW, float endW, Color color, Material mat, int order)
    {
        trail.time = trailTime;
        trail.startWidth = startW;
        trail.endWidth = endW;
        trail.material = mat;
        trail.sortingOrder = order;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.minVertexDistance = 0.02f;
        trail.autodestruct = false;

        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(color.a * 0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = g;
    }

}
