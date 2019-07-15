## NatCorder 1.1f1
+ We have added a native macOS backend! The NatCorder recording API is now fully supported on macOS.
+ We have also added a native Windows backend! This backend is still experimental so it should not be used in production builds.
+ The Standalone backend (using FFmpeg) has been deprecated because we have added native Windows and macOS implementations.
+ We have significantly improved recording stability on Android especially for GPU-bound games.
+ Added support for different Unity audio DSP latency modes.
+ Added `sampleCount` property in `IAudioSource` interface.
+ Fixed crash when `StartRecording` is called on iOS running OpenGL ES2 or ES3.
+ Fixed rare crash on Android when recording with audio.
+ Fixed audio-video timing discrepancies on Android.
+ Fixed video tearing on Android when app does not use multithreaded rendering.
+ Fixed `FileUriExposedException` when `Sharing.Share` is called on Android 24 or newer.
+ Fixed `Sharing.GetThumbnail` not working on iOS.
+ Fixed `Sharing.SaveToCameraRoll` failing when permission is requested and approved on iOS.
+ Fixed `Sharing.SaveToCameraRoll` not working on Android.
+ Fixed rare crash on Android when a very short video (less than 1 second) is recorded.
+ Fixed build failing due to missing symbols for Sharing library on iOS.
+ Improved on microphone audio stuttering in ReplayCam example by adding minimal `Microphone` API in `NatCorderU.Extensions` namespace.
+ Refactored `Configuration.Default` to `Configuration.Screen`.
+ *Everything below*

## NatCorder 1.0f1
+ First release