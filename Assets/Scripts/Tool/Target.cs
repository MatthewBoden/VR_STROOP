using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    /// <summary>
    /// The projectile meant to hit the target
    /// </summary>
    [SerializeField]
    GameObject projectile;
    /// <summary>
    /// The collider for the target used to check for collision with the target
    /// </summary>
    Collider targetCollider;
    bool targetHit = false;
    bool colliding = false;

    // Start is called before the first frame update
    void Start()
    {
        targetCollider = GetComponent<Collider>();

        if (!targetCollider)
            Debug.LogWarning("TARGET COULD NOT FIND COLLIDER");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetProjectile(GameObject p)
    {
        projectile = p;
    }

    public void ResetTarget()
    {
        //transform.position = Vector3.zero;
        targetHit = false;
        colliding = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject == projectile)
        {
            //Debug.Log("Projectile collided with " + name);
            targetHit = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject == projectile)
        {
            //Debug.Log("Projectile collided with " + name);
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Projectile" || collision.gameObject == projectile)
        {
            colliding = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Projectile" || other.gameObject == projectile)
        {
            //Debug.Log("Projectile triggered " + name);
            targetHit = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Projectile" || other.gameObject == projectile)
        {
            //Debug.Log("Projectile collided with " + name);
            colliding = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Projectile" || other.gameObject == projectile)
        {
            colliding = false;
        }
    }

    public bool TargetHit
    {
        get { return targetHit; }
    }

    public bool IsColliding
    {
        get { return colliding; }
    }
}
