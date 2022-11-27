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

    private Vector3 startPosition;
    private Vector3 movementDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private KeyCode jumpKey = KeyCode.Space;
    private bool isGrounded = true;
    private bool canMove = true;

    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    private float maxLook = 60f;

    [SerializeField] private PlayerBlink playerBlink;
    [SerializeField] private CustomNetworkPlayer player;
    private bool isBlinking;

    public Vector3 GetMoveDirection()
    {
        return movementDirection;
    }

    #region Server

    [Command]
    private void CmdMovePlayer(float horizontal, float vertical)
    {
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
        go.transform.position = startPosition;
    }

    #endregion

    #region Client

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

    private void Start()
    {
        SetPlayerCamera();
    }

    private void SetPlayerCamera()
    {
        if (!isLocalPlayer) return;

        Transform cameraTransform = Camera.main.gameObject.transform;
        cameraTransform.parent = cameraHolder.transform;
        cameraTransform.position = cameraHolder.transform.position;
        cameraTransform.rotation = cameraHolder.transform.rotation;
    }

    private void Update()
    {
        if (!isOwned) return;

        Jump();
        MouseMovement();
        CheckFallPosition();
    }

    private void FixedUpdate()
    {
        if (!isOwned) return;

        MovePlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isOwned) return;

        if (!isLocalPlayer) return;

        if (isGrounded) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void MovePlayer()
    {
        if (!canMove) return;

        if (!isLocalPlayer) return;

        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");
        CmdMovePlayer(horizontalMovement, verticalMovement);
    }

    private void Jump()
    {
        if (!isLocalPlayer) return;

        if (!Input.GetKeyDown(jumpKey) || !isGrounded) return;

        CmdJump();
    }

    private void MouseMovement()
    {
        if (!isLocalPlayer) return;
        
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
        if (!isLocalPlayer) return;

        if (transform.position.y < -5)
            CmdCheckFallPosition(gameObject);
    }
    
    private void OnCanMoveHandler(bool canMove)
    {
        if (!isLocalPlayer) return;

        this.canMove = canMove;
    }

    #endregion
}