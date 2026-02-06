using UnityEngine;


// Starter script - Planning to test with quest 3 then With Pro for tracking latr
// Reminder to add tooltip for easier expl
public class GazeFragmentLogger : MonoBehaviour
{
    public Transform cameraRig;      // Assign OVRCameraRig center eye
    public Transform target;         // Assign a cube/sphere in front of user
    public float hitRadius = 0.3f;

    private float fixationTime = 0f;

    void Update()
    {
        if (cameraRig == null || target == null) return;

        Vector3 origin = cameraRig.position;
        Vector3 dir = cameraRig.forward;

        // Simple gaze "hit" based on angle
        Vector3 toTarget = (target.position - origin).normalized;
        float dot = Vector3.Dot(dir, toTarget);

        if (dot > 1f - hitRadius)      // looking roughly at target
        {
            fixationTime += Time.deltaTime;
        }
        else
        {
            fixationTime = 0f;
        }
    }
}
