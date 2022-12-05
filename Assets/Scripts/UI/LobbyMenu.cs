using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class LobbyMenu : MonoBehaviour
{
    public static event Action OnGameStarted;

    [SerializeField] private GameObject landingPagePanel;
    [SerializeField] private GameObject enterAddressPagePanel;
    [SerializeField] private GameObject lobbyUI;

    [SerializeField] private Button hostLobbyButton;
    [SerializeField] private Button joinLobbyButton;

    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveRoomButton;

    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private CustomNetworkManager customNetworkManager;

    private void Awake()
    {
        CustomNetworkManager.OnClientConnected += OnClientConnectedHandler;
        CustomNetworkManager.OnClientDisconnected += OnClientDisconnectedHandler;

        hostLobbyButton.onClick.AddListener(HostLobby);
        joinLobbyButton.onClick.AddListener(JoinLobby);

        joinRoomButton.onClick.AddListener(JoinRoom);
        closeButton.onClick.AddListener(CloseAddressPagePanel);

        startGameButton.onClick.AddListener(StartGame);
        leaveRoomButton.onClick.AddListener(LeaveRoom);
    }

    private void OnDestroy()
    {
        CustomNetworkManager.OnClientConnected -= OnClientConnectedHandler;
        CustomNetworkManager.OnClientDisconnected -= OnClientDisconnectedHandler;
    }

    private void OnClientConnectedHandler()
    {
        joinRoomButton.interactable = true;

        enterAddressPagePanel.SetActive(false);
        landingPagePanel.SetActive(false);
        lobbyUI.SetActive(true);
    }

    private void OnClientDisconnectedHandler()
    {
        joinRoomButton.interactable = true;
    }

    private void HostLobby()
    {
        landingPagePanel.SetActive(false);
        lobbyUI.SetActive(true);
        startGameButton.gameObject.SetActive(true);
        NetworkManager.singleton.StartHost();
    }

    private void JoinLobby()
    {
        landingPagePanel.SetActive(false);
        enterAddressPagePanel.SetActive(true);
    }

    private void JoinRoom()
    {
        NetworkManager.singleton.networkAddress = addressInput.text;
        NetworkManager.singleton.StartClient();

        joinRoomButton.interactable = false;
    }

    private void CloseAddressPagePanel()
    {
        enterAddressPagePanel.SetActive(false);
        landingPagePanel.SetActive(true);
        joinRoomButton.interactable = true;
    }

    private void StartGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected && customNetworkManager.GetPlayersList().Count > 1)
        {
            OnGameStarted?.Invoke();
        }
    }

    private void LeaveRoom()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
            SceneManager.LoadScene(0);
        }
    }
}