using UnityEngine;
using Mirror;
using Cinemachine;

public class CustomPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speedValue = 5f;
    [SerializeField] private float jumpValue = 4f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private Rigidbody rb;

    private Vector3 startPosition;
    private Vector3 movementDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private KeyCode jumpKey = KeyCode.Space;
    private bool isGrounded = true;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    private float maxLook = 60f;


    #region Server

    [Command]
    private void CmdPlayerMove()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        if (Mathf.Approximately(horizontalMovement, 0f) &&
            Mathf.Approximately(verticalMovement, 0f) &&
            isGrounded)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else if (Mathf.Abs(rb.velocity.x) < maxSpeed &&
                 Mathf.Abs(rb.velocity.y) < maxSpeed &&
                 Mathf.Abs(rb.velocity.z) < maxSpeed)
        {
            movementDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
            rb.AddForce(movementDirection.normalized * speedValue, ForceMode.VelocityChange);
        }
    }

    [Command]
    private void CmdJump()
    {
        if (!Input.GetKeyDown(jumpKey) || !isGrounded) return;

        rb.AddForce(Vector3.up * jumpValue, ForceMode.VelocityChange);
        isGrounded = false;
    }

    [Command]
    private void CmdMouseMovement()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        xRotation -= mouseY * mouseSensitivity;
        yRotation += mouseX * mouseSensitivity;

        xRotation = Mathf.Clamp(xRotation, -maxLook, maxLook);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    [Command]
    private void CmdCheckFallPosition()
    {
        if (transform.position.y < -5)
            transform.position = startPosition;
    }

    #endregion

    #region Client

    public Vector3 GetMoveDirection()
    {
        return movementDirection;
    }

    [ClientCallback]
    private void Awake()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        startPosition = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    [ClientCallback]
    private void Update()
    {
        if (!isOwned) return;

        CmdJump();
        CmdMouseMovement();
        CmdCheckFallPosition();
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isOwned) return;

        CmdPlayerMove();
    }

    [ClientCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!isOwned) return;

        if (isGrounded) return;

        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    #endregion
}