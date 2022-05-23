using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputManager : MonoBehaviour
{
    OrbitCamera _camera;
    PlayerInput _input;

    void Awake()
    {
        TryGetComponent(out _camera);
        TryGetComponent(out _input);
    }

    void OnEnable()
    {
        _input.actions["Rotate"].performed += OnRotate;
        _input.actions["Rotate"].canceled += OnRotateStop;
        _input.actions["ZoomIn"].started += OnZoomIn;
        _input.actions["ZoomIn"].canceled += OnZoomInStop;
        _input.actions["ZoomOut"].started += OnZoomOut;
        _input.actions["ZoomOut"].canceled += OnZoomOutStop;
    }

    void OnDisable()
    {
        _input.actions["Rotate"].performed -= OnRotate;
        _input.actions["Rotate"].canceled -= OnRotateStop;
        _input.actions["ZoomIn"].started -= OnZoomIn;
        _input.actions["ZoomIn"].canceled -= OnZoomInStop;
        _input.actions["ZoomOut"].started -= OnZoomOut;
        _input.actions["ZoomOut"].canceled -= OnZoomOutStop;
    }

    void OnRotate(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();
        var direction = new Vector2(-value.y, value.x);
        _camera.SetCameraRotate(direction);
    }

    void OnRotateStop(InputAction.CallbackContext obj)
    {
        _camera.SetCameraRotate(Vector2.zero);
    }

    void OnZoomIn(InputAction.CallbackContext obj)
    {
        _camera.SetCameraZoomIn(true);
    }

    void OnZoomInStop(InputAction.CallbackContext obj)
    {
        _camera.SetCameraZoomIn(false);
    }

    void OnZoomOut(InputAction.CallbackContext obj)
    {
        _camera.SetCameraZoomOut(true);
    }

    void OnZoomOutStop(InputAction.CallbackContext obj)
    {
        _camera.SetCameraZoomOut(false);
    }
}
