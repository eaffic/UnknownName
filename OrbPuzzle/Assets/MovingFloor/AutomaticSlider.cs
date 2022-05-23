using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutomaticSlider : StageGimmick
{
    [System.Serializable]
    public class OnValueChangedEvent : UnityEvent<float> { }
    [SerializeField] OnValueChangedEvent _onValueChanged = default;
    [SerializeField, Min(0.01f)] float _duration = 1f;
    [SerializeField] bool _autoReversed = false; 
    [SerializeField] bool _smoothstep = false; //スムーズ移動

    float _value;
    float _smoothedValue => 3f * _value * _value - 2f * _value * _value * _value; //スムーズ関数
    bool Reversed { get; set; } //往復確認
    bool AutoReverse //往復移動
    {
        get => _autoReversed;
        set => _autoReversed = value;
    }

    void Start()
    {
        GameManager.Instance.RegisterMovingFloor(this.gameObject);
        enabled = false;
    }

    void FixedUpdate()
    {
        float delta = Time.deltaTime / _duration;
        if (IsOpen)
        {
            _value -= delta;
            if (_value <= 0f)
            {
                if (AutoReverse)
                {
                    _value = Mathf.Min(1f, -_value);
                    IsOpen = false;
                }
                else
                {
                    _value = 0f;
                    enabled = false;
                }
            }
        }
        else
        {
            //1の時停止
            _value += delta;
            if (_value >= 1f)
            {
                if (AutoReverse)
                {
                    _value = Mathf.Max(0f, 2f - _value);
                    Reversed = true;
                }
                else
                {
                    _value = 1f;
                    enabled = false;
                }
            }
        }

        _onValueChanged.Invoke(_smoothstep ? _smoothedValue : _value);
    }

    public override void Open()
    {
        IsOpen = false;
    }

    public override void Close()
    {
        IsOpen = true;
    }
}
