using UnityEngine;

/// <summary>
/// LineRenderer ile genişleyen halka animasyonu (patlama ring'i).
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ExpandRing : MonoBehaviour
{
    public int segments = 32;
    public float duration = 0.5f;
    public float maxRadius = 2f;

    private LineRenderer lr;
    private float elapsed;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments + 1;
        lr.loop = true;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float radius = Mathf.Lerp(0.1f, maxRadius, t);
        float alpha = 1f - t;
        if (lr == null) return;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 local = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            lr.SetPosition(i, transform.TransformPoint(local));
        }

        Gradient g = lr.colorGradient;
        GradientAlphaKey[] keys = g.alphaKeys;
        if (keys.Length > 0)
        {
            keys[0].alpha = alpha * 0.9f;
            keys[keys.Length - 1].alpha = 0f;
            g.SetKeys(g.colorKeys, keys);
            lr.colorGradient = g;
        }
        lr.startWidth = 0.4f * alpha;
        lr.endWidth = 0.15f * alpha;

        if (elapsed >= duration)
            Destroy(gameObject);
    }
}
