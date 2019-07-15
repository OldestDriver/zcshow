/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    using UnityEngine;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using Extensions;
    using NatCamU.Dispatch;

    public sealed partial class NatCorderWebGL : NatCorderOSX { // EXPERIMENTAL
        
        #region --Operations--

        public NatCorderWebGL () : base() {
            transformMat.EnableKeyword(@"PLATFORM_WEBGL");
            Debug.Log("NatCorder: Initialized NatCorder 1.1 WebGL backend with macOS implementation");
        }

        public override void StopRecording () {
            if (audioSource != null) audioSource.Dispose();
            #if UNITY_WEBGL && !UNITY_EDITOR
            var pathPtr = NatCorderBridge.StopRecording(); // Signature is different
            #else
            var pathPtr = IntPtr.Zero;
            #endif
            Texture2D.Destroy(framebuffer);
            audioSource = null;
            framebuffer = null;
            var callback = new GameObject("NatCorderWebGL Delegate").AddComponent<VideoDelegate>();
            callback.StartCoroutine(OnVideo(pathPtr, callback));
        }
        #endregion


        #region --Callbacks--

        private IEnumerator OnVideo (IntPtr pathPtr, VideoDelegate callback) {
            yield return new WaitUntil(() => Marshal.ReadIntPtr(pathPtr) != IntPtr.Zero);
            MonoBehaviour.Destroy(callback); // We don't need this anymore
            pathPtr = Marshal.ReadIntPtr(pathPtr);
            var path = Marshal.PtrToStringAnsi(pathPtr);
            videoCallback(path);
        }

        private class VideoDelegate : MonoBehaviour {}
        #endregion
    }
}