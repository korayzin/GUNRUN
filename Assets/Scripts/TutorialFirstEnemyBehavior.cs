using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Tutorial'da ilk düşman: Biraz ilerleyip durur. Oyuncu vurmak için hazır bekler.
/// </summary>
public class TutorialFirstEnemyBehavior : MonoBehaviour
{
    [Tooltip("İlerleme süresi (saniye)")]
    public float moveDuration = 2f;

    [Tooltip("İlerleme mesafesi (metre) - 0 ise sadece süre kullanılır")]
    public float moveDistance = 3f;

    private NavMeshAgent _agent;
    private EnemyBehavior _enemyBehavior;
    private float _elapsed;
    private Vector3 _startPos;
    private bool _stopped;

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

        if (_elapsed >= moveDuration)
            StopEnemy();
    }

    private void StopEnemy()
    {
        _stopped = true;
        if (_agent != null)
            _agent.isStopped = true;
    }
}
