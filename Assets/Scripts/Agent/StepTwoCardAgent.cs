using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class StepTwoCardAgent : CardBaseAgent
{

    //第二步
    //[SerializeField,Header("UI")] private Text _introduceText;

    void Reset()
    {
    }

    /// <summary>
    /// 准备阶段
    /// </summary>
    public override void DoPrepare() {
        Reset();

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
        _NextCard?.DoActive();
        CompleteDoEnd();
        gameObject.SetActive(false);
    }


    // 点击开始拍摄
    public void OnClick() {
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
