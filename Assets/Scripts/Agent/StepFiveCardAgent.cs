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
	[SerializeField , Header("QR CODE")] RectTransform _qrCodeRect;
	[SerializeField] private RawImage _qrCode;
	[SerializeField, Header("Video Factory")] private VideoFactoryAgent _videoFactoryAgent;
    [SerializeField, Header("Video Factory - No Logo")] private VideoFactoryNoLogoAgent _videoFactoryNoLogoAgent;
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


    private NotionApiClient notionApiClient;


    //  挂载的使用操作信息
    private Action _OnUpdateHandleTimeAction;
    private Action _OnKeepOpenAction;
    private Action _OnCloseKeepOpenAction;
    private Action _OnErrorHappened;

    private void Reset()
    {
        _erCodeIsGenerated = false;
        _showResultLock = false;
        _sendingEmail = false;
        _messageIdPrepared = false;
        _customKeyboard.ClearAll();
        _customKeyboard.Hide();

        _resultRect.gameObject.SetActive(true);

        //_loadingRect.gameObject.SetActive(true);

    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        _OnKeepOpenAction.Invoke();

        _inputEmailImage.color = _inputEmailDisableColor;
        gameObject.SetActive(true);

        _qrCodeRect.gameObject.SetActive(false);

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
        Debug.Log("场景 5 DoRunOut");

        CompleteRunOut();
    }


    /// <summary>
    /// 结束阶段
    /// </summary>
    public override void DoEnd() {
        Debug.Log("场景 5 结束");
        _NextCard?.DoActive();

        _qrCodeRect.gameObject.SetActive(false);
        gameObject.SetActive(false);

        CompleteDoEnd();
    }


    // 点击发送邮件
    public void OnClickSendEmail() {
        DoSendEmail();
    }



    private void ShowResult()
    {
        _OnCloseKeepOpenAction.Invoke();
		_qrCodeRect.gameObject.SetActive(true);
		emailInput.ActivateInputField();
    }

    //开始输入邮件
    public void StartInputEmail()
    {
        _OnUpdateHandleTimeAction.Invoke();

        if (_messageIdPrepared) {
            emailInput.ActivateInputField();
            FindObjectOfType<CustomKeyboard>().Show();
        }
    }


    /// <summary>
    /// 获取小程序码
    /// </summary>
    private void GetErCode() {

        _videoFactoryNoLogoAgent.DoActive(OnVideoNoLogoGenerate,_isMock);

        // 上传视频功能
        //获取token
        //GetToken();
        notionApiClient = new NotionApiClient(_isMock, OnApiError, OnMessageIdReceived, OnQRCodeReceived);
        notionApiClient.GetQrCodeIcon(_videoFactoryAgent.GetVideoAddress());

    }


    private void DoSendEmail() {

        _sendingEmail = true;
        SendEmail();
    }


    void SendEmail()
    {
        Debug.Log("发送邮件！！！");

        _OnKeepOpenAction.Invoke();

        string address = _inputEmailText.text;
        notionApiClient.SendEmail(address, OnSendEmailSuccess,OnSendEmailError);

    }


    /// <summary>
    ///     点击返回首页
    /// </summary>
    public void DoReturnHome() {
        _OnUpdateHandleTimeAction.Invoke();

        //  清理保存的数据
        _videoFactoryAgent.Clear();
        _videoFactoryNoLogoAgent.StopPlayVideoAndClear();

        _NextCard = _HomeCard;

        DoRunOut();
    }



    /// <summary>
    ///     点击空白
    /// </summary>
    public void DoClickEmpty() {
        _OnUpdateHandleTimeAction.Invoke();

        Debug.Log("Do Click Empty");
        _customKeyboard.Hide();

    }




    void OnVideoNoLogoGenerate(string videoUrl)
    {
        Debug.Log("Video 生成完成 ： " + videoUrl);

        // 投屏
        if (_isMock)
        {
            string path = Application.streamingAssetsPath + "/test-result_1.mp4";

            _previewAgent.UpdateVideo(path, _videoFactoryNoLogoAgent.GetVideoAddress());
        }
        else {
            _previewAgent.UpdateVideo(_videoFactoryAgent.GetVideoAddress(), _videoFactoryNoLogoAgent.GetVideoAddress());
        }

        
    }



    //public NotionApiClient(bool isMock, Action<string> onApiError, Action onMessageIdReceived, Action<Texture2D> onQRCodeReceived)
    public void OnApiError(string message) {
        // api error
        _OnErrorHappened.Invoke();
    }

    public void OnMessageIdReceived()
    {
        _messageIdPrepared = true;

        Debug.Log("Do Color");

        _inputEmailImage.DOColor(_inputEmailActiveColor, 0.5f);
    }

    public void OnQRCodeReceived(Texture2D texture2D)
    {
        // Todo 
        _qrCode.texture = texture2D;
        _erCodeIsGenerated = true;
        _inputEmailImage.DOColor(_inputEmailActiveColor, 0.5f);


    }

    public void OnSendEmailSuccess()
    {
        _messageBoxAgent.UpdateMessageTemp("邮件发送成功!");

        _customKeyboard.Hide();
        _customKeyboard.ClearAll();
    }

    public void OnSendEmailError(string message)
    {
        _messageBoxAgent.UpdateMessageTemp(message);
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
