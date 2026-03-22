using UnityEngine;
using UnityEngine.SceneManagement; // ステージリセット用

public class GameManager : MonoBehaviour
{
    // どこからでも GameManager.Instance で呼べるようにする（シングルトン）
    public static GameManager instance;

    private void Awake()
    {
        // 二重生成を防ぐ設定
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // プレイヤーが死んだときに呼ばれる関数
    public void PlayerDied(GameObject player)
    {
        // 現在アクティブなシーンの名前を取得してロードし直す
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        Debug.Log("シーンをリセットしました: " + currentSceneName);
    }
}