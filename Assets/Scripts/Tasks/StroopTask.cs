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
    [SerializeField] List<GameObject> buttonObjects = new List<GameObject>(); // Your 4 button objects
    [SerializeField] List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();
    
    // Button labels are now managed by JSON - no longer serialized
    private List<string> buttonLabels = new List<string>();
    
    [Header("Scoreboard")]
    [SerializeField] GameObject Scoreboard;
    [SerializeField] TextMeshProUGUI ScoreTXT;
    [SerializeField] TextMeshProUGUI TrialTXT;
    
    [Header("VR Components")]
    [SerializeField] GameObject directRight;
    [SerializeField] GameObject directLeft;
    [SerializeField] GameObject leftHand;
    [SerializeField] GameObject leftHandCtrl;
    [SerializeField] GameObject rightHand;
    [SerializeField] GameObject rightHandCtrl;
    [SerializeField] GameObject MainCamera;
    
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
    
    // Trial data
    private string currentWord = "";
    private Color currentColor = Color.white;
    private string correctAnswer = "";
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
    private List<string> correctAnswers = new List<string>();
    private List<string> participantResponses = new List<string>();
    
    // Cursor movement for 2D mode
    private float originalCursorY = 0f;
    private bool isLeftMouseHeld = false;
    private float cursorTransitionSpeed = 10f;

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
                Debug.Log("Left mouse button pressed - moving cursor to Y=0");
            }
            // Check for left mouse button release
            else if (Input.GetMouseButtonUp(0))
            {
                isLeftMouseHeld = false;
                Debug.Log("Left mouse button released - returning cursor to original height");
            }
            
            // Update cursor position based on left mouse button state
            UpdateCursorPosition();
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
                        // Check if cursor is colliding with dock (only when cursor is at Y=0)
                        if (cursor != null && dock != null && isLeftMouseHeld)
                        {
                            float distance = Vector3.Distance(cursor.transform.position, dock.transform.position);
                            dockHit = distance < 0.1f; // Adjust this threshold as needed
                        }
                        
                        // Also check for mouse click on dock (raycast) - but only if not clicking on buttons
                        if (!dockHit && Input.GetMouseButtonDown(0))
                        {
                            // First check if we're clicking on a button
                            bool clickingOnButton = CheckMouseClickOnButtons();
                            if (!clickingOnButton)
                            {
                                dockHit = CheckMouseClickOnDock();
                            }
                        }
                    }
                    
                    if (dockHit)
                    {
                        dock.GetComponent<Target>().ResetTarget();
                        audioSource.clip = buttonClickSFX;
                        audioSource.Play();
                        
                        // Don't disable dock immediately - let it stay visible during trial
                        // dock.GetComponent<Target>().enabled = false;
                        // dock.GetComponent<MeshCollider>().enabled = false;
                        // dock.SetActive(false);

                        // Check if this is the start of a new block or first trial
                        if (ExperimentController.Instance.Session.CurrentTrial.numberInBlock == 1)
                        {
                            // Starting first trial of a block
                            startTime = Time.time;
                        }

                        StartTrial();
                        IncrementStep();
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
        // Generate trial parameters
        GenerateTrialParameters();
        
        // Display the word
        DisplayWord();
        
        // Setup response buttons
        SetupResponseButtons();
        
        // Activate buttons for interaction
        ActivateButtons(true);
        
        // Hide dock during trial
        if (dock != null)
        {
            dock.SetActive(false);
        }
        
        // Start trial timing
        trialActive = true;
        responseGiven = false;
        trialStartTime = Time.time;
        
        Debug.Log($"Trial {ExperimentController.Instance.Session.CurrentTrial.numberInBlock}: Word='{currentWord}', Color={currentColor}, Correct='{correctAnswer}'");
    }

    private void ActivateButtons(bool active)
    {
        for (int i = 0; i < buttonObjects.Count; i++)
        {
            if (buttonObjects[i] != null)
            {
                ButtonCollisionHandler handler = buttonObjects[i].GetComponent<ButtonCollisionHandler>();
                if (handler != null)
                {
                    handler.SetActive(active);
                }
            }
        }
    }

    public override void SetUp()
    {
        base.SetUp();
        maxSteps = 3;

        // Initialize VR components
        directRight = GameObject.Find("RH Direct Interactor");
        directLeft = GameObject.Find("LH Direct Interactor");
        leftHand = GameObject.Find("Left Hand");
        rightHand = GameObject.Find("Right Hand");
        leftHandCtrl = GameObject.Find("Left Controller");
        rightHandCtrl = GameObject.Find("Right Controller");
        MainCamera = GameObject.Find("Main Camera");

        // Setup VR or desktop mode
        SetupXR();
        
        // Initialize cursor Y position for 2D mode
        if (!ExperimentController.Instance.UseVR && cursor != null)
        {
            originalCursorY = cursor.transform.position.y;
            Debug.Log($"Original cursor Y position: {originalCursorY}");
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

    public override void TaskBegin()
    {
        base.TaskBegin();
        
        // Reset trial data
        trialActive = false;
        responseGiven = false;
        
        // Show UI elements
        if (wordDisplayCanvas != null)
            wordDisplayCanvas.SetActive(true);
        if (buttonContainer != null)
            buttonContainer.SetActive(true);
        
        // Ensure buttons are disabled initially - they will be enabled when trials start
        ActivateButtons(false);
        
        // Setup dock
        dock.SetActive(true);
        dock.GetComponent<Target>().enabled = true;
        dock.GetComponent<MeshCollider>().enabled = true;
        dock.GetComponent<Target>().ResetTarget();
        
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
        // Get current trial data from JSON - use fallback if trial_name doesn't exist
        string currentTrialName;
        try
        {
            currentTrialName = ExperimentController.Instance.Session.CurrentTrial.settings.GetString("trial_name");
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            // Fallback: use trial number as trial name
            currentTrialName = $"trial_{ExperimentController.Instance.Session.CurrentTrial.numberInBlock}";
            Debug.LogWarning($"trial_name not found, using fallback: {currentTrialName}");
        }
        
        // Get trial data from session settings
        var trialData = ExperimentController.Instance.Session.settings.GetObject("trial_data");
        
        // Debug: Log trial data structure
        Debug.Log($"Trial data type: {trialData?.GetType()}");
        Debug.Log($"Trial data is null: {trialData == null}");
        
        if (trialData != null)
        {
            Debug.Log($"Trial data: {trialData}");
        }
        
        // Debug: Log available trial data keys
        if (trialData is System.Collections.Generic.Dictionary<string, object> trialDataDict)
        {
            Debug.Log($"Available trial data keys: {string.Join(", ", trialDataDict.Keys)}");
            Debug.Log($"Looking for trial: {currentTrialName}");
            
            if (trialDataDict.ContainsKey(currentTrialName))
            {
                var currentTrialData = trialDataDict[currentTrialName] as System.Collections.Generic.Dictionary<string, object>;
                
                if (currentTrialData != null)
                {
                    // Extract trial parameters from JSON
                    currentWord = currentTrialData["displayed_word"].ToString();
                    string colorName = currentTrialData["displayed_color"].ToString();
                    correctAnswer = currentTrialData["correct_answer"].ToString();
                    
                    // Convert color name to Unity Color
                    if (colorMap.ContainsKey(colorName))
                    {
                        currentColor = colorMap[colorName];
                    }
                    else
                    {
                        Debug.LogError($"Unknown color: {colorName}");
                        currentColor = Color.white;
                    }
                    
                    // Store trial data
                    presentedWords.Add(currentWord);
                    presentedColors.Add(colorName);
                    correctAnswers.Add(correctAnswer);
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
    }

    /// <summary>
    /// Generate fallback trial data when JSON data is not available
    /// </summary>
    private void GenerateFallbackTrialData()
    {
        Debug.LogWarning("=== GENERATING FALLBACK TRIAL DATA ===");
        
        // Simple fallback trial data
        string[] words = { "RED", "BLUE", "GREEN", "YELLOW" };
        string[] colors = { "red", "blue", "green", "yellow" };
        
        // Use trial number to determine word and color
        int trialIndex = ExperimentController.Instance.Session.CurrentTrial.numberInBlock - 1;
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
        correctAnswers.Add(correctAnswer);
        
        Debug.Log($"Fallback trial data: Word='{currentWord}', Color={colorName}, Correct='{correctAnswer}'");
    }

    private void DisplayWord()
    {
        wordText.text = currentWord;
        wordText.color = currentColor;
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
            // Fallback: use trial number as trial name
            currentTrialName = $"trial_{ExperimentController.Instance.Session.CurrentTrial.numberInBlock}";
            Debug.LogWarning($"trial_name not found, using fallback: {currentTrialName}");
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
                    // Get button options from JSON
                    var buttonOptionsJson = currentTrialData["button_options"];
                    List<string> buttonOptions = new List<string>();
                    
                    // Convert JSON array to List<string> - handle as object array
                    if (buttonOptionsJson is System.Collections.IList jsonArray)
                    {
                        foreach (var item in jsonArray)
                        {
                            buttonOptions.Add(item.ToString());
                        }
                    }
                    
                    // Update button texts with options from JSON
                    for (int i = 0; i < buttonTexts.Count && i < buttonOptions.Count; i++)
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
                    
                    Debug.Log($"Button setup from JSON: Correct='{correctAnswer}', Options=[{string.Join(", ", buttonOptions)}]");
                }
                else
                {
                    Debug.LogError($"Could not parse trial data for: {currentTrialName}");
                    SetupFallbackButtons();
                }
            }
            else
            {
                Debug.LogError($"Could not find trial data for: {currentTrialName}");
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
        Debug.LogWarning("Using fallback button setup");
        
        // Simple fallback button options
        string[] buttonOptions = { "red", "blue", "green", "yellow" };
        
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
        
        Debug.Log($"Fallback button setup: Options=[{string.Join(", ", buttonOptions)}]");
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
        // Wait 0.5 seconds before next trial
        yield return new WaitForSeconds(0.5f);
        
        // Check if we've completed all trials in this block using ExperimentController
        List<int> trialsPerBlock = ExperimentController.Instance.Session.CurrentBlock.settings.GetIntList("trials_in_block");
        int currentTrialInBlock = ExperimentController.Instance.Session.CurrentTrial.numberInBlock;
        int trialsInCurrentBlock = trialsPerBlock[ExperimentController.Instance.Session.currentBlockNum - 1];
        
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
            
            // Reset step to wait for dock press
            currentStep = 0;
            
            Debug.Log("Dock shown for next trial. Press to continue.");
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
        session.CurrentTrial.result["correct_answers"] = string.Join(",", correctAnswers);
        session.CurrentTrial.result["participant_responses"] = string.Join(",", participantResponses);
        session.CurrentTrial.result["reaction_times"] = string.Join(",", reactionTimes.Select(rt => rt.ToString("F3")));
        session.CurrentTrial.result["correct_responses"] = string.Join(",", correctResponses.Select(cr => cr.ToString()));

        // Block type information
        string blockType = currentBlock == 0 ? "congruent" : "incongruent";
        session.CurrentTrial.result["block_type"] = blockType;
        
        // Current trial data (if available)
        if (presentedWords.Count > 0)
        {
            session.CurrentTrial.result["current_word"] = presentedWords[presentedWords.Count - 1];
            session.CurrentTrial.result["current_color"] = presentedColors[presentedColors.Count - 1];
            session.CurrentTrial.result["current_correct_answer"] = correctAnswers[correctAnswers.Count - 1];
            session.CurrentTrial.result["current_participant_response"] = participantResponses[participantResponses.Count - 1];
            session.CurrentTrial.result["current_reaction_time"] = reactionTimes[reactionTimes.Count - 1];
            session.CurrentTrial.result["current_correct"] = correctResponses[correctResponses.Count - 1];
        }
    }

    private void UpdateScoreboard()
    {
        // Find child objects (requires proper hierarchy structure)
        TextMeshProUGUI scoreText = Scoreboard.transform.Find("ScoreTXT").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI trialText = Scoreboard.transform.Find("TrialTXT").GetComponent<TextMeshProUGUI>();

        int accuracy = completedTrials > 0 ? (int)((float)totalCorrect / completedTrials * 100) : 0;
        float avgRT = completedTrials > 0 ? totalReactionTime / completedTrials : 0f;
        string blockType = currentBlock == 0 ? "Congruent" : "Incongruent";

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
        if (!trialActive || responseGiven)
            return;

        responseGiven = true;
        trialActive = false;

        // Deactivate buttons to prevent multiple responses
        ActivateButtons(false);

        // Calculate reaction time
        reactionTime = Time.time - trialStartTime;
        totalReactionTime += reactionTime;

        // Check if response is correct
        bool isCorrect = response == correctAnswer;
        if (isCorrect)
        {
            totalCorrect++;
            totalScore += Mathf.Max(0, 100 - Mathf.RoundToInt(reactionTime * 100));
        }

        // Store trial data
        participantResponses.Add(response);
        reactionTimes.Add(reactionTime);
        correctResponses.Add(isCorrect);

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
        
        // Check if the ray hits the dock
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == dock)
            {
                Debug.Log("Mouse clicked on dock");
                return true;
            }
        }
        
        return false;
    }
}
