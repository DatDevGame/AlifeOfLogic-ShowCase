using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MyNativeBindings {

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern string GetSettingsURL();

    [DllImport("__Internal")]
    public static extern void OpenSettings();
#endif
}
