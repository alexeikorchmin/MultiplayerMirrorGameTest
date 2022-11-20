using UnityEngine;
using Mirror;
using TMPro;

public class CustomNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameText = null;
    [SerializeField] private Renderer colorRenderer = null;

    [SyncVar(hook = nameof(PlayerNameUpdateHandler))]
    [SerializeField] private string playerName;

    [SyncVar(hook = nameof(PlayerColorUpdateHandler))]
    [SerializeField] private Color playerColor = Color.black;

    #region Server

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

    [Command]
    private void CmdSetPlayerName(string newPlayerName)
    {
        if (newPlayerName.Length < 2 || newPlayerName.Length > 15) return;

        RpcShowNewName(newPlayerName);
        SetPlayerName(newPlayerName);
    }

    #endregion

    #region Cliend

    private void PlayerNameUpdateHandler(string oldName, string newName)
    {
        playerNameText.text = newName;
    }

    private void PlayerColorUpdateHandler(Color oldColor, Color newColor)
    {
        colorRenderer.material.color = newColor;
        playerNameText.color = newColor;
    }

    [ContextMenu("Set New Name")]
    private void SetNewName()
    {
        CmdSetPlayerName("Player New Name");
    }

    [ClientRpc]
    private void RpcShowNewName(string newName)
    {
        Debug.Log(newName);
    }

    #endregion
}