using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityScript : MonoBehaviour {
    PhysicsScript physicsScript;
    
	// Use this for initialization
	void Start () {
        physicsScript = GetComponent<PhysicsScript>();
	}
	
	// Update is called once per frame
	void Update () {
        physicsScript.velocity.y -= .01f;		
	}
}
