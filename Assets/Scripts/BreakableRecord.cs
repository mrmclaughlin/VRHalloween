using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableRecord : MonoBehaviour
{
 public AudioClip breakingGlassSound; // Drag your breaking glass sound clip here in Unity Editor
    private AudioSource audioSource;

     

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
		
    }

    void OnCollisionEnter(Collision collision)
    {
        
            float collisionForce = collision.relativeVelocity.magnitude;

            // Play breaking glass sound if the collision is strong enough
            // You can adjust the value '1.0f' to your liking
            if (collisionForce > .01f)
            {
                audioSource.PlayOneShot(breakingGlassSound);

                // Optional: Destroy the object after playing sound
                //Destroy(gameObject, breakingGlassSound.length);
            }
        
    }

}
