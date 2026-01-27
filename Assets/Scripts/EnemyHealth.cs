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
    public AudioClip deathSFX;
    private AudioSource audioSource;

    private GunFire gunFire;

    public int scoreValue = 50;
    private bool isDead = false;

    private Renderer[] enemyRenderers;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        currentHealth = totalHealth;
        audioSource = GetComponent<AudioSource>();
        gunFire = FindObjectOfType<GunFire>();
        InitializeHealthGlow();
    }

    public void TakeDamage(float damage, Collider hitCollider)
    {
        Debug.Log(hitCollider.name + " tarafından vuruldu! Hasar: " + damage);

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
            Die();
        }

        if (audioSource != null && damageSFX != null)
        {
            audioSource.PlayOneShot(damageSFX);
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
        if (!useHealthGlow || enemyRenderers == null || propertyBlock == null) return;
        
        float healthRatio = currentHealth / totalHealth;
        Color healthColor = GetHealthColor(healthRatio);
        
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

        OnEnemyKilled?.Invoke();

        GameManager.Instance.AddScore(scoreValue);

        if (gunFire != null)
        {
            gunFire.Reload();
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
