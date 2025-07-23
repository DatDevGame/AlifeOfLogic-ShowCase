using System;
using System.Collections;
using System.Collections.Generic;
using GameSparks.Api.Responses;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class RewardListUI : MonoBehaviour {
    public Transform scrollListContainer;
    public Button closeButton;
    public OverlayGroupController controller;
    public GameObject rewardCardTemplate;

    private void Awake()
    {
        TopLeaderBoardReward.topChallengeRewardListChanged += OnTopChallengeRewardListChanged;

        closeButton.onClick.AddListener(delegate
        {
            controller.HideIfNot();
        });
    }
    private void OnDestroy()
    {
        TopLeaderBoardReward.topChallengeRewardListChanged -= OnTopChallengeRewardListChanged;
    }

    private void Start()
    {
        UpdateRewardUI();
    }
    private void OnTopChallengeRewardListChanged(List<RewardInformation> arg1)
    {
        UpdateRewardUI();
    }

    private void UpdateRewardUI()
    {
        StopCoroutine("UpDateRewardUIDelay");
        StartCoroutine(UpDateRewardUIDelay());
    }

    private IEnumerator UpDateRewardUIDelay()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => { return TopLeaderBoardReward.Instance != null; });
        foreach (var child in scrollListContainer.GetAllChildren())
        {
            DestroyImmediate(child.gameObject);
        }
        foreach (var reward in TopLeaderBoardReward.Instance.rewards)
        {
            GameObject rewardCardObject = Instantiate(rewardCardTemplate, scrollListContainer);
            RewardCard rewardCard = rewardCardObject.GetComponent<RewardCard>();
            rewardCard.rewardInfor = reward;
            rewardCard.mainText.text = reward.rewardId;
            rewardCard.claimRewardButton.onClick.AddListener(delegate
            {
                TopLeaderBoardReward.getReward(rewardCard.rewardInfor, getRewardCallback);
            });
        }
    }

    private void getRewardCallback(LogEventResponse obj)
    {
        Debug.Log(obj.JSONString);
        if ((bool) obj.ScriptData.GetBoolean("claimSuccess"))
        {
            int coin =(int) obj.ScriptData.GetInt("coinAmount");
            CoinManager.Instance.AddCoins(coin);
        }
        else
        {
            Debug.Log("Some error occur while trying to claim reward");
        }
        TopLeaderBoardReward.RefreshRewards();
    }
}

