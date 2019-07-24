using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class StepOneCardAgent : CardBaseAgent
{
    // 视频幕布
    //[SerializeField,Header("UI")] RawImage _image;
    //[SerializeField] VideoPlayer videoPlayer;


    //private bool _videoIsPrepared = false;  // 视频加载情况标识符


    void Reset() {
        //_videoIsPrepared = false;
    }

    void Awake() {
        cardStatus = CardStatus.Active;
    }


    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        CompletePrepare();
    }

    public override void DoRunIn()
    {
        Debug.Log("场景1进入");


        // 显示在首个
        GetComponent<RectTransform>().SetAsLastSibling();


        CompleteRunIn();
    }

    /// <summary>
    /// 运行阶段
    /// </summary>
    public override void DoRun() {


    }


    public override void DoRunOut()
    {
        CompleteRunOut();
    }


    /// <summary>
    /// 结束阶段
    /// </summary>
    public override void DoEnd() {
        Debug.Log("场景1 结束");
        // 此处添加移除特效
        _NextCard?.DoActive();
        CompleteDoEnd();
        gameObject.SetActive(false);
    }



    public void DoClick() {
        DoRunOut();
    }


    public override void OnUpdateHandleTime()
    {
        Debug.Log("CardBaseAgent is runing!");
    }

    public override void OnKeepOpen()
    {
        Debug.Log("CardBaseAgent is runing!");
    }

    public override void OnCloseKeepOpen()
    {
        Debug.Log("CardBaseAgent is runing!");
    }

}
