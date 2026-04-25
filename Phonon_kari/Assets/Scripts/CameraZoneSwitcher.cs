using UnityEngine;
using Unity.Cinemachine; // Unity 6 用

public class CameraZoneSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera fixedCamera; // ステップ1で作った固定カメラ
    [SerializeField] private int activePriority = 20;       // エリア内での優先度
    [SerializeField] private int inactivePriority = 5;      // エリア外での優先度

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが入ったら、固定カメラの優先度をメイン(10)より高くする
        if (other.CompareTag("Player"))
        {
            fixedCamera.Priority = activePriority;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーが出たら、優先度を下げて通常カメラ(10)に戻す
        if (other.CompareTag("Player"))
        {
            fixedCamera.Priority = inactivePriority;
        }
    }
}