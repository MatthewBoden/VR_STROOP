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
using System.Runtime.CompilerServices;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using UXF.UI;
using UnityEngine.UIElements;

public class BongoTask: BaseTask
{
    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    List<MultipleTarget> goals = new List<MultipleTarget>();

    [SerializeField]
    List<MeshFilter> goalMeshes = new List<MeshFilter>();
    [SerializeField]
    List<Material> targetMaterials = new List<Material>();
    [SerializeField]
    GameObject spawnParent;
    [SerializeField]
    List<GameObject> spawnLocations = new List<GameObject>();

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
    GameObject leftHand;
    [SerializeField]
    GameObject leftHandCtrl;
    [SerializeField]
    GameObject rightHand;
    [SerializeField]
    GameObject rightHandCtrl;
    [SerializeField]
    GameObject bongoTargetPrefab;
    [SerializeField]
    MultipleTarget targetOutOfBounds;

    List<GameObject> activeTargets = new List<GameObject>();
    Queue<GameObject> spawnedObjects = new Queue<GameObject>();

    GameObject leftOuterTarget;
    GameObject leftInnerTarget;
    GameObject rightInnerTarget;
    GameObject rightOuterTarget;

    List<string> jsonSpawnLocation = new List<string>();

    float targetSpeed = 0.0f;
    float targetSpawnDelay = 1.0f;

    [SerializeField]
    GameObject PrefabCamera;

    [SerializeField]
    GameObject MainCamera;

    [SerializeField]
    GameObject directRight;
    [SerializeField]
    GameObject directLeft;

    [SerializeField]
    AudioSource LO_BongoAudio;
    [SerializeField]
    AudioSource LI_BongoAudio;
    [SerializeField]
    AudioSource RI_BongoAudio;
    [SerializeField]
    AudioSource RO_BongoAudio;

    [SerializeField]
    AudioClip bongoHitSFX;
    [SerializeField]
    AudioClip buttonClickSFX;

    [SerializeField]
    GameObject Scoreboard;
    [SerializeField]
    TextMeshProUGUI ScoreTXT;
    [SerializeField]
    TextMeshProUGUI TrialTXT;

    List<string> hittingHand = new List<string>();
    List<int> scorePerHit = new List<int>();
    List<Vector3> leftHandPos = new List<Vector3>();
    List<Vector3> rightHandPos = new List<Vector3>();

    List<Vector3> noteOnHitPos = new List<Vector3>();

    float startTime = 0.0f;
    float endTime = 0.0f;

    string noteOrder = "";

    int toolType = 0;
    int goalType = 0;
    static int totalScore = 0;
    static float totalTargets = 0.0f;
    static float totalHit = 0.0f;
    static float totalPerfect = 0.0f;

    bool hitTarget = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStep)
        {
            //Check if dock is pressed to start trial
            case 0:
                {
                    if (dock.GetComponent<Target>().TargetHit && dock.GetComponent<Target>().IsColliding && 
                        (ExperimentController.Instance.UseVR == true ? true : Input.GetMouseButtonDown(0)))
                    {
                        startTime = Time.time;
                        stepTime.Add(Time.deltaTime);

                        dock.GetComponent<Target>().ResetTarget();
                        audioSource.clip = buttonClickSFX;
                        audioSource.Play();
                        dock.GetComponent<Target>().enabled = false;
                        dock.GetComponent<MeshCollider>().enabled = false;
                        dock.SetActive(false);

                        StartCoroutine(DelayedIncrementStep(0.5f));

                        SpawnTargets();

                        Debug.Log("Button is pressed");
                    }
                }
                break;
            //Start moving objects
            case 1:
                {
                    StartCoroutine(MoveTargets());
                    IncrementStep();
                }
                break;
            //Check if all targets are active and for button click
            case 2:
                {
                    //Track hand position
                    if (ExperimentController.Instance.UseVR)
                    {
                        leftHandPos.Add(directLeft.transform.position);
                        rightHandPos.Add(directLeft.transform.position);
                        //If above does not work try this
                        //XR Rig is weird and the VR hands transform does not change although the objects are moving in the scene
                        //leftHandPos.Add(InputHandler.Instance.GetHandPosition("LeftHand"));
                        //rightHandPos.Add(InputHandler.Instance.GetHandPosition("RightHand"));
                    }
                    else
                    {
                        leftHandPos.Add(Input.mousePosition);
                        rightHandPos.Add(Input.mousePosition);
                    }

                    //Move targets into negative Z-axis
                    foreach (GameObject g in activeTargets)
                    {
                        Vector3 pos = g.transform.position;
                        g.transform.position = new Vector3(pos.x, pos.y, pos.z -= (targetSpeed * Time.deltaTime));
                    }

                    //Check if bongo is pressed
                    foreach(MultipleTarget g in goals)
                    {
                        //If a bongo is hit and a target is colliding with the bongo
                        if(g.IsToolCollding && g.IsTargetCollding && 
                            (ExperimentController.Instance.UseVR == true ? true : Input.GetMouseButtonDown(0)))
                        {
                            if (ExperimentController.Instance.UseVR == true)
                            {
                                if (g.CollidingTool == directLeft)
                                {
                                    hittingHand.Add("l");
                                } 
                                else
                                {
                                    hittingHand.Add("r");
                                }
                            }

                            totalTargets++;

                            GameObject hitTarget = g.CollidingTarget;
                            CapsuleCollider capsul = g.GetComponent<CapsuleCollider>();
                            float radius = capsul.bounds.extents.z;
                            float dist = Vector3.Distance(g.transform.position, hitTarget.transform.position);

                            int goalIndex = goals.IndexOf(g);

                            noteOnHitPos[goalIndex] = g.transform.position;

                            //Play audio vfx
                            //Left outer
                            if(goalIndex == 0)
                            {
                                LO_BongoAudio.Play();
                            }
                            //Left inner
                            else if(goalIndex == 1)
                            {
                                LI_BongoAudio.Play();
                            }
                            //Right inner
                            else if(goalIndex == 2)
                            {
                                RI_BongoAudio.Play();
                            }
                            //Right outer
                            else if(goalIndex == 3)
                            {
                                RO_BongoAudio.Play();
                            }

                            //Play visual feedback
                            GameObject bongoHit = goalMeshes[goalIndex].gameObject;
                            Vector3 movePos = new Vector3(bongoHit.transform.localPosition.x, bongoHit.transform.localPosition.y - 0.05f, bongoHit.transform.localPosition.z);
                            StartCoroutine(LerpBongo(bongoHit, movePos, 20.0f, 0.0125f));

                            //Check if distance is less than half the bounds of the collider
                            //If true than it's a "perfect" hit
                            if (dist < radius * 0.5)
                            {
                                totalPerfect++;
                                totalHit++;
                                totalScore += 5;
                                scorePerHit.Add(5);
                            }
                            //If not than it's a "normal" hit
                            else
                            {
                                totalHit++;
                                totalScore++;
                                scorePerHit.Add(1);
                            }

                            //Update the scoreboard
                            UpdateScoreboard();

                            g.targets.Remove(hitTarget);
                            activeTargets.Remove(hitTarget);
                            Destroy(hitTarget);
                            g.ResetState();
                        }
                        //Bongo is hit but there is no target colliding with the bongo
                        else
                        {
                            //TODO add some form of other feedback
                        }
                    }

                    //If the target hits the out of bounds
                    if (targetOutOfBounds.IsTargetCollding)
                    {
                        hittingHand.Add(" ");

                        GameObject o = targetOutOfBounds.CollidingTarget;
                        noteOnHitPos.Add(o.transform.position);
                        targetOutOfBounds.targets.Remove(o);
                        activeTargets.Remove(o);
                        Destroy(o);
                        targetOutOfBounds.ResetState();

                        totalTargets++;
                        scorePerHit.Add(0);
                        UpdateScoreboard();
                    }

                    //If no more targets increment step
                    if(activeTargets.Count == 0 && spawnedObjects.Count == 0)
                    {
                        //dock.SetActive(true);
                        stepTime.Add(Time.deltaTime);
                        endTime = Time.time;
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

        startTime = 0.0f;
        endTime = 0.0f;

        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        directRight = GameObject.Find("RH Direct Interactor");
        directLeft = GameObject.Find("LH Direct Interactor");


        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");

        if (ExperimentController.Instance.UseVR)
        {
            dock.GetComponent<Target>().SetProjectile(directRight);

            foreach(MultipleTarget g in goals)
            {
                g.tools.Add(directRight);
                g.tools.Add(directLeft);
            }
        }
        else
        {
            dock.GetComponent<Target>().SetProjectile(cursor);
            CursorController.Instance.planeOffset = new Vector3(0.0f, -spawnParent.transform.position.y, 0.0f);

            foreach (MultipleTarget g in goals)
            {
                g.tools.Add(cursor);
            }
        }

        MainCamera = GameObject.Find("Main Camera");
        SetupXR();
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        //the task start
        stepTime.Clear();
        UpdateScoreboard();

        //List<int> currentBlockTrials = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("trials_in_block");
        int currentBlockNum = ExperimentController.Instance.Session.currentBlockNum - 1;
        //For some reason just speed was not getting the right list for some reason so we use per_block_speed
        targetSpeed = ExperimentController.Instance.Session.CurrentBlock.settings.GetFloatList("per_block_speed")[currentBlockNum];

        hittingHand.Clear();
        scorePerHit.Clear();
        leftHandPos.Clear();
        rightHandPos.Clear();
        noteOnHitPos.Clear();

        noteOnHitPos.Capacity = 4;
        for (int i = 0; i < noteOnHitPos.Capacity; i++)
        {
            noteOnHitPos.Add(Vector3.zero);
        }
        


        dock.SetActive(true);
        dock.GetComponent<Target>().enabled = true;
        dock.GetComponent<MeshCollider>().enabled = true;
        dock.GetComponent<Target>().ResetTarget();

        foreach (MultipleTarget t in goals)
        {
            t.ResetState();
        }

        targetOutOfBounds.ClearLists();
        
        foreach (MultipleTarget g in goals)
        {
            g.targets.Clear();
        }
        if (jsonSpawnLocation.Count == 0)
        {
            jsonSpawnLocation = ExperimentController.Instance.Session.CurrentBlock.settings.GetStringList("target_location");
        }

        if (!ExperimentController.Instance.UseVR) // scoreboard direction
        {
            Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
        }
    }

    IEnumerator DelayedIncrementStep(float endDelayTime = 0.0f)
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

    IEnumerator MoveTargets()
    {
        float delayTime = 0.0f;

        while (spawnedObjects.Count > 0)
        {
            while (delayTime <= targetSpawnDelay)
            {
                delayTime += Time.deltaTime;
                yield return null;
            }

            GameObject obj = spawnedObjects.Dequeue();
            obj.GetComponent<MeshRenderer>().enabled = true;
            activeTargets.Add(obj);
            delayTime = 0.0f;
        }

        yield return new WaitForEndOfFrame();
    }

    IEnumerator LerpBongo(GameObject toMove, Vector3 pos, float speed, float holdTime)
    {
        Vector3 orgPos = toMove.transform.localPosition;
        float delayTime = 0.0f;

        while(Vector3.Distance(toMove.transform.localPosition, pos) > 0.001f)
        {
            Vector3 direction = pos - toMove.transform.localPosition;
            toMove.transform.localPosition += direction * speed * Time.deltaTime;

            yield return null;
        }

        while(delayTime <= holdTime)
        {
            delayTime += Time.deltaTime;
            yield return null;
        }

        while (Vector3.Distance(toMove.transform.localPosition, orgPos) > 0.001f)
        {
            Vector3 direction = orgPos - toMove.transform.localPosition;
            toMove.transform.localPosition += direction * speed * Time.deltaTime;

            yield return null;
        }

        yield return new WaitForEndOfFrame();
    }


    private void SpawnTargets()
    {
        List<int> currentBlockTrials = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("trials_in_block");
        int currentBlockNum = ExperimentController.Instance.Session.currentBlockNum - 1;
        string currentBlockLoc = jsonSpawnLocation[(ExperimentController.Instance.Session.currentTrialNum - 1) % currentBlockTrials[currentBlockNum]];
        noteOrder = currentBlockLoc;

        if (currentBlockLoc.Length != goals.Count)
        {
            Debug.LogError("The number of locations in the JSON is not the same as the number of goals");
        }

        foreach (char c in currentBlockLoc)
        {
            int val = int.Parse(c.ToString());

            GameObject t = Instantiate(bongoTargetPrefab,gameObject.transform);
            t.name = "Bongo Target " + c;
            t.transform.position = spawnLocations[val - 1].transform.position;
            t.GetComponent<MeshRenderer>().material = targetMaterials[val - 1];
            t.GetComponent<MeshRenderer>().enabled = false;
            goals[val - 1].targets.Add(t);
            targetOutOfBounds.targets.Add(t);
            spawnedObjects.Enqueue(t);
        }
    }

    void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            
            //rightHand = InputHandler.Instance.GetDominantHandGameObject();
            rightHand = GameObject.Find("Right Hand");
            directRight = GameObject.Find("RH Direct Interactor");
            leftHand = GameObject.Find("Left Hand");
            directLeft = GameObject.Find("LH Direct Interactor");
            //dock.GetComponent<Target>().SetProjectile(direct);

            //Switch Camera to VR
            PrefabCamera.SetActive(false);

            cursor.SetActive(false);

            // Centers player
            //ExperimentController.Instance.CentreOVRPlayerHand();
        }
        else
        {
            
            cursor.SetActive(true);
            //dock.GetComponent<Target>().SetProjectile(cursor);

            //Switch Camera to 2D
            PrefabCamera.SetActive(true);
            if (MainCamera != null)
            {
                MainCamera.SetActive(false);
            }
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
            session.CurrentTrial.result["hand"] = string.Join(",", hittingHand.Select(i => string.Format($"{i}")));
            session.CurrentTrial.result["controller_type"] = "vr_controller";
            session.CurrentTrial.result["participant_spawn_location_x"] = vrPos.transform.position.x;
            session.CurrentTrial.result["participant_spawn_location_y"] = vrPos.transform.position.y;
            session.CurrentTrial.result["participant_spawn_location_z"] = vrPos.transform.position.z;
        }
        else
        {
            session.CurrentTrial.result["hand"] = "mouse";
            session.CurrentTrial.result["controller_type"] = "mouse";
        }

        string colourNotes = "";

        foreach (char n in noteOrder)
        {
            
            if (n == '1') // red
            {
                colourNotes+= "red";
            } 
            else if (n == '2') // blue
            {
                colourNotes += "blue";
            }
            else if (n == '3') // yellow
            {
                colourNotes += "yellow";
            }
            else if (n == '4') // purple
            {
                colourNotes += "purple";
            }

            if (!(n == noteOrder[noteOrder.Length-1]))
            {
                colourNotes += "_";
            }
        }
        session.CurrentTrial.result["note_order"] = colourNotes;

        string successPerHit = "";
        for (int n = 0; n < scorePerHit.Count; n++)
        {

            if (scorePerHit[n] == 0) // miss
            {
                successPerHit += "miss";
            }
            else if (scorePerHit[n] == 1) // miss
            {
                successPerHit += "ok";
            }
            else if(scorePerHit[n] == 5) // miss
            {
                successPerHit += "great";
            
            }


            if (n+1 < scorePerHit.Count)
            {
                successPerHit += "_";
            }
        }

        session.CurrentTrial.result["success_per_hit"] = successPerHit;
        session.CurrentTrial.result["score_per_hit"] = string.Join(",", scorePerHit.Select(i => string.Format($"{i}")));

        session.CurrentTrial.result["hit_percentage"] = totalHit / totalTargets;
        session.CurrentTrial.result["perfect_percentage"] = totalPerfect / totalTargets;

        session.CurrentTrial.result["speed"] = targetSpeed;
        session.CurrentTrial.result["total_score"] = totalScore;
        session.CurrentTrial.result["total_time"] = (endTime - startTime);

        for (int i = 0; i < stepTime.Count; i++)
        {
            session.CurrentTrial.result["step_" + i + "_time"] = stepTime[i];
        }

        //Hand position
        session.CurrentTrial.result["left_hand_pos_x"] = string.Join(",", leftHandPos.Select(i => string.Format($"{i.x}")));
        session.CurrentTrial.result["left_hand_pos_y"] = string.Join(",", leftHandPos.Select(i => string.Format($"{i.y}")));
        session.CurrentTrial.result["left_hand_pos_z"] = string.Join(",", leftHandPos.Select(i => string.Format($" {i.z}")));

        session.CurrentTrial.result["right_hand_pos_x"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.x}")));
        session.CurrentTrial.result["right_hand_pos_y"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.y}")));
        session.CurrentTrial.result["right_hand_pos_z"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.z}")));

        //Bongo positions
        session.CurrentTrial.result["bongo_red_pos_x"] = goalMeshes[0].gameObject.transform.position.x;
        //session.CurrentTrial.result["red_pos_y"] = goalMeshes[0].gameObject.transform.position.y;
        session.CurrentTrial.result["bongo_red_pos_z"] = goalMeshes[0].gameObject.transform.position.z;

        session.CurrentTrial.result["bongo_blue_pos_x"] = goalMeshes[1].gameObject.transform.position.x;
        //session.CurrentTrial.result["blue_pos_y"] = goalMeshes[1].gameObject.transform.position.y;
        session.CurrentTrial.result["bongo_blue_pos_z"] = goalMeshes[1].gameObject.transform.position.z;

        session.CurrentTrial.result["bongo_yellow_pos_x"] = goalMeshes[2].gameObject.transform.position.x;
        //session.CurrentTrial.result["yellow_pos_y"] = goalMeshes[2].gameObject.transform.position.y;
        session.CurrentTrial.result["bongo_yellow_pos_z"] = goalMeshes[2].gameObject.transform.position.z;

        session.CurrentTrial.result["bongo_purple_pos_x"] = goalMeshes[3].gameObject.transform.position.x;
        //session.CurrentTrial.result["purple_pos_y"] = goalMeshes[3].gameObject.transform.position.y;
        session.CurrentTrial.result["bongo_purple_pos_z"] = goalMeshes[3].gameObject.transform.position.z;


        // All Note positions
        session.CurrentTrial.result["note_red_pos_x"] = noteOnHitPos[0].x;
        session.CurrentTrial.result["note_red_pos_z"] = noteOnHitPos[0].z;

        session.CurrentTrial.result["note_blue_pos_x"] = noteOnHitPos[1].x;
        session.CurrentTrial.result["note_blue_pos_z"] = noteOnHitPos[1].z;

        session.CurrentTrial.result["note_yellow_pos_x"] = noteOnHitPos[2].x;
        session.CurrentTrial.result["note_yellow_pos_z"] = noteOnHitPos[2].z;

        session.CurrentTrial.result["note_purple_pos_x"] = noteOnHitPos[3].x;
        session.CurrentTrial.result["note_purple_pos_z"] = noteOnHitPos[3].z;
    }

    private void UpdateScoreboard()
    {

        // Find child objects (requires proper hierarchy structure)
        TextMeshProUGUI scoreText = Scoreboard.transform.Find("ScoreTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trialText = Scoreboard.transform.Find("TrialTXT").GetComponent<TextMeshProUGUI>();

        int hitPerc = totalTargets > 0 ? (int)((totalHit / totalTargets) * 100) : 0;
        int perfectPerc = totalTargets > 0 ? (int)((totalPerfect / totalTargets) * 100) : 0;

        // Update the Text fields
        scoreText.text = $"Score: {totalScore}";
        trialText.text = $"Trial: {ExperimentController.Instance.Session.currentTrialNum}\n" +
                         $"Hit % {hitPerc}\n" +
                         $"Perfect % {perfectPerc}";
    }
}
