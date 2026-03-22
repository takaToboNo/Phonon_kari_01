using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GoalFlag : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private GameObject clearUI;
    [SerializeField] private string nextSceneName;

    [Header("音の設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clearSound; // クリア時の音
    [Range(0f, 0.3f)][SerializeField] private float pitchRandomness = 0.05f;

    private bool isCleared = false; // 二重発動防止

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // すでにクリア済みなら何もしない
        if (isCleared) return;

        if (collision.CompareTag("Player"))
        {
            isCleared = true;

            // --- クリア音を鳴らす ---
            PlayClearSE();

            ShowClearMenu();
        }
    }

    private void PlayClearSE()
    {
        if (clearSound == null || audioSource == null) return;

        // クリア音はあまりピッチを変えすぎないのがコツ（0.05くらい）
        audioSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);
        audioSource.PlayOneShot(clearSound);
    }

    private void ShowClearMenu()
    {
        clearUI.SetActive(true);

        // ゲームを一時停止
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- ボタンから呼び出す関数 ---

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }
}