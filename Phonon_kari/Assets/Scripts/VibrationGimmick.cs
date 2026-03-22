using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class VibrationGimmick : MonoBehaviour
{
    [Header("音波に反応して壊したいオブジェクト")]
    [SerializeField] private GameObject targetObject;

    [Header("判定の設定")]
    [SerializeField] private string soundWaveLayerName = "SoundWave";

    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip destroySound; // 壊れる時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    [Header("演出設定 (壊れる直前の震え)")]
    [SerializeField] private float punchScale = 1.2f;      // 一瞬どれくらい大きくするか
    [SerializeField] private float punchDuration = 0.05f;   // 大きくなる時間
    [SerializeField] private float delayBeforeDestroy = 0.05f; // 演出から破壊までのわずかなラグ

    private Vector3 originalScale;
    private bool isDestroyed = false; // 二重発動防止

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        originalScale = transform.localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckLayerAndDestroy(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckLayerAndDestroy(other.gameObject);
    }

    private void CheckLayerAndDestroy(GameObject hitObject)
    {
        if (isDestroyed) return;

        if (hitObject.layer == LayerMask.NameToLayer(soundWaveLayerName))
        {
            if (targetObject != null)
            {
                isDestroyed = true; // フラグを立てる
                StartCoroutine(DestroySequence());
            }
        }
    }

    private IEnumerator DestroySequence()
    {
        // 1. SEを鳴らす
        PlayDestroySE();

        // 2. 演出（一瞬ピクッと大きくする）
        float timer = 0f;
        Vector3 targetScale = originalScale * punchScale;
        while (timer < punchDuration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / punchDuration);
            yield return null;
        }

        // 3. ほんの少し待機（音が鳴り始めた瞬間に消えるより、少しラグがある方が自然）
        yield return new WaitForSeconds(delayBeforeDestroy);

        // 4. オブジェクトを破壊
        Debug.Log($"{targetObject.name} を破壊しました！");
        Destroy(targetObject);
    }

    private void PlayDestroySE()
    {
        if (destroySound == null || audioSource == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(destroySound);
    }
}