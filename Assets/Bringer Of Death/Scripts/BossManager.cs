using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    void Start()
    {
        // Automatically fade in whenever this scene is loaded
        if (fadeGroup != null)
        {
            StartCoroutine(FadeInRoutine());
        }
        else
        {
            Debug.LogWarning("BossManager is missing a CanvasGroup reference!");
        }
    }

    private IEnumerator FadeInRoutine()
    {
        // Ensure the screen starts totally black and blocks clicks
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        // Ensure it is completely invisible and allows clicks at the end
        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }

    // Call this public method when the player dies or beats the game!
    public void FadeToLevel(string nextSceneName)
    {
        StartCoroutine(FadeOutRoutine(nextSceneName));
    }

    private IEnumerator FadeOutRoutine(string nextSceneName)
    {
        // Block clicks immediately so the player can't double-click a "Start Game" button
        fadeGroup.blocksRaycasts = true;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;

        // Load the new scene only AFTER the screen is completely black
        SceneManager.LoadScene(nextSceneName);
    }
}