using System;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Takuzu;

public class LanguageSelectButton : MonoBehaviour {
    public Button button;
    public Text languageCodeText;
    public Text languageNativeNameText;
    private string languageCode;
    private Color usingBtnColor;
    private Color unuseBtnColor;
    private Color usingTextColor;
    private Color unuseTextColor;

    private void OnEnable()
    {
        LanguageSettingOverlayUI.SwitchLanguage += OnSwitchLanguage;
        LanguageSelectionManager.SwitchLanguageManager += OnSwitchLanguage;
    }

    private void OnDisable()
    {
        LanguageSettingOverlayUI.SwitchLanguage -= OnSwitchLanguage;
        LanguageSelectionManager.SwitchLanguageManager -= OnSwitchLanguage;
    }


    void OnSwitchLanguage(string selectedCode)
    {
        if(selectedCode.Equals(languageCode))
        {
            button.image.color = usingBtnColor;
            languageNativeNameText.color = usingTextColor;
        }
        else
        {
            button.image.color = unuseBtnColor;
            languageNativeNameText.color = unuseTextColor;
        }
    }

    public void UpdateReference(string languageCode, Action<string> onClick,Color useBtnColor,Color unuseBtnColor, Color useTxtColor, Color unuseTxtColor)
    {
        this.languageCode = languageCode;
        this.usingBtnColor = useBtnColor;
        this.unuseBtnColor = unuseBtnColor;
        this.usingTextColor = useTxtColor;
        this.unuseTextColor = unuseTxtColor;
        if (I2.Loc.LocalizationManager.CurrentLanguageCode == this.languageCode)
        {
            button.image.color = useBtnColor;
            languageNativeNameText.color = useBtnColor;
        }
        else
        {
            button.image.color = unuseBtnColor;
            languageNativeNameText.color = unuseTxtColor;
        }
        if (button)
            button.onClick.AddListener(() =>
            {
                onClick(languageCode);
            });
        if (languageCodeText)
            languageCodeText.text = (new CultureInfo(this.languageCode).TwoLetterISOLanguageName.ToUpper());
        if (languageNativeNameText)
            languageNativeNameText.text = (new CultureInfo(this.languageCode).NativeName.ToUpper());
    }

    public void UpdateReference(string languageCode, Action<string> onClick)
    {
        this.languageCode = languageCode;
        if (button)
            button.onClick.AddListener(() =>
            {
                onClick(languageCode);
            });
        if (languageCodeText)
            languageCodeText.text = (new CultureInfo(this.languageCode).TwoLetterISOLanguageName.ToUpper());
        if (languageNativeNameText)
            languageNativeNameText.text = (new CultureInfo(this.languageCode).NativeName.ToUpper());
    }
}
