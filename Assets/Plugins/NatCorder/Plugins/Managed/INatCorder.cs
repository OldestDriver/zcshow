/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    public interface INatCorder {

        #region --Properties--
        bool IsRecording {get;}
        bool Verbose {set;}
        #endregion
        
        #region --Operations--
        void StartRecording (Configuration configuration, VideoCallback videoCallback, IAudioSource audioSource);
        void StopRecording ();
        Frame AcquireFrame ();
        void CommitFrame (Frame frame);
        void CommitSamples (float[] sampleBuffer, long timestamp);
        #endregion
    }
}