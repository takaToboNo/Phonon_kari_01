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
    [SerializeField] private AudioClip vibrateSound; 
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    [Header("演出設定 (オブジェクト自体を大きくする)")]
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float punchDuration = 0.05f;
    [SerializeField] private float returnDuration = 0.15f;

    private float lastVibratedTime = -100f; // 初期値を十分小さなマイナスにする
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        // 出した瞬間、前のクールタイムを引きずらないように過去の時間に設定
        lastVibratedTime = Time.time - coolTime;
    }

    void OnDisable()
    {
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
            punchCoroutine = null;
        }
        transform.localScale = originalScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // クールタイム判定
        if (Time.time >= lastVibratedTime + coolTime)
        {
            // Tagチェック（"SoundWave"など）を追加するとより確実です
            if (collision.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform oldWave))
            {
                lastVibratedTime = Time.time;

                // 古い音波を消去
                Destroy(collision.gameObject);

                // 新しい音波を生成
                Vibrate();
            }
        }
    }

    private void Vibrate()
    {
        if (wavePrefab == null || exitPoint == null) return;

        GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
        }

        PlayVibrateSE();

        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchEffectRoutine());
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
}