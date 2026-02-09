using UnityEngine;

public class PlayMusicOnHover : MonoBehaviour
{
    public AudioClip audioClip;  // The audio clip you want to play
    private AudioSource audioSource;  // Reference to the AudioSource component

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
    }

    // This function is called when another object enters the trigger collider
    void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is tagged as "Hand"
        if (other.tag == "MusicOn")
        {
            // Play the audio clip
            audioSource.Play();
        }
    }

    // This function is called when another object exits the trigger collider
    void OnTriggerExit(Collider other)
    {
        // Check if the object that exited the trigger is tagged as "Hand"
        if (other.tag == "MusicOn")
        {
            // Stop the audio clip
            audioSource.Stop();
        }
    }
}
