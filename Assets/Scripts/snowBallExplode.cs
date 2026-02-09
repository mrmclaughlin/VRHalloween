using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class snowBallExplode : MonoBehaviour
{
	
	
	 
	public GameObject particleEffectPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

void OnCollisionEnter(Collision collision)
    {
		//Debug.Log("Snowball hit a " + collision.gameObject.tag+ "called " + collision.gameObject.name + "Should it explode??");
		
        if ((collision.gameObject.tag =="Target")|| collision.gameObject.name.Contains("Target") || collision.gameObject.name.Contains("Tree") || collision.gameObject.name.Contains("Spruce")|| collision.gameObject.name.Contains("Pine"))
        {
			//Debug.Log("YES because the Snowball hit a " + collision.gameObject.tag+ "called " + collision.gameObject.name);
            // Instantiate the particle effect at the target's location
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
           Destroy(gameObject);
			
        }
		
	}



    // Update is called once per frame
    void Update()
    {
        
    }
}
