using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;

    [Header("Keyboard Navigation")]
    [SerializeField] private Button defaultSelectedButton;

    void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

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
            if (RunController.Instance != null && RunController.Instance.IsAugmentMenuOpen)
            {
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;

        if (RunController.Instance != null)
        {
            RunController.Instance.RefreshPauseState();
        }
        else
        {
            Time.timeScale = 0f;
        }


        SelectDefaultButton();
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

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

    public void AugmentsMenu()
    {
        Debug.Log("Not implemented");
    }

    public void GoToMainMenu()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null || defaultSelectedButton == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultSelectedButton.gameObject);
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