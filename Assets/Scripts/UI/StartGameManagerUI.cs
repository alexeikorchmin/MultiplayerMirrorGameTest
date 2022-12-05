using UnityEngine;
using Mirror;

public class StartGameManagerUI : NetworkBehaviour
{
    [SerializeField] private Canvas menuCanvas;

    private void Awake()
    {
        LobbyMenu.OnGameStarted += RpcOnStartGameHandler;
    }

    private void OnDestroy()
    {
        LobbyMenu.OnGameStarted -= RpcOnStartGameHandler;
    }

    [ClientRpc]
    private void RpcOnStartGameHandler()
    {
        menuCanvas.enabled = false;
    }
}