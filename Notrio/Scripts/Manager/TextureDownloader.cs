using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TextureDownloader
{
    private string url;

    public TextureDownloader(string url)
    {
        this.url = url;
    }

    List<Action<Texture2D>> callbacks = new List<Action<Texture2D>>();
    public void Get(Action<Texture2D> callback)
    {
        callbacks.Add(callback);
        
        if(requesting)
            return;    
        CrDownloadLbAvatar();
    }

    private bool requesting = false;
    private bool finished = false;
    private Texture2D texture;
    private void CrDownloadLbAvatar()
    {
        requesting = true;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(this.url);
        www.SendWebRequest().completed += (asyncOp) =>
        {
            requesting = false;
            if(www.isHttpError || www.isNetworkError)
                return;
            finished = true;

            this.texture = DownloadHandlerTexture.GetContent(www);
            CallBackToListeners();
        };
    }

    private void CallBackToListeners()
    {
        foreach (var callback in this.callbacks)
        {
            try
            {
                callback(this.texture);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}