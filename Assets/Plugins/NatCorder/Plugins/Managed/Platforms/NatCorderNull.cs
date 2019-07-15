/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    using Extensions;

    public sealed class NatCorderNull : INatCorder {

        #region --Properties--
        public bool IsRecording { get { return false; }}
        public bool Verbose { set {}}
        #endregion


        #region --Operations--

        public NatCorderNull () {
            UnityEngine.Debug.Log("NatCorder: NatCorder is not supported on this platform");
        }

        public void StartRecording (Configuration configuration, VideoCallback videoCallback, IAudioSource audioSource) {
            // We don't need the audio source
            if (audioSource != null) audioSource.Dispose();
        }

        public void StopRecording () {}

        public Frame AcquireFrame () {return null;}

        public void CommitFrame (Frame frame) {}

        public void CommitSamples (float[] sampleBuffer, long timestamp) {}
        #endregion
    }
}