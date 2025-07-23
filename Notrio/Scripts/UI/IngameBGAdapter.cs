using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Takuzu
{
    public class IngameBGAdapter : MonoBehaviour
    {
        [System.Serializable]
        public struct InGameBgName
        {
            public string dayName;
            public string nightName;
        }

        [System.Serializable]
        public struct InGameBgSprite
        {
            public Sprite daySprite;
            public Sprite nightSprite;
        }

        public Renderer targetRender;

        [HideInInspector]
        public Material ingameBlendSpriteMaterial;

        public float blendSpeed;
        [Header("Config")]
        public List<InGameBgName> inGameBgNames;

        [HideInInspector]
        public List<InGameBgSprite> ingameBgs;

        private Coroutine blendSpriteCoroutine;
        private bool isBlending = false;
        // Use this for initialization

        private void LoadInGameBackgrounds()
        {
            ingameBgs = new List<InGameBgSprite>();
            for (int i = 0; i < inGameBgNames.Count; ++i)
            {
                InGameBgSprite bg;
                bg.daySprite = Background.Get(inGameBgNames[i].dayName);
                bg.nightSprite = Background.Get(inGameBgNames[i].nightName);
                ingameBgs.Add(bg);
            }
        }

        private void UnloadInGameBackgrounds()
        {
            for (int i = 1; i < inGameBgNames.Count; ++i)
            {
                Background.Unload(inGameBgNames[i].dayName);
                Background.Unload(inGameBgNames[i].nightName);
            }
            ingameBgs = null;
        }

        void Awake()
        {
            PersonalizeManager.onNightModeChanged += OnNightModeChanged;
            GameManager.GameStateChanged += OnGameStateChanged;
            LoadInGameBackgrounds();

            ingameBlendSpriteMaterial = new Material(Shader.Find("SgLib/BlendedSprite"));
            ingameBlendSpriteMaterial.enableInstancing = true;
            targetRender.sharedMaterial = ingameBlendSpriteMaterial;
            ChangeInGameBackground();
        }

        private void OnEnable()
        {
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            targetRender.GetPropertyBlock(p);
            float d = PersonalizeManager.NightModeEnable ? 1 : 0;
            p.SetFloat("_BlendFraction", d);
            targetRender.SetPropertyBlock(p);
        }

        private void OnDestroy()
        {
            PersonalizeManager.onNightModeChanged -= OnNightModeChanged;
            GameManager.GameStateChanged -= OnGameStateChanged;
            UnloadInGameBackgrounds();
        }

        private void ChangeInGameBackground()
        {
            int index = (int)(PuzzleManager.currentLevel);
            if (index >= 5)
                index = 0;
            PropertyInfo targetSprite = targetRender.GetType().GetProperty("sprite");
            if (targetSprite != null && targetSprite.CanWrite)
            {
                Sprite s = ingameBgs[index].daySprite;
                targetSprite.SetValue(targetRender, s, null);
            }

            MaterialPropertyBlock p = new MaterialPropertyBlock();
            targetRender.GetPropertyBlock(p);
            p.SetTexture("_MainTex", ingameBgs[index].daySprite.texture);
            p.SetTexture("_SecondaryTex", ingameBgs[index].nightSprite.texture);
            targetRender.SetPropertyBlock(p);
        }

        void OnGameStateChanged(GameState newState, GameState oldState)
        {
            if (newState == GameState.Playing && oldState != GameState.Playing)
            {
                ChangeInGameBackground();
            }
            if (newState == GameState.Prepare && oldState != GameState.Prepare)
            {
                if (isBlending)
                {
                    if (blendSpriteCoroutine != null)
                        StopCoroutine(blendSpriteCoroutine);
                    BlendSprite();
                    isBlending = false;
                }
            }
        }

        private void OnNightModeChanged(bool enable)
        {
            Adapt();
        }

        void Adapt()
        {
            if (blendSpriteCoroutine != null)
                StopCoroutine(blendSpriteCoroutine);
            blendSpriteCoroutine = StartCoroutine(CrBlendSprite());
        }

        private void BlendSprite()
        {
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            targetRender.GetPropertyBlock(p);
            float d = PersonalizeManager.NightModeEnable ? 1 : 0;
            p.SetFloat("_BlendFraction", d);
            targetRender.SetPropertyBlock(p);
        }

        private IEnumerator CrBlendSprite()
        {
            //float time = 0;
            MaterialPropertyBlock p = new MaterialPropertyBlock();
            targetRender.GetPropertyBlock(p);
            float f = p.GetFloat("_BlendFraction");
            float d = PersonalizeManager.NightModeEnable ? 1 : 0;
            isBlending = true;
            if (blendSpeed <= 0)
            {
                f = d;
                p.SetFloat("_BlendFraction", f);
                targetRender.SetPropertyBlock(p);
            }
            while (f != d)
            {
                f = Mathf.MoveTowards(f, d, blendSpeed * Time.smoothDeltaTime);
                p.SetFloat("_BlendFraction", f);
                targetRender.SetPropertyBlock(p);
                yield return null;
            }
            isBlending = false;
        }
    }
}
