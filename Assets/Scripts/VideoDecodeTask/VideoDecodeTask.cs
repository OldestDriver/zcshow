using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoDecodeTask 
{

    private byte[] _content;
    private RawImage _screen;

    public VideoDecodeTask(byte[] content,RawImage screen) {
        _content = content;
        _screen = screen;
    }


    public void Run() {
        StreamVideoPlayer.Play(_screen, _content);
    }

    
}
