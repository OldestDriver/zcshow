using System;
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

    //  挂载的使用操作信息
    private Action _OnUpdateHandleTimeAction;
    private Action _OnKeepOpenAction;
    private Action _OnCloseKeepOpenAction;

    private Action _OnErrorHappened;


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
        _OnCloseKeepOpenAction.Invoke();
        _OnUpdateHandleTimeAction.Invoke();

        Reset();

        CompletePrepare();
    }

    public override void DoRunIn()
    {
        Debug.Log("场景1进入");

        gameObject.SetActive(true);


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
        _OnUpdateHandleTimeAction.Invoke();

        Debug.Log("场景1 结束");
        // 此处添加移除特效
        _NextCard?.DoActive();
        CompleteDoEnd();
        gameObject.SetActive(false);
    }



    public void DoClick() {
        _OnUpdateHandleTimeAction.Invoke();

        DoRunOut();
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
