using System.Collections;
using UnityEngine;

public class PlayerBlink : MonoBehaviour
{
    [SerializeField] private CustomPlayerMovement customPlayerMovement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float blinkValue;
    [SerializeField] private float blinkCooldown;

    private float blinkClickTime;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private Vector3 moveDirection;

    private void Update()
    {
        Blink();
        blinkClickTime -= Time.deltaTime;
    }

    private void Blink()
    {
        if (!Input.GetKeyDown(blinkKey)) return;

        if (blinkClickTime > 0f) return;

        moveDirection = customPlayerMovement.GetMoveDirection();
        rb.AddForce(moveDirection.normalized * blinkValue, ForceMode.VelocityChange);
        blinkClickTime = blinkCooldown;
        StartCoroutine(ReduceBlinkSpeed());
    }

    private IEnumerator ReduceBlinkSpeed()
    {
        yield return new WaitForSeconds(1f);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}