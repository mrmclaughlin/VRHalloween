using UnityEngine;

public class RandomMaterialAssigner : MonoBehaviour
{
    public Material[] materials; // Array to hold the materials
    public Renderer teacherFaceRenderer; // Reference to the renderer of the TeacherFace object

    void Start()
    {
        if (materials.Length > 0 && teacherFaceRenderer != null)
        {
            // Randomly select a material
            Material selectedMaterial = materials[Random.Range(0, materials.Length)];
            // Assign the selected material to the TeacherFace
            teacherFaceRenderer.material = selectedMaterial;
        }
    }
}
