using UnityEngine;

/// <summary>
/// Fireball giderken etrafında titreşen alev çizgileri / kıvılcım efekti.
/// Minik çizgiler topun etrafında dolaşır ve titreşir.
/// </summary>
[RequireComponent(typeof(Bullet))]
public class FireballTravelEffect : MonoBehaviour
{
    [Header("Alev çizgileri")]
    [Tooltip("Çizgi sayısı (topun etrafında)")]
    [Range(6, 20)]
    public int lineCount = 12;

    [Tooltip("Çizginin top yüzeyinden dışa uzunluğu")]
    [Range(0.05f, 0.35f)]
    public float lineLength = 0.18f;

    [Tooltip("Çizgilerin başladığı yarıçap (topun yarıçapına yakın)")]
    [Range(0.2f, 0.6f)]
    public float baseRadius = 0.45f;

    [Tooltip("Titreşim hızı")]
    [Range(15f, 50f)]
    public float flickerSpeed = 28f;

    [Tooltip("Titreşim genliği (ne kadar sallanır)")]
    [Range(0.02f, 0.15f)]
    public float flickerAmount = 0.06f;

    [Tooltip("Etrafında dönme hızı")]
    [Range(0f, 90f)]
    public float orbitSpeed = 35f;

    [Header("Görünüm")]
    public Color coreColor = new Color(1f, 0.95f, 0.5f, 0.95f);
    public Color tipColor = new Color(1f, 0.35f, 0.05f, 0.4f);
    [Range(0.01f, 0.06f)]
    public float lineWidth = 0.03f;

    private LineRenderer[] lines;
    private float[] linePhases;
    private Transform container;

    private void Start()
    {
        container = new GameObject("FireballFlameLines").transform;
        container.SetParent(transform);
        container.localPosition = Vector3.zero;
        container.localRotation = Quaternion.identity;
        container.localScale = Vector3.one;

        lines = new LineRenderer[lineCount];
        linePhases = new float[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            linePhases[i] = i * 0.7f;
            GameObject go = new GameObject("FlameLine_" + i);
            go.transform.SetParent(container);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth * 0.4f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material.color = coreColor;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(coreColor, 0f), new GradientColorKey(tipColor, 1f) },
                new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.35f, 1f) }
            );
            lr.colorGradient = g;

            lines[i] = lr;
        }
    }

    private void Update()
    {
        if (lines == null) return;

        float t = Time.time;
        Vector3 center = transform.position;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null) continue;

            float phase = linePhases[i];
            float angle = (float)i / lineCount * Mathf.PI * 2f + t * orbitSpeed * Mathf.Deg2Rad;
            float tilt = Mathf.Sin(t * 1.2f + phase * 0.5f) * 0.4f;
            float jitterX = (Mathf.PerlinNoise(t * flickerSpeed + phase, phase * 2f) - 0.5f) * 2f;
            float jitterY = (Mathf.PerlinNoise(phase * 1.3f, t * flickerSpeed + phase) - 0.5f) * 2f;
            float jitterZ = (Mathf.PerlinNoise(t * 0.8f + phase, phase * 0.7f) - 0.5f) * 2f;

            Vector3 axis = new Vector3(Mathf.Cos(angle), tilt, Mathf.Sin(angle)).normalized;
            Vector3 jitter = new Vector3(jitterX, jitterY, jitterZ) * flickerAmount;
            Vector3 start = center + axis * baseRadius + jitter;
            Vector3 end = center + axis * (baseRadius + lineLength) + jitter * 1.5f;

            lines[i].SetPosition(0, start);
            lines[i].SetPosition(1, end);
        }
    }

    private void OnDestroy()
    {
        if (container != null)
            Destroy(container.gameObject);
    }
}
