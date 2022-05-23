using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputManager : MonoBehaviour
{
    //プレイヤーのスクリプトとインプットシステム
    PlayerMovement _movement;
    PlayerAction _action;
    PlayerInput _input;

    void Awake()
    {
        TryGetComponent(out _movement);
        TryGetComponent(out _action);
        TryGetComponent(out _input);
    }

    void Update()
    {
        //移動
        var value = _input.actions["Move"].ReadValue<Vector2>();
        var direction = new Vector3(value.x, 0f, value.y);
        _movement.SetInputDirection(direction);
    }

    //イベント登録
    void OnEnable()
    {
        _input.actions["Jump"].started += OnJump;
        _input.actions["Push"].started += OnPush;
        _input.actions["Run"].started += OnRun;
        _input.actions["Reset"].started += OnResetStart;
        _input.actions["Reset"].canceled += OnResetStop;
    }
    //イベント削除
    void OnDisable()
    {
        _input.actions["Jump"].started -= OnJump;
        _input.actions["Push"].started -= OnPush;
        _input.actions["Run"].started -= OnRun;
        _input.actions["Reset"].started -= OnResetStart;
        _input.actions["Reset"].canceled -= OnResetStop;
    }

    void OnJump(InputAction.CallbackContext obj)
    {
        _movement.SetInputJump(true);
    }

    void OnPush(InputAction.CallbackContext obj)
    {
        _action.SetInputPush();
    }

    void OnRun(InputAction.CallbackContext obj)
    {
        _movement.SetInputRun();
    }

    void OnResetStart(InputAction.CallbackContext obj)
    {
        GameManager.Instance.ResetGameStart();
    }

    void OnResetStop(InputAction.CallbackContext obj)
    {
        GameManager.Instance.ResetGameStop();
    }
}
