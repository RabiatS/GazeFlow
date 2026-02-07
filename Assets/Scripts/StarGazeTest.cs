using UnityEngine;
using System.Collections.Generic;
using System.IO;

// Attach this to an empty GameObject (e.g., "StarGazeManager").
// In the Inspector:
// - Drag LeftEyeAnchor (with OVREyeGaze) to leftEye
// - Drag RightEyeAnchor to rightEye
// - Drag your star sphere objects into the "stars" list
// - (Optional) Drag a small sphere into gazeCursor for a visible gaze pointer
public class StarGazeTest : MonoBehaviour
{
    [Header("References")]
    public OVREyeGaze leftEye;
    public OVREyeGaze rightEye;
    public List<Transform> stars = new List<Transform>();  // stars in the order you want
    public Transform gazeCursor;                           // optional small sphere

    [Header("Settings")]
    public float maxRayDistance = 10f;
    public float starHighlightScale = 0.1f;
    public float starNormalScale = 0.05f;
    public float maxWaitPerStar = 3f;       // seconds to wait for gaze before moving on
    public string csvFileName = "stargaze_log.csv";
    public string desktopFolderPath = "C:\\Users\\lolad\\rabiatS\\projects\\VR projects\\Tartan Hacks - GazeFlow\\csv data";

    private int currentIndex = -1;
    private float starStartTime = 0f;
    private bool waitingForGaze = false;

    // CSV: starIndex,startTime,firstLookTime,reactionTime,hit
    private List<string> rows = new List<string>();

    void Start()
    {
        rows.Add("starIndex,startTime,firstLookTime,reactionTime,hit");
        SetAllStarScale(starNormalScale);
        NextStar();
    }

    void Update()

    {

        Debug.Log("StarGazeTest Update running");
        // 1) Basic wiring checks
        if (leftEye == null || rightEye == null || stars.Count == 0)
        {
            Debug.LogWarning("StarGazeTest: missing refs or no stars assigned.");
            return;
        }

        // 2) Eye tracking on?
        if (!leftEye.EyeTrackingEnabled && !rightEye.EyeTrackingEnabled)
        {
            Debug.LogWarning("StarGazeTest: eye tracking not enabled on Quest Pro.");
            return;
        }

        if (!waitingForGaze)
            return;

        // 3) Eye ray
        Vector3 origin =
            (leftEye.transform.position + rightEye.transform.position) * 0.5f;
        Vector3 dir =
            (leftEye.transform.forward + rightEye.transform.forward).normalized;

        if (gazeCursor != null)
        {
            gazeCursor.position = origin + dir * 2f;
        }

        if (currentIndex < 0 || currentIndex >= stars.Count)
        {
            Debug.LogWarning("StarGazeTest: currentIndex out of range.");
            return;
        }

        Transform targetStar = stars[currentIndex];

        Ray ray = new Ray(origin, dir);
        bool hitStar = false;
        string hitName = "nothing";

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            hitName = hit.transform.name;
            if (hit.transform == targetStar)
                hitStar = true;
        }

        // Debug every frame so you can see if ray is even touching anything
        Debug.Log($"StarGazeTest: looking at {hitName}, target = {targetStar.name}");

        float now = Time.time;

        if (hitStar)
        {
            float firstLookTime = now;
            float reactionTime = firstLookTime - starStartTime;

            rows.Add($"{currentIndex},{starStartTime:F3},{firstLookTime:F3},{reactionTime:F3},true");
            Debug.Log($"Star {currentIndex} HIT in {reactionTime:F3}s");

            waitingForGaze = false;
            NextStar();
        }
        else if (now - starStartTime > maxWaitPerStar)
        {
            // Timed out without looking at the star
            rows.Add($"{currentIndex},{starStartTime:F3},,,false");
            Debug.Log($"Star {currentIndex} MISSED (timeout)");

            waitingForGaze = false;
            NextStar();
        }
    }


    void NextStar()
    {
        SetAllStarScale(starNormalScale);

        currentIndex++;
        if (currentIndex >= stars.Count)
        {
            Debug.Log("StarGazeTest finished all stars.");
            SaveCsv();
            return;
        }

        Transform s = stars[currentIndex];
        s.localScale = Vector3.one * starHighlightScale;   // make active star bigger/brighter
        starStartTime = Time.time;
        waitingForGaze = true;

        Debug.Log($"Star {currentIndex} activated at t={starStartTime:F3}");
    }

    void SetAllStarScale(float scale)
    {
        foreach (Transform t in stars)
        {
            if (t != null)
                t.localScale = Vector3.one * scale;
        }
    }

    void OnApplicationQuit()
    {
        SaveCsv();
    }

    public void SaveCsv()
    {
        string path;

#if UNITY_EDITOR
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
        path = Path.Combine(Application.persistentDataPath, csvFileName);
#endif

        File.WriteAllLines(path, rows.ToArray());
        Debug.Log("Star gaze log saved to: " + path);
    }
}
