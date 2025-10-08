using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Static manager class for tracking user sessions and button presses
/// </summary>
public static class ExcelDataManager
{
    // File names for data
    private const string USER_COUNT_CSV = "UserData.csv";
    private const string BUTTON_PRESSES_CSV = "AppTrackingData.csv";
    
    // File paths
    private static string streamingAssetsPath;
    private static string userCountPath;
    private static string buttonPressesPath;
    private static string writableUserCountPath;
    private static string writableButtonPressesPath;
    
    // Dictionary to store user count data
    private static Dictionary<string, int> userCountData = new Dictionary<string, int>();
    
    // List to store button press data
    private static List<ButtonPressEntry> buttonPressData = new List<ButtonPressEntry>();
    
    // Flag to track if data has changed
    private static bool isUserCountDirty = false;
    private static bool isButtonPressDirty = false;
    
    // Thread synchronization
    private static readonly object dataLock = new object();
    
    // Background saving thread
    private static Timer saveTimer;
    
    /// <summary>
    /// Struct to store button press entry data
    /// </summary>
    private struct ButtonPressEntry
    {
        public string Category;
        public int UserId;
        public DateTime Timestamp;
        
        public ButtonPressEntry(string category, int userId)
        {
            Category = category;
            UserId = userId;
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Initialize the data manager
    /// </summary>
    static ExcelDataManager()
    {
        // Set up file paths
        streamingAssetsPath = Application.streamingAssetsPath;
        userCountPath = Path.Combine(streamingAssetsPath, USER_COUNT_CSV);
        buttonPressesPath = Path.Combine(streamingAssetsPath, BUTTON_PRESSES_CSV);
        
        // For WebGL, we need a different approach as StreamingAssets is read-only
        #if UNITY_WEBGL
        writableUserCountPath = Path.Combine(Application.persistentDataPath, USER_COUNT_CSV);
        writableButtonPressesPath = Path.Combine(Application.persistentDataPath, BUTTON_PRESSES_CSV);
        #else
        writableUserCountPath = userCountPath;
        writableButtonPressesPath = buttonPressesPath;
        #endif
        
        // Create StreamingAssets directory if it doesn't exist
        #if UNITY_EDITOR
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        #endif
        
        // Load existing data - use synchronous loading for initialization
        // to ensure data is available before any operations
        LoadUserCountDataSync();
        LoadButtonPressData();
        
        // Set up auto-save timer (every 60 seconds)
        saveTimer = new Timer(
            _ => SaveAllData(), 
            null, 
            TimeSpan.FromSeconds(60), 
            TimeSpan.FromSeconds(60)
        );
        
        // Register for application quit
        Application.quitting += () => {
            SaveAllData();
            saveTimer?.Dispose();
        };
    }
    
    /// <summary>
    /// Increments the user count for a category and returns the new count as user ID
    /// </summary>
    /// <param name="category">Category name to increment</param>
    /// <returns>The new count value (used as user ID)</returns>
    public static int IncrementUserCount(string category)
    {
        if (string.IsNullOrEmpty(category))
        {
            Debug.LogWarning("ExcelDataManager: Cannot increment count for empty category");
            return -1;
        }
        
        lock (dataLock)
        {
            if (userCountData.ContainsKey(category))
            {
                userCountData[category]++;
            }
            else
            {
                userCountData.Add(category, 1);
            }
            
            isUserCountDirty = true;
            Debug.Log($"User count for '{category}' incremented to {userCountData[category]}");
            return userCountData[category]; // Return the new count as user ID
        }
    }
    
    /// <summary>
    /// Records a button press with user ID
    /// </summary>
    /// <param name="category">Button category</param>
    /// <param name="userId">User ID for the session</param>
    public static void RecordButtonPress(string category, int userId)
    {
        if (string.IsNullOrEmpty(category))
        {
            Debug.LogWarning("ExcelDataManager: Cannot record press for empty category");
            return;
        }
        
        if (userId <= 0)
        {
            Debug.LogWarning("ExcelDataManager: Cannot record press with invalid user ID");
            return;
        }
        
        lock (dataLock)
        {
            buttonPressData.Add(new ButtonPressEntry(category, userId));
            isButtonPressDirty = true;
            Debug.Log($"Button press recorded for '{category}' by user {userId}");
        }
    }
    
    /// <summary>
    /// Gets the current count for a specific user category
    /// </summary>
    /// <param name="category">Category to retrieve count for</param>
    /// <returns>Current count for the category, or 0 if category doesn't exist</returns>
    public static int GetUserCount(string category)
    {
        lock (dataLock)
        {
            return userCountData.TryGetValue(category, out int count) ? count : 0;
        }
    }
    
    /// <summary>
    /// Load user count data from CSV file synchronously for initialization
    /// </summary>
    private static void LoadUserCountDataSync()
    {
        try
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            // For WebGL builds, try to load from writablePath
            if (File.Exists(writableUserCountPath))
            {
                ParseUserCsvData(File.ReadAllText(writableUserCountPath));
            }
            #else
            // For other platforms, load from streamingAssetsPath
            if (File.Exists(userCountPath))
            {
                ParseUserCsvData(File.ReadAllText(userCountPath));
            }
            else
            {
                Debug.Log($"User count CSV file not found. Will create new file on first save.");
            }
            #endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading user count CSV data synchronously: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load user count data from CSV file asynchronously
    /// </summary>
    private static void LoadUserCountData()
    {
        Task.Run(() => {
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                // For WebGL builds, try to load from writablePath
                if (File.Exists(writableUserCountPath))
                {
                    ParseUserCsvData(File.ReadAllText(writableUserCountPath));
                }
                #else
                // For other platforms, load from streamingAssetsPath
                if (File.Exists(userCountPath))
                {
                    ParseUserCsvData(File.ReadAllText(userCountPath));
                }
                else
                {
                    Debug.Log($"User count CSV file not found. Will create new file on first save.");
                }
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading user count CSV data: {ex.Message}");
            }
        });
    }
    
    /// <summary>
    /// Load button press data from CSV file
    /// </summary>
    private static void LoadButtonPressData()
    {
        Task.Run(() => {
            try
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                // For WebGL builds, try to load from writablePath
                if (File.Exists(writableButtonPressesPath))
                {
                    ParseButtonPressCsvData(File.ReadAllText(writableButtonPressesPath));
                }
                #else
                // For other platforms, load from streamingAssetsPath
                if (File.Exists(buttonPressesPath))
                {
                    ParseButtonPressCsvData(File.ReadAllText(buttonPressesPath));
                }
                else
                {
                    Debug.Log($"Button presses CSV file not found. Will create new file on first save.");
                }
                #endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading button presses CSV data: {ex.Message}");
            }
        });
    }
    
    /// <summary>
    /// Parse user count CSV data into the dictionary
    /// </summary>
    private static void ParseUserCsvData(string csvData)
    {
        Dictionary<string, int> loadedData = new Dictionary<string, int>();
        
        using (StringReader reader = new StringReader(csvData))
        {
            // Skip header line
            reader.ReadLine();
            
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                string[] parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string category = parts[0].Trim();
                    if (int.TryParse(parts[1].Trim(), out int count) && !string.IsNullOrEmpty(category))
                    {
                        loadedData[category] = count;
                    }
                }
            }
        }
        
        lock (dataLock)
        {
            userCountData = loadedData;
            isUserCountDirty = false;
        }
        
        Debug.Log($"Loaded {loadedData.Count} user count categories from CSV");
    }
    
    /// <summary>
    /// Parse button press CSV data into the list
    /// </summary>
    private static void ParseButtonPressCsvData(string csvData)
    {
        List<ButtonPressEntry> loadedData = new List<ButtonPressEntry>();
        
        using (StringReader reader = new StringReader(csvData))
        {
            // Skip header line
            reader.ReadLine();
            
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                string[] parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    string category = parts[0].Trim();
                    if (int.TryParse(parts[1].Trim(), out int userId) && 
                        DateTime.TryParse(parts[2].Trim(), out DateTime timestamp) && 
                        !string.IsNullOrEmpty(category))
                    {
                        ButtonPressEntry entry = new ButtonPressEntry(category, userId)
                        {
                            Timestamp = timestamp
                        };
                        loadedData.Add(entry);
                    }
                }
            }
        }
        
        lock (dataLock)
        {
            buttonPressData = loadedData;
            isButtonPressDirty = false;
        }
        
        Debug.Log($"Loaded {loadedData.Count} button press entries from CSV");
    }
    
    /// <summary>
    /// Save all data to CSV files
    /// </summary>
    private static void SaveAllData()
    {
        SaveUserCountData();
        SaveButtonPressData();
    }
    
    /// <summary>
    /// Save user count data to CSV file
    /// </summary>
    private static void SaveUserCountData()
    {
        // Skip saving if there's no data to save
        if (!isUserCountDirty) return;
        
        Dictionary<string, int> dataToSave;
        
        lock (dataLock)
        {
            if (userCountData.Count == 0) return;
            
            // Create a copy of the data to save
            dataToSave = new Dictionary<string, int>(userCountData);
            isUserCountDirty = false;
        }
        
        Task.Run(() => {
            try
            {
                // In WebGL, we can only write to persistentDataPath
                #if UNITY_WEBGL && !UNITY_EDITOR
                string savePath = writableUserCountPath;
                #else
                // In editor or other platforms, we can write to StreamingAssets
                string savePath = userCountPath;
                #endif
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Create CSV content
                using (StringWriter writer = new StringWriter())
                {
                    writer.WriteLine("Category,Count");
                    
                    foreach (var item in dataToSave)
                    {
                        writer.WriteLine($"{EscapeCsvField(item.Key)},{item.Value}");
                    }
                    
                    // Write to a temporary file first, then move it to avoid corruption
                    string tempFilePath = savePath + ".tmp";
                    
                    File.WriteAllText(tempFilePath, writer.ToString());
                    
                    // Replace the old file with the new one
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    
                    File.Move(tempFilePath, savePath);
                }
                
                Debug.Log("User count data saved to CSV successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving user count data: {ex.Message}");
            }
        });
    }
    
    /// <summary>
    /// Save button press data to CSV file
    /// </summary>
    private static void SaveButtonPressData()
    {
        // Skip saving if there's no data to save
        if (!isButtonPressDirty) return;
        
        List<ButtonPressEntry> dataToSave;
        
        lock (dataLock)
        {
            if (buttonPressData.Count == 0) return;
            
            // Create a copy of the data to save
            dataToSave = new List<ButtonPressEntry>(buttonPressData);
            isButtonPressDirty = false;
        }
        
        Task.Run(() => {
            try
            {
                // In WebGL, we can only write to persistentDataPath
                #if UNITY_WEBGL && !UNITY_EDITOR
                string savePath = writableButtonPressesPath;
                #else
                // In editor or other platforms, we can write to StreamingAssets
                string savePath = buttonPressesPath;
                #endif
                
                // Ensure directory exists
                string directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Create CSV content
                using (StringWriter writer = new StringWriter())
                {
                    writer.WriteLine("Category,UserId,Timestamp");
                    
                    foreach (var item in dataToSave)
                    {
                        writer.WriteLine($"{EscapeCsvField(item.Category)},{item.UserId},{item.Timestamp}");
                    }
                    
                    // Write to a temporary file first, then move it to avoid corruption
                    string tempFilePath = savePath + ".tmp";
                    
                    File.WriteAllText(tempFilePath, writer.ToString());
                    
                    // Replace the old file with the new one
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    
                    File.Move(tempFilePath, savePath);
                }
                
                Debug.Log("Button press data saved to CSV successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving button press data: {ex.Message}");
            }
        });
    }
    
    /// <summary>
    /// Escapes a CSV field by adding quotes if needed
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        
        bool needsQuotes = field.Contains(",") || field.Contains("\"") || field.Contains("\n");
        
        if (needsQuotes)
        {
            // Replace quotes with double quotes for CSV escaping
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        
        return field;
    }
}