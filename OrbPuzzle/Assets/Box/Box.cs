using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    [Tooltip("移動時間"), SerializeField] float _moveTime = 0.3f;
    [Tooltip("検知Layer"), SerializeField] LayerMask _wallLayer = default;

    Rigidbody _rigidBody;
    Vector3 _velocity;
    bool OnHead => Physics.Raycast(transform.position, Vector3.up, 0.5f, _wallLayer);   //上方向確認
    bool OnGround => Physics.Raycast(transform.position, Vector3.down, 0.9f);   //地面確認

    void Awake()
    {
        TryGetComponent(out _rigidBody);
        if (!OnGround)
        {
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
    }
    
    void Update()
    {
        if (!OnGround)
        {
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (OnGround)
        {
            _rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            transform.position = new Vector3(transform.position.x, (int)transform.position.y, transform.position.z);
        }
    }

    //移動
    public void MoveBox(Vector3 direction)
    {
        if(MoveChecked(direction))
        {
            StartCoroutine(BoxMove(transform.position + direction));
        }
    }

    //移動先チェック
    public bool MoveChecked(Vector3 direction)
    {
        //if(OnHead){ return false; }
        return !Physics.Raycast(transform.position, direction, out RaycastHit hitInfo, direction.magnitude, _wallLayer);
    }   

    //箱移動
    IEnumerator BoxMove(Vector3 movePosition)
    {
        AudioManager.PlayBoxMoveAudio();

        while(Vector3.Distance(transform.position, movePosition) > 0.1f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, movePosition, ref _velocity, _moveTime);
            
            yield return null;
        }

        transform.position = movePosition;

        yield return null;
    }
}
