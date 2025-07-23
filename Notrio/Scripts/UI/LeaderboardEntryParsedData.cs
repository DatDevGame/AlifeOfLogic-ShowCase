using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public class LeaderboardEntryParsedData : IDisposable
    {
        public string debugId;
        public string playerId;
        public int rank;
        public string playerName;
        public Texture2D avatar;
        public string avatarUrl;
        public string primaryInfo;
        public string secondaryInfo;
        public Sprite secondaryIcon;
        public Sprite topIcon;
        public bool destroyAvatarOnDispose;
        public bool isLoadingAvatar;
        public bool hasSecondaryInfo;
        public bool isCurrentPlayerEntry;
        public string flagCode;
        public object primaryInfoData;
        public bool displayExpSlider;

        public void Dispose()
        {
            try
            {
                if (destroyAvatarOnDispose && avatar != null)
                    GameObject.Destroy(avatar);
            }
            catch { }
        }
    }
}