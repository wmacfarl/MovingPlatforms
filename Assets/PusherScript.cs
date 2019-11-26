using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PusherScript : MonoBehaviour {
    public int pusherLevel;
    //should I be able to push more than one thing?

    public GameObject thingIAmPushing;
    public GameObject thingIAmPulling;

    List<GameObject> thingsIAmFacing;
    MovementControllerScript movementControllerScript;
    PhysicsScript physicsScript;
    public bool amIPushing;
    public float pushFacing;
 

    private void Start()
    {
        physicsScript = GetComponent<PhysicsScript>();
        movementControllerScript = GetComponent<MovementControllerScript>();
    }

    public GameObject GetFirstPushable(List<GameObject> gameObjects)
    {
        GameObject pushable = null;
        foreach (GameObject go in gameObjects)
        {
            MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
            if (mcs != null)
            {
                if (mcs.pushableLevel.x < pusherLevel)
                {
                    return mcs.gameObject;
                }
            }
        }
        return pushable;
    }


    public void MovementUpdate()
    {
        StopPushing();
        thingsIAmFacing = WhatAmIFacing(pushFacing);



        if (amIPushing)
        {
            GameObject thingToMove = GetFirstPushable(thingsIAmFacing);
            if (Mathf.Sign(physicsScript.velocity.x) == pushFacing){
                thingIAmPushing = thingToMove;
            }else{
                thingIAmPulling = thingToMove;
            }
        }
    }

    public List<GameObject> WhatAmIFacing(float direction)
    {
        List<GameObject> objects = new List<GameObject>();

        if (direction != 0)
        {
            direction = Mathf.Sign(direction);

            List<RaycastHit2D> hits = movementControllerScript.HorizontalRaycastHits(direction * .01f);

            for (int i = 0; i <hits.Count/2; i++) {
               RaycastHit2D hit = hits[i];
            
                if (hit.transform != null)
                {
                    if (!objects.Contains(hit.transform.gameObject))
                    {
                        objects.Add(hit.transform.gameObject);
                    }
                }
            }
        }
        return objects;
    }

    public void StartPushing(List<GameObject> objects)
    {
        StopPushing();
        foreach (GameObject go in objects)
        {
            MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
            if (mcs != null) {
                if (mcs.basePushableLevel != -1)
                {
                    {
                        if (!movementControllerScript.attachedObjects.Contains(go))
                        {
                                thingIAmPushing = go;
                                mcs.collisionState.beingPushed = true;
                                movementControllerScript.attachedObjects.Add(go);
                                break;                          
                        }
                    }
                }
            }
        }
    }

    public void StopPushing()
    {
//        movementControllerScript.attachedObjects.Remove(thingIAmPushing);
        thingIAmPushing = null;
        thingIAmPulling = null;
    }
}
 