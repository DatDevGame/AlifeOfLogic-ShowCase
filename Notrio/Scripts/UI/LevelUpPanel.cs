using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;
using System.Data;
using System;

namespace Takuzu
{
    public class LevelUpPanel : OverlayPanel
    {
        public UiGroupController controller;
        public GameObject container;
        public Image rankIcon;
        public Text rankText;
        public Image sunburstImage;
        public AnimController sunburstAnim;
        public Button shareButton;
        public Button continueButton;
        public float showDelay;
        public Animator animator;
        public ParticleSystem starParticle;
        public ParticleSystem leavesParticle;
        public Image oldRankIcon;
        public Image newRankIcon;
        public AnimController textShinyAnim;
        public AnimController iconShinyAnim;
        public AnimController iconScaleAnim;
        public OverlayPanel callingSource;
        public bool readyToShow;
        [HideInInspector]
        public Image darkenImage;
        public Color[] sunburstColors = { Color.blue, Color.yellow, Color.red };

        public override void Show()
        {
            enabled = true;
            readyToShow = false;
            controller.ShowIfNot();
            //container.SetActive(true);
            IsShowing = true;
            //sunburstAnim.Play();
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        public override void Hide()
        {
            controller.HideIfNot();
            //container.SetActive(false);
            IsShowing = false;
            animator.SetTrigger("End");
            StopStarParticle();
            StopLeavesParticle();
            StopSunburstAnim();
            StopTextShinyAnim();
            StopRankIconShinyAnim();
            StopRankIconScaleAnim();
            //transform.SendToBack();
            onPanelStateChanged(this, false);
            if (callingSource != null)
                callingSource.Show();
            enabled = false;
        }

        private void Awake()
        {
            // if (UIReferences.Instance != null)
            // {
            //     UpdateReferences();
            // }
            // UIReferences.UiReferencesUpdated += UpdateReferences;
            // PlayerInfoManager.onLevelUp += OnLevelUp;
        }

        // private void UpdateReferences()
        // {
        //     darkenImage = UIReferences.Instance.darkenImage;
        // }

        private void OnDestroy()
        {
            // PlayerInfoManager.onLevelUp -= OnLevelUp;
            // UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        private void Start()
        {
            shareButton.onClick.AddListener(delegate
                {
                    SocialManager.Instance.NativeShareScreenshot();
                });

            continueButton.onClick.AddListener(delegate
                {
                    Hide();
                });

            enabled = false;
        }

        // private void Update()
        // {
        //     if (darkenImage != null)
        //         darkenImage.enabled = !IsShowing;
        // }

        public void SetInfo(PlayerInfo info)
        {
            rankText.text = ExpProfile.active.rank[info.level].ToUpper();
            //rankIcon.sprite = ExpProfile.active.icon[info.level];

            Color sunburst = Color.black;

            if (info.level <= 4)
                sunburst = sunburstColors[0];
            else if (info.level > 4 && info.level <= 7)
                sunburst = sunburstColors[1];
            else if (info.level > 7 && info.level <= 10)
                sunburst = sunburstColors[2];
            else
                Debug.Log("*** Looks like we've got a new level, time to revise this code!");

            Color accent = ExpProfile.active.accentColor[info.level];
            SetAccentColor(sunburst, accent);
        }

        public void SetIcon(PlayerInfo newInfo, PlayerInfo oldInfo)
        {
            Sprite newRankSprite = ExpProfile.active.icon[newInfo.level];
            newRankIcon.sprite = newRankSprite;
            Sprite oldRankSprite = ExpProfile.active.icon[oldInfo.level];
            oldRankIcon.sprite = oldRankSprite;
        }

        public void SetAccentColor(Color sunburstColor, Color textColor)
        {
            sunburstImage.color = new Color(sunburstColor.r, sunburstColor.g, sunburstColor.b, sunburstImage.color.a);
            rankText.color = sunburstColor;

            //            rankText.color = textColor;
            //ParticleSystem.MainModule m = starParticle.main;
            //m.startColor = new Color(c.r, c.g, c.b, m.startColor.color.a);
        }

        private void OnLevelUp(PlayerInfo info, PlayerInfo oldInfo)
        {
            //SetInfo(info);
            //SetIcon(info, oldInfo);
            //readyToShow = true;
            //StartCoroutine(CrOnLevelUp(info, oldInfo));
        }

        private IEnumerator CrOnLevelUp(PlayerInfo info, PlayerInfo oldInfo)
        {
            yield return new WaitForSeconds(showDelay);
            Show();
            PlayLevelUpAnim();
        }

        public void PlayLevelUpAnim()
        {
            animator.SetTrigger("Begin");
        }

        public void PlayStarParticle()
        {
            starParticle.Play();
        }

        public void StopStarParticle()
        {
            starParticle.Stop();
        }

        public void PlayLeavesParticle()
        {
            leavesParticle.Play();
        }

        public void StopLeavesParticle()
        {
            leavesParticle.Stop();
        }

        public void PlaySunburstAnim()
        {
            sunburstAnim.Play();
        }

        public void StopSunburstAnim()
        {
            sunburstAnim.Stop();
        }

        public void PlayTextShinyAnim()
        {
            textShinyAnim.Play();
        }

        public void StopTextShinyAnim()
        {
            textShinyAnim.Stop();
        }

        public void PlayRankIconShinyAnim()
        {
            iconShinyAnim.Play();
        }

        public void StopRankIconShinyAnim()
        {
            iconShinyAnim.Stop();
        }

        public void PlayRankIconScaleAnim()
        {
            iconScaleAnim.Play();
        }

        public void StopRankIconScaleAnim()
        {
            iconScaleAnim.Stop();
        }

        public void PlaySound()
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.rewarded, true);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Test/Level up")]
        public static void TestLevelUpAnim()
        {
            LevelUpPanel panel = FindObjectOfType<LevelUpPanel>();
            panel.PlayTest();
        }

        public void PlayTest()
        {
            //            int level = Random.Range(2, 11);
            int level = 8;
            Debug.Log("LEVEL UP TO: " + level);
            PlayerInfo newInfo = new PlayerInfo(level, 1);
            PlayerInfo oldInfo = new PlayerInfo(level - 1, 1);
            OnLevelUp(newInfo, oldInfo);
        }
#endif

    }
}