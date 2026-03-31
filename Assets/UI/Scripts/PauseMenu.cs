using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        if (RunController.Instance != null)
            RunController.Instance.RefreshPauseState();
        else
            Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (RunController.Instance != null && RunController.Instance.IsAugmentMenuOpen)
                return;

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;

        if (RunController.Instance != null)
            RunController.Instance.RefreshPauseState();
        else
            Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        if (RunController.Instance != null)
            RunController.Instance.RefreshPauseState();
        else
            Time.timeScale = 1f;
    }

    public void AugmentsMenu()
    {
        Debug.Log("Not implemented");
    }

    public void GoToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}