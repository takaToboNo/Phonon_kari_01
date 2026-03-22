using UnityEngine;

public class SoundAmplifier : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;    // 出口の場所（空のGameObjectを割り当て）
    [SerializeField] private GameObject bigWavePrefab; // 大きい音波のプレハブ

    [Header("強化パラメーター")]
    [SerializeField] private float amplifiedSpeed = 4.0f;    // 強化後の速度
    [SerializeField] private float amplifiedLifeTime = 8.0f; // 強化後の寿命
    [SerializeField] private float amplifiedScale = 2.0f;    // 見た目の大きさ

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 当たったものが音波（SoundWavePlatform）かチェック
        if (collision.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform oldWave))
        {
            // 1. 古い音波を消去する
            Destroy(collision.gameObject);

            // 2. 出口から新しい音波を生成する
            // 出口の向き（exitPoint.up または right）に合わせて発射
            GameObject newWave = Instantiate(bigWavePrefab, exitPoint.position, exitPoint.rotation);

            // 3. パラメーターを強化して初期化
            if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
            {
                // 出口の向いている方向（右方向なら exitPoint.right）に飛ばす
                platform.Initialize(exitPoint.right, amplifiedSpeed, amplifiedLifeTime, amplifiedScale);
            }

            Debug.Log("音を増幅しました！");
        }
    }
}