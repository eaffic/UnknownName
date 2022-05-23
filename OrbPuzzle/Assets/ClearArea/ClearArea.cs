using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ClearArea : StageGimmick
{
    [SerializeField] GameObject _magicCircle;
    [SerializeField] PlayableDirector _timeline;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterClearArea(this.gameObject);
        TryGetComponent(out _timeline);
    }

    private void OnTriggerEnter(Collider other) {
        if(IsOpen)
        {
            if(other.gameObject.tag == "Player")
            {
                AudioManager.PlayResultBGMAudio();
                GameManager.Instance.StageClear();
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.GetComponent<PlayerAnimation>().SetClearAnimation();
            }
        }
    }

    public override void Open()
    {
        IsOpen = true;
        _timeline.enabled = true;
        AudioManager.PlayMagicCircleAudio();
    }

    public override void Close()
    {
        IsOpen = false;
        _timeline.Stop();
        _timeline.enabled = false;
        _magicCircle.SetActive(false);
    }
}
