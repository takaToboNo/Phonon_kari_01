using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundAmplifier : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject bigWavePrefab;

    [Header("強化パラメーター")]
    [SerializeField] private float amplifiedSpeed = 4.0f;
    [SerializeField] private float amplifiedLifeTime = 8.0f;
    [SerializeField] private float amplifiedScale = 2.0f;

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip amplifySound; // 増幅した時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.05f;

    [Header("演出設定 (オブジェクト自体を大きくする)")]
    [SerializeField] private float punchScale = 1.4f;      // 増幅なのでかなり大きく(倍率)
    [SerializeField] private float punchDuration = 0.06f;
    [SerializeField] private float returnDuration = 0.2f;

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
        if (collision.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform oldWave))
        {
            // 1. 古い音波を消去
            Destroy(collision.gameObject);

            // 2. 増幅処理を実行
            Amplify();
        }
    }

    private void Amplify()
    {
        // 3. 新しい音波を生成
        GameObject newWave = Instantiate(bigWavePrefab, exitPoint.position, exitPoint.rotation);

        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(exitPoint.right, amplifiedSpeed, amplifiedLifeTime, amplifiedScale);
        }

        // --- SEを鳴らす ---
        PlayAmplifySE();

        // --- 演出（ドクン！と大きく動く） ---
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchEffectRoutine());

        Debug.Log("音を増幅しました！");
    }

    private void PlayAmplifySE()
    {
        if (amplifySound == null || audioSource == null) return;

        // 増幅音は少し重厚感を出したいので、ピッチのランダム幅は狭め(0.05)がおすすめ
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(amplifySound);
    }

    private IEnumerator PunchEffectRoutine()
    {
        float timer = 0f;
        Vector3 targetScale = originalScale * punchScale;

        // 大きくなる
        while (timer < punchDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / punchDuration);
            yield return null;
        }
        transform.localScale = targetScale;

        // ゆっくり元に戻る（余韻を長めに）
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