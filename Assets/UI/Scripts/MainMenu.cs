using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlayCanvasGroup;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject firstSelectedButton;

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

        RefreshContinueButton();
    }

    private void OnEnable()
    {
        RefreshContinueButton();
    }

    private void Start()
    {
        RefreshContinueButton();

        if (fadeOverlayCanvasGroup != null)
        {
            StartCoroutine(FadeInRoutine());
        }
        else if (menuRoot != null)
        {
            menuRoot.SetActive(true);
            SelectDefaultButton();
        }
    }

    private void RefreshContinueButton()
    {
        bool hasSave = SaveSystem.HasRunSave();

        if (continueButton != null)
        {
            continueButton.SetActive(hasSave);
        }
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);

        if (continueButton != null && continueButton.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
        }
        else if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    public void StartRun()
    {
        LoadSceneWithFade("DeepForest-FirstMap");
    }

    public void ContinueRun()
    {
        RunSaveData save = SaveSystem.LoadRun();

        if (save == null || string.IsNullOrEmpty(save.currentSceneName))
        {
            RefreshContinueButton();
            SelectDefaultButton();
            return;
        }

        LoadSceneWithFade(save.currentSceneName);
    }

    public void GoToMainMenu()
    {
        LoadSceneWithFade("MainMenu");
    }

    public void GoToEncyclopedia()
    {
        LoadSceneWithFade("EncyclopediaMenu");
    }

    public void GoToAugments()
    {
        LoadSceneWithFade("EncyclopediaAugmentMenu");
    }

    public void GoToFoes()
    {
        LoadSceneWithFade("EncyclopediaFoeMenu");
    }

    public void GoToSettings()
    {
        LoadSceneWithFade("SettingsMenu");
    }

    public void GoToAbout()
    {
        LoadSceneWithFade("AboutMenu");
    }

    public void QuitGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        StartCoroutine(QuitRoutine());
    }

    private void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning)
            return;

        isTransitioning = true;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator FadeInRoutine()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
        }

        yield return null;
        SelectDefaultButton();

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