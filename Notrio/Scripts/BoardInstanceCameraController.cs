using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class BoardInstanceCameraController : MonoBehaviour
    {

        public Camera boardCam;
        public BoardLogical boardLogical;
        private void Awake()
        {
            boardCam.enabled = false;
        }
        // Use this for initialization
        void Start()
        {
            if (boardLogical.HasPuzzle)
            {
                OnPuzzleInitialized(boardLogical.puzzle);
            }
            boardLogical.onPuzzleInitialized += OnPuzzleInitialized;
        }

        private void OnDestroy()
        {
            boardLogical.onPuzzleInitialized -= OnPuzzleInitialized;
        }

        private void OnPuzzleInitialized(int[][] p)
        {
            boardCam.orthographicSize = (p.Length * 1.0f) / 2 + 1.5f;
            Vector3 endPos = new Vector3(
                p.Length / 2 - 0.5f,
                p[0].Length / 2 - 0.5f,
                boardCam.transform.localPosition.z);
            boardCam.transform.localPosition = endPos;
            if (boardCam.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture(Screen.width, (int)(Screen.width / boardCam.aspect), 24);
                boardCam.targetTexture = rt;
            }
        }

        public Texture GetBoardTexture(float aspect, Color backgroundColor)
        {
            boardCam.aspect = aspect;
            boardCam.backgroundColor = backgroundColor;
            boardCam.clearFlags = CameraClearFlags.Color;
            if (boardCam.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture(Screen.width, (int)(Screen.width / aspect), 24);
                boardCam.targetTexture = rt;
            }
            boardCam.Render();
            
            return boardCam.targetTexture;
        }
    }
}
