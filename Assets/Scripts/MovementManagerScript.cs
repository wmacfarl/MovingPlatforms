using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManagerScript : MonoBehaviour {

    public GameObject objectToSpawn;

    List<BoxScript> boxScripts;
    List<PlayerScript> playerScripts;
    List<MovingPlatformScript> movingPlatformScripts;
    List<MovementControllerScript> movementControllerScripts;
    List<PhysicsScript> physicsScripts;

	// Use this for initialization
	void Start () {
        boxScripts = new List<BoxScript>();
        boxScripts.AddRange(FindObjectsOfType<BoxScript>());
        playerScripts = new List<PlayerScript>();
        playerScripts.AddRange(FindObjectsOfType<PlayerScript>());
        movingPlatformScripts = new List<MovingPlatformScript>();
        movingPlatformScripts.AddRange(FindObjectsOfType<MovingPlatformScript>());
        movementControllerScripts = new List<MovementControllerScript>();
        movementControllerScripts.AddRange(FindObjectsOfType<MovementControllerScript>());
        physicsScripts = new List<PhysicsScript>();
        physicsScripts.AddRange(FindObjectsOfType<PhysicsScript>());
    }

    // Update is called once per frame
    void Update () {


        foreach (MovingPlatformScript movingPlatformScript in movingPlatformScripts)
        {
            movingPlatformScript.MovementUpdate();
        }

        foreach (PlayerScript playerScript in playerScripts)
        {
            playerScript.MovementUpdate();
        }

        foreach (BoxScript boxScript in boxScripts)
        {
            boxScript.MovementUpdate();
        }

        foreach (MovementControllerScript mcs in movementControllerScripts)
        {
            mcs.Cleanup();
        }
    }
}