using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class SteeringAgent : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float maxForce = 10f; // Limit how "fast" we can change
    private float Acceleration;
    //direction(turning radius);
    [Header("Arrive")]
    public float slowingRadius = 3f;
    [Header("Separation")]
    public float separationRadius = 1.5f;
    public float separationStrength = 5f;
    [Header("Weights")]
    public float arriveWeight = 1f;
    public float separationWeight = 1f;
    [Header("Debug")]
    public bool drawDebug = true;
    private Vector3 velocity = Vector3.zero;
    // Optional target for Seek / Arrive
    public Transform target;
    // Static list so agents can find each other
    public static List<SteeringAgent> allAgents = new List<SteeringAgent>();

    private void OnEnable()
    {
        allAgents.Add(this);
    }
    private void OnDisable()
    {
        allAgents.Remove(this);
    }
    void Update()
    {
        // 1. Calculate Steering Force
        Vector3 steering = Vector3.zero;
        // TODO in Part B/C:
        if (target != null)
            steering += Arrive(target.position, slowingRadius) * arriveWeight;
        
        if (allAgents.Count > 1)
        steering += Separation(separationRadius, separationStrength) * separationWeight;
        // 2. Limit Steering (Truncate)
        // This prevents the agent from turning instantly.
        steering = Vector3.ClampMagnitude(steering, maxForce);
        // 3. Apply Steering to Velocity (Integration)
        //Acceleration = maxForce / 1;
        //velocity Change = Acceleration * Time.
        velocity += steering * Time.deltaTime;
        // 4. Limit Velocity
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        // 5. Move Agent
        transform.position += velocity * Time.deltaTime;
        // 6. Face Movement Direction
        if (velocity.sqrMagnitude > 0.0001f)
        {
            transform.forward = velocity.normalized;
        }
    }
    // -- BEHAVIOUR STUBS --
    public Vector3 Seek(Vector3 targetPos) 
    { 
        Vector3 toTarget = targetPos - transform.position;

        if (toTarget.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 desiredVelocity = toTarget.normalized * maxSpeed;

        return desiredVelocity - velocity;
    }
    public Vector3 Arrive(Vector3 targetPos, float slowRadius)
    {
        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;

        if (distance < 0.0001f)
            return Vector3.zero;

        float desiredSpeed = maxSpeed;

        if (distance < slowRadius)
        {
            desiredSpeed = maxSpeed * (distance / slowRadius);
        }

        Vector3 desiredVelocity = toTarget.normalized * desiredSpeed;
        return desiredVelocity - velocity;
    }
    public Vector3 Separation(float radius, float strength)
    {
        Vector3 force = Vector3.zero;
        int neighborCount = 0;
        foreach (SteeringAgent other in allAgents)
        {
            if (other == this) continue;
            Vector3 toOther = other.transform.position - transform.position;
            float distance = toOther.magnitude;
            if (distance < radius && distance > 0.0001f)
            {
                Vector3 awayFromOther = -toOther.normalized;
                force += awayFromOther / distance; // Weight by distance
                neighborCount++;
            }
        }
        if (neighborCount > 0)
        {
            force /= neighborCount;
            force = force.normalized * maxSpeed - velocity;
            force = Vector3.ClampMagnitude(force, strength);
        }
        return force;
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawDebug) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position +

        velocity);
    }
}

