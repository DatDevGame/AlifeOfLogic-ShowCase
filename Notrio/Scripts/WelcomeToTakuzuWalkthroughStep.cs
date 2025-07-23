using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
namespace Takuzu
{
    public class WelcomeToTakuzuWalkthroughStep : WalkthroughStep
    {
        public PositionAnimation cameraPositionAnimation;
        public string puzzle = ".00111000.1.011.";
        public string solution = "1001110000110110";
        public float timeMoveUp = 0.3f;
        [Header("UI References")]
        public UIScriptAnimationManager text_1;
        public UIScriptAnimationManager text_2;
        public UIScriptAnimationManager text_Welcome;
        public UIScriptAnimationManager text_3;
        public UIScriptAnimationManager text_4;
        public UIScriptAnimationManager ruleIntroductionTextAnim;
        public UIScriptAnimationManager introductionHeaderTextAnim;
        public UIScriptAnimationManager startBtnAnim;
        public RectTransform pointAnchor;
        public AnimationCurve welCurveAni;
        public Button startBtn;
        public GameObject endAnchor;
        public Text targetTxt;

        [SerializeField]
        private bool runWelcomeMessage = true;

        private bool startRule = false;
        private Vector3 originalStartBtnPos;
        private bool isShowedConsent;

        public void Awake()
        {
            transform.localScale = ((float)Screen.height / Screen.width >= 2 ? 1 : 1.15f) * Vector3.one;
            targetTxt.text = string.Format(I2.Loc.ScriptLocalization.Target_Description, 0, 1);
            isShowedConsent = PlayerPrefs.GetInt(ConfirmPolicyPanelController.CONFIRM_POLICY_KEY, 0) == 1;
        }

        public void StarTutorialtRuleStep()
        {
            startBtnAnim.FadeOut(0.6f);
            startBtnAnim.GetComponent<Image>().raycastTarget = false;
            startRule = true;
            startBtn.interactable = false;
            //StartCoroutine(CR_MoveStartBtn(endAnchor.transform.position, originalStartBtnPos,0.5f));
        }

        public override void StartWalkthrough(TutorialManager4 tutorialManager)
        {
            AlolAnalytics.StartTutorialMain();

            base.StartWalkthrough(tutorialManager);
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(CR_Welcome());
            originalStartBtnPos = startBtn.transform.position;

            AlolAnalytics.Tutorial(1);
        }

        void FadeAll()
        {
            //text_4.FadeOut(timeMoveUp + 0.5f);
            text_1.FadeOut(timeMoveUp);
            text_2.FadeOut(timeMoveUp);
            text_3.FadeOut(timeMoveUp + 0.25f);
            text_Welcome.FadeOut(timeMoveUp);
            startBtnAnim.FadeOut(timeMoveUp);
        }

        void MoveUI(GameObject obj, Vector3 dir, float time)
        {
            StartCoroutine(CR_MoveUI(obj, dir, time));
        }

        IEnumerator CR_MoveUI(GameObject obj, Vector3 dir, float time)
        {
            float value = 0;
            float speed = 1 / time;
            RectTransform rect = obj.GetComponent<RectTransform>();
            Vector3 startPos = rect.localPosition;
            Vector3 endPos = startPos + dir;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                rect.localPosition = Vector3.Lerp(startPos, endPos, welCurveAni.Evaluate(value));
                yield return null;
            }
        }

        IEnumerator CR_ScaleUI(GameObject obj, Vector3 adjustScale, float time)
        {
            Vector3 startScale = obj.transform.localScale;
            Vector3 endScale = new Vector3(startScale.x + adjustScale.x,
                startScale.y + adjustScale.y, startScale.z + adjustScale.z);
            float value = 0;
            float speed = 1 / time;
            while (value < 1)
            {
                value += Time.deltaTime * speed;
                obj.transform.localScale = Vector3.Lerp(startScale, endScale, welCurveAni.Evaluate(value));
                yield return null;
            }
        }


        private IEnumerator CR_Welcome()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.FadeOutMenuBackgroundMusic(1);
            }

            if (runWelcomeMessage)
            {
                if (!isShowedConsent)
                    yield return new WaitForSeconds(0.75f);
                else
                    yield return new WaitForSeconds(2);
                text_1.FadeIn(1);
                MoveUI(text_1.gameObject, Vector3.up * 20, 1);
                StartCoroutine(CR_ScaleUI(text_1.gameObject, Vector3.one * 0.03f, 1));

                yield return new WaitForSeconds(1.5f);
                MoveUI(text_1.gameObject, Vector3.up * 20, 0.8f);
                //StartCoroutine(CR_ScaleUI(text_1.gameObject, -Vector3.one * 0.03f, 0.8f));
                text_1.FadeOut(0.95f);

                yield return new WaitForSeconds(1.1f);
                MoveUI(text_2.gameObject, Vector3.up * 20, 1f);
                StartCoroutine(CR_ScaleUI(text_2.gameObject, Vector3.one * 0.02f, 1f));
                text_2.FadeIn(1f);

                yield return new WaitForSeconds(2.25f);
                //StartCoroutine(CR_ScaleUI(text_2.gameObject, -Vector3.one * 0.03f, 0.8f));
                MoveUI(text_2.gameObject, Vector3.up * 20, 0.8f);
                text_2.FadeOut(0.8f);
            }

            yield return new WaitForSeconds(1.5f);
            MoveUI(text_Welcome.gameObject, Vector3.up * 25, 1.5f);
            text_Welcome.FadeIn(1.5f);
            StartCoroutine(CR_ScaleUI(text_Welcome.gameObject, Vector3.one * 0.0f, 1f));

            yield return new WaitForSeconds(0.15f);
            MoveUI(text_3.gameObject, Vector3.up * 25, 1.5f);
            text_3.FadeIn(1.5f);
            StartCoroutine(CR_ScaleUI(text_3.gameObject, Vector3.one * 0.02f, 1f));

            //yield return new WaitForSeconds(0.15f);
            //RectTransform rect4 = text_4.GetComponent<RectTransform>();
            //rect4.localPosition = new Vector2(rect4.localPosition.x, rect4.localPosition.y - (text_4.transform.GetComponent<Text>().cachedTextGenerator.lineCount - 1) * 15);
            //MoveUI(text_4.gameObject, Vector3.up * 25, 1.5f);
            //text_4.FadeIn(1.5f);
            //StartCoroutine(CR_ScaleUI(text_4.gameObject, Vector3.one * 0.03f, 1));

            yield return new WaitForSeconds(1.75f);
            text_Welcome.FadeOut(0.8f);
            StartCoroutine(CR_ScaleUI(text_Welcome.gameObject, -Vector3.one * 0.03f, 0.8f));
            MoveUI(text_Welcome.gameObject, Vector3.up * 60, 0.8f);

            yield return new WaitForSeconds(0.1f);
            text_3.FadeOut(0.8f);
            StartCoroutine(CR_ScaleUI(text_3.gameObject, -Vector3.one * 0.03f, 0.8f));
            MoveUI(text_3.gameObject, Vector3.up * 65, 0.8f);

            //yield return new WaitForSeconds(0.1f);
            //text_4.FadeOut(0.8f);
            //StartCoroutine(CR_ScaleUI(text_4.gameObject, -Vector3.one * 0.03f, 0.8f));
            //MoveUI(text_4.gameObject, Vector3.up * 80, 0.8f);
            yield return new WaitForSeconds(0.65f);
            TutorialManager4.Instance.FadeMusic(true, 1);

            Index2D[] intereactableIndexes = null;
            base.tutorialManager.RequestNewLogicalBoard(puzzle, solution, intereactableIndexes);

            yield return new WaitUntil(() =>
            {
                return !cameraPositionAnimation.isAnimationRunning;
            });
            yield return new WaitForSeconds(0.3f);
            //TutorialManager4.Instance.ShowTargetTutorial();
            //yield return new WaitUntil(() => !TutorialManager4.Instance.dialog.IsShowing);
            introductionHeaderTextAnim.FadeIn(0.75f);
            ruleIntroductionTextAnim.FadeIn(0.75f);
            yield return new WaitForSeconds(0.8f);
            StartCoroutine(CR_MoveStartBtn(originalStartBtnPos, endAnchor.transform.position, 0.5f));
            startBtnAnim.FadeIn(0.2f);
            yield return new WaitUntil(() => startRule);
            yield return StartCoroutine(CR_WaitNext(0.6f, true));
            introductionHeaderTextAnim.FadeOut(0.5f);
            ruleIntroductionTextAnim.FadeOut(0.5f);
            yield return new WaitForSeconds(0.6f);

            isFinished = true;
            gameObject.SetActive(false);
        }

        IEnumerator CR_MoveStartBtn(Vector3 startPos, Vector3 endPos, float timeMove)
        {
            AlolAnalytics.Tutorial(2);

            float speed = 1 / timeMove;
            float value = 0;
            while (value < 1)
            {
                value += speed * Time.deltaTime;
                startBtn.transform.position = Vector3.Lerp(startPos, endPos, value);
                yield return null;
            }
            startBtn.interactable = true;
        }
    }
}
