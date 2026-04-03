using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartRun()
    {
        // In single-player, bypass network
        if (GameNetworkManager.Instance != null)
        {
            GameNetworkManager.Instance.StartSinglePlayer();
        }
        else
        {
            SceneManager.LoadScene("DeepForest-FirstMap");
        }
    }

    public void StartMultiplayer()
    {
        // Show the multiplayer panel - handled by MultiplayerLobbyUI
        var lobbyUI = FindFirstObjectByType<MultiplayerLobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.ShowMultiplayerPanel();
        }
        else
        {
            Debug.LogWarning("MultiplayerLobbyUI not found in scene.");
        }
    }

    public void GoToSettingsMenu()
    {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void GoToAboutPage()
    {
        SceneManager.LoadScene("AboutPage");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
