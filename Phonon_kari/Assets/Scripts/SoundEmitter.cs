using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private GameObject soundWavePrefab;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float minVelocity = 5.0f;

    [Header("音波の設定")]
    [SerializeField] private float waveSpeed = 2.0f;
    [SerializeField] private float waveLifeTime = 3.0f;
    [SerializeField] private float waveScale = 1.0f;

    [Header("音の設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip emitSound; // 音波が出た時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // レイヤーチェック
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // 衝突の勢いが一定以上かチェック
            if (collision.relativeVelocity.magnitude > minVelocity)
            {
                EmitWave(collision);
            }
        }
    }

    private void EmitWave(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];

        // 音波の生成
        GameObject wave = Instantiate(soundWavePrefab, contact.point, Quaternion.identity);

        if (wave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(contact.normal, waveSpeed, waveLifeTime, waveScale);
        }

        // --- SEを鳴らす ---
        PlayEmitSE();
    }

    private void PlayEmitSE()
    {
        if (emitSound == null || audioSource == null) return;

        // ピッチをランダム化して、連続発生時の機械的な感じをなくす
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(emitSound);
    }
}