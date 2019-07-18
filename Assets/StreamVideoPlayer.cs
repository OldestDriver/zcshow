using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class StreamVideoPlayer {

    static Texture2D texture2D = new Texture2D(200, 200);
    static bool flag = false;


    public static void Play(RawImage screen ,byte[] content) {
        //    初始化计时器
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //    计时器操作
        sw.Start();

      


        if (texture2D == null) {
            texture2D = new Texture2D(200, 200);
        }
        texture2D.LoadImage(content);

        if (!flag) {
            flag = true;
            screen.texture = texture2D;
        }

        sw.Stop();
        Debug.Log("Time : " + sw.ElapsedMilliseconds / 1000f);

    }

}