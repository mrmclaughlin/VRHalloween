using UnityEngine;

public class HeadZoneToggleObjects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hmd;     // drag Main Camera here
    [SerializeField] private Collider zone;     // drag this object's BoxCollider here

    [Header("Objects to toggle")]
    [SerializeField] private GameObject turnOnWhenInside;
    [SerializeField] private GameObject turnOffWhenInside;

    [Header("Behavior")]
    [SerializeField] private bool invertWhenOutside = true; // swap back when you leave
    [SerializeField] private bool startAssumingOutside = true;

    private bool isInside;

    private void Reset()
    {
        zone = GetComponent<Collider>();
    }

    private void Awake()
    {
        if (zone == null) zone = GetComponent<Collider>();
        isInside = !startAssumingOutside;
        ApplyState(startAssumingOutside ? false : true);
    }

    private void Update()
    {
        if (hmd == null || zone == null) return;

        bool nowInside = zone.bounds.Contains(hmd.position);
        if (nowInside == isInside) return;

        isInside = nowInside;
        ApplyState(isInside);
    }

    private void ApplyState(bool inside)
    {
        if (turnOnWhenInside != null)
            turnOnWhenInside.SetActive(inside);

        if (turnOffWhenInside != null)
            turnOffWhenInside.SetActive(!inside);

        if (!invertWhenOutside)
        {
            // If you don't want it to swap back outside, force "outside" to do nothing.
            if (!inside)
            {
                // revert outside effects
                if (turnOnWhenInside != null) turnOnWhenInside.SetActive(true);
                if (turnOffWhenInside != null) turnOffWhenInside.SetActive(false);
            }
        }
    }
}
