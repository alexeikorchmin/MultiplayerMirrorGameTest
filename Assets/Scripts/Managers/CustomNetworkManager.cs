using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    
    [SerializeField] private PlayerDisplayScoreDataList playerDisplayScoreDataList;

    private List<PlayerDisplayScoreData> playerDisplayScoreDatas = new List<PlayerDisplayScoreData>();
    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();

    private int playerIndex;

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

    public override void Awake()
    {
        base.Awake();
        playerDisplayScoreDatas = playerDisplayScoreDataList.GetDisplayScoreDataList();
        CustomNetworkPlayer.OnPlayerExitGame += OnPlayerExitGameHandler;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        CustomNetworkPlayer.OnPlayerExitGame -= OnPlayerExitGameHandler;
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
        PlayerDisplayScoreData playerDisplayScoreData = playerDisplayScoreDatas[playerIndex];
        player.SetPlayerDisplayScoreData(playerDisplayScoreData, true);

        player.SetPlayerName($"Player {players.Count}");

        Color color = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f));

        player.SetPlayerColor(color);
    }

    [Server]
    public List<CustomNetworkPlayer> GetPlayersList() => players;

    [Server]
    private void OnPlayerExitGameHandler(CustomNetworkPlayer playerLeft)
    {
        if (playerLeft == null) return;

        players.Remove(playerLeft);

        foreach (var player in players)
        {
            print($"UpdateIndex/ playerName= {player.GetPlayerName()} Old Index= {player.playerIndex}");
            player.SetPlayerIndex(players.IndexOf(player));
            print($"UpdateIndex/ playerName= {player.GetPlayerName()} New Index= {player.playerIndex}");
        }

        UpdatePlayersDisplayScores(playerDisplayScoreDatas);
    }

    [Server]
    private void UpdatePlayersDisplayScores(List<PlayerDisplayScoreData> displayScoreList)
    {
        print($"UpdatePlayerDisplayScore/ Before 0 Check: players.Count = {players.Count}");

        if (players.Count == 0) return;

        print($"UpdatePlayerDisplayScore/ After 0 Check: players.Count = {players.Count}");

        for (int i = 0; i < displayScoreList.Count; i++)
        {
            if (i < players.Count)
            {
                print($" [i] = {i}");
                players[i].SetPlayerDisplayScoreData(displayScoreList[i], true);
                print($"displayScoreList{i} = {displayScoreList[i]} = true by player");
            }
            else
            {
                displayScoreList[i].SetGOValue(false);
                print($"displayScoreList{i} = {displayScoreList[i]} = false");
            }
        }
    }

    [Server]
    private bool IsEmptySpaceInPlayersList()
    {
        if (players.Count == maxConnections) 
            return false;
        
        return true;
    }
}