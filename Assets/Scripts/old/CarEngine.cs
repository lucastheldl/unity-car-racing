using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    Rigidbody2D rb;
    public Transform path;
    public float maxSteeringAngle = 90f;

    private List<Transform> Nodes = new List<Transform>();
    private int currentNode = 0;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        path = FindObjectOfType<Path>().GetComponent<Transform>();

        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        Nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                Nodes.Add(pathTransforms[i]);
            }
        }
    }
    private void FixedUpdate()
    {
        ApplySteering();
        CheckWaypointDistance();
    }
    private void ApplySteering(){

       // Vector3 relativeVector = transform.InverseTransformPoint(Nodes[currentNode].position);
       // relativeVector = relativeVector / relativeVector.magnitude;
        //float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteeringAngle;
        //transform.Rotate(transform.rotation.x, transform.rotation.y, -newSteer);
        transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.Euler(Nodes[currentNode].position.x, Nodes[currentNode].position.y, Nodes[currentNode].position.z), 4f * Time.deltaTime);

        rb.AddForce(transform.up * speed);
    }
    private void CheckWaypointDistance()
    {
        if (Vector3.Distance(transform.position, Nodes[currentNode].position) < 1f)
        {
            if (currentNode == Nodes.Count -1) {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
}
