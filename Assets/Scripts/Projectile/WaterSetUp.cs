using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSetUp : MonoBehaviour
{
    public Transform spawnPoint; // The transform where debris spawns
    public Transform destroyPoint; // The transform where debris is destroyed
    public float riverWidth = 10f; // The width of the river
    // Start is called before the first frame update
    void Start()
    {
        SetupRiverPlane();
    }

    private void SetupRiverPlane()
    {
        // Calculate the midpoint between the spawn and destroy points
        Vector3 midpoint = (spawnPoint.position + destroyPoint.position) / 2;
        midpoint = new Vector3(midpoint.x, -2.0f, midpoint.z);

        // Calculate the distance between spawn and destroy points
        float riverLength = Vector3.Distance(spawnPoint.position, destroyPoint.position);

        // Set the position of the plane to the midpoint
        transform.position = midpoint;

        // Adjust the plane's scale to match the river length and width
        transform.localScale = new Vector3(riverWidth, 5, riverLength);

        // Ensure the plane is oriented correctly
        transform.rotation = Quaternion.LookRotation(destroyPoint.position - spawnPoint.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        SetupRiverPlane();
    }
}
