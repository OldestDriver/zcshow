/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Extensions {

    using UnityEngine;
    using System;

    public interface ISharing {
        bool Share (string path);
        bool SaveToCameraRoll (string path);
        void GetThumbnail (string path, Action<Texture2D> callback, float time);
    }
}