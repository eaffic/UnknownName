using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeInOut : MonoBehaviour
{
    public string _SceneToLoad; //シーン名
    public float _TransitionSpeed; //エフェクト速度
    public float _ResetTime;
    public bool _Reset{ get; set; }
    public Image _ResetImage;

    Image _theImage;
    bool _shouldReveal;
    float _resetTimer;

    void Start()
    {
        try{
            GameManager.Instance.RestigerFadeInOut(this);
        }catch{}
        
        _theImage = GetComponent<Image>();
        _theImage.material.SetFloat("_Cutoff", -0.3f);
        _shouldReveal = true;
    }

    void Update()
    {
        if (_Reset) { ResetScene(); }
        else { _resetTimer = 0; _ResetImage.fillAmount = 0; }

        if (_shouldReveal)
        {
            //FadeIn
            _theImage.material.SetFloat("_Cutoff", Mathf.MoveTowards(_theImage.material.GetFloat("_Cutoff"), 1.1f, _TransitionSpeed * Time.deltaTime));
        }
        else
        {
            //FadeOut
            _theImage.material.SetFloat("_Cutoff", Mathf.MoveTowards(_theImage.material.GetFloat("_Cutoff"), -1f, _TransitionSpeed * Time.deltaTime));

            if (_theImage.material.GetFloat("_Cutoff") <= -0.1f - _theImage.material.GetFloat("_Smoothing"))
            {
                AudioManager.StopMusicAudio();
                AudioManager.StopPlayerAudio();
                AudioManager.StopFXAudio();
                AudioManager.StopAmbientAudio();
                SceneManager.LoadScene(_SceneToLoad);
            }
        }
    }

    //----------------------------------------------------
    public void StartFadeOut()
    {
        _shouldReveal = false;
    }

    public void ResetScene()
    {
        _resetTimer += Time.fixedDeltaTime;
        _ResetImage.fillAmount = _resetTimer / _ResetTime;

        if (_resetTimer > _ResetTime)
        {
            _SceneToLoad = SceneManager.GetActiveScene().name;
            StartFadeOut();
        }
    }
}
