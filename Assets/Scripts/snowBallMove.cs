using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snowBallMove : MonoBehaviour
{
	
	 
	private Transform TransformBall;
	   	private GameObject[] snowBallPrefabs; 
    public float spacing = 1.0f; // The amount of space between each snowball
 public int pyramidBaseSize = 20; // Number of snowballs along the base edge
	
	
	
 
 
    void Start()
    {
		 // Dynamically populate the snowBallPrefabs array based on the naming pattern
        snowBallPrefabs = new GameObject[55];
        for (int i = 1; i <= 55; i++)
        {
            snowBallPrefabs[i - 1] = GameObject.Find("SnowBallPrefab (" + i + ")");
        }
		
		 
		Vector3 startPosition = Vector3.zero; // Starting position set to (0,0,0)

        int prefabIndex = 0; // To keep track of which prefab to instantiate next

        for (int y = 0; y < pyramidBaseSize; y++) // Loop for each layer
        {
            for (int x = 0; x < pyramidBaseSize - y; x++) // Loop for each row
            {
                for (int z = 0; z < pyramidBaseSize - y; z++) // Loop for each column
                {
                    // Calculate the position for the current snowball
                    Vector3 snowBallPosition = new Vector3(
                        startPosition.x + x * spacing,
                        startPosition.y+.3f + y * spacing,
                        startPosition.z + z * spacing
                    );

                    // Offset to center the pyramid at the starting position
                    snowBallPosition.x -= (pyramidBaseSize - y - 1) * spacing / 2;
                    snowBallPosition.z -= (pyramidBaseSize - y - 1) * spacing / 2;

                   if (snowBallPrefabs[prefabIndex])
                    {
                        snowBallPrefabs[prefabIndex].transform.position = snowBallPosition;
                    }

                  
                    prefabIndex = (prefabIndex + 1) % snowBallPrefabs.Length;
                }
            }
        }
    }
		
		
		
		
		
	 
    

 
	
	
	


    void Update()
    {
        
    }
}
