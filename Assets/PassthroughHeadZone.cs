using UnityEngine;

public class PassthroughHeadZone : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthrough;
    [SerializeField] private Transform hmd; // Main Camera
    [SerializeField] private Collider zone; // SafetyPortal BoxCollider (can be trigger)

    private bool isInside;

    private void Reset()
    {
        zone = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (zone == null) zone = GetComponent<Collider>();
        if (passthrough != null) passthrough.enabled = false;
    }

    private void Update()
    {
        if (passthrough == null || hmd == null || zone == null) return;

        bool nowInside = zone.bounds.Contains(hmd.position);

        if (nowInside != isInside)
        {
            isInside = nowInside;
            passthrough.enabled = isInside;
        }
    }
}
