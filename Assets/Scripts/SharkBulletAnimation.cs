using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Köpek Balığı Mermi Animasyonu - Bullet.cs ile birlikte çalışır.
/// Sadece görsel efektleri ve animasyonları ekler, hareket ve hasar Bullet.cs tarafından yönetilir.
/// 
/// Özellikler:
/// - Mesh forward axis etrafında dönme
/// - Mesh'in etrafında parlayan aura
/// - Mesh'in üzerinde akan enerji çizgileri
/// - Güçlü glow efekti
/// - Fire laser tarzı çok katmanlı trail efekti
/// </summary>
public class SharkBulletAnimation : MonoBehaviour
{
    [Header("=== DÖNME ANİMASYONU ===")]
    [Tooltip("Mesh dönme animasyonu aktif mi?")]
    public bool enableRotation = true;
    
    [Tooltip("Dönme hızı (derece/saniye)")]
    public float rotationSpeed = 720f;
    
    [Tooltip("Dönüş ekseni (varsayılan: forward)")]
    public Vector3 spinAxis = Vector3.forward;
    
    [Tooltip("Rastgele dönüş hızı ekle")]
    public bool randomizeSpinSpeed = true;
    
    [Tooltip("Rastgele dönüş hızı varyasyonu")]
    [Range(0f, 1f)]
    public float spinSpeedVariation = 0.3f;
    
    [Header("=== AURA EFEKTİ ===")]
    [Tooltip("Aura efekti aktif mi?")]
    public bool enableAura = true;
    
    [Tooltip("Aura yarıçapı")]
    public float auraRadius = 0.12f;
    
    [Tooltip("Aura kalınlığı")]
    public float auraWidth = 0.03f;
    
    [Tooltip("Aura rengi")]
    public Color auraColor = new Color(0.4f, 0.8f, 1f, 0.6f); // Parlak mavi
    
    [Tooltip("Aura pulse hızı")]
    public float auraPulseSpeed = 2f;
    
    [Tooltip("Aura pulse miktarı")]
    public float auraPulseAmount = 0.2f;
    
    [Header("=== ENERJİ ÇİZGİLERİ ===")]
    [Tooltip("Enerji çizgileri aktif mi?")]
    public bool enableEnergyLines = true;
    
    [Tooltip("Çizgi sayısı")]
    public int energyLineCount = 8;
    
    [Tooltip("Çizgi uzunluğu")]
    public float energyLineLength = 0.15f;
    
    [Tooltip("Çizgi kalınlığı")]
    public float energyLineWidth = 0.01f;
    
    [Tooltip("Çizgi rengi")]
    public Color energyLineColor = new Color(0.6f, 0.9f, 1f, 0.9f); // Parlak mavi
    
    [Tooltip("Çizgi akış hızı")]
    public float energyFlowSpeed = 3f;
    
    [Tooltip("Çizgi dönme hızı")]
    public float energyRotationSpeed = 90f;
    
    [Header("=== GLOW EFEKTİ ===")]
    [Tooltip("Glow efekti aktif mi?")]
    public bool enableGlow = true;
    
    [Tooltip("Glow parlaklığı")]
    public float glowIntensity = 4f; // Çok parlak
    
    [Tooltip("Glow rengi")]
    public Color glowColor = new Color(0.4f, 0.8f, 1f, 1f); // Parlak mavi
    
    [Tooltip("Glow titreşim hızı")]
    public float glowPulseSpeed = 3f;
    
    [Tooltip("Glow titreşim miktarı")]
    public float glowPulseAmount = 0.4f;
    
    [Header("=== PARÇACIK EFEKTİ ===")]
    [Tooltip("Parçacık efekti aktif mi?")]
    public bool enableParticles = true;
    
    [Tooltip("Parçacık hızı")]
    public float particleRate = 30f;
    
    [Tooltip("Parçacık boyutu")]
    public float particleSize = 0.02f;
    
    [Tooltip("Parçacık rengi")]
    public Color particleColor = new Color(0.5f, 0.85f, 1f, 0.8f);
    
    [Header("=== TRAIL EFEKTİ (FIRE LASER STYLE) ===")]
    [Tooltip("Trail efekti aktif mi?")]
    public bool enableTrail = true;
    
    [Tooltip("Trail süresi")]
    public float trailTime = 0.4f;
    
    [Tooltip("Ana trail genişliği")]
    public float trailMainWidth = 0.25f;
    
    [Tooltip("Glow trail genişliği")]
    public float trailGlowWidth = 0.5f;
    
    [Tooltip("Core trail genişliği")]
    public float trailCoreWidth = 0.1f;
    
    [Tooltip("Trail rengi (ana)")]
    public Color trailMainColor = new Color(0.2f, 0.6f, 1f, 0.8f); // Mavi
    
    [Tooltip("Trail rengi (glow)")]
    public Color trailGlowColor = new Color(0.1f, 0.4f, 0.9f, 0.3f); // Koyu mavi glow
    
    [Tooltip("Trail rengi (core - parlak)")]
    public Color trailCoreColor = new Color(0.6f, 0.9f, 1f, 1f); // Açık mavi parlak
    
    // Private değişkenler
    private Transform modelTransform; // Ana model transformu (child)
    private float currentSpinSpeed;
    private LineRenderer auraRing;
    private List<LineRenderer> energyLines = new List<LineRenderer>();
    private TrailRenderer trailGlow;
    private TrailRenderer trailMain;
    private TrailRenderer trailCore;
    private ParticleSystem particleSystem;
    private Renderer[] renderers;
    private Light glowLight;
    
    private Quaternion baseRotation;
    private float timeOffset;

    private void Awake()
    {
        // Model transformunu bul (SADECE child, ana transform'a dokunma!)
        if (transform.childCount > 0)
        {
            modelTransform = transform.GetChild(0);
        }
        else
        {
            modelTransform = null;
            enableRotation = false;
        }
        
        // Base rotasyonu sakla
        if (modelTransform != null)
        {
            baseRotation = modelTransform.localRotation;
        }
        
        // Dönüş hızını ayarla (RocketAnimation gibi)
        if (randomizeSpinSpeed)
        {
            float variation = 1f + Random.Range(-spinSpeedVariation, spinSpeedVariation);
            currentSpinSpeed = rotationSpeed * variation;
        }
        else
        {
            currentSpinSpeed = rotationSpeed;
        }
        
        // Rastgele zaman offset'i
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
        
        // Renderer'ları bul
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        // Trail oluştur
        if (enableTrail)
        {
            CreateTrail();
        }
        
        // Aura efekti oluştur
        if (enableAura)
        {
            CreateAura();
        }
        
        // Enerji çizgileri oluştur
        if (enableEnergyLines)
        {
            CreateEnergyLines();
        }
        
        // Glow efekti oluştur
        if (enableGlow)
        {
            CreateGlowEffect();
        }
        
        // Parçacık efekti oluştur
        if (enableParticles)
        {
            CreateParticleSystem();
        }
    }

    private void Update()
    {
        float time = Time.time + timeOffset;
        
        // Mesh dönme animasyonu
        if (enableRotation && modelTransform != null)
        {
            AnimateRotation(time);
        }
        
        // Aura efektini güncelle
        if (enableAura)
        {
            UpdateAura(time);
        }
        
        // Enerji çizgilerini güncelle
        if (enableEnergyLines)
        {
            UpdateEnergyLines(time);
        }
        
        // Glow efektini güncelle
        if (enableGlow)
        {
            UpdateGlowEffect(time);
        }
    }

    private void AnimateRotation(float time)
    {
        if (modelTransform == null) return;
        
        // Spin axis etrafında dönme (RocketAnimation gibi)
        float angle = currentSpinSpeed * Time.deltaTime;
        modelTransform.Rotate(spinAxis.normalized, angle, Space.Self);
    }

    private void CreateAura()
    {
        GameObject auraObj = new GameObject("AuraRing");
        auraObj.transform.SetParent(transform);
        auraObj.transform.localPosition = Vector3.zero;
        auraRing = auraObj.AddComponent<LineRenderer>();
        
        auraRing.material = CreateAuraMaterial(auraColor);
        auraRing.startWidth = auraWidth;
        auraRing.endWidth = auraWidth;
        auraRing.useWorldSpace = false;
        auraRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        auraRing.receiveShadows = false;
        auraRing.sortingOrder = 0;
        auraRing.loop = true;
        auraRing.enabled = true;
        
        // Dairesel aura için segment
        int segments = 64;
        auraRing.positionCount = segments;
        
        // Smooth görünüm
        auraRing.numCapVertices = 10;
        auraRing.numCornerVertices = 10;
        
        // Gradient renk
        Gradient gradient = new Gradient();
        Color brightColor = Color.Lerp(auraColor, Color.white, 0.4f);
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(brightColor, 0f),
                new GradientColorKey(auraColor, 0.5f),
                new GradientColorKey(brightColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(auraColor.a, 0f),
                new GradientAlphaKey(auraColor.a * 0.7f, 0.5f),
                new GradientAlphaKey(auraColor.a, 1f)
            }
        );
        auraRing.colorGradient = gradient;
    }

    private void UpdateAura(float time)
    {
        if (auraRing == null) return;
        
        // Aura yarıçapı pulse yapıyor
        float pulse = 1f + Mathf.Sin(time * auraPulseSpeed) * auraPulseAmount;
        float radius = auraRadius * pulse;
        
        // Aura pozisyonları (dairesel)
        int segments = auraRing.positionCount;
        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            // XY düzleminde (horizontal)
            auraRing.SetPosition(i, new Vector3(x, y, 0f));
        }
        
        // Kalınlık pulse
        float widthPulse = 1f + Mathf.Sin(time * auraPulseSpeed * 1.3f) * 0.3f;
        auraRing.startWidth = auraWidth * widthPulse;
        auraRing.endWidth = auraWidth * widthPulse;
    }

    private void CreateEnergyLines()
    {
        GameObject linesContainer = new GameObject("EnergyLines");
        linesContainer.transform.SetParent(transform);
        linesContainer.transform.localPosition = Vector3.zero;
        
        for (int i = 0; i < energyLineCount; i++)
        {
            GameObject lineObj = new GameObject($"EnergyLine_{i}");
            lineObj.transform.SetParent(linesContainer.transform);
            lineObj.transform.localPosition = Vector3.zero;
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            SetupEnergyLine(line, energyLineWidth, energyLineColor, i);
            energyLines.Add(line);
        }
    }

    private void SetupEnergyLine(LineRenderer line, float width, Color color, int index)
    {
        line.material = CreateEnergyLineMaterial(color);
        line.startWidth = width;
        line.endWidth = width * 0.5f;
        line.useWorldSpace = false;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sortingOrder = 1;
        line.positionCount = 3; // Kısa çizgi (mesh üzerinde akan)
        line.enabled = true;
        
        line.numCapVertices = 3;
        line.numCornerVertices = 3;
    }

    private Material CreateAuraMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            mat = new Material(Shader.Find("Sprites/Default"));
        }
        mat.SetColor("_Color", color);
        mat.color = color;
        return mat;
    }

    private Material CreateEnergyLineMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Unlit/Color"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            mat = new Material(Shader.Find("Sprites/Default"));
        }
        mat.SetColor("_Color", color);
        mat.color = color;
        return mat;
    }

    private void UpdateEnergyLines(float time)
    {
        if (energyLines.Count == 0) return;
        
        float radius = auraRadius * 0.7f; // Aura'nın içinde
        
        for (int i = 0; i < energyLineCount && i < energyLines.Count; i++)
        {
            if (energyLines[i] == null) continue;
            
            // Çizginin açısı (dönüyor)
            float baseAngle = (360f / energyLineCount) * i;
            float rotationAngle = baseAngle + time * energyRotationSpeed;
            float rad = rotationAngle * Mathf.Deg2Rad;
            
            // Akış efekti (ileri doğru akan)
            float flowOffset = (time * energyFlowSpeed + i * 0.5f) % energyLineLength;
            
            // Başlangıç pozisyonu (mesh yüzeyinde)
            Vector3 startPos = new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );
            
            // Bitiş pozisyonu (ileri doğru akan)
            Vector3 direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
            Vector3 endPos = startPos + direction * (energyLineLength - flowOffset);
            
            // Orta nokta (smooth geçiş için)
            Vector3 midPos = Vector3.Lerp(startPos, endPos, 0.5f);
            
            energyLines[i].SetPosition(0, startPos);
            energyLines[i].SetPosition(1, midPos);
            energyLines[i].SetPosition(2, endPos);
        }
    }

    private void CreateGlowEffect()
    {
        // Point light ekle
        GameObject lightObj = new GameObject("GlowLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = glowColor;
        glowLight.intensity = glowIntensity;
        glowLight.range = 1.5f;
        glowLight.shadows = LightShadows.None;
        
        // Material glow ekle
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null)
            {
                Material mat = renderer.material;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
            }
        }
    }

    private void UpdateGlowEffect(float time)
    {
        if (glowLight != null)
        {
            // Glow titreşimi (daha dinamik)
            float pulse = 1f + Mathf.Sin(time * glowPulseSpeed) * glowPulseAmount;
            glowLight.intensity = glowIntensity * pulse;
            
            // Renk titreşimi (daha canlı)
            float colorPulse = Mathf.Sin(time * glowPulseSpeed * 0.7f);
            Color pulseColor = Color.Lerp(glowColor, glowColor * 1.4f, colorPulse * 0.5f + 0.5f);
            glowLight.color = pulseColor;
            
            // Range de pulse yapsın
            glowLight.range = 1.5f + Mathf.Sin(time * glowPulseSpeed * 0.6f) * 0.3f;
        }
        
        // Material glow'u da güncelle
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material != null && renderer.material.HasProperty("_EmissionColor"))
            {
                float materialPulse = 1f + Mathf.Sin(time * glowPulseSpeed * 0.8f) * glowPulseAmount * 0.5f;
                renderer.material.SetColor("_EmissionColor", glowColor * glowIntensity * materialPulse);
            }
        }
    }

    private void CreateParticleSystem()
    {
        GameObject particleObj = new GameObject("SharkParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        particleObj.transform.localRotation = Quaternion.identity;
        
        particleSystem = particleObj.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSize * 0.8f, particleSize * 1.2f);
        main.startColor = particleColor;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 50;
        main.gravityModifier = -0.1f;
        
        var emission = particleSystem.emission;
        emission.rateOverTime = particleRate;
        
        var shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = auraRadius * 0.8f;
        
        // Size over lifetime
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime
        var colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(particleColor, 0f), 
                new GradientColorKey(particleColor * 1.5f, 0.3f),
                new GradientColorKey(particleColor * 0.3f, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(particleColor.a, 0f), 
                new GradientAlphaKey(particleColor.a * 0.9f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = -1;
    }

    private void CreateTrail()
    {
        // Fire laser tarzı çok katmanlı trail
        
        // 1. GLOW TRAIL (Dış glow - en geniş)
        GameObject glowObj = new GameObject("TrailGlow");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        trailGlow = glowObj.AddComponent<TrailRenderer>();
        SetupTrail(trailGlow, trailGlowWidth, trailGlowColor, 0.3f, 0);
        
        // 2. MAIN TRAIL (Ana trail)
        GameObject mainObj = new GameObject("TrailMain");
        mainObj.transform.SetParent(transform);
        mainObj.transform.localPosition = Vector3.zero;
        trailMain = mainObj.AddComponent<TrailRenderer>();
        SetupTrail(trailMain, trailMainWidth, trailMainColor, 0.4f, 1);
        
        // 3. CORE TRAIL (Parlak çekirdek - en dar)
        GameObject coreObj = new GameObject("TrailCore");
        coreObj.transform.SetParent(transform);
        coreObj.transform.localPosition = Vector3.zero;
        trailCore = coreObj.AddComponent<TrailRenderer>();
        SetupTrail(trailCore, trailCoreWidth, trailCoreColor, 0.35f, 2);
    }

    private void SetupTrail(TrailRenderer trail, float width, Color baseColor, float time, int sortingOrder)
    {
        trail.time = time;
        trail.minVertexDistance = 0.05f;
        trail.textureMode = LineTextureMode.Stretch;
        trail.material = CreateTrailMaterial(baseColor);
        trail.sortingOrder = sortingOrder;
        
        // Smooth width curve (fire laser gibi)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);   // Başta biraz dar
        widthCurve.AddKey(0.1f, 1f);  // Hemen genişle
        widthCurve.AddKey(0.5f, 1.05f); // Ortada biraz şişkin
        widthCurve.AddKey(0.9f, 0.9f);  // Sonlara doğru daral
        widthCurve.AddKey(1f, 0.5f);    // En sonda çok dar
        trail.widthCurve = widthCurve;
        trail.widthMultiplier = width;
        
        // Smooth gradient (fire laser efektine benzer)
        Gradient gradient = new Gradient();
        Color brightColor = Color.Lerp(baseColor, Color.white, 0.4f);
        Color midColor = baseColor;
        
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
        
        // Smooth köşeler
        trail.numCapVertices = 5;
        trail.numCornerVertices = 5;
    }

    private Material CreateTrailMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetColor("_Color", color);
        return mat;
    }

    private void OnDestroy()
    {
        if (trailGlow != null && trailGlow.gameObject != null)
            Destroy(trailGlow.gameObject);
        if (trailMain != null && trailMain.gameObject != null)
            Destroy(trailMain.gameObject);
        if (trailCore != null && trailCore.gameObject != null)
            Destroy(trailCore.gameObject);
        if (particleSystem != null && particleSystem.gameObject != null)
            Destroy(particleSystem.gameObject);
    }
}
