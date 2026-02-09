using UnityEngine;

public class LayoutPortalZone : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform mainCamera;   // <- Main Camera Transform
    [SerializeField] private BoxCollider zone;       // <- BoxCollider on this object

    [Header("Switch target")]
    [SerializeField] private GameObject targetLayoutRoot;

    [Header("Behavior")]
    [Tooltip("Prevents re-triggering until you exit the zone.")]
    [SerializeField] private bool latchUntilExit = true;

    [Tooltip("Extra delay after a switch request to avoid double-fires.")]
    [SerializeField] private float localCooldownSeconds = 0.25f;

    private bool isInside;
    private float nextAllowedTime;

    private void Reset()
    {
        zone = GetComponent<BoxCollider>();
    }

    private void Awake()
    {
        if (zone == null) zone = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        // Critical: sync inside state so enabling a layout doesn't instantly trigger
        if (mainCamera != null && zone != null)
            isInside = IsInsideBox(zone, mainCamera.position);

        // small grace period to avoid same-frame cascades
        nextAllowedTime = Time.time + 0.05f;
    }

    private void Update()
    {
        if (LayoutManager.Instance == null) return;
        if (mainCamera == null || zone == null || targetLayoutRoot == null) return;

        bool nowInside = IsInsideBox(zone, mainCamera.position);

        // Fire only on outside -> inside edge
        if (nowInside && !isInside && Time.time >= nextAllowedTime)
        {
            LayoutManager.Instance.RequestSwitchTo(targetLayoutRoot);
            nextAllowedTime = Time.time + localCooldownSeconds;

            if (latchUntilExit)
            {
                // latch "inside" until we exit, prevents re-trigger while staying in the zone
                isInside = true;
                return;
            }
        }

        // Normal tracking
        isInside = nowInside;
    }

    private static bool IsInsideBox(BoxCollider box, Vector3 worldPos)
    {
        Vector3 local = box.transform.InverseTransformPoint(worldPos) - box.center;
        Vector3 half = box.size * 0.5f;

        return Mathf.Abs(local.x) <= half.x &&
               Mathf.Abs(local.y) <= half.y &&
               Mathf.Abs(local.z) <= half.z;
    }
}
