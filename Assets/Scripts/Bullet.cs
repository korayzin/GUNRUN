using UnityEngine;
using Meta.XR.MRUtilityKit;

public class Bullet : MonoBehaviour
{
    [Tooltip("Merminin verdigi hasar miktari")]
    public float damage = 25f;
    [Tooltip("Tutorial: A tuşu (fireball) ile atıldıysa true - ikincil kill takibi için")]
    public bool isFromSecondary = false;
    [Tooltip("Tutorial: Hangi silahtan atıldı (0-8)")]
    public int weaponIndex = -1;
    public float speed = 20f; 
    public AudioClip hitSound;
    public GameObject damageEffectPrefab;
    private bool isGameOver = false;
    private Vector3 movementDirection; // Hareket yönü (prefab rotasyonundan bağımsız)
    private Quaternion visualRotation; // Görsel rotasyon (prefab rotasyonu + spawn rotasyonu)
    private bool directionSet = false; // Yön ayarlandı mı?

    private void Awake()
    {
        // Görsel rotasyonu sakla (prefab rotasyonu + spawn rotasyonu)
        visualRotation = transform.rotation;
    }

    private void Start()
    {
        // Eğer yön henüz set edilmediyse, velocity'den al
        if (!directionSet)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.velocity.magnitude > 0.01f)
            {
                // Hareket yönünü velocity'den al (target direction, prefab rotasyonundan bağımsız)
                SetMovementDirection(rb.velocity.normalized);
            }
            else
            {
                // Velocity yoksa, transform.forward'u kullan (fallback)
                SetMovementDirection(transform.forward);
            }
        }
        
        // Görsel rotasyonu koru (sadece görsel için, hareketi etkilemez)
        transform.rotation = visualRotation;
    }

    /// <summary>
    /// Hareket yönünü set eder (prefab rotasyonundan bağımsız, target direction'a göre)
    /// </summary>
    public void SetMovementDirection(Vector3 direction)
    {
        movementDirection = direction.normalized;
        directionSet = true;
        
        // Rigidbody'yi kinematic yap ve velocity'yi sıfırla
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (!isGameOver)
        {
            transform.position += movementDirection * speed * Time.unscaledDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Debug.Log("Dusmana hasar verildi: " + damage);
            int tw = weaponIndex >= 0 ? weaponIndex : (isFromSecondary ? 8 : -1);
            enemyHealth.TakeDamage(damage, other, fromFlameSpray: false, fromSecondary: isFromSecondary, tutorialWeaponIndex: tw);
            
            // WeaponManager üzerinden hit sesi ve kontrolcülere haptic
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.PlayHitSound();
                WeaponManager.Instance.TriggerHitHaptic();
            }
            
            Destroy(gameObject);
            return;
        }

        TargetBoardHealth boardHealth = other.GetComponentInParent<TargetBoardHealth>();
        if (boardHealth != null)
        {
            Debug.Log("Target Board'a hasar verildi: " + damage);
            boardHealth.TakeDamage(damage, other.ClosestPoint(transform.position));
            Destroy(gameObject);
            return;
        }

        // Destructible mesh (MRUK room) - mermi segment'e çarptığında kırsın
        DestructibleMeshComponent destructibleMesh = other.GetComponentInParent<DestructibleMeshComponent>();
        if (destructibleMesh != null && other.gameObject != destructibleMesh.ReservedSegment)
        {
            destructibleMesh.DestroySegment(other.gameObject);
            DestructibleMeshHint.NotifyWallDestroyed(); // Duvar ipuçlarını ilk kırılmada kaldır
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.PlayHitSound();
                WeaponManager.Instance.TriggerHitHaptic();
            }
            Destroy(gameObject);
        }
    }

    public void SetGameOverState(bool state)
    {
        isGameOver = state;
    }
}
