using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SoundWavePlatform : MonoBehaviour
{
    [Header("動きの設定")]
    [SerializeField] private float moveSpeed = 2.0f;
    private Vector2 moveDirection;

    [Header("特性レイヤーの設定")]
    [SerializeField] private LayerMask absorptionLayer;  // 吸収
    [SerializeField] private LayerMask penetrationLayer; // 透過
    [SerializeField] private LayerMask reflectionLayer;   // 反射

    [Header("透過の設定")]
    [SerializeField] private float maxWallCheckDistance = 2.0f;

    [Header("寿命・演出")]
    private float lifeTime = 3.0f;
    [SerializeField] private float fadeDuration = 0.8f;

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isInitialized = false;
    private bool isPenetrating = false;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        // 物理設定の強制適用
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.useFullKinematicContacts = true; // プレイヤーへの速度伝達を正確にする

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    public void Initialize(Vector2 direction, float speed, float customLifeTime, float scaleMultiplier = 1.0f)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        this.lifeTime = customLifeTime;

        isInitialized = true;
        transform.localScale = originalScale * scaleMultiplier;

        // 初期方向への回転と速度設定
        UpdateDirection(moveDirection);

        StartCoroutine(DestroyAfterTime());
    }

    void FixedUpdate()
    {
        if (!isInitialized || isPenetrating) return;

        // transform.Translate ではなく Velocity で動かす
        // これによりプレイヤーの移動スクリプトが「足場の速度」を検知できるようになる
        rb2d.linearVelocity = moveDirection * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int layer = collision.gameObject.layer;

        // 1. 吸収
        if (((1 << layer) & absorptionLayer) != 0)
        {
            Destroy(gameObject);
            return;
        }

        // 2. 透過
        if (((1 << layer) & penetrationLayer) != 0 && !isPenetrating)
        {
            StartCoroutine(PenetrateRoutine(collision));
            return;
        }

        // 3. 反射
        if (((1 << layer) & reflectionLayer) != 0)
        {
            ColliderDistance2D dist = collision.Distance(GetComponent<Collider2D>());
            if (dist.isValid)
            {
                Vector2 reflectDir = Vector2.Reflect(moveDirection, dist.normal);
                UpdateDirection(reflectDir);

                // めり込み防止
                rb2d.position = dist.pointA + dist.normal * 0.1f;
            }
        }
    }

    private void UpdateDirection(Vector2 newDir)
    {
        moveDirection = newDir.normalized;

        // 見た目の回転
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        rb2d.rotation = angle - 90f;

        // 物理速度を即座に更新
        rb2d.linearVelocity = moveDirection * moveSpeed;
    }

    private IEnumerator PenetrateRoutine(Collider2D wall)
    {
        isPenetrating = true;
        rb2d.linearVelocity = Vector2.zero; // 透過中は物理移動を止める

        Vector2 startPoint = (Vector2)transform.position + moveDirection * 0.1f;
        float checkDistance = 0.1f;
        bool foundExit = false;
        Vector3 exitPosition = transform.position;

        while (checkDistance < maxWallCheckDistance)
        {
            Vector2 testPoint = startPoint + (moveDirection * checkDistance);
            if (!Physics2D.OverlapPoint(testPoint, penetrationLayer))
            {
                exitPosition = testPoint + (moveDirection * 0.3f);
                foundExit = true;
                break;
            }
            checkDistance += 0.1f;
        }

        if (foundExit) transform.position = exitPosition;

        yield return new WaitForSeconds(0.1f);
        isPenetrating = false;
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(Mathf.Max(0, lifeTime - fadeDuration));
        if (spriteRenderer != null)
        {
            float timer = 0;
            Color startColor = spriteRenderer.color;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - (timer / fadeDuration));
                yield return null;
            }
        }
        Destroy(gameObject);
    }
}