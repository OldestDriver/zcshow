/* 
*   NatCorder
*   Copyright (c) 2017 Yusuf Olokoba
*/

namespace NatCorderU.Core.Platforms {

    using UnityEngine;
    using System;
    using Extensions;

    public sealed partial class NatCorderWindows : NatCorderOSX { // EXPERIMENTAL
        
        #region --Operations--

        public NatCorderWindows () : base() {
            Debug.Log("NatCorder: Initialized NatCorder 1.1 Windows backend with macOS implementation");
        }
        #endregion
    }
}