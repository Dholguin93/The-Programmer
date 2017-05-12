using UnityEngine;
using System.Collections;

public class DontDestroy : MonoBehaviour {

	static DontDestroy instance;
	// Use this for initialization
	void Start () 
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad (this.gameObject);

		}
		else
		{
			Destroy (this.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
