using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : StageGimmick
{
    [SerializeField] GameObject _startBeam; //エフェクト
    [SerializeField] GameObject _startPartical; //エフェクト
    [SerializeField] GameObject _endBeam;   //エフェクト
    [SerializeField] GameObject _endPartical;   //エフェクト
    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] LayerMask _blockLayer; //障害物レイヤー
    [SerializeField] float _laserDistance; //レーザー長さ
    [SerializeField, Range(0, 10)] int _maxReflectCount = 5; //反射可能回数


    GameObject _hitObject;
    GameObject _hitTarget;
    
    float _rayDistance;

    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        GameManager.Instance.RegisterLaser(this.gameObject);

        if(_Number == -1)
        {
            IsOpen = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen)
        {
            OnRay();
            CheckTarget();
        }
    }

    void OnRay()
    {
        ResetLaser();

        Vector3 direction = transform.forward;
        Vector3 pos = transform.position;

        RaycastHit hitInfo;
        //反射射線記録リスト
        List<Ray> rays = new List<Ray>();
        List<Vector3> laserPoint = new List<Vector3>(); //レイの経過、転向ポイント
        rays.Add(new Ray(pos, transform.forward)); //初期射線
        laserPoint.Add(pos);
        _lineRenderer.SetPosition(0, pos);

        int reflectCount = 0;
        for (int i = 0; i < rays.Count; ++i)
        {
            //射線判定
            if (Physics.Raycast(rays[i], out hitInfo, _laserDistance, _blockLayer))
            {
                //障害物位置記録(エフェクト位置設定のため)
                _hitObject = hitInfo.collider.gameObject;
                _lineRenderer.SetPosition(i + 1, hitInfo.point);
               
                if (hitInfo.collider.tag == "Target")
                {
                    //クリア目標
                    if (_hitObject.GetComponent<StageGimmick>()._Number == this._Number)
                    {
                        _hitTarget = _hitObject;
                        if (!_hitTarget.GetComponent<StageGimmick>().IsOpen)
                        {
                            _hitTarget.GetComponent<StageGimmick>().Open();
                        }
                    }
                }
                else if (reflectCount < _maxReflectCount && hitInfo.collider.tag == "Mirror")
                {
                    //ミラー
                    //反射方向を決める
                    direction = Vector3.Reflect((hitInfo.point - laserPoint[i]).normalized, hitInfo.normal); 
                    laserPoint.Add(hitInfo.point);
                    rays.Add(new Ray(hitInfo.point, direction)); //新しい射線を作る

                    _lineRenderer.positionCount++;
                    reflectCount++;
                    continue;
                }
                
                _endBeam.transform.position = hitInfo.point;
                _endPartical.transform.position = hitInfo.point;

                //反射があったら、エフェクトの角度を調整する
                if(rays.Count > 0)
                {
                    _endPartical.transform.LookAt(rays[i].origin);
                }
            }
            else
            {
                //何も当たらない
                //エフェクトは最大距離で設定する
                _hitObject = null;
                _lineRenderer.SetPosition(i + 1, laserPoint[i] + direction * _laserDistance);

                _endBeam.transform.position = laserPoint[i] + direction * _laserDistance;
                _endPartical.transform.position = laserPoint[i] + direction * _laserDistance;

                //反射がある場合、粒子エフェクトの角度を調整する
                if(rays.Count > 0)
                {
                    _endPartical.transform.LookAt(hitInfo.point);
                }
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction * _laserDistance, Color.green, 0.1f);
        }
    }

    void ResetLaser()
    {
        //laserPoint.Clear();
        _lineRenderer.positionCount = 2;
    }

    void CheckTarget()
    {
        if (_hitTarget == null) return;

        if (_hitTarget != _hitObject)
        {
            _hitTarget.GetComponent<StageGimmick>().Close();
            _hitTarget = null;
        }
    }

    void SetParticalEffect()
    {
        _startPartical.SetActive(!_startPartical.activeSelf);
        _startBeam.SetActive(!_startBeam.activeSelf);
        _endPartical.SetActive(!_endPartical.activeSelf);
        _endBeam.SetActive(!_endBeam.activeSelf);
    }
}
