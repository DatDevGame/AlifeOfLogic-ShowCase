using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class RewardEntry : MonoBehaviour
    {
        public Image icon;
        public Text rewardName;
        public Text summary;
        public Text coinText;
        public Button button;
        public string rewardId;


        public void SetIcon(Sprite s)
        {
            icon.sprite = s;
        }

        public void SetName(string n)
        {
            rewardName.text = n.ToUpper();
        }

        public void SetSummary(string t)
        {
            summary.text = t;
        }

        public void SetCoinText(string t)
        {
            coinText.text = t;
        }

        public void SetRewardId(string id)
        {
            rewardId = id;
        }

        public void SetInteractable(bool i)
        {
            button.interactable = i;
        }
    }
}