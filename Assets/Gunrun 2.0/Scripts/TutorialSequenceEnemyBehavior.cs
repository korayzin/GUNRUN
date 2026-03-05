using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// newtutorial için kullanılan "yürü-dur" davranışı. Eski tutorial'dan bağımsız.
/// Düşman belirli süre veya mesafe yürüdükten sonra durur, oyuncunun vurmasını bekler.
/// </summary>
public class TutorialSequenceEnemyBehavior : MonoBehaviour
{
    [Tooltip("İlerleme süresi (saniye)")]
    [SerializeField] private float moveDuration = 5f;

    [Tooltip("İlerleme mesafesi (metre) - 0 ise sadece süre kullanılır")]
    [SerializeField] private float moveDistance = 0f;

    private NavMeshAgent _agent;
    private EnemyBehavior _enemyBehavior;
    private float _elapsed;
    private Vector3 _startPos;
    private bool _stopped;

    /// <summary>Runtime'da süre ve mesafe atamak için. Spawn sonrası çağrılmalı.</summary>
    public void SetParams(float duration, float distance)
    {
        moveDuration = Mathf.Max(0f, duration);
        moveDistance = Mathf.Max(0f, distance);
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyBehavior = GetComponent<EnemyBehavior>();

        if (_enemyBehavior != null)
            _enemyBehavior.enabled = false;

        _startPos = transform.position;
        _elapsed = 0f;

        if (_agent != null && Camera.main != null)
        {
            Vector3 target = Camera.main.transform.position;
            target.y = transform.position.y;
            if (NavMesh.SamplePosition(target, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }
    }

    private void Update()
    {
        if (_stopped || _agent == null) return;

        _elapsed += Time.deltaTime;

        if (moveDistance > 0f)
        {
            float dist = Vector3.Distance(_startPos, transform.position);
            if (dist >= moveDistance)
                StopEnemy();
        }

        if (moveDuration > 0f && _elapsed >= moveDuration)
            StopEnemy();
    }

    private void StopEnemy()
    {
        _stopped = true;
        if (_agent != null)
            _agent.isStopped = true;
    }
}
