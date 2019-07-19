using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [SerializeField] CardBaseAgent card1;
    [SerializeField] CardBaseAgent card2;
    [SerializeField] CardBaseAgent card3;
    [SerializeField] CardBaseAgent card4;
    [SerializeField] CardBaseAgent card5;

    [SerializeField] CameraManager _cameraManager;

    private List<CardBaseAgent> cards;

    // Start is called before the first frame update
    void Start()
    {
        cards = new List<CardBaseAgent>();

        _cameraManager.Init(OnConnectCamfiSuccess,OnConnectCameraFailed,OnConnectCameraSuccess,OnConnectCameraFailed);

        //cards.Add(card1);
        //cards.Add(card2);
        //cards.Add(card3);
        //cards.Add(card4);
        //cards.Add(card5);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1)){
        //    Reset();
        //    card1.DoActive();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Reset();
        //    card2.DoActive();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    Reset();
        //    card3.DoActive();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    Reset();
        //    card4.DoActive();
        //}
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Reset();
            card5.DoActive();
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
        Debug.LogWarning("与CamFi的SokectIO连接");
    }

    private void OnConnectCamfiFailed()
    {
        Debug.LogWarning("与CamFi的SokectIO失败");
    }

    private void OnConnectCameraSuccess()
    {
        Debug.LogWarning("与 Camera 连接成功");
    }

    private void OnConnectCameraFailed()
    {
        Debug.LogWarning("与 Camera 连接失败");
    }





}
