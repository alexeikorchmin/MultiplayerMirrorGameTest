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
    [SerializeField] private TMP_Text blinkCooldownText;
    [SerializeField] private float blinkDistance = 20f;
    [SerializeField] private float blinkCooldownValue = 3f;
    [SerializeField] private float blinkDuration = 1f;

    [SyncVar(hook = nameof(BlinkCooldownUpdateHandler))]
    private float currentBlinkCooldown;

    private float blinkClickTime = 0;
    private float blinkIsReady;
    private bool isBlinking;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private Vector3 moveDirection;

    #region Server

    [Server]
    public bool GetIsBlinking()
    {
        return isBlinking;
    }

    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (!isBlinking) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent(out CustomNetworkPlayer enemy);

            if (player.playerIndex == enemy.playerIndex) return;

            if (!enemy.GetCanAttack()) return;

            OnPlayerHit?.Invoke(player.playerIndex, enemy.playerIndex);
        }
    }

    [Command]
    private void CmdBlink()
    {
        if (currentBlinkCooldown > 0) return;

        if ((int)rb.velocity.x == 0 &&
            (int)rb.velocity.z == 0)
        {
            return;
        }

        isBlinking = true;
        blinkClickTime = GameTimeManager.GetGameTime();
        blinkIsReady = blinkClickTime + blinkCooldownValue;
        OnPlayerBlinked?.Invoke(player.playerIndex, blinkClickTime);
        moveDirection = customPlayerMovement.GetMoveDirection();
        rb.AddForce(moveDirection.normalized * blinkDistance, ForceMode.VelocityChange);
        currentBlinkCooldown = blinkIsReady;
        StartCoroutine(ReduceBlinkSpeed());
    }

    [Command]
    private void CmdCheckBlinkCooldownTime()
    {
        if (currentBlinkCooldown == 0) return;

        currentBlinkCooldown = blinkIsReady - GameTimeManager.GetGameTime();

        if (currentBlinkCooldown < 0)
            currentBlinkCooldown = 0;
    }

    #endregion

    #region Client

    private void Awake()
    {
        GlobalScoreManager.OnGameOver += OnGameOverHandler;
    }

    private void OnDestroy()
    {
        GlobalScoreManager.OnGameOver -= OnGameOverHandler;
    }

    [ClientCallback]
    private void Update()
    {
        Blink();
        CheckBlinkCooldownTime();
    }

    private void RpcInit()
    {
        StopAllCoroutines();
        currentBlinkCooldown = 0;
    }

    public void SetBlinkColor(Color newColor)
    {
        blinkCooldownText.color = newColor;
    }

    private void Blink()
    {
        if (!Input.GetKeyDown(blinkKey)) return;

        CmdBlink();
    }

    private void CheckBlinkCooldownTime()
    {
        CmdCheckBlinkCooldownTime();
    }

    private void BlinkCooldownUpdateHandler(float oldValue, float newValue)
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

    #endregion
}