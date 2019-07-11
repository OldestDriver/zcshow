using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using BestHTTP;
using BestHTTP.SocketIO;
using System.Text;


public class StepThreeCardAgent : CardBaseAgent
{

    //第三步
    [SerializeField,Header("UI")] private Text _countdownText;
    [SerializeField] private List<RawImage> _dots;//红点
    [SerializeField] private Text _titleText;//标题
    [SerializeField] private RawImage _previewRawImage;//照片


    private int _countDownNum = 3;//倒计时数字
    private int _totalPicture = 0 ;//图片总数
    private bool _beginFlowComplete = false;
    private ShootFlowStatus _shootFlowStatus = ShootFlowStatus.Init;

    private bool _showPreview = false; // 显示预览


    private bool _doCountDownLock = false;
    private bool _doShootLock = false;
    private bool _doHandleLock = false;

    //拍照相关
    SocketManager socketioManager;
    public delegate void OnCamfiFileAddHandler(string fileurl);
    public event OnCamfiFileAddHandler CamfiFileAdd;


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
        //_introduceText.text = "";

        Reset();
        
 

        SocketOptions option = new SocketOptions();

        socketioManager = new SocketManager(new Uri(CamfiServerInfo.SockIOUrlStr));
        InitSocketIOManager();

        try
        {
            socketioManager.Open();


        }
        catch (Exception e)
        {
            Debug.Log("SocketIO Open 捕获到了异常" + e.ToString());
        }

        CompletePrepare();

    }

    
    #region Manager的一些方法
    void InitSocketIOManager()
    {
        socketioManager.Socket.On("connect", OnConnected);
        socketioManager.Socket.On("disconnect", OnDisConnected);
        socketioManager.Socket.On("camera_add", OnCameraAdd);
        socketioManager.Socket.On("camera_remove", OnCameraRemove);
        socketioManager.Socket.On("file_added", OnFileAdded);
        socketioManager.Socket.On("liveshow_error", OnLiveshowError);

    }
    #endregion

    #region CamFi服务器SocketIO事件回调函数
    void OnConnected(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接建立");
    }
    void OnDisConnected(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接 断开");
    }
    void OnCameraAdd(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi连接了相机");

        _cameraIsPrepared = true;

        StartLiveShow();

    }

    void OnCameraRemove(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi丢失相机连接");

        _cameraIsPrepared = false;
    }
    void OnFileAdded(Socket socket, Packet packet, params object[] args)
    {

        string pathInCamfi = args[0].ToString();
        Debug.Log("照片添加，其Url: " + pathInCamfi);
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.CamFiGetRawDataByEnCodeUrlStr(UrlEncode(pathInCamfi))), HTTPMethods.Get, GetRawDataFinished);
        request.Send();
    }
    void OnLiveshowError(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("实时取景发送错误");
    }
    #endregion

    void GetRawDataFinished(HTTPRequest request, HTTPResponse response)
    {
        Debug.Log("GetRawDataFinished! Text received: " + response.StatusCode + response.Data);
        if (response.StatusCode == 200)
        {
            //获取照片成功
            _previewRawImage.texture = response.DataAsTexture2D;
        }
    }

    public void StartLiveShow()
    {
        Debug.Log("开始实时取景");
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StartLiveShowUrlStr), HTTPMethods.Get, StartLiveShowRequestFinished);
        request.Send();
    }

    void StartLiveShowRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        Debug.Log("StartLiveShowRequestFinished! Text received: " + response.StatusCode);

    }
    
    
    public static string UrlEncode(string str)
    {
        StringBuilder sb = new StringBuilder();
        byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
        for (int i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }

        return (sb.ToString());
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
                //BeginShootFlow();
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

    }

    void OnDisable()
    {
        if (socketioManager != null)
        {
            socketioManager.Close();
        }
    }
}
