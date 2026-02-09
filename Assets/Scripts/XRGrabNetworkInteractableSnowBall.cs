using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;


public class XRGrabNetworkInteractableSnowBall : XRGrabInteractable
{
	
	private Rigidbody rigidBody;
    private XRBaseInteractor grabInteractor;
    private Vector3 interactorVelocity;
    private Vector3 interactorAngularVelocity;
	//[Tooltip("The projectile that's created")]
    //public GameObject projectilePrefab = null;

    //[Tooltip("The point that the project is created")]
    //public Transform startPoint = null;

    [Tooltip("The speed at which the projectile is launched")]
    public float launchSpeed = .0010f;
	 private XRBaseInteractor currentInteractor;
	
	private PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
      photonView = GetComponent<PhotonView>();
    }
	
	
	protected override void Awake()
    {
        base.Awake();
        rigidBody = GetComponent<Rigidbody>();
    }
	
	
	

    // Update is called once per frame
    void Update()
    {
        
    }
	
	
 
	/*protected override void OnSelectEntering(XRBaseInteractor interactor)
	{
		
		base.OnSelectEntering(interactor);
		photonView.RequestOwnership();
	currentInteractor = interactor;

        // Get and log the forward direction of the hand that grabbed the object
        Vector3 handForward = currentInteractor.transform.forward;
        Debug.Log("Hand forward direction on grab: " + handForward);
	}*/
	
	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		
		base.OnSelectEntering(args);
		photonView.RequestOwnership();
		currentInteractor = args.interactorObject as XRBaseInteractor;
	 

        // Get and log the forward direction of the hand that grabbed the object
        Vector3 handForward = currentInteractor.transform.forward;
        Debug.Log("Hand forward direction on grab: " + handForward);
	}
	
	protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
		currentInteractor = args.interactorObject as XRBaseInteractor;
       Vector3 handForward = currentInteractor.transform.forward;
	 Debug.Log("Hand forwards direction on release: " + handForward);
		GameObject obj = GameObject.Find(gameObject.name);
		Rigidbody rb = obj.GetComponent<Rigidbody>();
		Vector3 force = handForward * launchSpeed;
        rb.AddForce(force);
	 
	}
	 
	
	
	
	
	
	
	
	
	/*protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        base.OnSelectExited(interactor);
		currentInteractor = interactor;
       Vector3 handForward = currentInteractor.transform.forward;
        Debug.Log("Hand forward direction on release: " + handForward);
		GameObject obj = GameObject.Find(gameObject.name);
		Rigidbody rb = obj.GetComponent<Rigidbody>();
		Vector3 force = handForward * launchSpeed;
        rb.AddForce(force);
		
    }*/
	
	
	

	
	
	
}
