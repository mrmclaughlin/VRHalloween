using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MazeEndTrigger : MonoBehaviour
{
    public GridMazeHedgeBuilder builder;
    public Transform hmd;

    [Tooltip("Seconds between triggers (safety against repeated firing).")]
    public float cooldownSeconds = 1.0f;

    private BoxCollider box;
    private bool wasInside = false;
    private float nextAllowedTime = -999f;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        if (builder == null) return;

        // Resolve HMD if not assigned
        if (hmd == null)
        {
            if (builder.hmd != null) hmd = builder.hmd;
            else if (Camera.main != null) hmd = Camera.main.transform;
        }
        if (hmd == null) return;

        // "Camera transform enters trigger": check if HMD world position is inside our box volume.
        bool inside = IsWorldPointInsideBox(box, hmd.position);

        // Fire only on the transition from outside -> inside
        if (inside && !wasInside)
        {
            if (Time.time >= nextAllowedTime)
            {
                nextAllowedTime = Time.time + Mathf.Max(0f, cooldownSeconds);
                builder.NotifyReachedMazeEnd();
            }
        }

        wasInside = inside;
    }

    private static bool IsWorldPointInsideBox(BoxCollider bc, Vector3 worldPoint)
    {
        // Convert point into the collider’s local space
        Vector3 localPoint = bc.transform.InverseTransformPoint(worldPoint) - bc.center;

        // Compare against half extents (bc.size is in local space)
        Vector3 half = bc.size * 0.5f;
        return Mathf.Abs(localPoint.x) <= half.x
            && Mathf.Abs(localPoint.y) <= half.y
            && Mathf.Abs(localPoint.z) <= half.z;
    }
}
