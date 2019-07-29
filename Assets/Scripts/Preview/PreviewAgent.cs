using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PreviewAgent : MonoBehaviour
{
    [SerializeField,Header("Screen")] RawImage _leftScreen;
    [SerializeField] RawImage _left2Screen;
    [SerializeField] RawImage _rightScreen;
    [SerializeField] RawImage _right2Screen;
    [SerializeField] RawImage _screen;

    [SerializeField] RectTransform _mainNewVideoPlayerRect;
    [SerializeField] RectTransform _subNewVideoPlayerRect;

    [SerializeField] VideoPlayResAgent _videoPlayResAgent;

    VideoPlayer _mainNewVideoPlayer;
    VideoPlayer _subNewVideoPlayer;

    [SerializeField] int loopTime;
    [SerializeField,Header("Is Mock")] bool _isMock;


    bool _mainIsPrepared = false;
    bool _subIsParpared = false;
    bool _mainNewIsPrepared = false;
    bool _subNewIsParpared = false;
    bool _playLock = false;
    bool _playLockNew = false;
    bool _stopNewVideoLock = false;
    bool _onRepreparedDefault = false;

    bool _onLoopLock = false;

    bool _showNew = false;  
    int _loop = 0;



    private VideoPlayer _mainVideoPlayer;
    private VideoPlayer _subVideoPlayer;



    private bool _defaultPlayerIsPreparing = false;


    private void Reset()
    {
        _mainIsPrepared = false;
        _subIsParpared = false;
        _mainNewIsPrepared = false;
        _subNewIsParpared = false;
        _playLock = false;
        _playLockNew = false;
    }

    private void ResetLock() {
        _stopNewVideoLock = false;
        _playLock = false;
        _playLockNew = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        _videoPlayResAgent.onDoReprepareDefaultAction(onDoReprepareDefaultAction);
        //_mainNewVideoPlayer.loopPointReached += OnNewLoopReached;
    }

    void OnNewLoopReached(VideoPlayer source) {
        _loop++;

        if (_loop >= loopTime) {
            _showNew = false;

            // 销毁
            Destroy(_mainNewVideoPlayerRect.GetComponent<VideoPlayer>());
            Destroy(_subNewVideoPlayerRect.GetComponent<VideoPlayer>());

            ResetLock();
        }
    }




    // Update is called once per frame
    void Update()
    {
        if (_isMock) {
            if (Input.GetKeyDown(KeyCode.A))
            {
                string path = Application.streamingAssetsPath + "/test-result_1.mp4";
                string newPath = Application.streamingAssetsPath + "/test-result-nologo.mp4";

                UpdateVideo(path, newPath);
            }
        }


        if (!_showNew)
        {
            PlayDefaultVideo();
        }
        else {
            PlayNewVideo();
            
        }
    }


    public void UpdateVideo(string path,string pathNoLogo)
    {
        if (path == null || pathNoLogo == null) {
            Debug.Log("投屏数据缺失 ： p - " + (path == null) + "| pnl : " + (pathNoLogo == null));
        }
        else {

            ResetLock();
            _subNewIsParpared = false;
            _mainNewIsPrepared = false;

            // 初始化必要数据
            _loop = 0;

            _mainNewVideoPlayer = CreateNewVideoPlayer(path, _mainNewVideoPlayerRect,true);
            _subNewVideoPlayer = CreateNewVideoPlayer(pathNoLogo, _subNewVideoPlayerRect, false);

            StartCoroutine(LoadMainNewVideo());
            StartCoroutine(LoadSubNewVideo());
            _showNew = true;
        }
    }



    IEnumerator LoadMainNewVideo()
    {
        _mainNewVideoPlayer.Prepare();
        while (!_mainNewVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1f);
            break;
        }


        _mainNewIsPrepared = true;

    }

    IEnumerator LoadSubNewVideo()
    {
        _subNewVideoPlayer.Prepare();
        while (!_subNewVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1f);
            break;

        }
        _subNewIsParpared = true;
    }





    void PlayDefaultVideo() {
        // 当视频循环次数超过5次后，进行重置

        if (_videoPlayResAgent.IsPrepared())
        {

            if (!_playLock)
            {
                _playLock = true;

                // 翻转
                GetComponent<RectTransform>().rotation = Quaternion.Euler(0.0f, 180f, 90f);

                _mainVideoPlayer = _videoPlayResAgent.GetVideoPlayer(VideoPlayResAgent.VideoPlayerType.videoPlayerDemo);
                _subVideoPlayer = _videoPlayResAgent.GetVideoPlayer(VideoPlayResAgent.VideoPlayerType.videoPlayerDemoNoLogo);

                _screen.texture = _mainVideoPlayer.texture;

                _leftScreen.texture = _subVideoPlayer.texture;
                _left2Screen.texture = _subVideoPlayer.texture;
                _rightScreen.texture = _subVideoPlayer.texture;
                _right2Screen.texture = _subVideoPlayer.texture;

            }

            if (!_mainVideoPlayer.isPlaying) {
                _mainVideoPlayer.Play();
                _subVideoPlayer.Play();
            }

        }
        else {

            //Debug.Log("_videoPlayResAgent  is not prepared: ");

            _playLock = false;
        }
    }

    void PlayNewVideo() {
        if (_mainNewIsPrepared && _subNewIsParpared)
        {
            if (!_playLockNew)
            {
                _playLockNew = true;

                GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0.0f, 90f);


                _videoPlayResAgent.StopPlayer(VideoPlayResAgent.VideoPlayerType.videoPlayerDemo);
                _videoPlayResAgent.StopPlayer(VideoPlayResAgent.VideoPlayerType.videoPlayerDemoNoLogo);

                _leftScreen.texture = _subNewVideoPlayer.texture;
                _left2Screen.texture = _subNewVideoPlayer.texture;
                _rightScreen.texture = _subNewVideoPlayer.texture;
                _right2Screen.texture = _subNewVideoPlayer.texture;
                _screen.texture = _mainNewVideoPlayer.texture;

                _subNewVideoPlayer.Play();
                _mainNewVideoPlayer.Play();

                // 提前让原Video Player 准备
                _defaultPlayerIsPreparing = false;
                _videoPlayResAgent.PrepareDefaultPlayer();

                _playLockNew = false;
            }
        }
    }

    VideoPlayer CreateNewVideoPlayer(string path,RectTransform parentComponent,bool addLoopEvent) {
        VideoPlayer videoPlayer = parentComponent.gameObject.AddComponent<VideoPlayer>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.EnableAudioTrack(0, false);
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = path;
        videoPlayer.isLooping = true;
        if (addLoopEvent) {
            videoPlayer.loopPointReached += OnNewLoopReached;
        }
        return videoPlayer;
    }


    void PrepareDefaultSuccess() {
        //_defaultLoopTime = 0;
        _defaultPlayerIsPreparing = true;
        _playLock = false;
    }


    private void onDoReprepareDefaultAction() {
        _onRepreparedDefault = true;
        if (!_showNew) {
            _playLock = false;

            PlayDefaultVideo();
        }

    }

}