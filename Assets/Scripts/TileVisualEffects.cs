using UnityEngine;

// Adds beautiful visual effects to tiles when activated
public class TileVisualEffects : MonoBehaviour
{
    [Header("Particle Effects")]
    public ParticleSystem activationParticles;
    public ParticleSystem completionParticles;
    
    [Header("Glow Effect")]
    public Light glowLight;
    public float glowIntensity = 2f;
    public Color glowColor = Color.yellow;
    
    [Header("Scale Animation")]
    public float scaleBounceAmount = 0.2f;
    public float scaleBounceSpeed = 5f;
    
    private GazeTile tile;
    private Vector3 baseScale;
    private bool isActivated = false;
    
    void Start()
    {
        tile = GetComponent<GazeTile>();
        if (tile == null)
            tile = GetComponentInParent<GazeTile>();
            
        baseScale = transform.localScale;
        
        // Create particle systems if not assigned
        if (activationParticles == null)
        {
            CreateActivationParticles();
        }
        
        if (completionParticles == null)
        {
            CreateCompletionParticles();
        }
        
        // Create glow light if not assigned
        if (glowLight == null)
        {
            CreateGlowLight();
        }
    }
    
    void CreateActivationParticles()
    {
        GameObject particlesObj = new GameObject("ActivationParticles");
        particlesObj.transform.SetParent(transform);
        particlesObj.transform.localPosition = Vector3.zero;
        
        activationParticles = particlesObj.AddComponent<ParticleSystem>();
        var main = activationParticles.main;
        main.startColor = Color.yellow;
        main.startSize = 0.1f;
        main.startLifetime = 1f;
        main.maxParticles = 20;
        
        var emission = activationParticles.emission;
        emission.rateOverTime = 0;
        emission.enabled = false;
        
        var shape = activationParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
    }
    
    void CreateCompletionParticles()
    {
        GameObject particlesObj = new GameObject("CompletionParticles");
        particlesObj.transform.SetParent(transform);
        particlesObj.transform.localPosition = Vector3.zero;
        
        completionParticles = particlesObj.AddComponent<ParticleSystem>();
        var main = completionParticles.main;
        main.startColor = new Color(1f, 0.5f, 0f); // Orange
        main.startSize = 0.15f;
        main.startLifetime = 2f;
        main.maxParticles = 50;
        
        var emission = completionParticles.emission;
        emission.rateOverTime = 0;
        emission.enabled = false;
        
        var shape = completionParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
    }
    
    void CreateGlowLight()
    {
        GameObject lightObj = new GameObject("GlowLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.range = 2f;
        glowLight.intensity = 0f;
        glowLight.color = glowColor;
        glowLight.enabled = false;
    }
    
    void Update()
    {
        if (tile != null)
        {
            // Update glow based on tile state
            if (glowLight != null)
            {
                if (tile.isCompleted)
                {
                    glowLight.enabled = true;
                    glowLight.intensity = glowIntensity;
                }
                else
                {
                    // Pulse when being looked at (you'd need to track this)
                    glowLight.intensity = Mathf.Lerp(glowLight.intensity, 0f, Time.deltaTime * 2f);
                    if (glowLight.intensity < 0.1f)
                        glowLight.enabled = false;
                }
            }
            
            // Bounce animation when completed
            if (tile.isCompleted && !isActivated)
            {
                isActivated = true;
                PlayCompletionEffect();
            }
        }
    }
    
    public void PlayActivationEffect()
    {
        if (activationParticles != null)
        {
            activationParticles.Play();
        }
        
        if (glowLight != null)
        {
            glowLight.enabled = true;
            glowLight.intensity = glowIntensity * 0.5f;
        }
    }
    
    public void PlayCompletionEffect()
    {
        if (completionParticles != null)
        {
            completionParticles.Play();
        }
        
        // Bounce animation
        StartCoroutine(BounceAnimation());
    }
    
    System.Collections.IEnumerator BounceAnimation()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * scaleBounceAmount;
            transform.localScale = baseScale * scale;
            yield return null;
        }
        
        transform.localScale = baseScale;
    }
}
