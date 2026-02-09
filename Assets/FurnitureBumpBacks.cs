using UnityEngine;

/// <summary>
/// Pushes the XR rig back when the player's HEAD (camera) overlaps furniture colliders.
/// Attach to the same GameObject as the CharacterController (usually XR Rig root).
/// </summary>
[DisallowMultipleComponent]
public class FurnitureBumpBack : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your XR head/camera transform (Main Camera).")]
    public Transform head;

    [Tooltip("CharacterController on this rig root.")]
    public CharacterController characterController;

    [Header("Furniture Detection")]
    [Tooltip("Only colliders on these layers will cause bump-back (e.g., Furniture).")]
    public LayerMask furnitureLayers;

    [Tooltip("Radius around the head used to detect furniture overlap.")]
    [Range(0.05f, 0.5f)]
    public float headRadius = 0.12f;

    [Tooltip("How far we allow penetration before we start correcting aggressively.")]
    [Range(0.0f, 0.2f)]
    public float penetrationSlop = 0.01f;

    [Header("Push Settings")]
    [Tooltip("How quickly we push out of furniture (higher = snappier).")]
    [Range(1f, 40f)]
    public float pushStrength = 18f;

    [Tooltip("Max push distance per frame (meters). Helps prevent jolts.")]
    [Range(0.01f, 0.5f)]
    public float maxPushPerFrame = 0.08f;

    [Header("Debug")]
    public bool drawDebug = false;

    // Reusable buffer to avoid allocations
    private readonly Collider[] _hits = new Collider[24];

    void Reset()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (head == null)
        {
            var cam = Camera.main;
            if (cam != null) head = cam.transform;
        }
    }

    void Update()
    {
        if (head == null || characterController == null) return;

        Vector3 headPos = head.position;

        int count = Physics.OverlapSphereNonAlloc(
            headPos,
            headRadius,
            _hits,
            furnitureLayers,
            QueryTriggerInteraction.Ignore
        );

        if (count <= 0) return;

        // Compute a correction vector by resolving penetrations
        Vector3 totalCorrection = Vector3.zero;

        // We represent the "head" as a sphere collider for ComputePenetration.
        // We'll use a temporary SphereCollider-like representation via Physics.ComputePenetration overloads.
        // Create a virtual sphere at headPos with radius headRadius.
        // We'll compute the minimal translation vector (MTV) needed to separate them.
        for (int i = 0; i < count; i++)
        {
            Collider other = _hits[i];
            if (other == null) continue;

            // Compute penetration between a sphere and the other collider.
            // Unity doesn't have a direct "sphere collider instance" here, so we use the overload with Collider parameters
            // by using the CharacterController's collider as "self" would be wrong (body vs head).
            // Instead, we approximate by using a capsule cast style resolution:
            // We'll do a small sphere cast outward using ComputePenetration via a hidden primitive.
            // Practical approach: use Physics.ComputePenetration with a temporary SphereCollider created once.
            // To keep this script single-file and simple, we do a fallback: push away from closest point.

            Vector3 closest = other.ClosestPoint(headPos);
            Vector3 delta = headPos - closest;
            float dist = delta.magnitude;

            // If head is inside collider, ClosestPoint returns headPos (dist == 0).
            if (dist < 1e-5f)
            {
                // Push away from collider center as a fallback direction
                delta = (headPos - other.bounds.center);
                dist = delta.magnitude;
                if (dist < 1e-5f) continue;
            }

            float penetration = headRadius - dist;

if (penetration > penetrationSlop)
{
    Vector3 dir = delta / dist;
    Vector3 correction = dir * (penetration + penetrationSlop);
    totalCorrection += correction;
}

        }

        if (totalCorrection.sqrMagnitude < 1e-8f) return;

        // Smooth + clamp the push per frame
        Vector3 push = Vector3.Lerp(Vector3.zero, totalCorrection, 1f - Mathf.Exp(-pushStrength * Time.deltaTime));
        if (push.magnitude > maxPushPerFrame)
            push = push.normalized * maxPushPerFrame;

        // Important: move the RIG BODY, not the camera.
        characterController.Move(push);

        if (drawDebug)
        {
            Debug.DrawLine(headPos, headPos + push, Color.yellow, 0.02f, false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebug || head == null) return;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
        Gizmos.DrawWireSphere(head.position, headRadius);
    }
}
