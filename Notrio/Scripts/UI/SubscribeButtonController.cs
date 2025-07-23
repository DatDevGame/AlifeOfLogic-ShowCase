using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class SubscribeButtonController : MonoBehaviour
    {
        public RectTransform group;
        public ExtrudedButton subscribeBtn;
        public Text subscriptionName;
        public Text priceTxt;
        public GameObject subscribedIcon;

        public void SetData(bool isSubscribed, Color btnColor)
        {
            // subscriptionName.text = name.ToUpper();
            // priceTxt.text = price.ToUpper();
            subscribeBtn.image.color = btnColor;

            if (isSubscribed)
            {
                group.localPosition = new Vector2(0, -4);
                subscribeBtn.interactable = false;
                subscribedIcon.SetActive(true);

            }
            else
            {
                group.localPosition = Vector2.zero;
                subscribeBtn.interactable = true;
                subscribedIcon.SetActive(false);
            }
        }
    }
}
