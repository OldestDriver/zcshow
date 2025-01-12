﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class CustomKeyboard : MonoBehaviour
{
    public string word;

    [SerializeField]
    private InputField emailInput;
    [SerializeField]
    private Text[] letters;
    private bool isCapital = true;//是否大写
    private bool isShow = false;


    /// <summary>
    ///  上一次操作的事件
    /// </summary>
    private float _lastInputTime = 0f;


    [SerializeField,Header("输入周期")] float _inputInterval = 0.5f;


    private Action _onClickEnter;

    public void OnClickEnter(Action onClickEnter) {
        _onClickEnter = onClickEnter;
    }



    public void AlphaClick(string alpha)
    {
        if ((Time.time - _lastInputTime) > _inputInterval) {
            word = word + alpha;
            SetInputFieldText();
            _lastInputTime = Time.time;
        }


    }

    void SetInputFieldText()
    {
        emailInput.text = word;

    }


    public void Hide() {
        if (isShow)
        {
            word = "";
            //CapitalClick();
            GetComponent<RectTransform>().DOAnchorPosY(-1250, 0.3f);
            isShow = false;
        }
    }


    //显示键盘
    public void Show()
    {
        if (!isShow)
        {
            word = "";
            SetInputFieldText();
            //CapitalClick();
            GetComponent<RectTransform>().DOAnchorPosY(-150, 0.3f);
            isShow = true;
        }
    }

    //点击大小写键
    public void CapitalClick()
    {
        if (isCapital)
        {
            //变小写
            foreach(Text letter in letters)
            {
                string s = letter.text;
                letter.text = s.ToLower();
            }   
        }   else
        {
            //变大写
            foreach (Text letter in letters)
            {
                string s = letter.text;
                letter.text = s.ToUpper();
            }
        }
        isCapital = !isCapital;
    }

    //删除
    public void Delete()
    {
        Debug.Log(word.Length);
        if (word.Length > 0)
        {
            word = word.Substring(0, word.Length - 1);
            SetInputFieldText();
        }

    }

    public void ClearAll()
    {
        word = "";
        SetInputFieldText();
    }

    public void Enter()
    {
        GetComponent<RectTransform>().DOAnchorPosY(-1250, 0.3f);

        _onClickEnter?.Invoke();

        isShow = false;
    }
}
