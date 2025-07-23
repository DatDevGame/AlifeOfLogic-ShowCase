using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EasyMobile;
using System.IO;
using UnityEngine.Audio;

namespace Takuzu
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [System.Serializable]
        public class Sound
        {
            public AudioClip clip;
            [HideInInspector]
            public int simultaneousPlayCount = 0;
        }

        [Header("Max number allowed of same sounds playing together")]
        public int maxSimultaneousSounds = 7;

        // List of sounds used in this game
        public AudioClip[] menuTracks;
        public AudioClip[] ingameTracks;
        public Sound button;
        public Sound coin;
        public Sound gameOver;
        public Sound tick;
        public Sound rewarded;
        public Sound unlock;
        public Sound buyCoin;
        public Sound cellToggle;
        public Sound cellRevealed;
        public Sound cellUndone;
        public Sound ping;
        public Sound lose;
        public Sound confetti;
        public Sound highRewarded;

		public bool autoPlayInGameSound = true;

        public delegate void OnMuteStatusChanged(bool isMuted);

        public static event OnMuteStatusChanged MuteStatusChanged;

        public delegate void OnMusicStatusChanged(bool isOn);

        public static event OnMusicStatusChanged MusicStatusChanged;

        enum PlayingState
        {
            Playing,
            Paused,
            Stopped
        }

        public AudioSource sfxSource;
        public AudioSource sfxSpecialSource;
        public AudioSource bgmMenuSource;
        public AudioSource bgmIngameSource;
        public AudioMixer mixer;
        public AudioMixerSnapshot currentSnapshot;
        public string menuSnapshot;
        public string ingameSnapshot;
        public float snapshotTransitionDuration;

        private PlayingState menuBgmState = PlayingState.Stopped;
        private PlayingState ingameBgmState = PlayingState.Stopped;
        private const string MUTE_PREF_KEY = "MutePreference";
        private const int MUTED = 1;
        private const int UN_MUTED = 0;
        private const string MUSIC_PREF_KEY = "MusicPreference";
        private const int MUSIC_OFF = 0;
        private const int MUSIC_ON = 1;

        private Coroutine fadeBgMusicCoroutine;

        private void OnEnable()
        {
            GameManager.GameStateChanged += GameManager_GameStateChanged;
            LogicalBoard.onCellClicked += OnCellClicked;
            LogicalBoard.onCellRevealed += OnCellRevealed;
            LogicalBoard.onCellUndone += OnCellUndone;
            InAppPurchasing.PurchaseCompleted += OnPurchaseCompleted;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= GameManager_GameStateChanged;
            LogicalBoard.onCellClicked -= OnCellClicked;
            LogicalBoard.onCellRevealed -= OnCellRevealed;
            LogicalBoard.onCellUndone -= OnCellUndone;
            InAppPurchasing.PurchaseCompleted -= OnPurchaseCompleted;
        }

        void GameManager_GameStateChanged(GameState newState, GameState oldState)
        {
            if (SceneManager.GetActiveScene().name.Equals("ManagerClass")
                && PlayerPrefs.GetInt(PlayerDb.FINISH_TUTORIAL_KEY) == 0)
                return;
            if (newState == GameState.Prepare)
            {
                AudioClip clip = menuTracks[Random.Range(0, menuTracks.Length)];
                if (clip != bgmMenuSource.clip)
                    menuBgmState = PlayingState.Stopped;
                bgmMenuSource.clip = clip;
                if (menuBgmState == PlayingState.Stopped)
                {
                    bgmMenuSource.Play();
                    menuBgmState = PlayingState.Playing;
                }
                else if (menuBgmState == PlayingState.Paused)
                {
                    bgmMenuSource.UnPause();
                    menuBgmState = PlayingState.Playing;
                }
                currentSnapshot = mixer.FindSnapshot(menuSnapshot);
                currentSnapshot.TransitionTo(snapshotTransitionDuration);
                //CoroutineHelper.Instance.DoActionDelay(
                //        () =>
                //        {
                //            if (ingameBgmState == PlayingState.Playing || currentSnapshot.name.Equals(menuSnapshot))
                //            {
                //                bgmIngameSource.Pause();
                //                ingameBgmState = PlayingState.Paused;
                //            }
                //        },
                //        snapshotTransitionDuration);
            }
            if (newState == GameState.Playing && oldState == GameState.Prepare)
            {
                if(ingameMusicRunningCR != null)
                    StopCoroutine(ingameMusicRunningCR);
                ingameMusicRunningCR = StartCoroutine(InGameMusicCR());
            }
            if (newState == GameState.GameOver && autoPlayInGameSound)
            {
                if (!PuzzleManager.currentIsMultiMode || (PuzzleManager.currentIsMultiMode && MultiplayerSession.playerWin))
                {
                    PlaySound(gameOver, true);
                    //PlaySoundDelay(0.5f, confetti, true);
                }
                else
                    PlaySound(lose, true);
            }
        }


        private Coroutine ingameMusicRunningCR;
        private IEnumerator InGameMusicCR()
        {
            List<AudioClip> availableAudioClip = new List<AudioClip>(ingameTracks);
            availableAudioClip.RemoveAll(cl => cl == bgmIngameSource.clip);

            AudioClip clip = availableAudioClip[Random.Range(0, availableAudioClip.Count)];
            if (clip != bgmIngameSource.clip)
                ingameBgmState = PlayingState.Stopped;
            bgmIngameSource.clip = clip;
            if (ingameBgmState == PlayingState.Stopped)
            {
                bgmIngameSource.Play();
                ingameBgmState = PlayingState.Playing;
            }
            else if (ingameBgmState == PlayingState.Paused)
            {
                bgmIngameSource.UnPause();
                ingameBgmState = PlayingState.Playing;
            }
            currentSnapshot = mixer.FindSnapshot(ingameSnapshot);
            currentSnapshot.TransitionTo(snapshotTransitionDuration);

            //CoroutineHelper.Instance.DoActionDelay(
            //        () =>
            //        {
            //            if (menuBgmState == PlayingState.Playing && currentSnapshot.name.Equals(ingameSnapshot))
            //            {
            //                bgmMenuSource.Pause();
            //                menuBgmState = PlayingState.Paused;
            //            }
            //        },
            //        snapshotTransitionDuration);
            
            float totalT = bgmIngameSource.clip.length;
            while (totalT > 0)
            {
                yield return new WaitForEndOfFrame();
                totalT -= Time.deltaTime;
                if(GameManager.Instance == null)
                    yield break;
                //If not in prepare state then break
                if(GameManager.Instance.GameState == GameState.Prepare)
                    yield break;
            }
            StartCoroutine(InGameMusicCR());
        }

        public void StopIngameBackgroundMusic()
        {
            bgmIngameSource.Stop();
            ingameBgmState = PlayingState.Stopped;
        }

        public void StopMenuBackgroundMusic()
        {
            bgmMenuSource.Stop();
            menuBgmState = PlayingState.Stopped;
        }

        public void FadeOutMenuBackgroundMusic(float timeFade)
        {
            if (!IsMusicMuted())
            {
                if (fadeBgMusicCoroutine != null)
                    StopCoroutine(fadeBgMusicCoroutine);
                fadeBgMusicCoroutine = StartCoroutine(CR_FadeOutMenuBackgroundMusic(timeFade));
            }
        }

        IEnumerator CR_FadeOutMenuBackgroundMusic(float timeFade)
        {
            yield return StartCoroutine(CR_MenuMusicFadeEffect(timeFade, bgmMenuSource.volume, 0));
            bgmMenuSource.Stop();

        }

        IEnumerator CR_MenuMusicFadeEffect(float duration, float startVol, float endVol)
        {
            if (duration <= 0)
                yield break;

            float originalVol = bgmMenuSource.volume;

            startVol = Mathf.Clamp(startVol, 0, 1);
            endVol = Mathf.Clamp(endVol, 0, 1);

            bgmMenuSource.volume = startVol;
            float timePast = 0;
            while (timePast < duration)
            {
                timePast += Time.deltaTime;
                bgmMenuSource.volume = Mathf.Lerp(startVol, endVol, timePast / duration);
                yield return null;
            }
        }

        public void PlayMenuBackgroundMusic()
        {
            bgmMenuSource.Play();
            menuBgmState = PlayingState.Playing;
        }


        public void FadeInMenuBackgroundMusic(float timeFade)
        {
            if (!IsMusicMuted())
            {
                if (fadeBgMusicCoroutine != null)
                    StopCoroutine(fadeBgMusicCoroutine);
                fadeBgMusicCoroutine = StartCoroutine(CR_FadeInPlayMenuBackgroundMusic(timeFade));
            }
        }

        IEnumerator CR_FadeInPlayMenuBackgroundMusic(float timeFade)
        {
            bgmMenuSource.Play();
            yield return StartCoroutine(CR_MenuMusicFadeEffect(timeFade, bgmMenuSource.volume, 1));
        }

        //        IEnumerator CRPrepareGameSound(GameState oldState)
        //        {
        //            if (oldState == GameState.Startup)
        //                yield break;
        //
        //            if (oldState == GameState.Playing)
        //            {
        //                StopMusic(1);
        //                yield return new WaitForSeconds(1.1f);
        //            }
        //
        //            PlayMusic(menuTracks, 3);
        //        }

        //        IEnumerator CRInGameSound(GameState oldState)
        //        {
        //            if (oldState == GameState.Prepare)
        //            {
        //                StopMusic(1);
        //                yield return new WaitForSeconds(1.1f);
        //                PlayMusic(ingameBg, 3);
        //            }
        //        }

        IEnumerator CRGameOverSound()
        {
            StopMusic(0.5f);
            yield return new WaitForSeconds(0.6f);
            PlaySound(gameOver);
        }

        public void PlaySoundDelay(float delay, Sound sound, bool isSpecial = false, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            StartCoroutine(CR_PlaySoundDelay(0.5f, confetti, true));
        }

        IEnumerator CR_PlaySoundDelay(float delay, Sound sound, bool isSpecial = false, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            yield return new WaitForSeconds(delay);
            PlaySound(sound, isSpecial, autoScaleVolume, maxVolumeScale);
        }

        private void OnPurchaseCompleted(IAPProduct obj)
        {
            InAppPurchaser.CoinPack[] p = InAppPurchaser.Instance.coinPacks;
            for (int i = 0; i < p.Length; ++i)
            {
                if (obj.Name.Equals(p[i].productName))
                {
                    PlaySound(buyCoin, true);
                    break;
                }
            }
        }

        private void OnCellClicked(Index2D i)
        {
            PlaySound(cellToggle);
        }

        private void OnCellRevealed(Index2D i)
        {
            PlaySound(cellRevealed);
        }

        private void OnCellUndone(Index2D i)
        {
            PlaySound(cellUndone);
        }

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void Start()
        {
            // Set mute based on the valued stored in PlayerPrefs
            SetSoundMute(IsSoundMuted());
            SetMusicMute(IsMusicMuted());
        }

        /// <summary>
        /// Plays the given sound with option to progressively scale down volume of multiple copies of same sound playing at
        /// the same time to eliminate the issue that sound amplitude adds up and becomes too loud.
        /// </summary>
        /// <param name="sound">Sound.</param>
        /// <param name="isSpecial">set to true to enable ducking effect</param>
        /// <param name="autoScaleVolume">If set to <c>true</c> auto scale down volume of same sounds played together.</param>
        /// <param name="maxVolumeScale">Max volume scale before scaling down.</param>
        public void PlaySound(Sound sound, bool isSpecial = false, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            StartCoroutine(CRPlaySound(sound, isSpecial, autoScaleVolume, maxVolumeScale));
        }

        IEnumerator CRPlaySound(Sound sound, bool isSpecial = false, bool autoScaleVolume = true, float maxVolumeScale = 1f)
        {
            if (sound.simultaneousPlayCount >= maxSimultaneousSounds)
            {
                yield break;
            }

            sound.simultaneousPlayCount++;

            float vol = maxVolumeScale;

            // Scale down volume of same sound played subsequently
            if (autoScaleVolume && sound.simultaneousPlayCount > 0)
            {
                vol = vol / (float)(sound.simultaneousPlayCount);
            }

            AudioSource src = isSpecial ? sfxSpecialSource : sfxSource;
            src.PlayOneShot(sound.clip, vol);

            // Wait til the sound almost finishes playing then reduce play count
            float delay = sound.clip.length * 0.7f;

            yield return new WaitForSeconds(delay);

            sound.simultaneousPlayCount--;
        }

        /// <summary>
        /// Plays the given music.
        /// </summary>
        /// <param name="music">Music.</param>
        /// <param name="loop">If set to <c>true</c> loop.</param>
        public void PlayMusic(Sound music, float fadeInDuration = 0, bool loop = true)
        {
            if (IsMusicMuted())
            {
                return;
            }

            sfxSource.clip = music.clip;
            sfxSource.loop = loop;
            sfxSource.Play();
            menuBgmState = PlayingState.Playing;

            if (fadeInDuration > 0)
                StartCoroutine(CRMusicFadeEffect(fadeInDuration, 0, sfxSource.volume));
        }

        /// <summary>
        /// Pauses the music.
        /// </summary>
        public void PauseMusic()
        {
            if (menuBgmState == PlayingState.Playing)
            {
                bgmMenuSource.Pause();
                menuBgmState = PlayingState.Paused;
            }
        }

        /// <summary>
        /// Resumes the music.
        /// </summary>
        public void ResumeMusic()
        {
            if (menuBgmState == PlayingState.Paused)
            {
                bgmMenuSource.UnPause();
                menuBgmState = PlayingState.Playing;
            }
        }

        /// <summary>
        /// Stop music.
        /// </summary>
        public void StopMusic(float fadeOutDuration = 0)
        {
            if (fadeOutDuration <= 0)
            {
                bgmMenuSource.Stop();
                menuBgmState = PlayingState.Stopped;
            }
            else
            {
                StartCoroutine(CRStopMusicWithFadeOut(fadeOutDuration));
            }
        }

        IEnumerator CRStopMusicWithFadeOut(float fadeOutDuration)
        {
            yield return StartCoroutine(CRMusicFadeEffect(fadeOutDuration, sfxSource.volume, 0));
            bgmMenuSource.Stop();
            menuBgmState = PlayingState.Stopped;
        }

        IEnumerator CRMusicFadeEffect(float duration, float startVol, float endVol)
        {
            if (duration <= 0)
                yield break;

            float originalVol = sfxSource.volume;

            startVol = Mathf.Clamp(startVol, 0, 1);
            endVol = Mathf.Clamp(endVol, 0, 1);

            sfxSource.volume = startVol;
            float timePast = 0;
            while (timePast < duration)
            {
                timePast += Time.deltaTime;
                sfxSource.volume = Mathf.Lerp(startVol, endVol, timePast / duration);
                yield return null;
            }

            yield return null;

            // Revert to original volume
            sfxSource.volume = originalVol;
        }

        /// <summary>
        /// Determines whether sound is muted.
        /// </summary>
        /// <returns><c>true</c> if sound is muted; otherwise, <c>false</c>.</returns>
        public bool IsSoundMuted()
        {
            return (PlayerPrefs.GetInt(MUTE_PREF_KEY, UN_MUTED) == MUTED);
        }

        public bool IsMusicMuted()
        {
            return (PlayerPrefs.GetInt(MUSIC_PREF_KEY, MUSIC_ON) == MUSIC_OFF);
        }

        /// <summary>
        /// Toggles the mute status.
        /// </summary>
        public void ToggleSoundMute()
        {
            // Toggle current mute status
            bool mute = !IsSoundMuted();

            if (mute)
            {
                SetSoundMute(true);
            }
            else
            {
                SetSoundMute(false);
            }
        }

        /// <summary>
        /// Toggles the mute status.
        /// </summary>
        public void ToggleMusic()
        {
            if (IsMusicMuted())
            {
                // Turn music ON
                SetMusicMute(false);
            }
            else
            {
                // Turn music OFF
                SetMusicMute(true);
            }
        }

        public void SetSoundMute(bool isMuted)
        {
            PlayerPrefs.SetInt(MUTE_PREF_KEY, isMuted ? MUTED : UN_MUTED);
            sfxSource.mute = isMuted;
            sfxSpecialSource.mute = isMuted;
            if (MuteStatusChanged != null)
            {
                MuteStatusChanged(isMuted);
            }
        }

        public void SetMusicMute(bool isMuted)
        {
            PlayerPrefs.SetInt(MUSIC_PREF_KEY, isMuted ? MUSIC_OFF : MUSIC_ON);
            bgmMenuSource.mute = isMuted;
            bgmIngameSource.mute = isMuted;

            if (MusicStatusChanged != null)
            {
                MusicStatusChanged(isMuted);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(CrInitButtonSound(scene));
        }

        private IEnumerator CrInitButtonSound(Scene scene)
        {
            yield return null;
            List<GameObject> roots = new List<GameObject>();
            scene.GetRootGameObjects(roots);
            List<Button> b = new List<Button>();
            for (int i = 0; i < roots.Count; ++i)
            {
                b.AddRange(roots[i].GetComponentsInChildren<Button>(true));
            }

            for (int i = 0; i < b.Count; ++i)
            {
                b[i].onClick.AddListener(delegate
                    {
                        PlaySound(button);
                    });
            }
        }
    }
}
