using UnityEngine;
using System.Collections.Generic;

// Arranges tiles in a beautiful pattern around the player
public class TileLayoutManager : MonoBehaviour
{
    [Header("Layout Settings")]
    public float radius = 3f;              // Distance from center
    public float heightRange = 2f;        // Vertical spread
    public int tilesPerRow = 3;            // Tiles per horizontal row
    public float tileSpacing = 1.5f;      // Space between tiles
    public bool arrangeInSphere = true;    // Sphere vs grid layout
    
    [Header("Animation")]
    public float floatSpeed = 0.5f;
    public float floatAmount = 0.2f;
    public float rotationSpeed = 10f;
    
    private List<Transform> tiles = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private List<Vector3> basePositions = new List<Vector3>();
    
    void Start()
    {
        // Find all tiles
        GazeTile[] gazeTiles = FindObjectsOfType<GazeTile>();
        foreach (var tile in gazeTiles)
        {
            tiles.Add(tile.transform);
        }
        
        ArrangeTiles();
    }
    
    void ArrangeTiles()
    {
        if (tiles.Count == 0) return;
        
        basePositions.Clear();
        targetPositions.Clear();
        
        if (arrangeInSphere)
        {
            ArrangeInSphere();
        }
        else
        {
            ArrangeInGrid();
        }
        
        // Animate tiles to their positions
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null)
            {
                basePositions.Add(targetPositions[i]);
                StartCoroutine(MoveTileToPosition(tiles[i], targetPositions[i], i * 0.1f));
            }
        }
    }
    
    void ArrangeInSphere()
    {
        // Arrange tiles in a semi-sphere around the player
        int totalTiles = tiles.Count;
        float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f)); // Golden angle for even distribution
        
        for (int i = 0; i < totalTiles; i++)
        {
            float y = 1f - (i / (float)(totalTiles - 1)) * 2f; // -1 to 1
            float r = Mathf.Sqrt(1f - y * y); // Radius at this height
            float theta = goldenAngle * i;
            
            float x = Mathf.Cos(theta) * r;
            float z = Mathf.Sin(theta) * r;
            
            Vector3 pos = new Vector3(x, y, z) * radius;
            pos.y *= heightRange;
            targetPositions.Add(pos);
        }
    }
    
    void ArrangeInGrid()
    {
        // Arrange in a grid pattern
        int rows = Mathf.CeilToInt((float)tiles.Count / tilesPerRow);
        float startX = -(tilesPerRow - 1) * tileSpacing * 0.5f;
        float startY = (rows - 1) * tileSpacing * 0.5f;
        
        for (int i = 0; i < tiles.Count; i++)
        {
            int row = i / tilesPerRow;
            int col = i % tilesPerRow;
            
            float x = startX + col * tileSpacing;
            float y = startY - row * tileSpacing;
            float z = radius;
            
            // Add some variation
            x += Random.Range(-0.2f, 0.2f);
            y += Random.Range(-0.2f, 0.2f);
            
            targetPositions.Add(new Vector3(x, y, z));
        }
    }
    
    System.Collections.IEnumerator MoveTileToPosition(Transform tile, Vector3 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Vector3 startPos = tile.position;
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // Smooth easing
            
            tile.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        tile.position = targetPos;
    }
    
    void Update()
    {
        // Gentle floating animation
        for (int i = 0; i < tiles.Count && i < basePositions.Count; i++)
        {
            if (tiles[i] != null)
            {
                float offset = Mathf.Sin(Time.time * floatSpeed + i) * floatAmount;
                tiles[i].position = basePositions[i] + Vector3.up * offset;
                
                // Gentle rotation
                tiles[i].Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
            }
        }
    }
    
    public void RearrangeTiles()
    {
        ArrangeTiles();
    }
}
