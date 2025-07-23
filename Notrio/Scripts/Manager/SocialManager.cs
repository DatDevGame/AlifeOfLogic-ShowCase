using EasyMobile;
using Pinwheel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Takuzu
{
    public class SocialManager : MonoBehaviour
    {
        public static SocialManager Instance { get; private set; }

        public static Action onFbInitialized = delegate
        {
        };
        public static Action<bool> onFbLogin = delegate
        {
        };
        public static Action onFbLogout = delegate
        {
        };
        public static Action<bool> onFbShared = delegate
        {
        };
        public static Action onFailConnectFb = delegate
        {
        };
        [Header("Facebook")]
        public bool useFB;
        public string appLinkUrl;

        public const string giphyApiKey = "Neba69DgCjHGw";
        public const string giphyChannel = "Latte_Games";

        [Header("Other")]
        public const string screenShotAnimGameObjectName = "ScreenshotAnimGO";
        public ColorAnimation screenshotAnim;
        public ConfirmationDialog dialog;

        // Constant: keep the spooky base64 value at bay!
        public const string APP_PREVIEW_IMG_URL = "https://imgur.com/0s6JHuQ";

        public bool IsLoggedInFb
        {
            get
            {
                return false;
            }
        }

        public bool CanShareFB
        {
            get
            {
                return false;
            }
        }


        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode mode)
        {
            StartCoroutine(CrOnSceneLoaded());
        }

        IEnumerator CrOnSceneLoaded()
        {
            yield return null;
            GameObject g = GameObject.Find(screenShotAnimGameObjectName);
            if (g != null)
            {
                screenshotAnim = g.GetComponent<ColorAnimation>();
            }

            dialog = FindObjectOfType<ConfirmationDialog>();
        }

        public void InitFacebook()
        {
            useFB = true;
        }

        public void InitCallback()
        {

        }


        public void LogoutFB()
        {
            PlayerDb.Reset();
            onFbLogout();
        }


        public void NativeShareScreenshot()
        {
            StartCoroutine(CrNativeShare());
        }

        private IEnumerator CrNativeShare()
        {
            if (screenshotAnim != null)
            {
                screenshotAnim.Play(AnimConstant.IN);
                yield return new WaitForSeconds(screenshotAnim.duration);
            }
            yield return new WaitForEndOfFrame();
            Sharing.ShareScreenshot(
                string.Format("Screenshot-{0}", DateTime.Now.ToFileTime()),
                AppInfo.Instance.DEFAULT_SHARE_MSG);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void InviteFriend()
        {
            //FB.Mobile.AppInvite(new Uri(appLinkUrl), new Uri(APP_PREVIEW_IMG_URL), InviteCallback);
        }

        //! this is old way to invite a friend to an app => using FB.AppRequest instead
        //! call back from FB.AppRequest is handled using lambda expression no need for invite callback
        //! consider remove this comment block and the folowing code after finished testing
        // private void InviteCallback(IAppInviteResult result)
        // {
        //     if (!string.IsNullOrEmpty(result.Error))
        //     {
        //         Debug.LogWarning(result.Error);
        //     }
        //     else
        //     {
        //         Debug.Log(result.RawResult);
        //     }
        // }

        public void RequestUserName(Action<string> onSuccess, Action<string> onFailure = null)
        {
            string query = "me?field=name";
        }
    }
}