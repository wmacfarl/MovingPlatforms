using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MovementControllerScript : MonoBehaviour {

    public LayerMask collisionLayer;
    List<GameObject> gameObjectsToIgnoreCollisonsWith;
    new BoxCollider2D collider;
    public CollisionState collisionState;
    public int horizontalRayCount;
    public int verticalRayCount;
    float horizontalRaySpacing;
    float verticalRaySpacing;
    public float skinWidth;
    public List<GameObject> attachedObjects;
    public List<GameObject> passengers;
    public Vector2 amountMovedThisFrame;
    public Vector2 amountMovedLastFrame;
    public int basePushableLevel;
    public const float MIN_DISTANCE = .001f;
    public int tmpPushableLevel;
    public bool beenCarriedThisFrame = false; //this is to make sure that I can only be carried by one object, 
                                              //even if I am standing on two simultaneously and they're both moving
                                              //(standing between two boxes on a moving platform)


    public Vector2 pushableLevel;
    public Vector2 pusherLevel;

    int numberOfPassengerMovementPasses = 4;

    public class CollisionState
    {
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
        public Vector2 topLeft;
        public Vector2 topRight;

        public List<RaycastHit2D> leftHits;
        public List<RaycastHit2D> topHits;
        public List<RaycastHit2D> bottomHits;
        public List<RaycastHit2D> rightHits;

        public List<GameObject> thingsIAmStandingOn;
        public List<GameObject> thingsIWasStandingOn;
        public bool isPassenger;
        public bool beingPushed;
        public CollisionState()
        {
            thingsIAmStandingOn = new List<GameObject>();
            thingsIWasStandingOn = new List<GameObject>();

            leftHits = new List<RaycastHit2D>();
            topHits = new List<RaycastHit2D>();
            bottomHits = new List<RaycastHit2D>();
            rightHits = new List<RaycastHit2D>();
        }

        public void resetCollisions()
        {
            beingPushed = false;
            //this is used to make sure something can only be a passenger of 1 platform -- I don't know if it's the best way
            isPassenger = false;
            thingsIWasStandingOn = new List<GameObject>();
            thingsIWasStandingOn.AddRange(thingsIAmStandingOn);
            thingsIAmStandingOn = new List<GameObject>();
            foreach (RaycastHit2D hit in bottomHits)
            {
                if (hit.transform != null)
                {
                    if (!thingsIAmStandingOn.Contains(hit.transform.gameObject))
                    {
                        thingsIAmStandingOn.Add(hit.transform.gameObject);
                    }
                }
            }

            leftHits = new List<RaycastHit2D>();
            topHits = new List<RaycastHit2D>();
            bottomHits = new List<RaycastHit2D>();
            rightHits = new List<RaycastHit2D>();
        }

        public Vector2 getCorner(int x, int y)
        {
            if (x == -1 && y == -1)
            {
                return bottomLeft;
            }
            else if (x == -1 && y == 1)
            {
                return topLeft;
            }
            else if (x == 1 && y == -1)
            {
                return bottomRight;
            }
            else if (x == 1 && y == 1)
            {
                return topRight;
            }
            else
            {
                Debug.Log("INVALID CORNER");
                return new Vector2(Mathf.Infinity, Mathf.Infinity);
            }
        }
    }


    public void Cleanup()
    {
        collisionState.resetCollisions();
        amountMovedLastFrame = amountMovedThisFrame;
        amountMovedThisFrame = new Vector2();
        beenCarriedThisFrame = false;
    }

    // Use this for initialization
    void Start () {
        gameObjectsToIgnoreCollisonsWith = new List<GameObject>();
        attachedObjects = new List<GameObject>();
        this.collisionState = new CollisionState();
        collider = GetComponent<BoxCollider2D>();
        calculateRaySpacing();
        UpdateCollisionState();
        this.collisionState.resetCollisions();
	}

    void calculateRaySpacing()
    {
        horizontalRaySpacing = ((collider.bounds.extents.y) * 2) / (horizontalRayCount-1);
        verticalRaySpacing = ((collider.bounds.extents.x) * 2) / (verticalRayCount-1);
    }

    void UpdateCollisionState()
    {
        collisionState.bottomLeft = collider.bounds.min;
        collisionState.topRight = collider.bounds.max;
        collisionState.bottomRight = new Vector2(collisionState.topRight.x, collisionState.bottomLeft.y);
        collisionState.topLeft = new Vector2(collisionState.bottomLeft.x, collisionState.topRight.y);
    }

    List<RaycastHit2D> FindRaycastHits(Vector2 direction, float distance)
    {
     
        if (direction.y == 0)
        {

            return HorizontalRaycastHits(Mathf.Sign(direction.x)*distance);
        }
        else if (direction.x == 0)
        {
            return VerticalRaycastHits(Mathf.Sign(direction.y) * distance);
        }
        else
        {
            //can only do 1 dimension at a time
            Debug.Log("INVALID DIRECTION");
            return null;
        }
    }

    public List<RaycastHit2D> HorizontalRaycastHits(float distance)
    {
        if (distance > 0)
        {
            collisionState.bottomRight = new Vector2(collider.bounds.max.x, collider.bounds.min.y);
        }else if (distance < 0)
        {
            collisionState.bottomLeft = collider.bounds.min;
        }
        else { 
            return null;
        }
            int direction = (int)Mathf.Sign(distance);
            List<RaycastHit2D> horizontalHits = new List<RaycastHit2D>(horizontalRayCount);
            Vector2 corner = collisionState.getCorner(direction, -1);
            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = new Vector2(corner.x, horizontalRaySpacing * i + corner.y);
                if (i == 0)
                {
                    rayOrigin.y += skinWidth;
                }
                if (i == horizontalRayCount - 1)
                {
                    rayOrigin.y -= skinWidth;
                 }
            Debug.DrawRay(rayOrigin, new Vector2(distance, 0), Color.blue);
            horizontalHits.Add(ObstacleRaycast(rayOrigin, new Vector2(distance, 0), Mathf.Abs(distance)));

        }
        return horizontalHits;
    }


    public List<RaycastHit2D> VerticalRaycastHits(float distance)
    {
        if (distance < 0)
        {
            collisionState.bottomHits = new List<RaycastHit2D>();
            collisionState.bottomLeft = collider.bounds.min;
        }else if (distance > 0)
        {
            collisionState.topLeft = new Vector2(collider.bounds.min.x, collider.bounds.max.y);
            collisionState.topHits = new List<RaycastHit2D>();
        }
        else
        {
            return null;
        }
        
            int direction = (int) Mathf.Sign(distance);
            List<RaycastHit2D> verticalHits = new List<RaycastHit2D>(verticalRayCount);
            Vector2 corner = collisionState.getCorner(-1, direction);

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = new Vector2(corner.x + verticalRaySpacing * i, corner.y);
                if (i == 0)
                {
                    rayOrigin.x += skinWidth;
                }
                if (i == verticalRayCount - 1)
                {
                    rayOrigin.x -= skinWidth;
                }
            Debug.DrawRay(rayOrigin, new Vector2( 0, distance),Color.blue);

            verticalHits.Add(ObstacleRaycast(rayOrigin, new Vector2(0,distance), Mathf.Abs(distance)));
            }
            return verticalHits;
        
    }

    RaycastHit2D ObstacleRaycast(Vector2 origin, Vector2 direction, float distance) {
        RaycastHit2D goodHit = new RaycastHit2D();
        RaycastHit2D[] allHits = Physics2D.RaycastAll(origin, direction * distance, collisionLayer);
        float checkDistance = distance;
        foreach (RaycastHit2D hit in allHits)
        {
            if (hit.transform != null)
            {
                if (hit.transform.gameObject != gameObject && hit.distance <= checkDistance 
                    //&& !gameObjectsToIgnoreCollisonsWith.Contains(hit.transform.gameObject)
                    )
                {
                    goodHit = hit;
                    checkDistance = hit.distance;
                }
            }
        }
 
        return goodHit;
    }

    RaycastHit2D GetClosestHit(List<RaycastHit2D> hits)
    {
        RaycastHit2D closestHit = new RaycastHit2D();
        float nearDistance = Mathf.Infinity;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
            {
                if (hit.distance < nearDistance)
                {
                    closestHit = hit;
                }
            }
        }
        return closestHit;
    }

    public Vector2 Move(Vector2 delta)
    {
        return Move(delta, pusherLevel, 0);
    }

    public Vector2 Move(Vector2 delta, Vector2 sourcePusherLevel, int depthCount)
    {
        depthCount++;
        if (depthCount > 10)
        {
            return new Vector2(0, 0);
        }else { 
            //Move attempts to move in the x dimension and then the y
            //it only moves us as far as we can move, and returns however far that was

            float direction = 0;
            Vector2 amountLeftToMove = delta;
            Vector2 totalAmountMoved = new Vector2();

            List<RaycastHit2D> currentPushHits = new List<RaycastHit2D>();
            List<RaycastHit2D> currentCarryHits = new List<RaycastHit2D>();

            List<MovementControllerScript> hitThingsXmcs = new List<MovementControllerScript>();
            List<MovementControllerScript> hitThingsYmcs = new List<MovementControllerScript>();

            for (int i = 0; i < numberOfPassengerMovementPasses; i++)
            {
                Vector2 amountMoved = new Vector2(0, 0);

                if (Mathf.Abs(amountLeftToMove.x) >  0)
                {
                    direction = Mathf.Sign(amountLeftToMove.x);
                    currentPushHits = HorizontalRaycastHits(MIN_DISTANCE * direction);
                    RaycastHit2D closestHit = GetClosestHit(currentPushHits);
                    if (closestHit.transform != null)
                    {
                        MovementControllerScript mcs = closestHit.transform.gameObject.GetComponent<MovementControllerScript>();
                        if (mcs != null)
                        {
                            bool shouldGetPushed = (mcs.pushableLevel.x < sourcePusherLevel.x);
                            PhysicsScript physicsScript = mcs.GetComponent<PhysicsScript>();
                            if (physicsScript != null)
                            {
                                if (physicsScript.onGround == false || physicsScript.velocity.y > 0)
                                    //this is a convulted way of checking if I'm in the air and therefore should be pushed
                                {
                                    shouldGetPushed = true;
                                }
                            }
                            if (shouldGetPushed)
                            {
                                hitThingsXmcs.Add(mcs);
                                mcs.Move(new Vector2(amountLeftToMove.x, 0), sourcePusherLevel, depthCount);
                            }
                        }
                    }

                    amountMoved.x = CalculateMoveX(amountLeftToMove);
                    List<GameObject> carryObjects = new List<GameObject>();

                        currentCarryHits = VerticalRaycastHits(MIN_DISTANCE);
                        foreach (RaycastHit2D hit in currentCarryHits)
                        {
                            if (hit.transform != null)
                            {
                                if (!carryObjects.Contains(hit.transform.gameObject))
                                {
                                if (hit.transform.gameObject.GetComponent<PhysicsScript>() != null)
                                {
                                    carryObjects.Add(hit.transform.gameObject);
                                }
                                }
                            }
                        }
                    if (carryObjects.Count > 0 && amountMoved.x != 0)
                    {
                        List<GameObject> carryObjectsSorted = SortGameObjectsByPositionX(carryObjects);

                        int iStart;
                        int iDirection;
                        int iEnd;
                        if (amountMoved.x < 0)
                        {
                            iStart = 0;
                            iDirection = 1;
                            iEnd = carryObjectsSorted.Count;
                        }
                        else
                        {
                            iStart = carryObjectsSorted.Count - 1;
                            iDirection = -1;
                            iEnd = -1;
                        }
                        for (int j = iStart; j != iEnd; j += iDirection)
                        {
                            GameObject carryObject = carryObjectsSorted[j];
                            MovementControllerScript mcs = carryObject.GetComponent<MovementControllerScript>();
                            if (mcs != null)
                            {
                                if (!mcs.beenCarriedThisFrame)
                                {
                                    mcs.beenCarriedThisFrame = true;
                                    mcs.Move(new Vector2(amountMoved.x, 0));
                                }
                            }
                        }
                    }
                }

                TranslateAndRecord(amountMoved);
                if (Mathf.Abs(amountLeftToMove.y) > 0)
                {
                    direction = Mathf.Sign(amountLeftToMove.y);
                    currentPushHits = VerticalRaycastHits(MIN_DISTANCE * direction);

                    List<GameObject> hitObjects = new List<GameObject>();
                    foreach (RaycastHit2D hit in currentPushHits)
                    {
                        if (hit.transform != null)
                        {
                            if (!hitObjects.Contains(hit.transform.gameObject))
                            {
                                hitObjects.Add(hit.transform.gameObject);
                            }
                        }
                    }
                    List<GameObject> pushedObjects = new List<GameObject>();
                    foreach (GameObject hitObject in hitObjects)
                    {
                        MovementControllerScript hitMCS = hitObject.GetComponent<MovementControllerScript>();
                        if (hitMCS != null)
                        {
                            if (hitMCS.pushableLevel.y < sourcePusherLevel.y)
                            {
                                if (!pushedObjects.Contains(hitMCS.gameObject))
                                {
                                    pushedObjects.Add(hitMCS.gameObject);
                                }

                            }
                        }
                    }

                    float minDistanceMoved = Mathf.Infinity;
                    float[] distancesMoved = new float[pushedObjects.Count];

                    for (int j = 0; j < pushedObjects.Count; j++)
                    {
                        GameObject pushedObject = pushedObjects[j];
                        MovementControllerScript hitMCS = pushedObject.GetComponent<MovementControllerScript>();
                        distancesMoved[j] = hitMCS.Move(new Vector2(0, amountLeftToMove.y), sourcePusherLevel, depthCount).y;

                        if (distancesMoved[j] < minDistanceMoved)
                        {
                            minDistanceMoved = distancesMoved[j];
                        }
                    }
                    
                    for (int j = 0; j < pushedObjects.Count; j++)
                    {
                        GameObject pushedObject = pushedObjects[j];
                        if (distancesMoved[j] > minDistanceMoved)
                        {
                            MovementControllerScript mcs = pushedObject.GetComponent<MovementControllerScript>();
                            if (mcs != null)
                            {
                                float diff = distancesMoved[j] - minDistanceMoved;
                                mcs.Move(new Vector2(0, diff * direction * -1));
                            }
                        }
                    }
                    
                    List<GameObject> carryObjects = new List<GameObject>();

                    if (direction < 0)
                    {
                        currentCarryHits = VerticalRaycastHits(MIN_DISTANCE);
                        foreach (RaycastHit2D hit in currentCarryHits)
                        {
                            if (hit.transform != null)
                            {
                                if (!carryObjects.Contains(hit.transform.gameObject))
                                {
                                    carryObjects.Add(hit.transform.gameObject);
                                }
                            }
                        }
                    }

                    amountMoved.y = CalculateMoveY(amountLeftToMove);
                    TranslateAndRecord(new Vector2(0, amountMoved.y));
                    if (direction < 0)
                    {
                        foreach (GameObject carryObject in carryObjects)
                        {
                            MovementControllerScript mcs = carryObject.GetComponent<MovementControllerScript>();
                            PhysicsScript physicsScript = carryObject.GetComponent<PhysicsScript>();
                            //TODO: this is not the right way to prevent things from simultaneously pulling objects down and being pushed by them
                            if (physicsScript != null)
                            {
                                if (mcs.pushableLevel.y <= pusherLevel.y)
                                {
                                    mcs.Move(new Vector2(0, amountMoved.y));
                                }                                
                            }
                        }
                    }
                }
                amountLeftToMove -= amountMoved;
                totalAmountMoved += amountMoved;
            }
            return totalAmountMoved;
        }

    }

    public  enum Axis
    {
        X,
        Y
    };

    public float CalculateMove(Vector2 delta, Axis axis)
    {
        if (axis == Axis.X)
        {
            return CalculateMoveX(delta);
        }
        if (axis == Axis.Y)
        {
            return CalculateMoveY(delta);
        }
        else
        {
            Debug.Log("INVALID AXIS");
            return 0; 
        }
    }

    public float CalculateMoveWithGameObjects(Vector2 delta, Axis axis, List<GameObject> attachedObjects)
    {
        if (axis == Axis.X)
        {
            return CalculateMoveWithGameObjectsX(delta, attachedObjects);
        }else
        {
           if (axis == Axis.Y)
            {
                return CalculateMoveWithGameObjectsY(delta, attachedObjects);
            }
            else
            {
                return 0;
            }
        }
    }

    public float CalculateMoveWithGameObjectsX(Vector2 delta, List<GameObject> attachedObjects)
    {
        float distance = CalculateMoveX(delta, attachedObjects);

        List<GameObject> objectsToIgnore = new List<GameObject>();
        objectsToIgnore.Add(gameObject);

        foreach (GameObject go in attachedObjects)
        {
            MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
            if (mcs != null)
            {

                float testDistance = mcs.CalculateMoveX(delta, objectsToIgnore);
                if (Mathf.Abs(testDistance) < Mathf.Abs(distance))
                {
                    distance = testDistance;
                }
            }
        }
        return distance*Mathf.Sign(delta.x);
    }

    public float CalculateMoveWithGameObjectsY(Vector2 delta, List<GameObject> attachedObjects)
    {
        float distance = CalculateMoveY(delta, attachedObjects);

        List<GameObject> objectsToIgnore = new List<GameObject>();
        objectsToIgnore.Add(gameObject);

        foreach (GameObject go in attachedObjects)
        {
            MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
            if (mcs != null)
            {

                float testDistance = mcs.CalculateMoveY(delta, objectsToIgnore);
                if (Mathf.Abs(testDistance) < Mathf.Abs(distance))
                {
                    distance = testDistance;
                }
            }
        }
        return distance * Mathf.Sign(delta.y);
    }



    public float CalculateMoveX(Vector2 delta)
    {
        return CalculateMoveX(delta, new List<GameObject>());
    }

    public float CalculateMoveY(Vector2 delta)
    {
        return CalculateMoveY(delta, new List<GameObject>());
    }

    public float CalculateMoveY(Vector2 delta, List<GameObject> objectsToIgnore)
    {
        float direction = Mathf.Sign(delta.y);
        List<RaycastHit2D> hits = VerticalRaycastHits(delta.y);
        if (direction < 0)
        {
            collisionState.bottomHits = hits;
        }
        else
        {
            collisionState.topHits = hits;
        }
        float distance = (Mathf.Abs(delta.y));
        if (hits != null)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform != null)
                {
                    if (!objectsToIgnore.Contains(hit.transform.gameObject))
                    {
                        if (hit.distance < distance)
                        {
                            distance = hit.distance;
                        }
                    }
                }
            }
        }
        return distance * direction;
    }

    public Vector2 MoveWithGameObjects(Vector2 delta, List<GameObject> attachedObjects)
    {
        Vector2 newDelta = CalculateMoveWithGameObjects(delta, attachedObjects);
        TranslateAndRecord(newDelta);
        foreach (GameObject go in attachedObjects)
        {
            go.GetComponent<MovementControllerScript>().TranslateAndRecord(newDelta);
        }
        return newDelta;
    }

    public float CalculateMoveX(Vector2 delta, List<GameObject> objectsToIgnore)
    {
        float direction = Mathf.Sign(delta.x);
        List<RaycastHit2D> hits = HorizontalRaycastHits(delta.x);
        if (direction < 0)
        {
            collisionState.leftHits = hits;
        }
        else
        {
            collisionState.rightHits = hits;
        }
        float distance = Mathf.Abs(delta.x);
        float startingDistance = distance;

        GameObject thingHit = null;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform != null)
            {
                if (!objectsToIgnore.Contains(hit.transform.gameObject))
                {

                    if (hit.distance < distance)
                    {
                        distance = hit.distance;
                        thingHit = hit.transform.gameObject;
                    }
                }
            }
        }
        Vector2 amountMoved = new Vector2();
        amountMoved.x += (distance * direction);
        return amountMoved.x;
    }


    public Vector2 CalculateMoveWithGameObjects(Vector2 delta, List<GameObject> attachedObjects)
    {
        List<GameObject> tmpList = new List<GameObject>(gameObjectsToIgnoreCollisonsWith);
        gameObjectsToIgnoreCollisonsWith.AddRange(attachedObjects);
        Vector2 newDelta = new Vector2(delta.x, delta.y);
        if (delta.x != 0)
        {
            newDelta.x = CalculateMoveX(delta);
        }
        if (delta.y != 0)
        {
            newDelta.y = CalculateMoveY(delta);
        }

        foreach (GameObject go in attachedObjects)
        {
            MovementControllerScript mcs = go.GetComponent<MovementControllerScript>();
            List<GameObject> tmpList2 = new List<GameObject>(mcs.gameObjectsToIgnoreCollisonsWith);
            mcs.gameObjectsToIgnoreCollisonsWith.Add(gameObject);
            mcs.gameObjectsToIgnoreCollisonsWith.AddRange(attachedObjects);

            Vector2 objectDelta = new Vector2(0, 0);
            if (delta.x != 0)
            {
                objectDelta.x = mcs.CalculateMoveX(delta);
            }
            if (delta.y != 0)
            {
                objectDelta.y = mcs.CalculateMoveY(delta);
            }

            if (Mathf.Abs(objectDelta.x) < Mathf.Abs(newDelta.x))
            {
                newDelta.x = objectDelta.x;
            }
            if (Mathf.Abs(objectDelta.y) < Mathf.Abs(newDelta.y))
            {
                newDelta.y = objectDelta.y;
            }
            mcs.gameObjectsToIgnoreCollisonsWith = tmpList2;
        }

        gameObjectsToIgnoreCollisonsWith = tmpList;
        return newDelta;
    }

    public void TranslateAndRecord(Vector2 delta)
    {
        amountMovedThisFrame += delta;
        transform.Translate(delta);
    }

    public static List<GameObject> SortGameObjectsByPositionX(List<GameObject> objects)
    {
        return objects.OrderBy(go => go.transform.position.x).ToList<GameObject>();
    }
}
