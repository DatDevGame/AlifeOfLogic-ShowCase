using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSparks.Api.Responses;
using Pinwheel;
using UnityEngine.Events;
using System;

namespace Takuzu
{
    public class SkinShopEntry : MonoBehaviour
    {
        public Text skinNameText;
        public RawImage previewBoardRawImage;
        public Button skinButton;
        public Image SkinButtonBgImage;
        public Image WaveImage;
        public Image subWaveImage;
        public Text orderText;
        public Color purchasedColor;
        public Color lockedColor;
        public Color activatedColor;
        public GameObject selectedTag;
        public GameObject priceTag;
        public Text priceText;

        private SkinEntryData currentData;
        public SkinEntryData CurrentData {get { return this.currentData; } }

        public class SkinEntryData
        {
            public SkinScriptableObject scriptableObject;
            public GameObject previewBoard;
            public Texture renderTexture = null;
            public GameObject previewBoardTemplate = null;
            public string samplePuzzle = "";
            public string sampleSolution = "";
            public int index;
            public Vector3 instantiatePos = new Vector3();
        }

        public void UpdateUI(SkinEntryData data)
        {
            currentData = data;
            skinNameText.text = String.Format("{0}", data.scriptableObject.name);
            SkinButtonBgImage.color = lockedColor;
            if (data.scriptableObject.isFree || data.scriptableObject.purchased)
                SkinButtonBgImage.color = purchasedColor;
            if (SkinManager.GetActivatedSkin().name == data.scriptableObject.name)
                SkinButtonBgImage.color = activatedColor;

            WaveImage.color = SkinButtonBgImage.color;
            subWaveImage.color = Color.Lerp(SkinButtonBgImage.color, Color.white, 0.5f);
            orderText.text = (data.index + 1).ToString();
            priceText.text = currentData.scriptableObject.price.ToString();
            if (data.scriptableObject.isFree == false && data.scriptableObject.purchased == false)
                priceTag.SetActive(true);
            else
                priceTag.SetActive(false);

            if (SkinManager.GetActivatedSkin().name == data.scriptableObject.name)
                selectedTag.SetActive(true);
            else
                selectedTag.SetActive(false);
            skinButton.onClick.RemoveAllListeners();
            skinButton.onClick.AddListener(() =>
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySound(SoundManager.Instance.button);
                if (currentData.scriptableObject.isFree == false && currentData.scriptableObject.purchased == false)
                {
                    SkinManager.PurchasingSkin(SkinManager.GetSkinIndexFromName(currentData.scriptableObject.name));
                }
                else
                {
                    SkinManager.SetActivatedSkinIndex(SkinManager.GetSkinIndexFromName(currentData.scriptableObject.name));
                }
            });

            if(currentData.previewBoard == null)
            {
                GameObject go = Instantiate(currentData.previewBoardTemplate,currentData.instantiatePos , Quaternion.identity);
                BoardVisualizer boardVisualizer = go.GetComponent<BoardVisualizer>();
                boardVisualizer.skinSO = currentData.scriptableObject;
                BoardLogical boardLogical = go.GetComponent<BoardLogical>();
                boardLogical.InitPuzzle(currentData.samplePuzzle,currentData.sampleSolution, true);
                currentData.previewBoard = go;
            }
            previewBoardRawImage.texture = data.renderTexture;
            if (currentData.renderTexture == null)
            {
                StartCoroutine(SetPreviewTexture());
            }
        }

        public void ClearRenderTexture()
        {
            if(this.currentData == null)
                return;
            if(this.currentData.renderTexture != null)
                Destroy(this.currentData.renderTexture);
        }

        private IEnumerator SetPreviewTexture()
        {
            yield return null;
            currentData.previewBoard.GetComponent<BoardVisualizer>().container.gameObject.SetActive(true);
            currentData.renderTexture = currentData.previewBoard.GetComponent<BoardInstanceCameraController>().GetBoardTexture(1, new Color());
            currentData.previewBoard.GetComponent<BoardVisualizer>().container.gameObject.SetActive(false);
        }
    }
}