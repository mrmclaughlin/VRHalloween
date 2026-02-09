


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class moveXRRigUP : MonoBehaviour
{
  public GameObject xrRig; // Drag your XRRig GameObject here from the Unity Editor
  public int distanceUp;
    // Start is called before the first frame update
    void Start()
    {
        // Move it up by 5 units along the Y-axis
        if (xrRig != null)
        {
            xrRig.transform.position += new Vector3(0, distanceUp, 0);
        }
    }
}