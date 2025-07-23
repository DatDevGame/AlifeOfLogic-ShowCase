using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.Video;

public class EndingSceneManager : MonoBehaviour {


    public VideoPlayer videoPlayer;

	void Start () 
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMenuBackgroundMusic();
        }
        OndemandResourceLoader.GetAssetBundleWithCallback("video", ab =>{
            videoPlayer.clip = ab.LoadAsset<VideoClip>("ALOL_Ending_Short_Update");
            videoPlayer.Play();
            StartCoroutine(CR_CheckEndClip());
        });
    }

    IEnumerator CR_CheckEndClip()
    {
        yield return new WaitForSeconds(1f);
        while(videoPlayer.isPlaying)
        {
            yield return null;
        }

        if (SceneLoadingManager.Instance != null)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayMenuBackgroundMusic();
            SceneLoadingManager.Instance.LoadMainScene();
        }
    }
}
