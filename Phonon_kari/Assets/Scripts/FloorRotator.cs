using UnityEngine;

/// <summary>
/// 2Dオブジェクトを回転させるスクリプト。
/// インスペクターから回転速度・方向・イージング・往復回転などを設定できます。
/// 外部スクリプトから StartRotation() / StopRotation() で制御可能です。
/// </summary>
public class FloorRotator : MonoBehaviour
{
    // =====================================================================
    //  回転基本設定
    // =====================================================================
    [Header("── 回転基本設定 ──────────────────")]

    [Tooltip("回転速度（度/秒）")]
    [Min(0f)]
    public float rotationSpeed = 90f;

    public enum RotationDirection { Clockwise = -1, CounterClockwise = 1 }

    [Tooltip("回転方向")]
    public RotationDirection direction = RotationDirection.CounterClockwise;

    // =====================================================================
    //  ピボットオフセット
    // =====================================================================
    [Header("── ピボットオフセット ─────────────")]

    [Tooltip("回転中心点のオフセット（ローカル座標）\n(0,0) でオブジェクト中心を軸に回転します")]
    public Vector2 pivotOffset = Vector2.zero;

    // =====================================================================
    //  イージング（加速・減速）
    // =====================================================================
    [Header("── イージング ───────────────────")]

    [Tooltip("イージングを使用する")]
    public bool useEasing = false;

    [Tooltip("最大速度に達するまでの加速時間（秒）")]
    [Min(0f)]
    public float accelerationTime = 1f;

    [Tooltip("停止するまでの減速時間（秒）")]
    [Min(0f)]
    public float decelerationTime = 1f;

    // =====================================================================
    //  往復回転（Oscillation）
    // =====================================================================
    [Header("── 往復回転（Oscillation）─────────")]

    [Tooltip("往復回転モードを有効にする\n（振り子・ドアのような動き）")]
    public bool oscillate = false;

    [Tooltip("往復する角度の範囲（度）\n例: 45 → ±45° 往復")]
    [Min(0f)]
    public float oscillateAngle = 45f;

    public enum OscillationTurnMode { Instant, Smooth }

    [Tooltip("折り返し時の切り替え方法")]
    public OscillationTurnMode oscillationTurnMode = OscillationTurnMode.Instant;

    [Tooltip("Smooth時に、折り返し前後をどれくらいの時間なめらかにするか（秒）\n大きいほど端でゆっくり折り返します")]
    [Min(0.01f)]
    public float oscillationTurnSmoothTime = 0.15f;

    // =====================================================================
    //  内部状態
    // =====================================================================
    private bool _hasAppliedPivotOffset = false;
    private bool _wantsRotation = true;
    private bool _wasOscillating = false;
    private Vector2 _appliedPivotOffset = Vector2.zero;
    private float _currentSpeed = 0f;
    private float _oscillateCurrentAngle = 0f;
    private float _oscillationSegmentEndAngle = 0f;
    private float _oscillationSegmentProgress = 0f;
    private float _oscillationSegmentStartAngle = 0f;
    private Transform _pivotTransform;
    private Quaternion _oscillationBaseLocalRotation = Quaternion.identity;

    // =====================================================================
    //  初期化
    // =====================================================================
    void Start()
    {
        ApplyPivotOffset(force: true, preserveObjectWorldPose: true);
        CaptureOscillationBaseRotation();
        _wasOscillating = oscillate;
        _currentSpeed = useEasing ? 0f : GetClampedRotationSpeed();
    }

    // =====================================================================
    //  毎フレーム更新
    // =====================================================================
    void Update()
    {
        HandlePivotOffsetChange();
        HandleOscillationModeChange();
        UpdateSpeed();

        if (_currentSpeed <= 0f)
        {
            return;
        }

        float deltaAngle = _currentSpeed * Time.deltaTime;

        if (oscillate)
        {
            UpdateOscillation();
        }
        else
        {
            UpdateContinuousRotation(deltaAngle);
        }
    }

    // =====================================================================
    //  通常回転
    // =====================================================================
    private void UpdateContinuousRotation(float deltaAngle)
    {
        float rotAmount = deltaAngle * (int)direction;
        GetRotationTarget().Rotate(0f, 0f, rotAmount);
    }

    // =====================================================================
    //  往復回転
    // =====================================================================
    private void UpdateOscillation()
    {
        if (oscillateAngle <= 0f)
        {
            BeginOscillationSegment(0f, 0f);
            ApplyOscillationRotation();
            return;
        }

        float remainingTime = Time.deltaTime;
        float speed = Mathf.Max(0f, _currentSpeed);
        if (remainingTime <= 0f || speed <= 0f)
        {
            ApplyOscillationRotation();
            return;
        }

        while (remainingTime > 0f)
        {
            float segmentDistance = Mathf.Abs(_oscillationSegmentEndAngle - _oscillationSegmentStartAngle);
            if (segmentDistance <= 0.0001f)
            {
                AdvanceOscillationSegment();
                continue;
            }

            float segmentDuration = segmentDistance / speed;
            float timeLeftInSegment = (1f - _oscillationSegmentProgress) * segmentDuration;

            if (remainingTime >= timeLeftInSegment)
            {
                _oscillateCurrentAngle = _oscillationSegmentEndAngle;
                remainingTime -= timeLeftInSegment;
                AdvanceOscillationSegment();
            }
            else
            {
                _oscillationSegmentProgress += remainingTime / segmentDuration;
                remainingTime = 0f;
            }
        }

        ApplyOscillationRotation();
    }

    // =====================================================================
    //  イージング
    // =====================================================================
    private void UpdateSpeed()
    {
        float targetSpeed = _wantsRotation ? GetClampedRotationSpeed() : 0f;

        if (!useEasing)
        {
            _currentSpeed = targetSpeed;
            return;
        }

        float easingTime = _wantsRotation ? accelerationTime : decelerationTime;

        if (easingTime > 0f)
        {
            float referenceSpeed = Mathf.Max(_currentSpeed, GetClampedRotationSpeed());
            float step = referenceSpeed / easingTime * Time.deltaTime;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, step);
        }
        else
        {
            _currentSpeed = targetSpeed;
        }
    }

    // =====================================================================
    //  ピボットセットアップ
    // =====================================================================
    private void ApplyPivotOffset(bool force, bool preserveObjectWorldPose)
    {
        bool needsPivot = pivotOffset != Vector2.zero;
        bool offsetChanged = !_hasAppliedPivotOffset || _appliedPivotOffset != pivotOffset;
        bool pivotStateMismatch = needsPivot != (_pivotTransform != null);

        if (!force && !offsetChanged && !pivotStateMismatch)
        {
            return;
        }

        if (preserveObjectWorldPose)
        {
            ApplyPivotOffsetPreservingObjectPose(needsPivot);
        }
        else
        {
            ApplyPivotOffsetPreservingRotationCenter(needsPivot);
        }

        _appliedPivotOffset = pivotOffset;
        _hasAppliedPivotOffset = true;
    }

    private void ApplyPivotOffsetPreservingObjectPose(bool needsPivot)
    {
        Transform outerParent = _pivotTransform != null ? _pivotTransform.parent : transform.parent;

        if (_pivotTransform != null)
        {
            transform.SetParent(outerParent, true);
            DestroyPivotObject(_pivotTransform.gameObject);
            _pivotTransform = null;
        }

        if (!needsPivot)
        {
            return;
        }

        Vector3 pivotWorldPosition = transform.TransformPoint(new Vector3(pivotOffset.x, pivotOffset.y, 0f));

        GameObject pivotGO = new GameObject($"{gameObject.name}_Pivot");
        pivotGO.transform.SetParent(outerParent, true);
        pivotGO.transform.position = pivotWorldPosition;
        pivotGO.transform.rotation = transform.rotation;

        transform.SetParent(pivotGO.transform, true);
        _pivotTransform = pivotGO.transform;
    }

    private void ApplyPivotOffsetPreservingRotationCenter(bool needsPivot)
    {
        Transform outerParent = _pivotTransform != null ? _pivotTransform.parent : transform.parent;
        Vector3 rotationCenterWorld = _pivotTransform != null ? _pivotTransform.position : transform.position;
        Quaternion worldRotation = transform.rotation;

        if (!needsPivot)
        {
            if (_pivotTransform == null)
            {
                return;
            }

            GameObject pivotObject = _pivotTransform.gameObject;
            transform.SetParent(outerParent, true);
            transform.position = rotationCenterWorld;
            transform.rotation = worldRotation;
            _pivotTransform = null;
            DestroyPivotObject(pivotObject);
            return;
        }

        Transform pivot = _pivotTransform;
        if (pivot == null)
        {
            GameObject pivotGO = new GameObject($"{gameObject.name}_Pivot");
            pivot = pivotGO.transform;
        }

        pivot.SetParent(outerParent, true);
        pivot.position = rotationCenterWorld;
        pivot.rotation = worldRotation;

        transform.SetParent(pivot, false);
        transform.localPosition = new Vector3(-pivotOffset.x, -pivotOffset.y, 0f);
        transform.localRotation = Quaternion.identity;
        _pivotTransform = pivot;
    }

    private void HandlePivotOffsetChange()
    {
        bool needsPivot = pivotOffset != Vector2.zero;
        bool pivotMissing = needsPivot && _pivotTransform == null;
        bool offsetChanged = !_hasAppliedPivotOffset || _appliedPivotOffset != pivotOffset;

        if (!offsetChanged && !pivotMissing)
        {
            return;
        }

        ApplyPivotOffset(force: true, preserveObjectWorldPose: false);

        if (oscillate)
        {
            ResetOscillationFromCurrentPose();
        }
        else
        {
            CaptureOscillationBaseRotation();
        }
    }

    private void ResetOscillationFromCurrentPose()
    {
        _oscillateCurrentAngle = 0f;
        CaptureOscillationBaseRotation();
        BeginOscillationSegment(0f, oscillateAngle * (int)direction);
    }

    private void ApplyOscillationRotation()
    {
        float shapedProgress = EvaluateOscillationSegmentProgress();
        _oscillateCurrentAngle = Mathf.Lerp(
            _oscillationSegmentStartAngle,
            _oscillationSegmentEndAngle,
            shapedProgress);

        GetRotationTarget().localRotation =
            _oscillationBaseLocalRotation * Quaternion.Euler(0f, 0f, _oscillateCurrentAngle);
    }

    private void AdvanceOscillationSegment()
    {
        BeginOscillationSegment(_oscillationSegmentEndAngle, -_oscillationSegmentEndAngle);
    }

    private void BeginOscillationSegment(float startAngle, float endAngle)
    {
        _oscillationSegmentStartAngle = startAngle;
        _oscillationSegmentEndAngle = endAngle;
        _oscillationSegmentProgress = 0f;
    }

    private float EvaluateOscillationSegmentProgress()
    {
        float timeProgress = _oscillationSegmentProgress;

        if (oscillationTurnMode == OscillationTurnMode.Instant)
        {
            return timeProgress;
        }

        float segmentDistance = Mathf.Abs(_oscillationSegmentEndAngle - _oscillationSegmentStartAngle);
        float speed = Mathf.Max(0.0001f, _currentSpeed);
        float segmentDuration = segmentDistance / speed;
        float edgeFraction = Mathf.Min(0.5f, oscillationTurnSmoothTime / Mathf.Max(0.0001f, segmentDuration));

        if (edgeFraction <= 0f)
        {
            return timeProgress;
        }

        if (timeProgress < edgeFraction)
        {
            float u = timeProgress / edgeFraction;
            return edgeFraction * EvaluateTurnEdgeEase(u);
        }

        if (timeProgress > 1f - edgeFraction)
        {
            float u = (1f - timeProgress) / edgeFraction;
            return 1f - edgeFraction * EvaluateTurnEdgeEase(u);
        }

        return timeProgress;
    }

    private float EvaluateTurnEdgeEase(float normalizedValue)
    {
        return -normalizedValue * normalizedValue * normalizedValue
            + 2f * normalizedValue * normalizedValue;
    }

    private void DestroyPivotObject(GameObject pivotObject)
    {
        if (pivotObject == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(pivotObject);
        }
        else
        {
            DestroyImmediate(pivotObject);
        }
    }

    private Transform GetRotationTarget()
    {
        return _pivotTransform != null ? _pivotTransform : transform;
    }

    private float GetClampedRotationSpeed()
    {
        return Mathf.Max(0f, rotationSpeed);
    }

    private void CaptureOscillationBaseRotation()
    {
        _oscillationBaseLocalRotation = GetRotationTarget().localRotation;
    }

    private void HandleOscillationModeChange()
    {
        if (oscillate == _wasOscillating)
        {
            return;
        }

        if (oscillate)
        {
            ResetOscillationFromCurrentPose();
        }

        _wasOscillating = oscillate;
    }

    // =====================================================================
    //  外部制御 API
    // =====================================================================

    /// <summary>回転を開始します。</summary>
    public void StartRotation()
    {
        _wantsRotation = true;

        if (!useEasing)
        {
            _currentSpeed = GetClampedRotationSpeed();
        }
    }

    /// <summary>回転を停止します（イージングが有効な場合は徐々に減速）。</summary>
    public void StopRotation()
    {
        _wantsRotation = false;

        if (!useEasing)
        {
            _currentSpeed = 0f;
        }
    }

    /// <summary>回転中かどうかを返します。</summary>
    public bool IsRotating => _wantsRotation || _currentSpeed > 0.0001f;

    /// <summary>回転方向を反転します。</summary>
    public void ReverseDirection()
    {
        direction = direction == RotationDirection.Clockwise
            ? RotationDirection.CounterClockwise
            : RotationDirection.Clockwise;
    }

    /// <summary>回転速度を動的に変更します。</summary>
    public void SetSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0f, speed);

        if (!useEasing && _wantsRotation)
        {
            _currentSpeed = rotationSpeed;
        }
    }
}
