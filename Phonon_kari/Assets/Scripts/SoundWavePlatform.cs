using UnityEngine;
using System.Collections;

public class SoundWavePlatform : MonoBehaviour
{
    [Header("動きの設定")]
    [SerializeField] private float moveSpeed = 2.0f;
    private Vector2 moveDirection;

    [Header("特性レイヤーの設定")]
    [SerializeField] private LayerMask absorptionLayer;  // 吸収
    [SerializeField] private LayerMask penetrationLayer; // 透過
    [SerializeField] private LayerMask reflectionLayer;  // 反射

    [Header("透過の設定")]
    [SerializeField] private float maxWallCheckDistance = 2.0f; // 貫通できる最大厚さ

    [Header("寿命・演出")]
    private float lifeTime = 3.0f;
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("初期設定（プレハブの値）")]
    private Vector3 originalScale; // インスペクタで設定された元々の大きさを保存

    private SpriteRenderer spriteRenderer;
    private bool isInitialized = false;
    private bool isPenetrating = false; // 連続ワープ防止用

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Awakeの時点で、インスペクタで設定されているScale（例: 2, 1, 1）を覚えておく
        originalScale = transform.localScale;
    }

    public void Initialize(Vector2 direction, float speed, float customLifeTime, float scaleMultiplier = 1.0f)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;

        // 引数で受け取った寿命をセットする
        this.lifeTime = customLifeTime;

        // 向きの調整
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        isInitialized = true;

        transform.localScale = originalScale * scaleMultiplier;

        StartCoroutine(DestroyAfterTime());
    }

    void Update()
    {
        if (!isInitialized) return;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
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

    // 2. 反射（ここを修正！）
    if (((1 << layer) & reflectionLayer) != 0)
    {
        // Raycastではなく、衝突したコライダーの「一番近い点」から法線を割り出す
        // 自分の中心点から、相手のコライダーへの最短距離の情報を取得
        ColliderDistance2D dist = collision.Distance(GetComponent<Collider2D>());
        
        if (dist.isValid)
        {
            // dist.normal が「ぶつかった面の向き（法線）」そのものです
            Vector2 normal = dist.normal;

            // 反射ベクトルを計算
            Vector2 reflectDir = Vector2.Reflect(moveDirection, normal);
            UpdateDirection(reflectDir);

            // 【重要】めり込み防止：壁の表面（dist.pointA）に少しオフセットを足して移動させる
            transform.position = (Vector3)dist.pointA + (Vector3)normal * 0.05f;

            Debug.Log("反射成功（法線を直接取得）: " + reflectDir);
        }
        else
        {
            // 万が一失敗した時の予備（従来のRaycast）
            ReflectWave();
        }
    }
}

    private IEnumerator PenetrateRoutine(Collider2D wall)
    {
        isPenetrating = true;

        // 壁の向こう側を探す。
        // 現在地から進行方向にレイを飛ばして「壁の終わり」を見つける
        // ※ penetrationLayerを指定して、そのレイヤーが途切れる場所を探す
        Vector2 startPoint = (Vector2)transform.position + moveDirection * 0.1f;

        // 少しずつ先をチェックして、透過レイヤーがない場所（出口）を探す
        float checkDistance = 0.1f;
        bool foundExit = false;
        Vector3 exitPosition = transform.position;

        while (checkDistance < maxWallCheckDistance)
        {
            Vector2 testPoint = startPoint + (moveDirection * checkDistance);
            // その地点に透過レイヤーの壁があるか？
            if (!Physics2D.OverlapPoint(testPoint, penetrationLayer))
            {
                // 壁がなくなった地点を発見！
                exitPosition = testPoint + (moveDirection * 0.2f); // 少し余裕を持って外に出す
                foundExit = true;
                break;
            }
            checkDistance += 0.1f;
        }

        if (foundExit)
        {
            transform.position = exitPosition;
            Debug.Log("透過完了！");
        }

        // 連続ワープを防ぐため、少し待ってからフラグを戻す
        yield return new WaitForSeconds(0.1f);
        isPenetrating = false;
    }

    private void ReflectWave()
    {
        // 進行方向に少し長めにレイを飛ばして、確実に壁を捉える
        // 第3引数の距離を 0.5f から 1.0f くらいに伸ばしてみる
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 1.0f, reflectionLayer);

        if (hit.collider != null)
        {
            Vector2 reflectDir = Vector2.Reflect(moveDirection, hit.normal);
            UpdateDirection(reflectDir);

            // 【重要】反射した直後に壁から少し離す（連続衝突でハマるのを防ぐ）
            transform.position = hit.point + hit.normal * 0.1f;

            Debug.Log("反射に成功しました！");
        }
        else
        {
            // もし正面のレイが外れたら、現在地から全方位に小さく飛ばして壁を探す
            Debug.LogWarning("壁に触れたが、法線が見つかりませんでした。レイヤー設定を確認してください。");
        }
    }

    private void UpdateDirection(Vector2 newDir)
    {
        moveDirection = newDir.normalized;
        // デバッグ用の線を出す
        Debug.DrawRay(transform.position, moveDirection * 2f, Color.green, 2f);

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
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