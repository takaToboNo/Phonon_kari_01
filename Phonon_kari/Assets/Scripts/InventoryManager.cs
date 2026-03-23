using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("アイテム画像を表示する子要素のImageを指定")]
    [SerializeField] private Image itemIconDisplay;

    [Tooltip("アイテムがない時の色（透明度0推奨）")]
    [SerializeField] private Color emptyColor = new Color(1, 1, 1, 0);

    [Tooltip("アイテムがある時の色（白）")]
    [SerializeField] private Color hasItemColor = Color.white;

    [Header("保存データ（確認用）")]
    [SerializeField] private GameObject storedPrefab;
    private Sprite storedSprite;
    private Quaternion storedRotation;

    private RectTransform iconRectTransform;

    public bool HasItem => storedSprite != null;

    void Awake()
    {
        if (itemIconDisplay != null)
        {
            // RectTransformを取得
            iconRectTransform = itemIconDisplay.GetComponent<RectTransform>();

            // 重要：比率がおかしくならないようプログラムからも念のため設定
            itemIconDisplay.type = Image.Type.Simple;
            itemIconDisplay.preserveAspect = true;
        }
        UpdateUI();
    }

    /// <summary>
    /// アイテムの入れ替え処理
    /// </summary>
    public void SwapItem(GameObject currentHandItem, out GameObject outPrefab, out Quaternion outRot)
    {
        // 現在インベントリにあるものを出力（手に戻す用）
        outPrefab = storedPrefab;
        outRot = storedRotation;

        // 新しく手に持っていたものをインベントリに保存
        if (currentHandItem != null)
        {
            if (currentHandItem.TryGetComponent(out SpriteRenderer sr))
            {
                storedSprite = sr.sprite;
            }

            storedRotation = currentHandItem.transform.rotation;
            storedPrefab = currentHandItem;
        }
        else
        {
            // 手が空だった場合はインベントリを空にする準備
            storedSprite = null;
            storedPrefab = null;
            storedRotation = Quaternion.identity;
        }

        UpdateUI();
    }

    /// <summary>
    /// UIの見た目を最新の状態に更新する
    /// </summary>
    private void UpdateUI()
    {
        if (itemIconDisplay == null) return;

        if (HasItem)
        {
            itemIconDisplay.sprite = storedSprite;

            // 角度を反映（RectTransform経由）
            if (iconRectTransform != null)
            {
                iconRectTransform.localRotation = storedRotation;
            }

            itemIconDisplay.color = hasItemColor;
        }
        else
        {
            itemIconDisplay.sprite = null;
            itemIconDisplay.color = emptyColor;
        }
    }
}