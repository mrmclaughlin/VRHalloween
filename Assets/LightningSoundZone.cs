using UnityEngine;

public class LightningSoundZone : MonoBehaviour
{
    [Header("Who triggers this")]
    public string playerTag = "Player";

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Lightning FX")]
    public ParticleSystem lightningParticles;

    [Header("Behavior")]
    public bool oneShot = true;
    public float cooldownSeconds = 2f;

    private bool hasFired = false;
    private float nextAllowedTime = 0f;

    private void Reset()
    {
        // Helps auto-wire if you add the script in-editor
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (oneShot && hasFired) return;
        if (Time.time < nextAllowedTime) return;

        Fire();
    }

    private void Fire()
    {
        hasFired = true;
        nextAllowedTime = Time.time + cooldownSeconds;

        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }

        if (lightningParticles != null)
        {
            lightningParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            lightningParticles.Play(true);
        }
    }
}
