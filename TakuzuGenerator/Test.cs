using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;
using Takuzu;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EasyMobile;
using Pinwheel;
using System.Threading;

public class Test : MonoBehaviour
{
    public void ShowAd()
    {
        if (Advertising.IsInterstitialAdReady() && !Advertising.IsAdRemoved())
        {
#if UNITY_IOS
             Time.timeScale = 0;
             AudioListener.pause = true;
#endif
            Advertising.ShowInterstitialAd();
        }
    }
}
