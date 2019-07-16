/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Extensions {

    using System;
    using System.Runtime.InteropServices;

    public static class SharingBridge {

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        "__Internal";
        #else
        "Sharing";
        #endif

        #if INATCORDER_C
        [DllImport(Assembly, EntryPoint = "NCShare")]
        public static extern bool Share (string path);
        [DllImport(Assembly, EntryPoint = "NCSaveToCameraRoll")]
        public static extern bool SaveToCameraRoll (string path);
        [DllImport(Assembly, EntryPoint = "NCGetThumbnail")]
        public static extern bool GetThumbnail (string path, float time, ref IntPtr pixelBuffer, ref int width, ref int height);
        [DllImport(Assembly, EntryPoint = "NCFreeThumbnail")]
        public static extern void FreeThumbnail (IntPtr pixelBuffer);

        #else
        public static bool Share (string path) {return false;}
        public static bool SaveToCameraRoll (string path) {return false;}
        public static bool GetThumbnail (string path, float time, ref IntPtr pixelBuffer, ref int width, ref int height) {return false;}
        public static void FreeThumbnail (IntPtr pixelBuffer) {}
        #endif
    }
}