using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;

// Attach this to an empty GameObject in your scene.
// In the Inspector, drag in the LeftEyeAnchor and RightEyeAnchor
// (each must have an OVREyeGaze component on it).
public class EyeTrackingDebugger : MonoBehaviour
{
    [Header("References")]
    public OVREyeGaze leftEye;
    public OVREyeGaze rightEye;

    [Header("Settings")]
    public float logInterval = 0.25f;    // seconds between samples
    public float maxRayDistance = 5f;    // how far to raycast for hit info
    public string csvFileName = "eye_debug_log.csv";

    // PC folder path for editor testing (update to your own path)
    public string desktopFolderPath = "C:\\Users\\lolad\\rabiatS\\projects\\VR projects\\Tartan Hacks - GazeFlow\\csv data";

    private float timer = 0f;

    // time,dirX,dirY,dirZ,hitName,hitDistance
    private List<string> rows = new List<string>();

    void Start()
    {
        rows.Add("time,dirX,dirY,dirZ,hitName,hitDistance");
    }

    void Update()
    {
        if (leftEye == null || rightEye == null) return;

        // Only run if eye tracking is active
        if (!leftEye.EyeTrackingEnabled && !rightEye.EyeTrackingEnabled)
            return;

        timer += Time.deltaTime;
        if (timer < logInterval) return;
        timer = 0f;

        // Average position + direction of both eyes
        Vector3 origin =
            (leftEye.transform.position + rightEye.transform.position) * 0.5f;
        Vector3 dir =
            (leftEye.transform.forward + rightEye.transform.forward).normalized;

        // Raycast to see what you're looking at
        Ray ray = new Ray(origin, dir);
        string hitName = "nothing";
        float hitDist = maxRayDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            hitName = hit.collider.name;
            hitDist = hit.distance;
        }

        // Build a simple, human‑readable line for the Console
        StringBuilder sb = new StringBuilder();
        sb.Append("Eye Gaze → ");
        sb.Append($"dir = ({dir.x:F2}, {dir.y:F2}, {dir.z:F2}) | ");
        sb.Append($"hit = {hitName} @ {hitDist:F2}m");
        Debug.Log(sb.ToString());

        // Store a CSV row as well
        string row = string.Format(
            "{0:F3},{1:F4},{2:F4},{3:F4},{4},{5:F3}",
            Time.time,
            dir.x, dir.y, dir.z,
            hitName,
            hitDist
        );
        rows.Add(row);
    }

    void OnApplicationQuit()
    {
        SaveCsv();
    }

    public void SaveCsv()
    {
        string path;

#if UNITY_EDITOR
        // Save to a desktop folder when running in the editor
        if (!string.IsNullOrEmpty(desktopFolderPath))
        {
            if (!Directory.Exists(desktopFolderPath))
                Directory.CreateDirectory(desktopFolderPath);

            path = Path.Combine(desktopFolderPath, csvFileName);
        }
        else
        {
            path = Path.Combine(Application.persistentDataPath, csvFileName);
        }
#else
        // In builds (e.g., on Quest), use persistentDataPath
        path = Path.Combine(Application.persistentDataPath, csvFileName);
#endif

        File.WriteAllLines(path, rows.ToArray());
        Debug.Log("Eye debug log saved to: " + path);
    }
}
