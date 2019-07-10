using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;

public class StepFiveCardAgent : CardBaseAgent
{

    [SerializeField, Header("UI")] private InputField emailInput;
    [SerializeField] private RectTransform _loadingContentRect;
    [SerializeField] private RectTransform _loadingRect;
    [SerializeField] private RectTransform _resultRect;


    private bool _erCodeIsGenerated = false;
    private bool _showResultLock = false;
    private bool _sendingEmail = false;


    private void Reset()
    {
        _erCodeIsGenerated = false;
        _showResultLock = false;
        _sendingEmail = false;

        _resultRect.gameObject.SetActive(false);
        _loadingRect.gameObject.SetActive(true);

    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {

        Reset();

        // 获取小程序码
        GetErCode();

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
        if (!_erCodeIsGenerated)
        {
            // 中间视频预览加载中
            DoLoading();
        }
        else {
            if (!_showResultLock)
            {
                _showResultLock = true;
                ShowResult();
            }
        }

    }

    public override void DoRunOut()
    {
        CompleteRunOut();
    }


    /// <summary>
    /// 结束阶段
    /// </summary>
    public override void DoEnd() {
        gameObject.SetActive(false);
        Debug.Log("场景4结束");
    }


    // 点击发送邮件
    public void OnClickSendEmail() {
        DoSendEmail();
    }



    /// <summary>
    /// 加载动画
    /// </summary>
    private void DoLoading() {

        Debug.Log("DoLoading in five");

        float fillAmount = _loadingContentRect.GetComponent<Image>().fillAmount;
        fillAmount = fillAmount + 0.01f;
        if (fillAmount > 1f)
        {
            fillAmount = 0;
        }
        _loadingContentRect.GetComponent<Image>().fillAmount = fillAmount;

    }


    private void ShowResult() {
        _resultRect.gameObject.SetActive(true);
        _loadingRect.gameObject.SetActive(false);
    }


    /// <summary>
    /// 获取小程序码
    /// </summary>
    private void GetErCode() {

        // 上传视频功能

        // 小程序码获取功能
        StartCoroutine(DoMockGenerate());

    }

    IEnumerator DoMockGenerate()
    {

        Debug.Log("获取小程序码过程");
        yield return new WaitForSeconds(5);
        _erCodeIsGenerated = true;
    }


    private void DoSendEmail() {
        _sendingEmail = true;

        StartCoroutine(DoMockSendEmail());
    }

    IEnumerator DoMockSendEmail()
    {

        Debug.Log("模拟发送邮件");
        yield return new WaitForSeconds(3);
        _sendingEmail = false;
    }

}
