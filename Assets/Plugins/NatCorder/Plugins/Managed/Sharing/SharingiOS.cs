/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Extensions;

    public partial class NatCorderiOS : ISharing {

        public bool Share (string path) {
            return SharingBridge.Share(path);
        }

        public bool SaveToCameraRoll (string path) {
            return SharingBridge.SaveToCameraRoll(path);
        }

        public void GetThumbnail (string path, Action<Texture2D> callback, float time) {
            IntPtr pixelBuffer = IntPtr.Zero; int width = 0, height = 0;
            if (!SharingBridge.GetThumbnail(path, time, ref pixelBuffer, ref width, ref height)) {
                Debug.LogError("NatCorder Error: Failed to get thumbnail for video at path: "+path);
                callback(null);
            }
            var thumbnail = new Texture2D(width, height, TextureFormat.BGRA32, false);
            thumbnail.LoadRawTextureData(pixelBuffer, width * height * 4);
            thumbnail.Apply();
            SharingBridge.FreeThumbnail(pixelBuffer);
            callback(thumbnail);
        }
    }
}