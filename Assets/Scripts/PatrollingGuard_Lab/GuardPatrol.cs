using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class GuardPatrol : MonoBehaviour
{
    public Transform[] wayPoints;
    public float waypointTolerance = 0.5f;

    int _currentIndex = 0;
    NavMeshAgent _agent;
    public float patrolSpeed = 3.0f;
    public float chaseSpeed = 4.5f;
    public float guardfov = 12f;
    [Range(0, 360)] public float viewAngle = 90f;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    public Transform eyes;


    public GuardState currentState = GuardState.Patrolling;
    public Transform player;
    public float chaseRange = 5.0f;
    public float loseRange = 7.0f;
    public float searchDuration = 5.0f;
    public float searchRadius = 3.0f;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer;

    public enum GuardState
    {
        Patrolling,
        Chasing,
        ReturnToPatrol,
        Searching
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        if (wayPoints.Length > 0)
        {
            _agent.SetDestination(wayPoints[_currentIndex].position);
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case GuardState.Patrolling:
                UpdatePatrol();
                break;
            case GuardState.Chasing:
                UpdateChase();
                break;
            case GuardState.ReturnToPatrol:
                UpdateReturnToPatrol();
                break; 
            case GuardState.Searching:
                UpdateSearching();
                break;
        }
    }

    bool CanSeePlayer()
    {
        Collider[] targetsInViewRadius =
            Physics.OverlapSphere(transform.position, chaseRange, playerMask);

        if (targetsInViewRadius.Length == 0)
            return false;

        Transform target = targetsInViewRadius[0].transform;

        Vector3 dirToTarget = (target.position - eyes.position).normalized;
        float angleToTarget = Vector3.Angle(eyes.forward, dirToTarget);

        // Check cone angle
        if (angleToTarget < viewAngle / 2)
        {
            float distToTarget = Vector3.Distance(eyes.position, target.position);

            // Raycast for obstacles
            if (!Physics.Raycast(eyes.position, dirToTarget, distToTarget, obstacleMask))
            {
                lastKnownPlayerPosition = target.position;
                return true;
            }
        }

        return false;
    }

    public void UpdatePatrol()
    {
        if (wayPoints.Length == 0) return;

        // Check if the agent has reached the current waypoint
        if (!_agent.pathPending && _agent.remainingDistance < waypointTolerance)
        {
            //go to the next waypoint (loop)
            _currentIndex = (_currentIndex + 1) % wayPoints.Length;
            _agent.SetDestination(wayPoints[_currentIndex].position);
        }

        /*if (Vector3.Distance(transform.position, player.position) < chaseRange)
        {
            currentState = GuardState.Chasing;
        }*/ 

        /*if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out RaycastHit hit, chaseRange))
        {
            if (hit.transform == player)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                if (angleToPlayer < guardfov)
                {
                    currentState = GuardState.Chasing;
                }
            }
        }*/
        if (CanSeePlayer())
        {
            currentState = GuardState.Chasing;
        }

    }

    void UpdateChase()
    {
        _agent.speed = chaseSpeed;
        _agent.SetDestination(player.position);
        /*if (Vector3.Distance(transform.position, player.position) > loseRange)
        {
            currentState = GuardState.ReturnToPatrol;
        }*/

        /*if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out RaycastHit hit, chaseRange))
        {
            if (hit.transform != player)
            {
                currentState = GuardState.Searching;
                searchTimer = searchDuration;
                _agent.SetDestination(lastKnownPlayerPosition);
            }
        }*/

        if (!CanSeePlayer())
        {
            currentState = GuardState.Searching;
            searchTimer = searchDuration;
            _agent.SetDestination(lastKnownPlayerPosition);
        }
    }

    void UpdateReturnToPatrol()
    {
        _agent.speed = patrolSpeed;
        if (wayPoints.Length == 0) return;
        _agent.SetDestination(wayPoints[_currentIndex].position);
        if (!_agent.pathPending && _agent.remainingDistance < waypointTolerance)
        {
            currentState = GuardState.Patrolling;
        }

        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out RaycastHit hit, chaseRange))
        {
            if (hit.transform == player)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                if (angleToPlayer < guardfov)
                {
                    currentState = GuardState.Chasing;
                }
            }
        }
    }


    void UpdateSearching()
    {
        searchTimer -= Time.deltaTime;

        // If reached destination, wander around the area
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            Vector3 randomPoint;
            if (RandomPoint(lastKnownPlayerPosition, searchRadius, out randomPoint))
            {
                _agent.SetDestination(randomPoint);
            }
        }

        if (CanSeePlayer())
        {
            currentState = GuardState.Chasing;
        }

        // Stop searching after time runs out
        if (searchTimer <= 0f)
        {
            currentState = GuardState.ReturnToPatrol;
        }
    }
    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * range;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary * chaseRange);
        Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary * chaseRange);
    }

    Vector3 DirFromAngle(float angle)
    {
        float rad = (angle + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }
}
