using GameSparks.Api.Responses;
using Pinwheel;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class LevelSelector : MonoBehaviour
    {
        public static System.Action<string> onClickOnPlayablePuzzle = delegate { };
        public GameObject leftConnection;
        public GameObject rightConnection;
        public GameObject avatarHolder;
        public Button avatarButton;
        public Image playerAvatar;
        public Button button;
        public Image progressImage;
        public Image statusSubIcon;
        public Sprite solvedIcon;
        public Sprite inProgressIcon;
        public Color notPlayIconColor = Color.grey;
        public ProceduralAnimation[] solvedStatusAnim;
        public ProceduralAnimation[] inProgressStatusAnim;
        public const string STATUS_NOT_PLAY = "NOT_PLAY";
        public const string STATUS_SOLVED = "SOLVED";
        public const string STATUS_IN_PROGRESS = "IN_PROGRESS";

        public Color SolvedColor;
        public Color currentColor;
        public Color notPlayColor;

        private static int currentMaxNodeDisplayed = -2;
        private string currentStatus;
        private string id;
        [SerializeField]
        private bool needHighlightSolvedStatus;
        [SerializeField]
        private bool needHighlightInProgressStatus;
        [HideInInspector]
        public Action<LevelSelector> onClickOnUnPlayableLevel = delegate { };

        private IEnumerator changingStatusCoroutine;
        internal bool initAnimation = true;

        public void Awake()
        {
            //CloudServiceManager.onPlayerDbSyncSucceed += OnSyncSucceed;
            //PlayerDb.Resetted += OnPlayerDbResetted;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
            avatarButton.onClick.AddListener(() =>
            {
            });
            CloudServiceManager.onLoginGameSpark += OnLoginGameSpark;
            CloudServiceManager.onLoginGameSparkAsGuest += OnLoginAsGuest;
        }

        private void OnDestroy()
        {
            //CloudServiceManager.onPlayerDbSyncSucceed -= OnSyncSucceed;
            //PlayerDb.Resetted -= OnPlayerDbResetted;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
            CloudServiceManager.onLoginGameSpark -= OnLoginGameSpark;
            CloudServiceManager.onLoginGameSparkAsGuest -= OnLoginAsGuest;
        }

        private void OnLoginAsGuest(AuthenticationResponse obj)
        {
            playerAvatar.enabled = false;
            CloudServiceManager.Instance.GetPlayerAvatar(tx =>
            {
                playerAvatar.sprite = Sprite.Create(tx as Texture2D, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
                playerAvatar.enabled = true;
            });
        }

        private void OnLoginGameSpark(AuthenticationResponse obj)
        {
            playerAvatar.enabled = false;
            CloudServiceManager.Instance.GetPlayerAvatar(tx =>
            {
                playerAvatar.sprite = Sprite.Create(tx as Texture2D, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
                playerAvatar.enabled = true;
            });
        }

        private void Start()
        {
            currentStatus = STATUS_NOT_PLAY;
        }

        private void OnPuzzleSolved()
        {
            if (PuzzleManager.currentPuzzleId.Equals(id))
            {
                needHighlightSolvedStatus = true;
                needHighlightInProgressStatus = false;
            }
            StartCoroutine(WaitForGamePrepareState());
        }

        private IEnumerator WaitForGamePrepareState()
        {
            yield return null;
            UpdateNodeState(1.5f, id);
        }

        private void OnPuzzleSelected(string puzzleId, string puzzleStr, string solutionStr, string progress)
        {
            if (puzzleId.Equals(id))
            {
                if (!PuzzleManager.Instance.IsPuzzleSolved(id))
                {
                    needHighlightInProgressStatus = true;
                }
            }
        }

        public void OnSyncSucceed()
        {
            if (PuzzleManager.Instance.IsPuzzleSolved(id))
            {
                SetStatus(STATUS_SOLVED);
            }
            else if (PuzzleManager.Instance.IsPuzzleInProgress(id) &&
                     !currentStatus.Equals(STATUS_SOLVED))
            {
                SetStatus(STATUS_IN_PROGRESS);
            }
            playerAvatar.enabled = false;
            CloudServiceManager.Instance.GetPlayerAvatar(tx =>
            {
                playerAvatar.sprite = Sprite.Create(tx as Texture2D, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
                playerAvatar.enabled = true;
            });
        }

        private void OnPlayerDbResetted()
        {
            SetStatus(STATUS_NOT_PLAY);
        }

        public void SetPuzzle(string newId)
        {
            if (!newId.Equals(id))
            {
                needHighlightInProgressStatus = false;
                needHighlightSolvedStatus = false;
            }
            if (newId.Equals(PuzzleManager.currentPuzzleId) && !PuzzleManager.Instance.IsPuzzleSolved(newId))
            {
                needHighlightInProgressStatus = true;
            }

            id = newId;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate
                {
                    if (StoryPuzzlesSaver.Instance == null)
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                        GameManager.Instance.PlayAPuzzle(newId);
                    }
                    else
                    {
                        if (StoryPuzzlesSaver.Instance.ValidateLevel(this.nodeIndex)==StoryPuzzlesSaver.SolvableStatus.Solved || StoryPuzzlesSaver.Instance.ValidateLevel(this.nodeIndex) == StoryPuzzlesSaver.SolvableStatus.Current || StoryPuzzlesSaver.Instance.ValidateLevel(this.nodeIndex) == StoryPuzzlesSaver.SolvableStatus.MaxNode)
                        {
                            onClickOnPlayablePuzzle(newId);
                            SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                            /*
                            PuzzleManager.Instance.SelectPuzzle(newId);
                            GameManager.Instance.StartGame();
                            */
                        }
                        else
                        {
                            onClickOnUnPlayableLevel(this);
                        }
                    }
                });           
            UpdateNodeState(1.5f,newId);
            UpdateStatus();

            playerAvatar.enabled = false;
            CloudServiceManager.Instance.GetPlayerAvatar(tx =>
            {
                playerAvatar.sprite = Sprite.Create(tx as Texture2D, new Rect(0,0,tx.width, tx.height), new Vector2(0.5f, 0.5f));
                playerAvatar.enabled = true;
            });
        }
        private bool playChangeMaxNodeAnimation = false;
        private void UpdateNodeState(float delay ,string newId)
        {
            playChangeMaxNodeAnimation = currentMaxNodeDisplayed != StoryPuzzlesSaver.Instance.MaxNode || currentMaxNodeDisplayed < -1;
            StopCoroutine("StartUpdateStatusAfterDelay");
            StartCoroutine(StartUpdateStatusAfterDelay(delay, newId));
        }

        private IEnumerator StartUpdateStatusAfterDelay(float v, string newId)
        {
            yield return new WaitUntil(()=>GameManager.Instance.GameState == GameState.Prepare);
            //yield return new WaitForSeconds(v);
            if (gameObject)
            {
                if (changingStatusCoroutine != null)
                    StopCoroutine(changingStatusCoroutine);
                if (newId != null)
                {
                    switch (StoryPuzzlesSaver.Instance.ValidateLevel(this.nodeIndex))
                    {
                        case StoryPuzzlesSaver.SolvableStatus.Current:
                            {
                                changingStatusCoroutine = ChangeToCurrentState();
                            }
                            break;
                        case StoryPuzzlesSaver.SolvableStatus.MaxNode:
                            {
                                changingStatusCoroutine = ChangeToSolvedState(true, StoryPuzzlesSaver.Instance.MaxNode == 19);
                            }
                            break;
                        case StoryPuzzlesSaver.SolvableStatus.Solved:
                            {
                                changingStatusCoroutine = ChangeToSolvedState(false);
                            }
                            break;
                        case StoryPuzzlesSaver.SolvableStatus.Default:
                            {
                                changingStatusCoroutine = ChangeToDefaultState();
                            }
                            break;
                        default:
                            {
                                changingStatusCoroutine = ChangeToDefaultState();
                            }
                            break;
                    }
                }
                else
                {
                    changingStatusCoroutine = ChangeToDefaultState();
                }
                StartCoroutine(changingStatusCoroutine);
            }
        }

        private IEnumerator ChangeToDefaultState()
        {
            yield return null;
            progressImage.transform.localScale = Vector3.zero;
            statusSubIcon.sprite = AgePathIcon.Get(nodeIndex, false);
            avatarHolder.SetActive(false);
            currentMaxNodeDisplayed = StoryPuzzlesSaver.Instance.MaxNode;
        }

        private IEnumerator ChangeToSolvedState(bool isMaxNode, bool lastNode = false)
        {

            progressImage.transform.localScale = Vector3.one;
            statusSubIcon.sprite = AgePathIcon.Get(nodeIndex, false);
            avatarHolder.SetActive(true);

            if (isMaxNode == true && playChangeMaxNodeAnimation && lastNode == false)
            {
                yield return new WaitUntil(() => { return GameManager.Instance.GameState == GameState.Prepare; });
                yield return new WaitForSeconds(2);
                avatarHolder.SetActive(isMaxNode);
                float duration = 1f;
                float t = duration;
                while (t > 0)
                {
                    t -= Time.deltaTime;
                    (avatarHolder.transform as RectTransform).anchoredPosition = new Vector2(Mathf.Lerp((avatarHolder.transform as RectTransform).anchoredPosition.x, (avatarHolder.transform.parent.transform as RectTransform).rect.width, 1 - t / duration), (avatarHolder.transform as RectTransform).anchoredPosition.y);
                    progressImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, 1 - t / duration);
                    yield return null;
                }
                (avatarHolder.transform as RectTransform).anchoredPosition = new Vector2((avatarHolder.transform.parent.transform as RectTransform).rect.width, (avatarHolder.transform as RectTransform).anchoredPosition.y);
            }
            yield return null;
            progressImage.transform.localScale = Vector3.zero;
            statusSubIcon.sprite = AgePathIcon.Get(nodeIndex, true);
            
            if(lastNode == false)
                avatarHolder.SetActive(false);
            
            currentMaxNodeDisplayed = StoryPuzzlesSaver.Instance.MaxNode;
        }

        private int nodeIndex;
        public void SetNodeIndex(int nodeIndex)
        {
            this.nodeIndex = nodeIndex;
        }

        private IEnumerator ChangeToCurrentState()
        {
            progressImage.transform.localScale = Vector3.zero;
            statusSubIcon.sprite = AgePathIcon.Get(nodeIndex, false);
            avatarHolder.SetActive(true);
            (avatarHolder.transform as RectTransform).anchoredPosition = new Vector2(-(avatarHolder.transform.parent.transform as RectTransform).rect.width, (avatarHolder.transform as RectTransform).anchoredPosition.y);

            if (playChangeMaxNodeAnimation)
            {
                yield return new WaitUntil(() => { return GameManager.Instance.GameState == GameState.Prepare; });
                yield return new WaitForSeconds(2);

                float duration = 1f;
                float t = duration;
                while (t > 0)
                {
                    t -= Time.deltaTime;
                    (avatarHolder.transform as RectTransform).anchoredPosition = new Vector2(Mathf.Lerp((avatarHolder.transform as RectTransform).anchoredPosition.x, 0, 1 - t / duration), (avatarHolder.transform as RectTransform).anchoredPosition.y);
                    progressImage.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, 1 - t / duration);

                    yield return null;
                }
            }
            yield return null;
            (avatarHolder.transform as RectTransform).anchoredPosition = new Vector2(0, (avatarHolder.transform as RectTransform).anchoredPosition.y);
            progressImage.transform.localScale = Vector3.one;
            statusSubIcon.sprite = AgePathIcon.Get(nodeIndex, false);
            avatarHolder.SetActive(true);
            currentMaxNodeDisplayed = StoryPuzzlesSaver.Instance.MaxNode;
        }

        private Color GetButtonStatusColor(string newId)
        {
            if (newId != null)
            {
                switch (StoryPuzzlesSaver.Instance.ValidateLevel(newId))
                {
                    case StoryPuzzlesSaver.SolvableStatus.Current:
                        return currentColor;
                    case StoryPuzzlesSaver.SolvableStatus.Solved:
                        return SolvedColor;
                    case StoryPuzzlesSaver.SolvableStatus.Default:
                        return notPlayColor;
                    default:
                        return notPlayColor;
                }
            }
            else
                return notPlayColor;
            
        }

        public void SetProgress(float progress, string CurrentAge = "", bool displayIndicator = false, int segments = 1)
        {
            if(progress >= 1 && nodeIndex == StoryPuzzlesSaver.Instance.MaxNode + 1)
            {
                int newMaxNode = StoryPuzzlesSaver.Instance.MaxNode + 1;
                if (newMaxNode <= StoryPuzzlesSaver.maxNodeCount)
                    StoryPuzzlesSaver.Instance.MaxNode++;
            }

            progressImage.fillAmount = progress;
        }

        public void SetStatus(string s)
        {
            if (statusSubIcon == null)
                return;
            currentStatus = s;
        }

        public void UpdateStatus()
        {
            if (PuzzleManager.Instance.IsPuzzleSolved(id))
            {
                SetStatus(STATUS_SOLVED);
            }
            else if (PuzzleManager.Instance.IsPuzzleInProgress(id))
            {
                SetStatus(STATUS_IN_PROGRESS);
            }
            else
            {
                SetStatus(STATUS_NOT_PLAY);
            }
        }

        

        public void Reset()
        {
            id = null;
            SetStatus(STATUS_NOT_PLAY);
        }

        public void HighlightStatusIfNeeded()
        {
            if (needHighlightSolvedStatus)
            {
                HighlightSolvedStatus();
                needHighlightSolvedStatus = false;
            }
            else if (needHighlightInProgressStatus)
            {
                HighlightInProgressStatus();
                needHighlightInProgressStatus = false;
            }
        }

        private void HighlightSolvedStatus()
        {
            StartCoroutine(CrHighlightSolvedStatus());
        }

        private IEnumerator CrHighlightSolvedStatus()
        {
            yield return new WaitForSeconds(0.5f);
            SetStatus(STATUS_SOLVED);
            for (int i = 0; i < solvedStatusAnim.Length; ++i)
            {
                solvedStatusAnim[i].Play(AnimConstant.IN);
            }
            yield return new WaitForSeconds(0.3f);
            SoundManager.Instance.PlaySound(SoundManager.Instance.ping, true);
        }

        private void HighlightInProgressStatus()
        {
            StartCoroutine(CrHighlightInProgressStatus());
        }

        private IEnumerator CrHighlightInProgressStatus()
        {
            yield return new WaitForSeconds(0.5f);
            SetStatus(STATUS_IN_PROGRESS);
            for (int i = 0; i < inProgressStatusAnim.Length; ++i)
            {
                inProgressStatusAnim[i].Play(AnimConstant.IN);
            }
            yield return new WaitForSeconds(0.3f);
            SoundManager.Instance.PlaySound(SoundManager.Instance.ping, true);
        }
    }
}