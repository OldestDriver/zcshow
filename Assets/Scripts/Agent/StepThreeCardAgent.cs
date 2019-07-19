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
    [SerializeField,Header("UI")] private Text _countdownText;
    [SerializeField] private List<RawImage> _dots;//红点
    [SerializeField] private Text _titleText;//标题
    [SerializeField] private RawImage _previewRawImage;//照片

    private Color _color_dot_active = new Color(79 / 255,14 / 255,16 /255);
    private Color _color_dot = new Color(204 / 255,32 / 255,39 / 255);

    private int _countDownNum = 3;//倒计时数字
    private int _totalPicture = 0 ;//图片总数
    private bool _beginFlowComplete = false;
    private ShootFlowStatus _shootFlowStatus = ShootFlowStatus.Init;

    private bool _showPreview = false; // 显示预览


    private bool _connectCamfiSuccess = false;  // Cam-fi 连接状态
    private bool _connectCameraSuccess = false; //  相机 连接状态
    private bool _connectLiveShowSuccess = false;   //  实时取景  连接状态
    

    private bool _doCountDownLock = false;
    private bool _doShootLock = false;
    private bool _doHandleLock = false;

    //拍照相关
    SocketManager socketioManager;
    //public delegate void OnCamfiFileAddHandler(string fileurl);
    //public event OnCamfiFileAddHandler CamfiFileAdd;
    private bool isReceive = false;

    private bool flag;

    private void Reset() {
        _countDownNum = 3;
        _totalPicture = 0;
        _beginFlowComplete = false;
        _shootFlowStatus = ShootFlowStatus.Init;
        _showPreview = false;
        ResetLock();

    }

    private void ResetLock() {
        _doCountDownLock = false;
        _doShootLock = false;
        _doHandleLock = false;
    }


    enum ShootFlowStatus {
        Init,
        DoCountDown ,
        CountDownCompleted ,
        DoShoot ,
        ShootCompleted ,
        DoHandle ,
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
    /// 拍摄流程 - 倒计时
    /// </summary>
    private void DoCountDownStep() {
        if (!_doCountDownLock)
        {
            _doCountDownLock = true;
            _showPreview = true;
            DoCountDown();
        }
    }

    private void DoCountDown() {
        // 设置文字
        _titleText.text = "准备拍摄第" + (_totalPicture + 1) + "张照片";

        _countdownText.transform.DOScale(0, 0.5f)
            .OnComplete(() => {
                _countDownNum--;
                _countdownText.text = _countDownNum + "";
                _countdownText.transform.DOScale(2, 0.5f).OnComplete(() => {

                if (_countDownNum == 0)
                {
                    _countdownText.text = "";
                    _shootFlowStatus = ShootFlowStatus.CountDownCompleted;
                    _countDownNum = 3;
                }
                else
                {
                    DoCountDown();
                }
            });
        });
    }




    /// <summary>
    /// 拍摄流程 - 拍摄
    /// </summary>
    private void DoShootStep()
    {
        if (!_doShootLock) {
            _doShootLock = true;
            StartCoroutine(DoShoot());
        }
        
    }


    IEnumerator DoShoot() {

        Debug.Log("模拟拍摄");
        //TakePhoto();
        yield return new WaitForSeconds(3);
        _totalPicture++;
        Debug.Log("已拍" + _totalPicture + "张照片");

        _shootFlowStatus = ShootFlowStatus.ShootCompleted;
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
            StartCoroutine(DoHandlePhoto());
        }
    }

    IEnumerator DoHandlePhoto()
    {
        Debug.Log("模拟处理照片");
        yield return new WaitForSeconds(1);
        Debug.Log("已处理");

        // 红点功能
        _dots[_totalPicture - 1].color = Color.red;


        _countDownNum = 3;
        _countdownText.text = _countDownNum + "";

        ResetLock();

        _shootFlowStatus = ShootFlowStatus.HandleCompleted;
    }




    /// <summary>
    /// 显示预览
    /// </summary>
    private void ShowPreview() {
        //Debug.Log("模拟预览功能");
        Debug.Log("实时取景中...");
        if (_showPreview)
        {
            ShowImage();
        }
    }

    private void ShowImage()
    {


    }



    void OnEnable()
    {
        if (socketioManager != null)
        {
            socketioManager.Open();
        }

        if (_cameraIsPrepared)
        {
            //StartLiveShow();
        }
    }



}
