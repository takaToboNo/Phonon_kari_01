using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemを使うために必要
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SoundShooter : MonoBehaviour
{
    [Header("参照設定")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject wavePrefab;

    [Header("入力設定 (新しいInput System)")]
    [SerializeField] private Key zKey = Key.Z; // インスペクターからキーを変更可能

    [Header("音波のパラメーター")]
    [SerializeField] private float waveSpeed = 10.0f;
    [SerializeField] private float waveLifeTime = 2.0f;
    [SerializeField] private float waveScale = 1.0f;

    [Header("反動（リコイル）設定")]
    [SerializeField] private float recoilForce = 5.0f;
    [SerializeField] private float cooldown = 0.3f;

    private Rigidbody2D rb;
    private float lastShootTime;
    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        if (exitPoint == null)
        {
            Debug.LogError("Exit Pointが設定されていません！子オブジェクトを割り当ててください。");
        }
    }

    void Update()
    {
        // キーボードが接続されているかチェック
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 指定したキーが「このフレームで押されたか」を判定
        if (keyboard[zKey].wasPressedThisFrame && Time.time >= lastShootTime + cooldown)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (wavePrefab == null || exitPoint == null) return;

        lastShootTime = Time.time;

        // 1. 音波の生成
        GameObject newWave = Instantiate(wavePrefab, exitPoint.position, exitPoint.rotation);

        // 2. 音波の初期化
        if (newWave.TryGetComponent<SoundWavePlatform>(out SoundWavePlatform platform))
        {
            platform.Initialize(exitPoint.right, waveSpeed, waveLifeTime, waveScale);
        }

        // 3. 反動（出口の反対方向へ力を加える）
        Vector2 recoilDirection = -exitPoint.right;
        rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);

        // 4. 演出
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(PunchEffectRoutine());
    }

    private IEnumerator PunchEffectRoutine()
    {
        float duration = 0.05f;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(duration);
        transform.localScale = originalScale;
        punchCoroutine = null;
    }
}