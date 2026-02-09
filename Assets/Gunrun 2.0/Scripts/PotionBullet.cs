using UnityEngine;

/// <summary>
/// Potion Gun Bullet Visual Effects - Bullet.cs ile birlikte çalışır.
/// Sadece görsel efektleri ekler, hareket ve hasar Bullet.cs tarafından yönetilir.
/// 
/// Özellikler:
/// - Wobble efekti (içindeki sıvı sallanıyor)
/// - Spin efekti (yavaş dönme)
/// - Sparkle trail (arkasından parıltılar)
/// - Smooth ribbon trail (topu takip eden yumuşak şerit)
/// - Soft spawn efekti
/// </summary>
public class PotionBullet : MonoBehaviour
{
    [Header("=== WOBBLE EFFECT (Sivi Sallantisi) ===")]
    [Tooltip("Wobble efekti aktif mi?")]
    public bool enableWobble = true;
    
    [Tooltip("Wobble hizi")]
    public float wobbleSpeed = 5f;
    
    [Tooltip("Wobble genisligi (derece)")]
    public float wobbleAmount = 8f;
    
    [Header("=== SELF ROTATION (Kendi Etrafinda Donme) ===")]
    [Tooltip("Kendi etrafinda donme aktif mi?")]
    public bool enableSelfRotation = true;
    
    [Tooltip("Donme hizi (derece/saniye)")]
    public float selfRotationSpeed = 360f;
    
    [Tooltip("Donme ekseni (0=X, 1=Y, 2=Z, 3=Forward)")]
    public int rotationAxis = 3; // Default: Forward axis (ucus yonu)
    
    [Header("=== SPARKLE PARTICLES ===")]
    [Tooltip("Sparkle trail aktif mi?")]
    public bool enableSparkles = true;
    
    [Tooltip("Saniyede kac sparkle")]
    public float sparkleRate = 20f;
    
    [Tooltip("Sparkle boyutu min")]
    public float sparkleMinSize = 0.015f;
    
    [Tooltip("Sparkle boyutu max")]
    public float sparkleMaxSize = 0.04f;
    
    [Tooltip("Sparkle ana rengi")]
    public Color sparkleColor1 = new Color(0.6f, 1f, 0.7f, 0.9f); // Soft yesil
    
    [Tooltip("Sparkle ikinci rengi")]
    public Color sparkleColor2 = new Color(1f, 0.95f, 0.6f, 0.9f); // Soft sari
    
    [Tooltip("Sparkle omru")]
    public float sparkleLifetime = 0.6f;
    
    [Header("=== RIBBON TRAIL ===")]
    [Tooltip("Ribbon trail aktif mi?")]
    public bool enableRibbonTrail = true;
    
    [Tooltip("Trail suresi")]
    public float trailTime = 0.4f;
    
    [Tooltip("Trail baslangic genisligi")]
    public float trailStartWidth = 0.06f;
    
    [Tooltip("Trail bitis genisligi")]
    public float trailEndWidth = 0f;
    
    [Tooltip("Trail rengi")]
    public Color trailColor = new Color(0.5f, 1f, 0.6f, 0.6f);
    
    [Tooltip("Trail smoothness (vertex mesafesi)")]
    public float trailSmoothness = 0.01f;
    
    [Header("=== SPAWN EFFECT ===")]
    [Tooltip("Spawn efekti aktif mi?")]
    public bool enableSpawnEffect = true;
    
    [Tooltip("Spawn particle sayisi")]
    public int spawnParticleCount = 12;
    
    [Tooltip("Spawn efekt rengi")]
    public Color spawnEffectColor = new Color(0.8f, 1f, 0.7f, 0.8f);
    
    [Header("=== HIT EFFECT (Baloncuk Patlama) ===")]
    [Tooltip("Hit efekti aktif mi?")]
    public bool enableHitEffect = true;
    
    [Tooltip("Hit efekt sprite'i (bos birakilirsa varsayilan daire)")]
    public Sprite hitSprite;
    
    [Tooltip("Ana baloncuk sayisi")]
    public int hitBubbleCount = 8;
    
    [Tooltip("Kucuk parcacik sayisi")]
    public int hitSparkleCount = 15;
    
    [Tooltip("Baloncuk boyutu min")]
    public float hitBubbleMinSize = 0.04f;
    
    [Tooltip("Baloncuk boyutu max")]
    public float hitBubbleMaxSize = 0.12f;
    
    [Tooltip("Hit efekt rengi 1")]
    public Color hitColor1 = new Color(0.4f, 1f, 0.6f, 0.9f); // Yesil
    
    [Tooltip("Hit efekt rengi 2")]
    public Color hitColor2 = new Color(0.9f, 1f, 0.5f, 0.9f); // Sari-yesil
    
    [Tooltip("Hit efekt rengi 3 (parlak)")]
    public Color hitColorBright = new Color(1f, 1f, 1f, 1f); // Beyaz flash
    
    [Header("=== AUDIO ===")]
    [Tooltip("Firlatma sesi")]
    public AudioClip launchSound;
    
    [Tooltip("Hit/patlama sesi")]
    public AudioClip hitSound;
    
    // Private variables
    private float wobbleTime = 0f;
    private bool hasHit = false;
    private TrailRenderer ribbonTrail;
    private ParticleSystem sparkleParticles;
    private Transform visualChild;
    
    private void Start()
    {
        // Wobble için ilk child'ı bul (varsa)
        if (transform.childCount > 0)
        {
            visualChild = transform.GetChild(0);
        }
        
        // Spawn efekti
        if (enableSpawnEffect)
        {
            CreateSpawnEffect();
        }
        
        // Sparkle particle system
        if (enableSparkles)
        {
            CreateSparkleSystem();
        }
        
        // Ribbon trail
        if (enableRibbonTrail)
        {
            CreateRibbonTrail();
        }
        
        // Launch sound
        if (launchSound != null)
        {
            AudioSource.PlayClipAtPoint(launchSound, transform.position, 0.6f);
        }
    }
    
    private void Update()
    {
        // Kendi etrafinda donme
        if (enableSelfRotation)
        {
            ApplySelfRotation();
        }
        
        // Wobble efekti (child'lara)
        if (enableWobble)
        {
            ApplyWobbleEffect();
        }
    }
    
    private void ApplyWobbleEffect()
    {
        wobbleTime += Time.unscaledDeltaTime * wobbleSpeed;
        
        // Smooth sinusoidal wobble
        float wobbleX = Mathf.Sin(wobbleTime) * wobbleAmount;
        float wobbleY = Mathf.Sin(wobbleTime * 0.8f + 0.5f) * wobbleAmount * 0.6f;
        
        if (visualChild != null)
        {
            // Smooth lerp for extra smoothness
            Quaternion targetRot = Quaternion.Euler(wobbleX, wobbleY, visualChild.localEulerAngles.z);
            visualChild.localRotation = Quaternion.Slerp(visualChild.localRotation, targetRot, Time.unscaledDeltaTime * 10f);
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<ParticleSystem>() != null) continue;
                Quaternion targetRot = Quaternion.Euler(wobbleX, wobbleY, child.localEulerAngles.z);
                child.localRotation = Quaternion.Slerp(child.localRotation, targetRot, Time.unscaledDeltaTime * 10f);
            }
        }
    }
    
    private void ApplySelfRotation()
    {
        float rotAmount = selfRotationSpeed * Time.unscaledDeltaTime;
        
        // Secilen eksene gore don
        switch (rotationAxis)
        {
            case 0: // X axis
                transform.Rotate(rotAmount, 0, 0, Space.Self);
                break;
            case 1: // Y axis
                transform.Rotate(0, rotAmount, 0, Space.Self);
                break;
            case 2: // Z axis
                transform.Rotate(0, 0, rotAmount, Space.Self);
                break;
            case 3: // Forward axis (ucus yonunde donme)
            default:
                transform.Rotate(Vector3.forward * rotAmount, Space.Self);
                break;
        }
    }
    
    private void CreateSpawnEffect()
    {
        GameObject spawnFX = new GameObject("PotionSpawnEffect");
        spawnFX.transform.position = transform.position;
        
        ParticleSystem ps = spawnFX.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
        main.startColor = spawnEffectColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = spawnParticleCount;
        main.gravityModifier = -0.3f; // Hafif yukari dogru
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, spawnParticleCount)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;
        
        // Boyut kuculme
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Smooth fade out
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(spawnEffectColor, 0f), 
                new GradientColorKey(spawnEffectColor, 0.3f),
                new GradientColorKey(spawnEffectColor * 0.5f, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f), 
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        
        ps.Play();
        Destroy(spawnFX, 1.5f);
    }
    
    private void CreateSparkleSystem()
    {
        GameObject sparkleObj = new GameObject("SparkleTrail");
        sparkleObj.transform.SetParent(transform);
        sparkleObj.transform.localPosition = Vector3.zero;
        sparkleObj.transform.localRotation = Quaternion.identity;
        
        sparkleParticles = sparkleObj.AddComponent<ParticleSystem>();
        
        var main = sparkleParticles.main;
        main.startLifetime = sparkleLifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(sparkleMinSize, sparkleMaxSize);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;
        main.gravityModifier = -0.1f; // Hafif yukari suzulme
        
        // Iki renk arasi random
        main.startColor = new ParticleSystem.MinMaxGradient(sparkleColor1, sparkleColor2);
        
        var emission = sparkleParticles.emission;
        emission.rateOverTime = sparkleRate;
        
        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.04f;
        
        // Smooth boyut degisimi - buyuyup kuculme
        var sizeOverLifetime = sparkleParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(new Keyframe(0f, 0.3f, 0f, 3f));
        sizeCurve.AddKey(new Keyframe(0.2f, 1f, 0f, 0f));
        sizeCurve.AddKey(new Keyframe(1f, 0f, -2f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Smooth fade
        var colorOverLifetime = sparkleParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.15f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Hafif rotation
        var rotationOverLifetime = sparkleParticles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-90f, 90f);
        
        var renderer = sparkleParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        sparkleParticles.Play();
    }
    
    private void CreateRibbonTrail()
    {
        ribbonTrail = gameObject.AddComponent<TrailRenderer>();
        
        ribbonTrail.time = trailTime;
        ribbonTrail.minVertexDistance = trailSmoothness;
        
        // Smooth width curve - baslangicta ince, ortada kalin, sonda ince
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(new Keyframe(0f, 0.8f, 0f, 0f));
        widthCurve.AddKey(new Keyframe(0.3f, 1f, 0f, 0f));
        widthCurve.AddKey(new Keyframe(1f, 0f, -1f, 0f));
        ribbonTrail.widthCurve = widthCurve;
        ribbonTrail.widthMultiplier = trailStartWidth;
        
        // Smooth renk gecisi
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(trailColor, 0f),
                new GradientColorKey(trailColor * 0.8f, 0.5f),
                new GradientColorKey(trailColor * 0.5f, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(trailColor.a, 0f),
                new GradientAlphaKey(trailColor.a * 0.7f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        ribbonTrail.colorGradient = gradient;
        
        // Soft material
        ribbonTrail.material = new Material(Shader.Find("Sprites/Default"));
        ribbonTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ribbonTrail.receiveShadows = false;
        
        // Smooth texture mode
        ribbonTrail.textureMode = LineTextureMode.Stretch;
        ribbonTrail.numCapVertices = 5;
        ribbonTrail.numCornerVertices = 5;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!enableHitEffect || hasHit) return;
        
        // Dusman veya hedef tahtasina carpti mi?
        bool hitEnemy = other.GetComponentInParent<EnemyHealth>() != null;
        bool hitBoard = other.GetComponentInParent<TargetBoardHealth>() != null;
        bool hitSolid = !other.isTrigger && !hitEnemy;
        
        if (hitEnemy || hitBoard || hitSolid)
        {
            hasHit = true;
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            CreateHitEffect(hitPoint);
            
            // Hit sound
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, hitPoint, 0.7f);
            }
        }
    }
    
    private void CreateHitEffect(Vector3 position)
    {
        // Ana container
        GameObject hitFX = new GameObject("PotionHitEffect");
        hitFX.transform.position = position;
        
        // Sprite varsa material olustur
        Material hitMaterial = new Material(Shader.Find("Sprites/Default"));
        if (hitSprite != null)
        {
            hitMaterial.mainTexture = hitSprite.texture;
        }
        
        // === 1. BUYUK BALONCUKLAR ===
        GameObject bubblesObj = new GameObject("Bubbles");
        bubblesObj.transform.SetParent(hitFX.transform);
        bubblesObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem bubbles = bubblesObj.AddComponent<ParticleSystem>();
        var bubblesMain = bubbles.main;
        bubblesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
        bubblesMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        bubblesMain.startSize = new ParticleSystem.MinMaxCurve(hitBubbleMinSize, hitBubbleMaxSize);
        bubblesMain.startColor = new ParticleSystem.MinMaxGradient(hitColor1, hitColor2);
        bubblesMain.simulationSpace = ParticleSystemSimulationSpace.World;
        bubblesMain.maxParticles = hitBubbleCount;
        bubblesMain.gravityModifier = -0.5f; // Yukari suzulsun
        
        var bubblesEmission = bubbles.emission;
        bubblesEmission.rateOverTime = 0;
        bubblesEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, hitBubbleCount)
        });
        
        var bubblesShape = bubbles.shape;
        bubblesShape.shapeType = ParticleSystemShapeType.Sphere;
        bubblesShape.radius = 0.1f;
        
        // Baloncuklar buyuyup patlayarak kaybolsun
        var bubblesSizeOverLife = bubbles.sizeOverLifetime;
        bubblesSizeOverLife.enabled = true;
        AnimationCurve bubbleSizeCurve = new AnimationCurve();
        bubbleSizeCurve.AddKey(new Keyframe(0f, 0.5f, 0f, 2f));
        bubbleSizeCurve.AddKey(new Keyframe(0.3f, 1f, 0f, 0f));
        bubbleSizeCurve.AddKey(new Keyframe(0.8f, 1.2f, 0f, 0f));
        bubbleSizeCurve.AddKey(new Keyframe(1f, 0f, -5f, 0f)); // Hizli kucul (patlama)
        bubblesSizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, bubbleSizeCurve);
        
        // Renk degisimi - fade out
        var bubblesColorOverLife = bubbles.colorOverLifetime;
        bubblesColorOverLife.enabled = true;
        Gradient bubblesGradient = new Gradient();
        bubblesGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.white, 0.7f),
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.8f, 0.6f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        bubblesColorOverLife.color = bubblesGradient;
        
        // Hafif velocity randomness
        var bubblesVelocity = bubbles.velocityOverLifetime;
        bubblesVelocity.enabled = true;
        bubblesVelocity.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        bubblesVelocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        bubblesVelocity.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        var bubblesRenderer = bubbles.GetComponent<ParticleSystemRenderer>();
        bubblesRenderer.material = hitMaterial;
        bubblesRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // === 2. PARLAK SPARKLES ===
        GameObject sparklesObj = new GameObject("Sparkles");
        sparklesObj.transform.SetParent(hitFX.transform);
        sparklesObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem sparkles = sparklesObj.AddComponent<ParticleSystem>();
        var sparklesMain = sparkles.main;
        sparklesMain.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        sparklesMain.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        sparklesMain.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
        sparklesMain.startColor = new ParticleSystem.MinMaxGradient(hitColorBright, hitColor2);
        sparklesMain.simulationSpace = ParticleSystemSimulationSpace.World;
        sparklesMain.maxParticles = hitSparkleCount;
        sparklesMain.gravityModifier = 0.3f;
        
        var sparklesEmission = sparkles.emission;
        sparklesEmission.rateOverTime = 0;
        sparklesEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, hitSparkleCount)
        });
        
        var sparklesShape = sparkles.shape;
        sparklesShape.shapeType = ParticleSystemShapeType.Sphere;
        sparklesShape.radius = 0.05f;
        
        // Hizli kucul
        var sparklesSizeOverLife = sparkles.sizeOverLifetime;
        sparklesSizeOverLife.enabled = true;
        AnimationCurve sparkleSizeCurve = new AnimationCurve();
        sparkleSizeCurve.AddKey(0f, 1f);
        sparkleSizeCurve.AddKey(1f, 0f);
        sparklesSizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sparkleSizeCurve);
        
        // Parlak fade
        var sparklesColorOverLife = sparkles.colorOverLifetime;
        sparklesColorOverLife.enabled = true;
        Gradient sparklesGradient = new Gradient();
        sparklesGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f), 
                new GradientColorKey(Color.white, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        sparklesColorOverLife.color = sparklesGradient;
        
        var sparklesRenderer = sparkles.GetComponent<ParticleSystemRenderer>();
        sparklesRenderer.material = hitMaterial;
        sparklesRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // === 3. FLASH RING (Ani parlama halkasi) ===
        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(hitFX.transform);
        flashObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem flash = flashObj.AddComponent<ParticleSystem>();
        var flashMain = flash.main;
        flashMain.startLifetime = 0.15f;
        flashMain.startSpeed = 0f;
        flashMain.startSize = 0.05f;
        flashMain.startColor = hitColorBright;
        flashMain.simulationSpace = ParticleSystemSimulationSpace.World;
        flashMain.maxParticles = 1;
        
        var flashEmission = flash.emission;
        flashEmission.rateOverTime = 0;
        flashEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 1)
        });
        
        // Hizli buyume
        var flashSizeOverLife = flash.sizeOverLifetime;
        flashSizeOverLife.enabled = true;
        AnimationCurve flashSizeCurve = new AnimationCurve();
        flashSizeCurve.AddKey(new Keyframe(0f, 1f, 0f, 10f));
        flashSizeCurve.AddKey(new Keyframe(1f, 8f, 0f, 0f));
        flashSizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, flashSizeCurve);
        
        // Hizli fade
        var flashColorOverLife = flash.colorOverLifetime;
        flashColorOverLife.enabled = true;
        Gradient flashGradient = new Gradient();
        flashGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(hitColorBright, 0f), 
                new GradientColorKey(hitColor1, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        flashColorOverLife.color = flashGradient;
        
        var flashRenderer = flash.GetComponent<ParticleSystemRenderer>();
        flashRenderer.material = hitMaterial;
        flashRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Hepsini baslat
        bubbles.Play();
        sparkles.Play();
        flash.Play();
        
        // Temizlik
        Destroy(hitFX, 2f);
    }
    
    private void OnDestroy()
    {
        if (ribbonTrail != null)
        {
            ribbonTrail.Clear();
        }
    }
}
