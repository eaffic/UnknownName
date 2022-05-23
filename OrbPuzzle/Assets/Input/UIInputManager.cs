using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputManager : MonoBehaviour
{
    TitleSceneController _title;
    PlayerInput _input;

    void Awake()
    {
        TryGetComponent(out _title);
        TryGetComponent(out _input);
    }

    void OnEnable()
    {
        _input.actions["Submit"].started += OnSubmit;
        _input.actions["Select"].performed += OnSelect;
    }

    void OnDisable()
    {
        _input.actions["Submit"].started -= OnSubmit;
        _input.actions["Select"].performed -= OnSelect;
    }

    void OnSelect(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<float>();
        int select = 0;
        if (value > 0) { select = 1; }
        if (value < 0) { select = -1; }
        _title.SetSelectInput(select);
    }

    void OnSubmit(InputAction.CallbackContext obj)
    {
        _title.SetSubmitInput();
    }
}
