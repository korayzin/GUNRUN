using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speed = 3f; // Stage bazlı hız, spawn anında atanır

    [SerializeField] private List<Collider> colliders;
    
    // Slow mekanizması
    private float slowMultiplier = 1f;
    private bool isSlowed = false;
    private float originalSpeed;

    public void Stop()
    {
        agent.isStopped = true;
    }

    public void DisableColliders()
    {
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }
    
    // Slow mekanizması metodları
    public void ApplySlow(float multiplier)
    {
        if (!isSlowed)
        {
            originalSpeed = speed;
        }
        slowMultiplier = multiplier;
        isSlowed = true;
    }
    
    public void RemoveSlow()
    {
        slowMultiplier = 1f;
        isSlowed = false;
    }
    
    public bool IsSlowed()
    {
        return isSlowed;
    }

    private void Update()
    {
        if (agent == null || !agent.enabled)
        {
            return;
        }

        // Oyuncunun pozisyonunu hedef al
        Vector3 targetPosition = Camera.main.transform.position;

        // NavMesh üzerinde geçerli bir hedef pozisyonu hesapla
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }

        // Hedefe git
        agent.SetDestination(targetPosition);
        agent.speed = speed * slowMultiplier; // Slow multiplier uygula

        // Debug bilgisi (sadece arada)
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"Enemy hareket ediyor: Pozisyon={transform.position}, Hedef={targetPosition}, Mesafe={agent.remainingDistance}, Durum={agent.pathStatus}");
        }
    }
}
