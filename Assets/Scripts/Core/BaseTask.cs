using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Abstract class to define a task, all task objects should inherit from this task
/// </summary>
public abstract class BaseTask : MonoBehaviour
{
    /// <summary>
    /// The home for the experiment
    /// </summary>
    [SerializeField]
    protected GameObject home;
    [SerializeField]
    protected GameObject vrPos;
    public GameObject Home { get { return home; } set { home = value; } }
    public GameObject VRStartPos { get { return vrPos; } }
    /// <summary>
    /// The docking position for the experiment
    /// </summary>
    [SerializeField]
    protected GameObject dock;
    public GameObject Dock { get { return dock; } set { dock = value; } }
    /// <summary>
    /// Scenes plane object
    /// </summary>
    [SerializeField]
    protected GameObject plane;
    public GameObject Plane { get { return plane; } set { plane = value; } }
    /// <summary>
    /// Experiment target object
    /// </summary>
    [SerializeField]
    protected GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    [SerializeField]
    protected GameObject cursor;
    public GameObject Cursor { get { return cursor; } set { cursor = value; } }
    public GameObject TaskPrefab { get { return taskPrefab; } set { taskPrefab = value; } }

    /// <summary>
    /// The camera for the prefab when not using VR
    /// </summary>
    [SerializeField]
    protected Camera prefabCamera;
    /// <summary>
    /// Prefab for the task
    /// </summary>
    protected GameObject taskPrefab;
    //the current trial num
    protected int currentTrial;
    //the total trials
    protected int totalTrials;
    //public string prefabName;

    /// <summary>
    /// Type of task, usually for logging
    /// </summary>
    protected string taskType;

    /// <summary>
    /// Angles for the target GameObject
    /// </summary>
    protected List<float> targetAngles = new List<float>();

    /// <summary>
    /// Current step for the task
    /// </summary>
    protected int currentStep = 0;
    /// <summary>
    /// Number of steps for the task to be finished
    /// </summary>
    protected int maxSteps;

    /// <summary>
    /// Have we reached the final step
    /// </summary>
    protected bool finished = false;
    /// <summary>
    /// Has the task been setup by calling SetUp()
    /// </summary>
    protected bool ready = false;

    protected ExperimentController expController;

    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// Increment the step and return if we have reached the final step
    /// </summary>
    public virtual bool IncrementStep()
    {
        currentStep++;

        finished = currentStep == maxSteps;

        if (finished)
        {
            currentTrial++;
            LogParameters();
        }

        //Debug.Log(name + "step: " + currentStep);

        return finished;
    }

    public int CurrentStep
    {
        get { return currentStep; }
        set { currentStep = value; }
    }

    public int MaxSteps
    {
        get { return maxSteps; }
        set { maxSteps = value; }
    }

    public bool Finished
    {
        get { return finished; }
        set { finished = value; }
    }

    public string TaskType
    {
        get { return taskType; }
        set { taskType = value; }
    }

    public bool IsReady
    {
        get { return ready; }
    }

    /// <summary>
    /// Set up the task and related assets
    /// </summary>
    public virtual void SetUp()
    {
        ready = true;
        finished = false;
        currentStep = 0;
        expController = ExperimentController.Instance;
        totalTrials = expController.Session.CurrentBlock.trials.Count;
        currentTrial = expController.Session.CurrentTrial.numberInBlock - 1;

        //Not necessary for every task but just in case
        if(!dock)
            dock = GameObject.Find("Dock");
        if(!cursor)
            cursor = GameObject.Find("Cursor");
        if(!home)
            home = GameObject.Find("Home");
        if(!plane)
            plane = GameObject.Find("Plane");
        if(!target)
            target = GameObject.Find("Target");
        if(!prefabCamera)
            prefabCamera = GameObject.Find("PrefabCamera").GetComponent<Camera>();
        if (!vrPos)
            vrPos = GameObject.Find("VR Position");

        if (expController.UseVR == false)
        {
            Camera.SetupCurrent(prefabCamera);
        }
        else
        {
            Debug.Log("Setting VR position");
            prefabCamera.gameObject.SetActive(false);
            //expController.vrCtlr.transform.position = vrPos.transform.position;
            //expController.vrCtlr.transform.rotation = vrPos.transform.rotation;
        }

        CursorController.Instance.Cursor = cursor;

        if (cursor)
        {
            CursorController.Instance.planeOffset = new Vector3(0.0f, -cursor.transform.position.y, 0.0f);
        }
    }

    /// <summary>
    /// Begin the task
    /// </summary>
    public virtual void TaskBegin()
    {
        // Debug.Log("Current trial in block: " + ExperimentController.Instance.Session.CurrentTrial.numberInBlock);
        // Debug.Log("Current block number: " + ExperimentController.Instance.Session.CurrentBlock.number);

        currentStep = 0;
        finished = false;
    }

    /// <summary>
    /// End the task
    /// </summary>
    public virtual void TaskEnd()
    {
        taskPrefab.SetActive(false);
        Destroy(taskPrefab);
        LogParameters();
    }

    /// <summary>
    /// Log parameters when a trial ends
    /// </summary>
    public abstract void LogParameters();
}
