using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleSoundTrigger : MonoBehaviour
{
    private ParticleSystem myparticleSystem;
    private AudioSource audioSource;
    private bool wasEmitting;

    void Start()
    {
        myparticleSystem = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        wasEmitting = false;
    }

    void Update()
    {
        if (myparticleSystem.isEmitting && !wasEmitting)
        {
            
			StartCoroutine(DelayedAction(.1f));
			 
			wasEmitting = true;
        }
        else if (!myparticleSystem.isEmitting)
        {
            wasEmitting = false;
        }
    }
	
	
	IEnumerator DelayedAction(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay

        // The code here will execute after the delay
		audioSource.Play();
        //Debug.Log("This message is shown after a delay of " + delay + " seconds.");
    }
	
}
