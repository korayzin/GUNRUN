using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speed = 3f; // Stage bazlı hız, spawn anında atanır

    [SerializeField] private List<Collider> colliders;

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
        agent.speed = speed;

        // Debug bilgisi (sadece arada)
        if (Time.frameCount % 300 == 0)
        {
            Debug.Log($"Enemy hareket ediyor: Pozisyon={transform.position}, Hedef={targetPosition}, Mesafe={agent.remainingDistance}, Durum={agent.pathStatus}");
        }
    }
}
