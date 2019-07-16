/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Core;

    public class GreyWorld : MonoBehaviour {

        /**
        * GreyWorld Example
        * ------------------
        * This example records a WebCamTexture using the low-level `NatCorder` API
        * The WebCamTexture is recorded with a filter applied (using a shader/material)
        * When the user touches the screen, the greyness value is set to 1, making the preview become grey, and recording is started
        * Then in `Update`, we blit the WebCamTexture to encoder surfaces (NatCorder.AcquireFrame) with the greyscale material/shader
        * When the user stops pressing the screen, we revert the greyness and stop recording
        */

        [Header("UI")]
        public RawImage preview;
        public AspectRatioFitter aspectFitter;

        private WebCamTexture cameraTexture;
        private bool isPortrait;
        private float greyness;
        private const float GreySpeed = 3f;

        IEnumerator Start () {
            NatCorder.Verbose = true;
            // Request user authorization
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) yield break;
            // Start the camera
            cameraTexture = new WebCamTexture(null, 1280, 720);
            cameraTexture.Play();
            yield return new WaitUntil(() => cameraTexture.width != 16 && cameraTexture.height != 16); // Weird bug on macOS
            // Present the camera
            PresentCamera();
        }

        void PresentCamera () {
            // Borrow the GreyWorld shader because it supports rotation and mirroring
            preview.material = new Material(Shader.Find("Hidden/NatCorder/GreyWorld"));
            preview.texture = cameraTexture;
            // Orient the preview
            preview.material.SetFloat("_Rotation", cameraTexture.videoRotationAngle * Mathf.PI / 180f);
            preview.material.SetFloat("_Scale", cameraTexture.videoVerticallyMirrored ? -1 : 1);
            // Scale the preview panel
            isPortrait = cameraTexture.videoRotationAngle == 90 || cameraTexture.videoRotationAngle == 270;
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            aspectFitter.aspectRatio = isPortrait ? (float)cameraTexture.height / cameraTexture.width : (float)cameraTexture.width / cameraTexture.height;
        }

        void Update () {
            // Animate the greyness
            if (cameraTexture && preview.texture == cameraTexture) {
                var currentGreyness = preview.material.GetFloat("_Greyness");
                var targetGreyness = Mathf.Lerp(currentGreyness, greyness, GreySpeed * Time.deltaTime);
                preview.material.SetFloat("_Greyness", targetGreyness);
            }
            // Record frames
            if (NatCorder.IsRecording && cameraTexture.didUpdateThisFrame) {
                // Acquire an encoder frame
                var frame = NatCorder.AcquireFrame();
                // Blit with the preview material
                Graphics.Blit(cameraTexture, frame, preview.material);
                // Commit the encoder frame for encoding
                NatCorder.CommitFrame(frame);
            }
        }

        public void StartRecording () {
            // Become grey
            greyness = 1f;
            // If the camera is in a potrait rotation, then we swap the width and height for recording
            int recordingWidth = isPortrait ? cameraTexture.height : cameraTexture.width;
            int recordingHeight = isPortrait ? cameraTexture.width : cameraTexture.height;
            // Start recording
            NatCorder.StartRecording(new Configuration(recordingWidth, recordingHeight), OnVideo);
        }

        public void StopRecording () {
            // Revert to normal color
            greyness = 0f;
            // Stop recording
            NatCorder.StopRecording();
        }

        void OnVideo (string path) {
            Debug.Log("Saved recording to: "+path);
            #if UNITY_IOS || UNITY_ANDROID
            // Playback the video
            Handheld.PlayFullScreenMovie(path);
            #endif
        }
    }
}