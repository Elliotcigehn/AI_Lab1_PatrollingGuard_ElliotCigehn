using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Perception", description: "Updates Target/LOS/LastKnownPosition from GuardSensors.", story: "Update perception and write to the blackboard.", category: "Action/Sensing", id: "5ca9b4effc14b1642efc81204a83cb1a")]
public partial class UpdatePerceptionAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;

    protected override Status OnStart()
    {
        if (TimeSinceLastSeen != null && TimeSinceLastSeen.Value < 0f)
        {
            TimeSinceLastSeen.Value = 9999f;
        }
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        var sensors = GameObject != null ? GameObject.GetComponent<GuardSensors>() : null;
        if (sensors == null)
        {
            if (HasLineOfSight != null)
            {
                HasLineOfSight.Value = false;
            }
            if (TimeSinceLastSeen != null)
            {
                TimeSinceLastSeen.Value += Time.deltaTime;
            }
            return Status.Success;
        }

        bool sensed = sensors.TrySenseTarget(out GameObject sensedTarget, out Vector3 sensedPosition, out bool hasLOS);

        if (sensed && hasLOS)
        {
            if (Target != null) Target.Value = sensedTarget;
            if (HasLineOfSight != null) HasLineOfSight.Value = true;
            if (LastKnownPosition != null) LastKnownPosition.Value = sensedPosition;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 0f;
        }
        else
        {
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value += Time.deltaTime;
        }
        return Status.Success;
    }

    

    protected override void OnEnd()
    {
    }
}

