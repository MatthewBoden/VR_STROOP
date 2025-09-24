using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisMovement : MonoBehaviour
{
    float speed = 1.0f;
    Vector3 direction = Vector3.forward;

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection;
    }
}
