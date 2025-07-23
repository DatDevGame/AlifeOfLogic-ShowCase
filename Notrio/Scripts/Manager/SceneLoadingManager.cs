using System;
using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Takuzu
{
    public class SceneLoadingManager : MonoBehaviour
    {
        public static SceneLoadingManager Instance { get; private set; }

        public const string FINISH_TUTORIAL_KEY = "FINISH_TUTORIAL";
        [HideInInspector]
        public string MULTIPLAYER_INVITATION_TITLE;
        [HideInInspector]
        public string MULTIPLAYER_INVITATION_MESSAGE;
        public string managerSceneName = "ManagerClass";
        public string mainSceneName = "Main";
        public string tutorialSceneName = "Tutorial";
        public string tournamentSceneName = "Tournament";
        public string endingScene = "EndingScene";
        public string multiplayerScene = "Multiplayer";
        public float initDelay;

        [HideInInspector]
        public bool allSceneLoaded = false;

        private void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnEnable()
        {
            GameServices.RegisterInvitationDelegate(OnInvitationReceived);
            GameServices.UserLoginSucceeded += GameServices_UserLoginSucceeded;
        }

        private void GameServices_UserLoginSucceeded()
        {
        }

        private void OnInvitationReceived(Invitation invitation, bool shouldAutoAccept)
        {
            StartCoroutine(ShowInvitationCR(invitation, shouldAutoAccept));
        }

        private IEnumerator ShowInvitationCR(Invitation invitation, bool shouldAutoAccept)
        {
            MULTIPLAYER_INVITATION_TITLE = I2.Loc.ScriptLocalization.MULTIPLAYER_INVITATION_TITLE;
            MULTIPLAYER_INVITATION_MESSAGE = I2.Loc.ScriptLocalization.MULTIPLAYER_INVITATION_MESSAGE;

            if (allSceneLoaded == false)
                yield return new WaitUntil(() => allSceneLoaded);
            if(GameManager.Instance == null || GameManager.Instance.GameState == GameState.Prepare)
                yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.GameState == GameState.Prepare);

            //Player is in prepare gamestate ready to accept invitation
            if(shouldAutoAccept){
                //Player Accepts invitation outside of the game
                //Load multiplayer Scene and Accepts invitation;
                StartCoroutine(AcceptInvitationCR(invitation));
            }else
            {
                //Player haven't accept invitaion show popup for invitation
                yield return new WaitUntil(() => InGameNotificationPopup.Instance != null);
                InGameNotificationPopup.Instance.confirmationDialog.Show(MULTIPLAYER_INVITATION_TITLE, string.Format(MULTIPLAYER_INVITATION_MESSAGE, invitation.Inviter.DisplayName),
                I2.Loc.ScriptLocalization.OK.ToUpper(), "", () =>
                {
                    StartCoroutine(AcceptInvitationCR(invitation));
                });
            }
        }

        private IEnumerator AcceptInvitationCR(Invitation invitation)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PrepareGame();
            AsyncOperation ao = SceneManager.LoadSceneAsync(multiplayerScene);
            yield return ao;
            yield return new WaitForSeconds(0.5f);
            GameServices.RealTime.AcceptInvitation(invitation, true, MultiplayerManager.Instance);
        }

        private void Start()
        {
            LoadingScreen.Instance.loadingGraphic.color = Color.white;
            LoadingScreen.Instance.loadingGraphic.gameObject.SetActive(true);
            LoadingScreen.Instance.loadingGraphic.raycastTarget = true;
            LoadingScreen.Instance.AnimateLoading();
            StartCoroutine(CrInit());
        }

        private IEnumerator CrInit()
        {
            yield return new WaitForSeconds(initDelay);

            AsyncOperation ao1 = SceneManager.LoadSceneAsync(managerSceneName);
            //LoadingScreen.Instance.SetDisplayedProgress("Initializing", ao1);
            //LeaderboardBuilder.LoadFlags();
            //SceneManager.LoadScene(managerSceneName);
            yield return ao1;
            string sceneName = PlayerPrefs.HasKey(PlayerDb.FINISH_TUTORIAL_KEY) ? mainSceneName : tutorialSceneName;
            //if (sceneName.Equals(tutorialSceneName))
            //    PlayerPrefs.SetInt(FINISH_TUTORIAL_KEY, 1);
            AsyncOperation ao2 = SceneManager.LoadSceneAsync(sceneName);
            LoadingScreen.Instance.SetDisplayedProgress(string.Format("Loading {0}", sceneName), ao2);

            yield return ao2;
            //SceneManager.LoadScene(sceneName);
            LoadingScreen.Instance.loadingAnim.Play(1);
            LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);
            allSceneLoaded = true;
        }

        public void Reload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadTutorialScene()
        {
            StartCoroutine(CrLoadTutorialScene());
        }

        private IEnumerator CrLoadTutorialScene()
        {
            if (LoadingScreen.Instance == null)
            {
                SceneManager.LoadSceneAsync(tutorialSceneName);
            }
            else
            {
                LoadingScreen.Instance.ActivateLoadingGraphic();
                LoadingScreen.Instance.loadingAnim.Play(0);
                yield return new WaitForSeconds(LoadingScreen.Instance.loadingAnim.duration);

                AsyncOperation o = SceneManager.LoadSceneAsync(tutorialSceneName);
                LoadingScreen.Instance.SetDisplayedProgress("Loading Tutorial", o);

                yield return o;
                //SceneManager.LoadScene(tutorialSceneName);
                LoadingScreen.Instance.loadingAnim.Play(1);
                LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);
            }
        }

        public void LoadMainScene()
        {
            StartCoroutine(CrLoadMainScene());
        }

        private IEnumerator CrLoadMainScene()
        {
            if (LoadingScreen.Instance == null)
            {
                SceneManager.LoadSceneAsync(mainSceneName);
            }
            else
            {
                LoadingScreen.Instance.ActivateLoadingGraphic();
                LoadingScreen.Instance.loadingAnim.Play(0);
                yield return new WaitForSeconds(LoadingScreen.Instance.loadingAnim.duration);

                AsyncOperation o = SceneManager.LoadSceneAsync(mainSceneName);
                LoadingScreen.Instance.SetDisplayedProgress("Loading Main", o);

                yield return o;
                //SceneManager.LoadScene(mainSceneName);
                LoadingScreen.Instance.loadingAnim.Play(1);
                LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);

            }
        }

        public void LoadTournamentScene()
        {
            StartCoroutine(CrLoadTournamentScene());
        }

        public void LoadEndingScene()
        {
            StartCoroutine(CrLoadEndingScene());
        }

        private IEnumerator CrLoadEndingScene()
        {
            if (LoadingScreen.Instance == null)
            {
                SceneManager.LoadSceneAsync(endingScene);
            }
            else
            {
                LoadingScreen.Instance.ActivateLoadingGraphic();
                LoadingScreen.Instance.loadingAnim.Play(0);
                yield return new WaitForSeconds(LoadingScreen.Instance.loadingAnim.duration);

                AsyncOperation o = SceneManager.LoadSceneAsync(endingScene);
                LoadingScreen.Instance.SetDisplayedProgress("Loading Ending", o);

                yield return o;
                //SceneManager.LoadScene(tournamentSceneName);
                LoadingScreen.Instance.loadingAnim.Play(1);
                LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);

            }
        }

        private IEnumerator CrLoadTournamentScene()
        {
            if (LoadingScreen.Instance == null)
            {
                SceneManager.LoadSceneAsync(tournamentSceneName);
            }
            else
            {
                LoadingScreen.Instance.ActivateLoadingGraphic();
                LoadingScreen.Instance.loadingAnim.Play(0);
                yield return new WaitForSeconds(LoadingScreen.Instance.loadingAnim.duration);

                AsyncOperation o = SceneManager.LoadSceneAsync(tournamentSceneName);
                LoadingScreen.Instance.SetDisplayedProgress("Loading Tournament", o);

                yield return o;
                //SceneManager.LoadScene(tournamentSceneName);
                LoadingScreen.Instance.loadingAnim.Play(1);
                LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);

            }
        }

        public void LoadMultiPlayerScene()
        {
            StartCoroutine(CrLoadMultiplayerScene());
        }

        private IEnumerator CrLoadMultiplayerScene()
        {
            if (LoadingScreen.Instance == null)
            {
                SceneManager.LoadSceneAsync(multiplayerScene);
            }
            else
            {
                LoadingScreen.Instance.ActivateLoadingGraphic();
                LoadingScreen.Instance.loadingAnim.Play(0);
                yield return new WaitForSeconds(LoadingScreen.Instance.loadingAnim.duration);

                AsyncOperation o = SceneManager.LoadSceneAsync(multiplayerScene);
                LoadingScreen.Instance.SetDisplayedProgress("Loading MultiPlayer", o);

                yield return o;
                //SceneManager.LoadScene(tournamentSceneName);
                LoadingScreen.Instance.loadingAnim.Play(1);
                LoadingScreen.Instance.DeactivateLoadingGraphic(LoadingScreen.Instance.loadingAnim.duration);

            }
        }

    }
}