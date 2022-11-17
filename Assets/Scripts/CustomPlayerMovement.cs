using System.Collections;
using UnityEngine;
using Mirror;
using Cinemachine;

public class CustomPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speedValue = 5f;
    [SerializeField] private float jumpValue = 4f;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private float blinkValue = 10f;
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private Rigidbody rb;

    private Vector3 movementDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode blinkKey = KeyCode.Mouse0;
    private bool isGrounded = true;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    private float maxLook = 60f;

    private Vector3 startPosition;

    private void Awake()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        startPosition = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Jump();
        MouseMovement();
        PlayerBlink();
        CheckFallPosition();
    }

    private void FixedUpdate()
    {
        PlayerMove();
    }

    #region MouseMovement

    private void CheckFallPosition()
    {
        if (transform.position.y < -5)
            transform.position = startPosition;
    }

    private void MouseMovement()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        xRotation -= mouseY * sensitivity;
        yRotation += mouseX * sensitivity;

        xRotation = Mathf.Clamp(xRotation, -maxLook, maxLook);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    #endregion

    #region KeyboardMovement

    private void PlayerMove()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        movementDirection = new Vector3(horizontalMovement, 0, verticalMovement);
        transform.Translate(movementDirection * (speedValue * Time.deltaTime));
    }

    private void Jump()
    {
        if (!Input.GetKeyDown(jumpKey) || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpValue, ForceMode.VelocityChange);
        isGrounded = false;
        StartCoroutine(WaitForJumpValueChange());
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (isGrounded) return;

    //    if (collision.gameObject.CompareTag("Ground"))
    //        isGrounded = true;
    //}

    private IEnumerator WaitForJumpValueChange()
    {
        yield return new WaitForSeconds(1f);
        isGrounded = true;
    }

    #endregion

    #region Blink

    private void PlayerBlink()
    {
        if (!Input.GetKeyDown(blinkKey)) return;

        //rb.AddForce(movementDirection * blinkValue, ForceMode.VelocityChange);
        transform.Translate(movementDirection.normalized * blinkValue);
    }

    #endregion
}