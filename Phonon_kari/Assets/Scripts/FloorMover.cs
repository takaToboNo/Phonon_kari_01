using UnityEngine;

/// <summary>
/// 2Dオブジェクトを平行移動させるシンプルなスクリプト。
/// 配置した位置を中心に、指定した幅・速度で往復移動します。
/// </summary>
public class FloorMover : MonoBehaviour
{
    // =====================================================================
    //  移動設定
    // =====================================================================
    [Header("── 移動設定 ───────────────────")]

    [Tooltip("X軸方向の移動幅（片側の最大値）")]
    public float moveWidthX = 1f;

    [Tooltip("Y軸方向の移動幅（片側の最大値）")]
    public float moveWidthY = 0f;

    [Tooltip("移動速度（1秒あたりの往復周期数）")]
    [Min(0f)]
    public float moveSpeed = 1f;

    // =====================================================================
    //  内部状態
    // =====================================================================
    private float _phase = 0f;
    private Vector3 _startLocalPosition;

    // =====================================================================
    //  初期化
    // =====================================================================
    void Start()
    {
        _startLocalPosition = transform.localPosition;
    }

    // =====================================================================
    //  毎フレーム更新
    // =====================================================================
    void Update()
    {
        _phase += moveSpeed * Time.deltaTime * Mathf.PI * 2f;

        float wave = Mathf.Sin(_phase);
        Vector3 localOffset = new Vector3(moveWidthX * wave, moveWidthY * wave, 0f);
        transform.localPosition = _startLocalPosition + localOffset;
    }

    // =====================================================================
    //  外部制御 API
    // =====================================================================
    public void ResetStartPosition()
    {
        _startLocalPosition = transform.localPosition;
        _phase = 0f;
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }
}
