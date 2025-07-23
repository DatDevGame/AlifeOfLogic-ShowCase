using System;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class ECAsPanelController : OverlayPanel
{
    public enum RewardType
    {
        ShareCodeReward,
        RedeemCodeReward
    }

    public static Action<RewardType> onCodeRewardClaimed = delegate { };

    [System.Serializable]
    public struct ECAsTab
    {
        public GameObject tabContainer;
        public Button button;
        public Text coinAmount;
        public Text title;
        public Text info;
        public Text describle;
        public Image line;
    }

    public static Action onGameShared = delegate {};

    public List<ECAsTab> tabList;
    public InputField codeInputField;
    //public InvitationCodeTextureGenerator iCodeTextureGenerator;
    //public ConfirmationDialog ConfirmationDialog;
    //public Button shareThisGame;
    //public Button luckyspin;
    public Button closeBtn;
    public OverlayGroupController controller;

    public static ECAsPanelController Instance;

    public const string EnterGameCodeSaveKey = "ENTER_GAME_CODE_KEY";

    [Header("Config")]
    public Color activeBtnColor;
    public Color inactiveBtnColor;

    public override void Hide()
    {
        CancelInvoke("CheckButtonState");
        controller.HideIfNot();
        IsShowing = false;
        onPanelStateChanged(this, false);
    }

    public override void Show()
    {
        tabList[0].title.text = ToCapatalize(I2.Loc.ScriptLocalization.LUCKY_SPINNER_TITLE);
        tabList[0].button.interactable = UIReferences.Instance.luckySpinPanel.IsReadyLuckySpin && UIReferences.Instance.luckySpinPanel.IsRewardedAdReady;
        tabList[0].button.image.color = tabList[0].button.interactable ? activeBtnColor : inactiveBtnColor;
        tabList[0].coinAmount.text = "???";
        tabList[0].describle.text = I2.Loc.ScriptLocalization.LUCKY_SPINNER_DESCRIPTION;

        tabList[1].title.text = ToCapatalize(I2.Loc.ScriptLocalization.FIRST_LOGIN_TITLE.ToUpper());
        tabList[1].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnFbLogin.ToString();
        tabList[1].info.text = tabList[1].button.interactable ? I2.Loc.ScriptLocalization.FIRST_LOGIN_INFO : I2.Loc.ScriptLocalization.GOT_REWARD;
        tabList[1].button.image.color = tabList[1].button.interactable ? activeBtnColor : inactiveBtnColor;
        tabList[1].describle.text = I2.Loc.ScriptLocalization.FIRST_LOGIN_DESCRIPTION;

        tabList[2].title.text = ToCapatalize(I2.Loc.ScriptLocalization.SHARE_CODE_TITLE.ToUpper());
        tabList[2].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnShareCode.ToString();
        tabList[2].describle.text = I2.Loc.ScriptLocalization.SHARE_CODE_DESCRIPTION;
        string unavailable = I2.Loc.ScriptLocalization.Unavailable_;
        tabList[2].info.text = unavailable.Substring(0, 1).ToUpper() + unavailable.Substring(1, unavailable.Length - 1);
        tabList[2].button.interactable = false;
        tabList[2].button.image.color = inactiveBtnColor;
        CloudServiceManager.Instance.GetInvitationCode(res =>
        {
            if (!string.IsNullOrEmpty(res.iCode))
            {
                tabList[2].info.text = String.Format("{0}: <b>{1}</b>", I2.Loc.ScriptLocalization.YOUR_CODE, res.iCode);
                tabList[2].button.interactable = true;
                tabList[2].button.image.color = activeBtnColor;
            }
        });

        tabList[3].title.text = ToCapatalize(I2.Loc.ScriptLocalization.ENTER_CODE_TITLE.ToUpper());
        tabList[3].button.interactable = !PlayerPrefs.HasKey(EnterGameCodeSaveKey);
        if (tabList[3].button.interactable)
            codeInputField.gameObject.SetActive(true);
        else
        {
            codeInputField.gameObject.SetActive(false);
            tabList[3].info.gameObject.SetActive(true);
            tabList[3].info.text = I2.Loc.ScriptLocalization.GOT_REWARD;
        }
        tabList[3].button.image.color = tabList[3].button.interactable ? activeBtnColor : inactiveBtnColor;
        tabList[3].describle.text = I2.Loc.ScriptLocalization.ENTER_CODE_DESCRIPTION;
        tabList[3].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnEnterCode.ToString();
        if (tabList[3].button.interactable)
            codeInputField.gameObject.SetActive(true);
        else
        {
            codeInputField.gameObject.SetActive(false);
            tabList[3].info.text = I2.Loc.ScriptLocalization.GOT_REWARD;
        }

        InvokeRepeating("CheckButtonState", 1, 1);
        controller.ShowIfNot();
        IsShowing = true;
        transform.BringToFront();
        onPanelStateChanged(this, true);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        closeBtn.onClick.AddListener(() =>{
            Hide();
        });

        if (tabList != null)
        {
            tabList[0].button.onClick.AddListener(() =>
            {
                UIReferences.Instance.luckySpinPanel.Show();
            });

            tabList[1].button.onClick.AddListener(() =>
            {
                
            });

            tabList[2].button.onClick.AddListener(() =>
            {
                ShareThisGameURL();
            });

            tabList[3].button.onClick.AddListener(() =>
            {
                ManuallyVerifyInviteCode(codeInputField.text);
            });
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {

    }

    private void ShareThisGameURL()
    {
        CloudServiceManager.Instance.GetInvitationCode(res =>
        {
            Debug.Log(res.dynamicLink);
            EasyMobile.Sharing.ShareURL(res.dynamicLink);
        });
    }

    private void ManuallyVerifyInviteCode(string iCode)
    {
        CloudServiceManager.Instance.VerifyInvitationCode(iCode, true, (success) =>
        {
            // if(!success)
            // {
            //     Debug.Log("Invitation code is invalid");
            //     //Show error code
            //     UIReferences.Instance.overlayConfirmDialog.Show(I2.Loc.ScriptLocalization.ATTENTION, I2.Loc.ScriptLocalization.ENTER_CODE_INCORRECTLY, I2.Loc.ScriptLocalization.OK, "", () => { });
            // }else
            // {
            //     CoinManager.Instance.OnInvitationCodeVerifiedSuccessfully();
            //     PlayerPrefs.SetInt(EnterGameCodeSaveKey, 1);
            // }
            //Apple Guiline doesn't allow reward new invited user
            //=> Only Verify Invitation code => if the code is valid this will reward inviter
            //=> Ignore verification's result, Do nothing on invitee side
        });
    }

    private void OpenLuckySpin()
    {
        UIReferences.Instance.luckySpinPanel.Show();
    }

    private void CheckButtonState()
    {
        tabList[0].button.interactable = UIReferences.Instance.luckySpinPanel.IsReadyLuckySpin && UIReferences.Instance.luckySpinPanel.IsRewardedAdReady;
        tabList[0].button.image.color = tabList[0].button.interactable ? activeBtnColor : inactiveBtnColor;
        tabList[0].coinAmount.text = "???";

        tabList[1].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnFbLogin.ToString();
        tabList[1].info.text = tabList[1].button.interactable ? I2.Loc.ScriptLocalization.FIRST_LOGIN_INFO : I2.Loc.ScriptLocalization.GOT_REWARD;
        tabList[1].button.image.color = tabList[1].button.interactable ? activeBtnColor : inactiveBtnColor;

        tabList[2].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnShareCode.ToString();
        CloudServiceManager.Instance.GetInvitationCode(res =>
        {
            if (!string.IsNullOrEmpty(res.iCode))
            {
                tabList[2].info.text = String.Format("{0}: <b>{1}</b>", I2.Loc.ScriptLocalization.YOUR_CODE, res.iCode);
                tabList[2].button.interactable = true;
                tabList[2].button.image.color = activeBtnColor;
            }
        });

        tabList[3].button.interactable = !PlayerPrefs.HasKey(EnterGameCodeSaveKey);
        tabList[3].button.image.color = tabList[3].button.interactable ? activeBtnColor : inactiveBtnColor;
        if (tabList[3].button.interactable)
            codeInputField.gameObject.SetActive(true);
        else
        {
            codeInputField.gameObject.SetActive(false);
            tabList[3].info.gameObject.SetActive(true);
            tabList[3].info.text = I2.Loc.ScriptLocalization.GOT_REWARD;
        }
        tabList[3].coinAmount.text = CoinManager.Instance.rewardProfile.rewardOnEnterCode.ToString();

        if (!UIReferences.Instance.luckySpinPanel.IsReadyLuckySpin)
        {
            if (UIReferences.Instance.overlayRollPanelUI.numberOfSpinLeft > 0 && UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner > 0)
            {
                String timeSpanString = new TimeSpan(0, 0, (int)UIReferences.Instance.overlayRollPanelUI.timeUntilNextSpinner).ToString();
                tabList[0].info.text = I2.Loc.ScriptLocalization.LUCKY_SPINNER_NEXT_SPIN + ": " + timeSpanString;
            }
            else
            {
                tabList[0].info.text = I2.Loc.ScriptLocalization.Unavailable_;
            }
        }
        else
        {
            if (UIReferences.Instance.luckySpinPanel.IsRewardedAdReady)
                tabList[0].info.text = I2.Loc.ScriptLocalization.LUCKY_SPINNER_INFO;
            else
                tabList[0].info.text = I2.Loc.ScriptLocalization.Unavailable_;

        }
    }

    public static string ToCapatalize(string str)
    {
        string capStr = "";
        string[] words = str.Split(' ');
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        if (words.Length == 1)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }
        else
        {
            for (int i = 0; i < words.Length; i++)
            {
                capStr += char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower() + ((i != words.Length - 1) ? " " : "");
            }
            return capStr;
        }
    }
}
