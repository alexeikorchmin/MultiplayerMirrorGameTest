using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDisplayScoreDataList : NetworkBehaviour
{
    [SerializeField] private List<PlayerDisplayScoreData> playerDisplayScoreDataList;

    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();

    public List<PlayerDisplayScoreData> GetPlayerDisplayScoreDataList() => playerDisplayScoreDataList;

    private void Awake()
    {
        CustomNetworkManager.OnPlayersListUpdated += OnPlayersListUpdatedHandler;
    }

    private void OnDestroy()
    {
        CustomNetworkManager.OnPlayersListUpdated -= OnPlayersListUpdatedHandler;
    }

    [Server]
    private void OnPlayersListUpdatedHandler(List<CustomNetworkPlayer> newPlayersList)
    {
        players = newPlayersList;

        for (int i = 0; i < playerDisplayScoreDataList.Count; i++)
        {
            if (i < players.Count)
            {
                players[i].SetPlayerDisplayScoreData(playerDisplayScoreDataList[i]);
                playerDisplayScoreDataList[i].SetGOValue(true);
            }
            else
                playerDisplayScoreDataList[i].SetGOValue(false);
        }
    }
}