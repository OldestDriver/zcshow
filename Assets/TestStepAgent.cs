using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

using NatCorderU.Core;


public class TestStepAgent : MonoBehaviour
{
    // 视频幕布
    [SerializeField, Header("UI")] RawImage _image;
    [SerializeField] VideoPlayer _videoPlayer;
    [SerializeField] Camera _camera;

    private bool _videoIsPrepared = false;  // 视频加载情况标识符

    // Start is called before the first frame update
    void Start()
    {
        if (!_videoIsPrepared)
        {
            LoadVideo();
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
    }


    private void LoadVideo()
    {
        StartCoroutine(PrepareVideo());
    }

    //step1
    //播放视频
    IEnumerator PrepareVideo()
    {
        _videoPlayer.Prepare();
        while (!_videoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _videoIsPrepared = true;

        _image.texture = _videoPlayer.texture;

        _videoPlayer.Play();

        StartRecord();

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


    void OnReplay(string path)
    {
        Debug.Log("Saved recording to: " + path);
#if UNITY_IOS || UNITY_ANDROID
            // Playback the video
            Handheld.PlayFullScreenMovie(path);
#endif
    }

}
