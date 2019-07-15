/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

#define PERPETUAL_CLIP // Typically more stable on mobile platforms

namespace NatCorderU.Extensions {

    using UnityEngine;
    using System;
    using Core;

    [Doc(@"Microphone"), AddComponentMenu(""), DisallowMultipleComponent]
    public static class Microphone {

        #pragma warning disable 0649
        private static AudioClip clip;
        #pragma warning restore 0649

        #region --Client API--

        /// <summary>
        /// Start recording from the microphone
        /// </summary>
        /// <param name="deviceName">Optional. Microphone device name</param>
        [Doc(@"MicrophoneStartRecording")]
        public static AudioClip StartRecording (string deviceName = null) {
            #if !UNITY_WEBGL || UNITY_EDITOR // No `Microphone` API on WebGL :(
            clip = clip ?? UnityEngine.Microphone.Start(deviceName, true, 60, 48000);
            while (UnityEngine.Microphone.GetPosition(deviceName) <= 0) ;
            #endif
            return clip;
        }

        /// <summary>
        /// Stop recording from the microphone
        /// </summary>
        /// <param name="deviceName">Optional. Microphone device name</param>
        [Doc(@"MicrophoneStopRecording")]
        public static void StopRecording (string deviceName = null) {
            #if !PERPETUAL_CLIP && (!UNITY_WEBGL || UNITY_EDITOR)
            UnityEngine.Microphone.End(deviceName);
            clip = null;
            #endif
        }
        #endregion
    }
}