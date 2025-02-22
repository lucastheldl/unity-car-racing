using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    private List<Transform> Nodes = new List<Transform>();

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.white;

        Transform[] pathTransforms = GetComponentsInChildren<Transform>();
        Nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != this.transform)
            {
                Nodes.Add(pathTransforms[i]);
            }
        }
        for (int i = 0; i < Nodes.Count; i++)
        {
            Vector3 currentNodePos = Nodes[i].position;
            Vector3 previousNodePos = Vector3.zero;
            if (i > 0) {
                 previousNodePos = Nodes[i - 1].position;
            }
            else if(i == 0 && Nodes.Count > 1)
            {
                previousNodePos = Nodes[Nodes.Count -1].position;
            }
            Gizmos.DrawLine(previousNodePos, currentNodePos);
            Gizmos.DrawWireSphere(currentNodePos,0.3f);
        }
       
    }
}
