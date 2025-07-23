using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pinwheel;

namespace Takuzu
{
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance;

        public Canvas canvas;
        public Graphic loadingGraphic;
        public ColorAnimation loadingAnim;
        public Graphic syncingGraphic;
        public AnimController syncAnim;
        public Text progressText;
        public Slider progressBar;
        public Text versionText;

        [Header("Loading anim")]
        public Image loadingIcon;
        public Sprite[] spriteSheet;

        [Header("Aspect ratio not support")]
        public GameObject aspectNotSupportScreen;

        private const int IN = 0, OUT = 1;
        private Coroutine displayProgressCoroutine;
        private Coroutine animateCoroutine;

        private bool isShowing;
        public bool IsShowing
        {
            get { return isShowing; }
            private set { isShowing = value; }
        }

        private void OnEnable()
        {
            CloudServiceManager.onPlayerDbSyncBegin += OnSyncBegin;
            CloudServiceManager.onPlayerDbSyncEnd += OnSyncEnd;
            progressText.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(false);
            if (versionText != null)
                versionText.text = string.Format("{0} {1}", I2.Loc.ScriptLocalization.VERSION, Application.version);
        }

        private void OnDisable()
        {
            CloudServiceManager.onPlayerDbSyncBegin -= OnSyncBegin;
            CloudServiceManager.onPlayerDbSyncEnd -= OnSyncEnd;
        }

        private void OnSyncBegin()
        {
            syncingGraphic.gameObject.SetActive(true);
            syncAnim.Play();
        }

        private void OnSyncEnd()
        {
            syncAnim.Stop();
            syncingGraphic.gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                syncingGraphic.gameObject.SetActive(false);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            if (versionText != null)
                versionText.text = string.Format("{0} {1}", I2.Loc.ScriptLocalization.VERSION, Application.version);
            StartCoroutine(CrOnSceneLoaded(s, m));
        }

        IEnumerator CrOnSceneLoaded(Scene s, LoadSceneMode m)
        {
            yield return null;
            canvas.worldCamera = Camera.main;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void FlashLoadingScreen()
        {
            ActivateLoadingGraphic();
            AnimateLoading();
            loadingAnim.Play(2);
            DeactivateLoadingGraphic(loadingAnim.duration);
        }

        public void DeactivateLoadingGraphic(float delay = 0)
        {
            if (delay == 0)
            {
                loadingGraphic.gameObject.SetActive(false);
                StopAnimate();
            }
            else
                StartCoroutine(CrDeactivateLoadingGraphic(delay));
        }

        private IEnumerator CrDeactivateLoadingGraphic(float delay)
        {
            yield return new WaitForSeconds(delay);
            loadingGraphic.gameObject.SetActive(false);
            StopAnimate();
        }

        public void ActivateLoadingGraphic(float delay = 0)
        {
            if (delay == 0)
            {
                loadingGraphic.gameObject.SetActive(true);
                AnimateLoading();
            }
            else
                StartCoroutine(CrActivateLoadingGraphic(delay));
        }

        private IEnumerator CrActivateLoadingGraphic(float delay)
        {
            yield return new WaitForSeconds(delay);
            loadingGraphic.gameObject.SetActive(true);
            AnimateLoading();
        }

        public void SetDisplayedProgress(string progressName, AsyncOperation ops)
        {
            if (displayProgressCoroutine != null)
                StopCoroutine(displayProgressCoroutine);
            displayProgressCoroutine = StartCoroutine(CrDisplayProgress(progressName, ops));
        }

        private IEnumerator CrDisplayProgress(string progressName, AsyncOperation ops)
        {
            progressText.gameObject.SetActive(true);
            progressBar.gameObject.SetActive(true);
            while (!ops.isDone)
            {
                progressText.text = string.Format("{0}... {1:0%}", progressName, ops.progress);
                progressBar.value = ops.progress;
                yield return null;
            }
            progressText.text = string.Format("{0}... {1:0%}", progressName, ops.progress);
            progressBar.value = ops.progress;

            yield return new WaitForSeconds(0.15f);

            progressText.gameObject.SetActive(false);
            progressBar.gameObject.SetActive(false);
        }

        public void SetProgressDisplay(string progressName, float progress)
        {
            progressBar.gameObject.SetActive(true);
            if (progress < 1)
            {
                progressText.text = string.Format("{0}... {1:0%}", progressName, progress);
                progressBar.value = progress;
            }
            progressBar.gameObject.SetActive(false);
        }

        public void EnableDescriptionText()
        {
            progressText.enabled = true;
            progressText.gameObject.SetActive(true);
        }

        public void DisableDiscriptionText()
        {
            progressText.enabled = false;
            progressText.gameObject.SetActive(false);
        }

        public void AnimateLoading()
        {
            StopAnimate();
            animateCoroutine = StartCoroutine(CrAnimateLoading());
        }

        private IEnumerator CrAnimateLoading()
        {
            IsShowing = true;
            for (int i = 0; i < spriteSheet.Length; ++i)
            {
                loadingIcon.sprite = spriteSheet[i];
                if (i == spriteSheet.Length - 1)
                    i = 0;
                yield return null;
                yield return null;
            }
        }

        public void StopAnimate()
        {
            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
                animateCoroutine = null;
                IsShowing = false;
            }
        }
    }
}