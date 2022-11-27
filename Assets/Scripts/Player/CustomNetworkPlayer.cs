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

    public string GetPlayerName()
    {
        return playerName;
    }

    public bool GetCanAttack()
    {
        return canAttack;
    }

    #region Server

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

    private void Awake()
    {
        GlobalScoreManager.OnPlayerWinLose += OnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnPlayerWinLose -= OnPlayerWinLoseHandler;
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;
    }

    private void OnPlayerWinLoseHandler(int winnerIndex, int loserIndex)
    {
        if (playerIndex == winnerIndex)
            RpcWinBattle();
        if (playerIndex == loserIndex)
            RpcLoseBattle();
    }

    private void OnGameOverHandler(int winnerIndex)
    {
        RpcInit();
    }

    [ClientRpc]
    private void RpcInit()
    {
        if (!isLocalPlayer) return;

        StopAllCoroutines();
        SetPlayerColor(previousColor);
        canAttack = true;
    }

    [ContextMenu("Set New Index")]
    private void SetNewIndex()
    {
        playerIndex = 2;
    }

    #endregion

    #region Client

    private void PlayerNameUpdateHandler(string oldName, string newName)
    {
        if (!isLocalPlayer) return;

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

    [ClientRpc]
    private void RpcWinBattle()
    {
        Debug.Log($"Player {playerName} Index {playerIndex} Wins Battle");
    }

    [ClientRpc]
    private void RpcLoseBattle()
    {
        if (!isLocalPlayer) return;

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