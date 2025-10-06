using UnityEngine;

public class ButtonCollisionHandler : MonoBehaviour
{
    private StroopTask stroopTask;
    private string buttonLabel;
    private bool isActive = false;
    
    public void Initialize(StroopTask task, string label)
    {
        stroopTask = task;
        buttonLabel = label;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!isActive || stroopTask == null)
            return;
            
        // Enhanced VR hand detection
        bool isVRHand = false;
        string detectionMethod = "";
        
        // Check by tag first
        if (other.CompareTag("Hand"))
        {
            isVRHand = true;
            detectionMethod = "Hand tag";
        }
        else if (other.CompareTag("Controller"))
        {
            isVRHand = true;
            detectionMethod = "Controller tag";
        }
        else if (other.CompareTag("Cursor"))
        {
            isVRHand = true;
            detectionMethod = "Cursor tag";
        }
        // Check by name patterns
        else if (other.name.Contains("Hand") || other.name.Contains("Controller") || other.name.Contains("Cursor"))
        {
            isVRHand = true;
            detectionMethod = "name pattern";
        }
        // Check for XR Direct Interactor components
        else if (other.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRDirectInteractor>() != null)
        {
            isVRHand = true;
            detectionMethod = "XRDirectInteractor component";
        }
        // Check for XR Controller components
        else if (other.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRController>() != null)
        {
            isVRHand = true;
            detectionMethod = "XRController component";
        }
        
        if (isVRHand)
        {
            Debug.Log($"Button {buttonLabel} hit by {other.name} (detected via {detectionMethod})");
            stroopTask.OnButtonResponse(buttonLabel);
        }
        else
        {
            // Debug log for non-VR objects that enter trigger
            Debug.Log($"Button {buttonLabel} trigger entered by {other.name} (tag: {other.tag}, not a VR hand)");
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!isActive || stroopTask == null)
            return;
            
        // Enhanced VR hand detection for collisions
        bool isVRHand = false;
        string detectionMethod = "";
        
        // Check by tag first
        if (collision.gameObject.CompareTag("Hand"))
        {
            isVRHand = true;
            detectionMethod = "Hand tag";
        }
        else if (collision.gameObject.CompareTag("Controller"))
        {
            isVRHand = true;
            detectionMethod = "Controller tag";
        }
        else if (collision.gameObject.CompareTag("Cursor"))
        {
            isVRHand = true;
            detectionMethod = "Cursor tag";
        }
        // Check by name patterns
        else if (collision.gameObject.name.Contains("Hand") || collision.gameObject.name.Contains("Controller") || collision.gameObject.name.Contains("Cursor"))
        {
            isVRHand = true;
            detectionMethod = "name pattern";
        }
        // Check for XR Direct Interactor components
        else if (collision.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRDirectInteractor>() != null)
        {
            isVRHand = true;
            detectionMethod = "XRDirectInteractor component";
        }
        // Check for XR Controller components
        else if (collision.gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRController>() != null)
        {
            isVRHand = true;
            detectionMethod = "XRController component";
        }
        
        if (isVRHand)
        {
            Debug.Log($"Button {buttonLabel} collision with {collision.gameObject.name} (detected via {detectionMethod})");
            stroopTask.OnButtonResponse(buttonLabel);
        }
        else
        {
            // Debug log for non-VR objects that collide
            Debug.Log($"Button {buttonLabel} collision with {collision.gameObject.name} (tag: {collision.gameObject.tag}, not a VR hand)");
        }
    }
}
