using BestHTTP;
using BestHTTP.SocketIO;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class TestCameraScript : MonoBehaviour
{
    SocketManager socketioManager;
    SocketManager socketCaptureMovieManager;    // 实时取景Socket

    private bool _cameraAddSuccess = false;
    private bool _camfiConnectSuccess = false;

    private HTTPRequest _GetCameraConfigRequest;

    bool _testLock = false;


    //Queue<VideoDecodeTask> videoDecodeTasks = new Queue<VideoDecodeTask>();
    


    [SerializeField] RawImage _screen;


    private void Reset() {
        _cameraAddSuccess = false;
        _camfiConnectSuccess = false;
        //videoDecodeTasks = new Queue<VideoDecodeTask>();
    }


    // Start is called before the first frame update
    void Start()
    {
        Reset();

        //      连接卡菲
        socketioManager = new SocketManager(new Uri(CamfiServerInfo.SockIOUrlStr));

        InitSocketIOManager();

        socketioManager.Open();


    }

    // Update is called once per frame
    void Update()
    {
        // 读取视频流

        // 解码视频流并显示

        if (Input.GetKeyDown(KeyCode.L)) {

            InitLiveView();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {

            GetCameraConfig();
        }

        //if (videoDecodeTasks.Count > 0) {
        //    videoDecodeTasks.Dequeue().Run();
        //}

    }

    /// <summary>
    ///     初始化实时取景
    /// </summary>
    private void InitLiveView() {
        Debug.Log("尝试实时取景！");

        if (!_cameraAddSuccess) {
            Debug.Log("相机未正常连接，无法取景");
            return;
        }


        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StartLiveShowUrlStr), HTTPMethods.Get, onCaptureMovie);
        request.Send();

        Debug.Log("Show Live Video");

    }


    private System.Net.Sockets.Socket mediaSocket;
    private Thread mediaThread;


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
            if (response.StatusCode == 200) {

                VideoReceiver videoReceiver = new VideoReceiver(onVideoFramePrepared);
                videoReceiver.Receive();

            }
            else {
                // 实时取景失败
            }
        }
        else {
            // 连接实时取景失败
        }
    }

    void onVideoFramePrepared(byte[] content)
    {
        Debug.Log("On Video Frame Prepared");


        if (!_testLock) {
            _testLock = true;

            //VideoDecodeTask videoDecodeTask = new VideoDecodeTask(content, _screen);
            //videoDecodeTasks.Enqueue(videoDecodeTask);

            ScreenFactoryInvoker.AddCommand(new VideoDecodeTask(content, _screen));


             _testLock = false;

        }

    }

    int fid = 0;

    private void SaveBytesToFile(byte[] bytes) {

        string path = "E:\\workspace\\zcshow\\Assets\\Files\\sample\\" + fid + ".txt";
        FileStream file = File.Open(path, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        fid++;
    }


    #region Socket Manager
    void InitSocketIOManager()
    {
        socketioManager.Socket.On("connect", OnConnected);
        socketioManager.Socket.On("disconnect", OnDisConnected);
        socketioManager.Socket.On("camera_add", OnCameraAdd);
        socketioManager.Socket.On("camera_remove", OnCameraRemove);
        socketioManager.Socket.On("liveshow_error", OnLiveshowError);
        //socketioManager.Socket.On("file_added", OnFileAdded);

    }

    void OnConnected(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接建立");

        // 建立 Camfi 连接后，检查Camfi是否已连接相机
        GetCameraConfig();


    }
    void OnDisConnected(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("与CamFi的SokectIO连接 断开");
    }
    void OnCameraAdd(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi连接了相机");
        _cameraAddSuccess = true;
    }

    void OnCameraRemove(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("CamFi丢失相机连接");
        _cameraAddSuccess = false;

    }
    void OnLiveshowError(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogWarning("On Liveshow Error");
    }

    //void OnCameraConfig(Socket socket, Packet packet, params object[] args)
    //{
    //    Debug.LogWarning("CamFi丢失相机连接");

    //}
    //void OnFileAdded(Socket socket, Packet packet, params object[] args)
    //{

    //    string pathInCamfi = args[0].ToString();
    //    string fileUrl = CamfiServerInfo.CamFiGetImageByEnCodeUrlStr(UrlEncode(pathInCamfi));
    //    if (CamfiFileAdd != null) CamfiFileAdd.Invoke(fileUrl);
    //    Debug.Log("照片添加，其Url: " + pathInCamfi);
    //}
    #endregion


    private void GetCameraConfig() {

        //_GetCameraConfigRequest = new HTTPRequest(new Uri(CamfiServerInfo.GetVersionUrlStr), HTTPMethods.Get,onGetCameraConfigSuccess);
        _GetCameraConfigRequest = new HTTPRequest(new Uri(CamfiServerInfo.GetConfigUrlStr), HTTPMethods.Get,onGetCameraConfigSuccess);
        _GetCameraConfigRequest.Send();

    }


    private void onGetCameraConfigSuccess(HTTPRequest request, HTTPResponse response)
    {

        if (RequestIsSuccess(request, response))
        {
            if (!response.IsSuccess) {
                Debug.Log("response is not success : " + response.StatusCode + " | " + response.Message);
                _cameraAddSuccess = false;

                // 当相机进入休眠状态后，会返回500错误

                return;
            }
            
            string result = request.Response.DataAsText;
            JsonData rData = JsonMapper.ToObject(result);

            Debug.Log("result : " + result);

            if (!rData.Keys.Contains("message"))
            {

                Debug.Log("相机连接正常");
                _cameraAddSuccess = true;
            }
            else
            {
                Debug.Log("相机未连接");
                _cameraAddSuccess = false;
            }
        }
        else {
            _cameraAddSuccess = false;
        }

        //response.Data

    }


    private bool RequestIsSuccess(HTTPRequest request, HTTPResponse resp) {
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


    void OnDestroy() {
        Debug.Log("On Destory !");
        CloseVideoStream();
        //_GetCameraConfigRequest.Reset();
    }



    /// <summary>
    /// 关闭视频流
    /// </summary>
    private void CloseVideoStream() {
        HTTPRequest request = new HTTPRequest(new Uri(CamfiServerInfo.StopLiveShowUrlStr), HTTPMethods.Get, onCloseVideoStream);
        request.Send();
    }

    private void onCloseVideoStream(HTTPRequest request, HTTPResponse resp) {

    }


}
