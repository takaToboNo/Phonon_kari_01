using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("基本の設定")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 10.0f;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("接地判定の設定")]
    [SerializeField] private float groundCheckDistance = 0.2f; // 足元から線を飛ばす距離
    [SerializeField] private float maxSlopeAngle = 45f;      // 地面とみなす最大角度（45度以上は壁）

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float horizontalInput;
    private bool isGrounded;
    private float colliderHalfHeight;
    private PlayerGrab playerGrab;

    void Start()
    {
        playerGrab = GetComponent<PlayerGrab>(); // 参照を取得

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        // 常に最新のコライダーサイズから判定位置を計算できるようにします
        colliderHalfHeight = col.bounds.extents.y;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        HandleJump();
    }

    private void HandleInput()
    {
        // クリア画面（Time.timeScale = 0）のときは入力を受け付けない
        if (Time.timeScale == 0f) return;

        horizontalInput = 0f;

        // エイム中は移動入力を受け付けない
        if (playerGrab != null && playerGrab.IsAiming) return;

        // 1. コントローラーの入力をチェック
        if (Gamepad.current != null)
        {
            float stickInput = Gamepad.current.leftStick.x.ReadValue();
            // スティックが一定以上倒れている場合のみ入力を上書き
            if (Mathf.Abs(stickInput) > 0.1f)
            {
                horizontalInput = stickInput;
            }
        }

        // 2. コントローラーの入力がない場合、キーボードを確認
        if (horizontalInput == 0f && Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) horizontalInput = 1f;
            else if (Keyboard.current.aKey.isPressed) horizontalInput = -1f;
        }
    }

    private void CheckGround()
    {
        // 足元から真下にレイ（線）を飛ばす
        // colliderHalfHeight は Start で計算済みのものを使用
        Vector2 origin = (Vector2)transform.position + Vector2.down * (colliderHalfHeight - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);

        // デバッグ用の線（Scene画面で見えます）
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);

        if (hit.collider != null)
        {
            // 当たった面の法線（垂直なベクトル）から角度を計算
            // Vector2.up（真上）との角度差を出す
            float angle = Vector2.Angle(hit.normal, Vector2.up);

            // 角度が設定値以下なら地面とみなす
            if (angle <= maxSlopeAngle)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false; // 角度が急すぎる（壁）
            }
        }
        else
        {
            isGrounded = false; // 何も当たっていない
        }
    }

    private void HandleJump()
    {
        // キーボードまたはコントローラーのジャンプボタンが押されたか
        bool jumpPressed = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            jumpPressed = true;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // 移動速度の適用
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}