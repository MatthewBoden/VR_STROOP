using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLayer : MonoBehaviour
{
    public int LayerOnEnter; // BallInHole
    public int LayerOnExit;  // BallOnTable
	
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Tool")
        {
            other.gameObject.layer = LayerOnEnter;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Tool")
        {
            other.gameObject.layer = LayerOnExit;
        }
    }
}
