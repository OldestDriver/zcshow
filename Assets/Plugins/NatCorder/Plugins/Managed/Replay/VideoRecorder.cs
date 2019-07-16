/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core {

    using UnityEngine;

    public static partial class Replay {

        #region --Op vars--
        private static VideoRecorder videoRecorder;
        #endregion


        #region --Video recording--

        [AddComponentMenu(""), DisallowMultipleComponent]
        private sealed class VideoRecorder : MonoBehaviour {

            #region --Op vars--
            public Configuration configuration { set { frameSkip = (int)Mathf.Max(Application.targetFrameRate, 30f) / value.framerate; }}
            private float timestamp;
            private int frameSkip;
            #endregion


            #region --Operations--
            
            private void OnRenderImage (RenderTexture src, RenderTexture dst) {
                // Blit to recording frame
                bool canRecordFrame = IsRecording && !IsPaused;
                if (canRecordFrame && (Time.frameCount % frameSkip) == 0) {
                    var encoderFrame = NatCorder.AcquireFrame();
                    encoderFrame.timestamp = (long)(timestamp * 1e+9f);
                    Graphics.Blit(src, encoderFrame);
                    NatCorder.CommitFrame(encoderFrame);
                }
                // Increment timestamp
                if (canRecordFrame) timestamp += Time.unscaledDeltaTime;
                // Blit to render pipeline
                Graphics.Blit(src, dst);
            }
            #endregion
        }
        #endregion
    }
}