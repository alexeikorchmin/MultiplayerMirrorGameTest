using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class RestartGameManager : NetworkBehaviour
{
    public static event Action<bool> OnCanMove;

    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TMP_Text winnerNameText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float countdownInitValue;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
    [SerializeField] private GlobalScoreManager globalScoreManager;
    [SerializeField] private CustomNetworkManager customNetworkManager;
    
    [SyncVar(hook = nameof(UpdateWinnerName))]
    private string winnerName;

    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();
    private float countdownValue;
    private bool canCountdown;

    #region Server

    [Server]
    private void SetPlayersToSpawnPositions()
    {
        print($"SetPlayersToSpawnPositions: players.Count = {players.Count}");

        foreach (var player in players)
        {
            var random = UnityEngine.Random.Range(0, spawnPositions.Count);

            if (player == null) return;

            player.transform.position = spawnPositions[random].position;
        }
    }

    [Server]
    private void FindWinnerNameByIndex(int index)
    {
        players = customNetworkManager.GetPlayersList();
        print($"FindWinnerNameByIndex: players.Count = {players.Count}");

        foreach (var player in players)
        {
            if (player == null) return;

            if (player.playerIndex == index)
            {
                winnerName = player.GetPlayerName();
                return;
            }
        }
    }
        
    #endregion

    #region Client

    private void Awake()
    {
        GlobalScoreManager.OnGameOver += RpcOnGameOverHandler;
        GameOverPanel.SetActive(false);
        countdownValue = countdownInitValue;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnGameOver -= RpcOnGameOverHandler;
    }

    private void FixedUpdate()
    {
        Countdown();
    }

    private void Countdown()
    {
        if (!canCountdown) return;

        countdownText.text = ((int)countdownValue).ToString();
        countdownValue -= Time.deltaTime;
    }

    private void UpdateWinnerName(string oldWinner, string newWinner)
    {
        winnerNameText.text = $"Winner is {newWinner}";
    }

    [ClientRpc]
    private void RpcOnGameOverHandler(int winnerIndex)
    {
        OnCanMove?.Invoke(false);
        FindWinnerNameByIndex(winnerIndex);
        StartCoroutine(WaitForRestartingGame());
    }

    private IEnumerator WaitForRestartingGame()
    {
        yield return new WaitForSeconds(countdownInitValue);
        globalScoreManager.ResetAndUpdatePlayersScores();
        GameOverPanel.SetActive(true);
        SetPlayersToSpawnPositions();
        canCountdown = true;
        yield return new WaitForSeconds(countdownInitValue);
        GameOverPanel.SetActive(false);
        canCountdown = false;
        countdownValue = countdownInitValue;
        OnCanMove?.Invoke(true);
    }

    #endregion
}