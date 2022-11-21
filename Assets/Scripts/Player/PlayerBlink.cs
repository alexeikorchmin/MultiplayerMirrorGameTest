using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerBlink : NetworkBehaviour
{
    public static Action<int, int> OnPlayerHit;
    public static Action<int, float> OnPlayerBlinked;

    [SerializeField] private CustomNetworkPlayer player;
    [SerializeField] private CustomPlayerMovement customPlayerMovement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float blinkDistance = 20f;
    [SerializeField] private float blinkCooldownValue = 3f;
    [SerializeField] private float blinkDuration = 1f;

    private static float blinkCooldown;
    private float blinkClickTime = 0;
    private int playerIndex;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private Vector3 moveDirection;

    private bool isBlinking;

    public static float GetBlinkCooldown()
    {
        return blinkCooldown;
    }

    private void Awake()
    {
        playerIndex = player.playerIndex;
    }

    [Command]
    private void CmdBlink()
    {
        if (!Input.GetKeyDown(blinkKey)) return;

        if (blinkCooldown > 0f) return;

        if (rb.velocity == Vector3.zero) return;

        isBlinking = true;
        blinkClickTime = GameTimeManager.GetGameTime();
        OnPlayerBlinked?.Invoke(playerIndex, blinkClickTime);

        moveDirection = customPlayerMovement.GetMoveDirection();
        rb.AddForce(moveDirection.normalized * blinkDistance, ForceMode.VelocityChange);
        blinkCooldown = blinkCooldownValue;
        StartCoroutine(ReduceBlinkSpeed());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isBlinking) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent(out CustomNetworkPlayer enemy);
            OnPlayerHit?.Invoke(playerIndex, enemy.playerIndex);
        }
    }

    [Command]
    private void CmdCheckCooldownTime()
    {
        if (blinkCooldown == 0) return;

        blinkCooldown -= Time.deltaTime;

        if (blinkCooldown < 0)
            blinkCooldown = 0f;
    }

    [ClientCallback]
    private void Update()
    {
        if (!isOwned) return;

        CmdBlink();
        CmdCheckCooldownTime();
    }

    private IEnumerator ReduceBlinkSpeed()
    {
        yield return new WaitForSeconds(blinkDuration);
        isBlinking = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}