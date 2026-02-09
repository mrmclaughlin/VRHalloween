using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRigRandomLocationStart : MonoBehaviour
{
	private Transform randomPosition;
	private Transform TransformXRrig;
	public float areaRadius = 0f; 
    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = new Vector3(
                Random.Range(-areaRadius, areaRadius),
                Random.Range(0, 0),
                Random.Range(-areaRadius, areaRadius));
		TransformXRrig = GetComponent<Transform>();
		//TransformXRrig.position = randomPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
