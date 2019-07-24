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
    [SerializeField] VideoPlayer _mainVideoPlayerNew;
    [SerializeField] VideoPlayer _subVideoPlayerNew;


    bool _mainIsPrepared = false;
    bool _subIsParpared = false;
    bool _mainNewIsPrepared = false;
    bool _subNewIsParpared = false;
    bool _playLock = false;
    bool _playLockNew = false;

    bool _showNew = false;

    private void Reset()
    {
        _mainIsPrepared = false;
        _subIsParpared = false;
        _mainNewIsPrepared = false;
        _subNewIsParpared = false;
        _playLock = false;
        _playLockNew = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadMainVideo());
        StartCoroutine(LoadSubVideo());
    }


    // Update is called once per frame
    void Update()
    {
        if (_mainIsPrepared && _subIsParpared) {
            if (!_playLock) {
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

        if (_mainNewIsPrepared && _subNewIsParpared && _showNew)
        {
            if (!_playLockNew)
            {
                _playLockNew = true;
                _mainVideoPlayer.Stop();
                _subVideoPlayer.Stop();

                Debug.Log("Play2");


                _leftScreen.texture = _subVideoPlayerNew.texture;
                _left2Screen.texture = _subVideoPlayerNew.texture;
                _rightScreen.texture = _subVideoPlayerNew.texture;
                _right2Screen.texture = _subVideoPlayerNew.texture;

                _screen.texture = _mainVideoPlayerNew.texture;


                _subVideoPlayerNew.Play();
                _mainVideoPlayerNew.Play();
            }
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
        Debug.Log("Path : " + path);
        Debug.Log("pathNoLogo : " + pathNoLogo);

        if (path == null || pathNoLogo == null) { }
        else {
            _mainVideoPlayerNew.url = path;
            StartCoroutine(LoadMainNewVideo());

            _subVideoPlayerNew.url = pathNoLogo;
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
        _mainVideoPlayerNew.Prepare();
        while (!_mainVideoPlayerNew.isPrepared)
        {
            yield return new WaitForSeconds(1f);
            break;
        }


        _mainNewIsPrepared = true;

    }

    IEnumerator LoadSubNewVideo()
    {
        _subVideoPlayerNew.Prepare();
        while (!_subVideoPlayerNew.isPrepared)
        {
            yield return new WaitForSeconds(1f);
            break;

        }
        _subNewIsParpared = true;
    }
}