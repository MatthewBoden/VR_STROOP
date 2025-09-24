using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UXF;

public class SlingshotTask : BaseTask
{
    Slingshot slingshot;
    Target targetScript;

    //velocity mag to consider the shot ball has stopped
    const float BALL_LOW_VEL_THRES = 0.1f;

    public SlingshotTask()
    {
        taskType = "slingshot";
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
            //grab the slingshot
            case 0:
                if (slingshot.IsGrabbed && slingshot.IsAwayFromHome)
                    IncrementStep();
                break;
            //pull back the slingshot and release
            case 1:
                if (!slingshot.IsLoaded)
                    IncrementStep();
                break;
            //track ball movement
            case 2:
                if (slingshot.SlingFired)
                {
                    //float ballFromSlingDist = Vector3.Distance(slingshot.ShotBall.transform.position, target.transform.position);
                    //Debug.Log(ballFromSlingDist);
                    if (slingshot.ShotBall.GetComponent<Rigidbody>().velocity.magnitude <= BALL_LOW_VEL_THRES)
                    {
                        slingshot.ReloadSlingshot();
                        IncrementStep();
                    }
                }
                break;
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        slingshot = GameObject.Find("Slingshot").GetComponent<Slingshot>();
        CursorController.Instance.planeOffset = new Vector3(0.0f, -slingshot.transform.position.y, 0.0f);

        if (target)
        {
            targetScript = target.GetComponent<Target>();
        }
        else
        {
            Debug.LogWarning("NO TARGET FOUND");
        }
    }

    public override void TaskBegin()
    {
        base.TaskBegin();

        // Debug.Log("Current trial in block: " + expController.Session.CurrentTrial.numberInBlock);
        // Debug.Log("current block number: " + expController.Session.CurrentBlock.number);
        targetScript.ResetTarget();

        //if the target angles have not been set yet
        if (targetAngles.Count == 0)
        {
            targetAngles = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("target_angle");
        }

        Debug.Log("target angle: " + targetAngles[currentTrial]);

        // Debug.Log("target angle: " + targetAngles[currentTrial]);
        target.transform.rotation = Quaternion.Euler(0f, -targetAngles[currentTrial] + 90f, 0f);
        target.transform.Translate(new Vector3(0.0f, 0.0f, 3.0f));
    }


    public override void TaskEnd()
    {
        base.TaskEnd();
    }

    public override void LogParameters()
    {

    }
}
