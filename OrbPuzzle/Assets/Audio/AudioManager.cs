using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] float _volumeChangeSpeed = 0.02f;

    [Header("BGM")]
    public AudioClip _TitleBGMClip;
    public AudioClip _SelectBGMClip;
    public AudioClip _StageBGMClip;
    public AudioClip _ResultBGMClip;

    [Header("プレイヤー効果音")]
    public AudioClip _WalkStepClips;
    public AudioClip _RunStepClips;
    public AudioClip _JumpClip;
    public AudioClip _PunchClip;

    [Header("環境音")]
    public AudioClip _LaserClip;

    [Header("効果音")]
    public AudioClip _OpenTargetClip;
    public AudioClip _BoxMoveClip;
    public AudioClip _BoxDownClip;
    public AudioClip _MagicCircle;


    //各音声用のオーディオソース
    AudioSource _musicSource;
    AudioSource _playerSource;
    AudioSource _ambientSource;
    AudioSource _fxSource; //効果音

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        //オーディオソースコンポーネント追加
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _playerSource = gameObject.AddComponent<AudioSource>();
        _ambientSource = gameObject.AddComponent<AudioSource>();
        _fxSource = gameObject.AddComponent<AudioSource>();

        
    }

    #region BGM関係
    public static void PlayTitleBGMAudio()
    {
        StopMusicAudio();
        Instance._musicSource.clip = Instance._TitleBGMClip;
        Instance._musicSource.loop = true;
        Instance._musicSource.Play();
    }
    public static void PlaySelectBGMAudio()
    {
        StopMusicAudio();
        Instance._musicSource.clip = Instance._SelectBGMClip;
        Instance._musicSource.loop = true;
        Instance._musicSource.Play();
    }
    public static void PlayStageBGMAudio()
    {
        StopMusicAudio();
        Instance._musicSource.clip = Instance._StageBGMClip;
        Instance._musicSource.loop = true;
        Instance._musicSource.Play();
    }
    public static void PlayResultBGMAudio()
    {
        StopMusicAudio();
        Instance._musicSource.clip = Instance._ResultBGMClip;
        Instance._musicSource.Play();
    }
    #endregion

    #region プレイヤー関係
    public static void PlayWalkStepAudio()
    {
        StopPlayerAudio();
        Instance._playerSource.clip = Instance._WalkStepClips;
        Instance._playerSource.loop = true;
        Instance._playerSource.Play();
    }
    public static void PlayRunStepAudio()
    {
        StopPlayerAudio();
        Instance._playerSource.clip = Instance._RunStepClips;
        Instance._playerSource.loop = true;
        Instance._playerSource.Play();
    }
    public static void PlayJumpAudio()
    {
        StopPlayerAudio();
        Instance._playerSource.PlayOneShot(Instance._JumpClip);
    }
    public static void PlayPunchAudio()
    {
        StopPlayerAudio();
        Instance._playerSource.PlayOneShot(Instance._PunchClip);
    }
    #endregion

    #region 環境音、エフェクト
    public static void PlayMagicCircleAudio()
    {
        StopAmbientAudio();
        Instance._ambientSource.PlayOneShot(Instance._MagicCircle);
    }
    public static void PlayLaserAudio()
    {
        StopAmbientAudio();
        Instance._ambientSource.PlayOneShot(Instance._LaserClip);
    }
    #endregion

    #region SE関係
    public static void PlayBoxMoveAudio()
    {
        StopFXAudio();
        Instance._fxSource.PlayOneShot(Instance._BoxMoveClip);
    }
    public static void PlayBoxDownAudio()
    {
        StopFXAudio();
        Instance._fxSource.PlayOneShot(Instance._BoxDownClip);
    }
    public static void PlayOpenTarget()
    {
        StopFXAudio();
        Instance._fxSource.PlayOneShot(Instance._OpenTargetClip);
    }
    #endregion

    public static void StopMusicAudio()
    {
        Instance._musicSource.loop = false;
        Instance._musicSource.Stop();
    }
    public static void StopPlayerAudio()
    {
        Instance._playerSource.loop = false;
        Instance._playerSource.Stop();
    }
    public static void StopAmbientAudio()
    {
        Instance._ambientSource.loop = false;
        Instance._ambientSource.Stop();
    }
    public static void StopFXAudio()
    {
        Instance._fxSource.loop = false;
        Instance._fxSource.Stop();
        
    }

    IEnumerator DecreaseVolumetoClose(AudioSource audio_)
    {
        if (audio_.volume > 0.01f)
        {
            audio_.volume = Mathf.Min(0, audio_.volume - _volumeChangeSpeed);
            yield return new WaitForSeconds(0.01f);
        }

        audio_.Stop();
        // audio_.clip = nextClip;

        // if(audio_.volume < 0.99f)
        // {
        //     audio_.volume = Mathf.Max(0, audio_.volume + _volumeChangeSpeed);
        //     yield return new WaitForSeconds(0.01f);
        // }
        yield return null;
    }
}
