using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;

namespace Takuzu
{
    [RequireComponent(typeof(Button))]
    public class BuyCoinButton : MonoBehaviour
    {
        public string packName;
        public Text amount;
        public Text price;

        private Button b;

        private void Awake()
        {
            b = GetComponent<Button>();
        }

        public void Start()
        {
            InAppPurchaser.CoinPack p = InAppPurchaser.Instance.coinPacks.ToList().Find((pack) =>
                {
                    return pack.productName.Equals(packName);
                });
            if (string.IsNullOrEmpty(p.productName))
            {
                gameObject.SetActive(false);
                return;
            }
            amount.text = p.coinValue.ToString();
            price.text = InAppPurchaser.Instance.GetPriceCoinPack(p.productName);

            b.onClick.AddListener(delegate
                {
                    InAppPurchaser.Instance.Purchase(packName);
                });
        }
    }
}