using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Responses;
using Pinwheel;
using UnityEngine.Events;
using System;

namespace Takuzu
{
    public class LeaderboardEntry : MonoBehaviour, ILeaderboardEntry
    {

        public RawImage avatar;
        public Text playerName;
        public Text primaryInfo;
        public Text secondaryInfo;
        public Text secondaryYearsInfor;
        public GameObject yearInforGroup;
        public Image secondaryIcon;
        public GameObject secondaryInfoGroup;
        public Image background;
        public RawImage fakeAvatarMask;
        public Text rank;
        public Color oddRankBgColor;
        public Color evenRankBgColor;
        public Texture2D defaultAvatar;
        public ColorAnimation anim;
        public GameObject spinner;
        public Slider expSlider;
        public Image rankFrame;
        public Vector2 playerNameHasSecondaryInfoPosition;
        public Vector2 playerNameNoSecondaryInfoPosition;

        public UnityEvent onHasSecondaryInfoTrue;
        public UnityEvent onHasSecondaryInfoFalse;

        [Header("Debug")]
        public Text debugText;

        private string playerId;
        private LeaderboardEntryParsedData data;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            if (data != null)
            {
                data.Dispose();
            }
        }

        private void Awake()
        {
            avatar.texture = defaultAvatar;
        }

        private void Update()
        {
            if (data != null)
            {
                SetAvatar(data.avatar);
                if (spinner != null)
                    spinner.SetActive(data.isLoadingAvatar);
            }
        }

        public void SetName(string name)
        {
            playerName.text = name;
        }

        public void SetPrimaryInfo(string info)
        {
            primaryInfo.text = info;
        }

        public void SetSecondaryInfo(string info)
        {
            if (info == "loading")
            {
                StartCoroutine(WaitForRealAge(info));
            }
            int maxDiff = -1;
            if (!string.IsNullOrEmpty(info))
            {
                info = info[0].ToString().ToUpper() + (info.Length > 1 ? info.Substring(1) : string.Empty);
                int.TryParse(info, out maxDiff);
            }
            secondaryInfo.enabled = maxDiff == -1;
            //yearInforGroup.SetActive(maxDiff != -1);
            if (maxDiff != -1)
            {
                secondaryYearsInfor.color = PuzzleManager.Instance.accentColors[Mathf.Clamp((int)StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff) - 1, 0, PuzzleManager.Instance.accentColors.Count - 1)];
                info = Takuzu.Utilities.GetLocalizePackNameByLevel(StoryPuzzlesSaver.GetDifficultLevelFromIndex(maxDiff));
            }
            secondaryInfo.text = info;
            secondaryYearsInfor.text = info;
        }

        private IEnumerator WaitForRealAge(string info)
        {
            yield return new WaitUntil(() => info != "loading");
            SetSecondaryInfo(info);
        }

        public void SetSecondaryIcon(Sprite icon)
        {
            secondaryIcon.sprite = icon;
            if (secondaryIcon.sprite == null)
                secondaryIcon.enabled = false;
            else
                secondaryIcon.enabled = true;
        }

        public void SetRank(int r)
        {
            rank.text = r > 0 ? r.ToString() : "?";
            if (background != null)
                background.color = r % 2 == 0 ? evenRankBgColor : oddRankBgColor;
            if (fakeAvatarMask != null)
                fakeAvatarMask.color = r % 2 == 0 ? evenRankBgColor : oddRankBgColor;
        }

        public void SetRank(string rankString)
        {
            rank.text = rankString;
        }

        public void SetAvatar(string url)
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(DownloadAvatar(url));
            else
            {
                avatar.texture = defaultAvatar;
            }
        }

        public void SetAvatar(Sprite s)
        {
            Texture2D a = new Texture2D((int)s.rect.width, (int)s.rect.height);
            Color[] c = s.texture.GetPixels(
                (int)s.textureRect.x,
                (int)s.textureRect.y,
                (int)s.textureRect.width,
                (int)s.textureRect.height);
            a.SetPixels(c);
            a.Apply();
            avatar.texture = a;
        }

        public void SetAvatar(Texture2D t)
        {
            avatar.texture = t ?? defaultAvatar;
        }

        private IEnumerator DownloadAvatar(string url)
        {
            WWW w = new WWW(url);
            yield return w;
            if (string.IsNullOrEmpty(w.error))
            {
                avatar.texture = w.texture;
            }
            else
            {
                avatar.texture = defaultAvatar;
                Debug.Log("ouch! Download avatar failed!");
            }
        }

        public void SetPlayerId(string id)
        {
            playerId = id;
            try
            {
                CloudServiceManager.Instance.RequestAvatarForPlayer(playerId, OnRequestAvatar);
            }
            catch
            {
                Debug.LogWarning("An error occurs on setting player avatar.");
            }
        }

        private void OnRequestAvatar(LogEventResponse response)
        {
            if (response.HasErrors)
            {
                Debug.LogWarning(response.Errors.JSON);
            }
            else
            {
                string url = response.ScriptData.GetString("FbAvatarUrl");
                if (!string.IsNullOrEmpty(url))
                {
                    //to handle null error the response arrives after the entry was destroyed
                    try
                    {
                        SetAvatar(url);
                    }
                    catch
                    {
                        Debug.LogWarning("An error occurs on setting player avatar.");
                    }
                }
            }
        }

        public void SetTopIcon(Sprite icon)
        {
            if (rankFrame == null)
                return;
            if (icon != null)
            {
                rankFrame.sprite = icon;
                rankFrame.color = Color.white;
            }
            else
            {
                rankFrame.color = new Color(1, 1, 1, 0);
            }

        }
        public void SetData(LeaderboardEntryParsedData parsedData)
        {
            data = parsedData;
            SetRank(parsedData.rank);
            SetName(parsedData.playerName);
            SetPrimaryInfo(parsedData.primaryInfo);
            SetSecondaryInfo(parsedData.secondaryInfo);
            SetSecondaryIcon(parsedData.secondaryIcon);
            SetHasSecondaryInfo(parsedData.hasSecondaryInfo);
            SetTopIcon(parsedData.topIcon);

            if (!string.IsNullOrEmpty(parsedData.flagCode) && parsedData.avatar == null)
            {
                SetAvatar(LeaderboardBuilder.GetFlag(parsedData.flagCode));
            }
            else
            {
                SetAvatar(parsedData.avatar);
            }

            SetDisplayExpSlider(parsedData.displayExpSlider);
            if (parsedData.displayExpSlider)
            {
                try
                {
                    int exp = (int)parsedData.primaryInfoData;
                    SetExpSliderValue(exp);
                }
                catch
                {
                    SetDisplayExpSlider(false);
                }
            }

            if (debugText != null)
            {
                string s = string.Format("<color=blue>{0}</color> :: <color=red>{1}</color>", parsedData.playerId, parsedData.debugId);
                debugText.text = s;
            }
        }

        public void SetHasSecondaryInfo(bool hasSecondaryInfo)
        {
            if (hasSecondaryInfo)
            {
                //(playerName.transform as RectTransform).anchoredPosition = playerNameHasSecondaryInfoPosition;
                if (secondaryInfoGroup != null)
                    secondaryInfoGroup.SetActive(true);
                onHasSecondaryInfoTrue.Invoke();
            }
            else
            {
                //(playerName.transform as RectTransform).anchoredPosition = playerNameNoSecondaryInfoPosition;
                if (secondaryInfoGroup != null)
                    secondaryInfoGroup.SetActive(false);
                onHasSecondaryInfoFalse.Invoke();
            }
        }

        public void SetDisplayExpSlider(bool display)
        {
            if (expSlider != null)
                expSlider.gameObject.SetActive(display);
        }

        public void SetExpSliderValue(int exp)
        {
            PlayerInfo info = ExpProfile.active.FromTotalExp(exp);
            expSlider.normalizedValue = info.NormalizedExp;
        }
    }
}