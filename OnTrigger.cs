using UnityEngine;
using System.Collections;

public class OnTrigger: MonoBehaviour {

	void OnTriggerEnter(Collider x)
	{
		Debug.Log ("Trigger Activated");
	}
}
