using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SCENEINDEX{
    TITLE = 0,
    STAGE1_1 = 1,
    STAGE1_2 = 2,
    STAGE1_3 = 3
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject _clearArea;  //クリアエリア
    [SerializeField] FadeInOut _fadeInOut; //シーン移行
    [SerializeField] List<GameObject> _targetGroup; //レーザー目標
    [SerializeField] List<GameObject> _movingFloorGroup; //移動地面
    [SerializeField] List<GameObject> _laserGroup; //レーザー
    [SerializeField] List<GameObject> _buttonGroup; //ボタン

    public bool _StageClear => _stageClear;
    public SCENEINDEX _sceneindex;
    bool _stageClear;
    float _resetTimer;

    void Awake()
    {
        //シングルトーン
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        _targetGroup = new List<GameObject>();
        _movingFloorGroup = new List<GameObject>();
        _laserGroup = new List<GameObject>();
        _buttonGroup = new List<GameObject>();
    }

    void Start()
    {
        BGMAudioSetting();
    }

    void BGMAudioSetting()
    {
        _sceneindex = (SCENEINDEX)SceneManager.GetActiveScene().buildIndex;
        switch (_sceneindex)
        {
            case SCENEINDEX.TITLE:
                AudioManager.PlayTitleBGMAudio();
                break;
            case SCENEINDEX.STAGE1_1:
            case SCENEINDEX.STAGE1_2:
            case SCENEINDEX.STAGE1_3:
                AudioManager.PlayStageBGMAudio();
                break;
        }
    }

    //シーンエフェクト
    public void RestigerFadeInOut(FadeInOut fade)
    {
        _fadeInOut = fade;
    }

    //アイテム登録
    public void RestigerTarget(GameObject target)
    {
        if(!_targetGroup.Contains(target))
        {
            _targetGroup.Add(target);
        }

        StageClearCheck();
    }
    public void RegisterMovingFloor(GameObject floor)
    {
        if(!_movingFloorGroup.Contains(floor))
        {
            _movingFloorGroup.Add(floor);
        }
    }
    public void RegisterLaser(GameObject laser)
    {
        if (!_laserGroup.Contains(laser))
        {
            _laserGroup.Add(laser);
        }
    }
    public void RegisterButton(GameObject button)
    {
        if (!_buttonGroup.Contains(button))
        {
            _buttonGroup.Add(button);
        }
    }
    public void RegisterClearArea(GameObject clearArea)
    {
        _clearArea = clearArea;
    }

    //アイテム削除
    public void RemoveTarget(GameObject target)
    {
        if (_targetGroup.Contains(target))
        {
            _targetGroup.Remove(target);
        }

        //クリアチェック
        StageClearCheck();
    }

    //対応番号のアイテムを起動する
    public void OpenSameNumberItem(int num)
    {
        foreach (var item in _movingFloorGroup)
        {
            if (item.GetComponent<StageGimmick>()._Number == num)
            {
                item.GetComponent<AutomaticSlider>().enabled = true;
                item.GetComponent<StageGimmick>().Open();
            }
        }

        foreach (var item in _laserGroup)
        {
            if(item.GetComponent<StageGimmick>()._Number == num)
            {
                item.GetComponent<StageGimmick>().Open();
            }
        }
    }

    //対応番号のアイテムを終了する
    public void CloseSameNumberItem(int num)
    {
        foreach (var item in _movingFloorGroup)
        {
            if (item.GetComponent<StageGimmick>()._Number == num)
            {
                item.GetComponent<StageGimmick>().Close();
                item.GetComponent<AutomaticSlider>().enabled = false;
            }
        }

        foreach (var item in _laserGroup)
        {
            if (item.GetComponent<StageGimmick>()._Number == num)
            {
                item.GetComponent<StageGimmick>().Close();
            }
        }
    }

    /// <summary>
    /// 同じ番号のボタンを取得する
    /// </summary>
    /// <param name="num"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool GetSameNumberButton(int num, out List<GameObject> items)
    {
        items = new List<GameObject>();
        foreach (var button in _buttonGroup)
        {
            if (button.GetComponent<StageGimmick>()._Number == num)
            {
                items.Add(button);
            }
        }

        if (items.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //クリア判定
    void StageClearCheck()
    {
        if(_clearArea == null) { return; }

        if (_targetGroup.Count == 0)
        {
            _clearArea.GetComponent<StageGimmick>().Open();
            AudioManager.PlayMagicCircleAudio();
        }
        else
        {
            _clearArea.GetComponent<StageGimmick>().Close();
        }
    }

    public void StageClear()
    {
        Debug.Log("Clear");
        _fadeInOut.StartFadeOut();
        _stageClear = true;
    }

    public void ResetGameStart()
    {
        _fadeInOut._Reset = true;
    }

    public void ResetGameStop()
    {
        _fadeInOut._Reset = false;
    }
}
