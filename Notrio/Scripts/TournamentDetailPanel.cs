using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using GameSparks.Core;
using Takuzu;
using Takuzu.Generator;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public TopRewardInfoContainer topRewardInfoContainer;
    public Image rewardInfoBG;
    public Button moreRewardInfoBtn;

    public Button playButton;
    public Button unlockButton;

    public Button lbButton;
    public Button topPlayerButton;

    public Text title;
    public Text sizeText;
    public Text levelText;
    public Text costText;
    public Text numberOfPlayer;
    public Text playBtnText;
    public Image BgImage;
    public Image lockImg;
    public Image watchAdsIcon;
    public GameObject botBtnGroup;
    public GameObject energyCostObject;
    public GameObject solvedContainer;
    public GameObject unsolvedContainer;
    public Button[] SizeTabList;

    [HideInInspector]
    public Color sovledCurrentTabBtnColor;
    [HideInInspector]
    public Color currentTabBtnColor;
    [HideInInspector]
    public Color defaultTabBtnColor;
    [HideInInspector]
    public Color defaultTabTextColor;
    [HideInInspector]
    public Color currentTabTextColor;

    [HideInInspector]
    public string currentChallengeId = "";
    [HideInInspector]
    public List<string> challengeIdInPackList = new List<string>();

    private int currentTabIndex = 0;
    private float defaultSizeYrewardInfoBG = 50;
    private float fullSizeYrewardInfoBG = 477;
    private bool isRewardFullSize = false;

    private void Start()
    {
        currentTabIndex = 0;
        ChangeStateRewardIno(isRewardFullSize);
        moreRewardInfoBtn.onClick.AddListener(() =>
        {
            isRewardFullSize = !isRewardFullSize;
            ChangeStateRewardIno(isRewardFullSize);
        });

        InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
        InAppPurchasing.RestoreCompleted += OnRestorePurchses;
        PuzzleManager.onUnlockTournamentChapter += OnUnlockTournamentChapter;
    }

    private void OnDestroy()
    {
        InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
        InAppPurchasing.RestoreCompleted -= OnRestorePurchses;
        PuzzleManager.onUnlockTournamentChapter -= OnUnlockTournamentChapter;
    }

    void OnPurchaseCompleted(IAPProduct product)
    {
        CheckSubscriptionToSetUI();
    }

    void OnRestorePurchses()
    {
        CheckSubscriptionToSetUI();
    }

    private Coroutine changeStateRewardInfoCR;
    public void ChangeStateRewardIno(bool isFullSize)
    {
        if (changeStateRewardInfoCR != null)
            StopCoroutine(changeStateRewardInfoCR);
        changeStateRewardInfoCR = StartCoroutine(CR_ChangeStateRewardInfo(isFullSize));
    }

    IEnumerator CR_ChangeStateRewardInfo(bool isFullSize)
    {
        Vector2 start = rewardInfoBG.rectTransform.sizeDelta;
        Vector2 end = (isFullSize) ? new Vector2(rewardInfoBG.rectTransform.sizeDelta.x, fullSizeYrewardInfoBG)
            : new Vector2(rewardInfoBG.rectTransform.sizeDelta.x, defaultSizeYrewardInfoBG);
        float value = 0;
        float speed = 1 / 0.15f;

        topRewardInfoContainer.IsFullSize = isFullSize;
        while (value < 1)
        {
            value += Time.deltaTime * speed;
            rewardInfoBG.rectTransform.sizeDelta = Vector2.Lerp(start, end, value);
            yield return null;
        }
    }


    public void SetupDetailPanel(List<string> idList)
    {
        challengeIdInPackList.Clear();
        challengeIdInPackList = idList;
        if (SizeTabList.Length > 0)
            SwitchSizeTab(currentTabIndex);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (String.IsNullOrEmpty(currentChallengeId))
            return;
        Puzzle p = PuzzleManager.Instance.GetChallengeById(currentChallengeId);
        BgImage.sprite = Background.Get(String.Format("bg-tournament-Back{0}", (int)p.level));
        title.text = Takuzu.Utilities.GetLocalizePackNameByLevel(p.level).ToUpper();
        lockImg.gameObject.SetActive((int)p.level > (int)StoryPuzzlesSaver.Instance.GetMaxDifficultLevel());

        sizeText.text = string.Format("{0}x{0}", (int)p.size);
        levelText.text = Utilities.GetDifficultyDisplayName(p.level);
        costText.text = EnergyManager.Instance.GetCostByLevel(p, EnergyManager.Instance.TournamentEnergyCost).ToString();
        if (PuzzleManager.Instance.IsPuzzleInProgress(currentChallengeId))
            playBtnText.text = I2.Loc.ScriptLocalization.RESUME.ToUpper();
        else
            playBtnText.text = I2.Loc.ScriptLocalization.PLAY.ToUpper();
        TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.DailyWeeklyCount, (response) =>
        {
            if (response == null)
                return;
            if (!this)
                return;
            GSData playedData = response.GetGSDataList("ChallengesCount").Find(item => (item.GetString("ChallengeId").Equals(currentChallengeId)));
            int? played = playedData != null ? playedData.GetInt("COUNT") : 0;
            numberOfPlayer.text = NumberToStringUltility.ShortenNumberCount(played ?? 0);
        });

        CheckSubscriptionToSetUI();
    }

    public void SwitchSizeTab(int index)
    {
        currentTabIndex = index;
        currentChallengeId = challengeIdInPackList[currentTabIndex];
        Puzzle currentPuzzle = PuzzleManager.Instance.GetChallengeById(currentChallengeId);
        costText.text = EnergyManager.Instance.GetCostByLevel(currentPuzzle, EnergyManager.Instance.TournamentEnergyCost).ToString();
        bool solved = PuzzleManager.Instance.IsPuzzleSolved(currentChallengeId);
        solvedContainer.SetActive(solved);
        unsolvedContainer.SetActive(!solved);
        if (PuzzleManager.Instance.IsPuzzleInProgress(currentChallengeId))
            playBtnText.text = I2.Loc.ScriptLocalization.RESUME.ToUpper();
        else
            playBtnText.text = I2.Loc.ScriptLocalization.PLAY.ToUpper();

        TournamentDataRequest.RequestTournamentPlayerCount(TournamentDataRequest.LeadboardPlayerCountType.DailyWeeklyCount, (response) =>
        {
            if (response == null)
                return;
            if (!this)
                return;
            GSData playedData = response.GetGSDataList("ChallengesCount").Find(item => (item.GetString("ChallengeId").Equals(currentChallengeId)));
            int? played = playedData != null ? playedData.GetInt("COUNT") : 0;
            numberOfPlayer.text = NumberToStringUltility.ShortenNumberCount(played ?? 0);
        });

        topRewardInfoContainer.UpdateCoinReward(currentPuzzle.size, currentPuzzle.level);

        for (int i = 0; i < SizeTabList.Length; i++)
        {
            if (i != currentTabIndex)
            {
                SizeTabList[i].GetComponent<Image>().color = defaultTabBtnColor;
                SizeTabList[i].transform.GetChild(0).GetComponent<Text>().color = defaultTabTextColor;
            }
        }
        if (!solved)
            SizeTabList[currentTabIndex].GetComponent<Image>().color = currentTabBtnColor;
        else
            SizeTabList[currentTabIndex].GetComponent<Image>().color = sovledCurrentTabBtnColor;
        SizeTabList[currentTabIndex].transform.GetChild(0).GetComponent<Text>().color = currentTabTextColor;

        CheckSubscriptionToSetUI();
    }

    public void CheckSubscriptionToSetUI()
    {
        if (InAppPurchaser.Instance.IsSubscibed() || AdDisplayer.IsAllowToShowAd() == false || PuzzleManager.Instance.IsDailyChapterUnlocked(currentChallengeId))
        {
            //energyCostObject.gameObject.SetActive(false);
            watchAdsIcon.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            unlockButton.gameObject.SetActive(false);
        }
        else
        {
            //energyCostObject.gameObject.SetActive(true);
            watchAdsIcon.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
            unlockButton.gameObject.SetActive(true);
        }
    }

    public void OnUnlockTournamentChapter()
    {
        CheckSubscriptionToSetUI();
    }
}
