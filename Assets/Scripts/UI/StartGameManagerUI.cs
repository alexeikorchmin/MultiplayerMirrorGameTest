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

    //[SyncVar (hook = nameof(PlayersLobbyNamesUpdate))]
    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();

    private void OnPlayersListUpdatedHandler(List<CustomNetworkPlayer> newPlayersList)
    {
        RpcLobbyPlayersUpdateHandler(newPlayersList);
    }

    [Server]
    private void OnGameStartedHandler()
    {
        RpcStartGame();
    }

    private void PlayersLobbyNamesUpdate(List<CustomNetworkPlayer> oldPlayers, List<CustomNetworkPlayer> newPlayers)
    {
        for (int i = 0; i < lobbyPlayersTextList.Count; i++)
        {
            if (i < players.Count)
            {
                lobbyPlayersTextList[i].text = players[i].GetPlayerName();
                print($"i= {i}, playerName = {players[i].GetPlayerName()}");
                lobbyPlayersTextList[i].color = players[i].GetPlayerColor();
            }
            else
            {
                lobbyPlayersTextList[i].text = waitingForPlayer;
                lobbyPlayersTextList[i].color = Color.white;
            }
        }
    }

    [ClientRpc]
    private void RpcLobbyPlayersUpdateHandler(List<CustomNetworkPlayer> playersList)
    {
        players = playersList;

        UpdateLobbyPlayers();

        //lobbyPlayersPanel.SetActive(true);

        //for (int i = 0; i < lobbyPlayersTextList.Count; i++)
        //{
        //    if (i < players.Count)
        //    {
        //        lobbyPlayersTextList[i].text = players[i].GetPlayerName();
        //        print($"i= {i}, playerName = {players[i].GetPlayerName()}");
        //        lobbyPlayersTextList[i].color = players[i].GetPlayerColor();
        //    }
        //    else
        //    {
        //        lobbyPlayersTextList[i].text = waitingForPlayer;
        //        lobbyPlayersTextList[i].color = Color.white;
        //    }
        //}
    }

    private void UpdateLobbyPlayers()
    {
        lobbyPlayersPanel.SetActive(true);

        for (int i = 0; i < lobbyPlayersTextList.Count; i++)
        {
            if (i < players.Count)
            {
                lobbyPlayersTextList[i].text = players[i].GetPlayerName();
                print($"i= {i}, playerName = {players[i].GetPlayerName()}");
                lobbyPlayersTextList[i].color = players[i].GetPlayerColor();
            }
            else
            {
                lobbyPlayersTextList[i].text = waitingForPlayer;
                lobbyPlayersTextList[i].color = Color.white;
            }
        }
    }

    private void Awake()
    {
        LobbyMenu.OnGameStarted += OnGameStartedHandler;
        //CustomNetworkManager.OnPlayersListUpdated += OnPlayersListUpdatedHandler;
    }

    private void OnDestroy()
    {
        LobbyMenu.OnGameStarted -= OnGameStartedHandler;
        //CustomNetworkManager.OnPlayersListUpdated -= OnPlayersListUpdatedHandler;
    }

    [ClientRpc]
    private void RpcStartGame()
    {
        menuCanvas.enabled = false;
    }
}