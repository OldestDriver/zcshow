using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;
using System;

public class StepFourCardAgent : CardBaseAgent
{

    [SerializeField, Header("UI")] private RawImage _previewVideoPlayerHolder;//视频预览
    [SerializeField] private RectTransform _retakeRect;
    [SerializeField] private RectTransform _confirmRect;
    [SerializeField] private RectTransform _loadingContentRect;
    [SerializeField] private RectTransform _loadingRect;

    [SerializeField, Header("btn - RePhoto")] private CardBaseAgent _rephotoCardAgent;
    [SerializeField, Header("btn - Confirm")] private CardBaseAgent _confirmCardAgent;

    [SerializeField] private Color _btn_disable_Color;
    [SerializeField] private Color _btn_active_Color;


    [SerializeField, Header("Video Factory")] private VideoFactoryAgent _videoFactoryAgent;
    [SerializeField] private Texture _recordTexture;
    //[SerializeField, Header("Video Factory - No Logo")] private VideoFactoryAgent _videoFactoryNoLogoAgent;
    //[SerializeField] private Texture _recordTextureNoLogo;

    [SerializeField, Header("Preview")] private PreviewAgent _previewAgent;

     private VideoPlayer _previewVideoPlayer;


    private bool _videoIsGenerateCompleted = false;
    private bool _showResultLock = false;
    private bool _doRunWhenStartLock = false;


    private string newVideoUrl;

    //  挂载的使用操作信息
    private Action _OnUpdateHandleTimeAction;
    private Action _OnKeepOpenAction;
    private Action _OnCloseKeepOpenAction;

    private Action _OnErrorHappened;

    private void Reset()
    {
        _showResultLock = false;
        _doRunWhenStartLock = false;
        _videoIsGenerateCompleted = false;

        // 预处理UI

    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        _OnKeepOpenAction.Invoke();

        Text[] texts = _confirmRect.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            t.color = _btn_disable_Color;
            Debug.Log("T : " + t.text);

        }
        _confirmRect.GetComponent<Button>().interactable = false;

        _previewVideoPlayerHolder.texture = _recordTexture;

        // 此处处理视频拼接
        DoGenerateVideo();

        CompletePrepare();
    }

    public override void DoRunIn()
    {
        gameObject.SetActive(true);

        Debug.Log("场景4进入");
        // 显示在首个
        GetComponent<RectTransform>().SetAsLastSibling();
        CompleteRunIn();
    }

    /// <summary>
    /// 运行阶段
    /// </summary>
    public override void DoRun() {
        if (!_videoIsGenerateCompleted)
        {

            // 中间视频预览加载中
            //DoLoading();
        }
        else {
            DoRunWhenStart();

            if (!_showResultLock) {
                _showResultLock = true;
                ShowResult();
            }
        }

    }

    /// <summary>
    ///     生命周期 - Do Run Out
    /// </summary>
    public override void DoRunOut()
    {
        if (_previewVideoPlayer != null) {

            if (_previewVideoPlayer.isPlaying)
            {
                _previewVideoPlayer.Stop();

                Destroy(GetComponent<VideoPlayer>());
            }
        }


       
        CompleteRunOut();
    }


    /// <summary>
    /// 结束阶段
    /// </summary>
    public override void DoEnd() {
        gameObject.SetActive(false);
        Debug.Log("场景4结束");
        _NextCard.DoActive();
    }


    // 点击确认
    public void OnClickConfirm() {
        _OnUpdateHandleTimeAction.Invoke();

        nextCard = _confirmCardAgent;

        //_videoFactoryAgent.StopPlayVideo();

        //_previewAgent.UpdateVideo(_videoFactoryAgent.GetVideoAddress(),_videoFactoryNoLogoAgent.GetVideoAddress());

        DoRunOut();
    }


    // 点击重拍
    public void OnClickRePhoto()
    {
        _OnUpdateHandleTimeAction.Invoke();

        nextCard = _rephotoCardAgent;

        _videoFactoryAgent.StopPlayVideoAndClear();

        DoRunOut();
    }


    private void DoRunWhenStart() {
        if (!_doRunWhenStartLock) {
            _doRunWhenStartLock = true;

            Text[] texts = _confirmRect.GetComponentsInChildren<Text>();
            foreach (Text t in texts)
            {
                t.DOColor(_btn_active_Color, 0.5f);
            }

            _confirmRect.GetComponent<Button>().interactable = true;
        }
    }



    private void ShowResult() {
        PlayNewVideo();

        _OnCloseKeepOpenAction.Invoke();


    }

    /// <summary>
    /// 生成视频
    /// </summary>
    private void DoGenerateVideo() {

        _videoFactoryAgent.DoActive(OnVideoGenerate);

    }

    void OnVideoGenerate(string videoUrl)
    {
        Debug.Log("Video 生成完成 ： " + videoUrl);

        if (gameObject.activeSelf) {
            if (videoUrl != null)
            {
                newVideoUrl = videoUrl;

                _previewVideoPlayer = gameObject.AddComponent<VideoPlayer>();
                _previewVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                _previewVideoPlayer.EnableAudioTrack(0, false);

                _previewVideoPlayer.source = VideoSource.Url;
                _previewVideoPlayer.url = newVideoUrl;
                _previewVideoPlayer.isLooping = true;

                _videoIsGenerateCompleted = true;
            }
        }
    }


    // 播放新视频

    public void PlayNewVideo() {

        Debug.Log("播放新视频开始");

        StartCoroutine(PrepareVideoNewVideo());

    }

    IEnumerator PrepareVideoNewVideo()
    {

        _previewVideoPlayer.Prepare();
        while (!_previewVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }

        Debug.Log("准备新的视频成功");

        _previewVideoPlayerHolder.texture = _previewVideoPlayer.texture;
        _previewVideoPlayer.Play();

        //StartRecord();
    }


    public override void OnUpdateHandleTime(Action action)
    {
        _OnUpdateHandleTimeAction = action;
    }

    public override void OnKeepOpen(Action action)
    {
        _OnKeepOpenAction = action;
    }

    public override void OnCloseKeepOpen(Action action)
    {
        _OnCloseKeepOpenAction = action;
    }

    public override void OnErrorHappend(Action action)
    {
        _OnErrorHappened = action;
    }
}
