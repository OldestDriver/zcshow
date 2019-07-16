/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    using UnityEngine;
    using System;
    using Extensions;

    public partial class NatCorderAndroid : ISharing {

        public bool Share (string path) {
            return sharing.CallStatic<bool>("share", path);
        }

        public bool SaveToCameraRoll (string path) {
            return sharing.CallStatic<bool>("saveToCameraRoll", path);
        }

        public void GetThumbnail (string path, Action<Texture2D> callback, float time) {
            var thumbnail = sharing.CallStatic<AndroidJavaObject>("getThumbnail", path, time);
            if (!thumbnail.Call<bool>("isLoaded")) {
                Debug.LogError("NatCorder Error: Failed to get thumbnail for video at path: "+path);
                callback(null);
            }
            var pixelData = AndroidJNI.FromByteArray(thumbnail
                .Get<AndroidJavaObject>("pixelBuffer")
                .Call<AndroidJavaObject>("array")
                .GetRawObject()
            );
            var image = new Texture2D(
                thumbnail.Get<int>("width"),
                thumbnail.Get<int>("height"),
                TextureFormat.RGB565,
                false
            );
            image.LoadRawTextureData(pixelData);
            image.Apply();
            callback(image);
        }

        private AndroidJavaClass sharing { get { return _sharing = _sharing ?? new AndroidJavaClass("com.yusufolokoba.natcorder.sharing.Sharing");}}
        private AndroidJavaClass _sharing;
    }
}