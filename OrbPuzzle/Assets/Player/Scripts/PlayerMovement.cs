using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Tooltip("プレイヤー入力空間"), SerializeField] Transform _playerInputSpace = default;

    [Tooltip("最大地面歩行速度"), SerializeField, Range(0f, 100f)] float _maxWalkSpeed = 5f;
    [Tooltip("最大地面走る速度"), SerializeField, Range(0f, 100f)] float _maxRunSpeed = 10f;
    [Tooltip("最大地面捕獲速度"), SerializeField, Range(0f, 100f)] float _maxSnapSpeed = 100f;

    [Tooltip("地面検知距離"), SerializeField, Min(0f)] float _probeDistance = 1f;
    [Tooltip("キャラ向き調整タイマー"), SerializeField, Min(0f)] float _turnSmoothTime = 0.1f;

    [Tooltip("移動加速度"), SerializeField, Range(0f, 100f)] float _maxMoveAccleration = 10f;
    [Tooltip("空中加速度"), SerializeField, Range(0f, 100f)] float _maxAirAccleration = 1f;
    [Tooltip("最大ジャンプ高さ"), SerializeField, Range(0f, 10f)] float _jumpHeight = 2f;
    [Tooltip("最大空中ジャンプ数(地面ジャンプ除く)"), SerializeField, Range(0, 5)] int _maxAirJumps = 0;

    [Tooltip("最大斜面通行角度"), SerializeField, Range(0f, 90f)] float _maxGroundAngle = 45f;
    [Tooltip("地面Layer"), SerializeField] LayerMask probeMask = -1;

    Rigidbody _rigidBody;
    PlayerAnimation _animation;
    PlayerAction _action;
    Vector3 _directionInput;
    Vector3 _velocity;  //現在速度
    Vector3 _desiredVelocity;  //希望速度
    Vector3 _contactNormal; //接触斜面の法線合計
    float _minGroundDotProduct; //斜面角度のcosθ
    float _turnSmoothVelocity; //転向速度
    int _jumpPhase; //ジャンプ状態記録
    int _stepsSinceLastGrounded; //最後の地面接触記録
    int _stepsSinceLastJump; //最後のジャンプタイミング記録
    int _groundContactCount; //接触した地面の数
    bool _jumpInput;
    bool _runInput;

    public bool OnGround => _groundContactCount > 0; //地面接触記録
    public bool LastGrounded => _stepsSinceLastGrounded > 3;
    public bool isMove => new Vector2(_velocity.x, _velocity.z).magnitude > 1e-2;
    public bool isRun => _runInput;

    void OnValidate()
    {
        //通過可能の角度の最小内積値(最大角度)
        _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        TryGetComponent(out _rigidBody);
        TryGetComponent(out _animation);
        TryGetComponent(out _action);
        OnValidate();
    }

    void Update()
    {
        if (GameManager.Instance._StageClear)
        {
            _velocity = _rigidBody.velocity;
        }
    }

    void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad < 2f || GameManager.Instance._StageClear) { return; }
        if (_action._PlayerState == PLAYERSTATE.PUSH) { return; }

        UpdateState();
        AdjustVelocity();

        if (_jumpInput)
        {
            _jumpInput = false;
            _action._PlayerState = PLAYERSTATE.JUMP;
            AudioManager.PlayJumpAudio();
            Jump();
        }

        _rigidBody.velocity = _velocity;
        ClearState();
    }

    /// <summary>
    /// キャラ位置の状態更新
    /// </summary>
    void UpdateState()
    {
        _stepsSinceLastGrounded++;
        _stepsSinceLastJump++;
        _velocity = _rigidBody.velocity; //最新速度を取得

        if (OnGround || SnapToGround())
        {
            _stepsSinceLastGrounded = 0; //リセット
            if (_stepsSinceLastJump > 1)
            {
                _jumpPhase = 0;
                _action.JumpStateEnd();
            }

            if (_groundContactCount > 0)
            {
                _contactNormal.Normalize(); //上方向ベクトル
            }
        }
        else
        {
            _contactNormal = Vector3.up;
        }
    }

    /// <summary>
    /// 速度方向修正
    /// </summary>
    void AdjustVelocity()
    {
        //地面のxz軸の方向
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        //現在地面方向のxz軸の速度
        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);

        //加速度率
        float accelarion = OnGround ? _maxMoveAccleration : _maxAirAccleration;
        float maxSpeedChange = accelarion * Time.deltaTime;

        //新しい速度
        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);

        //速度更新(新旧速度の差から方向を調整する)
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        //速度から向き調整
        if (new Vector3(_velocity.x, 0f, _velocity.z).magnitude > 0.01f)
        {
            float turnAngle = Mathf.Atan2(_velocity.x, _velocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, turnAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    void Jump()
    {
        Vector3 jumpDirection;

        //ジャンプ状態、上方向を設定する
        if(OnGround)
        {
            jumpDirection = _contactNormal;
        }
        else if(_maxAirJumps > 0 && _jumpPhase <= _maxAirJumps)
        {
            //直接落下した場合、一回のジャンプ回数を減らす
            if(_jumpPhase == 0)
            {
                _jumpPhase = 1;
            }

            jumpDirection = _contactNormal;
        }
        else
        {
            //ジャンプ可能状態以外は終了する
            return;
        }

        _stepsSinceLastJump = 0;
        _jumpPhase++;

        //ジャンプ初期速度は設定した高さから計算する v = √-2gh
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * _jumpHeight);
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection); //現在速度のジャンプ方向の長さ（大きさ）
        //現在速度とジャンプ速度の調整
        if(alignedSpeed > 0f)
        {
            //ジャンプ速度をマイナスにならないように制限する
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        _velocity += jumpDirection * jumpSpeed; //ジャンプ速度を追加する

        _animation.SetJumpAnimation();
    }

    /// <summary>
    /// 接触コライダーの法線、地面判断など
    /// </summary>
    /// <param name="collision"></param>
    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minDot)
            {
                _groundContactCount++;
                _contactNormal += normal;
            }
            else if(normal.y > -0.01f)
            {
                //移動不可の斜面に挟まれたとき
            }
        }
    }

    /// <summary>
    /// 地面方向ベクトル
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    /// <summary>
    /// 地面接触判断(一瞬地面で離しても戻れるように)
    /// trueに返すとOnGroundと判断されている
    /// </summary>
    /// <returns></returns>
    bool SnapToGround()
    {
        //空中ではない時
        //ジャンプ速度を確保するため
        if(_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2)
        {
            return false;
        }
        //速度は地面検知速度以内のこと
        float speed = _velocity.magnitude;
        if(speed > _maxSnapSpeed)
        {
            return false;
        }
        //足元は地面コライダー
        if(!Physics.Raycast(_rigidBody.position, Vector3.down,out RaycastHit hit, _probeDistance, probeMask))
        {
            return false;
        }
        //足元のコライダーの角度確認(移動不可の斜面ではない)
        if(hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        _groundContactCount = 1;
        _contactNormal = hit.normal;

        //現在速度は地面法線方向の大きさ
        float dot = Vector3.Dot(_velocity, hit.normal);
        //この物体は地面から離れている時、速度を修正する
        if(dot > 0)
        {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    //壁の接触情報と法線をリセットする
    void ClearState()
    {
        _groundContactCount = 0;
        _contactNormal = Vector3.zero;
    }

    /// <summary>
    /// レイヤから最大通過内積値を取得する
    /// </summary>
    /// <param name="layer">障害物のレイヤ</param>
    /// <returns></returns>
    float GetMinDot(int layer)
    {
        return _minGroundDotProduct;
    }

    /// <summary>
    /// 走るや歩くの最大速度を取得
    /// </summary>
    /// <returns></returns>
    float GetMoveMaxSpeed()
    {
        return _runInput ? _maxRunSpeed : _maxWalkSpeed;
    }

    void OnCollisionEnter(Collision other)
    {
        EvaluateCollision(other);
    }

    void OnCollisionStay(Collision other)
    {
        EvaluateCollision(other);
    }

    //-----------------------------------------------------------------------
    //入力
    public void SetInputDirection(Vector3 input)
    {
        _directionInput = Vector3.ClampMagnitude(input, 1f);

        //カメラ方向を基に進行方向を調整する
        if(_playerInputSpace)
        {
            Vector3 forward = _playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = _playerInputSpace.right;
            right.y = 0;
            right.Normalize();
            _desiredVelocity = (right * _directionInput.x + forward * _directionInput.z) * GetMoveMaxSpeed(); //希望速度
        }
        else
        {
            _desiredVelocity = new Vector3(_directionInput.x, 0f, _directionInput.z) * GetMoveMaxSpeed(); //希望速度
        }

        if(_desiredVelocity.magnitude < 0.1f)
        {
            _runInput = false;
        }
    }

    public void SetInputJump(bool input)
    {
        _jumpInput |= input;
    }

    public void SetInputRun()
    {
        _runInput = !_runInput;
        if(_runInput && isMove)
        {
            AudioManager.PlayRunStepAudio();
        }
    }
}
