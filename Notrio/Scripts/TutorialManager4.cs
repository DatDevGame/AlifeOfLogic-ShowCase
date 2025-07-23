using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using System;
using UnityEngine.SceneManagement;
namespace Takuzu
{
    public class TutorialManager4 : MonoBehaviour
    {

        public static Action<float> onFinishTutorialFirstTime = delegate
        {

        };
        public static Action SkipTutorial = delegate { };
        public Action onSkipButtonClicked = delegate { };
        public static TutorialManager4 Instance;

        [Header("General")]
        public float puzzleSize = 4 / 4.5f;
        public TutorialCompletePanel tutorialComplatePanel;
        public ParticleSystem leavesParticle;
        public ParticleSystem centerParticle;
        public PositionAnimation cameraPositionAnimation;
        public Camera m_camera;
        public LogicalBoardTutorial lb;
        private Dictionary<string, GameObject> htlDict;
        public List<WalkthroughStep> walkthroughSteps = new List<WalkthroughStep>();
        [Header("Board Information")]
        public float cellSize = 0.7f;
        public float boardBorder = 0.05f;
        [Header("End Tutorial UI")]
        public UIInOutAnim headerInOutAnim;
        public UIInOutAnim instructionInOutAnim;
        public UIInOutAnim endInstruction;
        public Button instructionButton;
        public GameObject endAnchor;
        public string HAS_ENTER_TUTORIAL_KEY = "HAS_ENTER_TUTORIAL_KEY";
        public Button skipBtn;
        private Coroutine endLeavesParticleCR;
        private Coroutine confettiCR;

        [HideInInspector]
        public bool tutorialFinished = false;

        public ConfirmationDialog dialog;

        [Header("Audio")]
        [SerializeField]
        private AudioSource audioSource;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                DestroyImmediate(gameObject);

            if (m_camera)
                m_camera.orthographicSize = (float)Screen.height / Screen.width >= 1.95f ? 9.75f : 8;
        }

        void Start()
        {
            StartCoroutine(CR_WalkthroughSteps());
            instructionButton.interactable = false;
            if (!PlayerPrefs.HasKey(PlayerDb.FINISH_TUTORIAL_KEY))
                skipBtn.gameObject.SetActive(false);
            if (SoundManager.Instance)
                SoundManager.Instance.autoPlayInGameSound = false;
        }

        private void OnDestroy()
        {
            if (SoundManager.Instance)
                SoundManager.Instance.autoPlayInGameSound = true;
            if (confettiCR != null)
                StopCoroutine(confettiCR);
        }

        private float tutorialTime = 0;
        private IEnumerator CR_WalkthroughSteps()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => PlayerPrefs.GetInt(ConfirmPolicyPanelController.CONFIRM_POLICY_KEY, 0) == 1);
            PlayerPrefs.SetInt(HAS_ENTER_TUTORIAL_KEY, 1);
            tutorialTime = Time.time;
            foreach (WalkthroughStep step in walkthroughSteps)
            {
                step.StartWalkthrough(this);
                yield return new WaitUntil(() =>
                {
                    return step.isFinished;
                });
            }

            yield return null;
            tutorialFinished = true;
            tutorialTime = Time.time - tutorialTime;
            if (!HadFinishTutorialBefore)
                onFinishTutorialFirstTime(tutorialTime);
            PlayerPrefs.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_REWARD_KEY, 1);
            PlayerDb.Save();
            tutorialComplatePanel.GetTutorialBoardTexture();
            Debug.Log("Finish all walkthrough steps");

            //if (SoundManager.Instance)
            //{
            //    SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver, true);
            //    SoundManager.Instance.PlaySoundDelay(0, SoundManager.Instance.confetti, true);
            //}
            //PlayLeavesParticle(Mathf.Infinity);
            yield return new WaitForSeconds(2.3f);
            //headerInOutAnim.FadeIn (0.5f);
            //endInstruction.FadeIn(0.5f);
            tutorialComplatePanel.StopRecordingGif();
            yield return new WaitForSeconds(1);
            tutorialComplatePanel.Show();
            tutorialComplatePanel.ShowRibbonIfNot();
            //StartCoroutine(CR_MoveUpFinishBtn());

        }

        IEnumerator CR_MoveUpFinishBtn()
        {
            float timeMove = 0.75f;
            float speed = 1 / timeMove;
            float value = 0;
            Vector3 startPosBtn = instructionInOutAnim.transform.localPosition;
            while (value < 1)
            {
                value += speed * Time.deltaTime;
                instructionInOutAnim.transform.localPosition = Vector3.Lerp(startPosBtn,
                    endAnchor.transform.localPosition, value);
                yield return null;
            }
            instructionButton.interactable = true;
        }

        public LogicalBoardTutorial RequestNewLogicalBoard(string puzzle, string solution, Index2D[] intereactableIndexes)
        {
            if (lb == null)
                return lb;
            string progress = lb.GetProgressImmediately();
            if (puzzle.Equals(progress))
            {
                return lb;
            }
            else
            {
                InitNewTutorialLogicalBoard(puzzle, solution, intereactableIndexes, progress.Length != puzzle.Length);
            }
            return lb;
        }

        private void InitNewTutorialLogicalBoard(string puzzle, string solution, Index2D[] intereactableIndexes, bool doCameraAnim = false)
        {
            lb.InitPuzzle(puzzle, solution);
            lb.SetInteractableIndex(intereactableIndexes);
            if (cameraPositionAnimation && doCameraAnim)
            {
                cameraPositionAnimation.SetOrigin(new Vector2(1.15f, 1) * (Mathf.Sqrt(puzzle.Length) * cellSize / 2 + boardBorder));
                //if (m_camera)
                //	m_camera.orthographicSize = 1 / puzzleSize * Mathf.Sqrt (puzzle.Length);
                cameraPositionAnimation.Play(AnimConstant.IN);

            }
        }

        public string i2s(Index2D i, int size)
        {
            string s = string.Format(
                "{0}{1}",
                size - i.row,
                (char)(65 + i.column));
            return s;
        }

        public Index2D s2i(string s, int size)
        {
            return new Index2D()
            {
                row = size - int.Parse(s[0].ToString()),
                column = s[1] - 65
            };
        }

        public bool HadFinishTutorialBefore
        {
            get
            {
                return PlayerDb.HasKey(PlayerDb.FINISH_TUTORIAL_KEY) || PlayerPrefs.HasKey(PlayerDb.FINISH_TUTORIAL_KEY);
            }
        }

        public void Skip()
        {
            PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            PlayerPrefs.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            onSkipButtonClicked();
            dialog.Show(I2.Loc.ScriptLocalization.Skip, I2.Loc.ScriptLocalization.Skip_Msg, () =>
            {
                BackToMainMenu();
                SkipTutorial();
            }, null, null);
        }

        public void ShowTargetTutorial()
        {
            PlayerDb.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            PlayerPrefs.SetInt(PlayerDb.FINISH_TUTORIAL_KEY, 1);
            onSkipButtonClicked();
            dialog.Show("TARGET", "Fill the board with 0s and 1s while satisfying 3 simple rules.", "OK", "NO", () => { }, null, null);
        }

        public void BackToMainMenu()
        {
            if (SceneLoadingManager.Instance != null)
            {
                SceneLoadingManager.Instance.LoadMainScene();
                if (SoundManager.Instance != null)
                    SoundManager.Instance.FadeInMenuBackgroundMusic(0.5f);
                FadeMusic(false, 0.5f);
            }
            else
                SceneManager.LoadScene("Main");
        }

        public void PlayLeavesParticle(float duration)
        {
            if (confettiCR != null)
            {
                centerParticle.Stop();
                StopCoroutine(confettiCR);
            }
            confettiCR = StartCoroutine(CR_CompleteParticleSystem());
        }

        private IEnumerator CR_CompleteParticleSystem()
        {
            yield return new WaitForSeconds(0.5f);
            centerParticle.Play();
            yield return new WaitForSeconds(3.94f);
            centerParticle.Stop();
        }

        private IEnumerator CR_EndLeavesParticle(float duration)
        {
            yield return new WaitForSeconds(duration);
            leavesParticle.Stop();
        }

        public void FadeMusic(bool isFadeIn, float timeFade)
        {
            StartCoroutine(CR_FadeMuic(isFadeIn, timeFade));
        }
        IEnumerator CR_FadeMuic(bool isFadeIn, float timeFade)
        {
            if (SoundManager.Instance != null && !SoundManager.Instance.IsMusicMuted())
            {
                if (isFadeIn)
                    audioSource.Play();
                float value = 0;
                float speed = 1 / timeFade;
                float startVol = audioSource.volume;
                float endVol = isFadeIn ? 1 : 0;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    audioSource.volume = Mathf.Lerp(startVol, endVol, value);
                    yield return null;
                }
                if (!isFadeIn)
                    audioSource.Stop();
            }
        }

    }
}
