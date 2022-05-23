using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public bool _NeedAutomaticRotation = false;

    [Tooltip("注視点"), SerializeField] Transform _focus = default;
    [Tooltip("距離"), SerializeField, Range(1f, 20f)] float _distance = 5f;
    [Tooltip("最小距離"), SerializeField, Range(1f, 20f)] float _minDistance = 2f;
    [Tooltip("最大距離"), SerializeField, Range(1f, 20f)] float _maxDistance = 8f;
    [Tooltip("注視点半径"), SerializeField, Min(0f)] float _focusRadius = 1f;
    [Tooltip("注視点を追跡する >0は追跡する"), SerializeField, Range(0f, 1f)] float _focusCentering = 0.5f;
    [Tooltip("回転速度/秒"), SerializeField, Range(1f, 360f)] float _rotationSpeed = 90f;
    [Tooltip("垂直回転最小角度"), SerializeField, Range(-89f, 89f)] float _minVerticalAngle = -30f;
    [Tooltip("垂直回転最大角度"), SerializeField, Range(-89f, 89f)] float _maxVerticalAngle = 60f;
    [Tooltip("自動追尾遅延時間"), SerializeField, Min(0f)] float _alignDelay = 5f;
    [Tooltip("自動追尾速度"), SerializeField, Range(0f, 90f)] float _alignSmoothRange = 45f;
    [Tooltip("障害物Layer"), SerializeField] LayerMask _obstructionMask = -1;

    [Header("Debug數據")]
    [SerializeField] Vector3 _focusPoint; //注視点
    [SerializeField] Vector3 _previewFocusPoint; //前フレームの注視点
    [SerializeField] Vector2 _orbitAngles = new Vector2(45f, 0f); //カメラ角度
    [SerializeField] Vector2 _playerInput = Vector2.zero;
    [SerializeField] float _lastManuaRotationTime; //最後一回カメラ位置を操作した時間記録

    [SerializeField]bool _zoomIn;
    [SerializeField]bool _zoomOut;

    Camera _regularCamera; //カメラの範囲を取るため

    //カメラの視野平面の向き
    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            //nearClipPlane 近い視錐台面の距離 regularCamera.fieldOfView カメラ視野
            //yは近い視錐台面高度の半分(中心高度)
            halfExtends.y = _regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
            //アスペクト比から計算する
            halfExtends.x = halfExtends.y * _regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    /// <summary>
    /// インスペクターで変更した時の値検査
    /// </summary>
    void OnValidate()
    {
        //カメラ垂直回転角度
        if (_maxVerticalAngle < _minVerticalAngle)
        {
            _maxVerticalAngle = _minVerticalAngle;
        }

        //距離チェック
        if(_maxDistance < _minDistance)
        {
            _maxDistance = _minDistance;
        }
        if(_distance < _minDistance)
        {
            _distance = _minDistance;
        }
        if(_distance > _maxDistance)
        {
            _distance = _maxDistance;
        }

    }

    void Awake()
    {
        _regularCamera = GetComponent<Camera>();
        //初期位置、角度設置
        _focusPoint = _focus.position;
        transform.localRotation = Quaternion.Euler(_orbitAngles);
    }

    /// <summary>
    /// カメラの移動はプレイヤーの後でやる
    /// </summary>
    void LateUpdate()
    {
        UpdateFocusPoint();
        ManualZoomInOut();
        //回転入力検査
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation()) //角度修正
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(_orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }
        Vector3 lookDirection = lookRotation * Vector3.forward; //カメラ向き
        Vector3 lookPosition = _focusPoint - lookDirection * _distance; //カメラ位置 = 注視点 - 向き * 距離

        //注視点と焦点を合わせるためのカメラ位置調整
        Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane; //カメラから視錐台最近平面の距離
        Vector3 rectPosition = lookPosition + rectOffset; //視錐台最近平面の位置
        Vector3 castFrom = _focus.position; //注視点
        Vector3 castLine = rectPosition - castFrom; //視錐台最近平面と注視点の方向
        float castDistance = castLine.magnitude; //視錐台最近平面と注視点の距離
        Vector3 castDirection = castLine / castDistance; //単位ベクトル化

        //ブロックされたとき、カメラ位置を移動する
        //箱型の検知エリアで障害物検査
        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, _obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation); //カメラ位置と向き設置
    }

    /// <summary>
    /// カメラ注視点更新
    /// </summary>
    void UpdateFocusPoint()
    {
        _previewFocusPoint = _focusPoint; //このフレームの注視点を記録する
        Vector3 targetPoint = _focus.position;
       
        //注視点位置調整
        //注視点と目標の距離と焦点半径の比較、焦点半径を超えたら注視点を移動する
        if (_focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, _focusPoint);
            //カメラ位置の移動速度
            float t = 1f;
            
            if (distance > 0.01f && _focusCentering > 0f)
            {
                t = Mathf.Pow(1f - _focusCentering, Time.unscaledDeltaTime);
            }
            if (distance > _focusRadius)
            {
                t = Mathf.Min(t, _focusRadius / distance);
            }
            _focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
        }
        else
        {
            _focusPoint = targetPoint;
        }
    }

    /// <summary>
    /// プレイヤー入力修正
    /// </summary>
    /// <returns>回転入力の有無</returns>
    bool ManualRotation()
    {
        const float e = 0.001f; //誤差範囲
        if (Mathf.Abs(_playerInput.x) > e || Mathf.Abs(_playerInput.y) > e)
        {
            //回転速度はスティックの傾きによる
            _orbitAngles += _rotationSpeed * Time.unscaledDeltaTime * _playerInput;
            _lastManuaRotationTime = Time.unscaledTime; //リアル時間を記録
            return true;
        }
        return false;
    }

    void ManualZoomInOut()
    {
        if(_zoomIn && _distance > _minDistance)
        {
            _distance = Mathf.MoveTowards(_distance, _minDistance, 5 * Time.deltaTime);
        }

        if(_zoomOut && _distance < _maxDistance)
        {
            _distance = Mathf.MoveTowards(_distance, _maxDistance, 5 * Time.deltaTime);
        }
    }

    /// <summary>
    /// 角度修正
    /// </summary>
    void ConstrainAngles()
    {
        //x軸の角度表示修正(360度を超えないように)
        _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, _minVerticalAngle, _maxVerticalAngle);
        //y軸角度表示
        if (_orbitAngles.y < 0f)
        {
            _orbitAngles.y += 360f;
        }
        else if (_orbitAngles.y >= 360f)
        {
            _orbitAngles.y -= 360f;
        }
    }

    /// <summary>
    /// カメラ自動追跡
    /// </summary>
    /// <returns></returns>
    bool AutomaticRotation()
    {
        if(!_NeedAutomaticRotation)
        {
            return false;
        }

        if (Time.unscaledTime - _lastManuaRotationTime < _alignDelay)
        {
            return false;
        }

        //注視点の移動量
        Vector2 movement = new Vector2(
            _focusPoint.x - _previewFocusPoint.x,
            _focusPoint.z - _previewFocusPoint.z
        );
        
        float movementDeltaSqr = movement.sqrMagnitude;
        //誤差範囲
        if (movementDeltaSqr < 1e-4f)
        {
            return false;
        }

        //移動ベクトルから角度を取得する(x>0は時計回り x<0は反時計回り)
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        //xz平面の角度を取得する(カメラ角度と移動角度)
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
        //なめらか処理
        float rotationChange = _rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < _alignSmoothRange)
        {
            rotationChange *= deltaAbs / _alignSmoothRange;
        }
        else if (180f - deltaAbs < _alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / _alignSmoothRange;
        }
        _orbitAngles.y = Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);

        return true;
    }

    //角度を取得する
    static float GetAngle(Vector2 direction_)
    {
        float angle = Mathf.Acos(direction_.y) * Mathf.Rad2Deg;

        return direction_.x < 0f ? 360f - angle : angle;
    }

    //---------------------------------------------------------------------------------
    //入力
    public void SetCameraRotate(Vector2 input)
    {
        _playerInput = input;
    }

    public void SetCameraZoomIn(bool input)
    {
        _zoomIn = input;
    }

    public void SetCameraZoomOut(bool input)
    {
        _zoomOut = input;
    }
}
