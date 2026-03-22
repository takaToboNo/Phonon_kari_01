using UnityEngine;

public class SoundVibration : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject wavePrefab;

    [Header("音波の設定")]
    [SerializeField] private float waveSpeed = 2.0f;
    [SerializeField] private float waveLifeTime = 3.0f;
    [SerializeField] private float coolTime = 0.5f; // クールタイムを追加
    [SerializeField] private float waveScale = 1.0f;

    private float lastVibratedTime; // 最後に共振した時間

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. クールタイム中なら何もしない
        if (Time.time > lastVibratedTime + coolTime)
        {
            if (collision.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform oldWave))
            {
                // 2. 共振時間を更新
                lastVibratedTime = Time.time;

                // 3. 古い音波を消去
                Destroy(collision.gameObject);

                // 4. 新しい音波を生成
                GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

                if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
                {
                    platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
                }

                Debug.Log("共振しました！");
            }
        }
    }
}