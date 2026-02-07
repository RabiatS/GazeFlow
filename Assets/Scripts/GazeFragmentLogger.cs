using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

// Starter script - Planning to test with quest 3 then With Pro for tracking latr
// Reminder to add tooltip for easier expl


// Logs when the user looks at a target, for how long, and saves to CSV.
public class GazeFragmentLogger : MonoBehaviour
{
    [Header("References")]
    public Transform cameraRig;      // CenterEyeAnchor or camera
    public Transform target;         // The object to look at

    [Header("Settings")]
    public float hitRadius = 0.3f;   // How close to "center" counts as looking
    public string csvFileName = "gaze_log.csv";
    public string desktopFolderPath = "C:\\Users\\lolad\\rabiatS\\projects\\VR projects\\Tartan Hacks - GazeFlow\\csv data";

    // Internal state
    private float fixationTime = 0f;
    private bool isLooking = false;

    // In‑memory log of events
    private List<string> rows = new List<string>();

    void Start()
    {
        // CSV header row
        rows.Add("timestamp,event,fixationTime");
    }

    void Update()
    {
        if (cameraRig == null || target == null) return;

        Vector3 origin = cameraRig.position;
        Vector3 dir = cameraRig.forward;

        Vector3 toTarget = (target.position - origin).normalized;
        float dot = Vector3.Dot(dir, toTarget);

        bool lookingNow = dot > 1f - hitRadius;

        if (lookingNow)
        {
            // Started looking this frame
            if (!isLooking)
            {
                isLooking = true;
                fixationTime = 0f;
                Debug.Log("Started looking at target");
                rows.Add($"{Time.time:F3},Start,0");
            }

            fixationTime += Time.deltaTime;
            Debug.Log($"Looking at target, fixationTime = {fixationTime:F2}s");
        }
        else
        {
            // Stopped looking this frame
            if (isLooking)
            {
                isLooking = false;
                Debug.Log($"Stopped looking at target, total fixation = {fixationTime:F2}s");
                rows.Add($"{Time.time:F3},Stop,{fixationTime:F3}");
                fixationTime = 0f;
            }
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