using UnityEngine;

public class ImpactSound : MonoBehaviour
{
    public AudioClip bounceSound; // Drag your sound clip here in Unity Editor
    private AudioSource audioSource;
    private float initialHeight;
    private float maxHeight;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        initialHeight = transform.position.y; // Initial height of the ball
        maxHeight = initialHeight;
    }

    void Update()
    {
        // Update the maximum height the ball has reached
        if (transform.position.y > maxHeight)
        {
            maxHeight = transform.position.y;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate how far the ball has fallen
        float fallenDistance = maxHeight - transform.position.y;

        // Reset the maximum height for the next bounce
        maxHeight = transform.position.y;

        // Calculate the volume based on fallen distance (you can change this formula)
        float volume = Mathf.Clamp(fallenDistance / initialHeight, 0.1f, 1f);

        // Play the sound
        audioSource.PlayOneShot(bounceSound, volume);
    }
}
