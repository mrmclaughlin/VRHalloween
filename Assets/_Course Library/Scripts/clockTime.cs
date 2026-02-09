using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clockTime : MonoBehaviour
{

    public GameObject secondHand;
    public GameObject minuteHand;
    public GameObject hourHand;
// Update is called once per frame
    void Update()
    {
        UpdateHourHand();
    }

    void UpdateHourHand()
    {
       hourHand.transform.localRotation = Quaternion.Euler((System.DateTime.Now.Hour)*360f/12,0,0);
       minuteHand.transform.localRotation = Quaternion.Euler((System.DateTime.Now.Minute) * 360/60, 0, 0);
       secondHand.transform.localRotation = Quaternion.Euler(System.DateTime.Now.Second * 360/60, 0, 0);
    }
}
