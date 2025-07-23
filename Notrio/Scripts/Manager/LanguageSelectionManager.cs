using System;
using System.Collections;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LanguageSelectionManager : MonoBehaviour {

    private static string LANGUAGE_SELECTION_SAVE_KEY = "LANGUAGE_SELECTION";

    public static event System.Action<string> SwitchLanguageManager;

    public LanguageSettingOverlayUI languageSettingOverlay;

    private void Awake()
    {
        LanguageSettingOverlayUI.SelectedLanguage += OnSelectedLanguage;

        if (PlayerPrefs.HasKey(LANGUAGE_SELECTION_SAVE_KEY))
        {
            GoToNextScene();
            return;
        }

        PlayerPrefs.SetInt(LANGUAGE_SELECTION_SAVE_KEY, 1);
        StartCoroutine(CR_DelayShow());
    }

    IEnumerator CR_DelayShow()
    {
        yield return new WaitForSeconds(0.1f);
        languageSettingOverlay.Show();
    }

    private void OnDestroy()
    {
        LanguageSettingOverlayUI.SelectedLanguage -= OnSelectedLanguage;
    }

    private void OnSelectedLanguage(string code)
    {
        GoToNextScene();
    }

    private void Start()
    {
        LanguageSettingOverlayUI.selectedLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
    }

    private void GoToNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
