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

    public partial class NatCorderWebGL : ISharing {

        public bool Share (string path) {
            return SharingBridge.Share(path);
        }

        public bool SaveToCameraRoll (string path) {
            return SharingBridge.SaveToCameraRoll(path);
        }

        public void GetThumbnail (string path, Action<Texture2D> callback, float time) {
            IntPtr[] thumbnailData = new IntPtr[3]; int unused = 0;
            var thumbnailHandle = GCHandle.Alloc(thumbnailData, GCHandleType.Pinned);
            SharingBridge.GetThumbnail(path, time, ref thumbnailData[0], ref unused, ref unused);
            var callbackObject = new GameObject("NatCorderWebGL Delegate").AddComponent<VideoDelegate>();
            callbackObject.StartCoroutine(GetThumbnail(thumbnailHandle, callback, callbackObject));
        }

        private IEnumerator GetThumbnail (GCHandle thumbnailHandle, Action<Texture2D> callback, VideoDelegate callbackObject) {
            yield return new WaitUntil(() => Marshal.ReadIntPtr(thumbnailHandle.AddrOfPinnedObject()) != IntPtr.Zero);
            MonoBehaviour.Destroy(callbackObject); // We don't need this anymore
            var pixelBuffer = Marshal.ReadIntPtr(thumbnailHandle.AddrOfPinnedObject());
            var width = Marshal.ReadInt32(new IntPtr(thumbnailHandle.AddrOfPinnedObject().ToInt32() + sizeof(int)));
            var height = Marshal.ReadInt32(new IntPtr(thumbnailHandle.AddrOfPinnedObject().ToInt32() + 2 * sizeof(int)));
            var thumbnail = new Texture2D(width, height, TextureFormat.RGBA32, false);
            thumbnail.LoadRawTextureData(pixelBuffer, width * height * 4);
            thumbnail.Apply();
            SharingBridge.FreeThumbnail(thumbnailHandle.AddrOfPinnedObject());
            thumbnailHandle.Free();
            callback(thumbnail);
        }
    }
}