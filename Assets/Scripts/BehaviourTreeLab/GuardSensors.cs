using UnityEngine;

public class GuardSensors : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string targetTag = "Player";
    
    [Header("Sight Settings")]
    [SerializeField] private float sightRange = 10f;
    [Range(1f, 180f)]
    [SerializeField] private float sightAngle = 90f;
    [SerializeField] private LayerMask obstructionMask = ~0;
    [SerializeField] private Transform eyes;

    [SerializeField]private Transform cachedTarget;

    public float SightRange => sightRange;
    public float SightAngle => sightAngle;

    private Transform EyesTransform => eyes != null ? eyes : transform;

    private void Awake()
    {
        GameObject go = GameObject.FindGameObjectWithTag(targetTag);
        cachedTarget = go != null ? go.transform : null;
    }

    public bool TrySenseTarget(out GameObject target, out Vector3 lastKnownPosition, out bool hasLineOfSight)
    {
        target = null;
        lastKnownPosition = default;
        hasLineOfSight = false;

        if (target == null) return false;
        
        Vector3 eyePos = EyesTransform.position;
        Vector3 toTarget = cachedTarget.position - eyePos;

        float dist = toTarget.magnitude;
        if (dist > sightRange) return false;

        Vector3 toTargetDir = toTarget / Mathf.Max(dist, 0.0001f);

        float halfAngle = sightAngle * 0.5f;
        float angle = Vector3.Angle(EyesTransform.forward, toTargetDir);
        if (angle > halfAngle) return false;

        if (Physics.Raycast(eyePos, toTargetDir, out RaycastHit hit, dist, obstructionMask))
        {
            if (hit.transform != cachedTarget)
            {
                return false;
            }
        }
        target = cachedTarget.gameObject;
        lastKnownPosition = cachedTarget.position;
        hasLineOfSight = true;
        return true;
    }
}


