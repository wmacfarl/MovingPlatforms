using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PhysicsScript : MonoBehaviour {

    MovementControllerScript movementControllerScript;

    //this is the velocity of the object -- it persists and is modified from frame to frame
    public Vector2 velocity;
    public bool onGround = false;
    public bool lastOnGround = false;

    public float gravity;
    public float groundFriction;
    public float airFriction;

    //this is the velocity of the object's frame of reference (moving platform, etc) -- it is new every frame
    public Vector2 baseVelocity;

	// Use this for initialization
	void Start () {
        movementControllerScript = GetComponent<MovementControllerScript>();
	}

    public void MovementUpdate()
    {
        lastOnGround = onGround;
        onGround = checkIfGrounded();
     
        if (onGround && velocity.y <= 0)
        {
            velocity.y = 0;
            velocity.x *= (1 - groundFriction);
        }
        else
        {
            velocity.x *= (1 - airFriction);
        }

        if (velocity.y > 0 && hittingCeiling())
        {
            movementControllerScript.Move(new Vector2(0, velocity.y));
            velocity.y = 0;
        }

        if (!onGround && lastOnGround)
        {
            foreach (GameObject go in movementControllerScript.collisionState.thingsIWasStandingOn)
            {
                MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
                if (mcs != null)
                {
                    velocity += mcs.amountMovedLastFrame;
                }
            }
        }
        velocity.y -= gravity;
    }

    bool hittingCeiling()
    {
        foreach (RaycastHit2D hit in movementControllerScript.VerticalRaycastHits(velocity.y))
        {
            if (hit.transform != null)
            {
                return true;
            }
        }
        return false;
    }

    bool checkIfGrounded()
    {
        foreach (RaycastHit2D hit in movementControllerScript.VerticalRaycastHits(-.001f))
        {
            if (hit.transform != null)
            {
                return true;
            }
        }
        return false;
    }

}
