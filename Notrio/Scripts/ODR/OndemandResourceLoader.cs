using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class OndemandResourceLoader : MonoBehaviour {
	private static OndemandResourceLoader Instance;
	private Action ODRLoaderInitialized = delegate {};

	void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
		}
		else
		{
			if(Instance != this){
				DestroyImmediate(this);
				return;
			}
		}

        DontDestroyOnLoad(gameObject);
	}

    public class Request
    {
        public class Status
        {
#if !UNITY_EDITOR && UNITY_IOS
            public OnDemandResourcesRequest request;
#else
            public UnityWebRequest request;
#endif
            public float progress = 0.0f;
            public bool finished = false;
            public AssetBundle ab = null;
        }
        public class Information
        {
            public List<Action<AssetBundle>> callbacks = new List<Action<AssetBundle>>();
            public string assetName;
            public int reTryCount;
        }
        public Information information;
        public Status status = new Status();
        private OndemandResourceLoader loader;
        public Request(string name,int reTryCount, OndemandResourceLoader loader)
        {
            information = new Information() { assetName = name , reTryCount = reTryCount};
            status = new Status();
            this.loader = loader;
        }
        public void StartRequest(bool forceRequest = false)
        {
            if(information == null)
            {
                Debug.Log("On demand resource request information is null cancel request");
                status.finished = true;
                return;
            }
            if(loader == null)
            {
                Debug.Log("On demand resource loader is null cancel request");
                return;
            }
            if (status.ab != null && forceRequest == false)
            {
                Debug.Log("ODR reuqest is already finished cancel request");
                return;
            }
            Debug.Log("Starting new Request");
#if !UNITY_EDITOR && UNITY_IOS
            StartIOSOnDemandResourceRequest();
#else
            StartWebRequestOnDemandResoureRequest();
#endif
        }

#if !UNITY_EDITOR && UNITY_IOS
        private void StartIOSOnDemandResourceRequest()
        {
            loader.StartCoroutine(IOSOnDemandResourceRequestCR());
        }

        private IEnumerator IOSOnDemandResourceRequestCR()
        {
            Debug.Log("Starting ODR request");
            bool requestSuccess = false;
            yield return null;
            status.request = OnDemandResources.PreloadAsync(new string[] { information.assetName });
            if (status.request == null)
            {
                Debug.Log("ODR fail to create request, Retry");
                StartIOSOnDemandResourceRequest();
                yield break;
            }
            while (status.request.isDone == false)
            {
                if (Math.Abs(status.progress - status.request.progress) > 0.01f)
                    Debug.LogFormat("Downloading {0} ODR:: {1}", information.assetName, status.progress);
                status.progress = status.request.progress;
                yield return null;
            }
            if(string.IsNullOrEmpty(status.request.error) == false)
            {
                Debug.Log("ODR request fail:: " + status.request.error);
            }
            else
            {
                requestSuccess = true;
                Debug.Log("ODR request success");
                status.ab = AssetBundle.LoadFromFile("res://" + information.assetName);
            }
            if (requestSuccess == false)
            {
                if (information.reTryCount > 0)
                {
                    information.reTryCount--;
                    Debug.Log("Request failed, Retry");
                    StartIOSOnDemandResourceRequest();
                    yield break;
                }
                if (information.reTryCount == -1)
                {
                    Debug.Log("Request failed, Retry");
                    StartIOSOnDemandResourceRequest();
                    yield break;
                }
                Debug.Log("Request failed cancel request");
                status.finished = true;
            }
            FinishedRequest();
        }
#else

        private void StartWebRequestOnDemandResoureRequest()
        {
            loader.StartCoroutine(WebRequestOnDemandResourceRequestCR());
        }

        private IEnumerator WebRequestOnDemandResourceRequestCR()
        {
            Debug.Log("Starting ODR web request");
            yield return null;

            string url = CombinePaths(Application.streamingAssetsPath, "ODR", information.assetName);
            if (!url.Contains("://") && !url.Contains(":///"))
            {
                url = string.Format("File://{0}", url);
            }

            bool requestSuccess = false;
            status.request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0, 0);
            status.request.SendWebRequest();
            while (status.request.isDone == false)
            {
                status.progress = status.request.downloadProgress;
                Debug.LogFormat("Downloading {0} ODR:: {1}", information.assetName, status.progress);
                yield return null;
            }

            if (status.request.isHttpError || status.request.isNetworkError)
            {
                Debug.Log("ODR request fail:: " + status.request.error);
            }
            else
            {
                requestSuccess = true;
                Debug.Log("ODR request success");
                Debug.Log("Unload memory odr bundle try to load from cache");
                status.request.Dispose();

                status.request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0, 0);
                status.request.SendWebRequest();
                while(status.request.isDone == false)
                {
                    yield return null;
                }
                status.ab = DownloadHandlerAssetBundle.GetContent(status.request);

                status.request.Dispose();
            }
            if (requestSuccess == false)
            {
                if (information.reTryCount > 0)
                {
                    information.reTryCount--;
                    Debug.Log("Request failed, Retry");
                    StartWebRequestOnDemandResoureRequest();
                    yield break;
                }
                if (information.reTryCount == -1)
                {
                    Debug.Log("Request failed, Retry");
                    StartWebRequestOnDemandResoureRequest();
                    yield break;
                }
                Debug.Log("Request failed cancel request");
                status.finished = true;
            }
            FinishedRequest();
        }
#endif
        public static string CombinePaths(string first, params string[] others)
        {
            // Put error checking in here :)
            string path = first;
            foreach (string section in others)
            {
                path = Path.Combine(path, section);
            }
            return path;
        }

        private void FinishedRequest()
        {
            status.progress = 1.0f;
            status.finished = true;
            Debug.Log("ODR Finished");
            foreach (var cb in information.callbacks)
            {
                if(cb != null)
                {
                    try
                    {
                        cb(status.ab);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("ODR some errs occurs when try to callback to requested object:: \n"+ ex.ToString() +"\n"+ ex.StackTrace);
                    }
                }
            }
            information.callbacks.Clear();
        }
    }

    private static Dictionary<string, Request> ODRRequests = new Dictionary<string, Request>();

    public static bool IsBundleLoaded(string bundleName){
        if (ODRRequests.ContainsKey(bundleName))
        {
            if(ODRRequests[bundleName].status.finished)
            {
                if (ODRRequests[bundleName].status.request == null)
                {
                    return false;
                }
                if (ODRRequests[bundleName].status.ab != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

	public static AssetBundle GetAssetBundle(string bundleName){
        Debug.Log("GetAssetBundle");
		if(IsBundleLoaded(bundleName) == false)
			return null;
        return ODRRequests[bundleName].status.ab;
	}

    public static Request LoadAssetsBundle(string bundleName, int reTryCount = 0)
    {
        Debug.Log("LoadAssetsBundle");
        if(ODRRequests.ContainsKey(bundleName))
        {
            if (ODRRequests[bundleName].status.finished == true)
            {
                if (ODRRequests[bundleName].status.ab != null)
                {
                    return ODRRequests[bundleName];
                }
                else
                {
                    ODRRequests.Remove(bundleName);
                }
            }
            else
            {
                return ODRRequests[bundleName];
            }
        }

        Request rq = new Request(bundleName, reTryCount, GetOnDemandResourceLoader());
        ODRRequests.Add(bundleName, rq);

        rq.StartRequest();
        return rq; 
    }

    public static void GetAssetBundleWithCallback(string bundleName, Action<AssetBundle> callback)
    {
        Debug.Log("GetAssetBundleWithCallback");
        Request rq = LoadAssetsBundle(bundleName);
        if (callback == null)
            return;
        if(rq.status.ab != null)
        {
            callback(rq.status.ab);
            return;
        }
        rq.information.callbacks.Add(callback);
    }

    private static OndemandResourceLoader GetOnDemandResourceLoader()
    {
        if(Instance == null){
			GameObject go = new GameObject();
			go.name = "ODR_Loader";
			Instance = go.AddComponent<OndemandResourceLoader>();
            SetupLocalCaching();
            return Instance;
		}
		return Instance;
    }

    private static void SetupLocalCaching()
    {
        Caching.ClearCache();
        Caching.compressionEnabled = false;
        string cachePath = Path.Combine(Application.persistentDataPath, "BundleCache");

        if (!Directory.Exists(cachePath))
            Directory.CreateDirectory(cachePath);
        Cache newCache = Caching.AddCache(cachePath);

        if (newCache.valid)
            Caching.currentCacheForWriting = newCache;
    }
}
