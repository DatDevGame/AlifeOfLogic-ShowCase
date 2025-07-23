using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Takuzu
{
    public class LanguageSettingOverlayUI : OverlayPanel
    {
        public static LanguageSettingOverlayUI Instance { get; private set; }
        public static string selectedLanguageCode;

        public static event System.Action<string> SwitchLanguage = delegate { };
        public static event Action<string> SelectedLanguage = delegate { };

        public UiGroupController controller;
        public RectTransform rectContent;
        public Button confirmBtn;
        public Button closeBtn;
        public Scrollbar verticalScrollBar;
        public GameObject languageEntry;

        public bool isReloadScene = true;

        private List<LanguageEntryController> languageEntryList = new List<LanguageEntryController>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            closeBtn.onClick.AddListener(() =>
            {
                Hide();
            });
            confirmBtn.onClick.AddListener(() =>
            {
                ChangeCurrentLanguage();
            });
            InitLanguageBtn();
            Invoke("MoveToCurrentLanguage", 0.07f);
        }

        void InitLanguageBtn()
        {
            List<string> languageCodelist = I2.Loc.LocalizationManager.GetAllLanguagesCode();
            for (int i = 0; i < languageCodelist.Count; i++)
            {
                GameObject entry = Instantiate(languageEntry, rectContent);
                LanguageEntryController entryController = entry.GetComponent<LanguageEntryController>();
                languageEntryList.Add(entryController);
                entryController.UpdateUI(languageCodelist[i], (cd) =>
                {
                    selectedLanguageCode = cd;
                    SwitchLanguage(selectedLanguageCode);
                });
            }
        }

        public override void Hide()
        {
            controller.HideIfNot();
            IsShowing = false;
            onPanelStateChanged(this, false);
        }

        public override void Show()
        {
            selectedLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
            CheckStateAllEntries();
            Invoke("MoveToCurrentLanguage", 0.07f);
            controller.ShowIfNot();
            IsShowing = true;
            transform.BringToFront();
            onPanelStateChanged(this, true);
        }

        void CheckStateAllEntries()
        {
            foreach (var entry in languageEntryList)
            {
                entry.CheckCurrentLanguage(I2.Loc.LocalizationManager.CurrentLanguageCode);
            }
        }

        public void SwitchSelectedCodeLanuage()
        {
            SwitchLanguage(selectedLanguageCode);
        }

        private void ChangeCurrentLanguage()
        {
            I2.Loc.LocalizationManager.CurrentLanguageCode = selectedLanguageCode;
            if (isReloadScene)
                SceneLoadingManager.Instance.Reload();
            SelectedLanguage(I2.Loc.LocalizationManager.CurrentLanguageCode);
        }

        private void MoveToCurrentLanguage()
        {
            string curCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
            for (int i = 0; i < languageEntryList.Count; i++)
            {
                if (languageEntryList[i].LanguageCode.Equals(curCode))
                {
                    verticalScrollBar.value = Mathf.Clamp(1 - ((i) / (float)(languageEntryList.Count - 1)), 0, 1);
                }
            }
        }
    }
}
