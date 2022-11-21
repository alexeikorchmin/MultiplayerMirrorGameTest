using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDisplayScoreDataList: NetworkBehaviour
{
    [SerializeField] private List<PlayerDisplayScoreData> playerDisplayScoreDataList;

    public List<PlayerDisplayScoreData> GetDisplayScoreDataList()
    {
        return playerDisplayScoreDataList;
    }
}