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

        ShowMainPausePanel();

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
                ShowMainPausePanel();
                SelectButton(augmentsSelectedButton != null ? augmentsSelectedButton : defaultSelectedButton);

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

        SelectButton(defaultSelectedButton);
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

        SelectButton(augmentsBackButton);
    }

    public void ShowMainPausePanel()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(true);
        }

        if (augmentsPanel != null)
        {
            augmentsPanel.SetActive(false);
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
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SelectButton(Button button)
    {
        if (EventSystem.current == null || button == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
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