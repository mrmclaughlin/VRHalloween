using System.Collections;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class TransportSwitchWithVignette : MonoBehaviour
    {
        [Header("Zone + Head")]
        [SerializeField] Transform hmd;      // Main Camera
        [SerializeField] Collider zone;      // BoxCollider on this object (Is Trigger)

        [Header("Switch Objects")]
        [SerializeField] GameObject objectA;
        [SerializeField] GameObject objectB;
        [SerializeField] bool startWithAOn = true;

        [Header("Vignette")]
        [SerializeField] TunnelingVignetteController vignetteController;
        [SerializeField] TransportVignetteProvider transportProvider;

        [Header("Timing")]
        [SerializeField] float holdAtDarkSeconds = 0.10f;
        [SerializeField] float cooldownSeconds = 0.75f;

        bool isInside;
        bool aIsOn;
        bool isTransitioning;
        float nextAllowedTime;

        void Reset()
        {
            zone = GetComponent<Collider>();
        }

        void Awake()
        {
            if (zone == null) zone = GetComponent<Collider>();
            aIsOn = startWithAOn;
            ApplyState();
        }

        void Update()
        {
            if (hmd == null || zone == null || isTransitioning)
                return;

            bool nowInside = zone.bounds.Contains(hmd.position);

            // toggle only when crossing outside -> inside
            if (nowInside && !isInside && Time.time >= nextAllowedTime)
            {
                nextAllowedTime = Time.time + cooldownSeconds;
                StartCoroutine(DoTransport());
            }

            isInside = nowInside;
        }

        IEnumerator DoTransport()
        {
            isTransitioning = true;

            // Begin tunnel (close down)
            if (vignetteController != null && transportProvider != null)
                vignetteController.BeginTunnelingVignette(transportProvider);

            // Wait until we're likely at (or near) dark.
            // Use the provider's easeInTime so the timing matches your settings.
            float easeIn = transportProvider != null && transportProvider.vignetteParameters != null
                ? Mathf.Max(0f, transportProvider.vignetteParameters.easeInTime)
                : 0.3f;

            if (easeIn > 0f)
                yield return new WaitForSecondsRealtime(easeIn);

            // Swap at darkness
            aIsOn = !aIsOn;
            ApplyState();

            if (holdAtDarkSeconds > 0f)
                yield return new WaitForSecondsRealtime(holdAtDarkSeconds);

            // End tunnel (open back up)
            if (vignetteController != null && transportProvider != null)
                vignetteController.EndTunnelingVignette(transportProvider);

            isTransitioning = false;
        }

        void ApplyState()
        {
            if (objectA != null) objectA.SetActive(aIsOn);
            if (objectB != null) objectB.SetActive(!aIsOn);
        }
    }
}
