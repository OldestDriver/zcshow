/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Extensions {

    using UnityEngine;
    using System;
    using Core;
    using Extensions;

    [Doc(@"Sharing")]
    public static class Sharing {

        #region --Client API--

        /// <summary>
        /// Share a recorded video with the native sharing UI.
        /// Returns true if video is found and can be opened for sharing.
        /// </summary>
        /// <param name="path">Path to recorded video</param>
        [Doc(@"Share"), Ref(@"VideoCallback")]
        public static bool Share (string path) {
            if (NatCorder.Implementation is ISharing) return (NatCorder.Implementation as ISharing).Share(path);
            else Debug.LogError("NatCorder Error: Current implementation does not support Sharing API");
            return false;
        }

        /// <summary>
        /// Save a recorded video to the camera roll.
        /// Returns true if the video is found and can be saved to the camera roll.
        /// </summary>
        /// <param name="path">Path to recorded video</param>
        [Doc(@"SaveToCameraRoll"), Ref(@"VideoCallback")]
        public static bool SaveToCameraRoll (string path) {
            if (NatCorder.Implementation is ISharing) return (NatCorder.Implementation as ISharing).SaveToCameraRoll(path);
            else Debug.LogError("NatCorder Error: Current implementation does not support Sharing API");
            return false;
        }

        /// <summary>
        /// Get a thumbnail texture for a recorded video.
        /// If the thumbnail cannot be loaded, the callback will be invoked with null.
        /// </summary>
        /// <param name="path">Path to recorded video</param>
        /// <param name="callback">Callback that will be invoked with the thumbnail texture</param>
        /// <param name="time">Optional: Time to get thumbnail from in video</param>
        [Doc(@"GetThumbnail", @"GetThumbnailDiscussion"), Ref(@"VideoCallback"), Code(@"Thumbnail")]
        public static void GetThumbnail (string path, Action<Texture2D> callback, float time = 0f) {
            if (NatCorder.Implementation is ISharing) (NatCorder.Implementation as ISharing).GetThumbnail(path, callback, time);
            else Debug.LogError("NatCorder Error: Current implementation does not support Sharing API");
        }
        #endregion
    }
}