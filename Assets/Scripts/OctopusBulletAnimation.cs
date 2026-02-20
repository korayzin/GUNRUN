using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ahtapot mermisi animasyonu - Bullet.cs ile birlikte çalışır.
/// RocketAnimation'a benzer yapıda, ahtapot temasına özel efektler.
/// 
/// Özellikler:
/// - Dönüş animasyonu
/// - Mürekkep püskürtme partikül efekti (alev yerine)
/// - İnk trail (mor/violet tonlarında)
/// - 8 dokunaç benzeri dalgalı çizgiler (yıldırım yerine)
/// - Mor glow ışık efekti
/// </summary>
public class OctopusBulletAnimation : MonoBehaviour
{
    [Header("=== AHTAPOT DÖNÜŞ ANİMASYONU ===")]
    [Tooltip("Dönüş hızı (derece/saniye)")]
    public float spinSpeed = 540f;
    
    [Tooltip("Dönüş ekseni (varsayılan: forward)")]
    public Vector3 spinAxis = Vector3.forward;
    
    [Tooltip("Rastgele dönüş hızı ekle")]
    public bool randomizeSpinSpeed = true;
    
    [Tooltip("Rastgele dönüş hızı varyasyonu")]
    [Range(0f, 1f)]
    public float spinSpeedVariation = 0.25f;

    [Header("=== MÜREKKEP PÜSKÜRTME EFEKTİ ===")]
    [Tooltip("Mürekkep partikül efekti aktif mi?")]
    public bool enableInkEffect = true;
    
    [Tooltip("Mürekkep pozisyonu (merminin arkasına göre offset)")]
    public Vector3 inkOffset = new Vector3(0f, 0f, -0.25f);
    
    [Tooltip("Mürekkep partikül boyutu")]
    public float inkParticleSize = 0.08f;
    
    [Tooltip("Mürekkep çekirdek rengi (parlak mor)")]
    public Color inkCoreColor = new Color(0.9f, 0.7f, 1f, 1f);
    
    [Tooltip("Mürekkep ana rengi")]
    public Color inkStartColor = new Color(0.5f, 0.2f, 0.8f, 0.9f);
    
    [Tooltip("Mürekkep rengi (soluk - dağılma)")]
    public Color inkEndColor = new Color(0.3f, 0.1f, 0.5f, 0f);
    
    [Tooltip("Mürekkep yoğunluğu")]
    [Range(30f, 120f)]
    public float inkIntensity = 70f;
    
    [Tooltip("Mürekkep parçacık sayısı")]
    [Range(40f, 200f)]
    public float inkParticleCount = 100f;

    [Header("=== İNK TRAIL (İZ) ===")]
    [Tooltip("Trail renderer aktif mi?")]
    public bool enableTrail = true;
    
    [Tooltip("Trail süresi")]
    public float trailTime = 0.45f;
    
    [Tooltip("Ana trail genişliği (başlangıç)")]
    public float trailStartWidth = 0.12f;
    
    [Tooltip("Ana trail genişliği (bitiş)")]
    public float trailEndWidth = 0.02f;
    
    [Tooltip("Glow trail genişliği (başlangıç)")]
    public float glowTrailStartWidth = 0.25f;
    
    [Tooltip("Glow trail genişliği (bitiş)")]
    public float glowTrailEndWidth = 0.06f;
    
    [Tooltip("Trail çekirdek rengi (parlak merkez)")]
    public Color trailCoreColor = new Color(0.9f, 0.8f, 1f, 1f);
    
    [Tooltip("Trail ana rengi (mürekkep mor)")]
    public Color trailMainColor = new Color(0.5f, 0.25f, 0.9f, 0.85f);
    
    [Tooltip("Trail glow rengi (dış halo)")]
    public Color trailGlowColor = new Color(0.4f, 0.15f, 0.7f, 0.35f);

    [Header("=== AHTAPOT IŞIK EFEKTİ ===")]
    [Tooltip("Işık efekti aktif mi?")]
    public bool enableLight = true;
    
    [Tooltip("Işık şiddeti")]
    public float lightIntensity = 1.8f;
    
    [Tooltip("Işık menzili")]
    public float lightRange = 4f;
    
    [Tooltip("Işık rengi (mor)")]
    public Color lightColor = new Color(0.6f, 0.3f, 1f);

    [Header("=== DOKUNAÇ EFEKTLERİ (8 KOL) ===")]
    [Tooltip("Dokunaç benzeri dalgalı çizgiler aktif mi?")]
    public bool enableTentacles = true;
    
    [Tooltip("Dokunaç sayısı (ahtapot 8 kollu)")]
    [Range(4, 12)]
    public int tentacleCount = 8;
    
    [Tooltip("Dokunaç uzunluğu")]
    public float tentacleLength = 0.25f;
    
    [Tooltip("Dalga genliği (organik hareket)")]
    [Range(0.02f, 0.15f)]
    public float tentacleWaveAmplitude = 0.06f;
    
    [Tooltip("Dalga frekansı")]
    public float tentacleWaveFrequency = 4f;
    
    [Tooltip("Dokunaç segment sayısı")]
    [Range(4, 16)]
    public int tentacleSegments = 10;
    
    [Tooltip("Dokunaç çekirdek rengi")]
    public Color tentacleCoreColor = new Color(0.9f, 0.7f, 1f, 0.9f);
    
    [Tooltip("Dokunaç glow rengi")]
    public Color tentacleGlowColor = new Color(0.5f, 0.2f, 0.8f, 0.5f);
    
    [Tooltip("Dokunaç güncelleme hızı")]
    public float tentacleUpdateRate = 0.04f;
    
    [Tooltip("Dokunaç kalınlığı (çekirdek)")]
    public float tentacleCoreWidth = 0.015f;
    
    [Tooltip("Dokunaç kalınlığı (glow)")]
    public float tentacleGlowWidth = 0.06f;

    // Private değişkenler
    private float currentSpinSpeed;
    private Quaternion targetRotation;
    private ParticleSystem inkParticles;
    private Transform inkTransform;
    private TrailRenderer trailCore;
    private TrailRenderer trailMain;
    private TrailRenderer trailGlow;
    private Light octopusLight;
    private GameObject tentacleContainer;
    private List<LineRenderer> tentacleCores = new List<LineRenderer>();
    private List<LineRenderer> tentacleGlows = new List<LineRenderer>();
    private float tentacleTimer = 0f;

    void Start()
    {
        // Dönüş hızını ayarla
        if (randomizeSpinSpeed)
        {
            float variation = 1f + Random.Range(-spinSpeedVariation, spinSpeedVariation);
            currentSpinSpeed = spinSpeed * variation;
        }
        else
        {
            currentSpinSpeed = spinSpeed;
        }
        targetRotation = transform.rotation;

        // Mürekkep efekti oluştur
        if (enableInkEffect)
        {
            CreateInkEffect();
        }

        // Trail oluştur
        if (enableTrail)
        {
            CreateTrail();
        }

        // Işık efekti oluştur
        if (enableLight)
        {
            CreateLight();
        }
        
        // Dokunaç efektleri oluştur
        if (enableTentacles)
        {
            CreateTentacleEffects();
        }
    }

    void Update()
    {
        // Smooth dönüş animasyonu
        targetRotation *= Quaternion.AngleAxis(currentSpinSpeed * Time.deltaTime, spinAxis.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        
        // Dokunaç efektlerini güncelle
        if (enableTentacles)
        {
            tentacleTimer += Time.deltaTime;
            if (tentacleTimer >= tentacleUpdateRate)
            {
                tentacleTimer = 0f;
                UpdateTentacleEffects();
            }
        }

        // Mürekkep pozisyonunu güncelle
        if (inkParticles != null && inkTransform != null)
        {
            inkTransform.position = transform.position + transform.TransformDirection(inkOffset);
            inkTransform.rotation = transform.rotation;
        }

        // Işık efektini güncelle (yumuşak titreşim)
        if (octopusLight != null)
        {
            octopusLight.transform.position = transform.position;
            octopusLight.transform.rotation = transform.rotation;
            float pulse = 0.85f + Mathf.Sin(Time.time * 3f) * 0.15f;
            octopusLight.intensity = lightIntensity * pulse;
        }
    }

    void CreateInkEffect()
    {
        GameObject inkObj = new GameObject("OctopusInk");
        inkObj.transform.SetParent(transform);
        inkTransform = inkObj.transform;
        inkTransform.localPosition = inkOffset;
        inkTransform.localRotation = Quaternion.identity;

        inkParticles = inkObj.AddComponent<ParticleSystem>();
        var main = inkParticles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(inkParticleSize * 0.6f, inkParticleSize * 1.4f);
        main.startColor = new ParticleSystem.MinMaxGradient(inkStartColor, inkEndColor);
        main.maxParticles = Mathf.RoundToInt(inkParticleCount);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startRotation3D = true;
        main.gravityModifier = -0.2f; // Hafif yukarı (su gibi)
        
        var emission = inkParticles.emission;
        emission.rateOverTime = inkIntensity;
        emission.enabled = true;

        var shape = inkParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f; // Daha geniş açı - mürekkep püskürtmesi
        shape.radius = 0.02f;

        var velocity = inkParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.z = new ParticleSystem.MinMaxCurve(1.5f, 4f);

        var colorOverLifetime = inkParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(inkCoreColor, 0f),
                new GradientColorKey(inkStartColor, 0.25f),
                new GradientColorKey(inkStartColor, 0.6f),
                new GradientColorKey(inkEndColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0.4f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = inkParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.4f, 1.2f);  // Hafif büyüme (dağılma)
        sizeCurve.AddKey(1f, 0.3f);    // Küçülerek kaybolma
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = inkParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 1;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    void CreateTrail()
    {
        // Çok katmanlı ink trail
        
        GameObject glowObj = new GameObject("TrailGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        trailGlow = glowObj.AddComponent<TrailRenderer>();
        SetupTrail(trailGlow, glowTrailStartWidth, glowTrailEndWidth, trailGlowColor, 0.3f, 0);
        
        GameObject mainObj = new GameObject("TrailMain");
        mainObj.transform.SetParent(transform);
        mainObj.transform.localPosition = Vector3.zero;
        trailMain = mainObj.AddComponent<TrailRenderer>();
        SetupTrail(trailMain, trailStartWidth, trailEndWidth, trailMainColor, trailTime, 1);
        
        GameObject coreObj = new GameObject("TrailCore");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        trailCore = coreObj.AddComponent<TrailRenderer>();
        SetupTrail(trailCore, trailStartWidth * 0.4f, trailEndWidth * 0.4f, trailCoreColor, 0.4f, 2);
    }
    
    void SetupTrail(TrailRenderer trail, float startWidth, float endWidth, Color baseColor, float time, int sortingOrder)
    {
        trail.time = time;
        trail.minVertexDistance = 0.05f;
        trail.textureMode = LineTextureMode.Stretch;
        trail.material = CreateTrailMaterial(baseColor);
        trail.sortingOrder = sortingOrder;
        
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);
        widthCurve.AddKey(0.1f, 1f);
        widthCurve.AddKey(0.5f, 1.05f);
        widthCurve.AddKey(0.9f, 0.9f);
        widthCurve.AddKey(1f, 0.5f);
        trail.widthCurve = widthCurve;
        trail.widthMultiplier = startWidth;
        
        Gradient gradient = new Gradient();
        Color brightColor = Color.Lerp(baseColor, Color.white, 0.4f);
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),
                new GradientColorKey(baseColor, 0.15f),
                new GradientColorKey(baseColor, 0.85f),
                new GradientColorKey(brightColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(baseColor.a * 0.9f, 0f),
                new GradientAlphaKey(baseColor.a, 0.2f),
                new GradientAlphaKey(baseColor.a, 0.8f),
                new GradientAlphaKey(baseColor.a * 0.3f, 1f)
            }
        );
        trail.colorGradient = gradient;
        
        trail.numCapVertices = 5;
        trail.numCornerVertices = 5;
    }

    Material CreateTrailMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }

    void CreateLight()
    {
        GameObject lightObj = new GameObject("OctopusLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        octopusLight = lightObj.AddComponent<Light>();
        octopusLight.type = LightType.Point;
        octopusLight.color = lightColor;
        octopusLight.intensity = lightIntensity;
        octopusLight.range = lightRange;
        octopusLight.shadows = LightShadows.None;
    }

    void CreateTentacleEffects()
    {
        tentacleContainer = new GameObject("TentacleContainer");
        tentacleContainer.transform.position = transform.position;
        tentacleContainer.transform.rotation = transform.rotation;
        
        for (int i = 0; i < tentacleCount; i++)
        {
            LineRenderer glow = CreateTentacleLine(tentacleGlowWidth, tentacleGlowColor);
            tentacleGlows.Add(glow);
            
            LineRenderer core = CreateTentacleLine(tentacleCoreWidth, tentacleCoreColor);
            tentacleCores.Add(core);
        }
        
        UpdateTentacleEffects();
    }
    
    LineRenderer CreateTentacleLine(float width, Color color)
    {
        GameObject obj = new GameObject("Tentacle");
        obj.transform.SetParent(tentacleContainer.transform);
        
        LineRenderer line = obj.AddComponent<LineRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;
        
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(0.5f, 0.8f);
        widthCurve.AddKey(1f, 0.2f);  // Uçta incelerek biten dokunaç
        line.widthCurve = widthCurve;
        line.widthMultiplier = width;
        
        line.useWorldSpace = true;
        line.numCapVertices = 3;
        line.numCornerVertices = 5;
        line.textureMode = LineTextureMode.Stretch;
        
        Gradient grad = new Gradient();
        Color brightColor = Color.Lerp(color, Color.white, 0.3f);
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),
                new GradientColorKey(color, 0.2f),
                new GradientColorKey(color, 0.8f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(color.a, 0.5f),
                new GradientAlphaKey(color.a * 0.5f, 1f)
            }
        );
        line.colorGradient = grad;
        
        return line;
    }
    
    void UpdateTentacleEffects()
    {
        if (tentacleContainer == null) return;
        
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = transform.up;
        
        tentacleContainer.transform.position = pos;
        tentacleContainer.transform.rotation = transform.rotation;
        
        for (int i = 0; i < tentacleCount; i++)
        {
            // 8 dokunaç dairesel dağılım, hafif dönüş
            float baseAngle = (360f / tentacleCount) * i;
            float rotationOffset = Time.time * 60f;  // Yavaş dönüş
            float angle = (baseAngle + rotationOffset) * Mathf.Deg2Rad;
            
            Vector3 radialDir = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)).normalized;
            Vector3 startPos = pos + radialDir * 0.06f;
            
            // İleri + radyal yön (dokunaçlar geriye doğru süzülüyor)
            Vector3 direction = (-forward * 0.7f + radialDir * 0.3f).normalized;
            
            // Dalgalı dokunaç noktaları (organik, yumuşak)
            Vector3[] points = GenerateTentaclePoints(startPos, direction, tentacleLength, tentacleSegments, i);
            
            if (i < tentacleGlows.Count && tentacleGlows[i] != null)
            {
                tentacleGlows[i].positionCount = points.Length;
                tentacleGlows[i].SetPositions(points);
            }
            if (i < tentacleCores.Count && tentacleCores[i] != null)
            {
                tentacleCores[i].positionCount = points.Length;
                tentacleCores[i].SetPositions(points);
            }
        }
    }
    
    Vector3[] GenerateTentaclePoints(Vector3 start, Vector3 direction, float length, int segmentCount, int tentacleIndex)
    {
        Vector3[] points = new Vector3[segmentCount + 1];
        points[0] = start;
        
        Vector3 perp = Vector3.Cross(direction, Vector3.up).normalized;
        if (perp.magnitude < 0.1f) perp = Vector3.Cross(direction, Vector3.right).normalized;
        
        for (int i = 1; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 basePoint = start + direction * length * t;
            
            // Yumuşak sinüs dalgası - her dokunacın farklı fazı
            float phase = tentacleIndex * 0.8f + Time.time * tentacleWaveFrequency;
            float wave = Mathf.Sin(t * Mathf.PI * 2f + phase) * tentacleWaveAmplitude * t;
            
            points[i] = basePoint + perp * wave;
        }
        
        return points;
    }

    void OnDestroy()
    {
        if (inkTransform != null && inkTransform.gameObject != null)
            Destroy(inkTransform.gameObject);
        if (octopusLight != null && octopusLight.gameObject != null)
            Destroy(octopusLight.gameObject);
        if (trailCore != null && trailCore.gameObject != null)
            Destroy(trailCore.gameObject);
        if (trailMain != null && trailMain.gameObject != null)
            Destroy(trailMain.gameObject);
        if (trailGlow != null && trailGlow.gameObject != null)
            Destroy(trailGlow.gameObject);
        if (tentacleContainer != null)
            Destroy(tentacleContainer);
    }
}
