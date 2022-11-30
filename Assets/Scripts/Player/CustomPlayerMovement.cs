using UnityEngine;
using Mirror;

public class CustomPlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speedValue = 5f;
    [SerializeField] private float jumpValue = 4f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private PlayerBlink playerBlink;

    private Vector3 startPosition;
    private Vector3 movementDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private KeyCode jumpKey = KeyCode.Space;
    private bool isGrounded = true;
    private bool canMove = true;
    private bool isBlinking;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    private float maxLook = 60f;

    #region Server

    [Server]
    public Vector3 GetMoveDirection()
    {
        return movementDirection;
    }

    [Server]
    private void OnCollisionEnter(Collision collision)
    {
        if (isGrounded) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    [Command]
    private void CmdInit(bool canMove)
    {
        RpcInit(canMove);
    }

    [Command]
    private void CmdMovePlayer(float horizontal, float vertical)
    {
        if (!canMove) return;

        isBlinking = playerBlink.GetIsBlinking();

        if (isBlinking) return;

        if (Mathf.Approximately(horizontal, 0f) &&
            Mathf.Approximately(vertical, 0f) &&
            isGrounded == true)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else if (Mathf.Abs(rb.velocity.x) < maxSpeed &&
                 Mathf.Abs(rb.velocity.y) < maxSpeed &&
                 Mathf.Abs(rb.velocity.z) < maxSpeed &&
                 isBlinking == false)
        {
            movementDirection = transform.forward * vertical + transform.right * horizontal;
            rb.AddForce(movementDirection.normalized * speedValue, ForceMode.VelocityChange);
        }
    }

    [Command]
    private void CmdJump()
    {
        if (!canMove) return;

        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpValue, ForceMode.VelocityChange);
        isGrounded = false;
    }

    [Command]
    private void CmdMouseMovement(float rotationY)
    {
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    [Command]
    private void CmdCheckFallPosition(GameObject go)
    {
        if (transform.position.y < -5)
            go.transform.position = startPosition;
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        SetPlayerCamera();
    }

    private void Awake()
    {
        RestartGameManager.OnCanMove += OnCanMoveHandler;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        startPosition = transform.position;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDestroy()
    {
        RestartGameManager.OnCanMove -= OnCanMoveHandler;
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) return;

        Jump();
        MouseMovement();
        CheckFallPosition();
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!hasAuthority) return;

        MovePlayer();
    }

    [ClientRpc]
    private void RpcInit(bool canMove)
    {
        this.canMove = canMove;
    }

    private void SetPlayerCamera()
    {
        Transform cameraTransform = Camera.main.gameObject.transform;
        cameraTransform.parent = cameraHolder.transform;
        cameraTransform.position = cameraHolder.transform.position;
        cameraTransform.rotation = cameraHolder.transform.rotation;
    }

    private void MovePlayer()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");
        CmdMovePlayer(horizontalMovement, verticalMovement);
    }

    private void Jump()
    {
        if (!Input.GetKeyDown(jumpKey)) return;

        CmdJump();
    }

    private void MouseMovement()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        xRotation -= mouseY * mouseSensitivity;
        yRotation += mouseX * mouseSensitivity;

        xRotation = Mathf.Clamp(xRotation, -maxLook, maxLook);
        cameraHolder.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        CmdMouseMovement(yRotation);
    }

    private void CheckFallPosition()
    {
        CmdCheckFallPosition(gameObject);
    }

    private void OnCanMoveHandler(bool canMove)
    {
        CmdInit(canMove);
    }

    #endregion
}