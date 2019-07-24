using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PreviewAgent : MonoBehaviour
{
    [SerializeField] RawImage _leftScreen;
    [SerializeField] RawImage _left2Screen;
    [SerializeField] RawImage _rightScreen;
    [SerializeField] RawImage _right2Screen;
    [SerializeField] RawImage _screen;

    [SerializeField] VideoPlayer _mainVideoPlayer;
    [SerializeField] VideoPlayer _subVideoPlayer;
    [SerializeField] VideoPlayer _mainNewVideoPlayer;
    [SerializeField] VideoPlayer _subNewVideoPlayer;

    [SerializeField] int loopTime;

    [SerializeField,Header("Is Mock")] bool _isMock;


    bool _mainIsPrepared = false;
    bool _subIsParpared = false;
    bool _mainNewIsPrepared = false;
    bool _subNewIsParpared = false;
    bool _playLock = false;
    bool _playLockNew = false;
    bool _stopNewVideoLock = false;

    bool _showNew = false;  
    int _loop = 0;

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
        StartCoroutine(LoadMainVideo());
        StartCoroutine(LoadSubVideo());

        _mainNewVideoPlayer.loopPointReached += OnNewLoopReached;
    }

    void OnNewLoopReached(VideoPlayer source) {
        Debug.Log("loop!");
        _loop++;

        if (_loop == 2) {
            _showNew = false;
            ResetLock();
        }
    }


    // Update is called once per frame
    void Update()
    {


        if (!_showNew)
        {
            PlayDefaultVideo();

        }
        else {
            if (_mainNewIsPrepared && _subNewIsParpared)
            {
                if (!_playLockNew)
                {
                    _playLockNew = true;
                    _mainVideoPlayer.Pause();
                    _subVideoPlayer.Pause();


                    _leftScreen.texture = _subNewVideoPlayer.texture;
                    _left2Screen.texture = _subNewVideoPlayer.texture;
                    _rightScreen.texture = _subNewVideoPlayer.texture;
                    _right2Screen.texture = _subNewVideoPlayer.texture;

                    _screen.texture = _mainNewVideoPlayer.texture;


                    _subNewVideoPlayer.Play();
                    _mainNewVideoPlayer.Play();
                }
            }
        }

        if (_isMock) {

           


        }



    }

    IEnumerator PrepareVideo()
    {
        _subVideoPlayer.Prepare();
        while (!_subVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _leftScreen.texture = _subVideoPlayer.texture;
        _left2Screen.texture = _subVideoPlayer.texture;
        _rightScreen.texture = _subVideoPlayer.texture;
        _right2Screen.texture = _subVideoPlayer.texture;
        _screen.texture = _subVideoPlayer.texture;
        _subVideoPlayer.Play();

    }

    public void UpdateVideo(string path,string pathNoLogo)
    {
        //Debug.Log("Path : " + path);
        //Debug.Log("pathNoLogo : " + pathNoLogo);

        if (path == null || pathNoLogo == null) { }
        else {
            ResetLock();

            // 初始化必要数据
            _loop = 0;

            _mainNewVideoPlayer.url = path;
            StartCoroutine(LoadMainNewVideo());

            _subNewVideoPlayer.url = pathNoLogo;
            StartCoroutine(LoadSubNewVideo());
            _showNew = true;

        }

    }

    IEnumerator LoadMainVideo()
    {
        _mainVideoPlayer.Prepare();
        while (!_mainVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }



        _mainIsPrepared = true;

    }

    IEnumerator LoadSubVideo()
    {
        _subVideoPlayer.Prepare();
        while (!_subVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1f);
            break;
        }


        _subIsParpared = true;
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
        if (_mainIsPrepared && _subIsParpared)
        {
            if (!_playLock)
            {
                _playLock = true;

                _screen.texture = _mainVideoPlayer.texture;

                _leftScreen.texture = _subVideoPlayer.texture;
                _left2Screen.texture = _subVideoPlayer.texture;
                _rightScreen.texture = _subVideoPlayer.texture;
                _right2Screen.texture = _subVideoPlayer.texture;

                _subVideoPlayer.Play();
                _mainVideoPlayer.Play();

                Debug.Log("Play1");

            }
        }
    }

}