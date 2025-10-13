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
using System.Linq;
using UnityEngine.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using UXF.UI;

public class StroopTask : BaseTask
{
    [Header("Stroop Task Components")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip buttonClickSFX;
    [SerializeField] AudioClip correctSFX;
    [SerializeField] AudioClip incorrectSFX;
    [SerializeField] AudioClip bongoHitSFX;
    
    [Header("UI Components")]
    [SerializeField] GameObject wordDisplayCanvas;
    [SerializeField] TextMeshProUGUI wordText;
    [Header("Button Setup")]
    [SerializeField] GameObject buttonContainer;
    [SerializeField] public List<GameObject> buttonObjects = new List<GameObject>(); // Your 4 button objects
    [SerializeField] public List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();
    
    // Button labels are now managed by JSON - no longer serialized
    private List<string> buttonLabels = new List<string>();
    
    [Header("Scoreboard")]
    [SerializeField] GameObject Scoreboard;
    [SerializeField] TextMeshProUGUI ScoreTXT;
    [SerializeField] TextMeshProUGUI TrialTXT;
    
    [Header("VR Components")]
    [SerializeField] public GameObject directRight;
    [SerializeField] public GameObject directLeft;
    [SerializeField] public GameObject leftHand;
    [SerializeField] public GameObject leftHandCtrl;
    [SerializeField] public GameObject rightHand;
    [SerializeField] public GameObject rightHandCtrl;
    [SerializeField] public GameObject MainCamera;
    
    [Header("Additional Components")]
    [SerializeField] GameObject spawnParent;
    [SerializeField] List<GameObject> spawnLocations = new List<GameObject>();

    // Stroop-specific data (loaded from JSON)
    private Dictionary<string, Color> colorMap = new Dictionary<string, Color>
    {
        { "red", Color.red },
        { "blue", Color.blue },
        { "green", Color.green },
        { "yellow", Color.yellow }
    };
    
    // Direction-specific data for blocks 3 and 4
    private Dictionary<string, string> directionMap = new Dictionary<string, string>
    {
        { "up", "↑" },
        { "down", "↓" },
        { "left", "←" },
        { "right", "→" }
    };
    
    private Dictionary<string, string> oppositeDirectionMap = new Dictionary<string, string>
    {
        { "up", "down" },
        { "down", "up" },
        { "left", "right" },
        { "right", "left" }
    };
    
    // Trial data
    private string currentWord = "";
    private Color currentColor = Color.white;
    private string correctAnswer = "";
    private string currentDirection = "";
    private int currentBlock = 0;
    private float trialStartTime = 0f;
    private float reactionTime = 0f;
    private bool trialActive = false;
    private bool responseGiven = false;
    
    // Timing and scoring
    private float startTime = 0.0f;
    private float endTime = 0.0f;
    private int totalScore = 0;
    private int totalCorrect = 0;
    private int completedTrials = 0;
    private float totalReactionTime = 0f;
    
    // Hand tracking data
    private List<Vector3> leftHandPos = new List<Vector3>();
    private List<Vector3> rightHandPos = new List<Vector3>();
    private List<string> hittingHand = new List<string>();
    
    // Data collection
    private List<float> reactionTimes = new List<float>();
    private List<bool> correctResponses = new List<bool>();
    private List<string> presentedWords = new List<string>();
    private List<string> presentedColors = new List<string>();
    private List<string> presentedDirections = new List<string>();
    private List<string> correctAnswers = new List<string>();
    private List<string> participantResponses = new List<string>();
    
    // Block-specific data tracking
    private List<float> blockReactionTimes = new List<float>();
    private List<bool> blockCorrectResponses = new List<bool>();
    private List<string> blockPresentedWords = new List<string>();
    private List<string> blockPresentedColors = new List<string>();
    private List<string> blockPresentedDirections = new List<string>();
    private List<string> blockCorrectAnswers = new List<string>();
    private List<string> blockParticipantResponses = new List<string>();
    private int blockCorrect = 0;
    private float blockTotalReactionTime = 0f;
    private float blockStartTime = 0f;
    
    // Cursor movement for 2D mode
    private float originalCursorY = 0f;
    private bool isLeftMouseHeld = false;
    private float cursorTransitionSpeed = 10f;
    
    // Trigger detection for dock and buttons
    private bool isInDockTrigger = false;
    private bool isInButtonTrigger = false;
    private string currentButtonInTrigger = "";
    
    // Block progression tracking
    private bool waitingForNextBlock = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetupButtons();
    }

    void Update()
    {
        // Handle left mouse button for 2D mode cursor movement
        if (!ExperimentController.Instance.UseVR)
        {
            // Check for left mouse button press
            if (Input.GetMouseButtonDown(0))
            {
                isLeftMouseHeld = true;
                // Debug.Log("Left mouse button pressed - moving cursor to Y=0");
            }
            // Check for left mouse button release
            else if (Input.GetMouseButtonUp(0))
            {
                isLeftMouseHeld = false;
                // Debug.Log("Left mouse button released - returning cursor to original height");
            }
            
            // Update cursor position based on left mouse button state
            UpdateCursorPosition();
            
            // Check for cursor collision with buttons when cursor is at Y=0
            // Add a small delay to prevent immediate collision after trial start
            if (isLeftMouseHeld && trialActive && !responseGiven && (Time.time - trialStartTime) > 0.1f)
            {
                // Debug: Log trigger states
                if (isInButtonTrigger)
                {
                    // Debug.Log($"Cursor is in button trigger: {currentButtonInTrigger}");
                }
                
                // ONLY use trigger-based detection - no fallback to prevent plane interaction
                if (isInButtonTrigger && !string.IsNullOrEmpty(currentButtonInTrigger))
                {
                    Debug.Log($"Triggering button response: {currentButtonInTrigger}");
                    OnButtonResponse(currentButtonInTrigger);
                }
                // Removed fallback to prevent plane collider interaction
            }
        }
        
        switch (currentStep)
        {
            case 0: // Wait for dock press to start trial
                {
                    bool dockHit = false;
                    
                    // Check for VR dock interaction
                    if (ExperimentController.Instance.UseVR)
                    {
                        dockHit = dock.GetComponent<Target>().TargetHit && dock.GetComponent<Target>().IsColliding;
                    }
                    // Check for 2D mode dock interaction (cursor collision or mouse click)
                    else
                    {
                        // Check if cursor is in dock trigger (only when cursor is at Y=0)
                        if (isLeftMouseHeld && isInDockTrigger)
                        {
                            dockHit = true;
                            // Debug.Log("Cursor is in dock trigger");
                        }
                        
                        // Disable mouse click dock detection for now - only use cursor collision
                        // This prevents dock from being triggered by any screen click
                    }
                    
                    if (dockHit)
                    {
                        if (waitingForNextBlock)
                        {
                            // Dock hit after block completion - advance to next block
                            Debug.Log("Dock hit - advancing to next block");
                            Debug.Log($"Current block before advance: {ExperimentController.Instance.Session.currentBlockNum}");
                            waitingForNextBlock = false;
                            
                            // Restore UI elements for new block
                            if (wordDisplayCanvas != null)
                                wordDisplayCanvas.SetActive(true);
                            // Note: buttonContainer will be enabled in StartTrial()
                            
                            // Try to advance to next block
                            try
                            {
                                // End the current trial - UXF will automatically advance to next block if needed
                                Debug.Log("Block completed, ending current trial");
                                ExperimentController.Instance.Session.EndCurrentTrial();
                                Debug.Log($"Current block after trial end: {ExperimentController.Instance.Session.currentBlockNum}");
                                
                                // Let UXF handle block transitions automatically
                                Debug.Log($"Block {ExperimentController.Instance.Session.currentBlockNum} completed, letting UXF handle block transition");
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogWarning($"Could not end trial automatically: {e.Message}");
                                Debug.LogWarning("Letting UXF handle block transitions automatically");
                            }
                        }
                        else
                        {
                            // Dock hit for starting trial
                            Debug.Log("Dock hit - starting trial with 1 second delay");
                            dock.GetComponent<Target>().ResetTarget();
                            audioSource.clip = buttonClickSFX;
                            audioSource.Play();
                            
                            // Disable dock when hit to prevent multiple triggers
                            dock.GetComponent<Target>().enabled = false;
                            dock.GetComponent<MeshCollider>().enabled = false;
                            dock.SetActive(false);

                            // Check if this is the start of a new block or first trial
                            if (ExperimentController.Instance.Session.CurrentTrial.numberInBlock == 1)
                            {
                                // Starting first trial of a block
                                startTime = Time.time;
                                Debug.Log($"Starting first trial of block {ExperimentController.Instance.Session.currentBlockNum}");
                            }

                            // Start trial with 1 second delay to prevent hand clipping with buttons
                            StartCoroutine(DelayedStartTrial(1.0f));
                            IncrementStep();
                        }
                    }
                }
                break;
            case 1: // Trial active - waiting for response
                {
                    if (trialActive && !responseGiven)
                    {
                        // Track hand position during trial
                        if (ExperimentController.Instance.UseVR)
                        {
                            if (directLeft != null)
                                leftHandPos.Add(directLeft.transform.position);
                            if (directRight != null)
                                rightHandPos.Add(directRight.transform.position);
                            
                            // Check for VR hand button interactions using MultipleTarget system
                            CheckVRButtonInteractions();
                        }
                        else
                        {
                            leftHandPos.Add(Input.mousePosition);
                            rightHandPos.Add(Input.mousePosition);
                        }
                    }
                }
                break;
            case 2: // Trial completed - wait for next trial or end block
                {
                    // This step handles the delay between trials
                    // The actual trial progression is handled in CompleteTrial()
                }
                break;
        }
    }

    private void StartTrial()
    {
        Debug.Log($"=== STARTING TRIAL {ExperimentController.Instance.Session.CurrentTrial.numberInBlock} ===");
        Debug.Log($"Current block number: {ExperimentController.Instance.Session.currentBlockNum}");
        Debug.Log($"Total blocks in session: {ExperimentController.Instance.Session.blocks.Count}");
        
        // IMMEDIATE DEBUG: Check what block type is detected
        string immediateBlockType = GetCurrentBlockType();
        Debug.Log($"IMMEDIATE DEBUG: Block {ExperimentController.Instance.Session.currentBlockNum} detected as type: '{immediateBlockType}'");
        
        // Reset block waiting flag
        waitingForNextBlock = false;
        
        // Ensure UI elements are visible
        if (wordDisplayCanvas != null)
            wordDisplayCanvas.SetActive(true);
        if (buttonContainer != null)
            buttonContainer.SetActive(true);
        
        // Generate trial parameters
        Debug.Log($"BEFORE GenerateTrialParameters: Word='{currentWord}', Correct='{correctAnswer}'");
        GenerateTrialParameters();
        Debug.Log($"AFTER GenerateTrialParameters: Word='{currentWord}', Correct='{correctAnswer}'");
        
        // Display the word
        DisplayWord();
        
        // Setup response buttons
        SetupResponseButtons();
        
        Debug.Log($"After StartTrial: Word='{currentWord}', Color={currentColor}, Correct='{correctAnswer}'");
        
        // Auto-setup VR hand interactions if in VR mode (ensure Tools are populated)
        if (ExperimentController.Instance.UseVR)
        {
            Debug.Log("=== STARTING VR HAND AUTO-SETUP ===");
            AutoSetupVRHandInteractions();
            Debug.Log("=== VR HAND AUTO-SETUP COMPLETE ===");
        }
        else
        {
            Debug.Log("Not in VR mode - skipping VR hand setup");
        }
        
        // Activate buttons for interaction
        ActivateButtons(true);
        
        // Enable button container for trial
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(true);
        }
        
        // Hide dock during trial
        if (dock != null)
        {
            dock.SetActive(false);
        }
        
        // Start trial timing
        trialActive = true;
        responseGiven = false;
        trialStartTime = Time.time;
        
        // Move cursor away from buttons to prevent immediate collision
        if (cursor != null && isLeftMouseHeld)
        {
            // Move cursor to a safe position away from buttons
            Vector3 safePosition = new Vector3(0, 0, 0); // Center position
            cursor.transform.position = safePosition;
            // Debug.Log($"Moved cursor to safe position: {safePosition}");
        }
        
        Debug.Log($"Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock} is now ACTIVE");
        
        Debug.Log($"Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock}: Word='{currentWord}', Color={currentColor}, Correct='{correctAnswer}'");
    }

    private void ActivateButtons(bool active)
    {
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Activate/deactivate ButtonCollisionHandler for 2D mode
                ButtonCollisionHandler handler = buttonObjects[i].GetComponent<ButtonCollisionHandler>();
                if (handler != null)
                {
                    handler.SetActive(active);
                }
                
                // Find the Goal Collider child object
                Transform goalColliderTransform = buttonObjects[i].transform.Find("LO Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("LI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RO Goal Collider");
                
                if (goalColliderTransform == null)
                {
                    // Try to find any child with "Goal Collider" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("Goal Collider"))
                        {
                            goalColliderTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalColliderTransform != null)
                {
                    // Enable/disable MultipleTarget component
                    MultipleTarget multipleTarget = goalColliderTransform.GetComponent<MultipleTarget>();
                    if (multipleTarget != null)
                    {
                        multipleTarget.enabled = active;
                        if (active)
                        {
                            multipleTarget.ResetState();
                        }
                        // Debug.Log($"MultipleTarget on {goalColliderTransform.name} {(active ? "enabled" : "disabled")}");
                    }
                    
                    // Enable/disable capsule collider based on VR mode
                    CapsuleCollider capsuleCollider = goalColliderTransform.GetComponent<CapsuleCollider>();
                    if (capsuleCollider != null)
                    {
                        if (ExperimentController.Instance.UseVR)
                        {
                            capsuleCollider.enabled = active; // Enable/disable based on button state in VR mode
                            // Debug.Log($"Capsule collider on {goalColliderTransform.name} {(active ? "enabled" : "disabled")} for VR mode");
                        }
                        else
                        {
                            capsuleCollider.enabled = false; // Always disabled for 2D cursor mode
                            // Debug.Log($"Capsule collider on {goalColliderTransform.name} disabled for 2D cursor mode");
                        }
                    }
                }
                else
                {
                    // Fallback: activate/deactivate MultipleTarget on main button object
                    MultipleTarget multipleTarget = buttonObjects[i].GetComponent<MultipleTarget>();
                    if (multipleTarget != null)
                    {
                        multipleTarget.enabled = active;
                        if (active)
                        {
                            multipleTarget.ResetState();
                        }
                        // Debug.Log($"MultipleTarget on main button {i} {(active ? "enabled" : "disabled")} (fallback)");
                    }
                }
            }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        // Initialize VR components with better detection and debugging
        directRight = GameObject.Find("RH Direct Interactor");
        directLeft = GameObject.Find("LH Direct Interactor");
        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");
        MainCamera = GameObject.Find("Main Camera");
        
        // Debug VR component detection
        Debug.Log($"VR Component Detection:");
        Debug.Log($"  directRight: {(directRight != null ? directRight.name : "NOT FOUND")}");
        Debug.Log($"  directLeft: {(directLeft != null ? directLeft.name : "NOT FOUND")}");
        Debug.Log($"  leftHand: {(leftHand != null ? leftHand.name : "NOT FOUND")}");
        Debug.Log($"  rightHand: {(rightHand != null ? rightHand.name : "NOT FOUND")}");
        Debug.Log($"  leftHandCtrl: {(leftHandCtrl != null ? leftHandCtrl.name : "NOT FOUND")}");
        Debug.Log($"  rightHandCtrl: {(rightHandCtrl != null ? rightHandCtrl.name : "NOT FOUND")}");
        
        // Try alternative names for VR hands if primary names fail
        if (leftHand == null)
        {
            leftHand = GameObject.Find("LeftHand");
            if (leftHand == null) leftHand = GameObject.Find("LeftHand Controller");
            if (leftHand == null) leftHand = GameObject.Find("LeftHandController");
        }
        
        if (rightHand == null)
        {
            rightHand = GameObject.Find("RightHand");
            if (rightHand == null) rightHand = GameObject.Find("RightHand Controller");
            if (rightHand == null) rightHand = GameObject.Find("RightHandController");
        }
        
        if (directLeft == null)
        {
            directLeft = GameObject.Find("LeftHand Direct Interactor");
            if (directLeft == null) directLeft = GameObject.Find("Left Direct Interactor");
        }
        
        if (directRight == null)
        {
            directRight = GameObject.Find("RightHand Direct Interactor");
            if (directRight == null) directRight = GameObject.Find("Right Direct Interactor");
        }
        
        // Log final detection results
        Debug.Log($"Final VR Component Detection:");
        Debug.Log($"  directRight: {(directRight != null ? directRight.name : "NOT FOUND")}");
        Debug.Log($"  directLeft: {(directLeft != null ? directLeft.name : "NOT FOUND")}");
        Debug.Log($"  leftHand: {(leftHand != null ? leftHand.name : "NOT FOUND")}");
        Debug.Log($"  rightHand: {(rightHand != null ? rightHand.name : "NOT FOUND")}");

        // Setup VR or desktop mode
        SetupXR();
        
        // Setup VR hand interactions if in VR mode
        if (ExperimentController.Instance.UseVR)
        {
            SetupVRHandTags();
            SetupVRHandInteractions();
        }
        
        // Initialize cursor Y position for 2D mode
        if (!ExperimentController.Instance.UseVR && cursor != null)
        {
            originalCursorY = cursor.transform.position.y;
            // Debug.Log($"Original cursor Y position: {originalCursorY}");
            
            // Ensure cursor has a collider for collision detection
            if (cursor.GetComponent<Collider>() == null)
            {
                // Add a small box collider to the cursor for collision detection
                BoxCollider cursorCollider = cursor.AddComponent<BoxCollider>();
                cursorCollider.size = new Vector3(0.1f, 0.1f, 0.1f); // Small collider
                cursorCollider.isTrigger = true; // Make it a trigger so it doesn't interfere with physics
                // Debug.Log("Added collider to cursor for collision detection");
            }
            
            // Add trigger detection component to cursor
            CursorTriggerDetector triggerDetector = cursor.GetComponent<CursorTriggerDetector>();
            if (triggerDetector == null)
            {
                triggerDetector = cursor.AddComponent<CursorTriggerDetector>();
                triggerDetector.Initialize(this);
                // Debug.Log("Added trigger detector to cursor");
            }
        }
        
        // Setup buttons
        SetupButtons();
        
        // Disable buttons initially - they will be enabled when trials start
        ActivateButtons(false);
        
        // Initialize timing
        startTime = 0.0f;
        endTime = 0.0f;
        
        // Clear previous data
        reactionTimes.Clear();
        correctResponses.Clear();
        presentedWords.Clear();
        presentedColors.Clear();
        presentedDirections.Clear();
        correctAnswers.Clear();
        participantResponses.Clear();
        leftHandPos.Clear();
        rightHandPos.Clear();
        hittingHand.Clear();
        
        totalCorrect = 0;
        completedTrials = 0;
        totalReactionTime = 0f;
        totalScore = 0;
    }

    private void SetupButtons()
    {
        // Add collision detection to button objects
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Add a script to handle button collisions
                ButtonCollisionHandler handler = buttonObjects[i].GetComponent<ButtonCollisionHandler>();
                if (handler == null)
                {
                    handler = buttonObjects[i].AddComponent<ButtonCollisionHandler>();
                }
                // Initialize with placeholder - will be updated in SetupResponseButtons
                handler.Initialize(this, "placeholder");
            }
        }
    }
    
    /// <summary>
    /// Reset block-specific data when starting a new block
    /// </summary>
    private void ResetBlockData()
    {
        // Clear block-specific data lists
        blockReactionTimes.Clear();
        blockCorrectResponses.Clear();
        blockPresentedWords.Clear();
        blockPresentedColors.Clear();
        blockPresentedDirections.Clear();
        blockCorrectAnswers.Clear();
        blockParticipantResponses.Clear();
        
        // Reset block-specific counters
        blockCorrect = 0;
        blockTotalReactionTime = 0f;
        blockStartTime = Time.time;
        
        Debug.Log($"Block {ExperimentController.Instance.Session.currentBlockNum} data reset - starting fresh tracking");
    }

    public override void TaskBegin()
    {
        base.TaskBegin();
        
        // DEBUG: Check what trial name UXF is using
        try
        {
            string uxfTrialName = ExperimentController.Instance.Session.CurrentTrial.settings.GetString("trial_name");
            Debug.Log($"UXF TRIAL NAME: '{uxfTrialName}' for Block {ExperimentController.Instance.Session.currentBlockNum}, Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock}");
        }
        catch
        {
            Debug.Log($"UXF TRIAL NAME: NOT FOUND for Block {ExperimentController.Instance.Session.currentBlockNum}, Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock}");
        }
        
        // DEBUG: Check the per_block_target_location array
        try
        {
            var perBlockTargetLocation = ExperimentController.Instance.Session.settings.GetStringList("per_block_target_location");
            Debug.Log($"per_block_target_location array: [{string.Join(", ", perBlockTargetLocation)}]");
            if (perBlockTargetLocation != null && ExperimentController.Instance.Session.currentBlockNum > 0 && ExperimentController.Instance.Session.currentBlockNum <= perBlockTargetLocation.Count)
            {
                string expectedBlockType = perBlockTargetLocation[ExperimentController.Instance.Session.currentBlockNum - 1];
                Debug.Log($"Expected block type for Block {ExperimentController.Instance.Session.currentBlockNum}: '{expectedBlockType}'");
                
                // DEBUG: Check if this is the first trial of a new block
                if (ExperimentController.Instance.Session.CurrentTrial.numberInBlock == 1)
                {
                    Debug.Log($"=== STARTING NEW BLOCK {ExperimentController.Instance.Session.currentBlockNum} ===");
                    Debug.Log($"Block type: {expectedBlockType}");
                    Debug.Log($"Expected first trial: {expectedBlockType}_trial_1");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading per_block_target_location: {e.Message}");
        }
        
        // Reset block-specific data for new block
        ResetBlockData();
        
        // Reset trial data
        trialActive = false;
        responseGiven = false;
        
        // Show UI elements
        if (wordDisplayCanvas != null)
            wordDisplayCanvas.SetActive(true);
        
        // Disable button container when dock is active
        if (buttonContainer != null)
            buttonContainer.SetActive(false);
        
        // Ensure buttons are disabled initially - they will be enabled when trials start
        ActivateButtons(false);
        
        // Setup dock
        dock.SetActive(true);
        dock.GetComponent<Target>().enabled = true;
        dock.GetComponent<MeshCollider>().enabled = true;
        dock.GetComponent<Target>().ResetTarget();
        
        // Auto-setup VR hand interactions if in VR mode
        if (ExperimentController.Instance.UseVR)
        {
            AutoSetupVRHandInteractions();
        }
        else
        {
            // Disable capsule colliders for 2D cursor mode
            DisableCapsuleCollidersFor2D();
        }
        
        // Update scoreboard
        UpdateScoreboard();
        
        // Adjust scoreboard for desktop mode
        if (!ExperimentController.Instance.UseVR)
        {
            Scoreboard.transform.eulerAngles = new Vector3(90f, Scoreboard.transform.eulerAngles.y, Scoreboard.transform.eulerAngles.z);
        }
    }


    private void GenerateTrialParameters()
    {
        Debug.Log($"=== GenerateTrialParameters: Block {ExperimentController.Instance.Session.currentBlockNum}, Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock} ===");
        Debug.Log($"GenerateTrialParameters: BEFORE - Word='{currentWord}', Correct='{correctAnswer}'");
        
        // Get current trial data from JSON - use fallback if trial_name doesn't exist
        string currentTrialName;
        try
        {
            currentTrialName = ExperimentController.Instance.Session.CurrentTrial.settings.GetString("trial_name");
            Debug.Log($"GenerateTrialParameters: Found trial_name in session settings: '{currentTrialName}'");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            // Try to determine trial type and create proper trial name
            int trialNumber = ExperimentController.Instance.Session.CurrentTrial.numberInBlock;
            int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
            
            // Use the GetCurrentBlockType method for consistent block type detection
            string blockType = GetCurrentBlockType();
            
            currentTrialName = $"{blockType}_trial_{trialNumber}";
            Debug.Log($"=== BLOCK TYPE DETECTION RESULT ===");
            Debug.Log($"Block type determined: {blockType}");
            Debug.Log($"Current block number: {blockNumber}");
            Debug.Log($"Trial number: {trialNumber}");
            Debug.Log($"Generated trial name: {currentTrialName}");
            Debug.Log($"=== END BLOCK TYPE DETECTION ===");
            Debug.LogWarning($"trial_name not found, using generated name: {currentTrialName} (blockType: {blockType}, trialNumber: {trialNumber}, blockNumber: {blockNumber})");
            
            // Manually set the trial name in the session settings so UXF can use it
            ExperimentController.Instance.Session.CurrentTrial.settings["trial_name"] = currentTrialName;
            Debug.Log($"Manually set trial_name in session settings: '{currentTrialName}'");
        }
        
        // Get trial data from session settings
        var trialData = ExperimentController.Instance.Session.settings.GetObject("trial_data");
        
        // Debug: Log trial data structure
        Debug.Log($"Trial data type: {trialData?.GetType()}");
        Debug.Log($"Trial data is null: {trialData == null}");
        Debug.Log($"Looking for trial: {currentTrialName}");
        
        if (trialData != null)
        {
            Debug.Log($"Trial data: {trialData}");
        }
        
        // Debug: Log available trial data keys
        if (trialData is System.Collections.Generic.Dictionary<string, object> trialDataDict)
        {
            Debug.Log($"Available trial data keys: {string.Join(", ", trialDataDict.Keys)}");
            Debug.Log($"Looking for trial: {currentTrialName}");
            Debug.Log($"Trial found in data: {trialDataDict.ContainsKey(currentTrialName)}");
            
            // Debug: Check if the trial name matches any available keys
            if (!trialDataDict.ContainsKey(currentTrialName))
            {
                Debug.LogWarning($"Trial '{currentTrialName}' not found in data. Available keys: {string.Join(", ", trialDataDict.Keys)}");
                
                // Debug: Check if there are any trials that start with the block type
                string blockType = GetCurrentBlockType();
                var matchingTrials = trialDataDict.Keys.Where(key => key.StartsWith(blockType)).ToList();
                Debug.Log($"Trials starting with '{blockType}': {string.Join(", ", matchingTrials)}");
            }
            
                if (trialDataDict.ContainsKey(currentTrialName))
                {
                    var currentTrialData = trialDataDict[currentTrialName] as System.Collections.Generic.Dictionary<string, object>;
                    
                    if (currentTrialData != null)
                    {
                        Debug.Log($"GenerateTrialParameters: Found trial data for '{currentTrialName}' with keys: {string.Join(", ", currentTrialData.Keys)}");
                        
                        // Check if this is a direction-based block by checking the block type
                        string blockType = GetCurrentBlockType();
                        Debug.Log($"GenerateTrialParameters: Block type detected as '{blockType}' for block {ExperimentController.Instance.Session.currentBlockNum}");
                        Debug.Log($"GenerateTrialParameters: Trial data keys: {string.Join(", ", currentTrialData.Keys)}");
                        
                        // Check if trial data has direction keys or color keys
                        bool hasDirectionKeys = currentTrialData.ContainsKey("displayed_direction");
                        bool hasColorKeys = currentTrialData.ContainsKey("displayed_word") && currentTrialData.ContainsKey("displayed_color");
                        Debug.Log($"GenerateTrialParameters: Has direction keys: {hasDirectionKeys}, Has color keys: {hasColorKeys}");
                        
                        if (blockType == "direction_same" || blockType == "direction_opposite")
                        {
                            // Handle direction-based trials
                            currentDirection = currentTrialData["displayed_direction"].ToString();
                            correctAnswer = currentTrialData["correct_answer"].ToString();
                            
                            // For direction blocks, display the arrow symbol
                            currentWord = directionMap.ContainsKey(currentDirection) ? directionMap[currentDirection] : currentDirection;
                            currentColor = Color.white; // Use white color for direction arrows
                            
                            Debug.Log($"Found direction trial data for {currentTrialName}: Direction='{currentDirection}', Arrow='{currentWord}', Correct='{correctAnswer}'");
                            
                            // Store trial data
                            presentedWords.Add(currentWord);
                            presentedColors.Add(""); // Empty for direction trials
                            presentedDirections.Add(currentDirection);
                            correctAnswers.Add(correctAnswer);
                        }
                        else
                        {
                            // Handle color-based trials (blocks 1 and 2)
                            currentWord = currentTrialData["displayed_word"].ToString();
                            string colorName = currentTrialData["displayed_color"].ToString();
                            correctAnswer = currentTrialData["correct_answer"].ToString();
                            
                            Debug.Log($"Found COLOR trial data for {currentTrialName}: Word='{currentWord}', Color='{colorName}', Correct='{correctAnswer}'");
                        
                            // Convert color name to Unity Color
                            if (colorMap.ContainsKey(colorName))
                            {
                                currentColor = colorMap[colorName];
                                Debug.Log($"Color converted: '{colorName}' -> {currentColor}");
                                Debug.Log($"Color RGB: R={currentColor.r}, G={currentColor.g}, B={currentColor.b}, A={currentColor.a}");
                            }
                            else
                            {
                                Debug.LogError($"Unknown color: {colorName}");
                                Debug.LogError($"Available colors in colorMap: {string.Join(", ", colorMap.Keys)}");
                                currentColor = Color.white;
                            }
                            
                            // Store trial data
                            presentedWords.Add(currentWord);
                            presentedColors.Add(colorName);
                            presentedDirections.Add(""); // Empty for color trials
                            correctAnswers.Add(correctAnswer);
                        }
                }
                else
                {
                    Debug.LogError($"Could not parse trial data for: {currentTrialName}");
                    GenerateFallbackTrialData();
                }
            }
            else
            {
                Debug.LogWarning($"Could not find trial data for: {currentTrialName} - using fallback");
                GenerateFallbackTrialData();
            }
        }
        else
        {
            Debug.LogError("Trial data is not in expected format");
            GenerateFallbackTrialData();
        }
        
        Debug.Log($"GenerateTrialParameters: AFTER - Word='{currentWord}', Correct='{correctAnswer}'");
    }

    /// <summary>
    /// Get the current block type from session settings
    /// </summary>
    private string GetCurrentBlockType()
    {
        int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
        
        // First try to get from session settings
        try
        {
            var blockSettings = ExperimentController.Instance.Session.CurrentBlock.settings;
            Debug.Log($"GetCurrentBlockType: Block settings keys: {string.Join(", ", blockSettings.Keys)}");
            string blockType = blockSettings.GetString("target_location");
            Debug.Log($"GetCurrentBlockType: Found block type '{blockType}' from session settings for block {blockNumber}");
            return blockType;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GetCurrentBlockType: Could not get block type from session settings: {e.Message}");
        }
        
        // Try to get from the per_block_target_location array in session settings
        try
        {
            var perBlockTargetLocation = ExperimentController.Instance.Session.settings.GetStringList("per_block_target_location");
            if (perBlockTargetLocation != null && blockNumber > 0 && blockNumber <= perBlockTargetLocation.Count)
            {
                string blockType = perBlockTargetLocation[blockNumber - 1]; // Convert to 0-based index
                Debug.Log($"GetCurrentBlockType: Found block type '{blockType}' from per_block_target_location array for block {blockNumber}");
                return blockType;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"GetCurrentBlockType: Could not get block type from per_block_target_location: {e.Message}");
        }
        
        // Final fallback to hardcoded block number-based detection
        string fallbackType;
        if (blockNumber == 3) fallbackType = "direction_same";
        else if (blockNumber == 4) fallbackType = "direction_opposite";
        else if (blockNumber == 2) fallbackType = "incongruent";
        else fallbackType = "congruent";
        
        Debug.LogWarning($"GetCurrentBlockType: Using hardcoded fallback block type '{fallbackType}' for block {blockNumber}");
        return fallbackType;
    }

    /// <summary>
    /// Generate fallback trial data when JSON data is not available
    /// </summary>
    private void GenerateFallbackTrialData()
    {
        Debug.LogWarning("=== GENERATING FALLBACK TRIAL DATA ===");
        
        string blockType = GetCurrentBlockType();
        int trialIndex = ExperimentController.Instance.Session.CurrentTrial.numberInBlock - 1;
        
        if (blockType == "direction_same" || blockType == "direction_opposite")
        {
            // Direction-based fallback trial data
            string[] directions = { "up", "down", "left", "right" };
            currentDirection = directions[trialIndex % directions.Length];
            currentWord = directionMap.ContainsKey(currentDirection) ? directionMap[currentDirection] : currentDirection;
            currentColor = Color.white;
            
            // For direction_same: same direction, for direction_opposite: opposite direction
            if (blockType == "direction_same")
            {
                correctAnswer = currentDirection;
            }
            else // direction_opposite
            {
                correctAnswer = oppositeDirectionMap.ContainsKey(currentDirection) ? oppositeDirectionMap[currentDirection] : currentDirection;
            }
            
            // Store trial data
            presentedWords.Add(currentWord);
            presentedColors.Add(""); // Empty for direction trials
            presentedDirections.Add(currentDirection);
            correctAnswers.Add(correctAnswer);
            
            Debug.Log($"Fallback direction trial data: Direction='{currentDirection}', Arrow='{currentWord}', Correct='{correctAnswer}'");
        }
        else
        {
            // Color-based fallback trial data
            string[] words = { "RED", "BLUE", "GREEN", "YELLOW" };
            string[] colors = { "red", "blue", "green", "yellow" };
            
            currentWord = words[trialIndex % words.Length];
            string colorName = colors[trialIndex % colors.Length];
            correctAnswer = colorName; // For fallback, correct answer is the color name
            
            // Convert color name to Unity Color
            if (colorMap.ContainsKey(colorName))
            {
                currentColor = colorMap[colorName];
            }
            else
            {
                currentColor = Color.white;
            }
            
            // Store trial data
            presentedWords.Add(currentWord);
            presentedColors.Add(colorName);
            presentedDirections.Add(""); // Empty for color trials
            correctAnswers.Add(correctAnswer);
            
            Debug.Log($"Fallback trial data: Word='{currentWord}', Color={colorName}, Correct='{correctAnswer}'");
        }
    }

    private void DisplayWord()
    {
        wordText.text = currentWord;
        
        // Check if this is a direction-based block and increase font size for arrows
        string blockType = GetCurrentBlockType();
        Debug.Log($"DisplayWord: Block type '{blockType}', displaying text '{currentWord}'");
        if (blockType == "direction_same" || blockType == "direction_opposite")
        {
            // Make arrows bigger for direction blocks
            wordText.fontSize = 200f; // Increase from default size
            Debug.Log($"Direction block detected - setting font size to 200 for better arrow visibility");
        }
        else
        {
            // Reset to default font size for color blocks
            wordText.fontSize = 100f; // Default size for color words
        }
        
        // Try multiple approaches to set the color
        wordText.color = currentColor;
        wordText.faceColor = currentColor;
        
        // Force TextMeshPro to update the color and size
        wordText.ForceMeshUpdate();
        
        Debug.Log($"DisplayWord: Text='{currentWord}', Color={currentColor}, CorrectAnswer='{correctAnswer}', FontSize={wordText.fontSize}");
        Debug.Log($"Color RGB values: R={currentColor.r}, G={currentColor.g}, B={currentColor.b}, A={currentColor.a}");
        Debug.Log($"WordText component found: {wordText != null}");
        if (wordText != null)
        {
            Debug.Log($"WordText actual color after setting: {wordText.color}");
            Debug.Log($"WordText faceColor: {wordText.faceColor}");
            Debug.Log($"WordText outlineColor: {wordText.outlineColor}");
        }
    }

    private void SetupResponseButtons()
    {
        // Get current trial data from JSON - use fallback if trial_name doesn't exist
        string currentTrialName;
        try
        {
            currentTrialName = ExperimentController.Instance.Session.CurrentTrial.settings.GetString("trial_name");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            // Try to determine trial type and create proper trial name
            int trialNumber = ExperimentController.Instance.Session.CurrentTrial.numberInBlock;
            int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
            
            // Use the GetCurrentBlockType method for consistent block type detection
            string blockType = GetCurrentBlockType();
            
            currentTrialName = $"{blockType}_trial_{trialNumber}";
            Debug.LogWarning($"trial_name not found, using generated name: {currentTrialName}");
        }
        var trialData = ExperimentController.Instance.Session.settings.GetObject("trial_data");
        
        // Cast the trial data to a dictionary
        if (trialData is System.Collections.Generic.Dictionary<string, object> trialDataDict)
        {
                if (trialDataDict.ContainsKey(currentTrialName))
                {
                    var currentTrialData = trialDataDict[currentTrialName] as System.Collections.Generic.Dictionary<string, object>;
                    
                    if (currentTrialData != null)
                    {
                        // Debug.Log($"Found trial data for buttons: {currentTrialName}");
                        
                        // Check if this is a direction-based block
                        string blockType = GetCurrentBlockType();
                        List<string> buttonOptions = new List<string>();
                        
                        if (blockType == "direction_same" || blockType == "direction_opposite")
                        {
                            // For direction blocks, use direction names as button options
                            buttonOptions = new List<string> { "up", "down", "left", "right" };
                            // Debug.Log($"Direction block button options: [{string.Join(", ", buttonOptions)}]");
                        }
                        else
                        {
                            // For color blocks, get button options from JSON
                            var buttonOptionsJson = currentTrialData["button_options"];
                            
                            // Convert JSON array to List<string> - handle as object array
                            if (buttonOptionsJson is System.Collections.IList jsonArray)
                            {
                                foreach (var item in jsonArray)
                                {
                                    buttonOptions.Add(item.ToString());
                                }
                                // Debug.Log($"Button options from JSON: [{string.Join(", ", buttonOptions)}]");
                            }
                            else
                            {
                                Debug.LogError($"Button options is not a list: {buttonOptionsJson?.GetType()}");
                            }
                        }
                        
                        // Update button texts with options
                        for (int i = 0; i < buttonTexts.Count && i < buttonOptions.Count; i++)
                        {
                            if (buttonTexts[i] != null)
                            {
                                buttonTexts[i].text = buttonOptions[i];
                                // Debug.Log($"Button {i} text set to: {buttonOptions[i]}");
                            }
                        }
                    
                        // Update button labels for collision detection
                        buttonLabels.Clear();
                        buttonLabels.AddRange(buttonOptions);
                        
                        // Update ButtonCollisionHandler labels
                        for (int i = 0; i < buttonObjects.Count && i < buttonOptions.Count; i++)
                        {
                            if (buttonObjects[i] != null)
                            {
                                ButtonCollisionHandler handler = buttonObjects[i].GetComponent<ButtonCollisionHandler>();
                                if (handler != null)
                                {
                                    handler.Initialize(this, buttonOptions[i]);
                                }
                            }
                        }
                        
                        // Debug.Log($"Button setup from JSON: Correct='{correctAnswer}', Options=[{string.Join(", ", buttonOptions)}]");
                }
                else
                {
                    Debug.LogError($"Could not parse trial data for: {currentTrialName}");
                    SetupFallbackButtons();
                }
            }
            else
            {
                Debug.LogWarning($"Could not find trial data for: {currentTrialName} - using fallback buttons");
                SetupFallbackButtons();
            }
        }
        else
        {
            Debug.LogError("Trial data is not in expected format");
            SetupFallbackButtons();
        }
    }

    /// <summary>
    /// Setup fallback button options when JSON data is not available
    /// </summary>
    private void SetupFallbackButtons()
    {
        Debug.LogWarning("=== SETTING UP FALLBACK BUTTONS ===");
        
        string blockType = GetCurrentBlockType();
        string[] buttonOptions;
        
        if (blockType == "direction_same" || blockType == "direction_opposite")
        {
            // Direction-based fallback button options
            buttonOptions = new string[] { "up", "down", "left", "right" };
            // Debug.Log($"Direction block fallback button options: [{string.Join(", ", buttonOptions)}]");
        }
        else
        {
            // Color-based fallback button options
            buttonOptions = new string[] { "red", "blue", "green", "yellow" };
            // Debug.Log($"Color block fallback button options: [{string.Join(", ", buttonOptions)}]");
        }
        
        // Update button texts with fallback options
        for (int i = 0; i < buttonTexts.Count && i < buttonOptions.Length; i++)
        {
            if (buttonTexts[i] != null)
            {
                buttonTexts[i].text = buttonOptions[i];
            }
        }
        
        // Update button labels for collision detection
        buttonLabels.Clear();
        buttonLabels.AddRange(buttonOptions);
        
        // Update ButtonCollisionHandler labels
        for (int i = 0; i < buttonObjects.Count && i < buttonOptions.Length; i++)
        {
            if (buttonObjects[i] != null)
            {
                ButtonCollisionHandler handler = buttonObjects[i].GetComponent<ButtonCollisionHandler>();
                if (handler != null)
                {
                    handler.Initialize(this, buttonOptions[i]);
                }
            }
        }
        
        // Debug.Log($"Fallback button setup: Options=[{string.Join(", ", buttonOptions)}]");
    }

    private void OnButtonClick(int buttonIndex)
    {
        if (!trialActive || responseGiven) return;
        
        responseGiven = true;
        trialActive = false;
        
        // Calculate reaction time
        reactionTime = Time.time - trialStartTime;
        
        // Get the selected answer
        string selectedAnswer = buttonTexts[buttonIndex].text;
        
        // Check if correct
        bool isCorrect = selectedAnswer == correctAnswer;
        
        // Track which hand was used (for VR)
        if (ExperimentController.Instance.UseVR)
        {
            // Determine which hand was used based on button position or interaction
            // For now, we'll use a simple approach - you can enhance this based on your VR setup
            hittingHand.Add("vr_hand");
        }
        else
        {
            hittingHand.Add("mouse");
        }
        
        // Store data
        reactionTimes.Add(reactionTime);
        correctResponses.Add(isCorrect);
        participantResponses.Add(selectedAnswer);
        
        // Update statistics and score
        completedTrials++;
        totalReactionTime += reactionTime;
        if (isCorrect)
        {
            totalCorrect++;
            totalScore += 10; // Award points for correct answers
            audioSource.clip = correctSFX;
        }
        else
        {
            audioSource.clip = incorrectSFX;
        }
        audioSource.Play();
        
        // Update scoreboard
        UpdateScoreboard();
        
        // Complete trial
        StartCoroutine(CompleteTrial());
    }

    private IEnumerator CompleteTrial()
    {
        Debug.Log($"CompleteTrial called - completedTrials: {completedTrials}, totalCorrect: {totalCorrect}");
        
        // Only proceed if we actually have a response
        if (!responseGiven)
        {
            Debug.LogWarning("CompleteTrial called without a response - this should not happen!");
            yield break;
        }
        
        // Wait 0.5 seconds before next trial
        yield return new WaitForSeconds(0.5f);
        
        // Check if we've completed all trials in this block using ExperimentController
        List<int> trialsPerBlock = ExperimentController.Instance.Session.settings.GetIntList("trials_in_block");
        int currentTrialInBlock = ExperimentController.Instance.Session.CurrentTrial.numberInBlock;
        int trialsInCurrentBlock = trialsPerBlock[ExperimentController.Instance.Session.currentBlockNum - 1];
        
        Debug.Log($"Trial check - currentTrialInBlock: {currentTrialInBlock}, trialsInCurrentBlock: {trialsInCurrentBlock}");
        Debug.Log($"Current block number: {ExperimentController.Instance.Session.currentBlockNum}");
        Debug.Log($"Total blocks: {ExperimentController.Instance.Session.blocks.Count}");
        Debug.Log($"Trials per block array: [{string.Join(", ", trialsPerBlock)}]");
        
        if (currentTrialInBlock >= trialsInCurrentBlock)
        {
            // Block completed - show dock for next block instructions
            endTime = Time.time;
            Debug.Log($"Block {ExperimentController.Instance.Session.currentBlockNum} completed. Accuracy: {(float)totalCorrect / completedTrials * 100:F1}%, Avg RT: {totalReactionTime / completedTrials:F3}s");
            
            // Show dock button for next block instructions
            ShowDockForNextBlock();
        }
        else
        {
            // Show dock for next trial
            if (dock != null)
            {
                dock.SetActive(true);
                dock.GetComponent<Target>().enabled = true;
                dock.GetComponent<MeshCollider>().enabled = true;
                dock.GetComponent<Target>().ResetTarget();
            }
            
            // Disable button container when dock is active
            if (buttonContainer != null)
            {
                buttonContainer.SetActive(false);
            }
            
            // Reset step to wait for dock press
            currentStep = 0;
            
            Debug.Log($"Dock shown for next trial. Current trial: {currentTrialInBlock}/{trialsInCurrentBlock}. Press to continue.");
            
            // Try to advance to the next trial in UXF system
            try
            {
                // End current trial and advance to next
                ExperimentController.Instance.Session.EndCurrentTrial();
                Debug.Log("Trial ended, should advance to next trial automatically");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not end trial automatically: {e.Message}");
            }
        }
    }

    private void ShowDockForNextBlock()
    {
        // Hide UI elements during instruction period
        if (wordDisplayCanvas != null)
            wordDisplayCanvas.SetActive(false);
        if (buttonContainer != null)
            buttonContainer.SetActive(false);
        
        // Show dock button for next block
        dock.SetActive(true);
        dock.GetComponent<Target>().enabled = true;
        dock.GetComponent<MeshCollider>().enabled = true;
        dock.GetComponent<Target>().ResetTarget();
        
        // Set flag to indicate we're waiting for next block
        waitingForNextBlock = true;
        
        // Reset step to wait for dock press
        currentStep = 0;
        
        Debug.Log("Dock shown for next block instructions. Press to continue.");
    }

    private void SetupXR()
    {
        if (ExperimentController.Instance.UseVR)
        {
            // Switch Camera to VR
            if (prefabCamera != null)
                prefabCamera.gameObject.SetActive(false);
            if (cursor != null)
                cursor.SetActive(false);
        }
        else
        {
            if (cursor != null)
                cursor.SetActive(true);
            // Switch Camera to 2D
            if (prefabCamera != null)
                prefabCamera.gameObject.SetActive(true);
            if (MainCamera != null)
            {
                MainCamera.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Setup proper tags for VR hand objects to enable interaction detection
    /// </summary>
    private void SetupVRHandTags()
    {
        Debug.Log("Setting up VR hand tags...");
        
        // Tag the hand objects
        if (leftHand != null)
        {
            leftHand.tag = "Hand";
            Debug.Log($"Tagged {leftHand.name} as 'Hand'");
        }
        
        if (rightHand != null)
        {
            rightHand.tag = "Hand";
            Debug.Log($"Tagged {rightHand.name} as 'Hand'");
        }
        
        // Tag the direct interactors
        if (directLeft != null)
        {
            directLeft.tag = "Controller";
            Debug.Log($"Tagged {directLeft.name} as 'Controller'");
        }
        
        if (directRight != null)
        {
            directRight.tag = "Controller";
            Debug.Log($"Tagged {directRight.name} as 'Controller'");
        }
        
        // Tag the controller objects
        if (leftHandCtrl != null)
        {
            leftHandCtrl.tag = "Controller";
            Debug.Log($"Tagged {leftHandCtrl.name} as 'Controller'");
        }
        
        if (rightHandCtrl != null)
        {
            rightHandCtrl.tag = "Controller";
            Debug.Log($"Tagged {rightHandCtrl.name} as 'Controller'");
        }
        
        Debug.Log("VR hand tagging complete.");
    }
    
    /// <summary>
    /// Setup VR hand interactions using the same pattern as BongoTask
    /// </summary>
    private void SetupVRHandInteractions()
    {
        Debug.Log("Setting up VR hand interactions...");
        
        // Set up dock interaction with VR hands (same as BongoTask)
        if (dock != null)
        {
            dock.GetComponent<Target>().SetProjectile(directRight);
            Debug.Log($"Dock projectile set to: {directRight?.name}");
            
            // Note: Target component only supports one projectile, but we could potentially
            // add a second Target component or modify the approach for both hands
            if (directLeft != null)
            {
                Debug.Log($"Note: Dock only supports one hand (right). Left hand: {directLeft.name} not registered with dock.");
            }
        }
        
        // Set up button interactions with VR hands
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Debug.Log($"Setting up button {i}: {buttonObjects[i].name}");
                
                // Find the Goal Mesh child object (where MultipleTarget should be)
                Transform goalMeshTransform = buttonObjects[i].transform.Find("LOGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("LIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("RIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("ROGoalMesh");
                
                if (goalMeshTransform == null)
                {
                    // Try to find any child with "GoalMesh" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("GoalMesh"))
                        {
                            goalMeshTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalMeshTransform != null)
                {
                    // Debug.Log($"Found Goal Mesh: {goalMeshTransform.name}");
                    
                    // Add MultipleTarget component to the Goal Mesh object
                    MultipleTarget multipleTarget = goalMeshTransform.GetComponent<MultipleTarget>();
                    if (multipleTarget == null)
                    {
                        multipleTarget = goalMeshTransform.gameObject.AddComponent<MultipleTarget>();
                        // Debug.Log($"Added MultipleTarget component to {goalMeshTransform.name}");
                    }
                    else
                    {
                        // Debug.Log($"Goal Mesh already has MultipleTarget component");
                    }
                    
                    // Clear existing tools and add VR hands as tools
                    multipleTarget.tools.Clear();
                    
                    if (directRight != null)
                    {
                        multipleTarget.tools.Add(directRight);
                        // Debug.Log($"Added {directRight.name} as tool to {goalMeshTransform.name}");
                    }
                    else
                    {
                        // Debug.LogWarning($"directRight is null - cannot add to {goalMeshTransform.name}");
                    }
                    
                    if (directLeft != null)
                    {
                        multipleTarget.tools.Add(directLeft);
                        // Debug.Log($"Added {directLeft.name} as tool to {goalMeshTransform.name}");
                    }
                    else
                    {
                        // Debug.LogWarning($"directLeft is null - cannot add to {goalMeshTransform.name}");
                    }
                    
                    // Debug.Log($"Goal Mesh {goalMeshTransform.name} now has {multipleTarget.tools.Count} tools registered");
                    
                    // Check colliders on the Goal Mesh object
                    EnsureButtonHasCollider(goalMeshTransform.gameObject, i);
                }
                else
                {
                    Debug.LogError($"Could not find Goal Mesh child object for button {i} ({buttonObjects[i].name})");
                    
                    // Fallback: add MultipleTarget to the main button object
                    MultipleTarget multipleTarget = buttonObjects[i].GetComponent<MultipleTarget>();
                    if (multipleTarget == null)
                    {
                        multipleTarget = buttonObjects[i].AddComponent<MultipleTarget>();
                        Debug.Log($"Added MultipleTarget component to main button {i} as fallback");
                    }
                    
                    // Clear existing tools and add VR hands as tools
                    multipleTarget.tools.Clear();
                    
                    if (directRight != null)
                    {
                        multipleTarget.tools.Add(directRight);
                    }
                    if (directLeft != null)
                    {
                        multipleTarget.tools.Add(directLeft);
                    }
                    
                    Debug.Log($"Main button {i} now has {multipleTarget.tools.Count} tools registered (fallback)");
                }
            }
            else
            {
                Debug.LogWarning($"Button {i} is null!");
            }
        }
        
        Debug.Log("VR hand interactions setup complete.");
        
        // Debug: Log the final state of all button MultipleTarget components
        LogButtonMultipleTargetStates();
    }
    
    /// <summary>
    /// Debug method to log the current state of all button MultipleTarget components
    /// </summary>
    private void LogButtonMultipleTargetStates()
    {
        Debug.Log("=== BUTTON MULTIPLE TARGET STATES ===");
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                Debug.Log($"Button {i} ({buttonObjects[i].name}):");
                
                // Find the Goal Mesh child object
                Transform goalMeshTransform = buttonObjects[i].transform.Find("LOGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("LIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("RIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("ROGoalMesh");
                
                if (goalMeshTransform == null)
                {
                    // Try to find any child with "GoalMesh" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("GoalMesh"))
                        {
                            goalMeshTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalMeshTransform != null)
                {
                    Target target = goalMeshTransform.GetComponent<Target>();
                    if (target != null)
                    {
                        Debug.Log($"  GoalMesh: {goalMeshTransform.name}");
                        Debug.Log($"  Target enabled: {target.enabled}");
                        Debug.Log($"  TargetHit: {target.TargetHit}");
                        Debug.Log($"  IsColliding: {target.IsColliding}");
                        
                        // Check colliders on the GoalMesh
                        Collider[] colliders = goalMeshTransform.GetComponents<Collider>();
                        Debug.Log($"  Colliders on GoalMesh: {colliders.Length}");
                        foreach (var collider in colliders)
                        {
                            // Debug.Log($"    Collider: {collider.GetType().Name}, isTrigger: {collider.isTrigger}, enabled: {collider.enabled}");
                        }
                        
                        // Check for Rigidbody (needed for collision detection)
                        Rigidbody rb = goalMeshTransform.GetComponent<Rigidbody>();
                        Debug.Log($"  Rigidbody: {(rb != null ? "Present" : "MISSING")}");
                        if (rb != null)
                        {
                            Debug.Log($"    Rigidbody: isKinematic={rb.isKinematic}, useGravity={rb.useGravity}");
                        }
                        
                        // Check layer
                        Debug.Log($"  Layer: {goalMeshTransform.gameObject.layer} ({LayerMask.LayerToName(goalMeshTransform.gameObject.layer)})");
                    }
                    else
                    {
                        Debug.LogWarning($"  GoalMesh {goalMeshTransform.name} has no Target component!");
                    }
                }
                else
                {
                    Debug.LogWarning($"  No GoalMesh found for button {i}");
                }
            }
        }
        Debug.Log("=== END BUTTON STATES ===");
    }
    
    /// <summary>
    /// Manual test method to check button collision detection (for debugging)
    /// </summary>
    [ContextMenu("Test Button Collision Detection")]
    private void TestButtonCollisionDetection()
    {
        Debug.Log("=== MANUAL BUTTON COLLISION TEST ===");
        
        if (!ExperimentController.Instance.UseVR)
        {
            Debug.LogWarning("This test is only for VR mode!");
            return;
        }
        
        // Log current VR hand positions and configuration
        if (directRight != null)
        {
            Debug.Log($"Right hand position: {directRight.transform.position}");
            Debug.Log($"Right hand layer: {directRight.layer} ({LayerMask.LayerToName(directRight.layer)})");
            
            // Check colliders on VR hand
            Collider[] rightColliders = directRight.GetComponents<Collider>();
            Debug.Log($"Right hand colliders: {rightColliders.Length}");
            foreach (var collider in rightColliders)
            {
                Debug.Log($"  Right hand collider: {collider.GetType().Name}, isTrigger: {collider.isTrigger}, enabled: {collider.enabled}");
            }
            
            // Check Rigidbody on VR hand
            Rigidbody rightRB = directRight.GetComponent<Rigidbody>();
            Debug.Log($"Right hand Rigidbody: {(rightRB != null ? "Present" : "MISSING")}");
            if (rightRB != null)
            {
                Debug.Log($"  Right hand Rigidbody: isKinematic={rightRB.isKinematic}, useGravity={rightRB.useGravity}");
            }
        }
        if (directLeft != null)
        {
            Debug.Log($"Left hand position: {directLeft.transform.position}");
            Debug.Log($"Left hand layer: {directLeft.layer} ({LayerMask.LayerToName(directLeft.layer)})");
            
            // Check colliders on VR hand
            Collider[] leftColliders = directLeft.GetComponents<Collider>();
            Debug.Log($"Left hand colliders: {leftColliders.Length}");
            foreach (var collider in leftColliders)
            {
                Debug.Log($"  Left hand collider: {collider.GetType().Name}, isTrigger: {collider.isTrigger}, enabled: {collider.enabled}");
            }
            
            // Check Rigidbody on VR hand
            Rigidbody leftRB = directLeft.GetComponent<Rigidbody>();
            Debug.Log($"Left hand Rigidbody: {(leftRB != null ? "Present" : "MISSING")}");
            if (leftRB != null)
            {
                Debug.Log($"  Left hand Rigidbody: isKinematic={leftRB.isKinematic}, useGravity={leftRB.useGravity}");
            }
        }
        
        // Check each button's collision state
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Find the Goal Mesh child object
                Transform goalMeshTransform = buttonObjects[i].transform.Find("LOGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("LIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("RIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("ROGoalMesh");
                
                if (goalMeshTransform == null)
                {
                    // Try to find any child with "GoalMesh" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("GoalMesh"))
                        {
                            goalMeshTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalMeshTransform != null)
                {
                    MultipleTarget multipleTarget = goalMeshTransform.GetComponent<MultipleTarget>();
                    if (multipleTarget != null)
                    {
                        Debug.Log($"Button {i} ({goalMeshTransform.name}):");
                        Debug.Log($"  Position: {goalMeshTransform.position}");
                        Debug.Log($"  IsToolColliding: {multipleTarget.IsToolCollding}");
                        Debug.Log($"  Tools count: {multipleTarget.tools.Count}");
                        
                        // Calculate distances to VR hands
                        if (directRight != null)
                        {
                            float distanceToRight = Vector3.Distance(directRight.transform.position, goalMeshTransform.position);
                            Debug.Log($"  Distance to right hand: {distanceToRight:F3}");
                        }
                        if (directLeft != null)
                        {
                            float distanceToLeft = Vector3.Distance(directLeft.transform.position, goalMeshTransform.position);
                            Debug.Log($"  Distance to left hand: {distanceToLeft:F3}");
                        }
                    }
                }
            }
        }
        
        Debug.Log("=== END MANUAL TEST ===");
    }
    
    /// <summary>
    /// Automatically setup VR hand interactions at the start of each trial
    /// </summary>
    private void AutoSetupVRHandInteractions()
    {
        Debug.Log("Auto-setting up VR hand interactions...");
        
        // Ensure VR components are found
        if (directRight == null)
        {
            directRight = GameObject.Find("RH Direct Interactor");
            if (directRight == null) directRight = GameObject.Find("RightHand Direct Interactor");
            if (directRight == null) directRight = GameObject.Find("Right Direct Interactor");
        }
        
        if (directLeft == null)
        {
            directLeft = GameObject.Find("LH Direct Interactor");
            if (directLeft == null) directLeft = GameObject.Find("LeftHand Direct Interactor");
            if (directLeft == null) directLeft = GameObject.Find("Left Direct Interactor");
        }
        
        Debug.Log($"VR Hands found - Right: {(directRight != null ? directRight.name : "NOT FOUND")}, Left: {(directLeft != null ? directLeft.name : "NOT FOUND")}");
        
        // Setup dock interaction
        if (dock != null && directRight != null)
        {
            dock.GetComponent<Target>().SetProjectile(directRight);
            Debug.Log($"Dock projectile set to: {directRight.name}");
        }
        
        // Setup button interactions
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Find the Goal Collider child object (this has the trigger collider and Rigidbody)
                Transform goalColliderTransform = buttonObjects[i].transform.Find("LO Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("LI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RO Goal Collider");
                
                if (goalColliderTransform == null)
                {
                    // Try to find any child with "Goal Collider" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("Goal Collider"))
                        {
                            goalColliderTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalColliderTransform != null)
                {
                    // Use MultipleTarget component on the Goal Collider (same as BongoTask)
                    MultipleTarget multipleTarget = goalColliderTransform.GetComponent<MultipleTarget>();
                    if (multipleTarget == null)
                    {
                        multipleTarget = goalColliderTransform.gameObject.AddComponent<MultipleTarget>();
                        Debug.Log($"Added MultipleTarget component to {goalColliderTransform.name}");
                    }
                    
                    // Clear and add VR hands as tools (same as BongoTask)
                    multipleTarget.tools.Clear();
                    
                    if (directRight != null)
                    {
                        multipleTarget.tools.Add(directRight);
                        // Debug.Log($"Added {directRight.name} as tool to {goalColliderTransform.name}");
                    }
                    
                    if (directLeft != null)
                    {
                        multipleTarget.tools.Add(directLeft);
                        // Debug.Log($"Added {directLeft.name} as tool to {goalColliderTransform.name}");
                    }
                    
                    // Debug.Log($"Goal Collider {goalColliderTransform.name} now has {multipleTarget.tools.Count} tools registered");
                    
                    // Enable capsule collider for VR mode (disable for 2D cursor mode)
                    CapsuleCollider capsuleCollider = goalColliderTransform.GetComponent<CapsuleCollider>();
                    if (capsuleCollider != null)
                    {
                        capsuleCollider.enabled = true; // Enable for VR mode
                        Debug.Log($"Enabled capsule collider on {goalColliderTransform.name} for VR mode");
                    }
                    else
                    {
                        Debug.LogWarning($"No capsule collider found on {goalColliderTransform.name}");
                    }
                    
                    // Ensure proper collider setup
                    EnsureButtonHasCollider(goalColliderTransform.gameObject, i);
                }
                else
                {
                    Debug.LogWarning($"Could not find Goal Collider for button {i} ({buttonObjects[i].name})");
                }
            }
        }
        
        Debug.Log("Auto VR hand interactions setup complete.");
    }
    
    /// <summary>
    /// Disable capsule colliders for 2D cursor mode
    /// </summary>
    private void DisableCapsuleCollidersFor2D()
    {
        Debug.Log("Disabling capsule colliders for 2D cursor mode...");
        
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Find the Goal Collider child object
                Transform goalColliderTransform = buttonObjects[i].transform.Find("LO Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("LI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RO Goal Collider");
                
                if (goalColliderTransform == null)
                {
                    // Try to find any child with "Goal Collider" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("Goal Collider"))
                        {
                            goalColliderTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalColliderTransform != null)
                {
                    // Disable capsule collider for 2D mode
                    CapsuleCollider capsuleCollider = goalColliderTransform.GetComponent<CapsuleCollider>();
                    if (capsuleCollider != null)
                    {
                        capsuleCollider.enabled = false;
                        Debug.Log($"Disabled capsule collider on {goalColliderTransform.name} for 2D cursor mode");
                    }
                }
            }
        }
        
        Debug.Log("Capsule colliders disabled for 2D cursor mode.");
    }
    
    /// <summary>
    /// Start trial with a delay to prevent VR hand clipping with buttons
    /// </summary>
    private IEnumerator DelayedStartTrial(float delay)
    {
        Debug.Log($"Waiting {delay} seconds before starting trial to prevent hand clipping...");
        yield return new WaitForSeconds(delay);
        Debug.Log("Delay complete - starting trial now");
        StartTrial();
    }
    
    /// <summary>
    /// Manual method to force VR hand setup (for debugging)
    /// </summary>
    [ContextMenu("Force VR Hand Setup")]
    private void ForceVRHandSetup()
    {
        Debug.Log("=== FORCING VR HAND SETUP ===");
        
        if (!ExperimentController.Instance.UseVR)
        {
            Debug.LogWarning("Not in VR mode! This method only works in VR mode.");
            return;
        }
        
        // Force find VR hands again
        directRight = GameObject.Find("RH Direct Interactor");
        if (directRight == null) directRight = GameObject.Find("RightHand Direct Interactor");
        if (directRight == null) directRight = GameObject.Find("Right Direct Interactor");
        
        directLeft = GameObject.Find("LH Direct Interactor");
        if (directLeft == null) directLeft = GameObject.Find("LeftHand Direct Interactor");
        if (directLeft == null) directLeft = GameObject.Find("Left Direct Interactor");
        
        Debug.Log($"Force found VR hands - Right: {(directRight != null ? directRight.name : "NOT FOUND")}, Left: {(directLeft != null ? directLeft.name : "NOT FOUND")}");
        
        // Force setup
        AutoSetupVRHandInteractions();
        
        Debug.Log("=== FORCE VR HAND SETUP COMPLETE ===");
    }
    
    /// <summary>
    /// Fix common collision detection issues by adding Rigidbodies if needed
    /// </summary>
    [ContextMenu("Fix Collision Detection Issues")]
    private void FixCollisionDetectionIssues()
    {
        Debug.Log("=== FIXING COLLISION DETECTION ISSUES ===");
        
        if (!ExperimentController.Instance.UseVR)
        {
            Debug.LogWarning("This fix is only for VR mode!");
            return;
        }
        
        int fixedCount = 0;
        
        // Fix VR hands - add Rigidbodies if missing
        if (directRight != null)
        {
            Rigidbody rightRB = directRight.GetComponent<Rigidbody>();
            if (rightRB == null)
            {
                rightRB = directRight.AddComponent<Rigidbody>();
                rightRB.isKinematic = true; // Kinematic for VR hands
                rightRB.useGravity = false;
                Debug.Log($"Added Rigidbody to right hand: {directRight.name}");
                fixedCount++;
            }
        }
        
        if (directLeft != null)
        {
            Rigidbody leftRB = directLeft.GetComponent<Rigidbody>();
            if (leftRB == null)
            {
                leftRB = directLeft.AddComponent<Rigidbody>();
                leftRB.isKinematic = true; // Kinematic for VR hands
                leftRB.useGravity = false;
                Debug.Log($"Added Rigidbody to left hand: {directLeft.name}");
                fixedCount++;
            }
        }
        
        // Fix button GoalMesh objects - add Rigidbodies if missing
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Find the Goal Mesh child object
                Transform goalMeshTransform = buttonObjects[i].transform.Find("LOGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("LIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("RIGoalMesh") ?? 
                                            buttonObjects[i].transform.Find("ROGoalMesh");
                
                if (goalMeshTransform == null)
                {
                    // Try to find any child with "GoalMesh" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("GoalMesh"))
                        {
                            goalMeshTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalMeshTransform != null)
                {
                    Rigidbody goalRB = goalMeshTransform.GetComponent<Rigidbody>();
                    if (goalRB == null)
                    {
                        goalRB = goalMeshTransform.gameObject.AddComponent<Rigidbody>();
                        goalRB.isKinematic = true; // Kinematic for static buttons
                        goalRB.useGravity = false;
                        Debug.Log($"Added Rigidbody to GoalMesh: {goalMeshTransform.name}");
                        fixedCount++;
                    }
                    
                    // Ensure colliders are not triggers (for collision detection)
                    Collider[] colliders = goalMeshTransform.GetComponents<Collider>();
                    foreach (var collider in colliders)
                    {
                        if (collider.isTrigger)
                        {
                            collider.isTrigger = false;
                            Debug.Log($"Changed {goalMeshTransform.name} collider from trigger to collision");
                            fixedCount++;
                        }
                    }
                }
            }
        }
        
        Debug.Log($"=== FIXED {fixedCount} COLLISION DETECTION ISSUES ===");
        
        if (fixedCount > 0)
        {
            Debug.Log("Please test the VR interaction again. The collision detection should now work properly.");
        }
        else
        {
            Debug.Log("No collision detection issues found. The problem might be elsewhere.");
        }
    }
    
    /// <summary>
    /// Ensure button has proper collider for VR interaction
    /// </summary>
    private void EnsureButtonHasCollider(GameObject button, int buttonIndex)
    {
        if (button == null) return;
        
        // Check if button has any collider
        Collider[] colliders = button.GetComponents<Collider>();
        Debug.Log($"Button {buttonIndex} ({button.name}) has {colliders.Length} colliders");
        
        if (colliders.Length == 0)
        {
            // Add a box collider if no collider exists
            BoxCollider boxCollider = button.AddComponent<BoxCollider>();
            boxCollider.isTrigger = false; // Make it a solid collider for VR interaction
            Debug.Log($"Added BoxCollider to button {buttonIndex}");
        }
        else
        {
            // Log existing collider details
            foreach (var collider in colliders)
            {
                // Debug.Log($"  Collider: {collider.GetType().Name}, isTrigger: {collider.isTrigger}, enabled: {collider.enabled}");
            }
        }
    }
    
    /// <summary>
    /// Check for VR hand button interactions using the MultipleTarget system (same as BongoTask)
    /// </summary>
    private void CheckVRButtonInteractions()
    {
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                // Find the Goal Collider child object
                Transform goalColliderTransform = buttonObjects[i].transform.Find("LO Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("LI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RI Goal Collider") ?? 
                                                buttonObjects[i].transform.Find("RO Goal Collider");
                
                if (goalColliderTransform == null)
                {
                    // Try to find any child with "Goal Collider" in the name
                    foreach (Transform child in buttonObjects[i].transform)
                    {
                        if (child.name.Contains("Goal Collider"))
                        {
                            goalColliderTransform = child;
                            break;
                        }
                    }
                }
                
                if (goalColliderTransform != null)
                {
                    MultipleTarget multipleTarget = goalColliderTransform.GetComponent<MultipleTarget>();
                    if (multipleTarget != null)
                    {
                        // Check if VR hand is colliding with button (same as BongoTask but only check IsToolCollding)
                        if (multipleTarget.IsToolCollding)
                        {
                            // VR hand is colliding with button - trigger response
                            string buttonLabel = buttonTexts[i].text;
                            // Debug.Log($"VR hand hit button: {buttonLabel}");
                            OnButtonResponse(buttonLabel);
                            break; // Only process one button at a time
                        }
                        
                    }
                    else
                    {
                        // Debug.LogWarning($"Goal Collider {goalColliderTransform.name} has no MultipleTarget component!");
                    }
                }
            }
        }
    }

    private string GetColorName(Color color)
    {
        if (color == Color.red) return "red";
        if (color == Color.blue) return "blue";
        if (color == Color.green) return "green";
        if (color == Color.yellow) return "yellow";
        if (color == Color.black) return "black";
        if (color == Color.white) return "white";
        if (color == Color.magenta) return "purple";
        if (color == new Color(1f, 0.5f, 0f)) return "orange";
        return "unknown";
    }

    public override void TaskEnd()
    {
        // Hide UI elements
        if (wordDisplayCanvas != null)
            wordDisplayCanvas.SetActive(false);
        if (buttonContainer != null)
            buttonContainer.SetActive(false);
        
        // Clean up
        base.TaskEnd();
    }

    public override void LogParameters()
    {
        Session session = ExperimentController.Instance.Session;

        // Basic trial information - using ExperimentController for trial/block data
        session.CurrentTrial.result["block_number"] = ExperimentController.Instance.Session.currentBlockNum;
        session.CurrentTrial.result["trial_in_block"] = ExperimentController.Instance.Session.CurrentTrial.numberInBlock;
        session.CurrentTrial.result["total_correct"] = totalCorrect;
        session.CurrentTrial.result["total_score"] = totalScore;
        session.CurrentTrial.result["accuracy_percentage"] = completedTrials > 0 ? (float)totalCorrect / completedTrials * 100f : 0f;
        session.CurrentTrial.result["average_reaction_time"] = completedTrials > 0 ? totalReactionTime / completedTrials : 0f;
        session.CurrentTrial.result["total_time"] = endTime - startTime;
        
        // Block-specific data (resets each block)
        session.CurrentTrial.result["block_total_correct"] = blockCorrect;
        session.CurrentTrial.result["block_total_trials"] = blockReactionTimes.Count;
        session.CurrentTrial.result["block_accuracy_percentage"] = blockReactionTimes.Count > 0 ? (float)blockCorrect / blockReactionTimes.Count * 100f : 0f;
        session.CurrentTrial.result["block_average_reaction_time"] = blockReactionTimes.Count > 0 ? blockTotalReactionTime / blockReactionTimes.Count : 0f;
        session.CurrentTrial.result["block_total_time"] = Time.time - blockStartTime;
        
        // Block-specific trial data (arrays for this block only)
        session.CurrentTrial.result["block_presented_words"] = string.Join(",", blockPresentedWords);
        session.CurrentTrial.result["block_presented_colors"] = string.Join(",", blockPresentedColors);
        session.CurrentTrial.result["block_presented_directions"] = string.Join(",", blockPresentedDirections);
        session.CurrentTrial.result["block_correct_answers"] = string.Join(",", blockCorrectAnswers);
        session.CurrentTrial.result["block_participant_responses"] = string.Join(",", blockParticipantResponses);
        session.CurrentTrial.result["block_reaction_times"] = string.Join(",", blockReactionTimes.Select(rt => rt.ToString("F3")));
        session.CurrentTrial.result["block_correct_responses"] = string.Join(",", blockCorrectResponses.Select(cr => cr.ToString()));

        // Controller information
        if (ExperimentController.Instance.UseVR)
        {
            session.CurrentTrial.result["controller_type"] = "vr_controller";
            if (vrPos != null)
            {
                session.CurrentTrial.result["participant_spawn_location_x"] = vrPos.transform.position.x;
                session.CurrentTrial.result["participant_spawn_location_y"] = vrPos.transform.position.y;
                session.CurrentTrial.result["participant_spawn_location_z"] = vrPos.transform.position.z;
            }
        }
        else
        {
            session.CurrentTrial.result["controller_type"] = "mouse";
        }

        // Hand tracking data
        session.CurrentTrial.result["hand"] = string.Join(",", hittingHand);
        session.CurrentTrial.result["left_hand_pos_x"] = string.Join(",", leftHandPos.Select(i => string.Format($"{i.x}")));
        session.CurrentTrial.result["left_hand_pos_y"] = string.Join(",", leftHandPos.Select(i => string.Format($"{i.y}")));
        session.CurrentTrial.result["left_hand_pos_z"] = string.Join(",", leftHandPos.Select(i => string.Format($"{i.z}")));
        session.CurrentTrial.result["right_hand_pos_x"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.x}")));
        session.CurrentTrial.result["right_hand_pos_y"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.y}")));
        session.CurrentTrial.result["right_hand_pos_z"] = string.Join(",", rightHandPos.Select(i => string.Format($"{i.z}")));

        // Trial-by-trial data
        session.CurrentTrial.result["presented_words"] = string.Join(",", presentedWords);
        session.CurrentTrial.result["presented_colors"] = string.Join(",", presentedColors);
        session.CurrentTrial.result["presented_directions"] = string.Join(",", presentedDirections);
        session.CurrentTrial.result["correct_answers"] = string.Join(",", correctAnswers);
        session.CurrentTrial.result["participant_responses"] = string.Join(",", participantResponses);
        session.CurrentTrial.result["reaction_times"] = string.Join(",", reactionTimes.Select(rt => rt.ToString("F3")));
        session.CurrentTrial.result["correct_responses"] = string.Join(",", correctResponses.Select(cr => cr.ToString()));

        // Block type information - get from actual block settings
        string blockType = "congruent"; // default
        try
        {
            var blockSettings = ExperimentController.Instance.Session.CurrentBlock.settings;
            try
            {
                blockType = blockSettings.GetString("block_type");
            }
            catch
            {
                try
                {
                    string targetLocation = blockSettings.GetString("target_location");
                    if (targetLocation != null && (targetLocation == "congruent" || targetLocation == "incongruent"))
                    {
                        blockType = targetLocation;
                    }
                }
                catch
                {
                    // Fallback to alternating pattern
                    int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
                    blockType = (blockNumber % 2 == 0) ? "incongruent" : "congruent";
                }
            }
        }
        catch
        {
            // Fallback to alternating pattern
            int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
            blockType = (blockNumber % 2 == 0) ? "incongruent" : "congruent";
        }
        session.CurrentTrial.result["block_type"] = blockType;
        
        // Current trial data (if available)
        if (presentedWords.Count > 0)
        {
            session.CurrentTrial.result["current_word"] = presentedWords[presentedWords.Count - 1];
            session.CurrentTrial.result["current_color"] = presentedColors[presentedColors.Count - 1];
            session.CurrentTrial.result["current_direction"] = presentedDirections[presentedDirections.Count - 1];
            session.CurrentTrial.result["current_correct_answer"] = correctAnswers[correctAnswers.Count - 1];
            session.CurrentTrial.result["current_participant_response"] = participantResponses[participantResponses.Count - 1];
            session.CurrentTrial.result["current_reaction_time"] = reactionTimes[reactionTimes.Count - 1];
            session.CurrentTrial.result["current_correct"] = correctResponses[correctResponses.Count - 1];
        }
        
        // Additional analysis data
        session.CurrentTrial.result["total_trials_completed"] = completedTrials;
        session.CurrentTrial.result["total_incorrect"] = completedTrials - totalCorrect;
        session.CurrentTrial.result["error_rate"] = completedTrials > 0 ? (float)(completedTrials - totalCorrect) / completedTrials * 100f : 0f;
        
        // Reaction time statistics
        if (reactionTimes.Count > 0)
        {
            session.CurrentTrial.result["min_reaction_time"] = reactionTimes.Min();
            session.CurrentTrial.result["max_reaction_time"] = reactionTimes.Max();
            session.CurrentTrial.result["median_reaction_time"] = reactionTimes.OrderBy(x => x).Skip(reactionTimes.Count / 2).First();
        }
        
        // Block-specific reaction time statistics
        if (blockReactionTimes.Count > 0)
        {
            session.CurrentTrial.result["block_min_reaction_time"] = blockReactionTimes.Min();
            session.CurrentTrial.result["block_max_reaction_time"] = blockReactionTimes.Max();
            session.CurrentTrial.result["block_median_reaction_time"] = blockReactionTimes.OrderBy(x => x).Skip(blockReactionTimes.Count / 2).First();
        }
        
        // Congruency analysis
        int congruentCorrect = 0, congruentTotal = 0;
        int incongruentCorrect = 0, incongruentTotal = 0;
        
        for (int i = 0; i < presentedWords.Count && i < correctResponses.Count; i++)
        {
            bool isCongruent = presentedWords[i] == presentedColors[i];
            if (isCongruent)
            {
                congruentTotal++;
                if (correctResponses[i]) congruentCorrect++;
            }
            else
            {
                incongruentTotal++;
                if (correctResponses[i]) incongruentCorrect++;
            }
        }
        
        session.CurrentTrial.result["congruent_trials"] = congruentTotal;
        session.CurrentTrial.result["congruent_correct"] = congruentCorrect;
        session.CurrentTrial.result["congruent_accuracy"] = congruentTotal > 0 ? (float)congruentCorrect / congruentTotal * 100f : 0f;
        session.CurrentTrial.result["incongruent_trials"] = incongruentTotal;
        session.CurrentTrial.result["incongruent_correct"] = incongruentCorrect;
        session.CurrentTrial.result["incongruent_accuracy"] = incongruentTotal > 0 ? (float)incongruentCorrect / incongruentTotal * 100f : 0f;
        
        // Stroop effect calculation
        float stroopEffect = 0f;
        if (congruentTotal > 0 && incongruentTotal > 0)
        {
            float congruentRT = 0f, incongruentRT = 0f;
            int congruentRTCount = 0, incongruentRTCount = 0;
            
            for (int i = 0; i < presentedWords.Count && i < reactionTimes.Count; i++)
            {
                bool isCongruent = presentedWords[i] == presentedColors[i];
                if (isCongruent)
                {
                    congruentRT += reactionTimes[i];
                    congruentRTCount++;
                }
                else
                {
                    incongruentRT += reactionTimes[i];
                    incongruentRTCount++;
                }
            }
            
            if (congruentRTCount > 0 && incongruentRTCount > 0)
            {
                stroopEffect = (incongruentRT / incongruentRTCount) - (congruentRT / congruentRTCount);
            }
        }
        session.CurrentTrial.result["stroop_effect_rt"] = stroopEffect;
        
        // Block-specific congruency analysis
        int blockCongruentCorrect = 0, blockCongruentTotal = 0;
        int blockIncongruentCorrect = 0, blockIncongruentTotal = 0;
        
        for (int i = 0; i < blockPresentedWords.Count && i < blockCorrectResponses.Count; i++)
        {
            bool isCongruent = blockPresentedWords[i] == blockPresentedColors[i];
            if (isCongruent)
            {
                blockCongruentTotal++;
                if (blockCorrectResponses[i]) blockCongruentCorrect++;
            }
            else
            {
                blockIncongruentTotal++;
                if (blockCorrectResponses[i]) blockIncongruentCorrect++;
            }
        }
        
        session.CurrentTrial.result["block_congruent_trials"] = blockCongruentTotal;
        session.CurrentTrial.result["block_congruent_correct"] = blockCongruentCorrect;
        session.CurrentTrial.result["block_congruent_accuracy"] = blockCongruentTotal > 0 ? (float)blockCongruentCorrect / blockCongruentTotal * 100f : 0f;
        session.CurrentTrial.result["block_incongruent_trials"] = blockIncongruentTotal;
        session.CurrentTrial.result["block_incongruent_correct"] = blockIncongruentCorrect;
        session.CurrentTrial.result["block_incongruent_accuracy"] = blockIncongruentTotal > 0 ? (float)blockIncongruentCorrect / blockIncongruentTotal * 100f : 0f;
        
        // Block-specific Stroop effect calculation
        float blockStroopEffect = 0f;
        if (blockCongruentTotal > 0 && blockIncongruentTotal > 0)
        {
            float blockCongruentRT = 0f, blockIncongruentRT = 0f;
            int blockCongruentRTCount = 0, blockIncongruentRTCount = 0;
            
            for (int i = 0; i < blockPresentedWords.Count && i < blockReactionTimes.Count; i++)
            {
                bool isCongruent = blockPresentedWords[i] == blockPresentedColors[i];
                if (isCongruent)
                {
                    blockCongruentRT += blockReactionTimes[i];
                    blockCongruentRTCount++;
                }
                else
                {
                    blockIncongruentRT += blockReactionTimes[i];
                    blockIncongruentRTCount++;
                }
            }
            
            if (blockCongruentRTCount > 0 && blockIncongruentRTCount > 0)
            {
                blockStroopEffect = (blockIncongruentRT / blockIncongruentRTCount) - (blockCongruentRT / blockCongruentRTCount);
            }
        }
        session.CurrentTrial.result["block_stroop_effect_rt"] = blockStroopEffect;
    }

    private void UpdateScoreboard()
    {
        // Find child objects (requires proper hierarchy structure)
        TextMeshProUGUI scoreText = Scoreboard.transform.Find("ScoreTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trialText = Scoreboard.transform.Find("TrialTXT").GetComponent<TextMeshProUGUI>();

        int accuracy = completedTrials > 0 ? (int)((float)totalCorrect / completedTrials * 100) : 0;
        float avgRT = completedTrials > 0 ? totalReactionTime / completedTrials : 0f;
        
        // Get block type from actual block settings
        string blockType = "Congruent"; // default
        try
        {
            var blockSettings = ExperimentController.Instance.Session.CurrentBlock.settings;
            try
            {
                string blockTypeLower = blockSettings.GetString("block_type");
                blockType = char.ToUpper(blockTypeLower[0]) + blockTypeLower.Substring(1);
            }
            catch
            {
                try
                {
                    string targetLocation = blockSettings.GetString("target_location");
                    if (targetLocation != null && (targetLocation == "congruent" || targetLocation == "incongruent" || targetLocation == "direction_same" || targetLocation == "direction_opposite"))
                    {
                        blockType = char.ToUpper(targetLocation[0]) + targetLocation.Substring(1);
                    }
                }
                catch
                {
                    // Fallback to alternating pattern
                    int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
                    blockType = (blockNumber % 2 == 0) ? "Incongruent" : "Congruent";
                }
            }
        }
        catch
        {
            // Fallback to alternating pattern
            int blockNumber = ExperimentController.Instance.Session.currentBlockNum;
            blockType = (blockNumber % 2 == 0) ? "Incongruent" : "Congruent";
        }

        // Update the Text fields using ExperimentController data
        scoreText.text = $"Score: {totalScore}";
        trialText.text = $"Block: {ExperimentController.Instance.Session.currentBlockNum} ({blockType})\n" +
                         $"Trial: {ExperimentController.Instance.Session.CurrentTrial.numberInBlock}\n" +
                         $"Accuracy: {accuracy}%\n" +
                         $"Avg RT: {avgRT:F3}s";
    }

    // Method to handle button responses from the button objects
    public void OnButtonResponse(string response)
    {
        Debug.Log($"OnButtonResponse called with: {response}, trialActive: {trialActive}, responseGiven: {responseGiven}");
        
        if (!trialActive || responseGiven)
        {
            Debug.LogWarning("OnButtonResponse ignored - trial not active or response already given");
            return;
        }

        responseGiven = true;
        trialActive = false;

        // Deactivate buttons to prevent multiple responses
        ActivateButtons(false);

        // Calculate reaction time
        reactionTime = Time.time - trialStartTime;
        totalReactionTime += reactionTime;

        // Check if response is correct
        bool isCorrect = response == correctAnswer;
        Debug.Log($"Response comparison: '{response}' == '{correctAnswer}' = {isCorrect}");
        
        // Play audio feedback based on correctness
        if (audioSource != null)
        {
            if (isCorrect && correctSFX != null)
            {
                audioSource.clip = correctSFX;
                audioSource.Play();
                Debug.Log("Played correct sound");
            }
            else if (!isCorrect && incorrectSFX != null)
            {
                audioSource.clip = incorrectSFX;
                audioSource.Play();
                Debug.Log("Played incorrect sound");
            }
        }
        
        if (isCorrect)
        {
            totalCorrect++;
            totalScore += Mathf.Max(0, 100 - Mathf.RoundToInt(reactionTime * 100));
        }

        // Store trial data (overall)
        participantResponses.Add(response);
        reactionTimes.Add(reactionTime);
        correctResponses.Add(isCorrect);
        
        // Store trial data (block-specific)
        blockParticipantResponses.Add(response);
        blockReactionTimes.Add(reactionTime);
        blockCorrectResponses.Add(isCorrect);
        blockPresentedWords.Add(currentWord);
        blockPresentedColors.Add(GetColorName(currentColor));
        blockPresentedDirections.Add(currentDirection);
        blockCorrectAnswers.Add(correctAnswer);
        
        // Update block-specific counters
        if (isCorrect)
        {
            blockCorrect++;
        }
        blockTotalReactionTime += reactionTime;

        // Log hand positions at response time
        if (leftHand != null)
            leftHandPos.Add(leftHand.transform.position);
        if (rightHand != null)
            rightHandPos.Add(rightHand.transform.position);
        hittingHand.Add(ExperimentController.Instance.UseVR ? "vr_hand" : "mouse");

        completedTrials++;

        Debug.Log($"Response: {response}, Correct: {isCorrect}, RT: {reactionTime:F3}s");

        // Update scoreboard
        UpdateScoreboard();

        // Mark trial as completed in UXF system
        ExperimentController.Instance.Session.CurrentTrial.result["completed"] = true;
        ExperimentController.Instance.Session.CurrentTrial.result["response"] = response;
        ExperimentController.Instance.Session.CurrentTrial.result["correct"] = isCorrect;
        ExperimentController.Instance.Session.CurrentTrial.result["reaction_time"] = reactionTime;

        // Complete the trial
        StartCoroutine(CompleteTrial());
    }
    
    /// <summary>
    /// Update cursor Y position based on left mouse button state
    /// </summary>
    private void UpdateCursorPosition()
    {
        if (cursor == null) return;
        
        float targetY = isLeftMouseHeld ? 0f : originalCursorY;
        Vector3 currentPos = cursor.transform.position;
        
        // Smoothly move cursor Y position
        float newY = Mathf.Lerp(currentPos.y, targetY, cursorTransitionSpeed * Time.deltaTime);
        
        // Update cursor position
        cursor.transform.position = new Vector3(currentPos.x, newY, currentPos.z);
        
        // Check if we're close enough to the target position
        if (Mathf.Abs(newY - targetY) < 0.01f)
        {
            cursor.transform.position = new Vector3(currentPos.x, targetY, currentPos.z);
        }
    }
    
    /// <summary>
    /// Check if cursor collides with any button when at Y=0 using actual collider detection
    /// </summary>
    private void CheckCursorButtonCollision()
    {
        if (cursor == null) return;
        
        // Use collider detection instead of distance-based detection
        Collider cursorCollider = cursor.GetComponent<Collider>();
        if (cursorCollider == null) return;
        
        // Check for actual collider overlaps with button colliders
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                Collider buttonCollider = buttonObjects[i].GetComponent<Collider>();
                if (buttonCollider != null)
                {
                    // Check if cursor collider is overlapping with button collider
                    if (cursorCollider.bounds.Intersects(buttonCollider.bounds))
                    {
                        string buttonLabel = buttonTexts[i].text;
                        // Debug.Log($"Cursor collider intersected with button: {buttonLabel}");
                        OnButtonResponse(buttonLabel);
                        break; // Only trigger one button at a time
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Fallback button collision detection using distance-based method
    /// </summary>
    private void CheckCursorButtonCollisionFallback()
    {
        if (cursor == null) return;
        
        // Check collision with each button using distance
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                float distance = Vector3.Distance(cursor.transform.position, buttonObjects[i].transform.position);
                
                // If cursor is close enough to button, trigger selection
                if (distance < 0.15f) // Slightly larger threshold for fallback
                {
                    string buttonLabel = buttonTexts[i].text;
                    // Debug.Log($"Fallback: Cursor near button {buttonLabel} at distance {distance}");
                    OnButtonResponse(buttonLabel);
                    break; // Only trigger one button at a time
                }
            }
        }
    }
    
    /// <summary>
    /// Check if mouse click hits any button using raycast
    /// </summary>
    private bool CheckMouseClickOnButtons()
    {
        if (prefabCamera == null) return false;
        
        // Cast a ray from the camera through the mouse position
        Ray ray = prefabCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Check if the ray hits any of the button objects
        if (Physics.Raycast(ray, out hit))
        {
            for (int i = 0; i < buttonObjects.Count; i++)
            {
                if (buttonObjects[i] != null && hit.collider.gameObject == buttonObjects[i])
                {
                    // Found a button hit - trigger the response if trial is active
                    if (trialActive && !responseGiven)
                    {
                        string buttonLabel = buttonTexts[i].text;
                        Debug.Log($"Mouse clicked on button: {buttonLabel}");
                        OnButtonResponse(buttonLabel);
                    }
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if mouse click hits the dock using raycast
    /// </summary>
    private bool CheckMouseClickOnDock()
    {
        if (prefabCamera == null || dock == null) return false;
        
        // Cast a ray from the camera through the mouse position
        Ray ray = prefabCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Check if the ray hits the dock specifically
        if (Physics.Raycast(ray, out hit))
        {
            // Make sure we hit the dock and not something else
            if (hit.collider.gameObject == dock)
            {
                Debug.Log("Mouse clicked directly on dock");
                return true;
            }
            else
            {
                Debug.Log($"Mouse clicked on {hit.collider.gameObject.name}, not dock");
            }
        }
        else
        {
            Debug.Log("Mouse click didn't hit anything");
        }
        
        return false;
    }
    
    /// <summary>
    /// Set dock trigger state (called by CursorTriggerDetector)
    /// </summary>
    public void SetDockTriggerState(bool inTrigger)
    {
        isInDockTrigger = inTrigger;
    }
    
    /// <summary>
    /// Set button trigger state (called by CursorTriggerDetector)
    /// </summary>
    public void SetButtonTriggerState(bool inTrigger, string buttonLabel)
    {
        isInButtonTrigger = inTrigger;
        currentButtonInTrigger = buttonLabel;
    }
}

/// <summary>
/// Component to handle trigger detection for the cursor
/// </summary>
public class CursorTriggerDetector : MonoBehaviour
{
    private StroopTask stroopTask;
    
    public void Initialize(StroopTask task)
    {
        stroopTask = task;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (stroopTask == null) return;
        
        // Only respond to objects with trigger colliders - ignore plane colliders
        if (!other.isTrigger)
        {
            Debug.Log($"Ignoring non-trigger collider: {other.gameObject.name}");
            return;
        }
        
        // Check if we entered the dock trigger
        if (other.gameObject.name == "Dock" || other.gameObject.CompareTag("Dock"))
        {
            stroopTask.SetDockTriggerState(true);
            // Debug.Log("Cursor entered dock trigger");
        }
        
        // Check if we entered a button trigger (check both button object and its collider children)
        for (int i = 0; i < stroopTask.buttonObjects.Count; i++)
        {
            if (stroopTask.buttonObjects[i] != null)
            {
                // Check if we hit the button object itself or any of its children (like colliders)
                if (other.gameObject == stroopTask.buttonObjects[i] || 
                    other.transform.IsChildOf(stroopTask.buttonObjects[i].transform))
                {
                    stroopTask.SetButtonTriggerState(true, stroopTask.buttonTexts[i].text);
                    // Debug.Log($"Cursor entered button trigger: {stroopTask.buttonTexts[i].text} (object: {other.gameObject.name})");
                    break;
                }
            }
        }
        
        // Debug: Log all trigger entries
        Debug.Log($"Trigger entered: {other.gameObject.name}, Tag: {other.gameObject.tag}, IsTrigger: {other.isTrigger}");
    }
    
    void OnTriggerExit(Collider other)
    {
        if (stroopTask == null) return;
        
        // Only respond to objects with trigger colliders - ignore plane colliders
        if (!other.isTrigger)
        {
            return;
        }
        
        // Check if we exited the dock trigger
        if (other.gameObject.name == "Dock" || other.gameObject.CompareTag("Dock"))
        {
            stroopTask.SetDockTriggerState(false);
            // Debug.Log("Cursor exited dock trigger");
        }
        
        // Check if we exited a button trigger (check both button object and its collider children)
        for (int i = 0; i < stroopTask.buttonObjects.Count; i++)
        {
            if (stroopTask.buttonObjects[i] != null)
            {
                // Check if we exited the button object itself or any of its children (like colliders)
                if (other.gameObject == stroopTask.buttonObjects[i] || 
                    other.transform.IsChildOf(stroopTask.buttonObjects[i].transform))
                {
                    stroopTask.SetButtonTriggerState(false, "");
                    // Debug.Log($"Cursor exited button trigger: {stroopTask.buttonTexts[i].text} (object: {other.gameObject.name})");
                    break;
                }
            }
        }
    }
}
