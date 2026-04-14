using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Controls;

public class StartupStory : MonoBehaviour
{
    [SerializeField] private CanvasGroup storyCanvasGroup;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private GameObject rootPanel;

    [TextArea(3, 10)]
    [SerializeField] private string[] pages;

    [SerializeField] private float characterDelay = 0.03f;
    [SerializeField] private float delayBeforeTyping = 0.2f;
    [SerializeField] private float delayAfterTyping = 0.4f;

    [SerializeField] private string nextSceneName = "MainMenu";

    [SerializeField] private CanvasGroup fadeOverlayCanvasGroup;
    [SerializeField] private float sceneFadeDuration = 0.75f;
    
    private int currentPage = 0;
    private bool pageFullyShown = false;
    private bool pageInProgress = false;
    private bool revealRequested = false;
    private bool isEnding = false;
    private Coroutine pageRoutine;

    private void Start()
    {
        if (continueText != null)
        {
            SetTextAlpha(continueText, 1f);
        }

        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 1f;
        }

        if (fadeOverlayCanvasGroup != null)
        {
            fadeOverlayCanvasGroup.alpha = 0f;
        }

        if (pages == null || pages.Length == 0)
        {
            EndStory();
            return;
        }

        pageRoutine = StartCoroutine(PlayPage(currentPage));
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        bool continuePressed = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame;

        bool anyKeyPressed = false;

        foreach (KeyControl key in keyboard.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                anyKeyPressed = true;
                break;
            }
        }

        if (!anyKeyPressed)
        {
            return;
        }

        if (continuePressed)
        {
            if (pageInProgress && !pageFullyShown)
            {
                revealRequested = true;
                return;
            }

            if (pageFullyShown)
            {
                GoToNextPage();
            }

            return;
        }

        EndStory();
    }

    private void GoToNextPage()
    {
        currentPage++;

        if (currentPage >= pages.Length)
        {
            EndStory();
            return;
        }

        if (pageRoutine != null)
        {
            StopCoroutine(pageRoutine);
        }

        pageRoutine = StartCoroutine(PlayPage(currentPage));
    }
    private IEnumerator PlayPage(int pageIndex)
    {
        pageInProgress = true;
        pageFullyShown = false;
        revealRequested = false;

        if (continueText != null)
        {
            SetTextAlpha(continueText, 1f);
        }

        storyText.text = "";

        if (delayBeforeTyping > 0f)
        {
            yield return new WaitForSeconds(delayBeforeTyping);
        }

        string fullText = pages[pageIndex];
        storyText.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            if (revealRequested)
            {
                storyText.text = fullText;
                break;
            }

            storyText.text += fullText[i];
            yield return new WaitForSeconds(characterDelay);
        }

        revealRequested = false;

        if (delayAfterTyping > 0f)
        {
            yield return new WaitForSeconds(delayAfterTyping);
        }
            
        pageFullyShown = true;
        pageInProgress = false;
    }

    private void SetTextAlpha(TextMeshProUGUI textObject, float alpha)
    {
        if (textObject == null)
        {
            return;
        }


        Color color = textObject.color;
        color.a = alpha;
        textObject.color = color;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float time = 0f;
        canvasGroup.alpha = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator EndStoryRoutine()
    {
        if (fadeOverlayCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(fadeOverlayCanvasGroup, 0f, 1f, sceneFadeDuration));
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void EndStory()
    {
        if (isEnding)
        {
            return;
        }

        isEnding = true;
        StopAllCoroutines();
        pageInProgress = false;
        pageFullyShown = false;

        StartCoroutine(EndStoryRoutine());
    }
}