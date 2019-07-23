using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;

public class StepFourCardAgent : CardBaseAgent
{

    [SerializeField, Header("UI")] private RawImage _previewVideoPlayerHolder;//视频预览
    [SerializeField] private VideoPlayer _previewVideoPlayer;
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




    private bool _videoIsGenerateCompleted = false;
    private bool _showResultLock = false;
    private bool _doRunWhenStartLock = false;

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

        Text[] texts = _confirmRect.GetComponentsInChildren<Text>();
        foreach (Text t in texts)
        {
            t.color = _btn_disable_Color;
            Debug.Log("T : " + t.text);

        }
        _confirmRect.GetComponent<Button>().interactable = false;

        // 此处处理视频拼接
        DoGenerateVideo();

        CompletePrepare();
    }

    public override void DoRunIn()
    {
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

    public override void DoRunOut()
    {
        _previewVideoPlayer.Stop();
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
        nextCard = _confirmCardAgent;

        _videoFactoryAgent.StopPlayVideo();

        DoRunOut();
    }


    // 点击重拍
    public void OnClickRePhoto()
    {
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

    }

    /// <summary>
    /// 生成视频
    /// </summary>
    private void DoGenerateVideo() {

        _videoFactoryAgent.DoActive(OnVideoGenerate,true);

    }

    void OnVideoGenerate(string videoUrl)
    {
        Debug.Log("Video 生成完成 ： " + videoUrl);

        //_previewVideoPlayer.url = videoUrl;


        _videoIsGenerateCompleted = true;
    }




}
