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

        players.Remove(player);

        foreach (var eachPlayer in players)
        {
            eachPlayer.SetPlayerIndex(players.IndexOf(eachPlayer));
        }

        OnPlayersListUpdated?.Invoke(players);
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        players.Clear();
        OnPlayersListUpdated?.Invoke(players);
    }

    public override void OnClientConnect()
    {
        OnClientConnected?.Invoke();
        base.OnClientConnect();
    }

    public override void OnClientDisconnect()
    {
        OnClientDisconnected?.Invoke();
        base.OnClientDisconnect();
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