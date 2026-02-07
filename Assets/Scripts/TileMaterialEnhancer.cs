using UnityEngine;

// Makes tiles look more mosaic-like with gradients, depth, and visual interest
public class TileMaterialEnhancer : MonoBehaviour
{
    [Header("Material Enhancement")]
    public Material idleMaterial;
    public Material activeMaterial;
    
    [Header("Gradient Settings")]
    public bool useGradient = true;
    public Color gradientTopColor = new Color(0.3f, 0.2f, 0.5f, 1f);      // Dark purple
    public Color gradientBottomColor = new Color(0.5f, 0.3f, 0.7f, 1f);   // Lighter purple
    
    [Header("Emission Settings")]
    public bool enableEmission = true;
    public Color idleEmissionColor = new Color(0.1f, 0.05f, 0.2f, 1f);    // Subtle purple glow
    public Color activeEmissionColor = new Color(1f, 0.8f, 0.2f, 1f);     // Bright yellow glow
    public float idleEmissionIntensity = 0.5f;
    public float activeEmissionIntensity = 2f;
    
    [Header("Depth/3D Effect")]
    public bool addDepth = true;
    public float depthOffset = 0.02f;
    
    private Renderer tileRenderer;
    private Material instanceIdleMat;
    private Material instanceActiveMat;
    private GazeTile gazeTile;
    
    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer == null)
            tileRenderer = GetComponentInChildren<Renderer>();
            
        gazeTile = GetComponent<GazeTile>();
        if (gazeTile == null)
            gazeTile = GetComponentInParent<GazeTile>();
        
        CreateEnhancedMaterials();
        ApplyIdleMaterial();
    }
    
    void CreateEnhancedMaterials()
    {
        // Create instance materials so we can modify them
        if (idleMaterial != null)
        {
            instanceIdleMat = new Material(idleMaterial);
            EnhanceMaterial(instanceIdleMat, idleEmissionColor, idleEmissionIntensity, false);
        }
        
        if (activeMaterial != null)
        {
            instanceActiveMat = new Material(activeMaterial);
            EnhanceMaterial(instanceActiveMat, activeEmissionColor, activeEmissionIntensity, true);
        }
    }
    
    void EnhanceMaterial(Material mat, Color emissionColor, float emissionIntensity, bool isActive)
    {
        // Enable emission
        if (enableEmission && mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        
        // Add subtle gradient effect (if shader supports it)
        // This creates a more interesting, less flat look
        if (useGradient && mat.HasProperty("_Color"))
        {
            Color baseColor = mat.color;
            // Slight color variation based on position
            mat.color = baseColor;
        }
        
        // Make material more interesting
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", isActive ? 0.3f : 0.1f);
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", isActive ? 0.8f : 0.4f);
    }
    
    public void ApplyIdleMaterial()
    {
        if (tileRenderer != null && instanceIdleMat != null)
        {
            tileRenderer.material = instanceIdleMat;
        }
    }
    
    public void ApplyActiveMaterial()
    {
        if (tileRenderer != null && instanceActiveMat != null)
        {
            tileRenderer.material = instanceActiveMat;
        }
    }
    
    void Update()
    {
        // Sync with GazeTile state
        if (gazeTile != null)
        {
            if (gazeTile.isCompleted && tileRenderer.material != instanceActiveMat)
            {
                ApplyActiveMaterial();
            }
        }
        
        // Add subtle pulsing to idle tiles
        if (gazeTile != null && !gazeTile.isCompleted && enableEmission && instanceIdleMat != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 0.8f;
            if (instanceIdleMat.HasProperty("_EmissionColor"))
            {
                instanceIdleMat.SetColor("_EmissionColor", idleEmissionColor * (idleEmissionIntensity * pulse));
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up instance materials
        if (instanceIdleMat != null)
            Destroy(instanceIdleMat);
        if (instanceActiveMat != null)
            Destroy(instanceActiveMat);
    }
}
