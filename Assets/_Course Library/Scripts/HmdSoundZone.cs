using UnityEngine;

public class HmdSoundZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hmd;     // Main Camera (HMD)
    [SerializeField] private Collider zone;     // Trigger collider on this object

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    [Header("Behavior")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private float cooldownSeconds = 1.5f;

    private bool isInside;
    private bool fired;
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

        if (hmd == null && Camera.main != null)
            hmd = Camera.main.transform;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D
        }
    }

    public void Init(Transform hmdTransform, AudioClip soundClip, float vol, bool oneShotPlay, float cooldown)
    {
        hmd = hmdTransform;
        clip = soundClip;
        volume = vol;
        oneShot = oneShotPlay;
        cooldownSeconds = cooldown;
    }

private void Update()
{
    if (hmd == null || zone == null || clip == null || audioSource == null) return;

    // Compare only in the XZ plane (ignore head height)
    Vector3 c = zone.bounds.center;
    Vector3 p = hmd.position;

    float dx = p.x - c.x;
    float dz = p.z - c.z;

    // Approximate "inside" based on bounds extents in XZ
    float maxR = Mathf.Max(zone.bounds.extents.x, zone.bounds.extents.z);
    bool nowInside = (dx * dx + dz * dz) <= (maxR * maxR);

    if (nowInside && !isInside)
    {
        isInside = true;
        TryPlay();
    }
    else if (!nowInside && isInside)
    {
        isInside = false;
    }
}


    private void TryPlay()
    {
        if (oneShot && fired) return;
        if (Time.time < nextAllowedTime) return;

        nextAllowedTime = Time.time + cooldownSeconds;
        fired = true;

        audioSource.PlayOneShot(clip, volume);
    }
}
