using UnityEngine;

public class VibrationGimmick : MonoBehaviour
{
    [Header("音波に反応して壊したいオブジェクト")]
    [SerializeField] private GameObject targetObject;

    [Header("判定の設定")]
    [SerializeField] private string soundWaveLayerName = "SoundWave"; // レイヤー名

    // 衝突判定（物理）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckLayerAndDestroy(collision.gameObject);
    }

    // トリガー判定
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckLayerAndDestroy(other.gameObject);
    }

    private void CheckLayerAndDestroy(GameObject hitObject)
    {
        // NameToLayerで名前からレイヤー番号を取得し、当たったオブジェクトのレイヤーと比較
        if (hitObject.layer == LayerMask.NameToLayer(soundWaveLayerName))
        {
            if (targetObject != null)
            {
                Debug.Log($"{targetObject.name} をレイヤー判定により破壊しました！");
                Destroy(targetObject);
            }
        }
    }
}