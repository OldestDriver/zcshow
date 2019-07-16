/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core {

    using UnityEngine;
    using System;

    public static partial class Replay {

        #region --Op vars--
        private static AudioRecorder audioRecorder;
        #endregion


        #region --Audio recording--

        [AddComponentMenu(""), DisallowMultipleComponent]
        private sealed class AudioRecorder : MonoBehaviour, IAudioSource {
            
            #region --IAudioSource--
            public int sampleRate { get { return AudioSettings.outputSampleRate; }}
            public int sampleCount {
                get {
                    int sampleCount, bufferCount;
                    AudioSettings.GetDSPBufferSize(out sampleCount, out bufferCount);
                    return sampleCount;
                }
            }
            public int channelCount { get { return (int)AudioSettings.speakerMode; }}
            #endregion


            #region --Op vars--
            public bool pause, mute;
            private double timestamp, lastTime = -1; // Used to support pausing and resuming
            #endregion


            #region --Operations--

            void OnAudioFilterRead (float[] data, int channels) {
                // Calculate time
                if (!pause) timestamp += lastTime > 0 ? AudioSettings.dspTime - lastTime : 0f;
                lastTime = AudioSettings.dspTime;
                // Send to NatCorder for encoding
                NatCorder.CommitSamples(data, (long)(timestamp * 1e+9f));
                if (mute) Array.Clear(data, 0, data.Length);
            }

            void IDisposable.Dispose () {
                Destroy(this);
            }
            #endregion
        }
        #endregion
    }
}