using UnityEngine;

public class LightningSoundZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hmd;          // Main Camera transform (HMD)
    [SerializeField] private Collider zone;          // BoxCollider or any collider (trigger or not)

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource; // Put this on the zone object for 3D audio
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    [Header("Lightning FX")]
    [SerializeField] private ParticleSystem lightningParticles;

    [Header("Behavior")]
    [SerializeField] private bool fireOnEnterOnly = true;
    [SerializeField] private bool oneShot = false;          // if true, only ever fires once
    [SerializeField] private float cooldownSeconds = 2f;     // prevent rapid refire near edges

    private bool isInside;
    private bool hasFiredOnce;
    private float nextAllowedTime;

    private void Reset()
    {
        zone = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (zone == null) zone = GetComponent<Collider>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Auto-find main camera if not assigned
        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;

        // Safety: keep particles quiet until triggered
        if (lightningParticles != null)
            lightningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (hmd == null || zone == null) return;

        bool nowInside = zone.bounds.Contains(hmd.position);

        // Enter event
        if (nowInside && !isInside)
        {
            isInside = true;
            if (fireOnEnterOnly) TryFire();
        }
        // Exit event
        else if (!nowInside && isInside)
        {
            isInside = false;

            // If you want it to fire on exit instead, swap the call to here.
            // TryFire();
        }
        else
        {
            isInside = nowInside;
        }

        // Optional: if you want it to fire repeatedly while inside on a cooldown,
        // set fireOnEnterOnly = false and it will tick while inside.
        if (!fireOnEnterOnly && isInside)
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (oneShot && hasFiredOnce) return;
        if (Time.time < nextAllowedTime) return;

        nextAllowedTime = Time.time + cooldownSeconds;
        hasFiredOnce = true;

        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volume);

        if (lightningParticles != null)
        {
            lightningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            lightningParticles.Play(true);
        }
    }
}
