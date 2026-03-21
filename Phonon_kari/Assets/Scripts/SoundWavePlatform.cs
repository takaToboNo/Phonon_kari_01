using UnityEngine;
using System.Collections;

// このコンポーネントにはBoxCollider2Dが必要であることを保証する
[RequireComponent(typeof(BoxCollider2D))]
public class SoundWavePlatform : MonoBehaviour
{
    [Header("動きの設定")]
    [SerializeField] private float moveSpeed = 2.0f;    // 移動スピード

    [Header("寿命の設定")]
    [SerializeField] private float lifeTime = 3.0f;     // 消えるまでの時間
    [SerializeField] private float fadeDuration = 0.8f; // 消える時のフェード時間

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D platformCollider;
    private Vector2 moveDirection = Vector2.up; // デフォルトの移動方向
    private bool isInitialized = false;

    void Awake()
    {
        // 自身のアリは子要素からコンポーネントを取得
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        platformCollider = GetComponent<BoxCollider2D>();

        // アイテム（Itemレイヤー）との衝突をコードで無効化する（推奨）
        // ※レイヤー名が "Item" である必要があります。
        int itemLayer = LayerMask.NameToLayer("Item");
        if (itemLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, itemLayer, true);
        }
    }

    void Start()
    {
        // 生成されたら、消滅タイマーを開始
        StartCoroutine(DestroyAfterTime());
    }

    // 外部（SoundEmitter）から向きと速度を設定するためのメソッド
    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        // オプション：音波の向きを進行方向に向ける（スプライトが上向きの場合）
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        // 設定された方向に、世界座標系で移動させる
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    private IEnumerator DestroyAfterTime()
    {
        // 指定した時間（フェード開始前まで）待つ
        yield return new WaitForSeconds(Mathf.Max(0, lifeTime - fadeDuration));

        // フェードアウト（見た目を徐々に透明にする）
        if (spriteRenderer != null)
        {
            float timer = 0;
            Color startColor = spriteRenderer.color;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeDuration;
                // 透明度（Alpha）を徐々に下げる
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1f - progress);
                yield return null;
            }
        }

        // 最後にオブジェクトを削除
        Destroy(gameObject);
    }
}