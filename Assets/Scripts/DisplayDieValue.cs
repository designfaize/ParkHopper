using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkHopper.Events;

public class DisplayDieValue : MonoBehaviour {
	public LayerMask dieValueColliderLayer;
	public int currentValue = 1;
	public Rigidbody rb;

	private bool rollComplete=true;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if (Physics.Raycast (transform.position, Vector3.up, out hit, Mathf.Infinity, dieValueColliderLayer))
		{
			currentValue = hit.collider.GetComponent<DieValue> ().value;
		}
		if (rb.IsSleeping ()&& !rollComplete) 
		{
			EventDispatcher.DispatchEvent (new DieAtRestEvent (currentValue));
			rollComplete = true;
		}
		else if (!rb.IsSleeping ())
			rollComplete = false;
	}
	 
}