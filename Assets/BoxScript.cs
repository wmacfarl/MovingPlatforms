using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour {

    PhysicsScript physicsScript;
    MovementControllerScript movementControllerScript;

    public void MovementUpdate()
    {
        physicsScript.MovementUpdate();
        movementControllerScript.Move(physicsScript.velocity);
    }

	// Use this for initialization
	void Start () {
        physicsScript = GetComponent<PhysicsScript>();
        movementControllerScript = GetComponent<MovementControllerScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
