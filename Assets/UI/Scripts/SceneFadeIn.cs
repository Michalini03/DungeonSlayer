using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.75f;

    private void Start()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        if (fadeOverlayCanvasGroup == null)
        {
            yield break;
        }

        fadeOverlayCanvasGroup.alpha = 1f;
        fadeOverlayCanvasGroup.blocksRaycasts = true;

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeOverlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        fadeOverlayCanvasGroup.alpha = 0f;
        fadeOverlayCanvasGroup.blocksRaycasts = false;
    }
}