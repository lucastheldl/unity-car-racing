using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CarAiHandler : MonoBehaviour
{
    public enum AIMode { followPlayer, followWaypoints, followMouse };

    [Header("AI settings")]
    public AIMode aiMode;
    public float maxSpeed = 16;
    public bool isAvoidingCars = true;
    [Range(0.0f, 1.0f)]
    public float skillLevel = 1.0f;

    bool hasLap = false;
    int laps = 0;
    public int carId;

    //Local variables
    Vector3 targetPosition = Vector3.zero;
    Transform targetTransform = null;
    public float orignalMaximumSpeed = 0;

    //Avoidance
    Vector2 avoidanceVectorLerped = Vector3.zero;

    //Waypoints
    WaypointNode currentWaypoint = null;
    WaypointNode previousWaypoint = null;
    WaypointNode[] allWayPoints;

    //Colliders
    CapsuleCollider2D polygonCollider2D;

    //Components
    CarController topDownCarController;
    GameMaster gm;

    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        gm = FindObjectOfType<GameMaster>();
        topDownCarController = GetComponent<CarController>();
        allWayPoints = FindObjectsOfType<WaypointNode>();

        //polygonCollider2D = GetComponentInChildren<PolygonCollider2D>();
        polygonCollider2D = GetComponent<CapsuleCollider2D>();

        orignalMaximumSpeed = maxSpeed;

    }

    // Start is called before the first frame update
    void Start()
    {
        SetMaxSpeedBasedOnSkillLevel(maxSpeed);
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(this.gameObject);
        }
    }
    // Update is called once per frame and is frame dependent
    void FixedUpdate()
    {
        Vector2 inputVector = Vector2.zero;

        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;

            case AIMode.followWaypoints:
                FollowWaypoints();
                break;

            case AIMode.followMouse:
                FollowMousePosition();
                break;
        }
        if (gm.raceStart) {
            inputVector.x = TurnTowardTarget();
            inputVector.y = ApplyThrottleOrBrake(inputVector.x);
        }

        //Send the input to the car controller.
        topDownCarController.SetInputVector(inputVector);
    }

    //AI follows player
    void FollowPlayer()
    {
        if (targetTransform == null)
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (targetTransform != null)
            targetPosition = targetTransform.position;
    }

    //AI follows waypoints
    void FollowWaypoints()
    {
        //Pick the cloesest waypoint if we don't have a waypoint set.
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestWayPoint();
            previousWaypoint = currentWaypoint;
        }

        //Set the target on the waypoints position
        if (currentWaypoint != null)
        {
            //Set the target position of for the AI. 
            targetPosition = currentWaypoint.transform.position;

            //Store how close we are to the target
            float distanceToWayPoint = (targetPosition - transform.position).magnitude;

            //Check if we are close enough to consider that we have reached the waypoint
            if (distanceToWayPoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                if (currentWaypoint.maxSpeed > 0)
                    SetMaxSpeedBasedOnSkillLevel(currentWaypoint.maxSpeed);
                else SetMaxSpeedBasedOnSkillLevel(1000);

                //Store the current waypoint as previous before we assign a new current one.
                previousWaypoint = currentWaypoint;

                //If we are close enough then follow to the next waypoint, if there are multiple waypoints then pick one at random.
                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
            }
        }
    }

    //AI follows the mouse position
    void FollowMousePosition()
    {
        //Take the mouse position in screen space and convert it to world space
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Set the target position of for the AI. 
        targetPosition = worldPosition;
    }

    //Find the cloest Waypoint to the AI
    WaypointNode FindClosestWayPoint()
    {
        /*
        return allWayPoints
            .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
            .FirstOrDefault();*/
         return GameObject.Find("Node").GetComponent<WaypointNode>();
    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        //Apply avoidance to steering
        if (isAvoidingCars && !topDownCarController.IsJumping())
            AvoidCars(vectorToTarget, out vectorToTarget);

        //Calculate an angle towards the target 
        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        //We want the car to turn as much as possible if the angle is greater than 45 degrees and we wan't it to smooth out so if the angle is small we want the AI to make smaller corrections. 
        float steerAmount = angleToTarget / 45.0f;

        //Clamp steering to between -1 and 1.
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);

        return steerAmount;
    }

    float ApplyThrottleOrBrake(float inputX)
    {
        //If we are going too fast then do not accelerate further. 
        if (topDownCarController.GetVelocityMagnitude() > maxSpeed)
            return 0;

        //Apply throttle forward based on how much the car wants to turn. If it's a sharp turn this will cause the car to apply less speed forward. We store this as reduceSpeedDueToCornering so we can use it togehter with the skill level
        float reduceSpeedDueToCornering = Mathf.Abs(inputX) / 1.0f;

        //Apply throttle based on cornering and skill.
        return 1.05f - reduceSpeedDueToCornering * skillLevel;
    }

    void SetMaxSpeedBasedOnSkillLevel(float newSpeed)
    {
        maxSpeed = Mathf.Clamp(newSpeed, 0, orignalMaximumSpeed);

        float skillbasedMaxiumSpeed = Mathf.Clamp(skillLevel, 0.3f, 1.0f);
        maxSpeed = maxSpeed * skillbasedMaxiumSpeed;
    }


    //Finds the nearest point on a line. 
    Vector2 FindNearestPointOnLine(Vector2 lineStartPosition, Vector2 lineEndPosition, Vector2 point)
    {
        //Get heading as a vector
        Vector2 lineHeadingVector = (lineEndPosition - lineStartPosition);

        //Store the max distance
        float maxDistance = lineHeadingVector.magnitude;
        lineHeadingVector.Normalize();

        //Do projection from the start position to the point
        Vector2 lineVectorStartToPoint = point - lineStartPosition;
        float dotProduct = Vector2.Dot(lineVectorStartToPoint, lineHeadingVector);

        //Clamp the dot product to maxDistance
        dotProduct = Mathf.Clamp(dotProduct, 0f, maxDistance);

        return lineStartPosition + lineHeadingVector * dotProduct;
    }

    //Checks for cars ahead of the car.
    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        //Disable the cars own collider to avoid having the AI car detect itself. 
        polygonCollider2D.enabled = false;

        //Perform the circle cast in front of the car with a slight offset forward and only in the Car layer
        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 0.9f, transform.up, 12, 1 << LayerMask.NameToLayer("Car"));

        //Enable the colliders again so the car can collide and other cars can detect it.  
        polygonCollider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            //Draw a red line showing how long the detection is, make it red since we have detected another car
            Debug.DrawRay(transform.position, transform.up * 12, Color.red);

            position = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;
            return true;
        }
        else
        {
            //We didn't detect any other car so draw black line with the distance that we use to check for other cars. 
            Debug.DrawRay(transform.position, transform.up * 12, Color.black);
        }

        //No car was detected but we still need assign out values so lets just return zero. 
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;

        return false;
    }

    void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            //Calculate the reflecing vector if we would hit the other car. 
            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

            float distanceToTarget = (targetPosition - transform.position).magnitude;

            //We want to be able to control how much desire the AI has to drive towards the waypoint vs avoiding the other cars. 
            //As we get closer to the waypoint the desire to reach the waypoint increases.
            float driveToTargetInfluence = 6.0f / distanceToTarget;

            //Ensure that we limit the value to between 30% and 100% as we always want the AI to desire to reach the waypoint.  
            driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.30f, 1.0f);

            //The desire to avoid the car is simply the inverse to reach the waypoint
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;

            //Reduce jittering a little bit by using a lerp
            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * 4);

            //Calculate a new vector to the target based on the avoidance vector and the desire to reach the waypoint
            newVectorToTarget = (vectorToTarget * driveToTargetInfluence + avoidanceVector * avoidanceInfluence);
            newVectorToTarget.Normalize();

            //Draw the vector which indicates the avoidance vector in green
            Debug.DrawRay(transform.position, avoidanceVector * 10, Color.green);

            //Draw the vector that the car will actually take in yellow. 
            Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.yellow);

            //we are done so we can return now. 
            return;
        }

        //We need assign a default value if we didn't hit any cars before we exit the function. 
        newVectorToTarget = vectorToTarget;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("chegada") && hasLap)
        {
            hasLap = false;
            if(gm.toLap == true)
            {
                gm.currentLaps++;
                gm.toLap = false;
            }
            if (laps < gm.maxLaps) {
                laps++;
                
            }
            if (laps == gm.maxLaps) {
                gm.finishedCars.Add(this.gameObject);
                GameObject car = Instantiate(Resources.Load<GameObject>(carId.ToString()), new Vector2(gm.finishedCars.Count -5f, 0f), Quaternion.identity);
                car.GetComponent<CarAiHandler>().enabled = false;
            }

        }
        else if (collision.CompareTag("lap"))
        {
            hasLap = true;
            if(gm.toLap == false)
            {
                gm.toLap = true;
            }
        }
    }
}
