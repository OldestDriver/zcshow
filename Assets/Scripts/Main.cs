using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using System.Threading;

public class Main : MonoBehaviour
{
    [SerializeField]
    private RectTransform step1;
    [SerializeField]
    private RectTransform step2;
    [SerializeField]
    private RectTransform step3;
    [SerializeField]
    private RectTransform step4;
    [SerializeField]
    private RectTransform step5;

    //第一步
    [SerializeField]
    private RawImage videoPlayerHolder;//视频载体
    [SerializeField]
    private VideoPlayer videoPlayer;

    //第二步
    [SerializeField]
    private Text introduceText;

    //第三步
    [SerializeField]
    private Text countdownText;
    [SerializeField]
    private List<RawImage> dots;//红点
    [SerializeField]
    private Text titleText;//标题

    private int countdownNum = 3;//倒计时数字
    private int totalPicture;//图片总数

    //第四步
    [SerializeField]
    private RawImage previewVideoPlayerHolder;//视频预览
    [SerializeField]
    private VideoPlayer previewVideoPlayer;

    //第五步
    [SerializeField]
    private InputField emailInput;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayVideo());

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //step1
    //播放视频
    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        // 将texture 赋值 (必须等准备好才能赋值)
        videoPlayerHolder.texture = videoPlayer.texture;
        videoPlayer.Play();
    }

    //第一步点开始
    public void Step1Next()
    {
        videoPlayer.Stop();
        step1.transform.SetAsFirstSibling();
        introduceText.text = "";
        introduceText.DOText("请连续拍摄三张照片", 3f);
    }

    //第二步点开始
    public void Step2Next()
    {
        step2.transform.SetAsFirstSibling();
        StartShoot();
    }

    //第三步开始拍摄
    private void StartShoot()
    {
        totalPicture = 0;
        for (int i=0; i<dots.Count; i++)
        {
            dots[i].color = Color.black;
        }
        GetPicture();
        titleText.text = "准备拍摄第1张照片";
    }

    //获取照片(倒计时并拍照)
    private void GetPicture()
    {
        countdownNum = 3;
        countdownText.text = countdownNum + "";
        CountdownReduce();
    }

    private void CountdownReduce()
    {
        countdownText.transform.DOScale(0, 0.5f).OnComplete(() => {
            countdownNum--;
            countdownText.text = countdownNum + "";
            countdownText.transform.DOScale(2, 0.5f).OnComplete(() => {
                if (countdownNum == 0)
                {
                    countdownText.text = "";
                    StartCoroutine(GetPictureFromCamera());
                } else
                {
                    CountdownReduce();
                }
            });
        });
    }

    //获取相机照片
    IEnumerator GetPictureFromCamera()
    {
        //成功
        totalPicture++;
        Debug.Log("已拍" + totalPicture + "张照片");
        dots[totalPicture - 1].color = Color.red;
        yield return new WaitForSeconds(3);

        if (totalPicture < 3)
        {
            titleText.text = "准备拍摄第" + (totalPicture + 1) + "张照片";
            GetPicture();
        } else
        {
            //已拍完三张照片
            step3.transform.SetAsFirstSibling();
            StartCoroutine(PlayPreviewVideo());
        }
    }

    //第四步
    //播放预览视频
    IEnumerator PlayPreviewVideo()
    {
        previewVideoPlayer.Prepare();
        while (!previewVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(1);
            break;
        }
        // 将texture 赋值 (必须等准备好才能赋值)
        previewVideoPlayerHolder.texture = previewVideoPlayer.texture;
        previewVideoPlayer.Play();
    }

    //重新拍照
    public void Retake()
    {
        step3.transform.SetAsLastSibling();
        StartShoot();
    }

    //确认
    public void Confirm()
    {
        step4.transform.SetAsFirstSibling();
        previewVideoPlayer.Stop();
    }

    //第五步
    //发送邮件
    public void SendEmail()
    {
        Debug.Log("邮件已发送");
        emailInput.text = "";
        step5.transform.SetAsFirstSibling();
        StartCoroutine(PlayVideo());
    }

}
