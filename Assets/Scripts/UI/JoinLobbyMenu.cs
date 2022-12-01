using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel;
    [SerializeField] private GameObject addressPagePanel;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Canvas menuCanvas;

    private void OnEnable()
    {
        CustomNetworkManager.OnClientConnected += OnClientConnectedHandler;
        CustomNetworkManager.OnClientDisconnected += OnClientDisconnectedHandler;
    }

    private void OnDisable()
    {
        CustomNetworkManager.OnClientConnected -= OnClientConnectedHandler;
        CustomNetworkManager.OnClientDisconnected -= OnClientDisconnectedHandler;
    }

    private void Awake()
    {
        joinGameButton.onClick.AddListener(JoinGame);
        closeButton.onClick.AddListener(CloseAddressPagePanel);
    }
    private void OnClientConnectedHandler()
    {
        joinGameButton.interactable = true;

        addressPagePanel.SetActive(false);
        landingPagePanel.SetActive(false);
        //menuCanvas.enabled = false;
    }

    private void OnClientDisconnectedHandler()
    {
        joinGameButton.interactable = true;
    }

    private void JoinGame()
    {
        //string address = addressInput.text;

        NetworkManager.singleton.networkAddress = addressInput.text; ;
        NetworkManager.singleton.StartClient();

        joinGameButton.interactable = false;
    }

    private void CloseAddressPagePanel()
    {
        addressPagePanel.SetActive(false);
        joinGameButton.interactable = true;
    }
}