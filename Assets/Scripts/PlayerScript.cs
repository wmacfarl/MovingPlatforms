using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    public bool pushPressed;
    PhysicsScript physicsScript;
    public float speed;
    public float jumpVelocity;
    int facing;
    PusherScript pusherScript;
    MovementControllerScript movementControllerScript;
    SpriteRenderer sprite;


    // Use this for initialization
    void Start() {
        physicsScript = GetComponent<PhysicsScript>();
        sprite = GetComponent<SpriteRenderer>();
        pusherScript = GetComponent<PusherScript>();
        movementControllerScript = GetComponent<MovementControllerScript>();
        facing = 1;
    }


    public void MovementUpdate()
    {
        sprite.flipX = (facing == -1);
        physicsScript.MovementUpdate();
        pusherScript.amIPushing = (pushPressed && physicsScript.onGround && Mathf.Abs(physicsScript.velocity.x) > 0);
        pusherScript.pushFacing = facing;
        pusherScript.MovementUpdate();
        if (pusherScript.thingIAmPushing != null)
        {
            pusherScript.thingIAmPushing.GetComponent<MovementControllerScript>().Move(new Vector2(physicsScript.velocity.x, 0));
        }
        movementControllerScript.Move(physicsScript.velocity);
        if (pusherScript.thingIAmPulling != null)
        {
            pusherScript.thingIAmPulling.GetComponent<MovementControllerScript>().Move(new Vector2(physicsScript.velocity.x, 0));
        }
    }

    public void left()
    {
        physicsScript.velocity.x = -speed;
        if (!pushPressed)
        {
            facing = -1;
        }
    }
    public void right()
    {
        physicsScript.velocity.x = speed;
        if (!pushPressed)
        {
            facing = 1;
        }
    }
    public void up()
    {
        if (physicsScript.onGround)
        {
            physicsScript.velocity.y = jumpVelocity;
        }
    }
    public void down()
    {

    }

    public void push()
    {

    }
}
