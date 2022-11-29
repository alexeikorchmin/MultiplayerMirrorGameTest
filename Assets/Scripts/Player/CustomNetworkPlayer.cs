using System.Collections;
using UnityEngine;
using Color = UnityEngine.Color;
using TMPro;
using Mirror;

public class CustomNetworkPlayer : NetworkBehaviour
{
    public int playerIndex { get; set; }

    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private Renderer colorRenderer = null;
    [SerializeField] private PlayerBlink playerBlink;
    [SerializeField] private float invulnerabilityDuration = 5;

    [SyncVar(hook = nameof(PlayerNameUpdateHandler))]
    [SerializeField] private string playerName;

    [SyncVar(hook = nameof(PlayerColorUpdateHandler))]
    [SerializeField] private Color playerColor = Color.clear;

    private PlayerDisplayScoreData playerDisplayScoreData;
    private Color previousColor;
    private bool canAttack = true;

    #region Server

    [Server]
    public string GetPlayerName()
    {
        return playerName;
    }

    [Server]
    public bool GetCanAttack()
    {
        return canAttack;
    }

    [Server]
    public void SetPlayerDisplayScoreData(PlayerDisplayScoreData newPlayerDisplayScoreData, bool isActive)
    {
        playerDisplayScoreData = newPlayerDisplayScoreData;
        playerDisplayScoreData.SetGOValue(isActive);
    }

    [Server]
    public void SetPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
    }

    [Server]
    public void SetPlayerColor(Color newPlayerColor)
    {
        playerColor = newPlayerColor;
    }

    #endregion

    #region Client

    private void Awake()
    {
        GlobalScoreManager.OnPlayerWinLose += RpcOnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnPlayerWinLose -= RpcOnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;

        playerDisplayScoreData.SetGOValue(false);
    }

    private void OnGameOverHandler(int winnerIndex)
    {
        RpcInit();
    }

    [ClientRpc]
    private void RpcInit()
    {
        StopAllCoroutines();
        SetPlayerColor(previousColor);
        canAttack = true;
    }

    [ClientRpc]
    private void RpcOnPlayerWinLoseHandler(int winnerIndex, int loserIndex)
    {
        if (playerIndex == winnerIndex)
            WinBattle();
        if (playerIndex == loserIndex)
            LoseBattle();
    }

    private void PlayerNameUpdateHandler(string oldName, string newName)
    {
        playerNameText.text = newName;

        if (playerDisplayScoreData != null)
            playerDisplayScoreData.SetDisplayPlayerName(newName);
    }

    private void PlayerColorUpdateHandler(Color oldColor, Color newColor)
    {
        colorRenderer.material.color = newColor;
        playerNameText.color = newColor;
        playerBlink.SetBlinkColor(newColor);

        if (playerDisplayScoreData != null)
            playerDisplayScoreData.SetDisplayPlayerDataColor(newColor);
    }

    private void WinBattle()
    {
        Debug.Log($"Player {playerName} Won Battle");
    }

    private void LoseBattle()
    {
        Debug.Log($"Player {playerName} Lost Battle");
        previousColor = playerColor;
        SetPlayerColor(Color.black);
        canAttack = false;
        StartCoroutine(WaitForVulnerablitity());
    }

    private IEnumerator WaitForVulnerablitity()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        canAttack = true;
        SetPlayerColor(previousColor);
    }

    #endregion
}