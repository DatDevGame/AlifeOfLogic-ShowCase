using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Takuzu.Generator;
using System;

namespace Takuzu
{
    public class PackSelectionUI : MonoBehaviour
    {
        public UiGroupController controller;
        public SnappingScroller scroller;
        public GameObject packSelectorTemplate;
		[HideInInspector]
        public ConfirmationDialog dialog;
		[HideInInspector]
        public CoinShopUI coinShop;
		[HideInInspector]
        public LevelSelectorPanelController levelPanel;
        public PackSelector[] selector;
		private void Awake() {
			if(UIReferences.Instance!=null){
				UpdateReferences();
			}
			UIReferences.UiReferencesUpdated += UpdateReferences;
		}

		private void UpdateReferences()
		{
			dialog = UIReferences.Instance.overlayConfirmDialog;
			coinShop = UIReferences.Instance.overlayCoinShopUI;
			levelPanel = UIReferences.Instance.overlayLevelSelectorPanelController;
		}

		public const string LAST_PACK_INDEX_KEY = "LAST_PACK_INDEX";

        private void OnEnable()
        {
            PuzzleManager.onPackSelected += OnPackSelected;
            scroller.onSnapIndexChanged += OnSnapIndexChanged;
        }

        private void OnDisable()
        {
            PuzzleManager.onPackSelected -= OnPackSelected;
            scroller.onSnapIndexChanged -= OnSnapIndexChanged;
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
			UIReferences.UiReferencesUpdated -= UpdateReferences;
        }

        private void OnSnapIndexChanged(int newIndex, int oldIndex)
        {
            PlayerDb.SetInt(LAST_PACK_INDEX_KEY, newIndex);
        }

        private void Start()
        {
            GameManager.GameStateChanged += OnGameStateChanged;

            for (int i = 0; i < Mathf.Min(PuzzleManager.Instance.packs.Count, selector.Length); ++i)
            {
                selector[i].SetIndex(i);
                selector[i].SetPack(PuzzleManager.Instance.packs[i]);
                selector[i].levelPanel = levelPanel;
            }
            int lastPackIndex = PlayerDb.GetInt(LAST_PACK_INDEX_KEY, 0);
            lastPackIndex = Mathf.Clamp(lastPackIndex, -1, scroller.ElementCount - 1);
            if (lastPackIndex != -1)
            {
                scroller.SnapIndex = lastPackIndex;
                scroller.SnapImmediately();
            }
                controller.ShowIfNot();
        }

        private void OnPackSelected(PuzzlePack pack)
        {
            levelPanel.SetPack(pack);
            levelPanel.DisplayPackWithDefaultPuzzleSizeIfCurrentPackChanged();
        }

        private void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing)
            {
                controller.HideIfNot();
            }
            else if (newState == GameState.Prepare)
            {
                controller.ShowIfNot();
            }
        }
    }
}