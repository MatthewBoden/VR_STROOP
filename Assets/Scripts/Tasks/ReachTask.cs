using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
/// <summary>
/// Reach task
/// </summary>
public class ReachTask : BaseTask
{
    const float DOCK_DIST = 0.025f;

    public ReachTask()
    {
        taskType = "reach_to_target";
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //reach to dock
            case 0:
                if (Vector3.Distance(cursor.transform.position,dock.transform.position) <= 0.01f)
                {
                    dock.SetActive(false);
                    home.SetActive(true);
                    IncrementStep();
                }

                break;
            //reach to home
            case 1:
                if (Vector3.Distance(cursor.transform.position, home.transform.position) <= 0.01f)
                {
                    home.SetActive(false);
                    target.SetActive(true);
                    IncrementStep();
                }
                break;
            //reach to target
            case 2:
                if (Vector3.Distance(cursor.transform.position, target.transform.position) <= 0.01f)
                {
                    target.SetActive(false);
                    IncrementStep();
                }
                break;
        }
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;

        session.CurrentTrial.result["type"] = session.CurrentBlock.settings.GetString("task");
        session.CurrentTrial.result["home_pos"] = home.transform.position;
        session.CurrentTrial.result["target_angle"] = session.CurrentBlock.settings.GetFloatList("target_angle")[session.currentTrialNum-1];
        session.CurrentTrial.result["target_size_m"] = target.transform.localScale.x;
        session.CurrentTrial.result["cursor_pos"] = cursor.transform.position;
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);

        //set up dock position and hide it
        dock.transform.position = taskPrefab.transform.position - ExperimentController.Instance.transform.forward * DOCK_DIST;
        dock.SetActive(false);

        //set up home position and hide it
        home.transform.position = taskPrefab.transform.position;
        home.SetActive(false);

        //set up target position and hide it
        target.transform.position = taskPrefab.transform.position;
        target.SetActive(false);
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        //if the target angles have not been set yet
        if (targetAngles.Count == 0)
        {
            targetAngles = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }

        Debug.Log("target angle: " + targetAngles[currentTrial]);

        target.transform.position = Vector3.zero;

        target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        target.transform.Translate(new Vector3(0.0f, 0.0f, 0.075f));
        dock.SetActive(true);
    }

    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    
}
