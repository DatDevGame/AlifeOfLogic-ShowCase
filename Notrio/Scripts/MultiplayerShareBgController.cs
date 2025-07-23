using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class MultiplayerShareBgController : MonoBehaviour {

    [Header("References")]
    public Camera multiPlayerCam;
    public GameObject mainContainer;
    public RawImage rawBG;
    public Text winNumberTxt;
    public Text winPrefixTxt;
    public Text loseNumberTxt;
    public Text losePrefixTxt;
    public Text loseInfoTxt;
    public GameObject winContainer;
    public GameObject loseContainer;
    public GameObject tutorialContainer;
    [Header("Config")]
    public string loseBgName;
    public string winBgName;

    void Awake()
    {
        mainContainer.SetActive(false);
    }

    public string GetSuffixesByNumber(int num)
    {
        int mod = num % 10;
        switch (mod)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";
        }
    }

    public RenderTexture TakeMultiplayerCapture(int width, int height, bool isWin)
    {
        mainContainer.SetActive(true);
        if (isWin)
        {
            winContainer.SetActive(true);
            loseContainer.SetActive(false);
            tutorialContainer.SetActive(false);

            if (PlayerInfoManager.Instance != null)
            {
                winNumberTxt.text = PlayerInfoManager.Instance.winNumber.ToString();
                winPrefixTxt.text = GetSuffixesByNumber(PlayerInfoManager.Instance.winNumber);
            }
            rawBG.texture = Resources.Load<Sprite>("bg/" + winBgName).texture;
        }
        else
        {
            winContainer.SetActive(false);
            loseContainer.SetActive(true);
            tutorialContainer.SetActive(false);
            if (PlayerInfoManager.Instance != null)
            {
                loseNumberTxt.text = PlayerInfoManager.Instance.loseNumber.ToString();
                losePrefixTxt.text = GetSuffixesByNumber(PlayerInfoManager.Instance.loseNumber);
                loseInfoTxt.text = string.Format("{0} VICTOR{1}", PlayerInfoManager.Instance.winNumber, PlayerInfoManager.Instance.winNumber > 1 ? "IES" : "Y");
            }
            rawBG.texture = Resources.Load<Sprite>("bg/" + loseBgName).texture;
        }

        multiPlayerCam.enabled = true;
        RenderTexture rt = new RenderTexture(width, height, 24);
        multiPlayerCam.targetTexture = rt;
        multiPlayerCam.Render();
        multiPlayerCam.targetTexture = null;
        multiPlayerCam.enabled = false;
        mainContainer.SetActive(false);
        return rt;
    }

    public RenderTexture TakeStoryModeCapture(int width, int height, Texture flipTexture)
    {
        mainContainer.SetActive(true);
        tutorialContainer.SetActive(false);
        loseContainer.SetActive(false);
        winContainer.SetActive(false);
        rawBG.texture = flipTexture;
        multiPlayerCam.enabled = true;
        RenderTexture rt = new RenderTexture(width, height, 24);
        multiPlayerCam.targetTexture = rt;
        multiPlayerCam.Render();
        multiPlayerCam.targetTexture = null;
        multiPlayerCam.enabled = false;
        mainContainer.SetActive(false);
        return rt;
    }

    public RenderTexture TakeTutorialCapture(int width, int height, Texture flipTexture)
    {
        mainContainer.SetActive(true);
        tutorialContainer.SetActive(true);
        loseContainer.SetActive(false);
        winContainer.SetActive(false);
        rawBG.texture = flipTexture;
        multiPlayerCam.enabled = true;
        RenderTexture rt = new RenderTexture(width, height, 24);
        multiPlayerCam.targetTexture = rt;
        multiPlayerCam.Render();
        multiPlayerCam.targetTexture = null;
        multiPlayerCam.enabled = false;
        mainContainer.SetActive(false);
        return rt;
    }

    public static Texture2D RenderTextureToTexture2dConvert(RenderTexture renderTexture)
    {
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        if (renderTexture == null)
            return texture2D;
        RenderTexture currentActive = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentActive;
        return texture2D;
    }
}
