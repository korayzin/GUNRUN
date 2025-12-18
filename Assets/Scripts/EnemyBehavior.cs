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
            Debug.Log("Hýz arttý: 3. düþmaný öldürdün.");
        }
       
        else if (killCount == 6)
        {
            speed = speedIncrease2;
            Debug.Log("Hýz arttý: 6. düþmaný öldürdün.");
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
        Vector3 targetPosition = Camera.main.transform.position;
        agent.SetDestination(targetPosition);

        agent.speed = speed;
    }
}
