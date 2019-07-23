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
using System.Text;

public class StepFiveCardAgent : CardBaseAgent
{

    [SerializeField, Header("UI")] private InputField emailInput;
    [SerializeField] private RectTransform _loadingContentRect;
    [SerializeField] private RectTransform _loadingRect;
    [SerializeField] private RectTransform _resultRect;
    [SerializeField] private RawImage _qrCode;

    [SerializeField, Header("Video Factory")] private VideoFactoryAgent _videoFactoryAgent;
    [SerializeField, Header("Video Factory - No Logo")] private VideoFactoryAgent _videoFactoryNoLogoAgent;

    [SerializeField, Header("Preview")] private PreviewAgent _previewAgent;

    [SerializeField, Header("Card Index")] protected CardBaseAgent _HomeCard;
    [SerializeField, Header("Email")] Text _inputEmailText;
    [SerializeField] Color _inputEmailActiveColor;
    [SerializeField] Color _inputEmailDisableColor;
    [SerializeField] Image _inputEmailImage;

    [SerializeField, Header("Keyboard")] protected CustomKeyboard _customKeyboard;


    [SerializeField, Header("Mock")] protected bool _isMock;



    private bool _erCodeIsGenerated = false;
    private bool _messageIdPrepared = false;    // messageId 用在确认是否可发送邮件
    private bool _showResultLock = false;
    private bool _sendingEmail = false;

    //公共参数
    private int APP_ID = 99;
    private string api = "https://api.pixelplus.cc";
    private string qiniu_api = "https://upload.qiniu.com";
    private string h5_api = "https://h5.pixelplus.cc";
    private int EVENT_ID = 49;

    private int message_id;

    private void Reset()
    {
        _erCodeIsGenerated = false;
        _showResultLock = false;
        _sendingEmail = false;
        _messageIdPrepared = false;

        _resultRect.gameObject.SetActive(true);
        //_loadingRect.gameObject.SetActive(true);
        
    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        _inputEmailImage.color = _inputEmailDisableColor;


        // 获取小程序码
        GetErCode();
        //_erCodeIsGenerated = true;


        CompletePrepare();

        _customKeyboard.OnClickEnter(DoSendEmail);
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
            //DoLoading();
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



    private void ShowResult()
    {
        //_resultRect.gameObject.SetActive(true);
        //_loadingRect.gameObject.SetActive(false);
        emailInput.ActivateInputField();
    }

    //开始输入邮件
    public void StartInputEmail()
    {
        if (_messageIdPrepared) {
            emailInput.ActivateInputField();
            FindObjectOfType<CustomKeyboard>().Show();
        }
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

        _videoFactoryNoLogoAgent.DoActive(OnVideoNoLogoGenerate, false, false);

        // 上传视频功能
        //获取token
        GetToken();

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
            Debug.Log("请求token失败" + data["message"]);
            Error();
        }
    }

    void UploadVideo(string token)
    {

        HTTPRequest request = new HTTPRequest(new Uri(qiniu_api), HTTPMethods.Post, UploadVideoRequestFinished);

        if (_isMock)
        {
            request.AddBinaryData("file", GetVideoData(Application.dataPath + "/Out/1.mp4"));
        }
        else {
            request.AddBinaryData("file", GetVideoData(_videoFactoryAgent.GetVideoAddress()), "Video.mp4");
        }

        request.AddHeader("Accept", "application/json");
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
            //Debug.Log("上传视频成功:" + videoUrl);
        }
        else
        {
            Debug.Log("上传视频失败" + data["message"]);
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
            //Debug.Log(response.DataAsText);
            message_id = (int)data["data"]["options"]["mail"]["message"];
            string code = (string)data["data"]["code"];
            //Debug.Log("message_id：" + message_id);

            _messageIdPrepared = true;
            _inputEmailImage.DOColor(_inputEmailActiveColor, 0.5f);



            GetQRCode(code);
        }
        else
        {
            _messageIdPrepared = false;
            Debug.Log("上传文件路径失败" + data["message"]);
            Error();
        }
    }

    void GetQRCode(string code)
    {
        HTTPRequest request = new HTTPRequest(new Uri(h5_api + "/api/wxapp/getFileQrcode/" + code + "?type=beta_2"), HTTPMethods.Get, GetQRCodeRequestFinished);
        //request.AddField("type", "beta_2");
        //request.AddField("code", code);
        request.Send();
    }

    void GetQRCodeRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        if (response.StatusCode == 200)
        {
            _qrCode.texture = response.DataAsTexture2D;
            _erCodeIsGenerated = true;
        }   else
        {
            Debug.Log(response.DataAsText);
            Error();
        }
    }

    private void DoSendEmail() {

        _sendingEmail = true;
        Debug.Log("发送邮件！！！");
        //return;
        SendEmail();
    }


    void SendEmail()
    {
        HTTPRequest request = new HTTPRequest(new Uri(api + "/api/mail/messages/" + message_id + "/send"), HTTPMethods.Post, SendEmainRequestFinished);


        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("Accept", "application/json");


        // 设置地址

        string address = _inputEmailText.text;

        if (CheckEmailStr())
        {
            var fromJson = @"
            {
                ""to""     : [{""email"" : """ +
                    address
                    + @"""}]}";

            request.RawData = Encoding.UTF8.GetBytes(fromJson);
            request.Send();

            
        }
        else {
            _messageBoxAgent.UpdateMessageTemp("Email is valid.");
        }

        //string address = "873074332@qq.com";

    }

    void SendEmainRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        JsonData data = JsonMapper.ToObject(response.DataAsText);
        if ((bool)data["status"])
        {
            //发送邮件成功
            Debug.Log("发送邮件成功");
            _messageBoxAgent.UpdateMessageTemp("邮件发送成功!");

            _customKeyboard.Hide();
            _customKeyboard.ClearAll();
        }
        else
        {
            Debug.Log("发送邮件失败" + data["message"]);
            Error();
        }


        _inputEmailText.text = "";
    }

    /// <summary>
    ///     点击返回首页
    /// </summary>
    public void DoReturnHome() {

        //  清理保存的数据
        _videoFactoryAgent.Clear();

        nextCard = _HomeCard;

        DoRunOut();

    }



    /// <summary>
    ///     点击空白
    /// </summary>
    public void DoClickEmpty() {
        Debug.Log("Do Click Empty");
        _customKeyboard.Hide();
    }


    private bool CheckEmailStr() {
        string address = _inputEmailText.text;
        if (address.Length == 0)
            return false;
        if (address.IndexOf("@") == -1) {
            return false;
        }
        return true;
    }

    void OnVideoNoLogoGenerate(string videoUrl)
    {
        Debug.Log("Video 生成完成 ： " + videoUrl);

        //_previewVideoPlayer.url = videoUrl;

        //_videoIsGenerateCompleted = true;

        // 投屏
        _previewAgent.UpdateVideo(_videoFactoryAgent.GetVideoAddress(), _videoFactoryNoLogoAgent.GetVideoAddress()) ;
    }

}
