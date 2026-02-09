using UnityEngine;

public class PassthroughWindowBinder : MonoBehaviour
{
    public OVRPassthroughLayer passthroughLayer;
    public MeshFilter windowMesh;   // the Quad’s MeshFilter

    void Start()
    {
        if (!passthroughLayer || !windowMesh)
        {
            Debug.LogError("Assign passthroughLayer and windowMesh.");
            return;
        }

        // Make sure passthrough is running
        passthroughLayer.enabled = true;

        // Register this mesh as a passthrough surface
        passthroughLayer.AddSurfaceGeometry(windowMesh.gameObject);
    }
}
