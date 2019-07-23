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


        if (texture2D == null) {
            texture2D = new Texture2D(200, 200);
        }
        texture2D.LoadImage(content);

        float w = texture2D.width;
        float h = texture2D.height;

        if (!flag) {
            flag = true;
            screen.GetComponent<RectTransform>().sizeDelta = new Vector2(w * 1.5f, h * 1.5f);

            screen.texture = texture2D;
        }

    }

}