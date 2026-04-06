using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private GameObject menuRoot;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (fadeOverlayCanvasGroup != null)
        {
            fadeOverlayCanvasGroup.alpha = 1f;
            fadeOverlayCanvasGroup.blocksRaycasts = true;
        }
    }

    private void Start()
    {
        if (fadeOverlayCanvasGroup != null)
        {
            StartCoroutine(FadeInRoutine());
        }
        else if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }
    }

    public void StartRun()
    {
        LoadSceneWithFade("DeepForest-FirstMap");
    }

    public void GoToEncyclopedia()
    {
        LoadSceneWithFade("EncyclopediaPage");
    }

    public void GoToFoes()
    {
        LoadSceneWithFade("EncyclopediaFoePage");
    }
    public void GoToAugments()
    {
        LoadSceneWithFade("EncyclopediaAugmentPage");
    }

    public void GoToSettingsMenu()
    {
        LoadSceneWithFade("SettingsMenu");
    }

    public void GoToAboutPage()
    {
        LoadSceneWithFade("AboutPage");
    }

    public void GoToMainMenu()
    {
        LoadSceneWithFade("MainMenu");
    }

    public void QuitGame()
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        StartCoroutine(QuitRoutine());
    }


    private void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        isTransitioning = true;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator FadeInRoutine()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }

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

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        fadeOverlayCanvasGroup.blocksRaycasts = true;

        float time = 0f;
        float startAlpha = fadeOverlayCanvasGroup.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeOverlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / fadeDuration);
            yield return null;
        }

        fadeOverlayCanvasGroup.alpha = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator QuitRoutine()
    {
        fadeOverlayCanvasGroup.blocksRaycasts = true;

        float time = 0f;
        float startAlpha = fadeOverlayCanvasGroup.alpha;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeOverlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / fadeDuration);
            yield return null;
        }

        fadeOverlayCanvasGroup.alpha = 1f;
        Application.Quit();
    }
}