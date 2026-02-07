using UnityEngine;

public class SimpleEyeCheck : MonoBehaviour
{
    public OVREyeGaze leftEye;

    void Update()
    {
        if (leftEye == null)
        {
            Debug.Log("SimpleEyeCheck: leftEye is NULL");
            return;
        }

        Debug.Log("SimpleEyeCheck: EyeTrackingEnabled = " + leftEye.EyeTrackingEnabled);
    }
}
