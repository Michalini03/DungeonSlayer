using UnityEngine;
using System.Collections;

public class UIFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 1.0f;

    public void FadeIn()
    {
        if (canvasGroup == null) return;
        Debug.Log("Starting fade in...");
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 0, duration));
    }

    public void FadeOut()
    {
        if (canvasGroup == null) return;
        Debug.Log("Starting fade out...");
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(canvasGroup.alpha, 1, duration));
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float time)
    {
        float _elapsedTime = 0f;
        while (_elapsedTime < time)
        {
            _elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, _elapsedTime / time);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}