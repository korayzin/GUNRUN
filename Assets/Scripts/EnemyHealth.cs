using System;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public static event Action OnEnemyKilled;

    public UnityEngine.AI.NavMeshAgent agent;
    public float totalHealth = 100f;
    private float currentHealth;

    public float headshotMultiplier = 2f;
    public float bodyMultiplier = 1f;
    public float legsMultiplier = 0.7f;

    public Collider headCollider;
    public Collider bodyCollider;
    public Collider legsCollider;

    public GameObject deathVFX;
    public GameObject headshotFloatingTextPrefab;
    public GameObject bodyFloatingTextPrefab;
    public GameObject legsFloatingTextPrefab;
    [Header("Single Collider System (for new enemies)")]
    public GameObject floatingTextPrefab; // Tek collider için genel floating text

    [Header("Health Glow System")]
    public bool useHealthGlow = true;
    public string glowColorPropertyName = "_NeonColor"; // veya "_GlowColor", "_RimColor"

    public AudioClip damageSFX;
    [Tooltip("Hasar sesi çalınırken kullanılacak ses seviyesi (0-1). AdvancedPortalSpawner her düşman tipi için ayrı ayarlar.")]
    [Range(0f, 1f)] public float damageSFXVolume = 1f;
    public AudioClip deathSFX;
    private AudioSource audioSource;

    public int scoreValue = 50;
    private bool isDead = false;

    [Header("Tutorial")]
    [Tooltip("true ise hasar almaz - TutorialFirstEnemyController tarafından ayarlanır")]
    public bool isInvulnerable = false;

    private Renderer[] enemyRenderers;
    private MaterialPropertyBlock propertyBlock;
    
    // Buzlanma efekti
    private bool isFrozen = false;
    private ParticleSystem freezeParticleSystem;
    private Color originalGlowColor;
    private Color freezeColor = new Color(0.5f, 0.8f, 1f, 1f); // Buz mavisi
    
    // SLOWED yazısı
    private GameObject slowedTextObject;
    private TextMeshPro slowedTextMesh;

    // BURNED (ateş püskürtme) sadece renk - yazı yok
    private bool isBurned = false;

    void Start()
    {
        currentHealth = totalHealth;
        audioSource = GetComponent<AudioSource>();
        InitializeHealthGlow();
    }

    /// <summary>Son öldürme kaynağı - Tutorial için (5-9. silah ikincil kill takibi)</summary>
    public static bool LastKillWasFromFlameSpray { get; private set; }
    public static bool LastKillWasFromSecondary { get; private set; }
    public static int LastKillWeaponIndex { get; private set; } = -1;

    public void TakeDamage(float damage, Collider hitCollider, bool fromFlameSpray = false, bool fromSecondary = false, int tutorialWeaponIndex = -1)
    {
        if (isInvulnerable) return;
        Debug.Log(hitCollider.name + " tarafından vuruldu! Hasar: " + damage);
        if (fromFlameSpray)
            ApplyBurnEffect();

        float adjustedDamage = 0f;
        GameObject textPrefabToUse = null;

        // Eski sistem: Head/Body/Legs collider'ları varsa
        if (headCollider != null || bodyCollider != null || legsCollider != null)
        {
            if (hitCollider == headCollider && headCollider != null)
            {
                adjustedDamage = damage * headshotMultiplier;
                textPrefabToUse = headshotFloatingTextPrefab;
            }
            else if (hitCollider == bodyCollider && bodyCollider != null)
            {
                adjustedDamage = damage * bodyMultiplier;
                textPrefabToUse = bodyFloatingTextPrefab;
            }
            else if (hitCollider == legsCollider && legsCollider != null)
            {
                adjustedDamage = damage * legsMultiplier;
                textPrefabToUse = legsFloatingTextPrefab;
            }
            else
            {
                // Eşleşme yoksa body olarak kabul et
                adjustedDamage = damage * bodyMultiplier;
                textPrefabToUse = bodyFloatingTextPrefab;
            }
        }
        // Yeni sistem: Tek collider (head/body/legs yok)
        else
        {
            adjustedDamage = damage; // Multiplier yok, direkt damage
            textPrefabToUse = floatingTextPrefab != null ? floatingTextPrefab : bodyFloatingTextPrefab;
        }

        // Floating text göster
        if (textPrefabToUse != null)
        {
            ShowFloatingText(adjustedDamage, hitCollider, textPrefabToUse);
        }

        currentHealth -= adjustedDamage;

        //GameManager.Instance.AddScore((int)adjustedDamage);

        UpdateHealthGlow();

        if (currentHealth <= 0)
        {
            LastKillWasFromFlameSpray = fromFlameSpray;
            LastKillWasFromSecondary = fromSecondary;
            LastKillWeaponIndex = tutorialWeaponIndex >= 0 ? tutorialWeaponIndex : (fromFlameSpray || fromSecondary ? 8 : -1);
            Die();
        }

        if (damageSFX != null)
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.PlayOneShot(damageSFX, damageSFXVolume);
            else
                AudioSource.PlayClipAtPoint(damageSFX, hitCollider != null ? hitCollider.bounds.center : transform.position, damageSFXVolume);
        }
    }

    private void ShowFloatingText(float damage, Collider hitCollider, GameObject floatingTextPrefab)
    {
        if (floatingTextPrefab != null && hitCollider != null)
        {
            // Kafa pozisyonunu bul - düşmanın en üst noktası
            Vector3 headPosition = GetHeadPosition(hitCollider);
            
            GameObject damageText = Instantiate(floatingTextPrefab, headPosition, Quaternion.identity);

            // Text'i ayarla - hasar degerini "-X" formatinda goster
            // GetComponentInChildren kullan cunku TextMeshPro ve FloatingText child objede
            FloatingText floatingTextScript = damageText.GetComponentInChildren<FloatingText>();
            if (floatingTextScript != null)
            {
                floatingTextScript.SetText("-" + Mathf.RoundToInt(damage).ToString());
            }
            else
            {
                // Eger FloatingText script yoksa direkt TextMeshPro'ya yaz
                TextMeshPro textMesh = damageText.GetComponentInChildren<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = "-" + Mathf.RoundToInt(damage).ToString();
                }
            }
        }
    }
    
    private Vector3 GetHeadPosition(Collider hitCollider)
    {
        Vector3 headPos = transform.position;
        
        // Önce headCollider varsa onu kullan
        if (headCollider != null)
        {
            headPos = headCollider.bounds.center;
            headPos.y = headCollider.bounds.max.y; // En üst nokta
        }
        // Renderer'ların bounds'ını kullan
        else
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds combinedBounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers)
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
                headPos = combinedBounds.center;
                headPos.y = combinedBounds.max.y; // En üst nokta
            }
            // Renderer yoksa collider'ın üst noktasını kullan
            else if (hitCollider != null)
            {
                headPos = hitCollider.bounds.center;
                headPos.y = hitCollider.bounds.max.y;
            }
        }
        
        // Kafanın biraz üstüne yerleştir (daha görünür olsun)
        headPos.y += 0.2f;
        
        return headPos;
    }

    private void InitializeHealthGlow()
    {
        if (!useHealthGlow) return;
        
        enemyRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        UpdateHealthGlow(); // İlk renk ayarını yap
    }

    private void UpdateHealthGlow()
    {
        // Yanma: full kırmızı (health glow kullanmasa bile renderer rengi güncellenir)
        if (isBurned)
        {
            if (enemyRenderers == null) enemyRenderers = GetComponentsInChildren<Renderer>();
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            Color burnRed = new Color(1f, 0.1f, 0.05f, 1f);
            foreach (Renderer r in enemyRenderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(glowColorPropertyName, burnRed);
                r.SetPropertyBlock(propertyBlock);
            }
            return;
        }

        if (!useHealthGlow || enemyRenderers == null || propertyBlock == null) return;
        
        float healthRatio = currentHealth / totalHealth;
        Color healthColor = GetHealthColor(healthRatio);
        
        // Buzlanma efekti varsa renge ekle - GÜÇLENDİRİLMİŞ
        if (isFrozen)
        {
            healthColor = Color.Lerp(healthColor, freezeColor, 0.85f); // Daha güçlü buz efekti (0.6'dan 0.85'e)
            // Buz efekti için ekstra parlaklık
            healthColor.r = Mathf.Min(healthColor.r + 0.2f, 1f);
            healthColor.g = Mathf.Min(healthColor.g + 0.3f, 1f);
            healthColor.b = Mathf.Min(healthColor.b + 0.4f, 1f);
        }
        foreach (Renderer renderer in enemyRenderers)
        {
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(glowColorPropertyName, healthColor);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
    
    // Buzlanma efekti metodları
    public void ApplyFreezeEffect()
    {
        if (isFrozen) return; // Zaten buzlu
        
        isFrozen = true;
        
        // Buz partikül efekti oluştur
        CreateFreezeParticles();
        
        // SLOWED yazısını göster
        ShowSlowedText();
        
        // Glow rengini güncelle
        UpdateHealthGlow();
    }
    
    public void RemoveFreezeEffect()
    {
        if (!isFrozen) return; // Zaten buzlu değil
        
        isFrozen = false;
        
        // Buz partiküllerini kaldır
        if (freezeParticleSystem != null && freezeParticleSystem.gameObject != null)
        {
            freezeParticleSystem.Stop();
            Destroy(freezeParticleSystem.gameObject, 1f);
            freezeParticleSystem = null;
        }
        
        // SLOWED yazısını kaldır
        HideSlowedText();
        
        // Glow rengini güncelle
        UpdateHealthGlow();
    }
    
    private void ShowSlowedText()
    {
        // Zaten varsa tekrar oluşturma
        if (slowedTextObject != null) return;
        
        // SLOWED yazısı için GameObject oluştur
        slowedTextObject = new GameObject("SlowedText");
        slowedTextObject.transform.SetParent(transform);
        
        // TextMeshPro ekle
        slowedTextMesh = slowedTextObject.AddComponent<TextMeshPro>();
        slowedTextMesh.text = "SLOWED";
        slowedTextMesh.fontSize = 2f;
        slowedTextMesh.color = new Color(0.5f, 0.8f, 1f, 1f); // Buz mavisi
        slowedTextMesh.alignment = TextAlignmentOptions.Center;
        slowedTextMesh.sortingOrder = 10;
        
        // Pozisyonu ayarla (düşmanın üstünde) - local position kullan
        slowedTextObject.transform.localPosition = Vector3.up * 0.3f;
        
        // Kamera'ya bakması için script ekle
        SlowedTextController controller = slowedTextObject.AddComponent<SlowedTextController>();
    }
    
    private void HideSlowedText()
    {
        if (slowedTextObject != null)
        {
            Destroy(slowedTextObject);
            slowedTextObject = null;
            slowedTextMesh = null;
        }
    }

    public void ApplyBurnEffect()
    {
        if (isBurned) return;
        isBurned = true;
        UpdateHealthGlow();
    }

    private void Update()
    {
        // SLOWED yazısı parent'a bağlı olduğu için otomatik olarak düşmanla birlikte hareket eder
        // Sadece pozisyonu güncellemeye gerek yok, local position kullanıyoruz
    }
    
    private void CreateFreezeParticles()
    {
        // Buz partikül sistemi oluştur - GÜÇLENDİRİLMİŞ VERSİYON
        GameObject freezeParticleObj = new GameObject("FreezeParticles");
        freezeParticleObj.transform.SetParent(transform);
        freezeParticleObj.transform.localPosition = Vector3.zero;
        
        freezeParticleSystem = freezeParticleObj.AddComponent<ParticleSystem>();
        var main = freezeParticleSystem.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f); // Daha uzun ömür
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f); // Daha hızlı
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f); // Daha büyük partiküller
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.8f, 0.95f, 1f, 1f), // Daha parlak beyaz-mavi
            new Color(0.4f, 0.7f, 1f, 0.9f) // Daha koyu mavi
        );
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 300; // Daha fazla partikül (100'den 300'e)
        main.gravityModifier = -0.15f; // Daha fazla yukarı çıkış
        main.loop = true;
        
        var emission = freezeParticleSystem.emission;
        emission.rateOverTime = 80f; // Daha fazla partikül/saniye (30'dan 80'e)
        
        var shape = freezeParticleSystem.shape;
        // Düşmanın mesh'ini kullan, yoksa sphere kullan
        Mesh enemyMesh = GetEnemyMesh();
        if (enemyMesh != null)
        {
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.mesh = enemyMesh;
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
        }
        shape.scale = Vector3.one * 1.5f; // Daha geniş alan (1.2'den 1.5'e)
        
        // Size over lifetime - daha belirgin
        var sizeOverLifetime = freezeParticleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(0.7f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - daha parlak ve belirgin
        var colorOverLifetime = freezeParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 1f, 1f, 1f), 0f), // Beyaz başlangıç
                new GradientColorKey(new Color(0.7f, 0.9f, 1f, 1f), 0.3f), 
                new GradientColorKey(new Color(0.4f, 0.7f, 1f, 0.9f), 0.7f),
                new GradientColorKey(new Color(0.2f, 0.5f, 1f, 0f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f), // Tam opak başlangıç
                new GradientAlphaKey(0.9f, 0.3f), 
                new GradientAlphaKey(0.7f, 0.7f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = colorGradient;
        
        // Velocity over lifetime - daha dinamik
        var velocityOverLifetime = freezeParticleSystem.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(0.2f, 0.5f); // Dışa doğru genişleme
        
        var renderer = freezeParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 1;
    }
    
    private Mesh GetEnemyMesh()
    {
        // Düşmanın mesh'ini bul
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            return meshFilter.mesh;
        }
        
        // Mesh bulunamazsa sphere kullan
        return null; // Shape otomatik olarak sphere kullanacak
    }

    private Color GetHealthColor(float healthRatio)
    {
        if (healthRatio > 0.75f)
            return Color.Lerp(Color.green, Color.yellow, (1f - healthRatio) * 4f);
        else if (healthRatio > 0.5f)
            return Color.Lerp(Color.yellow, new Color(1f, 0.53f, 0f), (0.75f - healthRatio) * 4f);
        else if (healthRatio > 0.25f)
            return Color.Lerp(new Color(1f, 0.53f, 0f), Color.red, (0.5f - healthRatio) * 4f);
        else
            return Color.red;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        // Buzlanma efektini kaldır
        if (isFrozen)
            RemoveFreezeEffect();

        OnEnemyKilled?.Invoke();

        GameManager.Instance.AddScore(scoreValue);

        // Aktif silahı bul ve yenile
        GunFire activeGun = FindActiveGunFire();
        if (activeGun != null)
        {
            if (activeGun.isBaretta)
            {
                // Tüm aktif Baretta silahlarını yenile
                ReloadAllBarettas();
            }
            else
            {
                // Normal silah için sadece o silahı yenile
                activeGun.Reload();
                Debug.Log($"{activeGun.gameObject.name} silahı yenilendi.");
            }
        }

        if (agent != null) agent.enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;

        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshRenderers) mesh.enabled = false;

        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }

        float sfxDuration = 0f;
        if (audioSource != null && deathSFX != null)
        {
            audioSource.PlayOneShot(deathSFX);
            sfxDuration = deathSFX.length;
        }

        Destroy(gameObject, Mathf.Max(sfxDuration, 0.4f));
    }
    
    private void OnDestroy()
    {
        // Buzlanma efektini temizle
        if (freezeParticleSystem != null && freezeParticleSystem.gameObject != null)
        {
            Destroy(freezeParticleSystem.gameObject);
        }
        
        if (slowedTextObject != null)
            Destroy(slowedTextObject);
    }


    // Aktif silahı bul (enabled ve aktif olan GunFire)
    private GunFire FindActiveGunFire()
    {
        GunFire[] allGuns = FindObjectsOfType<GunFire>(true); // Include inactive objects
        foreach (GunFire gun in allGuns)
        {
            // Aktif ve enabled olan silahı bul
            if (gun.gameObject.activeInHierarchy && gun.enabled)
            {
                return gun;
            }
        }
        
        // Eğer aktif silah bulunamazsa, aktif hierarchy'deki ilk silahı dene
        // (Bazı durumlarda enabled=false olabilir ama activeInHierarchy=true)
        foreach (GunFire gun in allGuns)
        {
            if (gun.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"FindActiveGunFire: {gun.gameObject.name} aktif ama enabled=false. Yenileme deneniyor...");
                return gun;
            }
        }
        
        return null;
    }

    // Tüm aktif Baretta silahlarını yenile
    private void ReloadAllBarettas()
    {
        GunFire[] allGuns = FindObjectsOfType<GunFire>();
        foreach (GunFire gun in allGuns)
        {
            if (gun.isBaretta && gun.gameObject.activeInHierarchy)
            {
                gun.Reload();
                Debug.Log($"{gun.gameObject.name} Baretta yenilendi (Sol el: {gun.isLeftHanded})");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // VR sistemi için farklı tag'ler kontrol et
        if (other.CompareTag("Player") || other.CompareTag("MainCamera") || other.name.Contains("OVRCameraRig") || other.transform.root.name.Contains("OVRCameraRig"))
        {
            Debug.Log($"🚨 Düşman oyuncuya çarptı! Çarpılan obje: {other.name}, Tag: {other.tag}, Parent: {other.transform.root.name}");
            GameManager.Instance.GameOver(other);
        }
        else
        {
            Debug.Log($"ℹ️ Düşman farklı objeye çarptı: {other.name} (Tag: {other.tag})");
        }
    }
}
