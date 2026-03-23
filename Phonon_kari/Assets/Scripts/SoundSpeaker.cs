using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
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

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip emitSound; // 音波生成時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.05f;

    [Header("演出設定 (オブジェクト自体を大きくする)")]
    [SerializeField] private float punchScale = 1.2f;      // どれくらい大きくするか(倍率)
    [SerializeField] private float punchDuration = 0.05f;   // 大きくなるまでの時間 (短めが気持ちいい)
    [SerializeField] private float returnDuration = 0.1f;    // 元に戻るまでの時間

    private Vector3 originalScale; // 元々の大きさを保存
    private Coroutine punchCoroutine; // コルーチンの二重動作防止用

    void Awake()
    {
        // AudioSourceの取得と設定
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // このオブジェクト自体の初期Scale（大きさ）を覚えておく
        originalScale = transform.localScale;
    }

    // オブジェクトが有効になるたび（シーン開始時やインベントリから出した時）に呼ばれる
    void OnEnable()
    {
        // 既存の生成予約を念のためクリア
        CancelInvoke(nameof(EmitWave));
        // 音波の生成を再開
        InvokeRepeating(nameof(EmitWave), startDelay, interval);
    }

    // オブジェクトが無効になった時（インベントリにしまった時）に呼ばれる
    void OnDisable()
    {
        // 生成を停止
        CancelInvoke(nameof(EmitWave));

        // 演出用コルーチンが動いていたら止めてScaleを戻す
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }
        transform.localScale = originalScale;
    }

    private void EmitWave()
    {
        if (wavePrefab == null || exitPoint == null) return;

        // 音波を生成
        GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

        // 初期化
        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
        }

        // SEを鳴らす
        PlayEmitSE();

        // 演出（オブジェクト自体をピクッとする）
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchEffectRoutine());

        Debug.Log("スピーカーから音波が射出されました");
    }

    private void PlayEmitSE()
    {
        if (emitSound == null || audioSource == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(emitSound);
    }

    private IEnumerator PunchEffectRoutine()
    {
        float timer = 0f;
        Vector3 targetScale = originalScale * punchScale;

        // 1. 大きくなる
        while (timer < punchDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / punchDuration);
            yield return null;
        }
        transform.localScale = targetScale;

        // 2. 元に戻る
        timer = 0f;
        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / returnDuration);
            yield return null;
        }
        transform.localScale = originalScale;

        punchCoroutine = null;
    }
}