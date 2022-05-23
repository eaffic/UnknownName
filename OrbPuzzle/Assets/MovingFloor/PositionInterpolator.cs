using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInterpolator : MonoBehaviour
{
    [SerializeField] Rigidbody body = default;
    [Tooltip("原点"), SerializeField] Vector3 from = default;
    [Tooltip("目標点"), SerializeField] Vector3 to = default;
    [SerializeField] Transform relativeTo = default; //現在地(参考目標)

    void Start()
    {
        TryGetComponent(out body);
    }

    public void Interpolate(float t)
    {
        Vector3 p;
        if (relativeTo)
        {
            //参考目標のローカル座標に変換する(調整しやすい)
            p = Vector3.LerpUnclamped(relativeTo.TransformPoint(from), relativeTo.TransformPoint(to), t);
        }
        else
        {
            p = Vector3.LerpUnclamped(from, to, t);
        }
        body.MovePosition(p);
    }
}
