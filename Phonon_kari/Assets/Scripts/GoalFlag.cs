using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要

public class GoalFlag : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private GameObject clearUI; // 先ほど作ったClearCanvasをアタッチ
    [SerializeField] private string nextSceneName; // 次のステージのシーン名

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーが旗に触れたかチェック
        if (collision.CompareTag("Player"))
        {
            ShowClearMenu();
        }
    }

    private void ShowClearMenu()
    {
        // クリア画面を表示
        clearUI.SetActive(true);

        // ゲームを一時停止（動かしたくない場合）
        Time.timeScale = 0f;

        // マウスカーソルを表示する（隠している場合）
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- ボタンから呼び出す関数 ---

    public void NextLevel()
    {
        Time.timeScale = 1f; // 時間を動かす
        SceneManager.LoadScene(nextSceneName);
    }

    public void RestartLevel()
    {
        // 時間を動かす（一時停止していた場合のため）
        Time.timeScale = 1f;

        // 現在アクティブなシーンの名前を取得して、それをロードする
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void BackToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene"); // タイトルシーンの名前に合わせて変更
    }
}