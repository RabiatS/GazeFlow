using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
        string path = Path.Combine(Application.persistentDataPath, csvFileName);
        File.WriteAllLines(path, rows.ToArray());
        Debug.Log($"Gaze log saved to: {path}");
    }
}

