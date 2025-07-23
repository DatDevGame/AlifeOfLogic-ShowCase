using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public enum PowerupType
    {
        None, Flag, Reveal, Undo, Clear
    }

    public class Powerup : MonoBehaviour
    {
        public static Powerup Instance { get; private set; }
        public static Action<PowerupType, PowerupType> onPowerupChanged = delegate { };
        [SerializeField]
        private PowerupType current;
        private bool powerupIsEnable = true;
        public bool PowerupIsEnable {
            get {return powerupIsEnable;}
        }
        public PowerupType Current
        {
            get
            {
                return current;
            }
            set
            {
                if(PowerupIsEnable == false)
                    return;
                PowerupType old = current;
                current = value;
                if (current != old)
                {
                    onPowerupChanged(current, old);
                }
            }
        }
        public const string RevealPerGameKey = "RevealPerGameKey";
        public int CountRevealPerGame
        {
            get { return PlayerDb.GetInt(RevealPerGameKey + PuzzleManager.currentPuzzleId, 0); }
            set { PlayerDb.SetInt(RevealPerGameKey + PuzzleManager.currentPuzzleId, value); }
        }

        public const string UndoPerGameKey = "UndoPerGameKey";
        public int  CountUndoPerGame
        {
            get { return PlayerDb.GetInt(UndoPerGameKey + PuzzleManager.currentPuzzleId, 0); }
            set { PlayerDb.SetInt(UndoPerGameKey + PuzzleManager.currentPuzzleId, value); }
        }

        private Dictionary<string, PowerupType> powerupPref;

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance);
            Instance = this;

            powerupPref = new Dictionary<string, PowerupType>();
            string[] powerupName = Enum.GetNames(typeof(PowerupType));
            for (int i=0;i<powerupName.Length;++i)
            {
                powerupPref.Add(powerupName[i].ToLower(), (PowerupType)i);
            }

        }

        public void DisablePowerUp(){
            powerupIsEnable = false;
        }

        public void EnablepowerUp(){
            powerupIsEnable = true;
        }

        private void OnEnable()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
            LogicalBoard.onCellRevealed += OnCellRevealed;
            LogicalBoard.onPuzzleReseted += OnPuzzleReseted;
            LogicalBoard.onCancelReset += OnCancelReset;
            LogicalBoard.onCellUndone += OnCellUndo;
            LogicalBoard.onNoUndoAvailable += OnNoUndoAvailable;
        }

        private void OnDisable()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
            LogicalBoard.onCellRevealed -= OnCellRevealed;
            LogicalBoard.onPuzzleReseted -= OnPuzzleReseted;
            LogicalBoard.onCancelReset -= OnCancelReset;
            LogicalBoard.onCellUndone -= OnCellUndo;
            LogicalBoard.onNoUndoAvailable -= OnNoUndoAvailable;
        }

        public PowerupType NameToType(string name)
        {
            name = name.ToLower();
            if (powerupPref.ContainsKey(name))
                return powerupPref[name];
            else
                return PowerupType.None;
        }

        public void SetType(string name)
        {
            name = name.ToLower();
            Current = NameToType(name);
        }

        public void ToggleType(string name)
        {
            name = name.ToLower();
            PowerupType type = NameToType(name);
            if (Current == type)
                Current = PowerupType.None;
            else
                Current = type;
        }

        private void OnCellRevealed(Index2D i)
        {
            SetType("none");
        }

        private void OnPuzzleReseted()
        {
            SetType("none");
        }

        private void OnCancelReset()
        {
            SetType("none");
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Prepare)
            {
                SetType("none");
            }
        }

        private void OnCellUndo(Index2D i)
        {
            SetType("none");
        }

        private void OnNoUndoAvailable()
        {
            SetType("none");
        }
    }
}