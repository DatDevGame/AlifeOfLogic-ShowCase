using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvitationCodeTextureGenerator : MonoBehaviour
{
    public Text invitationCodeText;
    public Camera mCam;
    public void SetText(string text){
        invitationCodeText.text = text;
    }
    public Texture getInvitationCodeTexture(){
        RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        rt.Create();
        mCam.targetTexture = rt;
        mCam.Render();
        RenderTexture.active = rt;
        Texture2D tt = new Texture2D((int)rt.width, (int)rt.height, TextureFormat.RGB24, false);
        tt.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tt.Apply();
        return tt;
    }
}
