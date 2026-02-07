using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;



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

    void Start()
    {
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
        if (summaryCanvas != null)
        {
            summaryCanvas.gameObject.SetActive(true);
            summaryCanvas.enabled = true;

            // Position canvas in front of camera
            if (headCam != null)
            {
                summaryCanvas.transform.position = headCam.position + headCam.forward * 2f;
                summaryCanvas.transform.LookAt(headCam);
                summaryCanvas.transform.Rotate(0f, 180f, 0f); // Face the camera
            }

            if (tilesCompletedText != null)
            {
                tilesCompletedText.text = $"Tiles Completed: {completed}";
                tilesCompletedText.gameObject.SetActive(true);
            }

            if (avgDwellText != null)
            {
                avgDwellText.text = $"Average Dwell Time: {avgDwell:F2}s";
                avgDwellText.gameObject.SetActive(true);
            }
            
            Debug.Log($"Summary canvas shown! Completed: {completed}, Avg Dwell: {avgDwell:F2}s");
        }
        else
        {
            Debug.LogWarning("Summary canvas is not assigned!");
        }
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
}
