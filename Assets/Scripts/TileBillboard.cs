using UnityEngine;

// Makes tiles always face the camera (optional alternative to double-sided)
public class TileBillboard : MonoBehaviour
{
    [Header("Settings")]
    public bool billboardX = false;
    public bool billboardY = true;
    public bool billboardZ = false;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
    }
    
    void LateUpdate()
    {
        if (mainCamera == null) return;
        
        Vector3 direction = transform.position - mainCamera.transform.position;
        
        if (!billboardY) direction.y = 0;
        if (!billboardX) direction.x = 0;
        if (!billboardZ) direction.z = 0;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
