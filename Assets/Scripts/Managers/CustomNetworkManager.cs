using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<List<CustomNetworkPlayer>> OnPlayersListUpdated;

    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();
    private int playerIndex;

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        var player = conn.identity.GetComponent<CustomNetworkPlayer>();

        if (player == null) return;

        print($"OnServerDisconnect: player{player.playerIndex} disconnected");
        players.Remove(player);

        foreach (var eachPlayer in players)
        {
            print($"UpdateIndex/ playerName= {eachPlayer.GetPlayerName()} Old Index= {eachPlayer.playerIndex}");
            eachPlayer.SetPlayerIndex(players.IndexOf(eachPlayer));
            print($"UpdateIndex/ playerName= {eachPlayer.GetPlayerName()} New Index= {eachPlayer.playerIndex}");
        }

        OnPlayersListUpdated?.Invoke(players);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        print($"OnStopServer: players.Clear();");
        players.Clear();
        OnPlayersListUpdated?.Invoke(players);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (IsEmptySpaceInPlayersList() == false) return;

        base.OnServerAddPlayer(conn);

        CustomNetworkPlayer player = conn.identity.GetComponent<CustomNetworkPlayer>();
        players.Add(player);
        playerIndex = players.IndexOf(player);
        player.SetPlayerIndex(playerIndex);
        print($"NetManager: playerIndex = {playerIndex}");
        player.SetPlayerName($"Player {players.Count}");

        Color color = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f));

        player.SetPlayerColor(color);
        OnPlayersListUpdated?.Invoke(players);
    }

    [Server]
    private bool IsEmptySpaceInPlayersList()
    {
        if (players.Count == maxConnections)
            return false;

        return true;
    }
}