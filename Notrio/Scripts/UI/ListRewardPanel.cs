using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api;
using GameSparks.Core;
namespace Takuzu
{
    public class ListRewardPanel : OverlayPanel
    {
        public GameObject container;
        public Button closeButton;
        public ListView listView;
        public RewardUI rewardUI;


        private HashSet<string> claimedRewardId;

        

        public override void Show()
        {
            container.SetActive(true);
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            container.SetActive(false);
            IsShowing = false;
            transform.SendToBack();
            onPanelStateChanged(this, false);
        }

        private void Awake()
        {
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });
            claimedRewardId = new HashSet<string>();
            listView.Init();
            listView.displayDataAction += DisplayRewardEntry;
        }

        private void OnDestroy()
        {
            listView.displayDataAction -= DisplayRewardEntry;
        }

        private void DisplayRewardEntry(GameObject g, object data)
        {
            //RewardEntry entry = g.GetComponent<RewardEntry>();
            //GSData d = data as GSData;

            //string iconName = d.GetString("iconName");
            //Sprite entryIcon = rewardUI.GetIcon(iconName);
            //entry.SetIcon(entryIcon);

            //string entryName = d.GetString("rewardName");
            //entryName = string.IsNullOrEmpty(entryName) ? "reward" : entryName;
            //entry.SetName(entryName);

            //string entrySummary = d.GetString("summary");
            //entry.SetSummary(entrySummary);

            //long coinAmount = d.GetLong("coinAmount").GetValueOrDefault(0);
            //entry.SetCoinText(string.Format("+{0}", coinAmount.ToString()));

            //string rewardId = d.GetString("rewardId");
            //bool interactable = !claimedRewardId.Contains(rewardId);
            //entry.SetInteractable(interactable);

            //entry.button.onClick.RemoveAllListeners();
            //entry.button.onClick.AddListener(delegate
            //{
            //    if (string.IsNullOrEmpty(rewardId))
            //        return;
            //    CloudServiceManager.Instance.ClaimReward(rewardId, (response) =>
            //    {
            //        if (response.HasErrors)
            //            return;
            //        bool succeed = (bool)response.ScriptData.GetBoolean("claimSuccess");
            //        if (succeed)
            //        {
            //            int amount = (int)response.ScriptData.GetInt("coinAmount");
            //            CoinManager.Instance.AddCoins(amount);
            //            entry.button.interactable = false;
            //            claimedRewardId.Add(rewardId);
            //            if (d.GetBoolean("openDetailPanel").GetValueOrDefault(false))
            //            {
            //                rewardUI.ShowDetailPanel(d);
            //            }
            //        }
            //    });
            //});
        }

        public bool HasUnclaimedReward()
        {
            return listView != null && claimedRewardId != null && listView.DataCount > claimedRewardId.Count;
        }

        public bool HasEntries()
        {
            return listView != null && listView.DataCount > 0;
        }

        public void PopulateRewardList(List<GSData> data)
        {
            listView.ClearData();
            listView.AppendData(data);
        }
    }
}