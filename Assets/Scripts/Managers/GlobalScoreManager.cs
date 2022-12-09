using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GlobalScoreManager : NetworkBehaviour
{
    public static event Action<int, int> OnPlayerWinLose;
    public static event Action<int> OnGameOver;

    [SerializeField] private PlayerDisplayScoreDataList playerDisplayScoreDataList;

    private Dictionary<int, float> playersBlinkDatas = new Dictionary<int, float>();
    private Dictionary<int, int> playersScores = new Dictionary<int, int>();

    #region Server

    [Server]
    public void ResetAndUpdatePlayersScores()
    {
        playersBlinkDatas.Clear();

        for (int i = 0; i < playersScores.Count; i++)
        {
            playersScores[i] = 0;
            ShowUpdatedScore(i);
        }
    }

    [Server]
    private void SaveAndShowWinnerScore(int winnerIndex)
    {
        if (playersScores.ContainsKey(winnerIndex))
            playersScores[winnerIndex]++;
        else
            playersScores.Add(winnerIndex, 1);

        ShowUpdatedScore(winnerIndex);
    }

    [Server]
    private void ShowUpdatedScore(int winnerIndex)
    {
        if (winnerIndex >= playerDisplayScoreDataList.GetPlayerDisplayScoreDataList().Count) return;

        int newScore = playersScores[winnerIndex];
        playerDisplayScoreDataList.GetPlayerDisplayScoreDataList()[winnerIndex].SetDisplayPlayerScore(newScore.ToString());
    }

    [Server]
    private void CheckGameWinner()
    {
        foreach (var player in playersScores)
        {
            if (player.Value == 3)
            {
                OnGameOver?.Invoke(player.Key);
                break;
            }
        }
    }

    #endregion

    #region Client

    private void Awake()
    {
        PlayerBlink.OnPlayerBlinked += OnPlayerBlinkedHandler;
        PlayerBlink.OnPlayerHit += OnPlayerHitHandler;
    }

    private void OnDestroy()
    {
        PlayerBlink.OnPlayerBlinked -= OnPlayerBlinkedHandler;
        PlayerBlink.OnPlayerHit -= OnPlayerHitHandler;
    }

    private void OnPlayerBlinkedHandler(int index, float blinkTime)
    {
        if (playersBlinkDatas.ContainsKey(index))
            playersBlinkDatas[index] = blinkTime;
        else
            playersBlinkDatas.Add(index, blinkTime);
    }

    private void OnPlayerHitHandler(int playerIndex, int enemyIndex, bool isEnemyBlinking)
    {
        if (isEnemyBlinking == false)
        {
            OnPlayerWinLose?.Invoke(playerIndex, enemyIndex);
            SaveAndShowWinnerScore(playerIndex);
            CheckGameWinner();
        }
        else
        {
            if (playersBlinkDatas[playerIndex] < playersBlinkDatas[enemyIndex])
            {
                OnPlayerWinLose?.Invoke(playerIndex, enemyIndex);
                SaveAndShowWinnerScore(playerIndex);
            }
            else if (playersBlinkDatas[playerIndex] > playersBlinkDatas[enemyIndex])
            {
                OnPlayerWinLose?.Invoke(enemyIndex, playerIndex);
                SaveAndShowWinnerScore(enemyIndex);
            }
            else if (playersBlinkDatas[playerIndex] == playersBlinkDatas[enemyIndex]) return;

            CheckGameWinner();
        }
    }

    #endregion
}