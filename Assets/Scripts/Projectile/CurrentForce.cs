using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentForce : MonoBehaviour
{
    public float forwardForce;
    public float sideForce;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // void OnTriggerStay(Collider other)
    // {
    //     if (other.gameObject.tag == "Tool"){
    //         other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward*forwardForce*1);
    //         other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.left*sideForce*1);
            

    //     }
    // }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Tool")
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Debug.Log("current force: " + rb.velocity.x);
                // Set constant forward velocity
                // Vector3 velocity = rb.velocity;
                // velocity.z = forwardForce;
                // rb.velocity = velocity;

                // Apply side force
                rb.AddForce(Vector3.left * sideForce);
            }
        }
    }
}
