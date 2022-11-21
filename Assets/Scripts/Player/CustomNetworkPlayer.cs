using UnityEngine;
using Color = UnityEngine.Color;
using TMPro;
using Mirror;

public class CustomNetworkPlayer : NetworkBehaviour
{
    public int playerIndex { get; set; }

    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private Renderer colorRenderer = null;

    [SyncVar(hook = nameof(PlayerNameUpdateHandler))]
    [SerializeField] private string playerName;

    [SyncVar(hook = nameof(PlayerColorUpdateHandler))]
    [SerializeField] private Color playerColor;

    private PlayerDisplayScoreData playerDisplayScoreData;
    private int playerScore;

    #region Server

    [Server]
    public void SetPlayerDisplayScoreData(PlayerDisplayScoreData newPlayerDisplayScoreData, bool isActive)
    {
        playerDisplayScoreData = newPlayerDisplayScoreData;
        playerDisplayScoreData.SetGOValue(isActive);
        playerDisplayScoreData.SetDisplayPlayerScore(playerScore.ToString());
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
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnPlayerWinLose -= OnPlayerWinLoseHandler;
    }

    private void OnPlayerWinLoseHandler(int winnerIndex, int loserIndex)
    {
        if (playerIndex == winnerIndex)
            WinBattle();
        if (playerIndex == loserIndex)
            LoseBattle();
    }

    [Command]
    private void CmdWinBattle()
    {
        playerScore++;
    }

    [Command]
    private void CmdLoseBattle()
    {
        SetPlayerColor(Color.black);
    }

    #endregion

    #region Client

    private void PlayerNameUpdateHandler(string oldName, string newName)
    {
        playerNameText.text = newName;

        if (playerDisplayScoreData == null) return;

        playerDisplayScoreData.SetDisplayPlayerName(newName);
    }

    private void PlayerColorUpdateHandler(Color oldColor, Color newColor)
    {
        colorRenderer.material.color = newColor;
        playerNameText.color = newColor;

        if (playerDisplayScoreData == null) return;

        playerDisplayScoreData.SetDisplayPlayerDataColor(newColor);
    }

    private void WinBattle()
    {
        CmdWinBattle();
    }

    private void LoseBattle()
    {
        CmdLoseBattle();
    }

    #endregion
}