using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using Pinwheel;
using Takuzu;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCompletePanel : OverlayPanel
{
    public UiGroupController controller;
    public GameObject screenShotCover;
    public Button homeBtn;
    public Button shareBtn;

    [Header("Replay")]
    public Recorder recorder;
    public RawImage boardScreenshot;
    public ClipPlayerUI clipPlayer;
    public RawImage clipPlayerRaw;
    public float gifRecordingDelay;
    public GameObject screenshotButtonGroup;
    public Button pngButton;
    public Image pngButtonBackground;
    public GameObject pngGroup;
    public Button gifButton;
    public Image gifButtonBackground;
    public GameObject gifGroup;
    public Color buttonActiveColor;
    public Color buttonInactiveColor;
    [HideInInspector]
    public TaskPanel taskPanel;
    private AnimatedClip clip;
    private string gifUrl;
    private string gifPath;

    private bool countDownForScreeenShotIsDone = false;
    private bool ribbonHidden = true;
    public PositionAnimation ribbonAnim;

    private RenderTexture screenshotRT;
    private Texture2D convertedScreenshot;

    void Awake()
    {
        taskPanel = UIReferences.Instance.overlayTaskPanel;
        shareBtn.onClick.AddListener(delegate
        {
            if (pngGroup.activeInHierarchy)
            {
                ShareNewPng();
            }
            else if (gifGroup.activeInHierarchy)
            {
                if (string.IsNullOrEmpty(gifUrl))
                    ShareNewGif();
                else
                    ShareCachedGif();
            }

        });

        homeBtn.onClick.AddListener(delegate
        {
            TutorialManager4.Instance.BackToMainMenu();
            HideRibbonIfNot();
            Hide();
        });

        pngButton.onClick.AddListener(delegate
        {
            SelectPngGroup();
        });

        gifButton.onClick.AddListener(delegate
        {
            SelectGifGroup();
        });
    }

    public override void Hide()
    {
        //* Clean up generated texture2d and release render texture
        if (this.screenshotRT != null)
            this.screenshotRT.Release();
        if (this.convertedScreenshot != null)
            Destroy(this.convertedScreenshot);

        IsShowing = false;
        controller.HideIfNot();
        StopAllCoroutines();
        onPanelStateChanged(this, false);
    }

    public override void Show()
    {
        screenshotButtonGroup.SetActive(!PuzzleManager.currentIsChallenge && !PuzzleManager.currentIsMultiMode);
        //Checking if this is multiplayer mode then wait for the result
        //Display winning losing

        countDownForScreeenShotIsDone = false;
        screenShotCover.gameObject.SetActive(false);
        IsShowing = true;
        controller.ShowIfNot();
        transform.BringToFront();
        onPanelStateChanged(this, true);

        SelectPngGroup();
    }

    public void SetClipGif(AnimatedClip gif)
    {
        clip = gif;
    }

    private void SelectPngGroup()
    {
        gifGroup.SetActive(false);
        pngGroup.SetActive(true);
        gifButtonBackground.color = buttonInactiveColor;
        pngButtonBackground.color = buttonActiveColor;
        clipPlayer.Stop();
    }

    private void SelectGifGroup()
    {
        gifGroup.SetActive(true);
        pngGroup.SetActive(false);
        gifButtonBackground.color = buttonActiveColor;
        pngButtonBackground.color = buttonInactiveColor;
        if (clip != null)
        {
            clipPlayer.Play(clip, 0, true);
        }
    }

    public void GetTutorialBoardTexture()
    {
        StartCoroutine(CrGetTutorialBoardTexture());
    }

    private IEnumerator CrGetTutorialBoardTexture()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Texture bgTexture = FlipBackGround.GetMainSprite("bg-flip-tutorial").texture;
        this.screenshotRT = UIReferences.Instance.multiplayerShareBGController.TakeTutorialCapture(bgTexture.width, bgTexture.height, bgTexture);
        boardScreenshot.texture = this.screenshotRT;
        boardScreenshot.rectTransform.sizeDelta = new Vector2(boardScreenshot.rectTransform.sizeDelta.x, boardScreenshot.rectTransform.sizeDelta.x);
    }

    public void ShareCachedGif()
    {
        EasyMobile.Sharing.ShareURL(gifUrl, AppInfo.Instance.DEFAULT_SHARE_MSG);
    }

    public void ShareNewPng()
    {
        taskPanel.Show();
        float t = 0;
        if (!countDownForScreeenShotIsDone)
        {
            CoroutineHelper.Instance.RepeatUntil(() =>
            {
                taskPanel.SetProgress(t / 3);
                taskPanel.SetTask(I2.Loc.ScriptLocalization.PREPARE_SCREENSHOT);
                taskPanel.SetGiphyActive(false);
                taskPanel.SetEMActive(true);
                t += Time.deltaTime;
            }, Time.deltaTime, () => t > 3);
        }
        CoroutineHelper.Instance.PostponeActionUntil(() =>
        {
            countDownForScreeenShotIsDone = true;
            taskPanel.SetProgress(1);
            taskPanel.SetTask(I2.Loc.ScriptLocalization.COMPLETE_EXPORT);
            taskPanel.SetGiphyActive(false);
            taskPanel.SetEMActive(true);
            if (this.convertedScreenshot == null)
                this.convertedScreenshot = MultiplayerShareBgController.RenderTextureToTexture2dConvert(this.screenshotRT);
            CoroutineHelper.Instance.DoActionDelay(() =>
            {
                taskPanel.Hide();
                EasyMobile.Sharing.ShareTexture2D(
                this.convertedScreenshot,
                string.Format("takuzu-screenshot-{0}", System.DateTime.UtcNow.Millisecond),
                AppInfo.Instance.DEFAULT_SHARE_MSG);
            }, 0.5f);
        }, () => (t > 3 || countDownForScreeenShotIsDone == true));
    }

    public void ShareNewGif()
    {
        taskPanel.Show();
        bool completed = false;
        if (string.IsNullOrEmpty(gifPath))
        {
            Gif.ExportGif(
                clip,
                string.Format("takuzu-gif-{0}", System.DateTime.UtcNow.Millisecond),
                0,
                80,
                System.Threading.ThreadPriority.Normal,
                (c, progress) =>
                {
                    taskPanel.SetProgress(progress);
                    taskPanel.SetTask(I2.Loc.ScriptLocalization.EXPORTING_GIF);
                    taskPanel.SetGiphyActive(false);
                    taskPanel.SetEMActive(true);
                },
                (c, path) =>
                {
                    gifPath = path;
                    completed = true;
                }
            );
        }
        else
        {
            completed = true;
        }

        CoroutineHelper.Instance.PostponeActionUntil(() =>
        {

            if (string.IsNullOrEmpty(gifPath))
            {
                taskPanel.SetTask(I2.Loc.ScriptLocalization.EXPORTING_GIF_FAIL);
                CoroutineHelper.Instance.DoActionDelay(() =>
                {
                    taskPanel.SetTask(string.Empty);
                    taskPanel.Hide();
                }, 3);
                return;
            }
            else
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    taskPanel.SetTask(I2.Loc.ScriptLocalization.NO_INTERNET_FOR_SHARE);
                    taskPanel.SetProgress(0);
                    CoroutineHelper.Instance.DoActionDelay(() =>
                    {
                        taskPanel.SetTask(string.Empty);
                        taskPanel.Hide();
                    }, 3);
                    return;
                }

                GiphyUploadParams u = new GiphyUploadParams();
                u.localImagePath = gifPath;
                u.tags = "takuzu";
                taskPanel.SetTask(I2.Loc.ScriptLocalization.UPLOADING_GIF);
                taskPanel.SetGiphyActive(true);
                taskPanel.SetEMActive(false);
                Giphy.Upload(
                    SocialManager.giphyChannel,
                    SocialManager.giphyApiKey,
                    u,
                    (progress) =>
                    {
                        taskPanel.SetProgress(progress);
                    },
                    (url) =>
                    {
                        gifUrl = url;
                        taskPanel.SetTask(I2.Loc.ScriptLocalization.COMPLETE_EXPORT);
                        CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            taskPanel.SetTask(string.Empty);
                            taskPanel.Hide();
                            EasyMobile.Sharing.ShareURL(url, AppInfo.Instance.DEFAULT_SHARE_MSG);
                        }, 3);
                    },
                    (error) =>
                    {
                        taskPanel.SetTask(I2.Loc.ScriptLocalization.UPLOADING_GIF_FAIL);
                        Debug.Log("Upload Gif failed: " + error);
                        CoroutineHelper.Instance.DoActionDelay(() =>
                        {
                            taskPanel.SetTask(string.Empty);
                            taskPanel.Hide();
                        }, 3);
                    });
            }
        },
            () =>
            {
                return completed;
            });
    }

    public void DestroyScreenshot()
    {
        if (boardScreenshot.texture != null && (PuzzleManager.currentIsChallenge || PuzzleManager.currentIsMultiMode))
        {
            Destroy(boardScreenshot.texture);
        }
        if (clip != null && !clip.IsDisposed())
        {
            clip.Dispose();
            clip = null;
        }
        if (clipPlayerRaw.texture != null)
        {
            Destroy(clipPlayerRaw.texture);
            clipPlayerRaw.texture = null;
        }
        gifUrl = null;
        gifPath = null;
    }

    public void StartRecordingGif()
    {
        bool autoHeight = recorder.AutoHeight;
        int width = recorder.Width;
        int height = recorder.Height;
        int fps = recorder.FramePerSecond;
        float length = recorder.Length;
        recorder.Setup(autoHeight, width, height, fps, length);
        Gif.StartRecording(recorder);
    }

    public void StopRecordingGif()
    {
        clip = Gif.StopRecording(recorder);
    }

    private void OnResolutionChanged(Vector2 res)
    {
        if (recorder.IsRecording())
        {
            clip.Dispose();
        }
    }

    public void ShowRibbonIfNot()
    {
        if (ribbonHidden)
        {
            ribbonAnim.Play(AnimConstant.IN);
            ribbonHidden = false;
        }
    }

    public void HideRibbonIfNot()
    {
        if (!ribbonHidden)
        {
            ribbonAnim.Play(AnimConstant.OUT);
            ribbonHidden = true;
        }
    }
}
