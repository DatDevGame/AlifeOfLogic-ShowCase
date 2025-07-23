using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeSelectionTab
{
    public struct Config
    {
        public Color bgActiveColor;
        public Color bgInactiveColor;
        public Color txtActiveColor;
        public Color txtInactiveColor;
    }

    [Serializable]
    public class TabButton
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Text text;
        private int index;

        public TabButton(){}

        public TabButton(Button button, Image background, Text text)
        {
            this.button = button;
            this.background = background;
            this.text = text;
        }

        public Image Background
        {
            get
            {
                return background;
            }
        }

        public Text Text
        {
            get
            {
                return text;
            }
        }

        public void AddListenter(int index, Action<int> listener)
        {
            this.index = index;

            this.button.onClick.RemoveAllListeners();
            this.button.onClick.AddListener(() =>
            {
                listener(this.index);
            });
        }
    }

    private Config config;
    private int selectedTab = 0;
    private List<TabButton> tabButtons;
    private Action<int> onTabChanged;

    public SizeSelectionTab(Config config, int selectedTab, List<TabButton> tabButtons, Action<int> onTabChanged)
    {
        this.selectedTab = selectedTab;
        this.tabButtons = tabButtons;
        this.onTabChanged = onTabChanged;
        this.config = config;

        SettupEventListener();
        SelectSizeTab(selectedTab);
    }

    private void SettupEventListener()
    {
        for (int i = 0; i < this.tabButtons.Count; i++)
        {
            this.tabButtons[i].AddListenter(i, (index) => SelectSizeTab(index));
        }
    }

    public void SelectSizeTab(int index)
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            tabButtons[i].Background.color = this.config.bgInactiveColor;
            tabButtons[i].Text.color = this.config.txtInactiveColor;
        }
        tabButtons[index].Background.color = this.config.bgActiveColor;
        tabButtons[index].Text.color = this.config.txtActiveColor;

        selectedTab = index;
        
        this.onTabChanged(this.selectedTab);
    }
}