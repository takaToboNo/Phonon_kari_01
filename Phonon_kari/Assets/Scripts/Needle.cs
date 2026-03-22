using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Needle : MonoBehaviour
{
    [Header("SE設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound; // 針に当たった時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.1f;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 勝手に鳴らないように設定
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // --- SEを鳴らす ---
            PlayDeathSE();

            // GameManagerに「プレイヤーが死んだよ」と伝える
            // ※GameManager側の変数が Instance か instance か、自分のコードに合わせて確認してください
            GameManager.instance.PlayerDied(collision.gameObject);
        }
    }

    private void PlayDeathSE()
    {
        if (deathSound == null || audioSource == null) return;

        // ピッチをランダム化して再生
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(deathSound);
    }
}