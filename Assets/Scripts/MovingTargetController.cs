using UnityEngine;

public class MovingTargetController : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;         // e.g., CenterEyeAnchor from OVRCameraRig

    [Header("Spawn Settings")]
    public float initialDistance = 1.5f; // how far in front at start (meters)
    public float heightOffset = 0.0f;    // vertical offset from head height

    [Header("Step Settings")]
    public float waitSeconds = 3f;       // time to wait before moving further
    public float stepDistance = 0.5f;    // how much farther each step (meters)

    private float timer = 0f;

    void Start()
    {
        if (playerHead == null)
        {
            Debug.LogWarning("MovingTargetController: playerHead not assigned.");
            enabled = false;
            return;
        }

        // Place object in front of the user at the initial distance
        PositionInFront(initialDistance);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= waitSeconds)
        {
            // Move the object farther away by stepDistance
            initialDistance += stepDistance;
            PositionInFront(initialDistance);

            // Reset timer for the next step
            timer = 0f;
        }
    }

    void PositionInFront(float distance)
    {
        // Forward direction from the head
        Vector3 forward = playerHead.forward.normalized;

        // Base position in front of the head
        Vector3 targetPos = playerHead.position + forward * distance;

        // Optional: adjust height
        targetPos.y += heightOffset;

        transform.position = targetPos;
    }
}
