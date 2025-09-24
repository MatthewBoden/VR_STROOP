using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour
{
    Collider[] colliders;
    const float GRAB_DISTANCE = 0.5f;
    bool isGrabbed = false;
    bool isGrabbing = false;
    bool isProximity = false;

    // Start is called before the first frame update
    void Start()
    {
        colliders = gameObject.GetComponents<Collider>();

        if(colliders.Length == 0)
        {
            Debug.LogWarning(name + " does not have any colliders");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.Instance.UseVR == true)
        {
            bool primaryButton;
            bool secondaryButton;

            if (InputHandler.Instance.GetDominantHandString() == "LeftHand")
            {
                primaryButton = Input.GetButton("XRI_Left_PrimaryButton");

                secondaryButton = Input.GetButton("XRI_Left_SecondaryButton");

            }
            else
            {
                primaryButton = Input.GetButton("XRI_Right_PrimaryButton");
                secondaryButton = Input.GetButton("XRI_Right_SecondaryButton");
            }

            isGrabbing = primaryButton || secondaryButton;
        }
        else
        {
            bool leftMouse = Input.GetButton("Fire1");
            bool rightMouse = Input.GetButton("Fire2");


            isGrabbing = leftMouse || rightMouse;
        }

        isGrabbed = isGrabbing && isProximity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Cursor")
        {
            isProximity = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Cursor")
        {
            isProximity = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cursor")
        {
            isProximity = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cursor")
        {
            isProximity = false;
        }
    }

    public bool IsGrabbed
    {
        get { return isGrabbed; }
    }

    public bool IsGrabbing
    {
        get { return isGrabbing; }
    }

    public bool IsProximity
    {
        get { return isProximity; }
    }

}
