using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformScript : MonoBehaviour {

    public List<Vector2> waypoints;
    public Vector2 velocity;
    MovementControllerScript movementControllerScript;
    new BoxCollider2D collider;
    public float speed;
    int currentWaypointIndex;
    bool movedLastFrame;

	// Use this for initialization
	void Start () {
        collider = GetComponent<BoxCollider2D>();
        movementControllerScript = GetComponent<MovementControllerScript>();
        currentWaypointIndex = 0;
	}
	
    public void MovementUpdate()
    {
        bool switched = false;
        Vector2 delta = waypoints[currentWaypointIndex] - (Vector2)transform.position;

        if (delta.magnitude < speed)
        {
            velocity = delta;
            nextWaypoint();
            switched = true;
        }
        else
        {
            velocity = delta.normalized * speed;
        }

        Vector2 amountMoved = movementControllerScript.Move(velocity);

        //this goes to the next waypoint if the platform is stopped by something -- it's not clear that this is the right solution
        if (Mathf.Abs(amountMoved.magnitude) < Mathf.Abs(velocity.magnitude)*.2 && !switched)
        {
            if (!movedLastFrame)
            {
                nextWaypoint();
            }
            movedLastFrame = false;
        }
        else
        {
            movedLastFrame = true;
        }
    }

    void nextWaypoint()
    {
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Count)
        {
            currentWaypointIndex = 0;
        }
    }

   
}
