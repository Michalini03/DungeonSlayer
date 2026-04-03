using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages network session lifecycle: hosting, joining, and disconnection.
/// Attach to a GameObject with NetworkManager and UnityTransport components.
/// </summary>
public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager Instance { get; private set; }

    [Header("Network Settings")]
    [SerializeField] private ushort port = 7777;
    [SerializeField] private string gameSceneName = "DeepForest-FirstMap";

    public bool IsMultiplayer { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void HostGame()
    {
        IsMultiplayer = true;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("0.0.0.0", port);

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        NetworkManager.Singleton.StartHost();
        Debug.Log($"Hosting on port {port}");

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void JoinGame(string ipAddress)
    {
        IsMultiplayer = true;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        NetworkManager.Singleton.StartClient();
        Debug.Log($"Connecting to {ipAddress}:{port}");
    }

    public void SetPort(ushort newPort)
    {
        port = newPort;
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        IsMultiplayer = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void StartSinglePlayer()
    {
        IsMultiplayer = false;
        SceneManager.LoadScene(gameSceneName);
    }

    private void ApprovalCallback(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        // Allow up to 2 players
        int currentPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        response.Approved = currentPlayers < 2;
        response.CreatePlayerObject = true;

        if (!response.Approved)
        {
            Debug.Log("Connection denied: game is full (2 players max).");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
