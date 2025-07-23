using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public interface ILeaderboardEntry
    {
        void SetName(string name);
        void SetPrimaryInfo(string info);
        void SetSecondaryInfo(string info);
        void SetSecondaryIcon(Sprite icon);
        void SetRank(int r);
        void SetRank(string rankString);
        void SetAvatar(string url);
        void SetAvatar(Sprite s);
        void SetAvatar(Texture2D t);
        void SetPlayerId(string id);
    }
}