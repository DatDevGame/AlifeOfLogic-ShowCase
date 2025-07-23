using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using System.Globalization;
using System;
using GameSparks.Core;
using UnityEngine.SceneManagement;
using EasyMobile;

namespace Takuzu
{
    public class PlayUI : MonoBehaviour
    {
        public string guideUIUndoSaveKey;
        public string guideUIHintSaveKey;
        public string guideUIAssistSaveKey;
        public string UIGuideTipsButtonSaveKey;
        public string guideUIRuleDisplaySaveKey;
        public string guideFirstErrorSaveKey;
        public string guideUITileShopSaveKey;
        public const string guideUIChangeProgressSaveKey = "UI_GUI_CHANGE_PROGRESS_KEY";
        public GameObject container;
        public Button pauseButton;
        public Button tipsButton;
        public Button ruleButton;
        public Button tilesShopButton;
        [HideInInspector]
        public PauseMenu pauseMenu;
        public Button coinShopButton;
        public Button energyShopButton;
        [HideInInspector]
        public CoinShopUI coinShop;
        [HideInInspector]
        public EnergyExchangePanel energyExchangePanel;
        public Button shareButton;
        public GameObject powerupButtonGroup;
        public Text revealCostText;
        public Text revealFreeText;
        public ExtrudedButton revealButton;
        public CanvasGroup revealGroup;
        public Text undoCostText;
        public Text undoFreeText;
        public ExtrudedButton undoButton;
        public CanvasGroup undoGroup;
        public Button levelInfoButton;
        public ColorAnimation infoAnim;
        public RectTransform infoBody;
        public CanvasGroup infoGroup;
        [HideInInspector]
        public ConfirmationDialog dialog;
        public HeaderMultiplayerInfo headerMultiplayerInfo;
        public Image opponentContainerImg;
        [HideInInspector]
        public WinMenu winMenu;
        public UiGroupController controller;
        public GameObject looseCoinPrefab;
        public Text infoText1;
        public Text infoText2;
        public Text infoText3;
        public Text infoText4;
        public Image infoIcon2;
        public GameObject halfCircleTimer;
        public Sprite infoIconChallenge;
        public Sprite infoIconNormal;
        public float showInfoDuration;
        public CanvasGroup buttonsGroup;
        public CanvasGroup powerupGroup;
        public float footerAnimOverriddenDurationOnShowInfo = 0.5f;
        public PositionAnimation[] footerAnims;

        public Sprite cursorOn;
        public Sprite cursorOff;
        public Image assistIcon;

        [Header("Character facial anim")]
        public CharacterFacialAnim defaultFacialAnim;
        public CharacterFacialAnim[] facialAnims;

        public Button autoSolveButton;
        public Button assistToggleButton;

        private Coroutine showInfoCoroutine;
        public CoinDisplayer CoinDisplayer;

        private void Awake()
        {
            if (UIReferences.Instance != null)
            {
                UpdateReferences();
            }
            UIReferences.UiReferencesUpdated += UpdateReferences;
        }

        private void OnEnable()
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        private void UpdateReferences()
        {
            pauseMenu = UIReferences.Instance.overlayPauseMenu;
            winMenu = UIReferences.Instance.overlayWinMenu;
            coinShop = UIReferences.Instance.overlayCoinShopUI;
            energyExchangePanel = UIReferences.Instance.overlayEnergyExchangePanel;
            dialog = UIReferences.Instance.overlayConfirmDialog;
            CloudServiceManager.onConfigLoaded += OnAppConfigLoadded;
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            UIReferences.UiReferencesUpdated -= UpdateReferences;
            CloudServiceManager.onConfigLoaded -= OnAppConfigLoadded;
            InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted -= OnRestoreCompleted;
        }

        private void OnAppConfigLoadded(GSData appConfig)
        {
            if (SceneManager.GetActiveScene().name.Equals("Main"))
            {
                int? hide = appConfig.GetInt("hideTimerInStory");
                if (hide.HasValue)
                {
                    halfCircleTimer.SetActive(hide.Value == 1 ? false : true);
                }
            }
        }
        private SkinShopOverlayUI skinShopOverlayUI;
        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
            InAppPurchasing.RestoreCompleted += OnRestoreCompleted;
            if (SceneManager.GetActiveScene().name.Equals("Main"))
                halfCircleTimer.SetActive(false);
            (container.transform as RectTransform).anchoredPosition = Vector3.zero;
            //controller.group.alpha = 0;

            tipsButton.onClick.AddListener(delegate
            {
                UIReferences.Instance.overlayTipsPanel.Show();
            });
            for (int i = 0; i < UIReferences.Instance.overlayUIController.panels.Length; i++)
            {
                if (UIReferences.Instance.overlayUIController.panels[i].GetComponent<SkinShopOverlayUI>() != null)
                {
                    skinShopOverlayUI = UIReferences.Instance.overlayUIController.panels[i].GetComponent<SkinShopOverlayUI>();
                }
            }
            tilesShopButton.onClick.AddListener(() =>
            {
                skinShopOverlayUI.Show();
            });

            pauseButton.onClick.AddListener(delegate
                {
                    Pause();
                });

            coinShopButton.onClick.AddListener(delegate
                {
                    coinShop.Show();
                });
            energyShopButton.onClick.AddListener(delegate
                {
                    energyExchangePanel.Show();
                });
            revealButton.onClick.AddListener(delegate
                {
                    //bool enoughCoin = CoinManager.Instance.Coins >= ItemPriceProfile.active.revealPowerup * (Judger.Instance.revealCount + 1);
                    bool enoughCoin = (CoinManager.Instance.Coins >= ItemPriceProfile.active.revealPowerup * (Powerup.Instance.CountRevealPerGame + 1)) || (StoryPuzzlesSaver.Instance.MaxNode < 0);
                    if (enoughCoin)
                    {
                        CoinManager.InsufficentCoinsReason = "";
                        CoinManager.InSufficentCoins = false;
                        int amountSpend = ItemPriceProfile.active.revealPowerup * (Powerup.Instance.CountRevealPerGame + 1);
                        Powerup.Instance.CountRevealPerGame++;
                        Powerup.Instance.SetType("reveal");
                        if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
                            PlayLooseCoinAnim(ItemPriceProfile.active.revealPowerup * Powerup.Instance.CountRevealPerGame, revealButton.transform as RectTransform);

                        AlolAnalytics.IncreaseReveal();
                        AlolAnalytics.PowerUpUsed($"reveal", 1, StoryPuzzlesSaver.Instance.MaxNode < 0 ? 0 : amountSpend);
                    }
                    else
                    {
                        //CoinManager.InsufficentCoinsReason = "Reveal";
                        //CoinManager.InSufficentCoins = true;
                        //dialog.Show(
                        //    I2.Loc.ScriptLocalization.ATTENTION,
                        //    string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_COIN_FOR_HINT,
                        //        ItemPriceProfile.active.revealPowerup * (Powerup.Instance.CountRevealPerGame + 1)),
                        //    delegate
                        //    {
                        //        coinShop.Show();
                        //    },
                        //    null);
                    }
                });

            undoButton.onClick.AddListener(delegate
                {
                    bool enoughCoin = (CoinManager.Instance.Coins >= ItemPriceProfile.active.undoPowerup) || (StoryPuzzlesSaver.Instance.MaxNode < 0);
                    if (enoughCoin)
                    {
                        CoinManager.InsufficentCoinsReason = "";
                        CoinManager.InSufficentCoins = false;
                        Powerup.Instance.CountUndoPerGame++;
                        Powerup.Instance.SetType("undo");
                        if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
                            PlayLooseCoinAnim(ItemPriceProfile.active.undoPowerup /** Powerup.Instance.CountUndoPerGame*/, undoButton.transform as RectTransform);

                        AlolAnalytics.IncreaseUndo();
                        AlolAnalytics.PowerUpUsed($"undo", 1, StoryPuzzlesSaver.Instance.MaxNode < 0 ? 0 : ItemPriceProfile.active.undoPowerup);
                    }
                    else
                    {
                        //CoinManager.InsufficentCoinsReason = "Undo";
                        //CoinManager.InSufficentCoins = true;
                        //dialog.Show(
                        //    I2.Loc.ScriptLocalization.ATTENTION,
                        //    string.Format(I2.Loc.ScriptLocalization.NOT_ENOUGH_COIN_FOR_UNDO,
                        //        ItemPriceProfile.active.undoPowerup /** (Powerup.Instance.CountUndoPerGame + 1)*/),
                        //    delegate
                        //    {
                        //        coinShop.Show();
                        //    },
                        //    null);
                    }
                });

            autoSolveButton.onClick.AddListener(delegate
                {
                    LogicalBoard.Instance.StartCoroutine(LogicalBoard.Instance.AutoSolve());
                });

            // levelInfoButton.onClick.AddListener(delegate
            //     {
            //         showInfoCoroutine = StartCoroutine(CrShowLevelInfo());
            //     });

            shareButton.onClick.AddListener(delegate
                {
                    ShareProgress();
                });
            assistToggleButton.onClick.AddListener(delegate
            {
                InputHandler.Instance.ResetAssistCursorPosition();
                InputHandler.Instance.activeAssistiveInput = !InputHandler.Instance.activeAssistiveInput;
                if (InputHandler.Instance.activeAssistiveInput)
                {
                    InputHandler.Instance.EnableCursor();
                }
                else
                {
                    InputHandler.Instance.DisbaleCursor();
                }
                assistIcon.sprite = InputHandler.Instance.activeAssistiveInput ? cursorOff : cursorOn;

            });
            for (int i = 0; i < facialAnims.Length; ++i)
            {
                facialAnims[i].dontLookYet = true;
                facialAnims[i].gameObject.SetActive(false);
            }
        }

        void OnPurchaseCompleted(IAPProduct product)
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        void OnRestoreCompleted()
        {
            if (EnergyManager.Instance != null && energyShopButton != null)
                energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
        }

        private void Update()
        {
            bool canReveal = !LogicalBoard.Instance.isPlayingRevealAnim;
            if (MultiplayerSession.Instance != null)
            {
                if (!MultiplayerSession.Instance.IsCurrentPlayerViewProgress)
                    canReveal = false;
            }
            revealGroup.blocksRaycasts = canReveal;
            revealButton.interactable = canReveal;
            //revealCountGroup.SetActive(Powerup.Instance.CountRevealPerGame > 0);
            //revealCountText.text = Powerup.Instance.CountRevealPerGame.ToString();
            if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
            {
                revealCostText.gameObject.SetActive(true);
                revealFreeText.gameObject.SetActive(false);
                revealCostText.text = (ItemPriceProfile.active.revealPowerup * (Powerup.Instance.CountRevealPerGame + 1)).ToString();
            }
            else
            {
                revealCostText.gameObject.SetActive(false);
                revealFreeText.gameObject.SetActive(true);
            }

            bool canUndo = LogicalBoard.Instance.CanUndo();
            if (MultiplayerSession.Instance != null)
            {
                if (!MultiplayerSession.Instance.IsCurrentPlayerViewProgress)
                    canUndo = false;
            }
            undoGroup.blocksRaycasts = canUndo;
            undoButton.interactable = canUndo;
            //undoCountGroup.SetActive(Powerup.Instance.CountUndoPerGame > 0);
            //undoCountText.text = Powerup.Instance.CountUndoPerGame.ToString();

            if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
            {
                undoCostText.gameObject.SetActive(true);
                undoFreeText.gameObject.SetActive(false);
                undoCostText.text = ItemPriceProfile.active.undoPowerup.ToString();
            }
            else
            {
                undoCostText.gameObject.SetActive(false);
                undoFreeText.gameObject.SetActive(true);
            }

            //TODO: Cheat
            autoSolveButton.gameObject.SetActive(GameDataSO.Instance.isDevMode);
#if UNITY_EDITOR && UNITY_ANDROID
            if (GameDataSO.Instance.isDevMode)
            {
                if (Input.GetKeyDown(KeyCode.W))
                    LogicalBoard.Instance.StartCoroutine(LogicalBoard.Instance.AutoSolve());
            }
#endif
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                if (showInfoCoroutine != null)
                {
                    StopCoroutine(showInfoCoroutine);
                    showInfoCoroutine = null;
                    ResetShowInfo();
                }

                controller.HideIfNot();
                revealButton.gameObject.SetActive(false);
                undoButton.gameObject.SetActive(false);
                assistToggleButton.gameObject.SetActive(false);

                for (int i = 0; i < facialAnims.Length; ++i)
                {
                    facialAnims[i].dontLookYet = true;
                }

                if (EnergyManager.Instance != null && energyShopButton != null)
                    energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
            }
            if (newState == GameState.Prepare || newState == GameState.GameOver)
            {
                InputHandler.Instance.DisbaleCursor();
            }

            if (newState == GameState.Playing)
            {
                if (EnergyManager.Instance != null && energyShopButton != null)
                    energyShopButton.interactable = EnergyManager.Instance.AlwaysMaxEnergy() == false;
            }
        }

        private void OnPuzzleSolved()
        {
            if (showInfoCoroutine != null)
            {
                StopCoroutine(showInfoCoroutine);
                showInfoCoroutine = null;
            }
            controller.HideIfNot();
            controller.group.blocksRaycasts = false;
            //infoGroup.transform.localScale = Vector3.zero;
            // infoAnim.Play(AnimConstant.IN);
            // CoroutineHelper.Instance.DoActionDelay(
            //     () =>
            //     {
            //         infoAnim.Play(AnimConstant.OUT);
            //     }, winMenu.showDelayOnPuzzleSolved + ((!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode) ? VisualBoard.Instance.currentTotalTimeFlip : 0));
        }

        private void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
        {
            bool isChallenge = PuzzleManager.Instance.IsChallenge(id);
            /* 
            if (isChallenge)
            {
                powerupButtonGroup.SetActive(false);
            }
            else
            {
                powerupButtonGroup.SetActive(true);
            }
            */
            powerupButtonGroup.SetActive(true);

            PrepareFacialAnim(id);

            ResetShowInfo();
            SetLevelInfo(id);

            controller.group.blocksRaycasts = true;
            controller.ShowIfNot();
            revealButton.gameObject.SetActive(true);
            undoButton.gameObject.SetActive(true);
            assistToggleButton.gameObject.SetActive(true);
            buttonsGroup.interactable = true;
            powerupGroup.interactable = true;
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                CoroutineHelper.Instance.PostponeActionUntil(() =>
                {
                    GameObject playerHead = defaultFacialAnim.gameObject;
                    foreach (var item in facialAnims)
                    {
                        if (item.gameObject.activeSelf)
                        {
                            playerHead = item.gameObject;
                            break;
                        }
                    }
                    List<Image> maskedObject = playerHead.GetComponentsInChildren<Image>().ToList();
                    Vector3[] worldConners = new Vector3[4];
                    (playerHead.transform as RectTransform).GetWorldCorners(worldConners);
                    float offsetFactor = Mathf.Abs(worldConners[1].y - worldConners[0].y);

                    List<Image> undoButtonMaskedImage = new List<Image>();
                    undoButtonMaskedImage.AddRange(maskedObject);
                    undoButtonMaskedImage.AddRange(undoButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    undoButtonMaskedImage.Add(undoButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation undoUIGuideInformation = new UIGuide.UIGuideInformation(guideUIUndoSaveKey, undoButtonMaskedImage, playerHead, undoButton.gameObject, GameState.Playing);
                    undoUIGuideInformation.message = string.Format(I2.Loc.ScriptLocalization.UIGUIDE_UNDO, CoinManager.Instance.itemPrice.undoPowerup);
                    undoUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.6f, 0);

                    UIGuide.UIGuideInformation undoUIFreeGuideInformation = new UIGuide.UIGuideInformation(guideUIUndoSaveKey + "-Free", undoButtonMaskedImage, playerHead, undoButton.gameObject, GameState.Playing);
                    undoUIFreeGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_UNDO_FREE;
                    undoUIFreeGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.6f, 0);


                    List<Image> revealButtonMaskedImage = new List<Image>();
                    revealButtonMaskedImage.AddRange(maskedObject);
                    revealButtonMaskedImage.AddRange(revealButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    revealButtonMaskedImage.Add(revealButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation revealUIGuideInformation = new UIGuide.UIGuideInformation(guideUIHintSaveKey, revealButtonMaskedImage, playerHead, revealButton.gameObject, GameState.Playing);
                    revealUIGuideInformation.message = string.Format(I2.Loc.ScriptLocalization.UIGUIDE_HINT, CoinManager.Instance.itemPrice.revealPowerup);
                    revealUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.6f, 0);

                    UIGuide.UIGuideInformation revealUIFreeGuideInformation = new UIGuide.UIGuideInformation(guideUIHintSaveKey + "-Free", revealButtonMaskedImage, playerHead, revealButton.gameObject, GameState.Playing);
                    revealUIFreeGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_HINT_FREE;
                    revealUIFreeGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.6f, 0);


                    List<Image> assitButtonMaskedImage = new List<Image>();
                    assitButtonMaskedImage.AddRange(maskedObject);
                    assitButtonMaskedImage.AddRange(assistToggleButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    assitButtonMaskedImage.Add(assistToggleButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation assitUIGuideInformation = new UIGuide.UIGuideInformation(guideUIAssistSaveKey, assitButtonMaskedImage, playerHead, assistToggleButton.gameObject, GameState.Playing);
                    assitUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_ASSIT;
                    assitUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * (!PuzzleManager.currentIsMultiMode ? 0.8f : 1.05f), 0);


                    List<Image> tipsButtonMaskedImage = new List<Image>();
                    tipsButtonMaskedImage.AddRange(maskedObject);
                    tipsButtonMaskedImage.AddRange(tipsButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    tipsButtonMaskedImage.Add(tipsButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation tipsUIGuideInformation = new UIGuide.UIGuideInformation(UIGuideTipsButtonSaveKey, tipsButtonMaskedImage, playerHead, tipsButton.gameObject, GameState.Playing);
                    tipsUIGuideInformation.clickableButton = tipsButton;
                    tipsUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_INGAME_TIPS;
                    tipsUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * (!PuzzleManager.currentIsMultiMode ? 0.8f : 1.05f), 0);

                    List<Image> ruleButtonMaskedImage = new List<Image>();
                    ruleButtonMaskedImage.AddRange(maskedObject);
                    ruleButtonMaskedImage.AddRange(ruleButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    ruleButtonMaskedImage.Add(ruleButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation ruleUIGuideInformation = new UIGuide.UIGuideInformation(guideUIRuleDisplaySaveKey, ruleButtonMaskedImage, playerHead, ruleButton.gameObject, GameState.Playing);
                    ruleUIGuideInformation.clickableButton = ruleButton;
                    ruleUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_INGAME_RULE;
                    ruleUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * (!PuzzleManager.currentIsMultiMode ? 0.8f : 1.05f), 0);

                    List<Image> shopButtonMaskedImage = new List<Image>();
                    shopButtonMaskedImage.AddRange(maskedObject);
                    shopButtonMaskedImage.AddRange(tilesShopButton.gameObject.GetComponentsInChildren<Image>().ToList());
                    shopButtonMaskedImage.Add(tilesShopButton.gameObject.GetComponent<Image>());
                    UIGuide.UIGuideInformation shopUIGuideInformation = new UIGuide.UIGuideInformation(guideUITileShopSaveKey, shopButtonMaskedImage, playerHead, tilesShopButton.gameObject, GameState.Playing);
                    shopUIGuideInformation.clickableButton = tilesShopButton;
                    shopUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_INGAME_TILESHOP;
                    shopUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * (!PuzzleManager.currentIsMultiMode ? 0.8f : 1.05f), 0);

                    if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
                        UIGuide.instance.HighLightThis(undoUIGuideInformation);
                    else
                        UIGuide.instance.HighLightThis(undoUIFreeGuideInformation);


                    if (StoryPuzzlesSaver.Instance.MaxNode >= 0)
                        UIGuide.instance.HighLightThis(revealUIGuideInformation);
                    else
                        UIGuide.instance.HighLightThis(revealUIFreeGuideInformation);


                    if (PuzzleManager.currentSize == Generator.Size.Ten || PuzzleManager.currentSize == Generator.Size.Twelve)
                        UIGuide.instance.HighLightThis(assitUIGuideInformation);

                    UIGuide.instance.HighLightThis(ruleUIGuideInformation);

                    UIGuide.instance.HighLightThis(tipsUIGuideInformation);

                    UIGuide.instance.HighLightThis(shopUIGuideInformation);

                    if (headerMultiplayerInfo != null)
                    {
                        List<Image> progressMaskedImage = new List<Image>();
                        progressMaskedImage.AddRange(maskedObject);
                        progressMaskedImage.Add(opponentContainerImg);
                        UIGuide.UIGuideInformation changeProgressUIGuideInformation = new UIGuide.UIGuideInformation(guideUIChangeProgressSaveKey, progressMaskedImage, playerHead, headerMultiplayerInfo.changeProgressBtn.gameObject, GameState.Playing);
                        changeProgressUIGuideInformation.clickableButton = headerMultiplayerInfo.changeProgressBtn;
                        changeProgressUIGuideInformation.message = I2.Loc.ScriptLocalization.UIGUI_VIEW_PLAYER_PROGRESS;
                        changeProgressUIGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.6f, 0);
                        UIGuide.instance.HighLightThis(changeProgressUIGuideInformation);
                    }

                    float scaleFactor = 1;
                    Vector3[] wCs = new Vector3[4];
                    (infoGroup.transform as RectTransform).GetWorldCorners(wCs);
                    float infoW = Mathf.Abs((wCs[2] - wCs[0]).x);
                    if (VisualBoard.Instance.background != null)
                        scaleFactor = VisualBoard.Instance.background.bounds.size.x / infoW;
                    //(infoGroup.transform as RectTransform).sizeDelta = new Vector2((infoGroup.transform as RectTransform).sizeDelta.x * scaleFactor, (infoGroup.transform as RectTransform).sizeDelta.y);
                }, () => !TipsManager.Instance.tipPanel.IsShowing);
            }, 1);

        }

        private void PrepareFacialAnim(string puzzleId)
        {
            try
            {
                Generator.Level l = PuzzleManager.currentLevel;
                int index =
                    l == Generator.Level.Easy ? 0 :
                    l == Generator.Level.Medium ? 1 :
                    l == Generator.Level.Hard ? 2 :
                    l == Generator.Level.Evil ? 3 :
                    l == Generator.Level.Insane ? 4 : 0;
                for (int i = 0; i < facialAnims.Length; ++i)
                {
                    facialAnims[i].gameObject.SetActive(i == index);
                }

                facialAnims[index].dontLookYet = true;
                CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        facialAnims[index].dontLookYet = false;
                    }, controller.MaxDuration);
            }
            catch
            {
                for (int i = 0; i < facialAnims.Length; ++i)
                {
                    facialAnims[i].gameObject.SetActive(false);
                }
                defaultFacialAnim.gameObject.SetActive(true);
                defaultFacialAnim.dontLookYet = true;
                CoroutineHelper.Instance.DoActionDelay(
                    () =>
                    {
                        defaultFacialAnim.dontLookYet = false;
                    }, controller.MaxDuration);
            }
        }

        private void SetLevelInfo(string id)
        {
            string info1;
            try
            {
                if (!PuzzleManager.currentIsMultiMode)
                {
                    info1 = Takuzu.Utilities.GetLocalizePackNameByLevel(PuzzleManager.currentLevel);
                    info1 = info1.Substring(0, 1).ToUpper() + info1.Substring(1, info1.Length - 1).ToLower();
                }
                else
                {
                    string multiplayer = I2.Loc.ScriptLocalization.MULTIPLAYER;
                    info1 = multiplayer.Substring(0, 1).ToUpper() + multiplayer.Substring(1, multiplayer.Length - 1).ToLower();
                }
            }
            catch (System.Exception e)
            {
                info1 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }
            string info2;
            try
            {
                int nodeIndex = StoryPuzzlesSaver.GetIndexNode(PuzzleManager.currentLevel, PuzzleManager.currentSize);
                int currentAge = PuzzleManager.Instance.ageList[nodeIndex >= 0 ? nodeIndex : 0];
                string currentMileStone = String.Format("{0}.{1}", nodeIndex + 1,
                    StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) < StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex)
                    ? StoryPuzzlesSaver.Instance.GetMaxProgressInNode(nodeIndex) + 1 : StoryPuzzlesSaver.Instance.ProgressRequiredToFinishNode(nodeIndex));
                if (nodeIndex <= StoryPuzzlesSaver.Instance.MaxNode)
                {
                    currentMileStone = String.Format("{0}.{1}", nodeIndex + 1, StoryPuzzlesSaver.Instance.GetCurrentPuzzleIndesOffset(nodeIndex) + 1);
                }
                info2 = PuzzleManager.currentIsChallenge ?
                    PuzzleManager.Instance.GetChallengeCreationDate(PuzzleManager.currentPuzzleId).ToShortDateString() : currentMileStone;
                infoIcon2.sprite =
                    PuzzleManager.currentIsChallenge ?
                    infoIconChallenge : infoIconNormal;
            }
            catch (System.Exception e)
            {
                info2 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }

            string info3;
            try
            {
                info3 = Utilities.GetDifficultyDisplayName(PuzzleManager.currentLevel);
            }
            catch (System.Exception e)
            {
                info3 = string.Empty;
                Debug.LogWarning("Reported in PlayUI.SetLevelInfo(): " + e.ToString());
            }

            infoText1.text = info1;
            infoText2.text = info2;
            infoText3.text = info3;

            if (MultiplayerManager.Instance == null)
            {
                infoText2.gameObject.SetActive(true);
                if (infoText4 != null)
                    infoText4.gameObject.SetActive(false);
            }
            else
            {
                infoText2.gameObject.SetActive(false);
                infoText4.gameObject.SetActive(true);
                infoText4.text = MultiplayerRoom.Instance.currentBetCoin.ToString();
            }

            CoroutineHelper.Instance.DoActionDelay(
                () =>
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(infoGroup.transform as RectTransform);
                }, 0);
        }

        private IEnumerator CrShowLevelInfo(float duration = -1)
        {
            if (duration <= 0)
                duration = showInfoDuration;
            buttonsGroup.interactable = false;
            powerupGroup.interactable = false;
            for (int i = 0; i < footerAnims.Length; ++i)
            {
                if (footerAnims[i].gameObject.activeInHierarchy)
                    footerAnims[i].Play(AnimConstant.OUT, footerAnimOverriddenDurationOnShowInfo);
            }

            levelInfoButton.interactable = false;
            infoAnim.Play(0);
            yield return new WaitForSeconds(duration + infoAnim.duration);
            if (infoAnim.gameObject.activeInHierarchy)
            {
                infoAnim.Play(1);
            }
            else
            {
                HideInfoImmediately();
            }
            yield return new WaitForSeconds(infoAnim.duration);
            levelInfoButton.interactable = true;

            buttonsGroup.interactable = true;
            powerupGroup.interactable = true;
            for (int i = 0; i < footerAnims.Length; ++i)
            {
                if (footerAnims[i].gameObject.activeInHierarchy)
                    footerAnims[i].Play(AnimConstant.IN, footerAnimOverriddenDurationOnShowInfo);
            }
        }

        private void ResetShowInfo()
        {
            if (showInfoCoroutine != null)
            {
                StopCoroutine(showInfoCoroutine);
                showInfoCoroutine = null;
            }
            infoGroup.transform.localScale = Vector3.one;
            infoGroup.alpha = 0;
            levelInfoButton.interactable = true;
        }

        public void Pause()
        {
            GameManager.Instance.PauseGame();
            pauseMenu.Show();
        }

        public void ShowInfoImmediately()
        {
            //infoBody.localScale = new Vector3(1, 1, 1);
            infoGroup.alpha = 1;
            levelInfoButton.interactable = false;
        }

        public void HideInfoImmediately()
        {
            //infoBody.localScale = new Vector3(0, 1, 1);
            infoGroup.alpha = 0;
            levelInfoButton.interactable = true;
        }

        public void ShareProgress()
        {
            StartCoroutine(CrShareProgress());
        }

        private IEnumerator CrShareProgress()
        {
            //showInfoCoroutine = StartCoroutine(CrShowLevelInfo(1));
            yield return new WaitForSeconds(infoAnim.duration + 0.5f);
            SocialManager.Instance.NativeShareScreenshot();
        }

        private void PlayLooseCoinAnim(int amount, RectTransform parent)
        {
            GameObject g = Instantiate(looseCoinPrefab);
            g.transform.SetParent(parent, false);
            (g.transform as RectTransform).anchoredPosition = Vector2.zero;
            g.GetComponentInChildren<Text>().text = string.Format("-{0}", amount);

            CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    if (g != null)
                        Destroy(g);
                }, 1);
        }

        public void ShowGuideFirstError()
        {
            Debug.Log("ShowGuideFirstError");
            GameObject playerHead = defaultFacialAnim.gameObject;
            foreach (var item in facialAnims)
            {
                if (item.gameObject.activeSelf)
                {
                    playerHead = item.gameObject;
                    break;
                }
            }
            List<Image> maskedObject = playerHead.GetComponentsInChildren<Image>().ToList();
            Vector3[] worldConners = new Vector3[4];
            (playerHead.transform as RectTransform).GetWorldCorners(worldConners);
            float offsetFactor = Mathf.Abs(worldConners[1].y - worldConners[0].y);

            List<Image> ruleButtonMaskedImage = new List<Image>();
            ruleButtonMaskedImage.AddRange(maskedObject);
            ruleButtonMaskedImage.AddRange(ruleButton.gameObject.GetComponentsInChildren<Image>().ToList());
            ruleButtonMaskedImage.Add(ruleButton.gameObject.GetComponent<Image>());
            UIGuide.UIGuideInformation ErrorGuideInformation = new UIGuide.UIGuideInformation(guideFirstErrorSaveKey, ruleButtonMaskedImage, playerHead, ruleButton.gameObject, GameState.Playing);
            ErrorGuideInformation.clickableButton = ruleButton;
            ErrorGuideInformation.message = I2.Loc.ScriptLocalization.UIGUIDE_INGAME_FIRST_ERROR;
            ErrorGuideInformation.transformOffset = new Vector3(0, -offsetFactor * 0.8f, 0);
            UIGuide.instance.HighLightThis(ErrorGuideInformation);
        }

        public void DisplayHeaderGroupButton(bool isShow)
        {
            tipsButton.gameObject.SetActive(isShow);
            ruleButton.gameObject.SetActive(isShow);
            tilesShopButton.gameObject.SetActive(isShow);
        }
    }
}