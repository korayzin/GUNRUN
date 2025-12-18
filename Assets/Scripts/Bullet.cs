using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 100f;
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
            Debug.Log("Düþmana hasar verildi: " + damage);
            enemyHealth.TakeDamage(damage, other);
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
