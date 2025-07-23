using UnityEngine;
using System.Collections;
using EasyMobile;
using DG.Tweening;

namespace Takuzu
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public Camera camMain;
        public Camera camSecondary;
        public Camera boardCam;
        public float minSize;
        public float maxSize;
        public float zoomSpeed;
        public float movementSpeed;
        [Range(0f, 1f)]
        public float pinchThreshold;
        public Recorder recorder;
        public ParticleSystem winParticleBotCenter;

        private bool pinching;
        float lastPinchDistance;
        float pinchDistance;
        float pinchDelta;
        private float ratioScaleConfetti = 0.123f;


        private void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            LogicalBoard.onPuzzleInitialized += OnPuzzleInitialized;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            ScreenManager.onScreenResolutionChanged += OnResolutionChanged;
            MultiplayerSession.SessionFinished += OnSessionFinished;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            LogicalBoard.onPuzzleInitialized -= OnPuzzleInitialized;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            ScreenManager.onScreenResolutionChanged -= OnResolutionChanged;
            MultiplayerSession.SessionFinished -= OnSessionFinished;
        }

        private void Awake()
        {
            camMain.clearFlags = CameraClearFlags.SolidColor;
            camMain.cullingMask = ~0;
            camSecondary.enabled = false;
            boardCam.aspect = 1;
            boardCam.enabled = false;
            SetUpParticle();
        }

        //public void Update()
        //{
        //    if (GameManager.Instance.GameState != GameState.Playing)
        //        return;
        //#if UNITY_EDITOR
        //            ZoomInEditor();
        //#elif UNITY_ANDROID || UNITY_IOS
        //            ZoomInPlayer();
        //#endif
        //}

        private void ZoomInEditor()
        {
            float scroll = Input.mouseScrollDelta.y;
            float f = 10 * Time.smoothDeltaTime;
            float targetSize =
                scroll < 0 ? maxSize :
                scroll > 0 ? minSize :
                camMain.orthographicSize;
            camMain.orthographicSize = Mathf.MoveTowards(camMain.orthographicSize, targetSize, f * zoomSpeed);
        }

        private void ZoomInPlayer()
        {
            if (Input.touchCount == 2)
            {
                if (!pinching)
                {
                    pinching = true;
                    lastPinchDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
                }
                else
                {
                    pinchDistance = (Input.touches[0].position - Input.touches[1].position).magnitude;
                    pinchDelta = pinchDistance - lastPinchDistance;
                    float f = Mathf.Abs(pinchDelta) / (pinchThreshold * Screen.height);
                    float targetSize =
                        pinchDelta < 0 ? maxSize :
                        pinchDelta > 0 ? minSize :
                        camMain.orthographicSize;
                    camMain.orthographicSize = Mathf.MoveTowards(camMain.orthographicSize, targetSize, f * zoomSpeed);

                    lastPinchDistance = pinchDistance;
                }
            }
            else
            {
                pinching = false;
            }
        }

        private void OnPuzzleInitialized(int[][] p)
        {
            minSize = p.Length + 2f;
            float camMainWidth = (p.Length * 1.0f) / 2 + 1.5f;
            float camMainHeight = camMainWidth / camMain.aspect;

            if (!LogicalBoard.Instance.renderTexturePurpose)
                camMain.orthographicSize = Mathf.Max(minSize, camMainHeight);

            boardCam.orthographicSize = (p.Length * 1.0f) / 2 + 1.5f;

            Vector3 endPos = new Vector3(
                p.Length / 2f - 0.5f,
                p[0].Length / 2f - 0.5f,
                transform.position.z);

            float w = boardCam.aspect * boardCam.orthographicSize;
            Vector3 startPos = new Vector3(-w, endPos.y, endPos.z);

            camMain.transform.position = startPos;

            if (!LogicalBoard.Instance.playRevealAnim)
            {
                camMain.transform.position = endPos;
                return;
            }

            camMain.transform.DOMove(endPos, 1.2f)
                .SetEase(Ease.InOutQuad);
        }


        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            SetUpParticle();
            ResetCameraSize();
        }

        public void SetUpParticle()
        {
            float bias = Camera.main.orthographicSize * ratioScaleConfetti;
            winParticleBotCenter.transform.localScale = Vector3.one * bias;

            for (int i = 0; i < winParticleBotCenter.transform.childCount; i++)
                winParticleBotCenter.transform.GetChild(i).localScale = Vector3.one * bias;

            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector2((float)Screen.width / 4, 0));
            pos = Camera.main.ScreenToWorldPoint(new Vector2((float)Screen.width / 2, 0));
            winParticleBotCenter.transform.position = new Vector3(pos.x, pos.y - bias - 1, 0);
        }

        public void ResetCameraSize()
        {
            if (GameManager.Instance.GameState == GameState.Prepare)
            {
                camSecondary.enabled = false;
                camMain.orthographicSize = maxSize;
                camMain.cullingMask = ~0;
                camMain.clearFlags = CameraClearFlags.SolidColor;
                boardCam.enabled = false;
            }
            else if (GameManager.Instance.GameState == GameState.Playing)
            {
                camSecondary.enabled = true;
                camSecondary.cullingMask = LayerMask.GetMask("Background");
                camMain.cullingMask = ~camSecondary.cullingMask;
                camMain.clearFlags = CameraClearFlags.Depth;
                boardCam.enabled = false;
            }
        }

        public void EnableBoardCam(bool enable)
        {
            boardCam.enabled = enable;
        }

        public Texture2D GetBoardTexture(float aspect, Color backgroundColor)
        {
            boardCam.aspect = aspect;
            boardCam.backgroundColor = backgroundColor;
            boardCam.clearFlags = CameraClearFlags.Color;
            RenderTexture rt = new RenderTexture(1000, (int)(1000 / aspect), 24);
            boardCam.targetTexture = rt;
            boardCam.Render();
            Texture2D t = new Texture2D(boardCam.targetTexture.width, boardCam.targetTexture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = boardCam.targetTexture;
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();
            RenderTexture.active = null;
            rt.Release();
            Destroy(rt);
            return t;
        }

        private void OnSessionFinished(bool isWin)
        {
            if (PuzzleManager.currentIsMultiMode && isWin)
            {
                SoundManager.Instance.PlaySoundDelay(0.5f, SoundManager.Instance.confetti, true);
                PlayLeavesParticle(3);
            }
        }

        private void OnPuzzleSolved()
        {
            if (!PuzzleManager.currentIsMultiMode)
            {
                SoundManager.Instance.PlaySoundDelay(0.5f, SoundManager.Instance.confetti, true);
                PlayLeavesParticle(3);
            }
        }

        public void PlayLeavesParticle(float duration)
        {
            StartCoroutine(CR_PlayLeavesParticle(duration));
        }

        IEnumerator CR_PlayLeavesParticle(float duration)
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(CR_DelayRunConfetti());
        }

        IEnumerator CR_DelayRunConfetti()
        {
            yield return new WaitForSeconds(0.5f);
            winParticleBotCenter.Play();
            yield return new WaitForSeconds(4.05f);
            winParticleBotCenter.Stop();
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

        public AnimatedClip StopRecordingGif()
        {
            return Gif.StopRecording(recorder);
        }

        private void OnResolutionChanged(Vector2 res)
        {
            if (recorder.IsRecording())
            {
                StopRecordingGif().Dispose();
            }
        }
    }
}
