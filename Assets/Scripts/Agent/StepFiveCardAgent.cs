using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;
using BestHTTP;
using System;
using LitJson;
using System.IO;

public class StepFiveCardAgent : CardBaseAgent
{

    [SerializeField, Header("UI")] private InputField emailInput;
    [SerializeField] private RectTransform _loadingContentRect;
    [SerializeField] private RectTransform _loadingRect;
    [SerializeField] private RectTransform _resultRect;
    [SerializeField] private RawImage _qrCode;


    private bool _erCodeIsGenerated = false;
    private bool _showResultLock = false;
    private bool _sendingEmail = false;

    //公共参数
    private int APP_ID = 99;
    private string api = "https://api.pixelplus.cc";
    private string qiniu_api = "https://upload.qiniu.com";
    private string h5_api = "https://h5.pixelplus.cc";
    private int EVENT_ID = 49;

    private string message_id;

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


    private void ShowResult()
    {
        _resultRect.gameObject.SetActive(true);
        _loadingRect.gameObject.SetActive(false);
    }


    //流程出错
    void Error()
    {
        Debug.Log("流程内出错了！！！");
    }


    /// <summary>
    /// 获取小程序码
    /// </summary>
    private void GetErCode() {

        // 上传视频功能
        //获取token
        GetToken();


        // 小程序码获取功能
        //StartCoroutine(DoMockGenerate());

    }

    void GetToken()
    {
        HTTPRequest request = new HTTPRequest(new Uri(api + "/api/storage/qiniu/token"), HTTPMethods.Post, GetTokenRequestFinished);
        request.AddField("strategy", "default");
        request.AddField("app", "1");
        request.Send();
    }

    void GetTokenRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        JsonData data = JsonMapper.ToObject(response.DataAsText);
        if ((bool)data["status"])
        {
            //获取token请求成功
            string token = (string)data["data"]["token"];
            //上传视频
            UploadVideo(token);
        }
        else
        {
            Debug.Log("请求token失败");
            Error();
        }
    }

    void UploadVideo(string token)
    {

        HTTPRequest request = new HTTPRequest(new Uri(qiniu_api), HTTPMethods.Post, UploadVideoRequestFinished);
        request.AddBinaryData("file", GetVideoData(Application.dataPath + "/Files/1.mp4"));
        request.AddField("token", token);
        request.Send();
    }

    byte[] GetVideoData(string path)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        fs.Close();
        return data;
    }

    void UploadVideoRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        JsonData data = JsonMapper.ToObject(response.DataAsText);
        if ((bool)data["status"])
        {
            //上传视频成功
            string videoUrl = (string)data["data"]["url"];
            UploadUrl(videoUrl);
            Debug.Log("上传视频成功:" + videoUrl);
        }
        else
        {
            Debug.Log("上传视频失败");
            Error();
        }
    }

    void UploadUrl(string url)
    {
        HTTPRequest request = new HTTPRequest(new Uri(api+"/api/attachments"), HTTPMethods.Post, UploadUrlRequestFinished);
        request.AddField("app_id", APP_ID.ToString());
        request.AddField("file_url", url);
        request.Send();
    }

    void UploadUrlRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        JsonData data = JsonMapper.ToObject(response.DataAsText);
        if ((bool)data["status"])
        {
            //上传文件路径成功
            Debug.Log(response.DataAsText);
            return;
            message_id = (string)data["data"]["options"]["mail"]["message"];
            string code = (string)data["data"]["code"];
            GetQRCode(code);
        }
        else
        {
            Debug.Log("上传文件路径成功");
            Error();
        }
    }

    void GetQRCode(string code)
    {
        HTTPRequest request = new HTTPRequest(new Uri(h5_api + "/api/wxapp/getFileQrcode/" + code + "?type=beat_2"), HTTPMethods.Get, GetQRCodeRequestFinished);
        request.Send();
    }

    void GetQRCodeRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        _qrCode.texture = response.DataAsTexture2D;
        _erCodeIsGenerated = true;
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
