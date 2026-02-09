using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sunTime : MonoBehaviour
{
public GameObject sunAngle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateSunAngle();
    }
    void updateSunAngle()
    {
    
    sunAngle.transform.localRotation = Quaternion.Euler((System.DateTime.Now.Hour)*360f/12,0,0);
    }
}
