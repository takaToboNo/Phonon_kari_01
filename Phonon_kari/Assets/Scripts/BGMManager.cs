using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;

    [Header("このBGMを維持するシーン名（例: Stage1）")]
    [SerializeField] private string sceneGroupName;

    private void Awake()
    {
        // シングルトン設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // シーンが読み込まれた時に呼ばれるイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // すでに別のBGMManagerがいる場合
            // 同じ「グループ」のシーンなら自分を消して古い方を残す
            // 違うグループ（新しいステージ）なら古い方を消して自分を優先する
            if (instance.sceneGroupName != this.sceneGroupName)
            {
                Destroy(instance.gameObject);
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // もし「全く関係ないシーン（メニュー画面など）」に行った時に消したいならここで判定
        // if (scene.name == "MainMenu") Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // イベント登録を解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}