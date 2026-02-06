using UnityEngine;
using TMPro;

public class LetterChartController : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;        // CenterEyeAnchor
    public TextMeshPro textMesh;        // Your letter chart text

    [Header("Test Settings")]
    public float startDistance = 1.5f;  // meters
    public float distanceStep = 0.5f;   // meters per step
    public float fontStartSize = 8f;    // TextMeshPro font size
    public float fontStep = -0.5f;      // change per step (negative to shrink)
    public float stepInterval = 4f;     // seconds between changes

    private float timer = 0f;
    private int level = 0;

    void Start()
    {
        if (playerHead == null || textMesh == null)
        {
            Debug.LogWarning("LetterChartController: missing references.");
            enabled = false;
            return;
        }

        level = 0;
        PositionChart();
        textMesh.fontSize = fontStartSize;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= stepInterval)
        {
            level++;
            // Move farther
            startDistance += distanceStep;
            PositionChart();
            // Shrink letters (optional)
            textMesh.fontSize += fontStep;

            timer = 0f;
            Debug.Log($"Letter level: {level}, distance: {startDistance}");
        }
    }

    void PositionChart()
    {
        Vector3 forward = playerHead.forward.normalized;
        Vector3 pos = playerHead.position + forward * startDistance;
        transform.position = pos;
        transform.LookAt(playerHead);
        transform.rotation = Quaternion.LookRotation(transform.position - playerHead.position);
    }
}
