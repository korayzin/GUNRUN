using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Tooltip("Merminin verdigi hasar miktari")]
    public float damage = 25f;
    public float speed = 20f; 
    public AudioClip hitSound;
    public GameObject damageEffectPrefab;
    private bool isGameOver = false;

    private void Update()
    {
        if (!isGameOver)
        {
            transform.position += transform.forward * speed * Time.unscaledDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Debug.Log("Dusmana hasar verildi: " + damage);
            enemyHealth.TakeDamage(damage, other);
            
            // WeaponManager uzerinden hit sesi ve haptic cal
            if (WeaponManager.Instance != null)
            {
                WeaponManager.Instance.PlayHitSound();
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
        }
    }

    public void SetGameOverState(bool state)
    {
        isGameOver = state;
    }
}
