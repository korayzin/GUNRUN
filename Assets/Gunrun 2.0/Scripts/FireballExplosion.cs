using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Fireball düşmana değince belirgin patlama VFX (çok katmanlı) ve etrafındaki düşmanlara AOE hasar.
/// </summary>
[RequireComponent(typeof(Bullet))]
public class FireballExplosion : MonoBehaviour
{
    [Tooltip("Patlama yarıçapı (etrafındaki düşmanlar)")]
    public float explosionRadius = 3f;

    [Tooltip("Patlamadan etkilenen düşmanlara verilen hasar")]
    public float explosionDamage = 15f;

    private bool exploded;

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        EnemyHealth hitEnemy = other.GetComponentInParent<EnemyHealth>();
        if (hitEnemy == null) return;

        exploded = true;
        Vector3 point = other.ClosestPoint(transform.position);

        CreateFireExplosionVFX(point);
        ApplyAoeDamage(point, hitEnemy);
    }

    private void CreateFireExplosionVFX(Vector3 position)
    {
        GameObject root = new GameObject("FireballExplosionVFX");
        root.transform.position = position;

        // 1) Parlak merkez flaşı (anlık ışık patlaması)
        CreateCoreFlash(root.transform);

        // 2) Ana ateş patlaması (büyük, dışa doğru)
        CreateMainExplosion(root.transform);

        // 3) Kıvılcım / parça saçılması
        CreateSparksBurst(root.transform);

        // 4) Genişleyen ateş halkası (ring)
        CreateExpandingRing(root.transform);

        Destroy(root, 2.5f);
    }

    private void CreateCoreFlash(Transform parent)
    {
        GameObject go = new GameObject("CoreFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.25f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        main.startColor = new Color(1f, 0.95f, 0.7f, 1f);
        main.maxParticles = 12;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.duration = 0.15f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), new GradientColorKey(Color.black, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.4f, 0.4f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1.2f), new Keyframe(0.15f, 1.5f), new Keyframe(1f, 0f)
        ));

        SetupParticleRenderer(go, new Color(1f, 0.9f, 0.6f, 1f));
    }

    private void CreateMainExplosion(Transform parent)
    {
        GameObject go = new GameObject("MainExplosion");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.85f, 0.3f, 1f),
            new Color(1f, 0.35f, 0.05f, 1f)
        );
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.duration = 0.35f;
        main.gravityModifier = -0.2f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 45), new ParticleSystem.Burst(0.08f, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.3f),
                new GradientColorKey(new Color(0.9f, 0.2f, 0f), 0.7f),
                new GradientColorKey(new Color(0.3f, 0.05f, 0f), 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0.3f, 0.6f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.8f), new Keyframe(0.2f, 1.2f), new Keyframe(0.6f, 0.6f), new Keyframe(1f, 0.1f)
        ));

        SetupParticleRenderer(go, new Color(1f, 0.5f, 0.1f, 1f));
    }

    private void CreateSparksBurst(Transform parent)
    {
        GameObject go = new GameObject("Sparks");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.95f, 0.5f, 1f),
            new Color(1f, 0.4f, 0.05f, 1f)
        );
        main.maxParticles = 35;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.duration = 0.2f;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f), new GradientColorKey(new Color(0.8f, 0.2f, 0f), 0.7f), new GradientColorKey(Color.black, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.6f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;

        SetupParticleRenderer(go, new Color(1f, 0.7f, 0.2f, 1f));
    }

    private void CreateExpandingRing(Transform parent)
    {
        GameObject go = new GameObject("ExpandingRing");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        int segments = 32;
        lr.positionCount = segments + 1;
        lr.startWidth = 0.4f;
        lr.endWidth = 0.15f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f), new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f), new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f) },
            new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        lr.colorGradient = grad;

        ExpandRing anim = go.AddComponent<ExpandRing>();
        anim.segments = segments;
        anim.duration = 0.5f;
        anim.maxRadius = 2f;
    }

    private static void SetupParticleRenderer(GameObject go, Color color)
    {
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.material = new Material(Shader.Find("Sprites/Default"));
        r.material.color = color;
        r.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private void ApplyAoeDamage(Vector3 center, EnemyHealth primaryTarget)
    {
        Collider[] hits = Physics.OverlapSphere(center, explosionRadius);
        foreach (Collider col in hits)
        {
            EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                if (eh == primaryTarget) continue;
                eh.TakeDamage(explosionDamage, col);
                continue;
            }

            // DestructibleMesh (MRUK duvar) - patlama alanındaki segmentleri kır
            DestructibleMeshComponent destructibleMesh = col.GetComponentInParent<DestructibleMeshComponent>();
            if (destructibleMesh != null && col.gameObject != destructibleMesh.ReservedSegment)
            {
                destructibleMesh.DestroySegment(col.gameObject);
                DestructibleMeshHint.NotifyWallDestroyed();
                if (WeaponManager.Instance != null)
                {
                    WeaponManager.Instance.PlayHitSound();
                    WeaponManager.Instance.TriggerHitHaptic();
                }
            }
        }
    }
}
