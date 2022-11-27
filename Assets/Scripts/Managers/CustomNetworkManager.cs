using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerDisplayScoreDataList playerDisplayScoreDataList;
    [SerializeField] private RestartGameManager restartGameManager;

    private List<PlayerDisplayScoreData> playerDisplayScoreDatas = new List<PlayerDisplayScoreData>();
    private int playerIndex;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        playerDisplayScoreDatas = playerDisplayScoreDataList.GetDisplayScoreDataList();

        if (!CheckIsEmptySpace(playerDisplayScoreDatas)) return;

        CustomNetworkPlayer player = conn.identity.GetComponent<CustomNetworkPlayer>();
        player.playerIndex = playerIndex;
        PlayerDisplayScoreData playerDisplayScoreData = playerDisplayScoreDatas[playerIndex];
        player.SetPlayerDisplayScoreData(playerDisplayScoreData, true);

        player.SetPlayerName($"Player {playerIndex}");

        Color color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f));

        player.SetPlayerColor(color);
        restartGameManager.AddPlayerToList(player);
    }

    private bool CheckIsEmptySpace(List<PlayerDisplayScoreData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetGOValue() == false)
            {
                playerIndex = i;
                return true;
            }
        }
        return false;
    }
}