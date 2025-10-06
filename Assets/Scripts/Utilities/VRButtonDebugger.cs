using UnityEngine;

/// <summary>
/// Debug script to help troubleshoot VR button interaction issues
/// </summary>
public class VRButtonDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool showHandPositions = false;
    [SerializeField] private bool showButtonStates = false;
    
    [Header("References")]
    [SerializeField] private StroopTask stroopTask;
    
    private void Start()
    {
        if (stroopTask == null)
        {
            stroopTask = FindObjectOfType<StroopTask>();
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("=== VR Button Debugger Started ===");
        }
    }
    
    private void Update()
    {
        if (!enableDebugLogging || stroopTask == null) return;
        
        if (showHandPositions)
        {
            LogHandPositions();
        }
        
        if (showButtonStates)
        {
            LogButtonStates();
        }
    }
    
    private void LogHandPositions()
    {
        if (Time.frameCount % 60 == 0) // Log every 60 frames (once per second)
        {
            if (stroopTask.directLeft != null)
            {
                Debug.Log($"Left Direct Interactor Position: {stroopTask.directLeft.transform.position}");
            }
            if (stroopTask.directRight != null)
            {
                Debug.Log($"Right Direct Interactor Position: {stroopTask.directRight.transform.position}");
            }
        }
    }
    
    private void LogButtonStates()
    {
        if (Time.frameCount % 60 == 0) // Log every 60 frames (once per second)
        {
            Debug.Log("=== Button States ===");
            for (int i = 0; i < stroopTask.buttonObjects.Count; i++)
            {
                if (stroopTask.buttonObjects[i] != null)
                {
                    MultipleTarget multipleTarget = stroopTask.buttonObjects[i].GetComponent<MultipleTarget>();
                    if (multipleTarget != null)
                    {
                        Debug.Log($"Button {i} ({stroopTask.buttonObjects[i].name}): " +
                                 $"IsToolColliding={multipleTarget.IsToolCollding}, " +
                                 $"Tools.Count={multipleTarget.tools.Count}, " +
                                 $"Position={stroopTask.buttonObjects[i].transform.position}");
                        
                        // Check if any VR hands are near the button
                        if (stroopTask.directLeft != null)
                        {
                            float distanceLeft = Vector3.Distance(stroopTask.directLeft.transform.position, 
                                                                 stroopTask.buttonObjects[i].transform.position);
                            Debug.Log($"  Distance to Left Hand: {distanceLeft:F3}");
                        }
                        if (stroopTask.directRight != null)
                        {
                            float distanceRight = Vector3.Distance(stroopTask.directRight.transform.position, 
                                                                  stroopTask.buttonObjects[i].transform.position);
                            Debug.Log($"  Distance to Right Hand: {distanceRight:F3}");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Public method to manually test button interactions
    /// </summary>
    [ContextMenu("Test Button Interactions")]
    public void TestButtonInteractions()
    {
        Debug.Log("=== Manual Button Interaction Test ===");
        
        if (stroopTask == null)
        {
            Debug.LogError("StroopTask reference is null!");
            return;
        }
        
        for (int i = 0; i < stroopTask.buttonObjects.Count; i++)
        {
            if (stroopTask.buttonObjects[i] != null)
            {
                Debug.Log($"Testing Button {i}: {stroopTask.buttonObjects[i].name}");
                
                // Check colliders
                Collider[] colliders = stroopTask.buttonObjects[i].GetComponents<Collider>();
                Debug.Log($"  Colliders: {colliders.Length}");
                foreach (var collider in colliders)
                {
                    Debug.Log($"    - {collider.GetType().Name} (enabled: {collider.enabled}, isTrigger: {collider.isTrigger})");
                }
                
                // Check MultipleTarget
                MultipleTarget multipleTarget = stroopTask.buttonObjects[i].GetComponent<MultipleTarget>();
                if (multipleTarget != null)
                {
                    Debug.Log($"  MultipleTarget: enabled={multipleTarget.enabled}, tools={multipleTarget.tools.Count}");
                    foreach (var tool in multipleTarget.tools)
                    {
                        Debug.Log($"    - Tool: {tool?.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"  No MultipleTarget component found!");
                }
            }
        }
    }
    
    /// <summary>
    /// Public method to check VR hand positions relative to buttons
    /// </summary>
    [ContextMenu("Check Hand-Button Distances")]
    public void CheckHandButtonDistances()
    {
        Debug.Log("=== Hand-Button Distance Check ===");
        
        if (stroopTask == null) return;
        
        for (int i = 0; i < stroopTask.buttonObjects.Count; i++)
        {
            if (stroopTask.buttonObjects[i] != null)
            {
                Vector3 buttonPos = stroopTask.buttonObjects[i].transform.position;
                Debug.Log($"Button {i} ({stroopTask.buttonObjects[i].name}) at {buttonPos}");
                
                if (stroopTask.directLeft != null)
                {
                    float distLeft = Vector3.Distance(stroopTask.directLeft.transform.position, buttonPos);
                    Debug.Log($"  Distance to Left Hand: {distLeft:F3}");
                }
                
                if (stroopTask.directRight != null)
                {
                    float distRight = Vector3.Distance(stroopTask.directRight.transform.position, buttonPos);
                    Debug.Log($"  Distance to Right Hand: {distRight:F3}");
                }
            }
        }
    }
}
