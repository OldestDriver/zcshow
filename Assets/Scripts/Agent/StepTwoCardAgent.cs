using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class StepTwoCardAgent : CardBaseAgent
{

    //第二步
    //[SerializeField,Header("UI")] private Text _introduceText;

    //  挂载的使用操作信息
    private Action _OnUpdateHandleTimeAction;
    private Action _OnKeepOpenAction;
    private Action _OnCloseKeepOpenAction;

    private Action _OnErrorHappened;


    void Reset()
    {
    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

        _OnCloseKeepOpenAction.Invoke();
        _OnUpdateHandleTimeAction.Invoke();

        //_introduceText.text = "";
        CompletePrepare();
    }

    public override void DoRunIn()
    {
        Debug.Log("场景2进入");
        // 显示在首个
        GetComponent<RectTransform>().SetAsLastSibling();
        //_introduceText.DOText("请连续拍摄三张照片", 3f);
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
        Debug.Log("场景2结束");
        _OnUpdateHandleTimeAction.Invoke();

        _NextCard?.DoActive();
        CompleteDoEnd();
        gameObject.SetActive(false);
    }


    // 点击开始拍摄
    public void OnClick() {
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
