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
            
        // Check if it's a hand, controller, or cursor
        if (other.CompareTag("Hand") || other.CompareTag("Controller") || other.CompareTag("Cursor") ||
            other.name.Contains("Hand") || other.name.Contains("Controller") || other.name.Contains("Cursor"))
        {
            Debug.Log($"Button {buttonLabel} hit by {other.name}");
            stroopTask.OnButtonResponse(buttonLabel);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!isActive || stroopTask == null)
            return;
            
        // Check if it's a hand, controller, or cursor
        if (collision.gameObject.CompareTag("Hand") || collision.gameObject.CompareTag("Controller") || collision.gameObject.CompareTag("Cursor") ||
            collision.gameObject.name.Contains("Hand") || collision.gameObject.name.Contains("Controller") || collision.gameObject.name.Contains("Cursor"))
        {
            Debug.Log($"Button {buttonLabel} hit by {collision.gameObject.name}");
            stroopTask.OnButtonResponse(buttonLabel);
        }
    }
}
