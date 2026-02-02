using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Roket animasyonu için görsel efektler ve animasyonlar.
/// Prefab'a eklenip otomatik çalışır, Bullet.cs'e dokunmaz.
/// </summary>
public class RocketAnimation : MonoBehaviour
{
    [Header("=== ROKET DÖNÜŞ ANİMASYONU ===")]
    [Tooltip("Roket dönüş hızı (derece/saniye)")]
    public float spinSpeed = 720f;
    
    [Tooltip("Dönüş ekseni (varsayılan: forward)")]
    public Vector3 spinAxis = Vector3.forward;
    
    [Tooltip("Rastgele dönüş hızı ekle")]
    public bool randomizeSpinSpeed = true;
    
    [Tooltip("Rastgele dönüş hızı varyasyonu")]
    [Range(0f, 1f)]
    public float spinSpeedVariation = 0.3f;

    [Header("=== ROKET ALEV EFEKTİ ===")]
    [Tooltip("Alev efekti aktif mi?")]
    public bool enableFlameEffect = true;
    
    [Tooltip("Alev pozisyonu (roketin arkasına göre offset)")]
    public Vector3 flameOffset = new Vector3(0f, 0f, -0.3f);
    
    [Tooltip("Alev boyutu")]
    public float flameSize = 0.2f;
    
    [Tooltip("Alev çekirdek rengi (parlak beyaz)")]
    public Color flameCoreColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Alev ana rengi (başlangıç)")]
    public Color flameStartColor = new Color(1f, 0.7f, 0.3f, 1f);
    
    [Tooltip("Alev rengi (bitiş)")]
    public Color flameEndColor = new Color(1f, 0.2f, 0f, 0f);
    
    [Tooltip("Alev yoğunluğu")]
    [Range(20f, 150f)]
    public float flameIntensity = 80f;
    
    [Tooltip("Alev parçacık sayısı")]
    [Range(50f, 300f)]
    public float flameParticleCount = 150f;

    [Header("=== ROKET TRAIL (İZ) ===")]
    [Tooltip("Trail renderer aktif mi?")]
    public bool enableTrail = true;
    
    [Tooltip("Trail süresi")]
    public float trailTime = 0.5f;
    
    [Tooltip("Ana trail genişliği (başlangıç)")]
    public float trailStartWidth = 0.08f;
    
    [Tooltip("Ana trail genişliği (bitiş)")]
    public float trailEndWidth = 0.015f;
    
    [Tooltip("Glow trail genişliği (başlangıç) - dış glow")]
    public float glowTrailStartWidth = 0.2f;
    
    [Tooltip("Glow trail genişliği (bitiş) - dış glow")]
    public float glowTrailEndWidth = 0.05f;
    
    [Tooltip("Trail çekirdek rengi (parlak merkez)")]
    public Color trailCoreColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Trail ana rengi")]
    public Color trailMainColor = new Color(1f, 0.6f, 0.2f, 0.9f);
    
    [Tooltip("Trail glow rengi (dış glow)")]
    public Color trailGlowColor = new Color(1f, 0.4f, 0f, 0.4f);

    [Header("=== ROKET IŞIK EFEKTİ ===")]
    [Tooltip("Işık efekti aktif mi?")]
    public bool enableLight = true;
    
    [Tooltip("Işık şiddeti")]
    public float lightIntensity = 2f;
    
    [Tooltip("Işık menzili")]
    public float lightRange = 5f;
    
    [Tooltip("Işık rengi")]
    public Color lightColor = new Color(1f, 0.6f, 0.2f);

    [Header("=== YILDIRIM EFEKTLERİ ===")]
    [Tooltip("Roket etrafında yıldırım efektleri aktif mi?")]
    public bool enableLightning = true;
    
    [Tooltip("Yıldırım sayısı")]
    [Range(2, 8)]
    public int lightningCount = 4;
    
    [Tooltip("Yıldırım uzunluğu")]
    public float lightningLength = 0.3f;
    
    [Tooltip("Yıldırım sapma miktarı")]
    [Range(0f, 0.3f)]
    public float lightningJagged = 0.15f;
    
    [Tooltip("Yıldırım segment sayısı")]
    [Range(4, 12)]
    public int lightningSegments = 8;
    
    [Tooltip("Yıldırım çekirdek rengi")]
    public Color lightningCoreColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Yıldırım glow rengi")]
    public Color lightningGlowColor = new Color(0.4f, 0.7f, 1f, 0.6f);
    
    [Tooltip("Yıldırım güncelleme hızı (saniye)")]
    public float lightningUpdateRate = 0.05f;
    
    [Tooltip("Yıldırım kalınlığı (çekirdek)")]
    public float lightningCoreWidth = 0.02f;
    
    [Tooltip("Yıldırım kalınlığı (glow)")]
    public float lightningGlowWidth = 0.08f;

    // Private değişkenler
    private float currentSpinSpeed;
    private float targetSpinSpeed;
    private Quaternion targetRotation;
    private ParticleSystem flameParticles;
    private TrailRenderer trailCore;      // Çekirdek trail
    private TrailRenderer trailMain;       // Ana trail
    private TrailRenderer trailGlow;       // Glow trail
    private Light rocketLight;
    private Transform flameTransform;
    private GameObject lightningContainer;
    private List<LineRenderer> lightningCores = new List<LineRenderer>();
    private List<LineRenderer> lightningGlows = new List<LineRenderer>();
    private float lightningTimer = 0f;
    private float rotationSmoothVelocity = 0f;

    void Start()
    {
        // Dönüş hızını ayarla
        if (randomizeSpinSpeed)
        {
            float variation = 1f + Random.Range(-spinSpeedVariation, spinSpeedVariation);
            currentSpinSpeed = spinSpeed * variation;
            targetSpinSpeed = currentSpinSpeed;
        }
        else
        {
            currentSpinSpeed = spinSpeed;
            targetSpinSpeed = spinSpeed;
        }
        
        targetRotation = transform.rotation;

        // Alev efekti oluştur
        if (enableFlameEffect)
        {
            CreateFlameEffect();
        }

        // Trail renderer oluştur
        if (enableTrail)
        {
            CreateTrail();
        }

        // Işık efekti oluştur
        if (enableLight)
        {
            CreateLight();
        }
        
        // Yıldırım efektleri oluştur
        if (enableLightning)
        {
            CreateLightningEffects();
        }
    }

    void Update()
    {
        // Smooth dönüş animasyonu (Quaternion.Slerp ile)
        targetRotation *= Quaternion.AngleAxis(currentSpinSpeed * Time.deltaTime, spinAxis.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        
        // Yıldırım efektlerini güncelle
        if (enableLightning)
        {
            lightningTimer += Time.deltaTime;
            if (lightningTimer >= lightningUpdateRate)
            {
                lightningTimer = 0f;
                UpdateLightningEffects();
            }
        }

        // Alev efektini güncelle
        if (flameParticles != null && flameTransform != null)
        {
            flameTransform.position = transform.position + transform.TransformDirection(flameOffset);
            flameTransform.rotation = transform.rotation;
        }

        // Işık efektini güncelle
        if (rocketLight != null)
        {
            rocketLight.transform.position = transform.position;
            rocketLight.transform.rotation = transform.rotation;
            
            // Işık titreşimi (gerçekçi roket alevi efekti)
            float flicker = 0.8f + Random.Range(0f, 0.4f);
            rocketLight.intensity = lightIntensity * flicker;
        }
    }

    void CreateFlameEffect()
    {
        // Alev için GameObject oluştur
        GameObject flameObj = new GameObject("RocketFlame");
        flameObj.transform.SetParent(transform);
        flameTransform = flameObj.transform;
        flameTransform.localPosition = flameOffset;
        flameTransform.localRotation = Quaternion.identity;

        // Particle System ekle
        flameParticles = flameObj.AddComponent<ParticleSystem>();
        var main = flameParticles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(flameSize * 0.7f, flameSize * 1.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(flameStartColor, flameEndColor);
        main.maxParticles = Mathf.RoundToInt(flameParticleCount);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        main.startRotation3D = true;
        
        // Emission modülü (yeni Unity API)
        var emission = flameParticles.emission;
        emission.rateOverTime = flameIntensity;
        emission.enabled = true;

        // Shape modülü (konik alev)
        var shape = flameParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.01f;

        // Velocity over Lifetime (ileri doğru hız)
        var velocity = flameParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = new ParticleSystem.MinMaxCurve(2f, 5f);

        // Color over Lifetime (smooth renk geçişi - laser gibi)
        var colorOverLifetime = flameParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        Color brightCore = Color.Lerp(flameCoreColor, Color.white, 0.3f);
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightCore, 0f),
                new GradientColorKey(flameStartColor, 0.2f),
                new GradientColorKey(flameStartColor, 0.6f),
                new GradientColorKey(flameEndColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0.8f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        // Size over Lifetime (smooth küçülme)
        var sizeOverLifetime = flameParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, 1.1f);  // Biraz büyüme
        sizeCurve.AddKey(0.7f, 0.6f);  // Yavaş küçülme
        sizeCurve.AddKey(1f, 0.2f);    // Sonunda çok küçük
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Size by Speed (hıza göre boyut)
        var sizeBySpeed = flameParticles.sizeBySpeed;
        sizeBySpeed.enabled = true;
        sizeBySpeed.size = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        sizeBySpeed.range = new Vector2(0f, 10f);
        
        // Rotation over Lifetime (dönen parçacıklar)
        var rotationOverLifetime = flameParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

        // Renderer ayarları (daha parlak görünüm)
        var renderer = flameParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateFlameMaterial();
        renderer.sortingOrder = 1;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        
        // Trails modülü (parçacıkların kendi trail'leri)
        var trails = flameParticles.trails;
        trails.enabled = true;
        trails.lifetime = 0.1f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        trails.colorOverLifetime = gradient;
        trails.dieWithParticles = true;
    }

    Material CreateFlameMaterial()
    {
        // Basit alev materyali oluştur
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = flameStartColor;
        return mat;
    }

    void CreateTrail()
    {
        // Laser efektine benzer çok katmanlı trail sistemi
        
        // 1. GLOW TRAIL (Dış glow - en geniş)
        GameObject glowObj = new GameObject("TrailGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        trailGlow = glowObj.AddComponent<TrailRenderer>();
        SetupTrail(trailGlow, glowTrailStartWidth, glowTrailEndWidth, trailGlowColor, 0.3f, 0);
        
        // 2. MAIN TRAIL (Ana trail)
        GameObject mainObj = new GameObject("TrailMain");
        mainObj.transform.SetParent(transform);
        mainObj.transform.localPosition = Vector3.zero;
        trailMain = mainObj.AddComponent<TrailRenderer>();
        SetupTrail(trailMain, trailStartWidth, trailEndWidth, trailMainColor, 0.5f, 1);
        
        // 3. CORE TRAIL (Parlak çekirdek - en dar)
        GameObject coreObj = new GameObject("TrailCore");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        trailCore = coreObj.AddComponent<TrailRenderer>();
        SetupTrail(trailCore, trailStartWidth * 0.4f, trailEndWidth * 0.3f, trailCoreColor, 0.4f, 2);
    }
    
    void SetupTrail(TrailRenderer trail, float startWidth, float endWidth, Color baseColor, float time, int sortingOrder)
    {
        trail.time = time;
        trail.minVertexDistance = 0.05f; // Daha smooth
        trail.textureMode = LineTextureMode.Stretch;
        trail.material = CreateTrailMaterial(baseColor);
        trail.sortingOrder = sortingOrder;
        
        // Smooth width curve (laser gibi)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);   // Başta biraz dar
        widthCurve.AddKey(0.1f, 1f);  // Hemen genişle
        widthCurve.AddKey(0.5f, 1.05f); // Ortada biraz şişkin
        widthCurve.AddKey(0.9f, 0.9f);  // Sonlara doğru daral
        widthCurve.AddKey(1f, 0.5f);    // En sonda çok dar
        trail.widthCurve = widthCurve;
        trail.widthMultiplier = startWidth;
        
        // Smooth gradient (laser efektine benzer)
        Gradient gradient = new Gradient();
        Color brightColor = Color.Lerp(baseColor, Color.white, 0.5f);
        Color midColor = baseColor;
        Color fadeColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
        
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),      // Başta parlak
                new GradientColorKey(midColor, 0.15f),     // Hemen ana renge
                new GradientColorKey(midColor, 0.85f),     // Ortada sabit
                new GradientColorKey(brightColor, 1f)       // Sonda tekrar parlak
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(baseColor.a * 0.9f, 0f),
                new GradientAlphaKey(baseColor.a, 0.2f),
                new GradientAlphaKey(baseColor.a, 0.8f),
                new GradientAlphaKey(baseColor.a * 0.3f, 1f)  // Sonda soluyor
            }
        );
        trail.colorGradient = gradient;
        
        // Smooth köşeler ve uçlar
        trail.numCapVertices = 5;
        trail.numCornerVertices = 5;
    }

    Material CreateTrailMaterial(Color color)
    {
        // Parlak trail materyali (laser gibi)
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        // Daha parlak görünüm için
        mat.SetFloat("_Glow", 1.5f);
        return mat;
    }

    void CreateLight()
    {
        // Işık GameObject'i oluştur
        GameObject lightObj = new GameObject("RocketLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        // Light component ekle
        rocketLight = lightObj.AddComponent<Light>();
        rocketLight.type = LightType.Point;
        rocketLight.color = lightColor;
        rocketLight.intensity = lightIntensity;
        rocketLight.range = lightRange;
        rocketLight.shadows = LightShadows.None;
    }

    void CreateLightningEffects()
    {
        // Yıldırım container oluştur (parent olmadan, world space'de)
        lightningContainer = new GameObject("LightningContainer");
        lightningContainer.transform.position = transform.position;
        lightningContainer.transform.rotation = Quaternion.identity;
        
        // Her yıldırım için glow ve core layer oluştur
        for (int i = 0; i < lightningCount; i++)
        {
            // Glow layer
            LineRenderer glow = CreateLightningLine(lightningGlowWidth, lightningGlowColor);
            lightningGlows.Add(glow);
            
            // Core layer
            LineRenderer core = CreateLightningLine(lightningCoreWidth, lightningCoreColor);
            lightningCores.Add(core);
        }
        
        // İlk güncelleme
        UpdateLightningEffects();
    }
    
    LineRenderer CreateLightningLine(float width, Color color)
    {
        GameObject obj = new GameObject("LightningBolt");
        obj.transform.SetParent(lightningContainer.transform);
        
        LineRenderer line = obj.AddComponent<LineRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;
        
        // Smooth width curve
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);
        widthCurve.AddKey(0.1f, 1f);
        widthCurve.AddKey(0.5f, 1.05f);
        widthCurve.AddKey(0.9f, 0.9f);
        widthCurve.AddKey(1f, 0.5f);
        line.widthCurve = widthCurve;
        line.widthMultiplier = width;
        
        line.useWorldSpace = true;
        line.numCapVertices = 3;
        line.numCornerVertices = 3;
        line.textureMode = LineTextureMode.Stretch;
        
        // Smooth gradient
        Gradient grad = new Gradient();
        Color brightColor = Color.Lerp(color, Color.white, 0.5f);
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),
                new GradientColorKey(color, 0.15f),
                new GradientColorKey(color, 0.85f),
                new GradientColorKey(brightColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(color.a * 0.9f, 0f),
                new GradientAlphaKey(color.a, 0.2f),
                new GradientAlphaKey(color.a, 0.8f),
                new GradientAlphaKey(color.a * 0.7f, 1f)
            }
        );
        line.colorGradient = grad;
        
        return line;
    }
    
    void UpdateLightningEffects()
    {
        if (lightningContainer == null) return;
        
        Vector3 rocketPos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        
        // Container'ı roketin pozisyonuna taşı
        lightningContainer.transform.position = rocketPos;
        
        // Her yıldırım için pozisyon hesapla
        for (int i = 0; i < lightningCount; i++)
        {
            float baseAngle = (360f / lightningCount) * i;
            float rotationSpeed = 120f; // Derece/saniye
            float angle = baseAngle + Time.time * rotationSpeed; // Dönen yıldırımlar
            float rad = angle * Mathf.Deg2Rad;
            
            // Roket etrafında dairesel pozisyon (roketin merkezinden)
            Vector3 radialDir = (right * Mathf.Cos(rad) + up * Mathf.Sin(rad));
            Vector3 startPos = rocketPos + radialDir * 0.08f; // Roket yüzeyinden başla
            
            // Rastgele yön (roketten dışarı doğru, biraz forward/backward varyasyonu)
            Vector3 randomOffset = forward * Random.Range(-0.2f, 0.2f);
            Vector3 direction = (radialDir + randomOffset).normalized;
            Vector3 endPos = startPos + direction * lightningLength;
            
            // Smooth lightning points oluştur
            Vector3[] points = GenerateSmoothLightningPoints(startPos, endPos, lightningSegments, lightningJagged);
            
            // Glow ve core layer'ları güncelle
            if (i < lightningGlows.Count && lightningGlows[i] != null)
            {
                lightningGlows[i].positionCount = points.Length;
                lightningGlows[i].SetPositions(points);
            }
            if (i < lightningCores.Count && lightningCores[i] != null)
            {
                lightningCores[i].positionCount = points.Length;
                lightningCores[i].SetPositions(points);
            }
        }
    }
    
    Vector3[] GenerateSmoothLightningPoints(Vector3 start, Vector3 end, int segmentCount, float jagged)
    {
        Vector3[] points = new Vector3[segmentCount + 1];
        points[0] = start;
        points[segmentCount] = end;
        
        Vector3 dir = (end - start).normalized;
        
        // Perpendicular vektörler
        Vector3 perp1 = Vector3.Cross(dir, Vector3.up).normalized;
        if (perp1.magnitude < 0.1f) perp1 = Vector3.Cross(dir, Vector3.right).normalized;
        Vector3 perp2 = Vector3.Cross(dir, perp1).normalized;
        
        for (int i = 1; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 basePoint = Vector3.Lerp(start, end, t);
            
            // Uçlara yaklaştıkça sapma azalsın
            float edgeFalloff = Mathf.Sin(t * Mathf.PI);
            
            // Perlin noise + random
            float noiseX = (Mathf.PerlinNoise(i * 0.3f + Time.time, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, i * 0.3f + Time.time) - 0.5f) * 2f;
            noiseX = Mathf.Lerp(noiseX, Random.Range(-1f, 1f), 0.4f);
            noiseY = Mathf.Lerp(noiseY, Random.Range(-1f, 1f), 0.4f);
            
            float offsetX = noiseX * jagged * edgeFalloff;
            float offsetY = noiseY * jagged * edgeFalloff;
            
            points[i] = basePoint + perp1 * offsetX + perp2 * offsetY;
        }
        
        return points;
    }

    void OnDestroy()
    {
        // Temizlik
        if (flameTransform != null && flameTransform.gameObject != null)
        {
            Destroy(flameTransform.gameObject);
        }
        if (rocketLight != null && rocketLight.gameObject != null)
        {
            Destroy(rocketLight.gameObject);
        }
        if (trailCore != null && trailCore.gameObject != null)
        {
            Destroy(trailCore.gameObject);
        }
        if (trailMain != null && trailMain.gameObject != null)
        {
            Destroy(trailMain.gameObject);
        }
        if (trailGlow != null && trailGlow.gameObject != null)
        {
            Destroy(trailGlow.gameObject);
        }
        if (lightningContainer != null)
        {
            Destroy(lightningContainer);
        }
    }
}
