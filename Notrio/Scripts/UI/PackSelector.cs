using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;

namespace Takuzu
{
    public class PackSelector : MonoBehaviour
    {
        public Button button;
        //public Text actionText;
        public Image character;
        public string characterSpriteName;
        public SnappingScroller scroller;
        public Gradient characterBlend;
        public CanvasGroup group;
        public CanvasGroup buttonGroup;

        private PuzzlePack pack;
        private int index;

        [Header("On unlocked")]
        public GameObject looseCoinPrefab;
        public float autoSelectPackAfterUnlockDelay = 1.5f;
        public ProceduralAnimation[] unlockAnims;
        public bool ignoreAllAnims;
        [SerializeField]
        private bool isUnlocked;
        [HideInInspector]
        public LevelSelectorPanelController levelPanel;

        private List<System.Type> panelTypeToHide;

        private void Start()
        {
            //LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            CloudServiceManager.onPlayerDbSyncSucceed += OnSyncSucceed;
            PlayerDb.Resetted += OnPlayerDbReset;
            OverlayPanel.onPanelStateChanged += OnPanelStateChanged;
        }

        private void OnDestroy()
        {
            //LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            CloudServiceManager.onPlayerDbSyncSucceed -= OnSyncSucceed;
            PlayerDb.Resetted -= OnPlayerDbReset;
            OverlayPanel.onPanelStateChanged -= OnPanelStateChanged;
        }

        public void Awake()
        {
            panelTypeToHide = new List<System.Type>()
            {
                typeof(ProfilePanel),
                typeof(CoinShopUI),
                typeof(ChallengeDetailPanel),
                typeof(LevelSelectorPanelController),
                typeof(LeaderboardController),
                typeof(SettingPanel),
                typeof(CreditPanel)
            };
            /*
            button.onClick.AddListener(delegate
                {
                    bool unlocked = PuzzleManager.Instance.IsPackUnlocked(pack);
                    if (unlocked)
                    {
                        PuzzleManager.Instance.SelectPack(pack);
                    }
                    else
                    {
                        int coin = CoinManager.Instance.Coins;
                        if (coin >= pack.price)
                        {
                            dialog.Show(
                                "CONFIRMATION",
                                string.Format("Do you want to spend {0} coin{1} to unlock pack {2}?",
                                    pack.price,
                                    pack.price > 1 ? "s" : "",
                                    pack.packName.ToUpper()),
                                delegate
                                {
                                    PuzzleManager.Instance.SetPackUnlocked(pack);
                                    CoinManager.Instance.RemoveCoins(pack.price, "unlock pack " + pack.packName);
                                    //packPriceText.text = "";
                                    //packPriceGroup.SetActive(false);
                                    PlayLooseCoinAnim(pack.price, coinIcon.rectTransform);
                                    PlayUnlockAnim();
                                    //actionText.text = "SELECT";

                                    SoundManager.Instance.PlaySound(SoundManager.Instance.unlock);
                                    CoroutineHelper.Instance.DoActionDelay(
                                        () =>
                                        {
                                            //if (PuzzleManager.currentPack == null)
                                            if (!levelPanel.IsShowing)
                                                PuzzleManager.Instance.SelectPack(pack);
                                        },
                                        autoSelectPackAfterUnlockDelay);
                                },
                                null);

                        }
                        else
                        {
                            dialog.Show(
                                "UH OH!",
                                string.Format("Not enough coins, do you want to get some?"), delegate
                                {
                                    coinShop.Show();
                                },
                                null);
                        }
                    }
                });
                */
        }

        /*
        private void Update()
        {
            if (scroller == null)
                return;
            character.color = characterBlend.Evaluate(Mathf.Abs(index - scroller.RelativeNormalizedScrollPos * scroller.ElementCount));
            character.enabled = character.color.a > 0;
            buttonGroup.alpha = character.enabled ? 1 : 0;
        }
        */
        private void OnPanelStateChanged(OverlayPanel p, bool isShow)
        {
            if (isShow)
            {
                if (panelTypeToHide.Contains(p.GetType()) && ScreenManager.Instance.AspectRatio > CanvasScalerHelper.tallLayoutThreshold)
                {
                    //StartCoroutine(HideDelay(0.25f));
                }
            }
            else
            {
                StopAllCoroutines();
                group.alpha = 1;
            }
        }

        private IEnumerator HideDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            group.alpha = 0;
        }

        public void SetIndex(int index)
        {
            this.index = index;
        }

        public void SetPack(PuzzlePack pack)
        {
            this.pack = pack;
            List<string> difficultiesName = new List<string>();
            for (int i = 0; i < pack.difficulties.Count; ++i)
            {
                difficultiesName.Add(Utilities.GetDifficultyDisplayName(pack.difficulties[i]));
            }

            bool unlocked = PuzzleManager.Instance.IsPackUnlocked(pack);
            if (unlocked)
            {
                PlayUnlockAnim();
            }
            else
            {
                //packPriceGroup.SetActive(true);
                PlayLockAnim();
            }

            UpdateProgress();
        }

        public void UpdateProgress()
        {

            int puzzleCount = pack.puzzleCount;
            string dbName = System.IO.Path.GetFileName(pack.DbPath);
            string solvedPattern = string.Format("{0}{1}", PuzzleManager.SOLVED_PREFIX, dbName);
            int solvedCount = PlayerDb.CountKeyStartWith(solvedPattern);
        }

        private void OnPuzzleSolved()
        {
            UpdateProgress();
        }

        private void OnSyncSucceed()
        {
            bool unlocked = PuzzleManager.Instance.IsPackUnlocked(pack);
            if (unlocked)
            {
                //packPriceGroup.SetActive(false);
                if (isUnlocked)
                    PlayUnlockAnim();
                //actionText.text = "SELECT";
            }
            else
            {
                //packPriceGroup.SetActive(true);
                if (!isUnlocked)
                    PlayLockAnim();
                //actionText.text = "UNLOCK";
            }
            UpdateProgress();
        }

        private void OnPlayerDbReset()
        {
            bool unlocked = PuzzleManager.Instance.IsPackUnlocked(pack);
            if (unlocked)
            {
                //packPriceGroup.SetActive(false);
                if (isUnlocked)
                    PlayUnlockAnim();
                //actionText.text = "SELECT";
            }
            else
            {
                if (!isUnlocked)
                    PlayLockAnim();
                //actionText.text = "UNLOCK";
            }
        }

        private IEnumerator CrOnPlayerDbReset()
        {
            yield return null;
            bool unlocked = PuzzleManager.Instance.IsPackUnlocked(pack);
            if (unlocked)
            {
                //packPriceGroup.SetActive(false);
                if (isUnlocked)
                    PlayUnlockAnim();
                //actionText.text = "SELECT";
            }
            else
            {
                if (!isUnlocked)
                    PlayLockAnim();
                //actionText.text = "UNLOCK";
            }
        }

        private void PlayUnlockAnim()
        {
            if (ignoreAllAnims)
                return;
            for (int i = 0; i < unlockAnims.Length; ++i)
            {
                if (unlockAnims[i].gameObject.activeInHierarchy)
                {
                    unlockAnims[i].Play(AnimConstant.OUT);
                }
            }

            isUnlocked = false;
        }

        private void PlayLockAnim()
        {
            //if (ignoreAllAnims)
            //    return;
            //for (int i = 0; i < unlockAnims.Length; ++i)
            //{
            //    if (unlockAnims[i].gameObject.activeInHierarchy)
            //    {
            //        unlockAnims[i].Play(AnimConstant.IN);
            //    }
            //}
            //isUnlocked = true;
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
    }
}