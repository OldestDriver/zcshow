using BestHTTP;
using BestHTTP.SocketIO;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class StepThreeCardAgent : CardBaseAgent
{

    //第三步
    [SerializeField, Header("UI")] private Text _countdownText;
    [SerializeField] private Text _titleText;//标题

    [SerializeField, Header("Preview")] private RawImage _previewRawImage;//照片
    [SerializeField] private RawImage _previewPhoto;// 预览照片
    [SerializeField] private RectTransform _preparePrevRect;    // 准备预览画面

    [SerializeField, Header("Dots")] private Color _color_dot_active;
    [SerializeField] private Color _color_dot;
    [SerializeField] private List<RawImage> _dots;//红点


    [SerializeField, Header("Camera Manager")] CameraManager _cameraManager;
    [SerializeField, Header("Video Factory Agent")] VideoFactoryAgent _videoFactoryAgent;


    [SerializeField,Header("倒计时时间")] int _CountDownNumCost = 3;//倒计时数字常量

    private int _countDownNum;//倒计时数字
    
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

    private Texture2D _currentPhotoTexture;


    private bool isReceive = false;

    private bool flag;

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
        CompletePrepare();
    }


    public override void DoRunIn()
    {
        Debug.Log("场景3进入");
        // 显示在首个
        GetComponent<RectTransform>().SetAsLastSibling();

        // 获取实时取景
        _cameraManager.InitLiveShow(OnConnectLiveShowSuccess,OnConnectLiveShowFailed, _previewRawImage);

        CompleteRunIn();
    }

    /// <summary>
    /// 运行阶段
    /// </summary>
    public override void DoRun() {

        if (_cameraIsPrepared)
        {
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
        else {
            Debug.Log("相机未准备完成！");
        }

    }

    public override void DoRunOut()
    {
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
            _shootFlowStatus = ShootFlowStatus.DoCountDown;
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

            if (_totalPicture < 3)
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

        if (_showPreview) {
            ShowPreview();
        }

    }

    /// <summary>
    ///     拍摄流程 - 初始化
    /// </summary>
    private void DoShootFlowInit() {

        Debug.Log("流程初始化 ： " + _totalPicture);

        _previewPhoto.gameObject.SetActive(false);
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
        _titleText.text = "准备拍摄第" + (_totalPicture + 1) + "张照片";

        //_countdownText.transform.DOScale(0, 0.5f)
        //    .OnComplete(() => {
        //        _countDownNum--;
        //        _countdownText.text = _countDownNum + "";
        //        _countdownText.transform.DOScale(2, 0.5f).OnComplete(() => {

        //            if (_countDownNum == 0)
        //            {
        //                _countdownText.text = "";
        //                _shootFlowStatus = ShootFlowStatus.CountDownCompleted;
        //                _countDownNum = _CountDownNumCost;
        //            }
        //            else
        //            {
        //                DoCountDown();
        //            }
        //        });
        //    });


        Debug.Log("DoCountDown");

        if (_countDownNum > 0)
        {
            _countdownText.text = _countDownNum.ToString();
            StartCoroutine(WaitForOneSecond());
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





    /// <summary>
    /// 拍摄流程 - 拍摄
    /// </summary>
    private void DoShootStep()
    {
        if (!_doShootLock) {
            _doShootLock = true;
            DoShoot();
        }
    }


    void DoShoot() {

        _messageBoxAgent.UpdateMessageTemp("拍摄中!");

        _cameraManager.Shoot(OnShootSuccess, OnShootError);

        // 关闭实时预览
        _showPreview = false;

        //yield return new WaitForSeconds(3);
        //_totalPicture++;
        //Debug.Log("已拍" + _totalPicture + "张照片");

        //_shootFlowStatus = ShootFlowStatus.ShootCompleted;
    }

    /// <summary>
    /// 拍摄流程 - 拍摄后处理
    /// </summary>
    private void DoHandlePhotoStep()
    {
        if (!_doHandleLock)
        {
            _doHandleLock = true;
            _showPreview = false;

            //_messageBoxAgent.UpdateMessageTemp("正在处理照片!");

            StartCoroutine(DoHandlePhoto());

            //DoHandlePhoto();
        }
    }

    IEnumerator DoHandlePhoto()
    {
        // 显示照片
        _previewPhoto.gameObject.SetActive(true);
        _previewPhoto.texture = _currentPhotoTexture;

        yield return new WaitForSeconds(2);
        //_messageBoxAgent.UpdateMessageTemp("已处理照片!");

        _messageBoxAgent.UpdateMessageTemp("拍摄照片成功!");

        // 红点功能
        _dots[_totalPicture - 1].color = Color.red;

        //_countDownNum = 3;
        //_countdownText.text = _countDownNum + "";

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
        _preparePrevRect.gameObject.SetActive(false);
        _previewRawImage.gameObject.SetActive(true);

        _connectLiveShowSuccess = true;
        _cameraIsPrepared = true;
    }

    private void OnConnectLiveShowFailed()
    {
        _connectLiveShowSuccess = false;
        _cameraIsPrepared = false;
        Debug.Log("Connect LiveShow Failed");

        _messageBoxAgent.UpdateMessage("连接实时取景失败！");

    }
    #endregion

    #region 拍摄回调
    private void OnShootSuccess(byte[] content,Texture2D tex) {
        Debug.Log("拍摄回调 ");

        _doShootLock = false;

        // 保持内容
        _videoFactoryAgent.AddPhotoTexture(tex);
        _currentPhotoTexture = tex;

        // 调整计数器
        _totalPicture++;



        _shootFlowStatus = ShootFlowStatus.ShootCompleted;
    }


    private void OnShootError(string message)
    {
        _doShootLock = false;

        Debug.Log("拍摄失败 ： " + message);
        _shootFlowStatus = ShootFlowStatus.ShootCompleted;
    }
    #endregion



}
