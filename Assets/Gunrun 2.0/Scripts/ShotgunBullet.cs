using UnityEngine;

/// <summary>
/// Shotgun mermisine Z ekseni etrafında döndürme efekti, tail trail ve düşmana değince hit/patlama efekti ekler.
/// Bullet.cs ile birlikte çalışır - Bullet hareket/hasar, bu script rotasyon, trail ve hit efekti.
/// </summary>
public class ShotgunBullet : MonoBehaviour
{
    [Header("Spread (GunFire tarafından kullanılır)")]
    [Tooltip("Pompalı saçma koni açısı (derece). Her mermi bu açı içinde rastgele sapar. 0 = sapma yok.")]
    [Range(0f, 15f)] public float spreadConeAngleDeg = 6f;

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

    [Header("Pompalı saçma çarpma efekti")]
    [Tooltip("Düşmana çarptığında patlayıp en yakın düşmanlara doğru saçılma")]
    public bool enablePelletBurstEffect = true;
    [Tooltip("En yakın düşmanları aramak için yarıçap")]
    [Range(1f, 10f)] public float nearbyEnemySearchRadius = 4f;
    [Tooltip("Kaç düşmana doğru ayrı koni atılacak (en yakındakiler)")]
    [Range(1, 6)] public int maxEnemiesToTarget = 4;
    [Tooltip("Düşmana doğru giden koni açısı (derece)")]
    [Range(10f, 45f)] public float pelletBurstConeAngle = 28f;
    
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
    
    /// <summary>Düşman üstünde patlama; saçmalar en yakın düşmanlara doğru dağılır (yerçekimi yok).</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (enablePelletBurstEffect)
        {
            // Merkezde kısa bir patlama (her yöne, aşağı değil)
            SpawnCentralExplosion(position);
            // En yakın düşmanları bul, her birine doğru koni at
            Collider[] cols = Physics.OverlapSphere(position, nearbyEnemySearchRadius);
            var enemiesWithDist = new System.Collections.Generic.List<(EnemyHealth eh, float dist)>();
            foreach (Collider c in cols)
            {
                EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
                if (eh == null) continue;
                Vector3 enemyCenter = eh.transform.position;
                float d = Vector3.Distance(position, enemyCenter);
                if (d < 0.15f) continue; // çarptığımız düşmanı atla (isteğe bağlı)
                enemiesWithDist.Add((eh, d));
            }
            enemiesWithDist.Sort((a, b) => a.dist.CompareTo(b.dist));
            int count = Mathf.Min(maxEnemiesToTarget, enemiesWithDist.Count);
            for (int i = 0; i < count; i++)
            {
                Vector3 toEnemy = (enemiesWithDist[i].eh.transform.position - position).normalized;
                SpawnPelletBurstToward(position, toEnemy);
            }
            // Hiç yakın düşman yoksa yine de birkaç rastgele yöne koni at (patlama hissi)
            if (count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = (i / 3f) * 360f * Mathf.Deg2Rad;
                    Vector3 dir = (Vector3.right * Mathf.Cos(angle) + Vector3.forward * Mathf.Sin(angle)).normalized;
                    SpawnPelletBurstToward(position, dir);
                }
            }
        }
        else
        {
            SpawnSingleHitEffect(position, ParticleSystemShapeType.Sphere, 0.02f, Vector3.forward);
        }
    }

    /// <summary>Merkez patlama – küre, yerçekimi 0.</summary>
    private void SpawnCentralExplosion(Vector3 position)
    {
        GameObject hitObj = new GameObject("ShotgunCentralExplosion");
        hitObj.transform.position = position;

        ParticleSystem ps = hitObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.04f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(hitLifetime.x * 0.8f, hitLifetime.y * 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(hitSpeed.x, hitSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(hitSize.x * 0.9f, hitSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(hitColorStart, hitColorEnd);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 40;
        main.gravityModifier = 0f;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.02f;

        AddCommonParticleModules(ps);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateSoftParticleMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 20;
        ps.Play();
        Destroy(hitObj, hitEffectDuration);
    }

    /// <summary>Belirli bir yöne (en yakın düşmana) koni ile saçma – yerçekimi 0. Unity Cone -Z yönde emit eder.</summary>
    private void SpawnPelletBurstToward(Vector3 position, Vector3 directionTowardTarget)
    {
        GameObject hitObj = new GameObject("ShotgunPelletBurstToward");
        hitObj.transform.position = position;
        // Cone -Z yönde emit eder; biz directionTowardTarget'a gitmesini istiyoruz => forward = -directionTowardTarget
        hitObj.transform.rotation = Quaternion.LookRotation(-directionTowardTarget);

        ParticleSystem ps = hitObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.05f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(hitLifetime.x * 1.1f, hitLifetime.y * 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(hitSpeed.x * 1.2f, hitSpeed.y * 1.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(hitSize.x, hitSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(hitColorStart, hitColorEnd);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;
        main.gravityModifier = 0f;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = pelletBurstConeAngle * 0.5f * Mathf.Deg2Rad;
        shape.radius = 0.02f;
        shape.rotation = Vector3.zero;
        shape.length = 0.05f;

        AddCommonParticleModules(ps);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateSoftParticleMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 20;
        ps.Play();
        Destroy(hitObj, hitEffectDuration * 1.1f);
    }

    private void SpawnSingleHitEffect(Vector3 position, ParticleSystemShapeType shapeType, float radius, Vector3 shapeDir)
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
        shape.shapeType = shapeType;
        shape.radius = radius;

        AddCommonParticleModules(ps);
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateSoftParticleMaterial();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 20;
        ps.Play();
        Destroy(hitObj, hitEffectDuration);
    }

    private static void AddCommonParticleModules(ParticleSystem ps)
    {
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
