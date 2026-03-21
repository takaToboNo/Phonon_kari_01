using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替え用

public class TitleManager : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private string firstStageName = "Stage1"; // 最初のステージ名

    // スタートボタンから呼び出す
    public void StartGame()
    {
        // 最初のステージをロード
        SceneManager.LoadScene(firstStageName);
    }

    // 終了ボタンから呼び出す
    public void ExitGame()
    {
        // ゲームを終了する
        Debug.Log("ゲームを終了します"); // エディタ上での確認用

#if UNITY_EDITOR
        // Unityエディタ実行中の場合は再生を停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ビルドした実機（PC等）ではアプリケーションを終了
            Application.Quit();
#endif
    }
}