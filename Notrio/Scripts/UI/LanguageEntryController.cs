using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class LanguageEntryController : MonoBehaviour
    {
        [Header("Reference Objects")]
        public Text LanguageNameTxt;
        public Button languageBtn;
        public Image backGround;
        public GameObject line;

        [Header("Config")]
        public Color normalTxtColor;
        public Color selectedTxtColor;
        public Color normalBGColor;
        public Color selectedBGColor;

        public string LanguageCode { get; private set; }

        private void Awake()
        {
            LanguageSettingOverlayUI.SwitchLanguage += OnSwitchLanguage;
        }

        private void OnDestroy()
        {
            LanguageSettingOverlayUI.SwitchLanguage -= OnSwitchLanguage;
        }

        public void UpdateUI(string languageCode, Action<string> onClick)
        {
            LanguageCode = languageCode;
            LanguageNameTxt.text = UpperFirstChar(new CultureInfo(LanguageCode).NativeName);
            if (languageBtn != null)
            {
                languageBtn.onClick.AddListener(() =>
                    {
                        onClick(LanguageCode);
                    });
            }

            CheckCurrentLanguage(I2.Loc.LocalizationManager.CurrentLanguageCode);
        }

        public void CheckCurrentLanguage(string currentCode)
        {
            if (LanguageCode.Equals(currentCode))
            {
                LanguageNameTxt.color = selectedTxtColor;
                backGround.color = selectedBGColor;
                line.SetActive(false);
            }
            else
            {
                LanguageNameTxt.color = normalTxtColor;
                backGround.color = normalBGColor;
                line.SetActive(true);
            }
        }

        void OnSwitchLanguage(string code)
        {
            CheckCurrentLanguage(code);
        }

        public string UpperFirstChar(string str)
        {
            return str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1);
        }
    }
}
