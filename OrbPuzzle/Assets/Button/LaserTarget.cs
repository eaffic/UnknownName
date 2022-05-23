using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTarget : StageGimmick
{
    [SerializeField] Color _openColor;
    [SerializeField] Color _closeColor;

    //状態によって色を変更する
    MeshRenderer _meshRenderer;
    ParticleSystem.MainModule _particle;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RestigerTarget(this.gameObject);
        TryGetComponent(out _meshRenderer);
        _particle = GetComponentInChildren<ParticleSystem>().main;

        if (IsOpen)
        {
            _meshRenderer.material.color = _openColor;
            _particle.startColor = _openColor;
        }
        else
        {
            _meshRenderer.material.color = _closeColor;
            _particle.startColor = Color.white;
        }
    }

    public override void Open()
    {
        IsOpen = true;
        GameManager.Instance.RemoveTarget(this.gameObject);
        GameManager.Instance.OpenSameNumberItem(this._Number);
        _meshRenderer.material.color = _openColor;
        _particle.startColor = _openColor;
        AudioManager.PlayOpenTarget();
    }

    public override void Close()
    {
        IsOpen = false;
        GameManager.Instance.RestigerTarget(this.gameObject);
        GameManager.Instance.CloseSameNumberItem(this._Number);
        _meshRenderer.material.color = _closeColor;
        _particle.startColor = Color.white;
    }
}
