using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerGrab : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Vector2 grabOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private float maxGrabDistance = 2.0f;

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip grabSound;
    [SerializeField] private AudioClip throwSound;
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    private GameObject grabbedObject;
    private Rigidbody2D grabbedRb;
    private List<GameObject> canGrabItems = new List<GameObject>();

    private bool isAiming = false;
    private Vector2 aimDirection = Vector2.right;
    public bool IsAiming => isAiming;

    void Awake()
    {
        // AudioSourceの自動取得と初期設定
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

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

        bool grabPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                           (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        if (grabPressed && grabbedObject == null)
        {
            TryGrabNearestItem();
        }

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
        canGrabItems.RemoveAll(item => item == null);

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
            grabbedRb.linearVelocity = Vector2.zero;
            grabbedRb.angularVelocity = 0f;
        }

        // --- 音を鳴らす ---
        PlayRandomPitchSE(grabSound);
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
            rbToThrow.linearVelocity = Vector2.zero;
            rbToThrow.angularVelocity = 0f;
            rbToThrow.AddForce(aimDirection * throwForce, ForceMode2D.Impulse);

            // --- 音を鳴らす ---
            PlayRandomPitchSE(throwSound);
        }
    }

    private void PlayRandomPitchSE(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        // ピッチをランダム化して再生
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(clip);
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