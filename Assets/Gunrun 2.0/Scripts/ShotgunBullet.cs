using UnityEngine;

/// <summary>
/// Shotgun mermisine Z ekseni etrafında döndürme efekti, tail trail ve düşmana değince hit/patlama efekti ekler.
/// Bullet.cs ile birlikte çalışır - Bullet hareket/hasar, bu script rotasyon, trail ve hit efekti.
/// </summary>
public class ShotgunBullet : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Dönüş hızı (derece/saniye). 720 = saniyede 2 tam tur")]
    public float rotationSpeed = 720f;
    
    [Header("Trail Settings")]
    [Tooltip("Trail efekti aktif mi?")]
    public bool enableTrail = true;
    
    [Tooltip("Trail süresi (saniye)")]
    public float trailTime = 0.15f;
    
    [Tooltip("Trail başlangıç genişliği")]
    public float trailStartWidth = 0.03f;
    
    [Tooltip("Trail bitiş genişliği")]
    public float trailEndWidth = 0f;
    
    [Tooltip("Trail başlangıç rengi")]
    public Color trailStartColor = new Color(1f, 0.8f, 0.3f, 1f);
    
    [Tooltip("Trail bitiş rengi")]
    public Color trailEndColor = new Color(1f, 0.5f, 0.1f, 0f);
    
    [Header("Hit Effect (Enemy Impact)")]
    [Tooltip("Düşmana değince patlama/hit efekti aktif mi?")]
    public bool enableHitEffect = true;
    
    [Tooltip("Parçacık sayısı")]
    [Range(20, 120)] public int hitParticleCount = 70;
    
    [Tooltip("Parçacık ömür süresi (sn) min-max")]
    public Vector2 hitLifetime = new Vector2(0.35f, 0.55f);
    
    [Tooltip("Parçacık yayılma hızı min-max")]
    public Vector2 hitSpeed = new Vector2(5f, 9.5f);
    
    [Tooltip("Parçacık boyutu min-max")]
    public Vector2 hitSize = new Vector2(0.2f, 0.42f);
    
    [Tooltip("Başlangıç rengi (açık)")]
    public Color hitColorStart = new Color(1f, 1f, 0.85f, 1f);
    
    [Tooltip("Başlangıç rengi (koyu)")]
    public Color hitColorEnd = new Color(1f, 0.75f, 0.25f, 1f);
    
    [Tooltip("Yerçekimi etkisi (düşük = daha yatay yayılma)")]
    [Range(-0.5f, 2f)] public float hitGravityModifier = 0.2f;
    
    [Tooltip("Efekt nesnesinin yok edilme süresi (sn)")]
    [Range(0.3f, 2f)] public float hitEffectDuration = 0.8f;
    
    private void Start()
    {
        if (enableTrail)
        {
            AddTrailsToVisuals();
        }
    }
    
    private void Update()
    {
        // Z ekseni etrafında döndür (local space)
        transform.Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!enableHitEffect) return;
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            SpawnHitEffect(hitPoint);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!enableHitEffect) return;
        Collider other = collision.collider;
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            SpawnHitEffect(hitPoint);
        }
    }
    
    /// <summary>Düşman üstünde mermi patladığını net gösteren hit efekti.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        GameObject hitObj = new GameObject("BulletHitEffect");
        hitObj.transform.position = position;
        
        ParticleSystem ps = hitObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.05f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(hitLifetime.x, hitLifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(hitSpeed.x, hitSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(hitSize.x, hitSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(hitColorStart, hitColorEnd);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = hitParticleCount;
        main.gravityModifier = hitGravityModifier;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;
        
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, hitParticleCount) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.02f;
        
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1.1f);
        sizeCurve.AddKey(0.4f, 0.95f);
        sizeCurve.AddKey(0.8f, 0.5f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.4f), 0.4f),
                new GradientColorKey(new Color(1f, 0.5f, 0.15f), 0.8f),
                new GradientColorKey(new Color(0.9f, 0.4f, 0.1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.95f, 0.3f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        Material particleMat = CreateSoftParticleMaterial();
        renderer.material = particleMat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 20;
        
        ps.Play();
        Destroy(hitObj, hitEffectDuration);
    }
    
    private static Material _softParticleMat;
    private static Material CreateSoftParticleMaterial()
    {
        if (_softParticleMat != null) return _softParticleMat;
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - d);
                alpha = alpha * alpha;
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        
        Shader shader = Shader.Find("Legacy Shaders/Particles/Additive")
            ?? Shader.Find("Particles/Additive")
            ?? Shader.Find("Sprites/Default");
        _softParticleMat = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
        _softParticleMat.mainTexture = tex;
        _softParticleMat.SetInt("_ZWrite", 0);
        return _softParticleMat;
    }
    
    private void AddTrailsToVisuals()
    {
        // Tüm child'lara TrailRenderer ekle
        foreach (Transform child in transform)
        {
            // Zaten TrailRenderer varsa ekleme
            if (child.GetComponent<TrailRenderer>() != null)
                continue;
                
            TrailRenderer trail = child.gameObject.AddComponent<TrailRenderer>();
            ConfigureTrail(trail);
        }
    }
    
    private void ConfigureTrail(TrailRenderer trail)
    {
        // Temel ayarlar
        trail.time = trailTime;
        trail.startWidth = trailStartWidth;
        trail.endWidth = trailEndWidth;
        
        // Renk gradient'i
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(trailStartColor, 0f),
                new GradientColorKey(trailEndColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(trailStartColor.a, 0f),
                new GradientAlphaKey(trailEndColor.a, 1f)
            }
        );
        trail.colorGradient = gradient;
        
        // Material - Default-Line veya Sprites-Default kullan
        trail.material = new Material(Shader.Find("Sprites/Default"));
        
        // Gölge kapalı (performans için)
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        
        // Minimum vertex distance
        trail.minVertexDistance = 0.01f;
    }
}
