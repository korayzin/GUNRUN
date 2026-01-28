using UnityEngine;

/// <summary>
/// Shotgun mermisine Z ekseni etrafında döndürme efekti ve tail trail ekler.
/// Bullet.cs ile birlikte çalışır - Bullet hareket/hasar, bu script rotasyon ve trail.
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
