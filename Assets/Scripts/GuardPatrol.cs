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
    public int guardfov = 50;
    
    public GuardState currentState = GuardState.Patrolling;
    public Transform player;
    public float chaseRange = 5.0f;
    public float loseRange = 7.0f;

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
        }
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

    void UpdateChase()
    {
        _agent.speed = chaseSpeed;
        _agent.SetDestination(player.position);
        /*if (Vector3.Distance(transform.position, player.position) > loseRange)
        {
            currentState = GuardState.ReturnToPatrol;
        }*/

        if (Physics.Raycast(transform.position, (player.position - transform.position).normalized, out RaycastHit hit, chaseRange))
        {
            if (hit.transform != player)
            {
                currentState = GuardState.ReturnToPatrol;
            }
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
        
    }
}
