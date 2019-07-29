using BestHTTP;
using BestHTTP.SocketIO;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Camera 管理器,需要持续链接
/// </summary>
public class CameraManager : MonoBehaviour
{
    private RawImage _liveScreen;

    SocketManager socketioManager;

    Action _onConnectCamfiAction;
    Action _onBreakCamfiAction;
    Action _onConnectCameraAction;
    Action _onBreakCameraAction;
    Action _onConnectLiveShowAction;
    Action _onBreakLiveShowAction;
    Action<byte[],Texture2D> _onFileAddAction;
    Action<string> _onFileAddErrorAction;
    
    Action _onDisconnectLiveShowSuccessAction;

    Action<string> _onCameraManagerError;   // 相机管理器内部错误回调


    private bool _connectCamfiSuccess = false;  // Cam-fi 连接状态
    private bool _connectCameraSuccess = false; //  相机 连接状态
    private bool _connectLiveShowSuccess = false;   //  实时取景  连接状态

    VideoReceiver _videoReceiver;

    private bool _currentFrameLock = false;
    private bool _showLive = false;
    private bool _hasReceiveFrame = false;
    private bool _hasReceiveFrameLock = false;

    private byte[] _currentBytes;


    #region 【公开】 初始化相机状态


    private void Reset() {
        _currentFrameLock = false;
        _showLive = false;
        _hasReceiveFrame = false;
        _hasReceiveFrameLock = false;

    }


    /// <summary>
    ///     初始化相机状态
    /// </summary>
    public void Init(Action onConnectCamfi,Action onConnectCamfiFailed,
        Action onConnectCamera,Action onConnectCameraFailed,Action<string> onCameraErrAction) {
        _onConnectCamfiAction = onConnectCamfi;
        _onBreakCamfiAction = onConnectCamfiFailed;
        _onConnectCameraAction = onConnectCamera;
        _onBreakCameraAction = onConnectCameraFailed;
        _onCameraManagerError = onCameraErrAction;

        socketioManager = new SocketManager(new Uri(CamfiServerInfo.SockIOUrlStr));
        InitSocketIOManager();

        try
        {
            GetCameraConfig();
            socketioManager.Open();
        }
        catch (Exception e)
        {
            _onCameraManagerError.Invoke(e.Message);
        }
    }

    #endregion


    #region 【公开】 初始化实时取景


    /// <summary>
    ///     初始化实施取景
    /// </summary>
    /// <param name="onConnectLiveShowSuccess"></param>
    /// <param name="onConnectLiveShowFailed"></param>
    /// <param name="screen"></param>
    public void InitLiveShow(Action onConnectLiveShowSuccess, Action onConnectLiveShowFailed, RawImage screen) {
        SetLiveScreen(screen);
        _onConnectLiveShowAction = onConnectLiveShowSuccess;
        _onBreakLiveShowAction = onConnectLiveShowFailed;
        InitLiveView();
    }

    #endregion


    #region 【公开】 显示实时

    /// <summary>
    ///     显示实时
    /// </summary>
    public void ShowLive() {
        _showLive = true;
    }

    #endregion

    #region 【公开】 关闭实时

    /// <summary>
    ///     关闭实时
    /// </summary>
    public void CloseLive() {
        _showLive = false;
    }
    #endregion


    #region 【公开】 断开实时取景

    /// <summary>
    ///     断开实时取景
    /// </summary>
    public void DisconnectLiveShow(Action onSuccessAction)
    {
        _onDisconnectLiveShowSuccessAction = onSuccessAction;

        _videoReceiver.Close();

        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StopLiveShowUrlStr), HTTPMethods.Get, OnDisconnectLiveShowRequestCallback);
        request.Send();
    }

    #endregion


    void Update() {
        // 判断是否已经实时取景
        if (_hasReceiveFrame) {
            if (!_hasReceiveFrameLock) {
                _hasReceiveFrameLock = true;
                _onConnectLiveShowAction.Invoke();
            }
            
        }



    }



    /// <summary>
    ///     运行
    /// </summary>
    public void Run()
    {


    }

    #region 【公开】 设置实时取景框

    public void SetLiveScreen(RawImage screen)
    {
        _liveScreen = screen;
    }

    #endregion


    #region 【公开】 拍摄

    public void Shoot(Action<byte[], Texture2D> onShootSuccess,Action<string> onShootError)
    {
        _onFileAddAction = onShootSuccess;
        _onFileAddErrorAction = onShootError;

        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.TakePictureUrlStr), HTTPMethods.Get, onRequestCallback);
        request.Send();
    }
    #endregion




    public byte[] GetLastScreen() {
        return _currentBytes;
    }





    /// <summary>
    ///     初始化实时取景
    /// </summary>
    private void InitLiveView()
    {
        Debug.Log("尝试实时取景！");

        if (!_connectCameraSuccess)
        {
            Debug.Log("相机未正常连接，无法取景");
            return;
        }

        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StartLiveShowUrlStr), HTTPMethods.Get, onCaptureMovie);
        request.Send();
    }

    /// <summary>
    /// 启动实时取景视频流。调用这个API，会创建一个TCP服务器，端口为890.客户端可以通过SOCKET端口，读取视频流。视频流格式为MJPEG. 
    /// 返回: 成功返回 状态代码 200 OK，否则返回状态代码500.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    void onCaptureMovie(HTTPRequest request, HTTPResponse response)
    {

        if (RequestIsSuccess(request, response))
        {
            if (response.StatusCode == 200)
            {
                Debug.Log("连接实时取景成功 : ");

                if (_videoReceiver == null) {
                    _videoReceiver = new VideoReceiver(onVideoFramePrepared);
                }
                _videoReceiver.Receive();

                _connectLiveShowSuccess = true;
            }
            else
            {
                // 实时取景失败
                _connectLiveShowSuccess = false;
            }
        }
        else
        {
            Debug.Log("连接实时取景失败 : ");
            // 连接实时取景失败
            _connectLiveShowSuccess = false;
        }

        if (!_connectLiveShowSuccess) {
            _onCameraManagerError.Invoke("_connect Live Show Error");
        }



    }

    /// <summary>
    ///     需注意，此回调在分线程中运行
    /// </summary>
    /// <param name="content"></param>
    void onVideoFramePrepared(byte[] content)
    {
        // 当第一次收到实景信号后，返回成功信号
        if (!_hasReceiveFrame)
        {
            _hasReceiveFrame = true;

            Debug.Log("收到实时信号.");

            //_onConnectLiveShowAction.Invoke();
        }


        if (!_currentFrameLock)
        {
            _currentFrameLock = true;

            _currentBytes = content;

            if (_showLive) {
                ScreenFactoryInvoker.AddCommand(new VideoDecodeTask(content, _liveScreen));
            }
            _currentFrameLock = false;
        }
    }







    #region Socket 回调
    private void InitSocketIOManager()
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
        _connectCamfiSuccess = true;
        _onConnectCamfiAction.Invoke();


    }
    void OnDisConnected(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接 断开");
        _connectCamfiSuccess = false;
        _onBreakCamfiAction.Invoke();


    }
    void OnCameraAdd(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {        
        Debug.LogWarning("CamFi连接了相机");
        _connectCameraSuccess = true;
        _onConnectCameraAction.Invoke();
    }

    void OnCameraRemove(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi丢失相机连接");
        _connectCameraSuccess = false;
        _onBreakCameraAction.Invoke();

    }
    void OnFileAdded(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        string pathInCamfi = args[0].ToString();
        //Debug.Log("拍照成功，其Url: " + pathInCamfi);
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.CamFiGetRawDataByEnCodeUrlStr(UrlEncode(pathInCamfi))), HTTPMethods.Get, GetRawDataFinished);
        request.Send();

    }
    void OnLiveshowError(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("实时取景发送错误");
    }
    void OnLiveshowData(BestHTTP.SocketIO.Socket socket, Packet packet, params object[] args)
    {
        //print(packet.AttachmentCount);
    }

    /// <summary>
    ///     获取原图
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>
    void GetRawDataFinished(HTTPRequest request, HTTPResponse response)
    {
        try
        {
            if (response.StatusCode == 200)
            {

                //获取照片成功
                Debug.Log("获取照片成功");
                _onFileAddAction.Invoke(response.Data, response.DataAsTexture2D);
            }
            else
            {
                Debug.Log("获取照片失败：" + response.StatusCode + response.Data);
                _onFileAddErrorAction.Invoke(response.DataAsText);
            }
        }
        catch (Exception ex) {
            Debug.LogError(ex.Message);
            _onCameraManagerError.Invoke(ex.Message);
        }
    }


    #endregion


    /// <summary>
    ///     获取相机配置
    /// </summary>
    private void GetCameraConfig()
    {
        //_GetCameraConfigRequest = new HTTPRequest(new Uri(CamfiServerInfo.GetVersionUrlStr), HTTPMethods.Get,onGetCameraConfigSuccess);
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.GetConfigUrlStr), HTTPMethods.Get, onGetCameraConfigSuccess);
        request.Send();
    }


    private void onGetCameraConfigSuccess(HTTPRequest request, HTTPResponse response)
    {
        if (RequestIsSuccess(request, response))
        {
            if (!response.IsSuccess)
            {
                _connectCameraSuccess = false;

                // 当相机进入休眠状态后，会返回500错误

                return;
            }

            string result = request.Response.DataAsText;
            JsonData rData = JsonMapper.ToObject(result);
            if (!rData.Keys.Contains("message"))
            {

                Debug.Log("相机连接正常");
                _connectCameraSuccess = true;
            }
            else
            {
                Debug.Log("相机未连接");
                _connectCameraSuccess = false;
            }
        }
        else
        {
            _connectCameraSuccess = false;
        }

        if (_connectCameraSuccess)
        {
            _onConnectCameraAction.Invoke();
        }
        else {
            _onBreakCameraAction.Invoke();
        }

    }


    void OnDisconnectLiveShowRequestCallback(HTTPRequest request, HTTPResponse response)
    {
        if (RequestIsSuccess(request, response))
        {
            _onDisconnectLiveShowSuccessAction.Invoke();
        }
        else {

        }
    }


    void onRequestCallback(HTTPRequest request, HTTPResponse response)
    {
    }



    private bool RequestIsSuccess(HTTPRequest request, HTTPResponse resp)
    {
        bool result = false;
        switch (request.State)
        {
            // The request finished without any problem.
            case HTTPRequestStates.Finished:
                result = true;
                break;

            // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
            case HTTPRequestStates.Error:
                Debug.Log("HTTPRequestStates Error ");
                break;

            // The request aborted, initiated by the user.
            case HTTPRequestStates.Aborted:
                Debug.LogWarning("Request Aborted!");
                break;

            // Connecting to the server is timed out.
            case HTTPRequestStates.ConnectionTimedOut:
                Debug.LogError("Connection Timed Out!");
                break;

            // The request didn't finished in the given time.
            case HTTPRequestStates.TimedOut:
                Debug.LogError("Processing the request Timed Out!");
                break;
        }
        return result;
    }

    /// <summary>
    /// $filepath 是根据list命令中返回的照片路径.传入时需要进行url编码。编码格式如下： %2Fstorage001%2FDCIM%2Fxxxx1.jpg
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
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



    void OnDestroy() {
        // 关闭与 camfi 的 socket连接
        _videoReceiver?.Close();

        socketioManager?.Close();
    }


}
