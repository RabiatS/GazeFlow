using UnityEngine;
using TMPro;

// Makes the summary UI look better, more readable, and fun
public class SummaryUIController : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI tilesCompletedText;
    public TextMeshProUGUI avgDwellText;
    public TextMeshProUGUI percentageText;
    
    [Header("Animation")]
    public float fadeInDuration = 1f;
    public float scaleAnimationDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Start hidden
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    public void ShowSummary(int completed, int total, float avgDwell)
    {
        gameObject.SetActive(true);
        
        float completionPercent = total > 0 ? (float)completed / total * 100f : 0f;
        
        // Update texts with better formatting
        if (titleText != null)
        {
            titleText.text = "<size=80><b>Session Complete!</b></size>";
            titleText.alignment = TextAlignmentOptions.Center;
        }
        
        if (tilesCompletedText != null)
        {
            string emoji = GetCompletionEmoji(completionPercent);
            tilesCompletedText.text = $"{emoji}\n<size=64>Tiles Completed</size>\n<size=96><b>{completed}</b></size>\n<size=48>out of {total}</size>";
            tilesCompletedText.alignment = TextAlignmentOptions.Center;
        }
        
        if (percentageText != null)
        {
            percentageText.text = $"<size=72><b>{completionPercent:F0}%</b></size>\n<size=48>Complete</size>";
            percentageText.alignment = TextAlignmentOptions.Center;
        }
        
        if (avgDwellText != null)
        {
            string speedEmoji = GetSpeedEmoji(avgDwell);
            avgDwellText.text = $"{speedEmoji}\n<size=56>Average Focus</size>\n<size=80><b>{avgDwell:F2}s</b></size>\n<size=40>per tile</size>";
            avgDwellText.alignment = TextAlignmentOptions.Center;
        }
        
        // Animate in
        StartCoroutine(AnimateIn());
    }
    
    System.Collections.IEnumerator AnimateIn()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale * 0.5f;
        Vector3 targetScale = Vector3.one;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            // Fade in
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            // Scale animation
            if (elapsed < scaleAnimationDuration)
            {
                float scaleT = elapsed / scaleAnimationDuration;
                scaleT = Mathf.SmoothStep(0f, 1f, scaleT);
                transform.localScale = Vector3.Lerp(startScale, targetScale, scaleT);
            }
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        transform.localScale = targetScale;
    }
    
    string GetCompletionEmoji(float percent)
    {
        if (percent >= 100f) return "🎉";
        if (percent >= 80f) return "🌟";
        if (percent >= 60f) return "✨";
        if (percent >= 40f) return "⭐";
        if (percent >= 20f) return "💫";
        return "🌙";
    }
    
    string GetSpeedEmoji(float avgDwell)
    {
        if (avgDwell < 0.8f) return "⚡";
        if (avgDwell < 1.2f) return "✨";
        if (avgDwell < 1.8f) return "🌟";
        return "💫";
    }
}
