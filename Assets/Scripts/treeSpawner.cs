using UnityEngine;

public class treeSpawner : MonoBehaviour
{
    public GameObject treePrefab;  // Assign the tree prefab in the Unity Inspector
    private int numberOfTrees = 20;  // Number of trees you want to spawn
   private float spawnAreaWidth = 50f;  // Width of the spawn area
    private float spawnAreaDepth = 50f;  // Depth of the spawn area
    public Vector3 spawnAreaCenter;  // Center of the spawn area
    private float yOffset = 0f;  // Vertical offset for spawned trees

    void Start()
    {
        int spawnedTrees = 0;

        while (spawnedTrees < numberOfTrees)
        {
            // Generate random x and z coordinates within the spawn area
            float x = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2) + spawnAreaCenter.x;
            float z = Random.Range(-spawnAreaDepth / 2, spawnAreaDepth / 2) + spawnAreaCenter.z;

            // Check if the position is outside the 10x10 center box
            if (Mathf.Abs(x - spawnAreaCenter.x) > 20 || Mathf.Abs(z - spawnAreaCenter.z) > 20)
            {
                // Create the position Vector3
                Vector3 spawnPosition = new Vector3(x, spawnAreaCenter.y + yOffset, z);

                // Instantiate the tree prefab at the random position
                GameObject tree = Instantiate(treePrefab, spawnPosition, Quaternion.identity);

                // Randomly rotate tree around the Y-axis
                float randomYRotation = Random.Range(0, 360);
                tree.transform.eulerAngles = new Vector3(0, randomYRotation, 0);

                spawnedTrees++;
            }
        }
    }
}
