using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : StageGimmick
{
    [SerializeField] bool _stateLock;   //一度起動したら終了しない
    [SerializeField] LayerMask _searchLayer = default;

    Light _light;
    MeshRenderer _meshRenderer;

    void Awake()
    {
        _light = GetComponentInChildren<Light>();
        TryGetComponent(out _meshRenderer);
    }

    void Start()
    {
        GameManager.Instance.RegisterButton(this.gameObject);

        if(TopCheck())
        {
            IsOpen = true;
            GameManager.Instance.OpenSameNumberItem(_Number);
            _light.GetComponent<Light>().color = Color.green;
            _meshRenderer.material.color = Color.green;
        }
        else
        {
            IsOpen = false;
            GameManager.Instance.CloseSameNumberItem(_Number);
            _light.GetComponent<Light>().color = Color.red;
            _meshRenderer.material.color = Color.red;
        }
    }

    //載せているものチェック
    bool TopCheck()
    {
        var colliders = Physics.OverlapBox(transform.position + transform.up * 0.5f, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity, _searchLayer);
        if (colliders.Length > 0)
        {
            return true;
        }
        return false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsOpen && TopCheck())
        {
            IsOpen = true;
            GameManager.Instance.OpenSameNumberItem(_Number);
            _light.GetComponent<Light>().color = Color.green;
            _meshRenderer.material.color = Color.green;
        }
        else if (!_stateLock && IsOpen && !TopCheck())
        {
            IsOpen = false;
            GameManager.Instance.CloseSameNumberItem(_Number);
            _light.GetComponent<Light>().color = Color.red;
            _meshRenderer.material.color = Color.red;
        }
    }
    
    void OnTriggerExit(Collider other) {
        IsOpen = false;
        GameManager.Instance.CloseSameNumberItem(_Number);
        _light.GetComponent<Light>().color = Color.red;
        _meshRenderer.material.color = Color.red;
    }
}
