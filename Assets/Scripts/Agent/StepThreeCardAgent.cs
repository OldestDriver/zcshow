using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class StepThreeCardAgent : CardBaseAgent
{

    //第三步
    [SerializeField, Header("UI - Count Down")] private Text _countdownText;
    [SerializeField] private RectTransform _countdown;
    [SerializeField] private RectTransform _countdownReady;
    [SerializeField] private Text _titleText;//标题

    [SerializeField, Header("Preview")] private RawImage _liveViewContent;//照片
    [SerializeField] private RawImage _previewPhoto;// 预览照片
    [SerializeField] private RectTransform _preparePrevRect;    // 准备预览画面
    [SerializeField] private RectTransform _previewRect; // rect

    [SerializeField, Header("Dots")] private Color _color_dot_active;
    [SerializeField] private Color _color_dot;
    [SerializeField] private List<RawImage> _dots;//红点


    [SerializeField, Header("Camera Manager")] CameraManager _cameraManager;
    [SerializeField, Header("Video Factory Agent")] VideoFactoryAgent _videoFactoryAgent;
    [SerializeField, Header("Video Factory Agent - Logo")] VideoFactoryNoLogoAgent _videoFactoryAgentNoLogo;

    [SerializeField, Header("Photo Mask")] private RectTransform photoMask1;
    [SerializeField] private RectTransform photoMask2;
    [SerializeField] private RectTransform photoMask3;

    [SerializeField,Header("倒计时时间")] int _CountDownNumCost = 4;//倒计时数字常量
    [SerializeField,Header("拍摄的数量")] int _totalPictureConst = 3;//倒计时数字常量

    [SerializeField, Header("Mock")] bool _isMock;

    private int _countDownNum;//倒计时数字

    private Texture2D _lastTextureBeforeShoot; // 拍摄前最后一次内容
    
    private int _totalPicture = 0;//图片总数
    private bool _beginFlowComplete = false;
    private ShootFlowStatus _shootFlowStatus = ShootFlowStatus.Init;

    private bool _showPreview = false; // 显示预览
 


    private bool _connectCamfiSuccess = false;  // Cam-fi 连接状态
    private bool _connectCameraSuccess = false; //  相机 连接状态
    private bool _connectLiveShowSuccess = false;   //  实时取景  连接状态


    private bool _doCountDownLock = false;
    private bool _doShootLock = false;
    private bool _doHandleLock = false;

    private bool _doFlowInitLock = false;

    private Texture2D _currentPhotoTexture;


    private bool isReceive = false;

    private bool flag;


    //  挂载的使用操作信息
    private Action _OnUpdateHandleTimeAction;
    private Action _OnKeepOpenAction;
    private Action _OnCloseKeepOpenAction;

    private Action _OnErrorHappened;



    private void Reset() {
        _countDownNum = _CountDownNumCost;
        _totalPicture = 0;
        _beginFlowComplete = false;
        _shootFlowStatus = ShootFlowStatus.Init;
        _showPreview = false;
        ResetLock();

        ResetCounts();

    }

    /// <summary>
    ///  重置计数器
    /// </summary>
    void ResetCounts(){
        for (int i = 0; i < _dots.Count; i++) {
            _dots[i].color = _color_dot;
        }
    }


    private void ResetLock() {
        _doCountDownLock = false;
        _doShootLock = false;
        _doHandleLock = false;
    }


    enum ShootFlowStatus {
        Init,
        DoCountDown,
        CountDownCompleted,
        DoShoot,
        ShootCompleted,
        DoHandle,
        HandleCompleted,
        Finished
    }


    private bool _cameraIsPrepared = false;



    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        _OnKeepOpenAction.Invoke();

        gameObject.SetActive(true);

        _previewRect.gameObject.SetActive(false);


        if (_isMock) {
            _connectLiveShowSuccess = true;
            _liveViewContent.color = Color.black;
        }

        CompletePrepare();
    }


    public override void DoRunIn()
    {
        Debug.Log("场景3进入");
        // 显示在首个
        GetComponent<RectTransform>().SetAsLastSibling();

        _cameraManager.InitLiveShow(OnConnectLiveShowSuccess, OnConnectLiveShowFailed, _liveViewContent);

        if (_connectLiveShowSuccess) {
            _preparePrevRect.gameObject.SetActive(false);

            // 显示取景控件
            _liveViewContent.gameObject.SetActive(true);
            _previewRect.gameObject.SetActive(true);

            ShowImage();

        }


        CompleteRunIn();
    }

    /// <summary>
    /// 运行阶段
    /// </summary>
    public override void DoRun() {

        if (_connectLiveShowSuccess) {

            // 依次拍摄三张照片
            if (!_beginFlowComplete)
            {
                BeginShootFlow();
            }
            else
            {
                DoRunOut();
            }
        }

    }

    public override void DoRunOut()
    {
        // 关闭取景流
        //_cameraManager.DisconnectLiveShow();


        // 开始生成视频       
        CompleteRunOut();
    }


    /// <summary>
    /// 结束阶段
    /// </summary>
    public override void DoEnd() {
        gameObject.SetActive(false);

        _NextCard.DoActive();

        Debug.Log("场景3结束");
    }


    // 点击开始拍摄
    public void OnClick() {
        DoRunOut();
    }



    /// <summary>
    /// 开始拍照流程
    /// </summary>
    private void BeginShootFlow() {

        if (_shootFlowStatus == ShootFlowStatus.Init) {
             DoShootFlowInit();
        }

        if (_shootFlowStatus == ShootFlowStatus.DoCountDown) {
            DoCountDownStep();
        }

        if (_shootFlowStatus == ShootFlowStatus.CountDownCompleted) {
            _shootFlowStatus = ShootFlowStatus.DoShoot;
        }

        if (_shootFlowStatus == ShootFlowStatus.DoShoot) {
            DoShootStep();
        }

        if (_shootFlowStatus == ShootFlowStatus.ShootCompleted)
        {
            _shootFlowStatus = ShootFlowStatus.DoHandle;
        }

        if (_shootFlowStatus == ShootFlowStatus.DoHandle)
        {
            DoHandlePhotoStep();
        }

        if (_shootFlowStatus == ShootFlowStatus.HandleCompleted) {

            Debug.Log("Do Handle Completed");

            if (_totalPicture < _totalPictureConst)
            {
                _shootFlowStatus = ShootFlowStatus.Init;
            }
            else {
                _shootFlowStatus = ShootFlowStatus.Finished;
            }
        }

        if (_shootFlowStatus == ShootFlowStatus.Finished)
        {
            _beginFlowComplete = true;
        }
    }

    /// <summary>
    ///     拍摄流程 - 初始化
    /// </summary>
    private void DoShootFlowInit() {


        _shootFlowStatus = ShootFlowStatus.DoCountDown;


    }

    /// <summary>
    /// 拍摄流程 - 倒计时
    /// </summary>
    private void DoCountDownStep() {
        if (!_doCountDownLock)
        {
            _doCountDownLock = true;
            _showPreview = true;

            _countDownNum = _CountDownNumCost;
            DoCountDown();
        }
    }

    private void DoCountDown() {
        // 设置文字

        //_titleText.text = "准备拍摄第" + (_totalPicture + 1) + "张照片";

        _previewPhoto.gameObject.SetActive(false);

        if (_countDownNum > 1)
        //if (_countDownNum == 1)
        {
            UpdatePhotoMask();
            _countdown.gameObject.SetActive(true);
            _countdownReady.gameObject.SetActive(false);

            _countdownText.text = (_countDownNum - 1).ToString();
            StartCoroutine(WaitForOneSecond());
        }
        else if (_countDownNum == 1)
        {
            //} else if (_countDownNum > 50) {
            UpdatePhotoMask();
            _countdown.gameObject.SetActive(false);
            _countdownReady.gameObject.SetActive(true);

            StartCoroutine(WaitForTwoSecond());
        }
        else {
            _shootFlowStatus = ShootFlowStatus.CountDownCompleted;
            _countDownNum = _CountDownNumCost;
            _countdownText.text = "";
        }
    }

    IEnumerator WaitForOneSecond() {
        yield return new WaitForSeconds(1);
        _countDownNum--;
        DoCountDown();
    }

    IEnumerator WaitForTwoSecond()
    {
        yield return new WaitForSeconds(2);
        _countDownNum--;
        DoCountDown();
    }





    /// <summary>
    /// 拍摄流程 - 拍摄
    /// </summary>
    private void DoShootStep()
    {
        if (!_doShootLock) {
            _doShootLock = true;

            // 先给画面赋值
            Texture2D texture2D = new Texture2D(200, 200);
            texture2D.LoadImage(_cameraManager.GetLastScreen());

            float w = texture2D.width;
            float h = texture2D.height;

            _currentPhotoTexture = texture2D;

            // 进行拍摄
            DoShoot();
        }
    }

    private void OnDisconnectLiveShowAction() {
        _connectLiveShowSuccess = false;
    }


    void DoShoot() {

        _messageBoxAgent.UpdateMessageTemp("拍摄中!");

        if (_isMock)
        {
            _doShootLock = false;
            _totalPicture++;
            _shootFlowStatus = ShootFlowStatus.ShootCompleted;
        }
        else {
            _cameraManager.Shoot(OnShootSuccess, OnShootError);
        }

    }

    /// <summary>
    /// 拍摄流程 - 拍摄后处理
    /// </summary>
    private void DoHandlePhotoStep()
    {
        if (!_doHandleLock)
        {
            _doHandleLock = true;

             StartCoroutine(DoHandlePhoto());

        }
    }

    IEnumerator DoHandlePhoto()
    {
        // 显示照片
        _previewPhoto.gameObject.SetActive(true);
        //_previewPhoto.texture = _currentPhotoTexture;

        yield return new WaitForSeconds(1);

        _messageBoxAgent.UpdateMessageTemp("拍摄照片成功!");

        // 红点功能
        _dots[_totalPicture - 1].color = Color.red;

        ResetLock();
        _shootFlowStatus = ShootFlowStatus.HandleCompleted;
    }




    /// <summary>
    /// 显示预览
    /// </summary>
    private void ShowPreview() {
        if (_showPreview)
        {
            ShowImage();
        }
        else {
            NotShowImage();
        }
    }

    private void ShowImage()
    {
        _cameraManager.ShowLive();
    }

    private void NotShowImage()
    {
        _cameraManager.CloseLive();
    }



    void OnEnable()
    {

        if (_cameraIsPrepared)
        {
            //StartLiveShow();
        }
    }

    #region 连接实时取景回调
    private void OnConnectLiveShowSuccess() {
        Debug.Log("连接实时取景回调");

        // _preparePrevRect.gameObject.SetActive(false);
        _connectLiveShowSuccess = true;

        // 显示取景控件
        _liveViewContent.gameObject.SetActive(true);
        _previewRect.gameObject.SetActive(true);

        ShowImage();


    }

    private void OnConnectLiveShowFailed()
    {
        _connectLiveShowSuccess = false;
        _cameraIsPrepared = false;
        Debug.Log("Connect LiveShow Failed");

        _messageBoxAgent.UpdateMessage("连接实时取景失败！");

        _OnErrorHappened.Invoke();

    }
    #endregion

    #region 拍摄回调
    private void OnShootSuccess(byte[] content,Texture2D tex) {
        Debug.Log("拍摄回调 ");

        _doShootLock = false;

        // 保存
        _videoFactoryAgent.AddPhotoTexture(tex);
        _videoFactoryAgentNoLogo.AddPhotoTexture(tex);

        
        _previewPhoto.texture = tex ;

        // 调整计数器  5D4
        _totalPicture++;

        Debug.Log("拍摄成功 ");

        _shootFlowStatus = ShootFlowStatus.ShootCompleted;
    }


    private void OnShootError(string message)
    {
        _doShootLock = false;

        Debug.Log("拍摄失败 ： " + message);
        _shootFlowStatus = ShootFlowStatus.ShootCompleted;

        _OnErrorHappened.Invoke();
    }
    #endregion



    /// <summary>
    /// 更新取景框
    /// </summary>
    private void UpdatePhotoMask() {
        if (_totalPicture == 0) {
            photoMask1.gameObject.SetActive(true);
            photoMask2.gameObject.SetActive(false);
            photoMask3.gameObject.SetActive(false);
        }
        else if (_totalPicture == 1)
        {
            photoMask1.gameObject.SetActive(false);
            photoMask2.gameObject.SetActive(true);
            photoMask3.gameObject.SetActive(false);
        }
        else if (_totalPicture == 2)
        {
            photoMask1.gameObject.SetActive(false);
            photoMask2.gameObject.SetActive(false);
            photoMask3.gameObject.SetActive(true);
        }

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
