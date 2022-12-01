using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject LobbyUI;
    [SerializeField] Button leaveLobbyButton;

    private void Awake()
    {
        leaveLobbyButton.onClick.AddListener(LeaveLobby);
    }

    private void OnEnable()
    {
        CustomNetworkManager.OnClientConnected += OnClientConnectedHandler;
    }

    private void OnDisable()
    {
        CustomNetworkManager.OnClientConnected -= OnClientConnectedHandler;
    }

    private void OnClientConnectedHandler()
    {
        LobbyUI.SetActive(true);
    }

    private void OnClientDisconnectedHandler()
    {

    }

    private void LeaveLobby()
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