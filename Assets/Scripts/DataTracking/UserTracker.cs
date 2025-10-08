using UnityEngine;
using VInspector;

/// <summary>
/// Component to handle user sessions
/// </summary>
public class UserTracker : MonoBehaviour
{
    private string _category      = "Users";
    private int    _currentUserId = -1;
    
    /// <summary>
    /// Create a new user session and get the assigned user ID
    /// </summary>
    /// <returns>The unique user ID for this session</returns>
    [Button]
    public int CreateUserSession()
    {
        if (_currentUserId != -1)
        {
            Debug.LogWarning("Trying to create a new session while one is already active. Ending current session first.");
            EndUserSession();
        }
        
        _currentUserId = ExcelDataManager.IncrementUserCount(_category);
        Debug.Log($"New user session created with ID: {_currentUserId}");
        return _currentUserId;
    }
    
    /// <summary>
    /// End the current user session
    /// </summary>
    [Button]
    public void EndUserSession()
    {
        Debug.Log($"User session ended for ID: {_currentUserId}");
        _currentUserId = -1;
    }
    
    /// <summary>
    /// Get the current user ID
    /// </summary>
    /// <returns>Current user ID, or -1 if no active session</returns>
    public int GetCurrentUserId()
    {
        return _currentUserId;
    }
}