using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UXF;
/// <summary>
/// Handles everything regarding the experiment (ex. moving to next trial, calls to generate blocks, setting up and ending the experiment)
/// </summary>
public class ExperimentController : MonoBehaviour
{
    private static ExperimentController instance = null;
    private ExperimentGenerator expGenerator = null;
    private Session session;

    [SerializeField]
    private GameObject endSessionPrefab;

    /// <summary>
    ///Dictionary of the lists for the experiment defined in ExperimentGenerator
    /// </summary>
    Dictionary<string, List<object>> expLists = new Dictionary<string, List<object>>();
    /// <summary>
    /// List of the prefab names in Resources > Prefab to spawn
    /// </summary>
    public List<string> taskPrefabNames = new List<string>();
    /// <summary>
    /// The VR controller GameObject
    /// </summary>
    public GameObject vrCtlr;
    /// <summary>
    /// The prefab to spawn if using VR
    /// </summary>
    public GameObject vrPrefab;

    Vector3 prefabPosition = Vector3.zero;
    /// <summary>
    /// The active task
    /// </summary>
    private BaseTask currentTask;
    public BaseTask CurrentTask { get { return currentTask; } }
    //The total number of trials for the experiment
    int totalNumOfTrials = 0;
    //The total number of blocks
    int totalNumOfBlocks = 0;
    //Is the experiment using VR
    bool useVR = false;
    //Is the experiment running
    bool isRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
        {
            instance = this;

            expGenerator = new ExperimentGenerator();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // when the experiment is running
        // this is here because the experimentcontroller object exists before the
        // experiment starts and the parameters are made in UXF
        if (isRunning)
        {
            if (currentTask.Finished)
            {
                session.EndCurrentTrial();
            }


            if (Input.GetKeyDown(KeyCode.J))
            {
                CentreOVRPlayerHand();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                Session.EndCurrentTrial();
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Vector3 vec = vrCtlr.transform.position;
                vrCtlr.transform.position = new Vector3(vec.x, vec.y - 0.05f, vec.z);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector3 vec = vrCtlr.transform.position;
                vrCtlr.transform.position = new Vector3(vec.x, vec.y + 0.05f, vec.z);
            }
        }
    }

    public static ExperimentController Instance
    {
        get
        {
            if (!instance)
                Debug.LogError("ExperimentController is unitialized");
            
            return instance; 
        }
    }

    public int GetTotalTrials()
    {
        return totalNumOfTrials - (int) session.currentTrialNum +1;
    }

    public Session Session
    {
        get { return session; }
    }

    public ExperimentGenerator ExperimentGenerator
    {
        get { return expGenerator; }
    }


    public Dictionary<string, List<object>> ExperimentLists
    {
        get { return expLists; }
    }


    public int TotalNumOfTrials
    {
        get { return totalNumOfTrials; }
        set { totalNumOfTrials = value; }
    }

    public int TotalNumOfBlocks
    {
        get { return totalNumOfBlocks; }
        set { totalNumOfBlocks = value; }
    }

    public bool UseVR
    {
        get { return useVR; }
        set { useVR = value; }
    }
    /// <summary>
    /// UXF SessionBegin method
    /// </summary>
    public void SessionBegin(Session session)
    {
        this.session = session;

        expGenerator.GenerateBlocks(session);
        //tasks.Capacity = TotalNumOfTrials;
        taskPrefabNames.Capacity = TotalNumOfTrials;
        expGenerator.GenerateTasks();

        // if using VR
        if (useVR == true)
        {
            Debug.Log("The experiment is being run in VR");

            if (!vrCtlr)
            {
                vrCtlr = Instantiate(vrPrefab);
                vrCtlr.transform.position = Vector3.zero;
            }

            Camera.SetupCurrent(GameObject.Find("Main Camera").GetComponent<Camera>());
            //Debug.Log(Camera.main);
        }
        // if not using VR
        else
        {
            Debug.Log("The experiment is not being run in VR");

            //disable the VR controller
            vrCtlr.SetActive(false);
        }

        //find input devices
        InputHandler.Instance.FindDevices();
        string prefabName = session.settings.GetString("experiment_name") + "Prefab";
        GameObject prefabToLoad = Resources.Load<GameObject>("Prefabs/" + prefabName);
        
        if (prefabToLoad == null)
        {
            Debug.LogError($"Prefab not found: Prefabs/{prefabName}. Please check that the prefab exists in Assets/Resources/Prefabs/");
            return;
        }
        
        GameObject currentTaskPrefab = Instantiate(prefabToLoad);
        currentTaskPrefab.name = prefabName;
        currentTaskPrefab.transform.position = prefabPosition;

        currentTask = currentTaskPrefab.GetComponent<BaseTask>();
        currentTask.enabled = true;
        currentTask.TaskPrefab = currentTaskPrefab;
        isRunning = true;

        BeginNextTrial();
    }

    /// <summary>
    /// Center the OVRPlayerController to the dock location
    /// </summary>
    public void CenterOVRPlayerController()
    {
        if (currentTask.VRStartPos)
        {
            Debug.Log("Setting VR to " + currentTask.VRStartPos.transform.position);
            vrCtlr.transform.position = new Vector3(currentTask.VRStartPos.transform.position.x, currentTask.VRStartPos.transform.position.y, currentTask.VRStartPos.transform.position.z);
        }
        else
        {
            Debug.Log("Setting VR to " + InputHandler.Instance.GetSpatialPosition());
            vrCtlr.transform.position = InputHandler.Instance.GetSpatialPosition();
        }

        GameObject cameraOffset = GameObject.Find("Camera Offset");
        GameObject rotationAlignment = GameObject.Find("RotationAlignment");
        if (rotationAlignment != null && cameraOffset != null)
        {
            rotationAlignment.transform.position = new Vector3(rotationAlignment.transform.position.x, cameraOffset.transform.position.y, rotationAlignment.transform.position.z);
        
            // get difference between the camera offset rotation and the rotation alignment
            float angle = cameraOffset.transform.GetChild(0).transform.rotation.eulerAngles.y - rotationAlignment.transform.rotation.eulerAngles.y;
            // rotate the camera offset by the difference
            cameraOffset.transform.Rotate(0, -angle, 0);
        }
    }

    public void CentreOVRPlayerHand()
    {
        if(!useVR)
        {
            return;
        }

        Vector3 pos = new Vector3(0.0f, GameObject.Find("R_Wrist").transform.position.y, 0.0f);
        currentTask.gameObject.transform.position = pos;
        prefabPosition = pos;
    }

    /// <summary>
    /// Begin the next trial, if available. On the final trial this call will end the experiment.
    /// </summary>
    public void BeginNextTrial()
    {
        //for the first trial
        if (session.currentTrialNum == 0)
        {
            session.FirstTrial.Begin();
        }
        //every other trial that is not the first or last
        else if (session.currentTrialNum < totalNumOfTrials)
        {
            session.BeginNextTrialSafe();
        }
        //final trial
        else if (session.currentTrialNum == totalNumOfTrials) 
        {
            isRunning = false;
            session.End();
        }

        if (isRunning)
        {
            PlayerPrefs.SetInt("currentTrial", session.currentTrialNum - 1);
            PlayerPrefs.SetInt("currentBlock", session.CurrentBlock.number - 1);
            PlayerPrefs.SetInt("trialInBlock", session.CurrentTrial.numberInBlock - 1);
        }
    }
    /// <summary>
    /// UXF TrialBegin method
    /// </summary>
    public void TrialBegin()
    {
        //At the start of a block
        if (Session.CurrentTrial == Session.CurrentBlock.firstTrial)
        {
            //Set the input device to use
            string deviceName = (string)expLists["input_name"][session.currentBlockNum - 1];
            Debug.Log("Input device set as " + deviceName);
            InputHandler.Instance.UseThisDevice(deviceName);
        }

        //if the task hasn't been setup yet
        if (!currentTask.IsReady)
        {
            currentTask.enabled = true;
            currentTask.SetUp();
        }

        currentTask.TaskBegin();

        //If using VR and first trial, setup the VR position and rotation
        if (UseVR == true && Session.CurrentTrial == Session.FirstTrial)
        {
            CenterOVRPlayerController();

            //Offset the camera if needed
            if (currentTask.VRStartPos) {
                GameObject cameraOffset = vrCtlr.transform.GetChild(0).gameObject;
                GameObject vrCamera = cameraOffset.transform.GetChild(0).gameObject;

                Vector3 offsetVec = currentTask.VRStartPos.transform.position - vrCamera.transform.position;
                cameraOffset.transform.position += offsetVec;
            }
        }
    }
    /// <summary>
    /// UXF TrialEnd method
    /// </summary>
    public void TrialEnd()
    {
        //if this is the last trial of the block
        if (session.CurrentTrial == session.CurrentBlock.lastTrial)
        {
            //end current task and disable
            currentTask.TaskEnd();
            currentTask.enabled = false;

            //if there are more blocks to go through
            if (session.CurrentBlock.number < taskPrefabNames.Count)
            {
                string prefabName = session.settings.GetString("experiment_name") + "Prefab";
                GameObject prefabToLoad = Resources.Load<GameObject>("Prefabs/" + prefabName);
                
                if (prefabToLoad == null)
                {
                    Debug.LogError($"Prefab not found: Prefabs/{prefabName}. Please check that the prefab exists in Assets/Resources/Prefabs/");
                    return;
                }
                
                //move on to the next task and prepare it
                GameObject currentTaskPrefab = Instantiate(prefabToLoad);
                currentTaskPrefab.name = prefabName;
                currentTaskPrefab.transform.position = prefabPosition;

                currentTask = currentTaskPrefab.GetComponent<BaseTask>();
                currentTask.enabled = true;
                currentTask.TaskPrefab = currentTaskPrefab;
            }
        }
        if(session.isApplicationQuitting == false)
        {
            BeginNextTrial();
        }
    }
    /// <summary>
    /// UXF OnSessionEnd method
    /// </summary>
    public void OnSessionEnd()
    {
        //Application.Quit();
        Instantiate(endSessionPrefab);
    }

}
