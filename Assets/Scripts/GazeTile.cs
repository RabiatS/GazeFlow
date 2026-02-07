using UnityEngine;
//using TMPro.TextMeshProUGUI;

public class GazeTile : MonoBehaviour
{
    [Header("Visuals")]
    public int tileIndex;
    public Renderer tileRenderer;
    public Material idleMaterial;
    public Material activeMaterial;
    public float activeScale = 1.3f;
    public float floatAmplitude = 0.05f;
    public float floatFrequency = 0.5f;

    public AudioSource audioSource;
    public AudioClip activationClip;


    [HideInInspector] public bool isCompleted;

    float _baseScale = 1f;
    Vector3 _basePos;
    float _firstLookTime = -1f;
    bool _hasBeenLookedAt;

    void Start()
    {
        if (tileRenderer == null) tileRenderer = GetComponent<Renderer>();
        _baseScale = transform.localScale.x;
        _basePos = transform.localPosition;

        SetIdle();
    }

    void Update()
    {
        if (isCompleted) return;

        // subtle float
        float offset = Mathf.Sin(Time.time * floatFrequency + tileIndex) * floatAmplitude;
        transform.localPosition = _basePos + new Vector3(0f, offset, 0f);
    }

    void SetIdle()
    {
        if (tileRenderer != null && idleMaterial != null)
        {
            tileRenderer.material = idleMaterial;
            Debug.Log($"Tile {tileIndex}: Set to IDLE material");
        }
        else
        {
            Debug.LogWarning($"Tile {tileIndex}: Cannot set idle - Renderer: {tileRenderer != null}, Material: {idleMaterial != null}");
        }

        transform.localScale = Vector3.one * _baseScale;
    }

    void SetActive()
    {
        if (tileRenderer != null && activeMaterial != null)
        {
            tileRenderer.material = activeMaterial;
            Debug.Log($"Tile {tileIndex}: Set to ACTIVE material");
        }
        else
        {
            Debug.LogWarning($"Tile {tileIndex}: Cannot set active - Renderer: {tileRenderer != null}, Material: {activeMaterial != null}");
        }

        transform.localScale = Vector3.one * _baseScale * activeScale;
    }

    // Called by MosaicGazeManager
    public void OnGazeEnter(float timeNow)
    {
        if (isCompleted)
        {
            Debug.Log($"Tile {tileIndex}: Already completed, ignoring gaze");
            return;
        }

        if (!_hasBeenLookedAt)
        {
            _hasBeenLookedAt = true;
            _firstLookTime = timeNow;
            Debug.Log($"Tile {tileIndex}: First look at time {timeNow:F2}");
        }

        SetActive();
    }

    public void OnGazeExit()
    {
        if (isCompleted) return;
        SetIdle();
    }

    public void OnGazeDwellComplete(float dwellDuration, System.Action<GazeTile, float, float> onComplete)
    {
        if (isCompleted) return;

        isCompleted = true;
        SetActive(); // lock in bright
        
        // Play visual effects
        TileVisualEffects effects = GetComponent<TileVisualEffects>();
        if (effects == null)
            effects = GetComponentInParent<TileVisualEffects>();
        if (effects != null)
        {
            effects.PlayCompletionEffect();
        }

        float firstLookTime = _firstLookTime > 0f ? _firstLookTime : Time.time;
        if (audioSource != null && activationClip != null)
        {
            audioSource.PlayOneShot(activationClip, 0.7f);
        }
        
        // notify manager so it can log
        if (onComplete != null)
            onComplete(this, firstLookTime, dwellDuration);
    }
}
