using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleSoundTriggerE1 : MonoBehaviour
{
    private ParticleSystem myparticleSystemE1;
    private AudioSource audioSource;
    private bool wasEmitting;

    void Start()
    {
        myparticleSystemE1 = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        wasEmitting = false;
    }

    void Update()
    {
        if (myparticleSystemE1.isEmitting && !wasEmitting)
        {
            audioSource.Play(); // Play the sound when particle system starts emitting
             
			wasEmitting = true;
        }
        else if (!myparticleSystemE1.isEmitting)
        {
            wasEmitting = false;
        }
    }
}
