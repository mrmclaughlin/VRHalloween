using UnityEngine;

public class PassthroughTrigger : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer passthrough;
    [SerializeField] private string rigTag = "XR Rig";

    private void Awake()
    {
        if (passthrough != null)
            passthrough.enabled = false; // start OFF
    }

   private void OnTriggerEnter(Collider other)
{
    if (passthrough == null) return;
    if (other.GetComponentInParent<CharacterController>() != null)
        passthrough.enabled = true;
}

private void OnTriggerExit(Collider other)
{
    if (passthrough == null) return;
    if (other.GetComponentInParent<CharacterController>() != null)
        passthrough.enabled = false;
}

}
