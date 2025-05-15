using UnityEngine;
using UnityEngine.EventSystems;

public class PulsingHighlight : MonoBehaviour, IPointerClickHandler
{
    public CanvasGroup canvasGroup;             // For UI pulsing
    public float minOpacity = 0.4f;
    public float maxOpacity = 1f;
    public float pulseSpeed = 2f;

    private bool isPulsing = true;

    void Update()
    {
        if (!isPulsing) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
        float value = Mathf.Lerp(minOpacity, maxOpacity, t);

        if (canvasGroup != null)
            canvasGroup.alpha = value;
    }

    public void StopPulse()
    {
        isPulsing = false;
        if (canvasGroup != null)
            canvasGroup.alpha = maxOpacity;
    }

    // Called when UI element is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        StopPulse();
        FindObjectOfType<ChatController>()?.StartChat();
    }
}
