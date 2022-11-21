using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GlobalScoreManager : NetworkBehaviour
{
    public static Action<int, int> OnPlayerWinLose;
    public static Action OnRestartGame;

    [SerializeField] PlayerDisplayScoreDataList playerDisplayScoreDataList;

    private Dictionary<int, float> playersBlinkDatas = new Dictionary<int, float>();
    private Dictionary<int, int> playersScores = new Dictionary<int, int>();

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

    private void OnPlayerHitHandler(int playerIndex, int enemyIndex)
    {
        if (playersBlinkDatas.ContainsKey(enemyIndex) == false)
        {
            OnPlayerWinLose?.Invoke(playerIndex, enemyIndex);
            SaveWinnerScore(playerIndex);
            CheckGameWinner();
            return;
        }

        if (playersBlinkDatas[playerIndex] < playersBlinkDatas[enemyIndex])
        {
            OnPlayerWinLose?.Invoke(playerIndex, enemyIndex);
            SaveWinnerScore(playerIndex);
        }
        else
        {
            OnPlayerWinLose?.Invoke(enemyIndex, playerIndex);
            SaveWinnerScore(enemyIndex);
        }

        CheckGameWinner();
    }

    private void SaveWinnerScore(int winnerIndex)
    {
        if (playersScores.ContainsKey(winnerIndex))
        {
            playersScores[winnerIndex]++;
        }
        else
        {
            playersScores.Add(winnerIndex, 1);
        }

        ShowUpdatedScore(winnerIndex);
    }

    private void ShowUpdatedScore(int winnerIndex)
    {
        int newScore = playersScores[winnerIndex];
        playerDisplayScoreDataList.GetDisplayScoreDataList()[winnerIndex].SetDisplayPlayerScore(newScore.ToString());
    }

    private void CheckGameWinner()
    {
        foreach (var player in playersScores)
        {
            if (player.Value == 3)
            {
                OnRestartGame?.Invoke();
                break;
            }
        }
    }
}