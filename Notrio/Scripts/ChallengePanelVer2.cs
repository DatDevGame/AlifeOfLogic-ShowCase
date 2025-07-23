using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using Takuzu.Generator;
using UnityEngine.UI;
using System;
using GameSparks.Api.Responses;
using GameSparks.Core;

public class ChallengePanelVer2 : MonoBehaviour
{
    DailyChallenges dailyChallenges;
 
    internal void SettupLeaderBoard(GSEnumerable<LeaderboardDataResponse._LeaderboardData> leaderboardDatas)
    {
        int index = 0;
        foreach (var entry in leaderboardDatas)
        {
            GameObject go = transform.GetChild(index).gameObject;
            if (go == null)
                break;
            LeaderBoardCardView cardView = go.GetComponent<LeaderBoardCardView>();
            cardView.SetupCardView(entry);
            index++;
        }
    }

}
