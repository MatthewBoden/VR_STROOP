using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DebrisDespawner : MonoBehaviour
{
    // private GameObject spawner;
    // void Start(){
    //     spawner = GameObject.FindWithTag("Spawner");
    // }
    
    // void Update(){
    //     transform.LookAt(spawner.transform.position, Vector3.up);
    // }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Debris"))
        {
            Destroy(other.gameObject);
        }
    }
}
