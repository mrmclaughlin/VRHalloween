using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events; // Add this if not already present
public class snowBallMonitor : MonoBehaviour
{
 private Rigidbody rb;
    private XRGrabInteractable grabInteractable;

   



  
private void Awake()
{
    rb = GetComponent<Rigidbody>();
    grabInteractable = GetComponent<XRGrabInteractable>();

    // Use the new event with updated signature
    grabInteractable.selectExited.AddListener(HandleSelectExited);

    Debug.Log("awake");
}

private void OnDestroy()
{
    // Remove the listener when the object is destroyed
    grabInteractable.selectExited.RemoveListener(HandleSelectExited);
}

private void HandleSelectExited(SelectExitEventArgs args)
{
    if (!args.isCanceled)
    {
        // Your logic for when the object is released
		Debug.Log("released");
		XRBaseInteractor currentInteractor = args.interactorObject as XRBaseInteractor;
        // Get and log the forward direction of the hand that grabbed the object
        Vector3 handForward = currentInteractor.transform.forward;
        Debug.Log("Hand forward direction on grab: " + handForward);
		
		// Get the releasing hand
        GameObject releasingHand = currentInteractor.gameObject;

        // Get the direction the hand is pointing
        Vector3 releaseDirection = releasingHand.transform.forward;
  Debug.Log("released");
        // Set the sphere's velocity to move it in that direction at a speed of 2 m/s
        rb.velocity = releaseDirection * 2f;
		
    }
}



/* private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.onSelectExited.AddListener(HandleRelease);
		 
		 Debug.Log("awake");
    }
*/


   /* private void OnDestroy()
    {
        grabInteractable.onSelectExited.RemoveListener(HandleRelease);
    }*/

   /* private void HandleRelease(XRBaseInteractor interactor)
    {
        // Get the releasing hand
        GameObject releasingHand = interactor.gameObject;

        // Get the direction the hand is pointing
        Vector3 releaseDirection = releasingHand.transform.forward;
  Debug.Log("released");
        // Set the sphere's velocity to move it in that direction at a speed of 2 m/s
        rb.velocity = releaseDirection * 2f;
    }
	*/
}

