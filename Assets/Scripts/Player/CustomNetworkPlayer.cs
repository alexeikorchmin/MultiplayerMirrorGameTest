using System.Collections;
using UnityEngine;
using Color = UnityEngine.Color;
using TMPro;
using Mirror;
using System;

public class CustomNetworkPlayer : NetworkBehaviour
{
    public static event Action<CustomNetworkPlayer> OnPlayerExitGame;

    public int playerIndex { get; private set; }
        
    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private Renderer colorRenderer = null;
    [SerializeField] private PlayerBlink playerBlink;
    [SerializeField] private float invulnerabilityDuration = 5;

    [SyncVar(hook = nameof(PlayerNameUpdateHandler))]
    [SerializeField] private string playerName;

    [SyncVar(hook = nameof(PlayerColorUpdateHandler))]
    [SerializeField] private Color playerColor = Color.clear;

    [SyncVar(hook = nameof(PlayerDisplayScoreUpdateHandler))]
    private PlayerDisplayScoreData playerDisplayScoreData;

    private Color previousColor = Color.clear;
    private Color isAttackedColor = Color.black;
    private bool canBeAttacked = true;

    #region Server

    [Server]
    public bool GetIsBlinking() => playerBlink.GetIsBlinking();

    [Server]
    public string GetPlayerName() => playerName;

    [Server]
    public bool GetCanBeAttacked() => canBeAttacked;

    [Server]
    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
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

    [Server]
    public void SetPlayerDisplayScoreData(PlayerDisplayScoreData newPlayerDisplayScoreData, bool isActive)
    {
        playerDisplayScoreData = newPlayerDisplayScoreData;
        playerDisplayScoreData.SetGOValue(isActive);
    }

    [Command]
    private void CmdInit()
    {
        RpcInit();
    }

    #endregion

    #region Client

    public override void OnStopClient()
    {
        OnPlayerExitGame?.Invoke(this);
        base.OnStopClient();
    }

    private void Awake()
    {
        GlobalScoreManager.OnPlayerWinLose += RpcOnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnPlayerWinLose -= RpcOnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;
    }

    [ClientRpc]
    private void RpcInit()
    {
        StopAllCoroutines();
        canBeAttacked = true;

        if (previousColor != Color.black &&
            previousColor != null &&
            previousColor != Color.clear)
        {
            SetPlayerColor(previousColor);
        }
    }

    [ClientRpc]
    private void RpcOnPlayerWinLoseHandler(int winnerIndex, int loserIndex)
    {
        if (playerIndex == winnerIndex)
            WinBattle();
        else if (playerIndex == loserIndex)
            LoseBattle();
    }

    private void OnGameOverHandler(int winnerIndex)
    {
        CmdInit();
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

    private void PlayerDisplayScoreUpdateHandler(PlayerDisplayScoreData oldData, PlayerDisplayScoreData newData)
    {
        if (playerDisplayScoreData == null) return;

        playerDisplayScoreData.SetDisplayPlayerName(playerName);
        playerDisplayScoreData.SetDisplayPlayerDataColor(playerColor);
    }

    private void WinBattle()
    {
        Debug.Log($"WinBattle: PlayerIndex= {playerIndex}, PlayerName = {playerName} Won Battle");
    }

    private void LoseBattle()
    {
        previousColor = playerColor;

        SetPlayerColor(isAttackedColor);
        canBeAttacked = false;
        StartCoroutine(WaitForVulnerablitity());
    }

    private IEnumerator WaitForVulnerablitity()
    {
        yield return new WaitForSeconds(invulnerabilityDuration);
        canBeAttacked = true;
        SetPlayerColor(previousColor);
    }

    #endregion
}