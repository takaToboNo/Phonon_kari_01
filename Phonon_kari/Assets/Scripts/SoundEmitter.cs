using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private GameObject soundWavePrefab;
    [SerializeField] private LayerMask groundLayer; // ‘S‚Ä‚Ì”½‰‚·‚é•Ç‚ğŠÜ‚ß‚é
    [SerializeField] private float minVelocity = 5.0f;

    [Header("‰¹”g‚Ìİ’è")]
    [SerializeField] private float waveSpeed = 2.0f;
    [SerializeField] private float waveLifeTime = 3.0f;
    [SerializeField] private float waveScale = 1.0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            if (collision.relativeVelocity.magnitude > minVelocity)
            {
                ContactPoint2D contact = collision.contacts[0];
                GameObject wave = Instantiate(soundWavePrefab, contact.point, Quaternion.identity);

                if (wave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
                {
                    // ‘æ3ˆø”‚Éõ–½‚ğ“n‚·
                    platform.Initialize(contact.normal, waveSpeed, waveLifeTime, waveScale);
                }
            }
        }
    }
}