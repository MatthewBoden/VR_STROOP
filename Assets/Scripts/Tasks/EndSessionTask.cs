using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public class EndSessionTask : BaseTask
{
    TMP_Text endText;

    public EndSessionTask()
    {
        taskType = "endsessions";
    }

    private void Awake()
    {
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            IncrementStep();
            TaskEnd();
            enabled = false;
        }
    }

    public override void LogParameters()
    {

    }

    public override void SetUp()
    {
        //base.SetUp();
        maxSteps = 1;

        endText = GameObject.Find("EndText").GetComponent<TMP_Text>();
        endText.text = "You have completed all trials\n[Press anything to finish]";
        Debug.Log("THE EXPERIMENT HAS BEEN COMPLETE");
        TaskEnd();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
    }

    public override void TaskEnd()
    {
        //base.TaskEnd();
        if (Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }
}
