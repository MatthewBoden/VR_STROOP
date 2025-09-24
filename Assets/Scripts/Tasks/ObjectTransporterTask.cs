using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UXF;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System.Linq;

public class ObjectTransporterTask : BaseTask
{
    [SerializeField]
    GameObject objectResetPlane;
    AudioSource audioSource;

    [SerializeField]
    List<Target> goals = new List<Target>();
    [SerializeField]
    Target resetPlane;
    [SerializeField]
    List<MeshFilter> goalMeshes = new List<MeshFilter>();

    List<string> goalMeshesVal = new List<string>();
    List<float> stepTime = new List<float>();
    /*
    [SerializeField]
    Target leftGoal;
    [SerializeField]
    Target rightGoal;
    [SerializeField]
    Target middleGoal;
    */
    [SerializeField]
    MeshFilter SquareGoalMesh;

    [SerializeField]
    MeshFilter SphereGoalMesh;

    [SerializeField]
    GameObject grabbedObject;
    [SerializeField]
    GameObject grabbedObjectVisable;

    [SerializeField]
    MeshFilter toolMesh;

    [SerializeField]
    GameObject toolPrefab;

    [SerializeField]
    GameObject leftHand;
    [SerializeField]
    GameObject leftHandCtrl;
    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject rightHandCtrl;

    [SerializeField]
    GameObject PrefabCamera;

    [SerializeField]
    GameObject MainCamera;

    [SerializeField]
    GameObject direct;

    [SerializeField]
    AudioClip correctSFX;
    [SerializeField]
    AudioClip incorrectSFX;
    [SerializeField]
    AudioClip buttonClickSFX;

    [SerializeField]
    GameObject Scoreboard;
    [SerializeField]
    TextMeshProUGUI ScoreTXT;
    [SerializeField]
    TextMeshProUGUI TrialTXT;
    [SerializeField]
    TextMeshProUGUI TimeTXT;


    private bool trial_active = false;
    private string tool_x = ""; // String to store all X positions instead of list to solve log parameter issues 
    private string tool_y = ""; 
    private string tool_z = ""; 



    float startTime = 0.0f;
    float endTime = 0.0f;

    int toolType = 0;
    int goalType = 0;
    static int totalScore = 0;

    bool hitTarget = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.Instance.UseVR)
        {
            if (resetPlane.TargetHit)
            {
                Debug.Log("Collided with reset plane");
                Destroy(grabbedObject);
                
                grabbedObject = Instantiate(toolPrefab, home.transform.position, Quaternion.identity);

                

                //SetupXR();
                grabbedObject.transform.position = home.transform.position;
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.transform.rotation = Quaternion.identity;

                grabbedObjectVisable.transform.position = home.transform.position;
                grabbedObjectVisable.transform.rotation = grabbedObject.transform.rotation;
                resetPlane.SetProjectile(grabbedObject);
                
                resetPlane.ResetTarget();
            }
        } else
        {
            Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
        }

        if (trial_active)
        {
            if (grabbedObject != null)
            {
                tool_x += grabbedObject.transform.position.x + ",";
                tool_y += grabbedObject.transform.position.y + ",";
                tool_z += grabbedObject.transform.position.z + ",";

                //Debug.Log(grabbedObject.transform.position.x + ", " + grabbedObject.transform.position.y + ", " + grabbedObject.transform.position.z);
            }

        }



        switch (currentStep)
        {
            //Check for initial grab, record time for start
            case 0:
                if (IsGrabbed())
                {
                    startTime = Time.time;
                    trial_active = true;
                    IncrementStep();
                    stepTime.Add(Time.time);
                }
                break;
            //Check for which goal hit
            case 1:
                {
                    if (!ExperimentController.Instance.UseVR)
                    {
                        //Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
                        if (grabbedObject.GetComponent<Tool>().IsGrabbed)
                        {
                            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                            grabbedObject.transform.position = new Vector3(cursor.transform.position.x, grabbedObject.transform.position.y, cursor.transform.position.z);
                        }
                        else
                        {
                            grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                        }
                    }

                    int targetIndex = 0;

                    //Check goals for collision
                    foreach (Target t in goals)
                    {
                        if (t.TargetHit)
                        {
                            //Check if correct
                            endTime = Time.time;
                            stepTime.Add(Time.time);
                            trial_active = false;

                            //Get types of goal and check if same as tool
                            string valString = goalMeshesVal[(ExperimentController.Instance.Session.currentTrialNum - 1) % 4];
                            char c = valString[targetIndex];
                            goalType = int.Parse(c.ToString());

                            //If not same target missed
                            if(goalType != toolType)
                            {
                                hitTarget = false;
                                audioSource.clip = incorrectSFX;
                                audioSource.Play();
                            }
                            //If same type target hit
                            else if(goalType == toolType)
                            {
                                hitTarget = true;
                                audioSource.clip = correctSFX;
                                audioSource.Play();
                                totalScore++;
                            }

                            dock.SetActive(true);
                            grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                            if (ExperimentController.Instance.UseVR)
                            {
                                grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
                                grabbedObject.GetComponent<XRBaseGrabTransformer>().enabled = false;
                            }
                            UpdateScoreboard(ExperimentController.Instance.Session.currentTrialNum, totalScore, endTime - startTime);
                            IncrementStep();
                            break;
                        }

                        targetIndex++;
                    }
                    
                    float rotation = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");
                    if (rotation != 0)
                    {
                        grabbedObjectVisable.transform.position = Quaternion.Euler(0, -rotation, 0) * (grabbedObject.transform.position - home.transform.position) + home.transform.position;
                        grabbedObjectVisable.transform.rotation = grabbedObject.transform.rotation;
                    }
                }
                break;
            //Return to dock
            case 2:
                {
                    Target dockTarget = dock.GetComponent<Target>();

                    if (dockTarget.IsColliding && dockTarget.TargetHit)
                    {
                        Debug.Log("Dock TargetHit: " + dockTarget.TargetHit);
                        Debug.Log("Dock IsColliding: " + dockTarget.IsColliding);
                        Destroy(grabbedObject);
                        grabbedObjectVisable.SetActive(false);
                        stepTime.Add(Time.time);
                        IncrementStep();
                        audioSource.clip = buttonClickSFX;
                        audioSource.Play();
                        StartCoroutine(PlayFeedback(0.5f));
                    }

                    break;
                }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 4;

        startTime = 0.0f;
        endTime = 0.0f;

        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        direct = GameObject.Find("RH Direct Interactor");

        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");

        MainCamera = GameObject.Find("Main Camera");

        CursorController.Instance.planeOffset = new Vector3(0.0f, plane.transform.position.y, 0.0f);

        if (goalMeshesVal.Count == 0)
        {
            goalMeshesVal = ExperimentController.Instance.Session.CurrentBlock.settings.GetStringList("target_location");
        }

        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start
        stepTime.Clear();
        toolType = 0;
        goalType = 0;

        if (grabbedObject == null)
        {
            grabbedObject = Instantiate(toolPrefab, home.transform.position, Quaternion.identity);
            
        }

        //SetupXR();
        grabbedObject.transform.position = home.transform.position;
        grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
        grabbedObject.transform.rotation = Quaternion.identity;

        grabbedObjectVisable.SetActive(true);
        grabbedObjectVisable.transform.position = home.transform.position;
        grabbedObjectVisable.transform.rotation = grabbedObject.transform.rotation;


        UpdateScoreboard(ExperimentController.Instance.Session.currentTrialNum, totalScore, endTime - startTime);

        foreach (Target t in goals)
        {
            t.ResetTarget();
        }
        resetPlane.ResetTarget();
        resetPlane.SetProjectile(grabbedObject);

        float rotation = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");

        //If not rotated
        if (rotation == 0)
        {
            foreach (Target t in goals)
            {
                t.SetProjectile(grabbedObject);
            }

            grabbedObjectVisable.SetActive(false);
        }

        //If rotated
        else
        {

            grabbedObjectVisable.SetActive(true);

            foreach (Target t in goals)
            {
                t.SetProjectile(grabbedObjectVisable);
            }

        }

        /*
        leftGoal.ResetTarget();
        middleGoal.ResetTarget();
        rightGoal.ResetTarget();
        */

        foreach (MeshFilter m in goalMeshes)
        {
            m.sharedMesh = null;
        }

        dock.GetComponent<Target>().ResetTarget();
        dock.SetActive(false);

        string valString = goalMeshesVal[(ExperimentController.Instance.Session.currentTrialNum  - 1) % 4];

        int counter = 0;
        foreach(char c in valString)
        {
            int v = int.Parse(c.ToString());

            if (v == 1)
            {
                goalMeshes[counter].GetComponent<MeshFilter>().sharedMesh = SquareGoalMesh.sharedMesh;
            }
            else if(v == 2)
            {
                goalMeshes[counter].GetComponent<MeshFilter>().sharedMesh = SphereGoalMesh.sharedMesh;
            }
            else if(v == 0)
            {
                goals[counter].SetProjectile(null);
            }

            counter++;
        }

        if(ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
        }

        //Setting mesh for grabbed object and visual object
        toolMesh = grabbedObject.GetComponent<MeshFilter>();
        string meshName = (string) ExperimentController.Instance.ExperimentLists["mesh"][ExperimentController.Instance.Session.currentBlockNum-1];
        switch (meshName) 
            {
            case "Cube":
                toolMesh.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                grabbedObjectVisable.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                grabbedObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                grabbedObjectVisable.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                toolType = 1;
                break;

            case "Sphere":
                toolMesh.sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                grabbedObjectVisable.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                grabbedObject.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
                grabbedObjectVisable.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
                toolType = 2;
                break;

        }

        
        //if (ExperimentController.Instance.UseVR) {
        //    rhCollider = rightHand.transform.GetChild(1).gameObject;
        //}


    }

    IEnumerator PlayFeedback(float endDelayTime = 0.0f)
    {
        float delayTime = 0.0f;

        while (delayTime <= endDelayTime)
        {
            delayTime += Time.deltaTime;
            yield return null;
        }

        IncrementStep();
        yield return new WaitForEndOfFrame();
    }

    void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            grabbedObject.GetComponent<Tool>().enabled = false;
            //Enable XRInteractableScript
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = true;
            cursor.SetActive(false);

            //Get which hand is being used
            //rightHand.SetActive(true);
            //rightHandCtrl.SetActive(false);
            //leftHand.SetActive(false);
            //leftHandCtrl.SetActive(false);

            objectResetPlane.SetActive(true);    // Comment out for now 
            //rightHand = InputHandler.Instance.GetDominantHandGameObject();
            rightHand = GameObject.Find("Right Hand");
            direct = GameObject.Find("RH Direct Interactor");
            dock.GetComponent<Target>().SetProjectile(direct);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

            // Centers player
            ExperimentController.Instance.CentreOVRPlayerHand();
        }
        else
        {
            grabbedObject.GetComponent<Tool>().enabled = true;
            //Disable XRInteractableScript
            grabbedObject.GetComponent<XRGrabInteractable>().enabled = false;
            grabbedObject.transform.GetChild(0).gameObject.SetActive(false);
            cursor.SetActive(true);
            dock.GetComponent<Target>().SetProjectile(cursor);


            objectResetPlane.SetActive(false);
            dock.GetComponent<Target>().SetProjectile(cursor);

            //Switch Camera to 2D
            PrefabCamera.SetActive(true);
            if (MainCamera != null)
            {
                MainCamera.SetActive(false);
            }
        }
    }

    bool IsGrabbed()
    {
        if (ExperimentController.Instance.UseVR)
        {
            return grabbedObject.GetComponent<XRGrabInteractable>().isSelected;
        }
        else
        {
            return grabbedObject.GetComponent<Tool>().IsGrabbed;
        }
    }

    public override void TaskEnd()
    {
        //clean up
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;


        if (ExperimentController.Instance.UseVR)
        {
            session.CurrentTrial.result["hand"] = "r";
            session.CurrentTrial.result["cursor_active"] = "N/A";
        }
        else
        {
            session.CurrentTrial.result["hand"] = "N/A";
            session.CurrentTrial.result["cursor_active"] = "True";
        }

        session.CurrentTrial.result["tool_x_coordinates"] = tool_x;
        session.CurrentTrial.result["tool_y_coordinates"] = tool_y;
        session.CurrentTrial.result["tool_z_coordinates"] = tool_z;



        session.CurrentTrial.result["correct_target"] = hitTarget;
        session.CurrentTrial.result["rotation"] = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloat("rotation");

        if (toolType == 1)
        {
            session.CurrentTrial.result["tool_type"] = "cube";
        }
        else
        {
            session.CurrentTrial.result["tool_type"] = "sphere";
        }
        
        if (goalType == 1)
        {
            session.CurrentTrial.result["goal_type"] = "cube";
        }
        else
        {
            session.CurrentTrial.result["goal_type"] = "sphere";
        }
        session.CurrentTrial.result["total_score"] = totalScore;
        session.CurrentTrial.result["start_grabbed_time"] = startTime;
        session.CurrentTrial.result["goal_hit_time"] = endTime;
        session.CurrentTrial.result["total_time"] = (endTime - startTime);




        for (int i = 0; i < stepTime.Count; i++)
        {
            session.CurrentTrial.result["step_" + i + "_time"] = stepTime[i];
        }
    }

    private void UpdateScoreboard(int trialNumber, int score, float totalTime)
    {
        int seconds = Mathf.FloorToInt(totalTime);
        int milliseconds = Mathf.FloorToInt((totalTime - seconds) * 1000);

        // Format the time as "Seconds:Milliseconds"
        string formattedTime = $"{seconds:00}.{milliseconds:000}";

        // Find child objects (requires proper hierarchy structure)
        TextMeshProUGUI scoreText = Scoreboard.transform.Find("ScoreTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trialText = Scoreboard.transform.Find("TrialTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeText = Scoreboard.transform.Find("TimeTXT").GetComponent<TextMeshProUGUI>();

        // Update the Text fields
        scoreText.text = $"Score: {score}";
        trialText.text = $"Trial: {trialNumber}";
        timeText.text = $"Time: {formattedTime}";
    }
}
