using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningReticle : MonoBehaviour
{
    public GameObject reticleCircular;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateSpin();
    }
    void updateSpin(){
 
    reticleCircular.transform.localRotation = Quaternion.Euler(0, System.DateTime.Now.Second * 360/60, 0);
    }
}
