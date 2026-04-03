using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple lobby UI for hosting or joining a game.
/// Add this to a Canvas in the MainMenu scene.
/// </summary>
public class MultiplayerLobbyUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject multiplayerPanel;

    [Header("Multiplayer UI Elements")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostClicked);
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinClicked);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);

        if (ipInputField != null)
            ipInputField.text = "127.0.0.1";
        if (portInputField != null)
            portInputField.text = "7777";
    }

    public void ShowMultiplayerPanel()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(true);
        if (statusText != null)
            statusText.text = "";
    }

    private void OnHostClicked()
    {
        if (GameNetworkManager.Instance == null)
        {
            SetStatus("Error: GameNetworkManager not found in scene!");
            return;
        }

        ApplyPort();
        SetStatus("Starting host...");
        GameNetworkManager.Instance.HostGame();
    }

    private void OnJoinClicked()
    {
        if (GameNetworkManager.Instance == null)
        {
            SetStatus("Error: GameNetworkManager not found in scene!");
            return;
        }

        string ip = ipInputField != null ? ipInputField.text.Trim() : "127.0.0.1";
        if (string.IsNullOrEmpty(ip))
        {
            SetStatus("Please enter an IP address.");
            return;
        }

        ApplyPort();
        SetStatus($"Connecting to {ip}...");
        GameNetworkManager.Instance.JoinGame(ip);
    }

    private void OnBackClicked()
    {
        if (multiplayerPanel != null)
            multiplayerPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    private void ApplyPort()
    {
        if (portInputField != null && ushort.TryParse(portInputField.text.Trim(), out ushort port))
        {
            GameNetworkManager.Instance.SetPort(port);
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
        Debug.Log($"[Lobby] {message}");
    }
}
