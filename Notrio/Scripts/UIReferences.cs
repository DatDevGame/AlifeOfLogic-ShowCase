using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Achievements;
using Pinwheel;

namespace Takuzu
{
    public class UIReferences : MonoBehaviour
    {
        public static UIReferences Instance;
        public static System.Action UiReferencesUpdated = delegate { };

        [Header("Overlay UI references")]
        public Canvas overlayCanvas;
        public Image darkenImage;
        public OverlayUIController overlayUIController;
        public ConfirmationDialog overlayConfirmDialog;
        public SettingPanel overlaySettingPanel;
        public AchievementPanel overlayAchievementPanel;
        public LevelSelectorPanelController overlayLevelSelectorPanelController;
        public WinMenu overlayWinMenu;
        public TaskPanel overlayTaskPanel;
        public LeaderBoardScreenUI overlayLeaderBoardScreenUI;
        public LevelUpPanel overlayLevelUpPanel;
        public PauseMenu overlayPauseMenu;
        public CoinShopUI overlayCoinShopUI;
        public RewardDetailPanel overlayRewardDetailPanel;
        public ProfilePanel overlayProfilePanel;
        public CreditPanel overlayCreditPanel;
        public RewardListUI overlayRewardList;
        public LevelSelectionPopup overlayLevelSelectionPopup;
        public TounamentsPanel overlayTournamentsPanel;
        public TipsPanel overlayTipsPanel;
        public RollPanelUI overlayRollPanelUI;
        public EnergyExchangePanel overlayEnergyExchangePanel;
        public RulePanel rulePanel;
        public LuckySpinPromptPanel luckySpinPanel;
        public UIGuide uiGui;
        public MatchingPanelController matchingPanelController;
        public ECAsPanelController ecasPanelController;
        public TopPlayerPanel topPlayePanel;
        public SubscriptionDetailPanelController subscriptionDetailPanel;
        [Header("Game UI references")]
        public Canvas gameUICanvas;
        public PackSelectionUI gameUiPackSelectionUI;
        public FooterUI gameUiFooterUI;
        public PlayUI gameUiPlayUI;
        public PlayingModeAdUI gameUiPlayingModeAdUI;
        public AchievementUI gameUiAchievementUI;
        public DailyWeeklyChallengePanelUI gameUiDailyWeeklyChallengePanelUI;
        public SideUI gameUiSideUI;
        public InGameNotificationPopup gameUiIngameNotificationPopup;
        public Transform gameUiTournamentContainer;
        public HeaderUI gameUiHeaderUI;
        public ClockUI gameUiClockUI;
        public RulePopUpController rulePopUpController;
        public ErrorsDisplayer errorDisplay;
        public SideUI gameUiSideUIBuble;
        public HeaderMultiplayerInfo headerMultiplayeInfo;
        public TournamentSideUI tournamentSideUI;
        [Header("Globle UI Blocker")]
        public GameObject GlobleUIBlocker;
        [Header("Input Handler")]
        public InputHandler InputHandler;
        [Header("Ad preparation")]
        public GameObject adPreparation;
        public Text adPreparationCountDownText;
        public ColorAnimation adPreparationFadeAnim;
        public ColorAnimation adPreparationPopupAnim;
        [Header("Camera")]
        public CameraController mainCameraController;
        public Camera boardCamera;
        public RTCamera RTCamera;
        [Header("Logical board")]
        public LogicalBoard lb;
        [Header("Overlay Effect")]
        public OverlayEffect overlayEffect;
        [Header("Timer")]
        public Timer timer;
        [Header("Container")]
        public Transform tipsBoardContainer;
        public Transform energyIconIngame;
        public MultiplayerShareBgController multiplayerShareBGController;
        public IngameBGAdapter ingameBGAdapter;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                DestroyImmediate(Instance.gameObject);
                Instance = this;
            }
            UiReferencesUpdated();
        }
    }
}