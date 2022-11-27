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

    private List<CustomNetworkPlayer> players = new List<CustomNetworkPlayer>();
    private float countdownValue;
    private bool canCountdown;

    [SyncVar(hook = nameof(UpdateWinnerName))]
    private string winnerName;

    private void UpdateWinnerName(string oldWinner, string newWinner)
    {
        winnerNameText.text = $"Winner is {newWinner}";
    }

    [Server]
    public void AddPlayerToList(CustomNetworkPlayer player)
    {
        players.Add(player);
    }

    [Server]
    private void SetPlayersToSpawnPositions()
    {
        foreach (var player in players)
        {
            var random = UnityEngine.Random.Range(0, spawnPositions.Count);
            player.transform.position = spawnPositions[random].position;
        }
    }

    private void Awake()
    {
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
        GameOverPanel.SetActive(false);
        countdownValue = countdownInitValue;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;
    }

    private void Update()
    {
        if (!canCountdown) return;

        countdownText.text = ((int)countdownValue).ToString();
        countdownValue -= Time.deltaTime;
    }

    [Server]
    private void FindWinnerNameByIndex(int index)
    {
        foreach (var player in players)
        {
            if (player.playerIndex == index)
            {
                winnerName = player.GetPlayerName();
                return;
            }
        }

        return;
    }

    private void OnGameOverHandler(int winnerIndex)
    {
        FindWinnerNameByIndex(winnerIndex);
        RpcOnGameOverHandler(winnerIndex);
        SetPlayersToSpawnPositions();
    }

    [ClientRpc]
    private void RpcOnGameOverHandler(int winnerIndex)
    {
        OnCanMove?.Invoke(false);
        StartCoroutine(WaitForRestartingGame());
    }

    private IEnumerator WaitForRestartingGame()
    {
        GameOverPanel.SetActive(true);
        canCountdown = true;
        yield return new WaitForSeconds(countdownInitValue);
        GameOverPanel.SetActive(false);
        canCountdown = false;
        countdownValue = countdownInitValue;
        OnCanMove?.Invoke(true);
    }
}