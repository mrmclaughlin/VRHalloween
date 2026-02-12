using UnityEngine;
using UnityEngine.XR;

public class ForceRecenter : MonoBehaviour
{
    public void Recenter()
    {
        InputTracking.Recenter();
    }
}
