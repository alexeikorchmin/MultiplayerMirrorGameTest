using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel;
    [SerializeField] private GameObject enterAddressPagePanel;
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] Button hostLobbyButton;
    [SerializeField] Button joinLobbyButton;

    private void Awake()
    {
        hostLobbyButton.onClick.AddListener(HostLobby);
        joinLobbyButton.onClick.AddListener(JoinLobby);
    }

    private void JoinLobby()
    {
        enterAddressPagePanel.SetActive(true);
    }

    private void HostLobby()
    {
        landingPagePanel.SetActive(false);
        menuCanvas.enabled = false;

        NetworkManager.singleton.StartHost();
    }
}