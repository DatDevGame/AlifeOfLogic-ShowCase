using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    public class PersonalizeManager : MonoBehaviour
    {
        public static PersonalizeManager Instance { get; private set; }

        public static System.Action<bool> onNightModeChanged = delegate
        {
        };
        public static System.Action<bool> onColorBlindFriendlyModeChanged = delegate
        {
        };
        public static System.Action<bool> onVibrateChanged = delegate { };

        public const string NIGHT_MODE_KEY = "NIGHT_MODE";
        public const string COLOR_BLIND_FRIENDLY_KEY = "COLOR_BLIND_FRIENDLY";
        public const string VIBRATE_KEY = "VIBRATE";

        private static bool? nightModeEnable = null;
        public static bool NightModeEnable
        {
            get
            {
                bool defaultValue = false; //Change default value here
                if (!nightModeEnable.HasValue)
                    nightModeEnable = PlayerDb.GetBool(NIGHT_MODE_KEY, defaultValue);
                return nightModeEnable.Value;
            }
            set
            {
                bool? oldValue = nightModeEnable;
                bool newValue = value;
                nightModeEnable = newValue;
                PlayerDb.SetBool(NIGHT_MODE_KEY, newValue);
                if (oldValue != newValue)
                    onNightModeChanged(nightModeEnable.Value);
            }
        }

        private static bool? colorBlindFriendlyModeEnable = null;
        public static bool ColorBlindFriendlyModeEnable
        {
            get
            {
                bool defaultValue = false; //change default value here
                if (!colorBlindFriendlyModeEnable.HasValue)
                    colorBlindFriendlyModeEnable = PlayerDb.GetBool(COLOR_BLIND_FRIENDLY_KEY, defaultValue);
                return colorBlindFriendlyModeEnable.Value;
            }
            set
            {
                bool? oldValue = colorBlindFriendlyModeEnable;
                bool newValue = value;
                colorBlindFriendlyModeEnable = newValue;
                PlayerDb.SetBool(COLOR_BLIND_FRIENDLY_KEY, newValue);
                if (oldValue != newValue)
                    onColorBlindFriendlyModeChanged(colorBlindFriendlyModeEnable.Value);
            }
        }

        private static bool? vibrateEnable = null;
        public static bool VibrateEnable
        {
            get
            {
                bool defaultValue = false; //change default value here
                if (!vibrateEnable.HasValue)
                    vibrateEnable = PlayerDb.GetBool(VIBRATE_KEY, defaultValue);
                return vibrateEnable.Value;
            }
            set
            {
                bool? oldValue = vibrateEnable;
                bool newValue = value;
                vibrateEnable = newValue;
                PlayerDb.SetBool(VIBRATE_KEY, newValue);
                if (oldValue != newValue)
                    onVibrateChanged(vibrateEnable.Value);
            }
        }

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

        private void OnEnable()
        {
            PlayerDb.Resetted += OnPlayerDbResetted;
        }

        private void OnDisable()
        {
            PlayerDb.Resetted -= OnPlayerDbResetted;
        }

        private void OnPlayerDbResetted()
        {
            PlayerDb.SetBool(NIGHT_MODE_KEY, NightModeEnable);
            PlayerDb.SetBool(COLOR_BLIND_FRIENDLY_KEY, ColorBlindFriendlyModeEnable);
        }
    }
}