using UnityEngine;

public class HeadZoneSwitchToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hmd;     // Main Camera
    [SerializeField] private Collider zone;     // Collider on this object

    [Header("Objects to toggle")]
    [SerializeField] private GameObject objectA;
    [SerializeField] private GameObject objectB;

    [Header("Switch state")]
    [SerializeField] private bool startWithAOn = true;

    private bool isInside;
    private bool aIsOn;
    private bool initialized;

    private bool applying;          // re-entrancy guard
    private float nextAllowedTime;  // tiny debounce to prevent rapid ping-pong

    private void Reset()
    {
        zone = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (zone == null) zone = GetComponent<Collider>();

        if (!initialized)
        {
            aIsOn = startWithAOn;
            initialized = true;
        }
		

        ApplyState();
    }

    private void OnEnable()
    {
        if (hmd != null && zone != null)
            isInside = IsInsideZone(hmd.position);

        // prevent immediate flip if multiple zones enable each other
        nextAllowedTime = Time.time + 0.05f;

        ApplyState();
    }

    private void Update()
    {
        if (hmd == null || zone == null) return;

        bool nowInside = IsInsideZone(hmd.position);

        if (nowInside && !isInside && Time.time >= nextAllowedTime)
        {
            aIsOn = !aIsOn;
            ApplyState();
            nextAllowedTime = Time.time + 0.25f; // adjust if needed
        }

        isInside = nowInside;
    }

    private bool IsInsideZone(Vector3 worldPos)
    {
        if (zone is BoxCollider box)
        {
            Vector3 local = box.transform.InverseTransformPoint(worldPos);
            Vector3 d = local - box.center;
            Vector3 half = box.size * 0.5f;

            return Mathf.Abs(d.x) <= half.x &&
                   Mathf.Abs(d.y) <= half.y &&
                   Mathf.Abs(d.z) <= half.z;
        }

        return zone.bounds.Contains(worldPos);
    }

    private void ApplyState()
    {
        if (applying) return;  // stops enable/disable cascades from re-entering
        applying = true;

        if (objectA != null)
        {
            bool wantA = aIsOn;
            if (objectA.activeSelf != wantA) objectA.SetActive(wantA);
        }

        if (objectB != null)
        {
            bool wantB = !aIsOn;
            if (objectB.activeSelf != wantB) objectB.SetActive(wantB);
        }

        applying = false;
    }
}
