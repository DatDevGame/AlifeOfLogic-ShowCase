using System;
using System.Collections;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using UnityEngine;
using UnityEngine.UI;
using Takuzu;

public class LeaderBoardCardView : MonoBehaviour {

    public Text text;
    public Text rank;
    public RawImage avatar;
    public Image bg;
    public Color oddRankColor;
    public Color evenRankColor;
    string avatarUrl = "";
    LeaderboardDataResponse._LeaderboardData item;
    internal void SetupCardView(LeaderboardDataResponse._LeaderboardData item)
    {
        this.item = item;
        CloudServiceManager.Instance.RequestAvatarForPlayer(item.UserId, (response) =>
        {
            if (response.HasErrors)
                return;
            string url = response.ScriptData.GetString("FbAvatarUrl");
            if (string.IsNullOrEmpty(url))
                return;
            avatarUrl = url;
        });
        if (gameObject.activeInHierarchy)
            SetupUI();
    }
    private void OnEnable()
    {
        if (item == null)
            return;
        SetupUI();
    }

    private void SetupUI()
    {
        text.text = item.UserName != "" ? item.UserName : item.UserId;
        rank.text = item.Rank.ToString();
        bg.color = (item.Rank % 2)==0?evenRankColor:oddRankColor;
        if (avatarUrl != "")
        {
            StopAllCoroutines();
            StartCoroutine(LoadLbAvatar(avatarUrl));
        }
    }

    private IEnumerator LoadLbAvatar(string url)
    {
        WWW w = new WWW(url);
        yield return w;
        if (string.IsNullOrEmpty(w.error))
        {
            avatar.texture = w.texture;
        }
    }
}
