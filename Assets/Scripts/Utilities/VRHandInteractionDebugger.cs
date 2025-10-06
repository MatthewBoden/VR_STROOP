using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Debug script to help troubleshoot VR hand interaction issues
/// </summary>
public class VRHandInteractionDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = true;
    [SerializeField] private bool logAllCollisions = false;
    [SerializeField] private bool showHandPositions = false;
    
    [Header("VR Hand References")]
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject directLeft;
    [SerializeField] private GameObject directRight;
    
    private void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log("=== VR Hand Interaction Debugger Started ===");
            FindAndLogVRComponents();
        }
    }
    
    private void Update()
    {
        if (showHandPositions && enableDebugLogging)
        {
            LogHandPositions();
        }
    }
    
    private void FindAndLogVRComponents()
    {
        Debug.Log("=== Searching for VR Components ===");
        
        // Find all possible VR hand objects
        string[] possibleHandNames = {
            "Left Hand", "Right Hand", "LeftHand", "RightHand",
            "LeftHand Controller", "RightHand Controller", "LeftHandController", "RightHandController",
            "LH Direct Interactor", "RH Direct Interactor", "LeftHand Direct Interactor", "RightHand Direct Interactor",
            "Left Direct Interactor", "Right Direct Interactor"
        };
        
        foreach (string name in possibleHandNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Debug.Log($"Found VR component: {name}");
                LogObjectDetails(obj);
            }
        }
        
        // Find all objects with XR components
        XRDirectInteractor[] directInteractors = FindObjectsOfType<XRDirectInteractor>();
        Debug.Log($"Found {directInteractors.Length} XRDirectInteractor components:");
        foreach (var interactor in directInteractors)
        {
            Debug.Log($"  - {interactor.name} (tag: {interactor.tag})");
        }
        
        XRController[] controllers = FindObjectsOfType<XRController>();
        Debug.Log($"Found {controllers.Length} XRController components:");
        foreach (var controller in controllers)
        {
            Debug.Log($"  - {controller.name} (tag: {controller.tag})");
        }
        
        Debug.Log("=== VR Component Search Complete ===");
    }
    
    private void LogObjectDetails(GameObject obj)
    {
        Debug.Log($"  Object: {obj.name}");
        Debug.Log($"    Tag: {obj.tag}");
        Debug.Log($"    Layer: {obj.layer}");
        Debug.Log($"    Active: {obj.activeInHierarchy}");
        
        // Check for colliders
        Collider[] colliders = obj.GetComponents<Collider>();
        Debug.Log($"    Colliders: {colliders.Length}");
        foreach (var collider in colliders)
        {
            Debug.Log($"      - {collider.GetType().Name} (isTrigger: {collider.isTrigger})");
        }
        
        // Check for XR components
        XRDirectInteractor directInteractor = obj.GetComponent<XRDirectInteractor>();
        if (directInteractor != null)
        {
            Debug.Log($"    XRDirectInteractor: Present");
        }
        
        XRController xrController = obj.GetComponent<XRController>();
        if (xrController != null)
        {
            Debug.Log($"    XRController: Present");
        }
    }
    
    private void LogHandPositions()
    {
        if (leftHand != null)
        {
            Debug.Log($"Left Hand Position: {leftHand.transform.position}");
        }
        if (rightHand != null)
        {
            Debug.Log($"Right Hand Position: {rightHand.transform.position}");
        }
        if (directLeft != null)
        {
            Debug.Log($"Left Direct Interactor Position: {directLeft.transform.position}");
        }
        if (directRight != null)
        {
            Debug.Log($"Right Direct Interactor Position: {directRight.transform.position}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (logAllCollisions && enableDebugLogging)
        {
            Debug.Log($"=== TRIGGER ENTER ===");
            Debug.Log($"Triggered by: {other.name}");
            Debug.Log($"Tag: {other.tag}");
            Debug.Log($"Layer: {other.gameObject.layer}");
            Debug.Log($"IsTrigger: {other.isTrigger}");
            LogObjectDetails(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (logAllCollisions && enableDebugLogging)
        {
            Debug.Log($"=== TRIGGER EXIT ===");
            Debug.Log($"Exited by: {other.name}");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (logAllCollisions && enableDebugLogging)
        {
            Debug.Log($"=== COLLISION ENTER ===");
            Debug.Log($"Collided with: {collision.gameObject.name}");
            Debug.Log($"Tag: {collision.gameObject.tag}");
            LogObjectDetails(collision.gameObject);
        }
    }
    
    /// <summary>
    /// Public method to manually test VR hand detection
    /// </summary>
    [ContextMenu("Test VR Hand Detection")]
    public void TestVRHandDetection()
    {
        Debug.Log("=== Manual VR Hand Detection Test ===");
        FindAndLogVRComponents();
    }
    
    /// <summary>
    /// Public method to check if VR hands are properly tagged
    /// </summary>
    [ContextMenu("Check VR Hand Tags")]
    public void CheckVRHandTags()
    {
        Debug.Log("=== Checking VR Hand Tags ===");
        
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Hand") || obj.name.Contains("Controller") || obj.name.Contains("Direct"))
            {
                Debug.Log($"VR-related object: {obj.name} (tag: {obj.tag})");
            }
        }
    }
}
