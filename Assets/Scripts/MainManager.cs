using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    [SerializeField, Header("System Config"),Range(1f,100f)] float _keepOpenTime;

    [SerializeField] CardBaseAgent card1;
    [SerializeField] CardBaseAgent card2;
    [SerializeField] CardBaseAgent card3;
    [SerializeField] CardBaseAgent card4;
    [SerializeField] CardBaseAgent card5;

    [SerializeField] RectTransform _messageBox;

    [SerializeField,Header("Manager")] CameraManager _cameraManager;
    [SerializeField] VideoFactoryAgent _videoFactoryAgent;

    [SerializeField, Header("Message")] MessageBoxAgent _messageBoxAgent;

    private List<CardBaseAgent> cards;

    private bool _camfiConnect = false;
    private bool _cameraConnect = false;


    private bool _isKeepOpen;
    private float _lastActiveTime;

    // Start is called before the first frame update
    void Awake() {




        //Display.displays[0].Activate();
        //Display.displays[0].SetRenderingResolution(1080, 1920);

        //Display.displays[1].Activate();
        //Display.displays[1].SetRenderingResolution(1920, 1080);

    }


    void Start()
    {
        Application.targetFrameRate = 60;


        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            //Screen.SetResolution(1920, 1080, true);
        }

        //for (int i = 0; i < Display.displays.Length; i++)
        //{

        //    if (i == 0)
        //    {
        //        Display.displays[0].Activate(1920, 1080, 60);
        //        //Screen.SetResolution(1920, 1080, true);
        //    }

        //    if (i == 1)
        //    {
        //        Display.displays[1].Activate(1920, 1080, 60);
        //        //Screen.SetResolution(1080, 1920, true);
        //    }
        //}

        //_messageBoxAgent.UpdateMessage("连接了" + Display.displays.Length + "个Screen");

        cards = new List<CardBaseAgent>();

        //for (int i = 0; i < Display.displays.Length; i++)
        //{
        //    Display.displays[i].Activate();

        //    //Screen.SetResolution(Display.displays[i].renderingWidth, Display.displays[i].renderingHeight, true);
        //}

        //Display.displays[0].Activate();


        _cameraManager.Init(OnConnectCamfiSuccess,OnConnectCamfiFailed,OnConnectCameraSuccess,OnConnectCameraFailed,OnCameraError);

        cards.Add(card1);
        cards.Add(card2);
        cards.Add(card3);
        cards.Add(card4);
        cards.Add(card5);

        //  装载
        foreach (CardBaseAgent card in cards) {
            card.OnUpdateHandleTime(OnUpdateHandleTime);
            card.OnKeepOpen(OnKeepOpen);
            card.OnCloseKeepOpen(OnCloseKeepOpen);
            card.OnErrorHappend(OnInnerError);
        }

        card1.DoActive();

        _lastActiveTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Reset();
            card1.DoActive();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Reset();
            card2.DoActive();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Reset();
            card3.DoActive();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Reset();
            card4.DoActive();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Reset();
            card5.DoActive();
        }

      
        if (_camfiConnect && _cameraConnect)
        {
            _messageBoxAgent.Close();
        }
        else if (!_camfiConnect)
        {
            //_messageBoxAgent.UpdateMessage("未连接 Camfi！");
        }
        else if (!_cameraConnect) {
            //_messageBoxAgent.UpdateMessage("未连接相机 ！");
        }


        // 判断是否回到首页

        if ((!_isKeepOpen) && ((Time.time - _lastActiveTime) > _keepOpenTime)) {
            // 回到首页

            ReStart();
        }





    }

    private void Reset() {
        for (int i = 0; i < cards.Count; i++) {
            cards[i].DoEnd();
        }
    }


    private void ReStart() {
        Reset();
        card1.DoActive();
    }




    private void OnConnectCamfiSuccess()
    {
        _camfiConnect = true;
        Debug.LogWarning("与CamFi的SokectIO连接");
    }

    private void OnConnectCamfiFailed()
    {
        _camfiConnect = false;

         Debug.LogWarning("与CamFi的SokectIO失败");
    }

    private void OnConnectCameraSuccess()
    {
        _cameraConnect = true;
        Debug.LogWarning("与 Camera 连接成功");
    }

    private void OnConnectCameraFailed()
    {
        _cameraConnect = false;
        Debug.LogWarning("与 Camera 连接失败");
    }



    private void OnCameraError(string message)
    {
        Debug.Log("内部发生了问题:" + message);
    }

    private void OnInnerError()
    {
        Debug.Log("内部发生了问题");
        ReStart();

    }




    private void OnUpdateHandleTime() {
        _lastActiveTime = Time.time;
    }

    private void OnKeepOpen()
    {
        _lastActiveTime = Time.time;
        _isKeepOpen = true;
    }

    private void OnCloseKeepOpen()
    {
        _lastActiveTime = Time.time;
        _isKeepOpen = false;
    }


}
