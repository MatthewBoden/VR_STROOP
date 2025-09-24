using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controller for the experiment cursor
/// </summary>
public class CursorController : MonoBehaviour
{
    private static CursorController instance = null;
    public Vector3 planeOffset = Vector3.zero;
    GameObject cursor;
    MovementType moveType;
    
    /// <summary>
    /// The behaviour of the cursor
    /// </summary>
    public enum MovementType
    {
        aligned,
        rotated,
        clamped
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        moveType = MovementType.aligned;
    }

    // Update is called once per frame
    void Update()
    {

        //if not use vr and both a camera and a cursor object has been set
        if (ExperimentController.Instance.UseVR == false && Camera.main && cursor)
        {

            //cursor.transform.position = Camera.main.ScreenToWorldPoint(InputHandler.Instance.GetPosition());
            cursor.transform.position = Camera.main.ScreenToWorldPoint(InputHandler.Instance.GetRayPosition());
            if (ExperimentController.Instance.CurrentTask.Plane)
            {
                Plane plane = new Plane(ExperimentController.Instance.CurrentTask.Plane.transform.up, planeOffset.y);
                cursor.transform.position = InputHandler.Instance.GetRaycastPosition(plane);
            }
        }
        else if(ExperimentController.Instance.UseVR == true && Camera.main && cursor)
        {
            cursor.transform.position = InputHandler.Instance.GetSpatialPosition();
            cursor.transform.rotation = InputHandler.Instance.GetSpatialRotation();
        }


        if (cursor)
        {
            ConvertCursorPosition();
        }

    }

    private void LateUpdate()
    {
    }

    public static CursorController Instance
    {
        get 
        {
            if (!instance)
                Debug.LogError("ExperimentController is unitialized"); 
            return instance; 
        }
    }

    public GameObject Cursor
    {
        get { return cursor; }
        set { cursor = value; }
    }

    public Vector3 CursorPos
    {
        get
        {
            if (cursor)
            {
                return cursor.transform.position;
            }

            return Vector3.zero;
        }

        set
        {
            if (cursor)
                cursor.transform.position = value;
        }
    }

    public Quaternion CursorRot
    {
        get
        {
            if (cursor)
            {
                return cursor.transform.rotation;
            }

            return Quaternion.identity;
        }

        set
        {
            if (cursor)
                cursor.transform.rotation = value;
        }
    }

    public Vector3 ConvertCursorPosition()
    {
        GameObject target = ExperimentController.Instance.CurrentTask.Target;
        GameObject home = ExperimentController.Instance.CurrentTask.Home;

        Vector3 targetPos = target != null ? target.transform.position : Vector3.zero;
        Vector3 homePos = home != null ? home.transform.position : Vector3.zero;

        switch (moveType)
        {
            case MovementType.aligned:
                return cursor.transform.position;
            case MovementType.rotated:
                float rotation = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");
                cursor.transform.position = Quaternion.Euler(0, -rotation, 0) * (cursor.transform.position - homePos) + homePos;
                return cursor.transform.position;
            case MovementType.clamped:
                Vector3 normal = targetPos - homePos;

                // Rotate vector by 90 degrees to get plane parallel to the vector
                normal = Quaternion.Euler(0f, -90f, 0f) * normal;

                /*          ^  normal
                  target O  |
                          \ |
                           \|
                            \
                             \
                         home X
                 */

                cursor.transform.position = Vector3.ProjectOnPlane(cursor.transform.position - homePos, normal.normalized) + homePos;
                return cursor.transform.position;
            default:
                throw new System.ArgumentOutOfRangeException("No moveType defined for CursorController");
        }
    }
}
