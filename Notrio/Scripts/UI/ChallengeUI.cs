using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;
using Pinwheel;

namespace Takuzu
{
    public class ChallengeUI : MonoBehaviour
    {
        public Button challengeButton;
        public CanvasGroup challengeButtonGroup;
        public GameObject badge;
        public Text badgeNumber;
        public UiGroupController challengeDetailPanel;
        public ConfirmationDialog dialog;
        public AnimController[] badgeAnims;

        [Header("Challenge button show/hide anim")]
        public bool isChallengeButtonShowing;
        public bool isBadgeShowing;
        public float sizingSpeed;

        private Vector2 challengeButtonOriginalSize;
        private Vector2 badgeOriginalSize;
        private RectTransform challengeButtonRt;
        private RectTransform badgeRt;


        public const string DAILY_TITLE = "DAILY CHALLENGE";
        public const string DAILY_DESCRIPTION_NO_CONNECTION = "Connect to play daily challenge";
        public const string DAILY_DESCRIPTION_LOADING = "Loading challenge...";
        public const string DAILY_DESCRIPTION_NOT_PLAY = "Play the daily challenge to earn coin";
        public const string DAILY_DESCRIPTION_IN_PROGRESS = "In progress";
        public const string DAILY_DESCRIPTION_SOLVED = "Solved";

        public const string WEEKLY_TITLE = "WEEKLY CHALLENGE";
        public const string WEEKLY_DESCRIPTION_NO_CONNECTION = "Connect to play weekly challenge";
        public const string WEEKLY_DESCRIPTION_LOADING = "Loading challenge...";
        public const string WEEKLY_DESCRIPTION_NOT_PLAY = "Play the weekly challenge to earn coin";
        public const string WEEKLY_DESCRIPTION_IN_PROGRESS = "In progress";
        public const string WEEKLY_DESCRIPTION_SOLVED = "Solved";

        public const string TIME_PREFIX = "Remaining time: ";

        private void OnEnable()
        {
            PuzzleManager.onChallengeListChanged += OnDailyPuzzleListChanged;
            CloudServiceManager.onPlayerDbSyncSucceed += OnPlayerDbSynced;
            PlayerDb.Resetted += OnPlayerDbResetted;
        }

        private void OnDisable()
        {
            PuzzleManager.onChallengeListChanged -= OnDailyPuzzleListChanged;
            CloudServiceManager.onPlayerDbSyncSucceed -= OnPlayerDbSynced;
            PlayerDb.Resetted -= OnPlayerDbResetted;
        }

        private void Awake()
        {
            challengeButtonRt = challengeButton.GetComponent<RectTransform>();
            challengeButtonOriginalSize = challengeButtonRt.sizeDelta;

            badgeRt = badge.GetComponent<RectTransform>();
            badgeOriginalSize = badgeRt.sizeDelta;
        }

        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;

            challengeButton.onClick.AddListener(delegate
            {
                challengeDetailPanel.Show();
            });

            challengeButtonGroup.interactable = false;
            badge.SetActive(!IsCompleteAllChallenges());
            if (PuzzleManager.Instance.challengePuzzles != null &&
                PuzzleManager.Instance.challengePuzzles.Count > 0)
            {
                SetupChallenge(PuzzleManager.Instance.challengePuzzles);
            }
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void Update()
        {
            if (badge.activeInHierarchy && isBadgeShowing)
            {
                for (int i = 0; i < badgeAnims.Length; ++i)
                {
                    if (!badgeAnims[i].isPlaying)
                        badgeAnims[i].Play();
                }
            }
            int badgeNum = getNumberOfAvailableChallenge();
            badgeNumber.text = badgeNum > 0 ? badgeNum.ToString() : "";
            /*
            challengeButtonRt.sizeDelta = Vector2.MoveTowards(
                challengeButtonRt.sizeDelta,
                isChallengeButtonShowing ? challengeButtonOriginalSize : Vector2.zero,
                sizingSpeed);

            badgeRt.sizeDelta = Vector2.MoveTowards(
                badgeRt.sizeDelta,
                isBadgeShowing ? badgeOriginalSize : Vector2.zero,
                sizingSpeed);
            */
            SetChallengeButtonActive(HasChallenge());
        }

        public void SetBadgeActive(bool active)
        {
            isBadgeShowing = active;
        }

        public void SetChallengeButtonActive(bool active)
        {
            isChallengeButtonShowing = active;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                badge.SetActive(!IsCompleteAllChallenges());
                SetBadgeActive(!IsCompleteAllChallenges());
                if (badge.activeInHierarchy && isBadgeShowing)
                {
                    for (int i = 0; i < badgeAnims.Length; ++i)
                    {
                        if (!badgeAnims[i].isPlaying)
                            badgeAnims[i].Play();
                    }
                }
            }
        }

        private bool IsCompleteAllChallenges()
        {
            return getNumberOfAvailableChallenge()==0;
        }

        private int getNumberOfAvailableChallenge()
        {
            int N = 0;
            List<string> puzzleIds = PuzzleManager.Instance.challengeIds;
            List<Puzzle> challenges = PuzzleManager.Instance.challengePuzzles;
            if (challenges != null)
            {
                N += challenges.FindAll(challenge => challenge.level <= StoryPuzzlesSaver.Instance.GetMaxDifficultLevel()).Count;
                foreach (var challengeId in puzzleIds)
                {
                    N -= (PuzzleManager.Instance.IsPuzzleSolved(challengeId)) ? 1 : 0;
                }
            }
            return N;
        }

        private void OnDailyPuzzleListChanged(List<Puzzle> p)
        {
            SetupChallenge(p);
        }

        private void SetupChallenge(List<Puzzle> p)
        {
            challengeButtonGroup.interactable = true;
            SetBadgeActive(!IsCompleteAllChallenges());
            if (badge.activeInHierarchy && isBadgeShowing)
            {
                for (int i = 0; i < badgeAnims.Length; ++i)
                {
                    if (!badgeAnims[i].isPlaying)
                        badgeAnims[i].Play();
                }
            }
        }

        private void OnPlayerDbSynced()
        {
            SetBadgeActive(!IsCompleteAllChallenges());
            //UpdateSelector();
            //badge.SetActive(!IsCompleteAllChallenges());
            //if (badge.activeInHierarchy)
            //{
            //    for (int i = 0; i < badgeAnims.Length; ++i)
            //    {
            //        if (!badgeAnims[i].isPlaying)
            //            badgeAnims[i].Play();
            //    }
            //}
        }

        public bool HasChallenge()
        {
            return PuzzleManager.Instance.challengeIds.Count > 0;
        }

        private void OnPlayerDbResetted()
        {
            SetBadgeActive(true);
        }
    }
}