/* 
*   NatCam Core
*   Copyright (c) 2016 Yusuf Olokoba
*/

namespace NatCamU.Dispatch {

    using UnityEngine;
    using System;
    using System.Collections;
    
    [AddComponentMenu("")]
    public sealed class DispatchUtility : MonoBehaviour {

        #region --Events--
        public static event Action onFrame, onQuit;
        public static event Action<bool> onPause;
        #endregion


        #region --State--
        private static DispatchUtility instance;

        static DispatchUtility () {
            instance = new GameObject("NatCam Dispatch Utility").AddComponent<DispatchUtility>();
            instance.StartCoroutine(instance.OnFrame());
        }
        #endregion


        #region --Operations--

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
        }
        
        void OnApplicationPause (bool paused) {
            if (onPause != null) onPause(paused);
        }
        
        void OnApplicationQuit () {
            if (onQuit != null) onQuit();
        }

        IEnumerator OnFrame () {
            YieldInstruction yielder = new WaitForEndOfFrame();
            for (;;) {
                yield return yielder;
                if (onFrame != null) onFrame();
            }
        }
        #endregion
    }
}