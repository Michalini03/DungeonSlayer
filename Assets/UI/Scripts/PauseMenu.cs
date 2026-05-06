using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;

    [Header("Panels")]
    [SerializeField] private GameObject mainPausePanel;
    [SerializeField] private GameObject augmentsPanel;

    [Header("Keyboard Navigation")]
    [SerializeField] private Button defaultSelectedButton;
    [SerializeField] private Button augmentsSelectedButton;
    [SerializeField] private Button augmentsBackButton;

    [Header("UI References")]
    [SerializeField] private GameObject playerUI;

    void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        ShowMainPausePanel(false);

        if (RunController.Instance != null)
        {
            RunController.Instance.RefreshPauseState();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isPaused)
            {
                PauseGame();
                return;
            }

            if (augmentsPanel != null && augmentsPanel.activeSelf)
            {
                ShowMainPausePanel(true);
                return;
            }

            ResumeGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;

        if (playerUI != null)
        {
            playerUI.SetActive(false);
        }

        if (RunController.Instance != null)
        {
            RunController.Instance.RefreshPauseState();
        }
        else
        {
            Time.timeScale = 0f;
        }

        ShowMainPausePanel(true);
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        if (RunController.Instance != null)
        {
            RunController.Instance.RefreshPauseState();
        }
        else
        {
            Time.timeScale = 1f;
        }

        ClearSelectedButton();
    }

    public void OpenAugmentsMenu()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(false);
        }

        if (augmentsPanel != null)
        {
            augmentsPanel.SetActive(true);
        }

        PauseAugmentPage page = augmentsPanel != null ? augmentsPanel.GetComponent<PauseAugmentPage>() : null;
        if (page != null)
        {
            page.Rebuild();
        }

        SelectButtonNextFrame(augmentsBackButton);
    }

    public void ShowMainPausePanel()
    {
        ShowMainPausePanel(true);
    }

    private void ShowMainPausePanel(bool selectButton)
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
        }

        if (augmentsPanel != null)
        {
            augmentsPanel.SetActive(false);
        }

        if (selectButton)
        {
            SelectButtonNextFrame(defaultSelectedButton);
        }
    }

    public void GoToMainMenu()
    {
        if (playerUI != null)
        {
            playerUI.SetActive(true);
        }

        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("main_menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SelectButton(Button button)
    {
        if (EventSystem.current == null || button == null || !button.gameObject.activeInHierarchy)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void SelectButtonNextFrame(Button button)
    {
        StartCoroutine(SelectButtonNextFrameRoutine(button));
    }

    private IEnumerator SelectButtonNextFrameRoutine(Button button)
    {
        yield return null;
        SelectButton(button);
    }

    private void ClearSelectedButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
    }
}