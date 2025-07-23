using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Takuzu;
using System;
using Debug = UnityEngine.Debug;

public class Timer : MonoBehaviour
{
    public const string ELAPSED_PREFIX = "ELAPSED-";

    private Stopwatch timer;
    private TimeSpan lastElapsed;
    private Action saveElapsedAction;
    private Action resetElapsedAction;

    private bool isFocus = true;

    public TimeSpan Elapsed
    {
        get
        {
            return lastElapsed + timer.Elapsed;
        }
    }

    private void OnEnable()
    {
        GameManager.GameStateChanged += OnGameStateChanged;
        PuzzleManager.onPuzzleSelected += OnPuzzleSelected;
        Judger.onJudgingCompleted += OnJudgingCompleted;
        ProgressSavingScheduler.Tick += OnSchedulerTick;
    }

    private void OnDisable()
    {
        GameManager.GameStateChanged -= OnGameStateChanged;
        PuzzleManager.onPuzzleSelected -= OnPuzzleSelected;
        Judger.onJudgingCompleted -= OnJudgingCompleted;
        ProgressSavingScheduler.Tick -= OnSchedulerTick;
    }

    private void Awake()
    {
        timer = new Stopwatch();
    }

    public void Start()
    {
        //timer.Start();
    }

    public void Reset()
    {
        timer.Reset();
    }

    private void OnGameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {
            timer.Start();
        }
        if (newState == GameState.GameOver)
        {
            timer.Stop();
        }
        if (newState == GameState.Prepare && oldState == GameState.Paused)
        {
            saveElapsedAction();
        }
    }

    private void OnPuzzleSelected(string id, string puzzle, string solution, string progress)
    {
        timer.Reset();
        LoadLastElapsed(id);
        saveElapsedAction = delegate
        {
            SaveElapsed(id);
        };
        resetElapsedAction = delegate
        {
            ResetElapsed(id);
        };
    }

    private void OnJudgingCompleted(Judger.JudgingResult result)
    {
        resetElapsedAction();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (GameManager.Instance.GameState == GameState.Playing || GameManager.Instance.GameState == GameState.Paused)
        {
            if (focus)
            {
                isFocus = true;
                LoadLastElapsed(PuzzleManager.currentPuzzleId);
                timer.Reset();
                timer.Start();
            }
            else
            {
                SaveElapsed(PuzzleManager.currentPuzzleId);
                timer.Stop();
                isFocus = false;
            }
        }
    }

    private void OnSchedulerTick()
    {
        SaveElapsed(PuzzleManager.currentPuzzleId);
    }

    public void LoadLastElapsed(string puzzleId)
    {
        string key = ELAPSED_PREFIX + puzzleId;
        int totalSeconds = PlayerDb.GetInt(key, 0);
        lastElapsed = new TimeSpan(0, 0, totalSeconds);
    }

    public void SaveElapsed(string id)
    {
        if(!isFocus)
            return;
        string key = ELAPSED_PREFIX + id;
        PlayerDb.SetInt(key, (int)Elapsed.TotalSeconds);
    }

    public void ResetElapsed(string id)
    {
        string key = ELAPSED_PREFIX + id;
        PlayerDb.DeleteKey(key);
    }

    public string GetTimeString()
    {
        string s = string.Format("{0:D2}:{1:D2}:{2:D2}", Elapsed.Hours, Elapsed.Minutes, Elapsed.Seconds);
        return s;
    }
}
