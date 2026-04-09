using UnityEngine;
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(CanvasGroup))]
public class DeathScreenFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f; // How long the fade takes in seconds
    public float targetAlpha = 1f;    // 1 is fully opaque, 0.8 is slightly see-through

    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Grab the CanvasGroup component attached to this object
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // OnEnable runs automatically the exact moment the object is SetActive(true)
    void OnEnable()
    {
        canvasGroup.alpha = 0f; // Ensure it always starts completely invisible
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // We use unscaledDeltaTime so the screen still fades even if you paused the game (Time.timeScale = 0) upon dying
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate the current alpha based on how much time has passed
            canvasGroup.alpha = Mathf.Lerp(0f, targetAlpha, elapsedTime / fadeDuration);

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // Snap to the final target alpha just to be perfectly precise at the end
        canvasGroup.alpha = targetAlpha;
    }
}