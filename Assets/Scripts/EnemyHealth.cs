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

    public AudioClip damageSFX;
    public AudioClip deathSFX;
    private AudioSource audioSource;

    private GunFire gunFire;

    public int scoreValue = 50;
    private bool isDead = false;

    void Start()
    {
        currentHealth = totalHealth;
        audioSource = GetComponent<AudioSource>();
        gunFire = FindObjectOfType<GunFire>();
    }

    public void TakeDamage(float damage, Collider hitCollider)
    {
        Debug.Log(hitCollider.name + " tarafından vuruldu! Hasar: " + damage);

        float adjustedDamage = 0f;

        if (hitCollider == headCollider)
        {
            adjustedDamage = damage * headshotMultiplier;
            ShowFloatingText(adjustedDamage, headCollider, headshotFloatingTextPrefab);
        }
        else if (hitCollider == bodyCollider)
        {
            adjustedDamage = damage * bodyMultiplier;
            ShowFloatingText(adjustedDamage, bodyCollider, bodyFloatingTextPrefab);
        }
        else /*if (hitCollider == legsCollider)*/
        {
            adjustedDamage = damage * legsMultiplier;
            ShowFloatingText(adjustedDamage, legsCollider, legsFloatingTextPrefab);
        }
        //else
        //{
        //    adjustedDamage = damage;
        //    ShowFloatingText(adjustedDamage, hitCollider, bodyFloatingTextPrefab);
        //}

        currentHealth -= adjustedDamage;

        //GameManager.Instance.AddScore((int)adjustedDamage);

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
        if (floatingTextPrefab != null)
        {
            Vector3 hitPosition = hitCollider.ClosestPointOnBounds(transform.position);
            GameObject damageText = Instantiate(floatingTextPrefab, hitPosition, Quaternion.identity);

            Vector3 moveDirection = agent.velocity.normalized;

            if (moveDirection.magnitude < 0.1f)
            {
                moveDirection = transform.forward;
            }

            damageText.transform.rotation = Quaternion.LookRotation(moveDirection);
            damageText.transform.position += new Vector3(0, 0.5f, 0);

            TextMeshPro textMesh = damageText.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = damage.ToString();
            }
        }
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
        if (other.CompareTag("Player")) 
        {
            Debug.Log("Düşman oyuncuya çarptı! " + other.name + " vuruldu!");
            GameManager.Instance.GameOver(other);
        }
    }
}
