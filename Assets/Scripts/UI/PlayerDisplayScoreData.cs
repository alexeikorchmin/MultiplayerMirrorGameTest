using UnityEngine;
using TMPro;
using Mirror;

public class PlayerDisplayScoreData : NetworkBehaviour
{
    [SerializeField] private GameObject go;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerScoreText;

    [SyncVar(hook = nameof(UpdatedName))]
    private string playerName;

    [SyncVar(hook = nameof(UpdatedScore))]
    private string playerScore;

    [SyncVar(hook = nameof(UpdatedValueGO))]
    private bool isGOActive;

    [SyncVar(hook = nameof(UpdatedColor))]
    private Color color;

    #region Server

    [Server]
    public bool GetGOValue()
    {
        return go.activeSelf;
    }

    [Server]
    public void SetDisplayPlayerName(string name)
    {
        playerName = name;
    }

    [Server]
    public void SetDisplayPlayerScore(string score)
    {
        playerScore = score;
    }

    [Server]
    public void SetDisplayPlayerDataColor(Color newColor)
    {
        color = newColor;
    }

    [Server]
    public void SetGOValue(bool isActive)
    {
        isGOActive = isActive;
    }

    #endregion

    #region Client

    private void UpdatedColor(Color oldColor, Color newColor)
    {
        playerNameText.color = newColor;
        playerScoreText.color = newColor;
    }

    private void UpdatedName(string oldName, string newName)
    {
        playerNameText.text = newName;
    }

    private void UpdatedScore(string oldScore, string newScore)
    {
        playerScoreText.text = newScore;
    }

    private void UpdatedValueGO(bool oldState, bool newState)
    {
        go.SetActive(newState);
    }

    #endregion
}