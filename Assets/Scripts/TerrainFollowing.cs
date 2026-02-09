using UnityEngine;

public class TerrainFollowing : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject XRRig;
    private float yOffset = -.5f; // Offset to adjust the height (optional)

public void Start(){
	XRRig.transform.position =  new Vector3(0f, -.5f, -2f);
}



    private void Update()
    {
        if (mainCam != null && XRRig != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCam.transform.position, Vector3.down, out hit))
            {
                // Check if the object hit by the raycast is a terrain
                Terrain terrain = hit.collider.GetComponent<Terrain>();
                if (terrain != null)
                {
                    // Get terrain height at hit point
                    float terrainHeight = terrain.SampleHeight(hit.point);

                    // Adjust the position of the XR rig camera
                    Vector3 newPosition = XRRig.transform.position;
                    newPosition.y = terrainHeight + yOffset;
                    XRRig.transform.position = newPosition;
					Debug.Log(terrainHeight + yOffset);
                }
                else
                {
                    Debug.Log("Terrain object not found.");
                }
            }
            else
            {
                Debug.Log("No terrain found below the camera.");
            }
        }
        else
        {
            Debug.Log("Main camera or XR rig not assigned.");
        }
    }
}
