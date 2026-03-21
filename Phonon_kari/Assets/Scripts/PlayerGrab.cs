using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerGrab : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Vector2 grabOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float maxGrabDistance = 2.0f; // 念のための最大到達距離

    private GameObject grabbedObject;
    private Rigidbody2D grabbedRb;
    private List<GameObject> canGrabItems = new List<GameObject>();

    private bool isAiming = false;
    private Vector2 aimDirection = Vector2.right;
    public bool IsAiming => isAiming;

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        if (grabbedObject != null)
        {
            FollowPlayer();
        }
    }

    private void HandleInput()
    {
        if (Gamepad.current == null && Keyboard.current == null) return;

        // --- 掴む処理の修正 ---
        bool grabPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        if (grabPressed && grabbedObject == null)
        {
            TryGrabNearestItem();
        }

        // --- 投げる・離す処理 ---
        bool dropButtonHeld = (Gamepad.current != null && Gamepad.current.buttonEast.isPressed) ||
                              (Mouse.current != null && Mouse.current.rightButton.isPressed);

        bool dropButtonReleased = (Gamepad.current != null && Gamepad.current.buttonEast.wasReleasedThisFrame) ||
                                  (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame);

        if (grabbedObject != null)
        {
            if (dropButtonHeld)
            {
                isAiming = true;
                UpdateAimDirection();
            }

            if (dropButtonReleased)
            {
                ThrowItem();
                isAiming = false;
            }
        }
    }

    private void TryGrabNearestItem()
    {
        // リストの中から「本当に今近くにあるもの」だけを探す
        canGrabItems.RemoveAll(item => item == null); // 削除済みオブジェクトを掃除

        GameObject nearest = null;
        float minDist = maxGrabDistance;

        foreach (var item in canGrabItems)
        {
            float dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = item;
            }
        }

        if (nearest != null)
        {
            GrabItem(nearest);
        }
    }

    private void GrabItem(GameObject target)
    {
        grabbedObject = target;
        grabbedObject.TryGetComponent(out grabbedRb);

        if (grabbedRb != null)
        {
            grabbedRb.simulated = false;
            // 掴んだ瞬間に速度を完全にゼロにする（持ち越し防止）
            grabbedRb.linearVelocity = Vector2.zero;
            grabbedRb.angularVelocity = 0f;
        }
    }

    private void ThrowItem()
    {
        GameObject itemToThrow = grabbedObject;
        Rigidbody2D rbToThrow = grabbedRb;

        grabbedObject = null;
        grabbedRb = null;

        if (rbToThrow != null)
        {
            rbToThrow.simulated = true;

            // 【重要】投げる直前に現在の速度をリセットする
            // これをしないと、プレイヤーの移動速度や前回の投げの慣性が乗って加速し続けます
            rbToThrow.linearVelocity = Vector2.zero;
            rbToThrow.angularVelocity = 0f;

            // 新たに一定の力を加える
            rbToThrow.AddForce(aimDirection * throwForce, ForceMode2D.Impulse);
        }
    }

    private void UpdateAimDirection()
    {
        if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().magnitude > 0.1f)
        {
            aimDirection = Gamepad.current.leftStick.ReadValue().normalized;
        }
        else if (Mouse.current != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mousePos.z = 0;
            aimDirection = ((Vector2)mousePos - (Vector2)transform.position).normalized;
        }
    }

    private void FollowPlayer()
    {
        Vector2 currentOffset = grabOffset;
        currentOffset.x *= Mathf.Sign(transform.localScale.x);
        Vector2 targetPosition = (Vector2)transform.position + currentOffset;
        grabbedObject.transform.position = Vector2.Lerp(grabbedObject.transform.position, targetPosition, smoothSpeed * Time.fixedDeltaTime);
    }

    // --- 範囲判定のリスト管理を厳格化 ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & itemLayer) != 0)
        {
            if (!canGrabItems.Contains(collision.gameObject))
                canGrabItems.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (canGrabItems.Contains(collision.gameObject))
        {
            canGrabItems.Remove(collision.gameObject);
        }
    }
}