using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MirrorMat_Change : MonoBehaviour
{
	public Material Clear;
	public Material Mirror;
	//private Material theMat;
    // Start is called before the first frame update
    void Start()
    {
        //theMat = GetComponent<Renderer>().material;
    }

    public void SetMetClear()
	{
		GetComponent<Renderer>().material = Clear;
	}
	
	public void SetMatMirror()
	{
		GetComponent<Renderer>().material = Mirror;
	}
	
	
	
	// Update is called once per frame
    void Update()
    {
        
    }
}
