using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PLAYERSTATE
{
    NORMAL, //通常状態
    JUMP,   //ジャンプ
    PUSH,    //プッシュ(箱)
}

public class PlayerAction : MonoBehaviour
{
    [Tooltip("プレイヤー状態")] public PLAYERSTATE _PlayerState;

    [Tooltip("ヒントUI"), SerializeField] GameObject _hintUI = default;
    [Tooltip("目の高さ"), SerializeField] float _eyeHeight = 0.7f;
    [Tooltip("前方レイの長さ(箱判定用)"), SerializeField] float _forwardRayLength = 0.5f;

    PlayerAnimation _animation;
    GameObject _targetBox;  //目標箱
    bool _inBoxPushArea;    //箱のプッシュ可能範囲
    bool _pushInput;        //プレイヤーインプット

    void Awake()
    {
        TryGetComponent(out _animation);

        _PlayerState = PLAYERSTATE.NORMAL;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad < 2f) { return; }

        if (_pushInput)
        {
            _pushInput = false;
            PushBox();
        }

        _hintUI.SetActive(CheckForward());

        Debug.DrawRay(transform.position + new Vector3(0f, _eyeHeight, 0f), transform.forward * _forwardRayLength);
    }


    void PushBox()
    {
        bool check = false;

        //プレイヤーの向きから箱のプッシュ方向を決める
        //箱の移動可能を確認
        if (transform.forward.z > 0.8f)
        {
            check = _targetBox.GetComponent<Box>().MoveChecked(Vector3.forward);
        }
        else if (transform.forward.z < -0.8f)
        {
            check = _targetBox.GetComponent<Box>().MoveChecked(Vector3.back);
        }
        else if (transform.forward.x > 0.8f)
        {
            check = _targetBox.GetComponent<Box>().MoveChecked(Vector3.right);
        }
        else if (transform.forward.x < -0.8f)
        {
            check = _targetBox.GetComponent<Box>().MoveChecked(Vector3.left);
        }

        if (check)
        {
            //状態変更、アニメーション設置
            _PlayerState = PLAYERSTATE.PUSH;
            _animation.SetPushAnimation();
        }
    }

    //前方箱チェック
    bool CheckForward()
    {
        Ray ray = new Ray(transform.position + new Vector3(0f, _eyeHeight, 0f), transform.forward);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hitInfo, _forwardRayLength, LayerMask.GetMask("Box")))
        {
            _targetBox = hitInfo.collider.gameObject;
            return true;
        }
        else
        {
            _targetBox = null;
            return false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "BoxPushArea")
        {
            _inBoxPushArea = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "BoxPushArea")
        {
            _inBoxPushArea = false;
        }
    }

    //---------------------------------------------------------------------
    //入力
    public void SetInputPush()
    {
        if (_PlayerState == PLAYERSTATE.NORMAL && _inBoxPushArea && CheckForward())
        {
            _pushInput = true;
        }
    }

    public void JumpStateEnd()
    {
        _PlayerState = PLAYERSTATE.NORMAL;
    }

    public void PushStateEnd()
    {
        _PlayerState = PLAYERSTATE.NORMAL;
    }

    public void PushTargetBox()
    {
        if (transform.forward.z > 0.8f)
        {
            _targetBox.GetComponent<Box>().MoveBox(Vector3.forward);
        }
        else if (transform.forward.z < -0.8f)
        {
            _targetBox.GetComponent<Box>().MoveBox(Vector3.back);
        }
        else if (transform.forward.x > 0.8f)
        {
            _targetBox.GetComponent<Box>().MoveBox(Vector3.right);
        }
        else if (transform.forward.x < -0.8f)
        {
            _targetBox.GetComponent<Box>().MoveBox(Vector3.left);
        }
    }

    //---------------------------------------------------------------------
    //SE
    public void PlayerAction_PlayPunchAudio()
    {
        AudioManager.PlayPunchAudio();
    }
}
