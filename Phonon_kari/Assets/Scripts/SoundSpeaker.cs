using UnityEngine;

public class SoundSpeaker : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;    // 出口の場所
    [SerializeField] private GameObject wavePrefab; // 音波のプレハブ

    [Header("生成の設定")]
    [SerializeField] private float interval = 2.0f;  // 何秒おきに生成するか
    [SerializeField] private float startDelay = 0f;  // ゲーム開始から最初の生成までの遅延

    [Header("音波のパラメーター")]
    [SerializeField] private float waveSpeed = 2.0f;
    [SerializeField] private float waveLifeTime = 3.0f;
    [SerializeField] private float waveScale = 1.0f;

    void Start()
    {
        // 指定した間隔（interval）で EmitWave 関数を繰り返し実行する
        InvokeRepeating(nameof(EmitWave), startDelay, interval);
    }

    private void EmitWave()
    {
        if (wavePrefab == null || exitPoint == null) return;

        // 音波を生成
        GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

        // 初期化（これまでの SoundWavePlatform の仕様に合わせる）
        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            // exitPoint の右方向に発射
            platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
        }

        Debug.Log("スピーカーから音波が射出されました");
    }

    // スピーカーが無効になったら止める（念のため）
    void OnDisable()
    {
        CancelInvoke(nameof(EmitWave));
    }
}