using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("基本移動")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 12.0f;

    [Header("接地判定 (BoxCast)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private float groundCheckOffset = -0.1f;
    [SerializeField] private float maxSlopeAngle = 45f;

    [Header("操作感の調整")]
    [SerializeField] private float coyoteTime = 0.15f;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private PlayerGrab playerGrab;

    private float horizontalInput;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float colliderHalfHeight;

    // --- 足場追従用の変数 ---
    private Rigidbody2D movingPlatformRb;
    private Vector2 platformVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        playerGrab = GetComponent<PlayerGrab>();
        colliderHalfHeight = col.size.y / 2f;

        // 回転を物理で変えられないように固定（念のため）
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        HandleJump();
    }

    private void HandleInput()
    {
        if (Time.timeScale == 0f) { horizontalInput = 0f; return; }
        horizontalInput = 0f;
        if (playerGrab != null && playerGrab.IsAiming) return;

        if (Gamepad.current != null)
        {
            float stickInput = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickInput) > 0.1f) horizontalInput = stickInput;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;
            else if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        }
    }

    private void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0, -colliderHalfHeight + groundCheckOffset);
        RaycastHit2D hit = Physics2D.BoxCast(origin, groundCheckSize, 0f, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle <= maxSlopeAngle)
            {
                isGrounded = true;
                coyoteTimeCounter = coyoteTime;

                // 足場のRigidbody2Dを取得（動く足場用）
                movingPlatformRb = hit.collider.GetComponent<Rigidbody2D>();
            }
            else
            {
                isGrounded = false;
                movingPlatformRb = null;
            }
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
            movingPlatformRb = null;
        }
    }

    private void HandleJump()
    {
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressed = true;

        if (jumpPressed && coyoteTimeCounter > 0f)
        {
            coyoteTimeCounter = 0f;
            // ジャンプ時は足場の速度にジャンプ力を足す
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        Vector2 currentVel = rb.linearVelocity;
        Vector2 platformVel = Vector2.zero;

        // 音波が物理的に動いていれば、ここで速度(linearVelocity)が取得できる
        if (movingPlatformRb != null)
        {
            platformVel = movingPlatformRb.linearVelocity;
        }

        float targetRelativePosX = horizontalInput * moveSpeed;
        float currentRelativePosX = currentVel.x - platformVel.x;

        // 加速感（10fの数値）はお好みで調整してください
        float newRelativePosX = Mathf.MoveTowards(currentRelativePosX, targetRelativePosX, moveSpeed * 20f * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newRelativePosX + platformVel.x, currentVel.y);
    }

    // --- SetParentを使わない方式にしたため、OnCollision系は削除してOK ---
}