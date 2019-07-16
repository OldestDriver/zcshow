/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Core;

    public class ReplayCam : MonoBehaviour {

        /**
        * ReplayCam Example
        * -----------------
        * This example records the screen using the high-level `Replay` API
        * We simply call `Replay.StartRecording` to start recording, and `Replay.StopRecording` to stop recording
        * When we want mic audio, we play the mic to an AudioSource and pass the audio source to `Replay.StartRecording`
        * -----------------
        * Note that UI canvases in Overlay mode cannot be recorded, so we use a different mode (this is a Unity issue)
        */

        [Header("UI")]
        public RawImage preview;
        public AspectRatioFitter aspectFitter;

        [Header("Recording")]
        public bool recordMicrophoneAudio;
        public AudioSource audioSource;

        private WebCamTexture cameraTexture;

        IEnumerator Start () {
            NatCorder.Verbose = true;
            // Request microphone and camera
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone)) yield break;
            // Start the WebCamTexture
            cameraTexture = new WebCamTexture(null, 1280, 720);
            cameraTexture.Play();
            yield return new WaitUntil(() => cameraTexture.width != 16 && cameraTexture.height != 16); // Workaround for weird bug on macOS
            // Display the camera
            PresentCamera();
        }

        void PresentCamera () {
            // Borrow the GreyWorld shader because it supports rotation and mirroring
            preview.texture = cameraTexture;
            preview.material = new Material(Shader.Find("Hidden/NatCorder/GreyWorld"));
            // Orient the preview panel
            preview.material.SetFloat("_Rotation", cameraTexture.videoRotationAngle * Mathf.PI / 180f);
            preview.material.SetFloat("_Scale", cameraTexture.videoVerticallyMirrored ? -1 : 1);
            // Scale the preview panel
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            if (cameraTexture.videoRotationAngle == 90 || cameraTexture.videoRotationAngle == 270)
                aspectFitter.aspectRatio = (float)cameraTexture.height / cameraTexture.width;
            else
                aspectFitter.aspectRatio = (float)cameraTexture.width / cameraTexture.height;
        }

        public void StartRecording () {
            // Create a recording configuration
            const float DownscaleFactor = 2f / 3;
            var configuration = new Configuration((int)(Screen.width * DownscaleFactor), (int)(Screen.height * DownscaleFactor));
            // Start recording with microphone audio
            if (recordMicrophoneAudio) {
                // Start the microphone
                audioSource.clip = Extensions.Microphone.StartRecording();
                audioSource.loop = true;
                audioSource.Play();
                // Start recording with microphone audio
                Replay.StartRecording(Camera.main, configuration, OnReplay, audioSource, true);
            }
            // Start recording without microphone audio
            else Replay.StartRecording(Camera.main, configuration, OnReplay);
        }

        public void StopRecording () {
            // Stop playing mic audio
            if (recordMicrophoneAudio) {
                audioSource.Stop();
                Extensions.Microphone.StopRecording();
            }
            // Stop recording
            Replay.StopRecording();
        }

        void OnReplay (string path) {
            Debug.Log("Saved recording to: "+path);
            #if UNITY_IOS || UNITY_ANDROID
            // Playback the video
            Handheld.PlayFullScreenMovie(path);
            #endif
        }
    }
}