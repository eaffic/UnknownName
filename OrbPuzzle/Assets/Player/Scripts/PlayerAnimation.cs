using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//プレイヤーアニメーション制御
public class PlayerAnimation : MonoBehaviour
{
    Animator _animator;
    PlayerMovement _movement;
    Rigidbody _rigidbody;

    bool _isPlayingMoveAnimation;
    bool _isPushPlaying;

    void Awake()
    {
        TryGetComponent(out _animator);
        TryGetComponent(out _movement);
        TryGetComponent(out _rigidbody);
    }
    
    void Update()
    {
        _animator.SetBool("Walking", _movement.isMove);
        _animator.SetBool("Falling", !_movement.OnGround && _rigidbody.velocity.y < 0f && _movement.LastGrounded);
        _animator.SetBool("OnGround", _movement.OnGround);
        _animator.SetBool("Running", _movement.isRun && _movement.isMove);
        PlayerStepAudioSetting();
    }

    void PlayerStepAudioSetting()
    {
        if (!_isPlayingMoveAnimation && _movement.isMove)
        {
            _isPlayingMoveAnimation = true;
            AudioManager.PlayWalkStepAudio();
        }
        else if(_isPlayingMoveAnimation && !_movement.isMove)
        {
            _isPlayingMoveAnimation = false;
            AudioManager.StopPlayerAudio();
        }
    }

    //----------------------------------------------------------------------------
    public void SetClearAnimation()
    {
        _animator.SetTrigger("Clear");
    }

    public void SetJumpAnimation()
    {
        _animator.SetTrigger("Jump");
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("JumpToTop"))
        {
            _animator.Play("JumpToTop", 0);
        }
    }

    public void SetPushAnimation()
    {
        
        _animator.SetTrigger("Push");
        if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Push"))
        {
            _animator.Play("Push", 0);
        }
    }

    public void PunchAnimationEnd()
    {
        _isPlayingMoveAnimation = false;
    }
}
