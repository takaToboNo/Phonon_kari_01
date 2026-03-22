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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        playerGrab = GetComponent<PlayerGrab>();

        // 最新のコライダーサイズから判定位置を計算
        colliderHalfHeight = col.size.y / 2f;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        HandleJump();
    }

    private void HandleInput()
    {
        if (Time.timeScale == 0f)
        {
            horizontalInput = 0f;
            return;
        }

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
            }
            else
            {
                isGrounded = false;
            }
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        bool jumpPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) jumpPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) jumpPressed = true;

        if (jumpPressed && coyoteTimeCounter > 0f)
        {
            // ジャンプした瞬間に親子関係を解消しないと、空中で足場の影響を受けてしまうため解除
            transform.SetParent(null);

            coyoteTimeCounter = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- 動く足場（Groundレイヤー）への対応 ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突相手がGroundレイヤーに含まれているか
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 上から踏んでいる（法線が上向き）場合のみ子になる
            if (collision.contactCount > 0 && collision.contacts[0].normal.y > 0.5f)
            {
                transform.SetParent(collision.transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // 足場から離れたら親子関係を解除
        if (transform.parent == collision.transform)
        {
            transform.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.yellow;
        Vector2 origin = (Vector2)transform.position + new Vector2(0, -(col.size.y / 2f) + groundCheckOffset);
        Gizmos.DrawWireCube(origin, groundCheckSize);
    }
}