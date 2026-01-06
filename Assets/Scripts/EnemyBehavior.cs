using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    public NavMeshAgent agent;
    public float speed = 1f; 
    private int killCount = 0; 

    [Header("Speed Increases")]
    public float speedIncrease1 = 1.5f; 
    public float speedIncrease2 = 2f;

    [SerializeField] private List<Collider> colliders;


    private void OnEnable()
    {
        EnemyHealth.OnEnemyKilled += OnEnemyKilled; 
    }

    private void OnDisable()
    {
        EnemyHealth.OnEnemyKilled -= OnEnemyKilled; 
    }   

    private void OnEnemyKilled()
    {
        killCount++; 

        if (killCount == 3)
        {
            speed = speedIncrease1;
            // Debug.Log("H�z artt�: 3. d��man� �ld�rd�n.");
        }
       
        else if (killCount == 6)
        {
            speed = speedIncrease2;
            // Debug.Log("H�z artt�: 6. d��man� �ld�rd�n.");
        }
    }

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
        // if (Time.frameCount % 300 == 0)
        // {
        //     Debug.Log($"Enemy hareket ediyor: Pozisyon={transform.position}, Hedef={targetPosition}, Mesafe={agent.remainingDistance}, Durum={agent.pathStatus}");
        // }
    }
}
