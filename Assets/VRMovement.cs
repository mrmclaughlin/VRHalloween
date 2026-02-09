using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class VRMovement : MonoBehaviour
{
	public Slider moveSlider;
    public float moveDistance = 1f;
	private Transform cameraTransform; // Reference to the camera transform

    void Start()
    {
        // Find and store a reference to the main camera's transform
        cameraTransform = Camera.main.transform;
    }
public void OnSliderValueChanged()
    {
        moveDistance = moveSlider.value;
        // You can add additional logic here if you want to do something with the variable
        Debug.Log("New value: " + moveDistance);
    }
     public void MoveForward()
    {
        Vector3 direction = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        transform.position += direction * moveDistance;
    }
	
    public void MoveBackward()
    {
        Vector3 direction = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        transform.position -= direction * moveDistance;
    }
	
    public void MoveLeft()
    {
        Vector3 direction = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;
        transform.position -= direction * moveDistance;
    }
	
    public void MoveRight()
    {
        Vector3 direction = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;
        transform.position += direction * moveDistance;
    }
}
