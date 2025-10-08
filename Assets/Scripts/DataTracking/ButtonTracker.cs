using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Component to track button presses and record them with user ID
/// </summary>
public class ButtonTracker : MonoBehaviour
{
    [Tooltip("Category name for this button's presses")]
    [SerializeField] private string category = "DefaultButton";
    
    [Tooltip("Optional button component - if not assigned, will try to find one on this GameObject")]
    [SerializeField] private Button button;
    
    [Tooltip("Reference to the UserTracker that manages the current session")]
    [SerializeField] private UserTracker userTracker;
    
    private void Awake()
    {
        // Try to find the button if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogWarning($"ButtonTracker on {gameObject.name}: No Button component found!");
            }
        }
        
        // Try to find the UserTracker if not assigned
        if (userTracker == null)
        {
            userTracker = FindObjectOfType<UserTracker>();
            if (userTracker == null)
            {
                Debug.LogWarning($"ButtonTracker on {gameObject.name}: No UserTracker found in scene!");
            }
        }
    }
    
    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(RecordButtonPress);
        }
    }
    
    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(RecordButtonPress);
        }
    }
    
    /// <summary>
    /// Record button press with current user ID
    /// </summary>
    public void RecordButtonPress()
    {
        if (userTracker != null)
        {
            int userId = userTracker.GetCurrentUserId();
            if (userId != -1)
            {
                ExcelDataManager.RecordButtonPress(category, userId);
            }
            else
            {
                Debug.LogWarning($"ButtonTracker on {gameObject.name}: No active user session!");
            }
        }
    }
    
    /// <summary>
    /// Set or change the category for this button
    /// </summary>
    /// <param name="newCategory">New category name</param>
    public void SetCategory(string newCategory)
    {
        if (!string.IsNullOrEmpty(newCategory))
        {
            category = newCategory;
        }
    }
}
