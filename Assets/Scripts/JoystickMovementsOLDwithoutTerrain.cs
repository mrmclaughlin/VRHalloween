using UnityEngine;
using UnityEngine.XR;

public class JoystickMovementsold : MonoBehaviour
{
    public float speed = 1.0f;
    public Transform xrRig;
private Transform cameraTransform; // Reference to the camera transform

    void Start()
    {
        // Find and store a reference to the main camera's transform
        cameraTransform = Camera.main.transform;
    }
    void Update()
    {
        InputDevice deviceRight = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		InputDevice deviceLeft = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        Vector2 primary2DAxisValue;
		Vector2 joystickYValue;
        if (deviceRight.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxisValue))
        {
            // Move the XR Rig
            //Vector3 direction = new Vector3(primary2DAxisValue.x, 0, primary2DAxisValue.y);
           // xrRig.position += xrRig.rotation * direction * speed * Time.deltaTime;
		   // Calculate the forward direction relative to the camera's orientation, ignoring vertical tilt
            Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
            Vector3 right = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

            // Move the XR Rig in the direction the camera is facing
            Vector3 direction = forward * primary2DAxisValue.y + right * primary2DAxisValue.x;
            xrRig.position += direction * speed * Time.deltaTime;
        }
		
		
		if (deviceLeft.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickYValue))
            {
                // Move the XR Rig up and down based on joystick's Y-axis value
                Vector3 moveDirection = new Vector3(0, joystickYValue.y, 0);
                xrRig.position += moveDirection * speed * Time.deltaTime;
            }
		
		
    }
}
