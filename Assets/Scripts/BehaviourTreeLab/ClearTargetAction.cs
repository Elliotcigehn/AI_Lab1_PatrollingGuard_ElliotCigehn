using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Clear Target", description: "Clears Target and Resets LOS memory", story: "Forget the target and reset perception flags.", category: "Action/Sensing", id: "7d35ad09f6d726cf22f34ae83a38a9b0")]
public partial class ClearTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;

    protected override Status OnUpdate()
    {
        if (Target != null) Target.Value = null;
        if (HasLineOfSight != null) HasLineOfSight.Value = false;
        if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 9999f;
        return Status.Success;
    }
}

