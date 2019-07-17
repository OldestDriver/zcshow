﻿using BestHTTP;
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
        socketioManager.Socket.On("camera_remove", OnCameraRemove);
        socketioManager.Socket.On("camera_add", OnCameraAdd);
        socketioManager.Socket.On("file_added", OnFileAdded);

    }
    #endregion

    #region CamFi服务器SocketIO事件回调函数
    void OnConnected(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接建立");
    }
    void OnDisConnected(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接 断开");
    }
    void OnCameraAdd(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi连接了相机");

        _cameraIsPrepared = true;

        StartLiveShow();

    }

    void OnCameraRemove(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi丢失相机连接");

        _cameraIsPrepared = false;
    }
    void OnFileAdded(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {

        string pathInCamfi = args[0].ToString();
        Debug.Log("照片添加，其Url: " + pathInCamfi);
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.CamFiGetRawDataByEnCodeUrlStr(UrlEncode(pathInCamfi))), HTTPMethods.Get, GetRawDataFinished);
        request.Send();
    }
    void OnLiveshowError(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("实时取景发送错误");
    }
    void OnLiveshowData(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        print(packet.AttachmentCount);
    }
    #endregion

    void GetRawDataFinished(HTTPRequest request, HTTPResponse response)
    {
        Debug.Log("GetRawDataFinished! Text received: " + response.StatusCode + response.Data);
        if (response.StatusCode == 200)
        {
            //获取照片成功
            Debug.Log("获取照片成功");
            _previewRawImage.texture = response.DataAsTexture2D;
        }   else
        {
            Debug.Log("获取照片失败：" + response.StatusCode + response.Data);

        }
    }

    public void StartLiveShow()
    {
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StartLiveShowUrlStr), HTTPMethods.Get, StartLiveShowRequestFinished);
        request.Send();
    }

    private System.Net.Sockets.Socket mediaSocket;
    private Thread mediaThread;

    void StartLiveShowRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        if (response.StatusCode == 200)
        {
            Debug.Log("开始实时取景成功");
            
            if (mediaSocket != null)
            {
                mediaSocket.Close();
            }
            
            mediaSocket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mediaSocket.Connect(IPAddress.Parse("192.168.9.67"), 890);
            mediaThread = new Thread(SocketReceivce);
            mediaThread.IsBackground = true;
            mediaThread.Start();
            isReceive = true;
        }   else
        {
            Debug.Log("开始实时取景失败：" + response.StatusCode);
        }
    }

    bool soi = false;
    List<byte> photoBytes = new List<byte>();
    private bool isShowImage;
    List<byte> bytes = new List<byte>();
    void SocketReceivce()
    {
        photoBytes = new List<byte>();
        while (isReceive)
        {

            byte[] data = new byte[1024*1024];
            int len = mediaSocket.Receive(data);
            if (len > 0)
            {
                //收到数据

                
                //Debug.Log("返回数据：" + BitConverter.ToString(data));

                //开始结束下标
                int start = -1;
                int end = -1;
                for (int i=0; i<data.Length-1; i++)
                {
                    if (data[i] == 0xff)
                    {
                        if (i != data.Length-1 && data[i+1] == 0xd8)
                        {
                            soi = true;
                            start = i;
                        }
                    }
                    if (data[i] == 0xff)
                    {
                        if (i != data.Length-1 && data[i+1] == 0xd9)
                        {
                            end = i;
                            soi = false;
                        }
                    }
                }
                //Debug.Log("start:" + start + "---" + "end:" + end);


                
                if (soi && (end == -1 && start == -1))
                {
                    for (int i=0; i< data.Length; i++)
                    {
                        photoBytes.Add(data[i]);
                    }
                }   
                else if ((start > -1) && (end > -1))
                {
                    for (int i=0; i<end+2; i++)
                    {
                        photoBytes.Add(data[i]);
                    }
                    isShowImage = true;
                    bytes = photoBytes;
                    photoBytes = new List<byte>();
                    for (int i=start; i< data.Length-start; i++)
                    {
                        photoBytes.Add(data[i]);
                    }
                }
                else if ((start == -1) && (end > -1))
                {
                    for (int i=0; i<end+2; i++)
                    {
                        photoBytes.Add(data[i]);
                    }
                    isShowImage = true;
                    bytes = photoBytes;
                    photoBytes = new List<byte>();
                }
                else if (soi && (start > -1) && (end == -1))
                {
                    for (int i = start; i < data.Length - start; i++)
                    {
                        photoBytes.Add(data[i]);
                    }
                }              
            
            }
        }
    }

    private static string byteToHexStr(byte[] bytes, int length)
    {
        string returnStr = "";
        if (bytes != null)
        {
            for (int i = 0; i < length; i++)
            {
                returnStr += bytes[i].ToString("X2");
            }
        }
        return returnStr;
    }

    public void StopLiveShow()
    {
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StopLiveShowUrlStr), HTTPMethods.Get, StopLiveShowRequestFinished);
        request.Send();
    }

    void StopLiveShowRequestFinished(HTTPRequest request, HTTPResponse response)
    {
        if (response.StatusCode == 200)
        {
            Debug.Log("关闭视频流成功");          
        }   else
        {
            Debug.Log("关闭视频流失败：" + response.StatusCode);
        }
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

    #region 向CamFi 发送RESTAPI的函数
    public void TakePhoto()
    {
        Debug.Log("向Camfi下达拍照指令");
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.TakePictureUrlStr), HTTPMethods.Get);
        request.Send();
    }
    #endregion

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
        if (isShowImage)
        {
            ShowImage();
        }
    }

    private void ShowImage()
    {
            byte[] data = bytes.ToArray();

        //Debug.Log("照片：" + BitConverter.ToString(data));
        Texture2D texture = new Texture2D(100, 100);
        texture.LoadImage(data);
        _previewRawImage.texture = texture;

        //if (!flag)
        //{
        //    flag = true;
        //    byte[] data = bytes.ToArray();

        //    FileStream fs = File.Open(Application.dataPath + "/1.txt", FileMode.Create);
        //    BinaryWriter writer = new BinaryWriter(fs);
        //    writer.Write(data);
        //    fs.Close();
        //    TestShowFromFile();
        //    Debug.Log("保存完成");
        //    //File.WriteAllText(Application.dataPath + "/2.txt", BitConverter.ToString(data));

        //    return;

        //}

    }

    void TestShowFromFile()
    {
        Debug.Log("测试取图");
        Texture2D texture = new Texture2D(100, 100);
        texture.LoadImage(File.ReadAllBytes(Application.dataPath + "/1.txt"));
        _previewRawImage.texture = texture;
    }

    void OnDisable()
    {
        if (socketioManager != null)
        {
            socketioManager.Close();
        }
        isReceive = false;
        StopLiveShow();
    }

    void OnEnable()
    {
        if (socketioManager != null)
        {
            socketioManager.Open();
        }

        if (_cameraIsPrepared)
        {
            StartLiveShow();
        }
    }



}
