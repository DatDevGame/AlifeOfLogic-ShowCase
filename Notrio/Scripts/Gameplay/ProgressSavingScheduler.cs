using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class ProgressSavingScheduler : MonoBehaviour
    {
        public delegate void TickHandler();
        public static event TickHandler Tick;

        public static ProgressSavingScheduler Instance { get; private set; }

        public float intervalSecond = 1f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }
        
        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                StartCoroutine(CrTickScheduled());
            }
            else
            {
                StopAllCoroutines();
            }
        }

        private IEnumerator CrTickScheduled()
        {
            yield return null;
            while (true)
            {
                yield return new WaitForSeconds(intervalSecond);
                if (Tick != null)
                    Tick();
            }
        }
    }
}