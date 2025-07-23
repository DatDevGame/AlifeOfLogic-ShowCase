using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;

namespace Takuzu
{
    public class ChallengeDetailPanel : OverlayPanel
    {
        public UiGroupController controller;
        public Button closeButton;
        public Button lbButton;
        public Button playButton;
        public Text playButtonLabel;
        public Image background;
        public string dailyBackgroundName;
        public string weeklyBackgroundName;
        public float backgroundBlendingSpeed;
        public float targetBlendFraction;
        public Shader blendSpriteShader;

        [Header("Color")]
        public Color buttonActiveColor;
        public Color buttonInactiveColor;
        public Color iconActiveColor;
        public Color iconInactiveColor;

        [Header("Daily")]
        public Button dailyButton;
        public Image dailyButtonBackground;
        public Image dailyButtonIcon;
        public Text currentDateText;

        [Header("Weekly")]
        public Button weeklyButton;
        public Image weeklyButtonBackground;
        public Image weeklyButtonIcon;

        [Header("Info")]
        public Text title;
        public Text difficultyText;
        public Text dateText;
        public Text sizeText;
        public Text rewardAmount;
        public GameObject rewardGroup;
        public GameObject solvedGroup;
        public TimerCharacter[] digit;

        [Space]
        public LeaderboardController lb;

        public const string DAILY_TITLE = "DAILY PUZZLE";
        public const string WEEKLY_TITLE = "WEEKLY PUZZLE";
        public const string TIME_PREFIX = "";
        public const int TYPE_DAILY = 2;
        public const int TYPE_WEEKLY = 1;

        private string challengeId;
        private int type = 1;

        private string dailyChallengeId;
        private Puzzle dailyChallengePuzzle;
        private string dailyDateString;
        private string dailyRewardAmount;
        private int dailyChallengeState;

        private string weeklyChallengeId;
        private Puzzle weeklyChallengePuzzle;
        private string weeklyDateString;
        private string weeklyRewardAmount;
        private int weeklyChallengeState;

        public override void Show()
        {
            IsShowing = true;
            controller.ShowIfNot();
            transform.BringToFront();
            onPanelStateChanged(this, true);

            currentDateText.text = DateTime.UtcNow.Day.ToString();
            if (type != TYPE_DAILY)
                ShowWeeklyChallenge();
            else
                ShowDailyChallenge();
        }

        public override void Hide()
        {
            IsShowing = false;
            controller.HideIfNot();
            onPanelStateChanged(this, false);
        }

        private void Awake()
        {
            Material backgroundMaterial = new Material(blendSpriteShader);
            background.material = backgroundMaterial;
        }

        private void Start()
        {
            closeButton.onClick.AddListener(delegate
            {
                Hide();
            });

            dailyButton.onClick.AddListener(delegate
            {
                ShowDailyChallenge();
            });

            weeklyButton.onClick.AddListener(delegate
            {
                ShowWeeklyChallenge();
            });
        }

        private void Update()
        {
            if (controller.isShowing != true)
                return;
            System.DateTime d = System.DateTime.UtcNow;
            string s = string.Empty;
            if (type == TYPE_DAILY)
            {
                s = string.Format("{0}{1}{2}{3}",
                    0.ToString("D2"),
                    (23 - d.Hour).ToString("D2"),
                    (59 - d.Minute).ToString("D2"),
                    (59 - d.Second).ToString("D2"));
            }
            else if (type == TYPE_WEEKLY)
            {
                s = string.Format("{0}{1}{2}{3}",
                    (6 - d.DayOfWeek.ToMondayBased()).ToString("D2"),
                    (23 - d.Hour).ToString("D2"),
                    (59 - d.Minute).ToString("D2"),
                    (59 - d.Second).ToString("D2"));
            }

            for (int i = 0; i < digit.Length; ++i)
            {
                digit[i].SetNewText(s[i].ToString());
            }

            float currentBlendFraction = background.material.GetFloat("_BlendFraction");
            currentBlendFraction = Mathf.MoveTowards(currentBlendFraction, targetBlendFraction, backgroundBlendingSpeed * Time.smoothDeltaTime);
            background.material.SetFloat("_BlendFraction", currentBlendFraction);
        }

        public void SetDailyChallenge(string id, Puzzle p)
        {
            dailyChallengeId = id;
            dailyChallengePuzzle = p;
            dailyDateString = GetCreationDate(id);
            dailyRewardAmount = Judger.Instance.GetRewardForDailyChallenge(dailyChallengePuzzle.size).ToString();

            background.material.SetTexture("_SecondaryTex", Background.Get(dailyBackgroundName).texture);
        }

        public void SetWeeklyChallenge(string id, Puzzle p)
        {
            weeklyChallengeId = id;
            weeklyChallengePuzzle = p;
            weeklyDateString = GetCreationDate(id);
            weeklyRewardAmount = Judger.Instance.GetRewardForWeeklyChallenge(weeklyChallengePuzzle.size).ToString();

            background.material.SetTexture("_MainTex", Background.Get(weeklyBackgroundName).texture);
        }

        private string GetCreationDate(string id)
        {
            string[] s = id.Split('-');
            int y = int.Parse(s[1]);
            int m = int.Parse(s[2]);
            int d = int.Parse(s[3]);
            DateTime date = new DateTime(y, m, d);

            return date.ToShortDateString();
        }

        public void ShowDailyChallenge()
        {
            dailyButtonBackground.color = buttonActiveColor;
            dailyButtonIcon.color = iconActiveColor;
            currentDateText.color = iconActiveColor;
            weeklyButtonBackground.color = buttonInactiveColor;
            weeklyButtonIcon.color = iconInactiveColor;
            //background.sprite = Background.Get(dailyBackgroundName);
            targetBlendFraction = 1;

            type = TYPE_DAILY;
            title.text = DAILY_TITLE;
            dateText.text = dailyDateString;
            difficultyText.text = Utilities.GetDifficultyDisplayName(dailyChallengePuzzle.level);
            sizeText.text = string.Format("{0}x{0}", (int)dailyChallengePuzzle.size);
            rewardAmount.text = dailyRewardAmount;

            bool isSolved = PuzzleManager.Instance.IsPuzzleSolved(dailyChallengeId);
            rewardGroup.SetActive(!isSolved);
            solvedGroup.SetActive(isSolved);
            lbButton.onClick.RemoveAllListeners();
            lbButton.onClick.AddListener(delegate
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                Hide();
                lb.Show(LeaderboardController.DAILY_LB_INDEX);
                lb.callingSource = this;
            });

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(delegate
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                Hide();
                GameManager.Instance.PlayAPuzzle(dailyChallengeId);
            });

            bool isInProgress = PuzzleManager.Instance.IsPuzzleInProgress(dailyChallengeId);
            playButtonLabel.text = isInProgress ? I2.Loc.ScriptLocalization.RESUME.ToUpper() : I2.Loc.ScriptLocalization.PLAY.ToUpper();
        }

        public void ShowWeeklyChallenge()
        {
            dailyButtonBackground.color = buttonInactiveColor;
            dailyButtonIcon.color = iconInactiveColor;
            currentDateText.color = iconInactiveColor;
            weeklyButtonBackground.color = buttonActiveColor;
            weeklyButtonIcon.color = iconActiveColor;
            //background.sprite = Background.Get(weeklyBackgroundName);
            targetBlendFraction = 0;

            type = TYPE_WEEKLY;
            title.text = WEEKLY_TITLE;
            dateText.text = weeklyDateString;
            difficultyText.text = Utilities.GetDifficultyDisplayName(weeklyChallengePuzzle.level);
            sizeText.text = string.Format("{0}x{0}", (int)weeklyChallengePuzzle.size);
            rewardAmount.text = weeklyRewardAmount;

            bool isSolved = PuzzleManager.Instance.IsPuzzleSolved(weeklyChallengeId);
            rewardGroup.SetActive(!isSolved);
            solvedGroup.SetActive(isSolved);
            lbButton.onClick.RemoveAllListeners();
            lbButton.onClick.AddListener(delegate
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                lb.Show(LeaderboardController.WEEKLY_LB_INDEX);
                Hide();
                lb.callingSource = this;
            });

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(delegate
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                Hide();
                GameManager.Instance.PlayAPuzzle(weeklyChallengeId);
            });

            bool isInProgress = PuzzleManager.Instance.IsPuzzleInProgress(weeklyChallengeId);
            playButtonLabel.text = isInProgress ? I2.Loc.ScriptLocalization.RESUME.ToUpper() : I2.Loc.ScriptLocalization.PLAY.ToUpper();
        }

#if UNITY_EDITOR
        GUIStyle style;
        private void OnDrawGizmos()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;
            if (Camera.current != null && Vector3.Distance(Camera.current.transform.position, background.transform.position) < 100)
            {
                if (style == null)
                {
                    style = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
                }

                style.normal.textColor = Color.magenta;
                style.alignment = TextAnchor.MiddleCenter;
                UnityEditor.Handles.Label(background.transform.position, "<Background image\nis assigned\nat runtime>", style);
            }
        }
#endif
    }
}