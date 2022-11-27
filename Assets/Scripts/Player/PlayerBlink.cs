using System;
using System.Collections;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerBlink : NetworkBehaviour
{
    public static event Action<int, int> OnPlayerHit;
    public static event Action<int, float> OnPlayerBlinked;

    [SerializeField] private CustomNetworkPlayer player;
    [SerializeField] private CustomPlayerMovement customPlayerMovement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float blinkDistance = 20f;
    [SerializeField] private float blinkCooldownInitValue = 3f;
    [SerializeField] private float blinkDuration = 1f;

    [SyncVar(hook = nameof(UpdateCDText))]
    private float blinkCooldown;

    private float blinkClickTime = 0;
    private bool isBlinking;
    private static int playerIndex;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private Vector3 moveDirection;

    [SerializeField] private TMP_Text blinkCooldownText;

    public void SetBlinkColor(Color newColor)
    {
        blinkCooldownText.color = newColor;
    }

    public bool GetIsBlinking()
    {
        //print($"Player {player.playerIndex} GetIsBlinking = {isBlinking}");
        return isBlinking;
    }

    private void Awake()
    {
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
        playerIndex = player.playerIndex;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isBlinking) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent(out CustomNetworkPlayer enemy);

            if (!enemy.GetCanAttack()) return;

            OnPlayerHit?.Invoke(playerIndex, enemy.playerIndex);
        }
    }

    [Command]
    private void CmdBlink()
    {
        RpcBlink();
    }

    [Command]
    private void CmdCheckCooldownTime()
    {
        RpcCheckCooldownTime();
    }

    [ClientCallback]
    private void Update()
    {
        if (!isOwned) return;

        Blink();
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isOwned) return;

        CheckCooldownTime();
    }

    private void Blink()
    {
        if (!isLocalPlayer) return;

        if (!Input.GetKeyDown(blinkKey)) return;

        CmdBlink();
    }

    [ClientRpc]
    private void RpcBlink()
    {
        if (blinkCooldown > 0f) return;

        if (rb.velocity == Vector3.zero) return;

        isBlinking = true;
        blinkClickTime = GameTimeManager.GetGameTime();
        OnPlayerBlinked?.Invoke(playerIndex, blinkClickTime);
        moveDirection = customPlayerMovement.GetMoveDirection();
        rb.AddForce(moveDirection.normalized * blinkDistance, ForceMode.VelocityChange);
        blinkCooldown = blinkCooldownInitValue;
        StartCoroutine(ReduceBlinkSpeed());
    }

    private void CheckCooldownTime()
    {
        CmdCheckCooldownTime();
    }

    [ClientRpc]
    private void RpcCheckCooldownTime()
    {
        if (blinkCooldown == 0)
        {
            return;
        }

        blinkCooldown -= Time.deltaTime;

        if (blinkCooldown < 0)
            blinkCooldown = 0f;
    }

    private void UpdateCDText(float oldValue, float newValue)
    {
        blinkCooldownText.text = $"{Math.Round(newValue, 1)}";
    }

    private IEnumerator ReduceBlinkSpeed()
    {
        yield return new WaitForSeconds(blinkDuration);
        isBlinking = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
        blinkCooldown = blinkCooldownInitValue;
    }
}