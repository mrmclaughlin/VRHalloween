using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MazeEndTrigger : MonoBehaviour
{
    public GridMazeHedgeBuilder builder;
    public Transform hmd;

    public float cooldownSeconds = 1.0f;

    // The builder stores which cell this trigger represents (so it can continue from here)
    [HideInInspector]
    public Vector2Int triggerCell;

    private BoxCollider box;
    private bool wasInside = false;
    private float nextAllowedTime = -999f;

    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    public void ForceOutside()
    {
        wasInside = false;
        nextAllowedTime = -999f;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        if (builder == null) return;

        if (hmd == null)
        {
            if (builder.hmd != null) hmd = builder.hmd;
            else if (Camera.main != null) hmd = Camera.main.transform;
        }
        if (hmd == null) return;

        bool inside = IsWorldPointInsideBox(box, hmd.position);

        if (inside && !wasInside)
        {
            if (Time.time >= nextAllowedTime)
            {
                nextAllowedTime = Time.time + Mathf.Max(0f, cooldownSeconds);
                builder.NotifyReachedSegmentTrigger();
            }
        }

        wasInside = inside;
    }
private void OnDrawGizmosSelected()
{
    var bc = GetComponent<BoxCollider>();
    Gizmos.color = Color.magenta;
    Gizmos.matrix = transform.localToWorldMatrix;
    Gizmos.DrawWireCube(bc.center, bc.size);
}

    private static bool IsWorldPointInsideBox(BoxCollider bc, Vector3 worldPoint)
    {
        Vector3 localPoint = bc.transform.InverseTransformPoint(worldPoint) - bc.center;
        Vector3 half = bc.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= half.x
            && Mathf.Abs(localPoint.y) <= half.y
            && Mathf.Abs(localPoint.z) <= half.z;
    }
}
