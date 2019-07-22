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

    [SerializeField, Header("Video Factory")] private VideoFactoryAgent _videoFactoryAgent;


    private bool _videoIsGenerateCompleted = false;
    private bool _showResultLock = false;

    private void Reset()
    {
        _showResultLock = false;
        _videoIsGenerateCompleted = false;

        // 预处理UI
        _retakeRect.gameObject.SetActive(false);
        _confirmRect.gameObject.SetActive(false);

        _loadingRect.GetComponent<CanvasGroup>().alpha = 1f;


    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();



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
            DoLoading();
        }
        else {
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
        DoRunOut();
    }


    // 点击重拍
    public void OnClickRePhoto()
    {
        nextCard = _rephotoCardAgent;

        DoRunOut();
    }


    /// <summary>
    /// 加载动画
    /// </summary>
    private void DoLoading() {

        float fillAmount = _loadingContentRect.GetComponent<Image>().fillAmount;
        fillAmount = fillAmount + 0.01f;
        if (fillAmount > 1f) {
            fillAmount = 0;
        }
        _loadingContentRect.GetComponent<Image>().fillAmount = fillAmount;

    }


    private void ShowResult() {
        // 关闭加载动画
        _loadingRect.GetComponent<CanvasGroup>()
            .DOFade(0, 0.5f)
            .OnComplete(() => {
                // 显示按钮
                _retakeRect.gameObject.SetActive(true);
                _confirmRect.gameObject.SetActive(true);

                // 显示视频内容
                LoadVideo();

            });
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


        _videoIsGenerateCompleted = true;
    }


    private void LoadVideo()
    {
        StartCoroutine(PrepareVideo());
    }

    //step1
    //播放视频
    IEnumerator PrepareVideo()
    {
        _previewVideoPlayer.Prepare();
        while (!_previewVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        _previewVideoPlayerHolder.texture = _previewVideoPlayer.texture;
        _previewVideoPlayer.Play();
    }


}
