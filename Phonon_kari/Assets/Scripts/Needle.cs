using UnityEngine;

public class Needle : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // GameManagerに「プレイヤーが死んだよ」と伝える
            GameManager.instance.PlayerDied(collision.gameObject);
        }
    }
}