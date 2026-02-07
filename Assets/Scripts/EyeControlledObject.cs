using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class EyeControlledObject : MonoBehaviour
{
    [Header("References")]
    public OVREyeGaze leftEye;
    public OVREyeGaze rightEye;
    public Transform gazeCursor;          // object that follows gaze

    [Header("Settings")]
    public float cursorDistance = 2f;
    public float logInterval = 0.25f;     // seconds between samples
    public string csvFileName = "eye_control_log.csv";

    // PC folder path for editor testing (change to your own)
    // Example (Windows): C:\Users\YourName\Documents\GazeFlowLogs
    // Example (macOS):  /Users/YourName/Documents/GazeFlowLogs
    public string desktopFolderPath = "C:\\Users\\lolad\\rabiatS\\projects\\VR projects\\Tartan Hacks - GazeFlow\\csv data";

    private float logTimer = 0f;

    // Each row we’ll store: time, cursorX, cursorY, cursorZ, convergenceAngle
    private List<string> rows = new List<string>();

    void Start()
    {
        rows.Add("time,cursorX,cursorY,cursorZ,convergenceAngleDeg");
    }

    void Update()
    {
        if (leftEye == null || rightEye == null || gazeCursor == null)
            return;

        if (!leftEye.EyeTrackingEnabled && !rightEye.EyeTrackingEnabled)
            return;

        Vector3 leftPos = leftEye.transform.position;
        Vector3 rightPos = rightEye.transform.position;
        Vector3 leftDir = leftEye.transform.forward.normalized;
        Vector3 rightDir = rightEye.transform.forward.normalized;

        Vector3 origin = (leftPos + rightPos) * 0.5f;
        Vector3 dir = (leftDir + rightDir).normalized;

        // Move the object with eyes
        Vector3 cursorPos = origin + dir * cursorDistance;
        gazeCursor.position = cursorPos;

        // Convergence angle
        float angle = Vector3.Angle(leftDir, rightDir);

        // Sample at intervals
        logTimer += Time.deltaTime;
        if (logTimer >= logInterval)
        {
            logTimer = 0f;

            string row = string.Format(
                "{0:F3},{1:F4},{2:F4},{3:F4},{4:F2}",
                Time.time,
                cursorPos.x, cursorPos.y, cursorPos.z,
                angle
            );
            rows.Add(row);

            Debug.Log($"Eye sample → pos=({cursorPos.x:F2},{cursorPos.y:F2},{cursorPos.z:F2}), angle={angle:F2}°");
        }
    }

    void OnApplicationQuit()
    {
        SaveCsv();
    }

    public void SaveCsv()
    {
        string path;

        // If we're running in the editor, save to a desktop folder on the laptop
#if UNITY_EDITOR
        if (!string.IsNullOrEmpty(desktopFolderPath))
        {
            if (!Directory.Exists(desktopFolderPath))
                Directory.CreateDirectory(desktopFolderPath);

            path = Path.Combine(desktopFolderPath, csvFileName);
        }
        else
        {
            // Fallback to persistentDataPath if no desktop path set
            path = Path.Combine(Application.persistentDataPath, csvFileName);
        }
#else
        // In a build (e.g., on Quest), always use persistentDataPath
        path = Path.Combine(Application.persistentDataPath, csvFileName);
#endif

        File.WriteAllLines(path, rows.ToArray());
        Debug.Log("Eye control log saved to: " + path);
    }
}
