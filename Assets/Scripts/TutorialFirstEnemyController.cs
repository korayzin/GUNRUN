using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Tutorial ilk düşman: Stop/Resume ve invulnerability kontrolü.
/// Dialogue 4 bitene kadar öldürülemez (EnemyHealth.isInvulnerable).
/// </summary>
public class TutorialFirstEnemyController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private EnemyHealth _health;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _health = GetComponent<EnemyHealth>();
        SetVulnerable(true); // Hareket başladığı andan itibaren öldürülebilir
    }

    public void Stop()
    {
        if (_agent != null)
            _agent.isStopped = true;
    }

    public void Resume()
    {
        if (_agent != null)
            _agent.isStopped = false;
    }

    public void SetVulnerable(bool vulnerable)
    {
        if (_health != null)
            _health.isInvulnerable = !vulnerable;
    }
}
