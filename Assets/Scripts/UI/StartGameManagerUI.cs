using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class StartGameManagerUI : NetworkBehaviour
{
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private List<TMP_Text> lobbyPlayersTextList;
    [SerializeField] private GameObject lobbyPlayersPanel;

    private string waitingForPlayer = "Waiting For Player...";

    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();
    private List<string> lobbyPlayersNames = new List<string>();
    private List<Color> lobbyPlayersColors = new List<Color>();

    [Server]
    private void OnGameStartedHandler()
    {
        RpcStartGame();
    }

    [Server]
    private void OnPlayersListUpdatedHandler(List<CustomNetworkPlayer> newPlayersList)
    {
        players = newPlayersList;

        lobbyPlayersNames.Clear();
        lobbyPlayersColors.Clear();

        for (int i = 0; i < players.Count; i++)
        {
            lobbyPlayersNames.Insert(i,players[i].GetPlayerName());
            lobbyPlayersColors.Insert(i,players[i].GetPlayerColor());
        }

        RpcUpdateLobbyPlayersNamesText(lobbyPlayersNames, lobbyPlayersColors);
    }

    private void Awake()
    {
        LobbyMenu.OnGameStarted += OnGameStartedHandler;
        CustomNetworkManager.OnPlayersListUpdated += OnPlayersListUpdatedHandler;
    }

    private void OnDestroy()
    {
        LobbyMenu.OnGameStarted -= OnGameStartedHandler;
        CustomNetworkManager.OnPlayersListUpdated -= OnPlayersListUpdatedHandler;
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        menuCanvas.enabled = false;
    }

    [ClientRpc]
    private void RpcUpdateLobbyPlayersNamesText(List<string> newlobbyPlayersNames, List<Color> newlobbyPlayersColors)
    {
        lobbyPlayersPanel.SetActive(true);

        for (int i = 0; i < lobbyPlayersTextList.Count; i++)
        {
            if (i < newlobbyPlayersNames.Count && i < newlobbyPlayersColors.Count)
            {
                lobbyPlayersTextList[i].text = newlobbyPlayersNames[i];
                lobbyPlayersTextList[i].color = newlobbyPlayersColors[i];
            }
            else
            {
                lobbyPlayersTextList[i].text = waitingForPlayer;
                lobbyPlayersTextList[i].color = Color.white;
            }
        }
    }
}