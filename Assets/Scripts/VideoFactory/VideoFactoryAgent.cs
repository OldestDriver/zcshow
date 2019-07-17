using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using NatCorderU.Core;


public class VideoFactoryAgent : MonoBehaviour
{
    // 视频幕布
    [SerializeField, Header("Video 1")] RawImage _image1;
    [SerializeField] VideoPlayer _videoPlayer1;

    [SerializeField, Header("Video 2")] RawImage _image2;
    [SerializeField] VideoPlayer _videoPlayer2;

    [SerializeField, Header("Video 3")] RawImage _image3;
    [SerializeField] VideoPlayer _videoPlayer3;

    [SerializeField, Header("Camera")] Camera _camera;
    [SerializeField] Animator _animator;

    [SerializeField] Image[] content1s = new Image[2];  //
    [SerializeField] Image[] content2s = new Image[2];
    [SerializeField] Image[] content3s = new Image[2];


    [SerializeField] bool _record;

    private bool _video1Prepared = false;
    private bool _video2Prepared = false;
    private bool _video3Prepared = false;

    private bool _sourceVideoIsPrepared = false;  // 视频源标识符

    private bool _videoIsPrepared = false;  // 视频加载情况标识符

    private List<string> _photos = new List<string>();   // 照片集合

    private VideoFactoryStatus _videoFactoryStatus = VideoFactoryStatus.Disabled;

    private Action<string> _videoGenerateCompletedCallback;
    private string _videoAddress;


    enum VideoFactoryStatus {
        Disabled,
        Prepare,
        Active,
        Running,
        Finish
    }

    private bool doPreparedLock = false;
    private bool doRunLock = false;
    private bool doActiveLock = false;
    private bool doFinishLock = false;



    public void PlaySecnarioOne() {
        _videoPlayer1.Play();

        Debug.Log("Play Secnario One");
    }

    public void StopSecnarioOne()
    {
        _videoPlayer1.Stop();

        Debug.Log("Stop Secnario One");
    }

    public void PlaySecnarioTwo()
    {
        _image2.texture = _videoPlayer2.texture;

        _videoPlayer2.Play();
        Debug.Log("Play Secnario One");
    }

    public void StopSecnarioTwo()
    {
        _videoPlayer2.Stop();
        Debug.Log("Stop Secnario One");
    }

    public void PlaySecnarioFive()
    {
        _image3.texture = _videoPlayer3.texture;

        _videoPlayer3.Play();
        Debug.Log("Play Secnario One");
    }

    public void StopSecnarioFive()
    {
        _videoPlayer3.Stop();
        Debug.Log("Stop Secnario One");
    }


    private void Reset() {
        doPreparedLock = false;
        doRunLock = false;
        doActiveLock = false;
        doFinishLock = false;
        _videoAddress = "";

        _videoFactoryStatus = VideoFactoryStatus.Disabled;
    }


    /// <summary>
    ///     激活视频工厂
    /// </summary>
    public void DoActive(Action<string> completeCallBack) {
        Reset();

        _videoGenerateCompletedCallback = completeCallBack;

        _videoFactoryStatus = VideoFactoryStatus.Prepare;
    }



    // Start is called before the first frame update
    void Start()
    {
        Reset();

        if (!_videoIsPrepared)
        {
            //LoadVideo();
        }
        else
        {
            //CompletePrepare();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            StopRecord();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 开始动画
            //_animator.Play("Play");
            DoActive(DoAfterCompleted);

        }

        if (_videoFactoryStatus == VideoFactoryStatus.Prepare)
        {
            if (!doPreparedLock) {
                doPreparedLock = true;

                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

                Sprite sprite = null, sprite2 = null, sprite3 = null;

                //// 获取三张照片 / 处理照片 
                //for (int i = 0; i < _photos.Count; i++)
                //{
                //    string photoStr = _photos[i];
                //}

                //// 将图片赋值至控件
                //for (int i = 0; i < content1s.Length; i++)
                //{
                //    content1s[i].sprite = sprite;
                //}

                //for (int i = 0; i < content2s.Length; i++)
                //{
                //    content2s[i].sprite = sprite2;
                //}

                //for (int i = 0; i < content3s.Length; i++)
                //{
                //    content3s[i].sprite = sprite3;
                //}

                //  准备好视频
                LoadVideo();
            }

            if (!_sourceVideoIsPrepared)
            {
                CheckVideoStatus();
            }

            if (_sourceVideoIsPrepared)
            {
                Debug.Log("_sourceVideoIsPrepared");

                _videoFactoryStatus = VideoFactoryStatus.Running;
            }
        }



        if (_videoFactoryStatus == VideoFactoryStatus.Active) {

            if (!doActiveLock) {
                doActiveLock = true;

                _videoFactoryStatus = VideoFactoryStatus.Running;
            }

        }



        if (_videoFactoryStatus == VideoFactoryStatus.Running) {
            if (!doRunLock)
            {
                doRunLock = true;

                if (_record)
                {
                    //  开始录屏
                    StartRecord();
                }

                //  开始动画
                _animator.Play("Play");

            }
        }

        if (_videoFactoryStatus == VideoFactoryStatus.Finish)
        {
            if (!doFinishLock)
            {
                doFinishLock = true;

                if (_record)
                {
                    //  结束录屏
                    StopRecord();

                    //  生成视频文件 _videoAddress
                }

                _videoGenerateCompletedCallback.Invoke(_videoAddress);


                //  结束并重置
                Reset();

            }
        }

        // 判断动画状态
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        //Debug.Log("stateInfo.fullPathHash : " + stateInfo.fullPathHash);
        //Debug.Log("stateInfo.normalizedTime : " + stateInfo.normalizedTime);

    }


    private void LoadVideo()
    {
        Debug.Log("加载视频开始");

        StartCoroutine(PrepareVideo1());
        StartCoroutine(PrepareVideo2());
        StartCoroutine(PrepareVideo3());
    }

    //step1
    //播放视频
    IEnumerator PrepareVideo1()
    {
        _videoPlayer1.Prepare();
        while (!_videoPlayer1.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _video1Prepared = true;
        _image1.texture = _videoPlayer1.texture;

        //StartRecord();
    }

    IEnumerator PrepareVideo2()
    {
        _videoPlayer2.Prepare();
        while (!_videoPlayer2.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _video2Prepared = true;
        _image2.texture = _videoPlayer2.texture;

        //StartRecord();
    }

    IEnumerator PrepareVideo3()
    {
        _videoPlayer3.Prepare();
        while (!_videoPlayer3.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _video3Prepared = true;
        _image3.texture = _videoPlayer3.texture;

        //StartRecord();
    }



    private void CheckVideoStatus(){
        if (_video1Prepared && _video2Prepared && _video3Prepared)
        {
            _sourceVideoIsPrepared = true;
        }
        else {
            _sourceVideoIsPrepared = false;
        }
    }



    /// <summary>
    ///     开始录屏
    /// </summary>
    private void StartRecord() {


        // Create a recording configuration
        const float DownscaleFactor = 2f / 3;

        int w = (int)(Screen.width * DownscaleFactor);
        int h = (int)(Screen.height * DownscaleFactor);

        Debug.Log("W : " + w + " | H : " + h);


        var configuration = new Configuration(w, h);
        // Start recording with microphone audio
        //if (recordMicrophoneAudio)
        //{
        //    // Start the microphone
        //    audioSource.clip = Extensions.Microphone.StartRecording();
        //    audioSource.loop = true;
        //    audioSource.Play();
        //    // Start recording with microphone audio
        //    Replay.StartRecording(Camera.main, configuration, OnReplay, audioSource, true);
        //}
        // Start recording without microphone audio
        Replay.StartRecording(_camera, configuration, OnReplay);


    }

    /// <summary>
    ///     停止录屏
    /// </summary>
    private void StopRecord() {

        // Stop playing mic audio
        //if (recordMicrophoneAudio)
        //{
        //    audioSource.Stop();
        //    Extensions.Microphone.StopRecording();
        //}
        // Stop recording
        Replay.StopRecording();

    }

    public void DoAfterCompleted(string str) {
        Debug.Log("Do After Completed");
    }


    void OnReplay(string path)
    {
        Debug.Log("Saved recording to: " + path);
#if UNITY_IOS || UNITY_ANDROID
            // Playback the video
            Handheld.PlayFullScreenMovie(path);
#endif
    }

}
