using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif



public class MosaicGazeManager1 : MonoBehaviour
{
    [Header("Gaze Sources")]
    public OVREyeGaze leftEye;      // Changed from Transform to OVREyeGaze
    public OVREyeGaze rightEye;      // Changed from Transform to OVREyeGaze
    public Transform headCam;        // Camera.main transform (fallback)

    [Header("Gaze Settings")]
    public float maxRayDistance = 10f;
    public LayerMask tileLayer;
    public float dwellThreshold = 0.7f;   // seconds to activate

    [Header("Tiles")]
    public GazeTile[] tiles;

    [Header("UI Summary")]
    public Canvas summaryCanvas;     // world-space
    public TextMeshProUGUI tilesCompletedText;
    public TextMeshProUGUI avgDwellText;
    public TextMeshProUGUI percentageText;      // NEW: Percentage display
    public TextMeshProUGUI analysisText;        // NEW: Analysis/metrics display
    public SummaryUIController summaryUIController; // Optional better UI controller
    public float sessionDuration = 90f;   // seconds

    [Header("Debug")]
    public bool useHeadDirectionFallback = true;
    public Transform debugGazeCursor;     // optional small sphere
    public bool showDebugLogs = false;

    GazeTile _currentTile;
    float _currentGazeStartTime;
    float _sessionStartTime;
    bool _sessionEnded;

    int _completedCount;
    float _dwellSum;

    string _csvPath;
    StringBuilder _csvBuilder = new StringBuilder();

    public AudioSource completionSource;
    public AudioClip completionClip;
    
    bool _usingEyeTracking = false;
    
    // Metrics tracking for analysis
    List<Vector3> _gazePositions = new List<Vector3>();
    List<float> _gazeTimestamps = new List<float>();
    List<float> _saccadeSpeeds = new List<float>();
    float _totalGazeDistance = 0f;
    Vector3 _lastGazePosition = Vector3.zero;
    float _lastGazeTime = 0f;
    int _totalSaccades = 0;

    void Start()
    {
        // Ensure game is not paused
        Time.timeScale = 1f;
        
        // Ensure XR is enabled (safety check for Quest)
        if (!XRSettings.enabled)
        {
            Debug.LogWarning("MosaicGazeManager: XR not enabled! Attempting to enable...");
            XRSettings.enabled = true;
        }

        if (headCam == null && Camera.main != null)
            headCam = Camera.main.transform;

        // Check if eye tracking is available
        _usingEyeTracking = CheckEyeTrackingAvailable();
        
        // Initialize session state
        _sessionEnded = false;
        _currentTile = null;
        _currentGazeStartTime = 0f;
        _completedCount = 0;
        _dwellSum = 0f;
        
        if (showDebugLogs)
        {
            Debug.Log($"MosaicGazeManager: XR enabled = {XRSettings.enabled}");
            Debug.Log($"MosaicGazeManager: Eye tracking available = {_usingEyeTracking}");
            if (!_usingEyeTracking && useHeadDirectionFallback)
                Debug.Log("MosaicGazeManager: Using head direction fallback");
        }

        _sessionStartTime = Time.time;

        if (summaryCanvas != null)
            summaryCanvas.enabled = false;

        InitCsv();
    }

    bool CheckEyeTrackingAvailable()
    {
        // Check if we have OVREyeGaze components and they're enabled
        if (leftEye != null && rightEye != null)
        {
            // Try to check if eye tracking is enabled
            // Note: EyeTrackingEnabled might not be available immediately, so we'll check in Update too
            return true; // We'll verify in Update
        }
        return false;
    }

    void Update()
    {
        // Check for restart key (R key on keyboard)
        bool restartPressed = false;
        
        #if ENABLE_INPUT_SYSTEM
        // Use new Input System (Unity is configured for this)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            restartPressed = true;
        }
        #else
        // Legacy Input System fallback
        restartPressed = Input.GetKeyDown(KeyCode.R);
        #endif
        
        if (restartPressed)
        {
            RestartSession();
            return;
        }
        
        if (_sessionEnded) return;

        if (Time.time - _sessionStartTime >= sessionDuration)
        {
            EndSession();
            return;
        }

        Vector3 origin = Vector3.zero;
        Vector3 direction = Vector3.forward;
        bool validGaze = false;

        // Try to use eye tracking first
        if (leftEye != null && rightEye != null)
        {
            // Check if eye tracking is actually enabled
            bool leftEnabled = leftEye.EyeTrackingEnabled;
            bool rightEnabled = rightEye.EyeTrackingEnabled;
            
            if (leftEnabled || rightEnabled)
            {
                Vector3 leftPos = leftEye.transform.position;
                Vector3 rightPos = rightEye.transform.position;
                Vector3 leftDir = leftEye.transform.forward.normalized;
                Vector3 rightDir = rightEye.transform.forward.normalized;

                origin = (leftPos + rightPos) * 0.5f;
                direction = ((leftDir + rightDir) * 0.5f).normalized;
                validGaze = true;
                _usingEyeTracking = true;
            }
        }

        // Fallback to head direction if eye tracking not available
        if (!validGaze && useHeadDirectionFallback && headCam != null)
        {
            origin = headCam.position;
            direction = headCam.forward;
            validGaze = true;
            _usingEyeTracking = false;
        }

        if (!validGaze)
        {
            if (showDebugLogs)
                Debug.LogWarning("MosaicGazeManager: No valid gaze source available!");
            return;
        }

        if (debugGazeCursor != null)
            debugGazeCursor.position = origin + direction * 2f;

        // Track gaze metrics for analysis
        TrackGazeMetrics(origin + direction * 2f);

        RaycastHit hit;
        // If tileLayer is empty (0), raycast on all layers
        int layerMask = (tileLayer.value == 0) ? -1 : tileLayer.value;
        
        if (Physics.Raycast(origin, direction, out hit, maxRayDistance, layerMask))
        {
            if (showDebugLogs)
                Debug.Log($"Raycast hit: {hit.collider.name} at distance {hit.distance:F2}m");
            
            var hitTile = hit.collider.GetComponent<GazeTile>();
            if (hitTile != null)
            {
                if (showDebugLogs)
                    Debug.Log($"Found GazeTile component on {hit.collider.name}");
                HandleGazeOnTile(hitTile);
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"Hit {hit.collider.name} but no GazeTile component found!");
                ClearCurrentTile();
            }
        }
        else
        {
            if (showDebugLogs && Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
                Debug.Log($"No raycast hit. Origin: {origin}, Direction: {direction}, MaxDist: {maxRayDistance}");
            ClearCurrentTile();
        }
    }

    void HandleGazeOnTile(GazeTile hitTile)
    {
        if (_currentTile != hitTile)
        {
            if (_currentTile != null)
            {
                if (showDebugLogs)
                    Debug.Log($"Switching from tile {_currentTile.tileIndex} to {hitTile.tileIndex}");
                _currentTile.OnGazeExit();
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"Started looking at tile {hitTile.tileIndex}");
            }

            _currentTile = hitTile;
            _currentGazeStartTime = Time.time;
            _currentTile.OnGazeEnter(Time.time);
        }
        else
        {
            if (_currentTile.isCompleted) return;

            float dwell = Time.time - _currentGazeStartTime;
            if (showDebugLogs && Time.frameCount % 30 == 0) // Log every 30 frames
                Debug.Log($"Looking at tile {_currentTile.tileIndex}, dwell: {dwell:F2}s / {dwellThreshold}s");
            
            if (dwell >= dwellThreshold)
            {
                // capture values before callback
                float dwellCopy = dwell;
                _currentTile.OnGazeDwellComplete(
                    dwellCopy,
                    (tile, firstLookTime, dwellDur) =>
                    {
                        _completedCount++;
                        _dwellSum += dwellDur;
                        LogTileActivation(tile.tileIndex, firstLookTime, dwellDur);
                        OnTileCompleted();
                    });

                _currentTile = null;
                _currentGazeStartTime = 0f;
            }
        }
    }

    void ClearCurrentTile()
    {
        if (_currentTile != null)
            _currentTile.OnGazeExit();

        _currentTile = null;
        _currentGazeStartTime = 0f;
    }

    void InitCsv()
    {
        string dir = Application.persistentDataPath;
        string fileName = "MosaicGaze_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        _csvPath = Path.Combine(dir, fileName);

        _csvBuilder.AppendLine("tileIndex,firstLookTime,dwellDuration");
        File.WriteAllText(_csvPath, _csvBuilder.ToString(), Encoding.UTF8);
        
        if (showDebugLogs)
            Debug.Log($"CSV initialized at: {_csvPath}");
    }

    void LogTileActivation(int tileIndex, float firstLookTime, float dwellDuration)
    {
        string row = $"{tileIndex},{firstLookTime:F3},{dwellDuration:F3}";
        File.AppendAllText(_csvPath, row + "\n", Encoding.UTF8);
        
        if (showDebugLogs)
            Debug.Log($"Tile {tileIndex} completed! Dwell: {dwellDuration:F2}s");
    }

    void EndSession()
    {
        if (completionSource != null && completionClip != null)
        {
            completionSource.PlayOneShot(completionClip, 0.8f);
        }

        _sessionEnded = true;
        ClearCurrentTile();

        float avgDwell = _completedCount > 0 ? _dwellSum / _completedCount : 0f;

        // Show summary canvas
        ShowSummaryCanvas(_completedCount, avgDwell);
        
        Debug.Log($"Session ended! Completed {_completedCount} tiles, avg dwell: {avgDwell:F2}s");
    }
    
    void ShowSummaryCanvas(int completed, float avgDwell)
    {
        // Use better UI controller if available
        if (summaryUIController != null)
        {
            summaryUIController.ShowSummary(completed, tiles.Length, avgDwell);
            Debug.Log($"Summary UI shown via controller! Completed: {completed}, Avg Dwell: {avgDwell:F2}s");
            return;
        }
        
        // Fallback to original method
        if (summaryCanvas != null)
        {
            summaryCanvas.gameObject.SetActive(true);
            summaryCanvas.enabled = true;

            // Position canvas in front of camera with better spacing
            if (headCam != null)
            {
                summaryCanvas.transform.position = headCam.position + headCam.forward * 2.5f;
                summaryCanvas.transform.LookAt(headCam);
                summaryCanvas.transform.Rotate(0f, 180f, 0f); // Face the camera
                
                // Scale canvas to be more readable (adjust based on your canvas setup)
                summaryCanvas.transform.localScale = Vector3.one * 0.002f;
            }

            // Calculate completion percentage
            float completionPercent = tiles.Length > 0 ? (float)completed / tiles.Length * 100f : 0f;
            
            // Calculate analysis metrics
            float avgSaccadeSpeed = CalculateAverageSaccadeSpeed();
            float totalGazeDistance = CalculateTotalGazeDistance();
            float avgLookSpeed = CalculateAverageLookSpeed();
            
            // Tiles Completed Text
            if (tilesCompletedText != null)
            {
                string emoji = GetCompletionEmoji(completionPercent);
                tilesCompletedText.text = $"{emoji}\n\n<size=64>Tiles Completed</size>\n<size=88><b>{completed}</b></size>\n<size=44>out of {tiles.Length}</size>";
                tilesCompletedText.gameObject.SetActive(true);
                tilesCompletedText.alignment = TMPro.TextAlignmentOptions.Center;
                tilesCompletedText.textWrappingMode = TMPro.TextWrappingModes.Normal;
                tilesCompletedText.lineSpacing = 8f;
            }

            // Percentage Text (NEW)
            if (percentageText != null)
            {
                string performance = GetPerformanceRating(completionPercent);
                percentageText.text = $"<size=80><b>{completionPercent:F0}%</b></size>\n<size=52>{performance}</size>";
                percentageText.gameObject.SetActive(true);
                percentageText.alignment = TMPro.TextAlignmentOptions.Center;
                percentageText.textWrappingMode = TMPro.TextWrappingModes.Normal;
                percentageText.lineSpacing = 5f;
            }

            // Average Dwell Text
            if (avgDwellText != null)
            {
                string speedEmoji = GetSpeedEmoji(avgDwell);
                avgDwellText.text = $"{speedEmoji}\n\n<size=56>Average Focus</size>\n<size=76><b>{avgDwell:F2}s</b></size>\n<size=44>per tile</size>";
                avgDwellText.gameObject.SetActive(true);
                avgDwellText.alignment = TMPro.TextAlignmentOptions.Center;
                avgDwellText.textWrappingMode = TMPro.TextWrappingModes.Normal;
                avgDwellText.lineSpacing = 8f;
            }
            
            // Analysis Text (NEW)
            if (analysisText != null)
            {
                string analysis = BuildAnalysisText(avgSaccadeSpeed, totalGazeDistance, avgLookSpeed, completed);
                analysisText.text = analysis;
                analysisText.gameObject.SetActive(true);
                analysisText.alignment = TMPro.TextAlignmentOptions.Center;
                analysisText.textWrappingMode = TMPro.TextWrappingModes.Normal;
                analysisText.lineSpacing = 6f;
            }
            
            Debug.Log($"Summary canvas shown! Completed: {completed}, Avg Dwell: {avgDwell:F2}s");
        }
        else
        {
            Debug.LogWarning("Summary canvas is not assigned!");
        }
    }
    
    void TrackGazeMetrics(Vector3 currentGazePos)
    {
        float currentTime = Time.time;
        
        // Store gaze position
        _gazePositions.Add(currentGazePos);
        _gazeTimestamps.Add(currentTime);
        
        // Calculate saccade speed (movement between gaze points)
        if (_lastGazeTime > 0f && Vector3.Distance(_lastGazePosition, currentGazePos) > 0.1f)
        {
            float distance = Vector3.Distance(_lastGazePosition, currentGazePos);
            float timeDelta = currentTime - _lastGazeTime;
            
            if (timeDelta > 0f)
            {
                float speed = distance / timeDelta; // units per second
                _saccadeSpeeds.Add(speed);
                _totalGazeDistance += distance;
                _totalSaccades++;
            }
        }
        
        _lastGazePosition = currentGazePos;
        _lastGazeTime = currentTime;
        
        // Keep only recent data (last 10 seconds worth)
        if (_gazeTimestamps.Count > 0 && currentTime - _gazeTimestamps[0] > 10f)
        {
            _gazePositions.RemoveAt(0);
            _gazeTimestamps.RemoveAt(0);
        }
    }
    
    float CalculateAverageSaccadeSpeed()
    {
        if (_saccadeSpeeds.Count == 0) return 0f;
        float sum = 0f;
        foreach (float speed in _saccadeSpeeds)
            sum += speed;
        return sum / _saccadeSpeeds.Count;
    }
    
    float CalculateTotalGazeDistance()
    {
        return _totalGazeDistance;
    }
    
    float CalculateAverageLookSpeed()
    {
        if (_completedCount == 0) return 0f;
        float sessionTime = Time.time - _sessionStartTime;
        return _completedCount / sessionTime; // tiles per second
    }
    
    string BuildAnalysisText(float avgSaccadeSpeed, float totalDistance, float lookSpeed, int completed)
    {
        string text = "<size=52><b>Your Gaze Analysis</b></size>\n\n";
        
        // Look speed
        text += $"<size=44>Look Speed:</size>\n<size=56><b>{lookSpeed:F2}</b> tiles/sec</size>\n\n";
        
        // Saccade speed
        if (avgSaccadeSpeed > 0f)
        {
            string speedRating = avgSaccadeSpeed > 5f ? "Fast" : avgSaccadeSpeed > 2f ? "Moderate" : "Calm";
            text += $"<size=44>Eye Movement:</size>\n<size=56><b>{speedRating}</b></size>\n";
            text += $"<size=40>({avgSaccadeSpeed:F1} units/sec)</size>\n\n";
        }
        
        // Exploration
        text += $"<size=44>Total Exploration:</size>\n<size=56><b>{totalDistance:F1}</b> units</size>\n\n";
        
        // Efficiency
        float efficiency = completed > 0 ? (float)_completedCount / (_totalSaccades + 1) : 0f;
        text += $"<size=44>Focus Efficiency:</size>\n<size=56><b>{efficiency:F2}</b></size>";
        
        return text;
    }
    
    string GetPerformanceRating(float percent)
    {
        if (percent >= 90f) return "Excellent! 🌟";
        if (percent >= 75f) return "Great Job! ✨";
        if (percent >= 60f) return "Good Work! ⭐";
        if (percent >= 40f) return "Nice Try! 💫";
        return "Keep Going! 🌙";
    }
    
    string GetSpeedEmoji(float avgDwell)
    {
        if (avgDwell < 0.8f) return "⚡";
        if (avgDwell < 1.2f) return "✨";
        if (avgDwell < 1.8f) return "🌟";
        return "💫";
    }
    
    string GetCompletionEmoji(float percent)
    {
        if (percent >= 100f) return "🎉";
        if (percent >= 80f) return "🌟";
        if (percent >= 60f) return "✨";
        if (percent >= 40f) return "⭐";
        if (percent >= 20f) return "💫";
        return "🌙";
    }
    
    // Call this when a tile completes to show progress
    public void OnTileCompleted()
    {
        // Optionally show progress updates
        if (showDebugLogs)
        {
            float progress = (float)_completedCount / tiles.Length * 100f;
            Debug.Log($"Progress: {_completedCount}/{tiles.Length} tiles ({progress:F1}%)");
        }
    }
    
    public void RestartSession()
    {
        Debug.Log("Restarting session...");
        
        // Reset session state
        _sessionEnded = false;
        _sessionStartTime = Time.time;
        _completedCount = 0;
        _dwellSum = 0f;
        _currentTile = null;
        _currentGazeStartTime = 0f;
        
        // Reset metrics
        _gazePositions.Clear();
        _gazeTimestamps.Clear();
        _saccadeSpeeds.Clear();
        _totalGazeDistance = 0f;
        _lastGazePosition = Vector3.zero;
        _lastGazeTime = 0f;
        _totalSaccades = 0;
        
        // Hide summary canvas
        if (summaryCanvas != null)
        {
            summaryCanvas.gameObject.SetActive(false);
            summaryCanvas.enabled = false;
        }
        
        // Reset all tiles
        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                tile.ResetTile();
            }
        }
        
        // Reinitialize CSV
        InitCsv();
        
        Debug.Log("Session restarted! Press R again to restart anytime.");
    }
}
