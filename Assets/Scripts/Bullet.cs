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
    [Tooltip("İlk 2 silah (FirstGun, SecondGun) destructible mesh'e çarptığında mermi iade için")]
    public GunFire sourceGunFire;
    public float speed = 20f;
    [Tooltip("Açıkken mermi yerçekiminden etkilenir (eğik atış). Kapalıyken düz gider.")]
    public bool useGravity = false;
    public AudioClip hitSound;
    public GameObject damageEffectPrefab;
    [Tooltip("Açıkken destructible mesh'i çarpma noktasında kendi büyüklüğü kadar alan ile kırar (OverlapSphere). Kapalıyken sadece çarpılan segment kırılır.")]
    public bool destructibleMeshAreaDamage = false;
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
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (useGravity)
            {
                // Yerçekimli mermi: Bullet.speed ile başlangıç hızı, yerçekimi physics ile
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.velocity = movementDirection * speed;
            }
            else
            {
                // Normal mermi: kinematic hareket
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }

    private void Update()
    {
        if (isGameOver) return;
        
        if (useGravity)
        {
            // Yerçekimli mermi: Rigidbody physics hareket ettirir, elle hareket yok
            return;
        }
        
        transform.position += movementDirection * speed * Time.unscaledDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            ShotgunBullet shotgunBullet = GetComponent<ShotgunBullet>();
            if (shotgunBullet != null && shotgunBullet.enableHitEffect)
                shotgunBullet.SpawnHitEffect(hitPoint);
            FirstGunBulletHitEffect firstGunHit = GetComponent<FirstGunBulletHitEffect>();
            if (firstGunHit != null && firstGunHit.enableHitEffect)
                firstGunHit.SpawnHitEffect(hitPoint);
            FifthAmmoHitEffect fifthAmmoHit = GetComponent<FifthAmmoHitEffect>();
            if (fifthAmmoHit != null && fifthAmmoHit.enableHitEffect)
                fifthAmmoHit.SpawnHitEffect(hitPoint);
            SixthAmmoHitEffect sixthAmmoHit = GetComponent<SixthAmmoHitEffect>();
            if (sixthAmmoHit != null && sixthAmmoHit.enableHitEffect)
                sixthAmmoHit.SpawnHitEffect(hitPoint);
            OctopusAmmoHitEffect octopusAmmoHit = GetComponent<OctopusAmmoHitEffect>();
            if (octopusAmmoHit != null && octopusAmmoHit.enableHitEffect)
                octopusAmmoHit.SpawnHitEffect(hitPoint);
            EightAmmoHitEffect eightAmmoHit = GetComponent<EightAmmoHitEffect>();
            if (eightAmmoHit != null && eightAmmoHit.enableHitEffect)
                eightAmmoHit.SpawnHitEffect(hitPoint);
            FireballEffect fireballEffect = GetComponent<FireballEffect>();
            if (fireballEffect != null && fireballEffect.enableHitEffect)
                fireballEffect.SpawnHitEffect(hitPoint);
            
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
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            DestroyDestructibleMeshSegments(hitPoint, other);
        }
    }

    /// <summary>
    /// Non-trigger collider'lı mermiler (PotionGunBullet, SixthAmmo) için - OnCollisionEnter tetiklenir.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            ShotgunBullet shotgunBullet = GetComponent<ShotgunBullet>();
            if (shotgunBullet != null && shotgunBullet.enableHitEffect)
                shotgunBullet.SpawnHitEffect(hitPoint);
            FirstGunBulletHitEffect firstGunHit = GetComponent<FirstGunBulletHitEffect>();
            if (firstGunHit != null && firstGunHit.enableHitEffect)
                firstGunHit.SpawnHitEffect(hitPoint);
            FifthAmmoHitEffect fifthAmmoHit = GetComponent<FifthAmmoHitEffect>();
            if (fifthAmmoHit != null && fifthAmmoHit.enableHitEffect)
                fifthAmmoHit.SpawnHitEffect(hitPoint);
            SixthAmmoHitEffect sixthAmmoHit = GetComponent<SixthAmmoHitEffect>();
            if (sixthAmmoHit != null && sixthAmmoHit.enableHitEffect)
                sixthAmmoHit.SpawnHitEffect(hitPoint);
            OctopusAmmoHitEffect octopusAmmoHit = GetComponent<OctopusAmmoHitEffect>();
            if (octopusAmmoHit != null && octopusAmmoHit.enableHitEffect)
                octopusAmmoHit.SpawnHitEffect(hitPoint);
            EightAmmoHitEffect eightAmmoHit = GetComponent<EightAmmoHitEffect>();
            if (eightAmmoHit != null && eightAmmoHit.enableHitEffect)
                eightAmmoHit.SpawnHitEffect(hitPoint);
            FireballEffect fireballEffect = GetComponent<FireballEffect>();
            if (fireballEffect != null && fireballEffect.enableHitEffect)
                fireballEffect.SpawnHitEffect(hitPoint);
            
            Debug.Log("Dusmana hasar verildi: " + damage);
            int tw = weaponIndex >= 0 ? weaponIndex : (isFromSecondary ? 8 : -1);
            enemyHealth.TakeDamage(damage, other, fromFlameSpray: false, fromSecondary: isFromSecondary, tutorialWeaponIndex: tw);
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

        DestructibleMeshComponent destructibleMesh = other.GetComponentInParent<DestructibleMeshComponent>();
        if (destructibleMesh != null && other.gameObject != destructibleMesh.ReservedSegment)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            DestroyDestructibleMeshSegments(hitPoint, other);
        }
    }

    /// <summary>
    /// Destructible mesh segmentlerini kırar. destructibleMeshAreaDamage açıksa kendi büyüklüğü kadar alan, değilse sadece çarpılan segment.
    /// </summary>
    private void DestroyDestructibleMeshSegments(Vector3 hitPoint, Collider hitCollider)
    {
        float radius = destructibleMeshAreaDamage ? GetDestructibleMeshAreaRadius() : 0f;

        if (radius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(hitPoint, radius);
            foreach (Collider col in hits)
            {
                DestructibleMeshComponent dm = col.GetComponentInParent<DestructibleMeshComponent>();
                if (dm != null && col.gameObject != dm.ReservedSegment)
                {
                    dm.DestroySegment(col.gameObject);
                }
            }
        }
        else
        {
            if (hitCollider != null)
            {
                DestructibleMeshComponent dm = hitCollider.GetComponentInParent<DestructibleMeshComponent>();
                if (dm != null && hitCollider.gameObject != dm.ReservedSegment)
                    dm.DestroySegment(hitCollider.gameObject);
            }
        }

        DestructibleMeshHint.NotifyWallDestroyed();
        if ((weaponIndex == 0 || weaponIndex == 1) && sourceGunFire != null)
            sourceGunFire.RestoreAmmo(1);
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.PlayHitSound();
            WeaponManager.Instance.TriggerHitHaptic();
        }
        Destroy(gameObject);
    }

    /// <summary>Mermi büyüklüğüne göre destructible mesh kırma yarıçapı (SphereCollider.radius * scale).</summary>
    private float GetDestructibleMeshAreaRadius()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        if (sc == null) return 0.5f;
        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        return sc.radius * maxScale;
    }

    public void SetGameOverState(bool state)
    {
        isGameOver = state;
    }
}
