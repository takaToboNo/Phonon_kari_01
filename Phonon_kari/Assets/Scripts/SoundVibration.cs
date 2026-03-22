using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundVibration : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject wavePrefab;

    [Header("音波の設定")]
    [SerializeField] private float waveSpeed = 2.0f;
    [SerializeField] private float waveLifeTime = 3.0f;
    [SerializeField] private float coolTime = 0.5f;
    [SerializeField] private float waveScale = 1.0f;

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip vibrateSound; // 共振した時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    [Header("演出設定 (オブジェクト自体を大きくする)")]
    [SerializeField] private float punchScale = 1.3f;      // 共振は少し大きめに(倍率)
    [SerializeField] private float punchDuration = 0.05f;
    [SerializeField] private float returnDuration = 0.15f;

    private float lastVibratedTime;
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Time.time > lastVibratedTime + coolTime)
        {
            if (collision.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform oldWave))
            {
                lastVibratedTime = Time.time;

                // 1. 古い音波を消去
                Destroy(collision.gameObject);

                // 2. 新しい音波を生成
                Vibrate();
            }
        }
    }

    private void Vibrate()
    {
        // 音波生成
        GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
        }

        // --- SEを鳴らす ---
        PlayVibrateSE();

        // --- 演出（ピクッとする） ---
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchEffectRoutine());

        Debug.Log("共振しました！");
    }

    private void PlayVibrateSE()
    {
        if (vibrateSound == null || audioSource == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(vibrateSound);
    }

    private IEnumerator PunchEffectRoutine()
    {
        float timer = 0f;
        Vector3 targetScale = originalScale * punchScale;

        while (timer < punchDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / punchDuration);
            yield return null;
        }
        transform.localScale = targetScale;

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

    void OnDisable()
    {
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        transform.localScale = originalScale;
    }
}