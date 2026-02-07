using UnityEngine;
using System.Collections.Generic;

// Creates a beautiful starfield around the scene
public class StarFieldGenerator : MonoBehaviour
{
    [Header("Star Settings")]
    public int starCount = 200;
    public float minDistance = 5f;
    public float maxDistance = 20f;
    public float minSize = 0.01f;
    public float maxSize = 0.05f;
    
    [Header("Materials")]
    public Material starMaterial;
    
    [Header("Animation")]
    public float twinkleSpeed = 2f;
    public float twinkleAmount = 0.5f;
    public bool rotateStars = true;
    public float rotationSpeed = 5f;
    
    private List<GameObject> stars = new List<GameObject>();
    private List<Vector3> starPositions = new List<Vector3>();
    private List<float> twinkleOffsets = new List<float>();
    
    void Start()
    {
        GenerateStars();
    }
    
    void GenerateStars()
    {
        // Create default material if none assigned
        if (starMaterial == null)
        {
            starMaterial = new Material(Shader.Find("Unlit/Color"));
            starMaterial.color = Color.white;
        }
        
        for (int i = 0; i < starCount; i++)
        {
            // Random position in sphere
            Vector3 randomDir = Random.onUnitSphere;
            float distance = Random.Range(minDistance, maxDistance);
            Vector3 position = randomDir * distance;
            
            // Create star
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = $"Star_{i}";
            star.transform.SetParent(transform);
            star.transform.position = position;
            
            float size = Random.Range(minSize, maxSize);
            star.transform.localScale = Vector3.one * size;
            
            // Set material
            Renderer renderer = star.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = starMaterial;
                renderer.material.color = new Color(1f, 1f, Random.Range(0.8f, 1f), 1f); // Slight color variation
            }
            
            // Remove collider (we don't need it for stars)
            Collider col = star.GetComponent<Collider>();
            if (col != null)
                Destroy(col);
            
            stars.Add(star);
            starPositions.Add(position);
            twinkleOffsets.Add(Random.Range(0f, Mathf.PI * 2f));
        }
    }
    
    void Update()
    {
        // Twinkle effect
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null)
            {
                float twinkle = Mathf.Sin(Time.time * twinkleSpeed + twinkleOffsets[i]) * twinkleAmount + 1f;
                stars[i].transform.localScale = Vector3.one * (starPositions[i].magnitude / maxDistance * (maxSize - minSize) + minSize) * twinkle;
                
                // Rotate around center
                if (rotateStars)
                {
                    stars[i].transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        foreach (var star in stars)
        {
            if (star != null)
                Destroy(star);
        }
    }
}
