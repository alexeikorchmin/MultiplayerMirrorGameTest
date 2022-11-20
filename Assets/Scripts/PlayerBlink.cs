using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class PlayerBlink : NetworkBehaviour
{
    public static Action<float> OnBlinkValue;
    
    [SerializeField] private CustomPlayerMovement customPlayerMovement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float blinkValue = 20f;
    [SerializeField] private float blinkCooldown = 3f;
    [SerializeField] private float blinkDuration = 1f;

    private float blinkClickTime;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private Vector3 moveDirection;

    [Command]
    private void CmdBlink()
    {
        if (!Input.GetKeyDown(blinkKey)) return;

        if (blinkClickTime > 0f) return;

        if (rb.velocity == Vector3.zero) return;

        moveDirection = customPlayerMovement.GetMoveDirection();
        rb.AddForce(moveDirection.normalized * blinkValue, ForceMode.VelocityChange);
        blinkClickTime = blinkCooldown;
        StartCoroutine(ReduceBlinkSpeed());
    }

    [ClientCallback]
    private void Update()
    {
        if (!isOwned) return;

        CmdBlink();
        CheckCooldownTime();
    }

    private void CheckCooldownTime()
    {
        if (blinkClickTime == 0) return;

        blinkClickTime -= Time.deltaTime;

        if (blinkClickTime < 0)
            blinkClickTime = 0f;

        OnBlinkValue?.Invoke(blinkClickTime);
    }

    private IEnumerator ReduceBlinkSpeed()
    {
        yield return new WaitForSeconds(blinkDuration);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}