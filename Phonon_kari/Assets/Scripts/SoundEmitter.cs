using UnityEngine;

// このコンポーネントにはRigidbody2Dが必要であることを保証する
[RequireComponent(typeof(Rigidbody2D))]
public class SoundEmitter : MonoBehaviour
{
    [Header("参照設定")]
    [Tooltip("生成する音波のプレハブを割り当ててください")]
    [SerializeField] private GameObject soundWavePrefab;

    [Header("音波の生成条件")]
    [Tooltip("地面として判定するレイヤーを選択してください")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("この速度（勢い）より速くぶつかった時だけ音波を出します")]
    [SerializeField] private float minVelocityToEmit = 5.0f;

    [Header("音波の動き設定")]
    [Tooltip("生成された音波が移動するスピード")]
    [SerializeField] private float waveMoveSpeed = 2.0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. ぶつかった相手が「地面」レイヤーかチェック
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 2. ぶつかった時の「相対速度」の大きさを取得
            float impactVelocity = collision.relativeVelocity.magnitude;

            // 3. 一定以上の強さでぶつかったか判定
            if (impactVelocity > minVelocityToEmit)
            {
                // 衝突情報全体を渡して音波を生成
                EmitSoundWave(collision);
            }
        }
    }

    private void EmitSoundWave(Collision2D collision)
    {
        if (soundWavePrefab == null)
        {
            Debug.LogWarning($"SoundEmitter on {gameObject.name}: SoundWavePrefabが割り当てられていません。");
            return;
        }

        // 最初にぶつかった点（接点）の情報を取得
        ContactPoint2D contact = collision.contacts[0];

        // 壁の垂直方向（法線）を取得。これが進む向きになる。
        Vector2 wallNormal = contact.normal;

        // 音波プレハブを、ぶつかった位置に生成
        GameObject wave = Instantiate(soundWavePrefab, contact.point, Quaternion.identity);

        // 生成した音波のスクリプトを取得し、初期化（向きと速度を渡す）
        if (wave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(wallNormal, waveMoveSpeed);
        }
        else
        {
            Debug.LogError($"SoundEmitter: 生成したプレハブに 'SoundWavePlatform' スクリプトが付いていません。");
        }
    }
}