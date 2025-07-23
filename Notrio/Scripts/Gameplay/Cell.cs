using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;
using UnityEngine.SceneManagement;
using System;

namespace Takuzu
{
    /// <summary>
    /// Represent for a cell in the board.
    /// </summary>
    public class Cell : MonoBehaviour
    {
        public GameObject container;
        public SpriteRenderer backgroundSpriteRenderer;
        public SpriteRenderer zeroValueSpriteRenderer;
        public SpriteRenderer colorBlindZeroValueSpriteRenderer;
        public SpriteRenderer oneValueSpriteRenderer;
        public SpriteRenderer colorBlindOneValueSpriteRenderer;
        public SpriteRenderer lockSpriteRenderer;
        public SpriteRenderer errorSpriteRenderer;
        public SpriteRenderer shinySpriteRenderer;
        public SpriteRenderer highLighterRenderer;
        public SpriteMask spriteMask;
        private Color activeZeroColor;
        private Color activeOneColor;
        private Color activeBGColor;
        public float setInactiveColorDuration = 0.5f;
        //public ScaleAnimation backgroundAnim;
        public ColorAnimation zeroValueAnim;
        public ColorAnimation oneValueAnim;
        //public ColorAnimation hightlightAnim;
        //public ScaleAnimation flagAnim;

        public ColorAnimation errorAnim;
        public ColorAnimation lockAnim;
        public ColorAnimation shinyAnim;

        public const int IN = 0;
        public const int OUT = 1;
        public const int SOLVED = 2;

        public bool isHighlighting;
        public bool isFlag;
        public bool isSolved;
        private bool isUpdateColor = false;
        private bool isActive = true;
        private bool isActiveFlipAni;

        public ColorAnimation currentValue;
        public bool listenToSkinChangedEvent = false;
        private ColorController backgroundColorController;
        private Coroutine flipCoroutine;
        private Sprite flipedSprite;
        private bool supportNumber = true;

        private void Awake()
        {
            activeZeroColor = zeroValueSpriteRenderer.color;
            activeOneColor = oneValueSpriteRenderer.color;
            PersonalizeManager.onColorBlindFriendlyModeChanged += OnColorBlindModeChanged;
            LogicalBoard.onPuzzleSolved += OnPuzzleSolved;
            LogicalBoard.onPuzzleReseted += OnPuzzleReseted;

            container.gameObject.SetActive(false);

            if (!SceneManager.GetActiveScene().name.Equals("Tutorial"))
                isUpdateColor = true;
            spriteMask.enabled = false;
            oneValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            zeroValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            SkinManager.ActivatedSkinChanged += SetSkin;
            SkinManager.TemporarySkinChanged += SetSkin;
        }

        private void OnDestroy()
        {
            PersonalizeManager.onColorBlindFriendlyModeChanged -= OnColorBlindModeChanged;
            LogicalBoard.onPuzzleSolved -= OnPuzzleSolved;
            LogicalBoard.onPuzzleReseted -= OnPuzzleReseted;
            if (backgroundColorController != null)
                backgroundColorController.onColorChanged -= OnColorChanged;
            SkinManager.ActivatedSkinChanged -= SetSkin;
            SkinManager.TemporarySkinChanged -= SetSkin;
        }

        private void OnPuzzleSolved()
        {
            //go away from my gif!
            errorSpriteRenderer.gameObject.SetActive(false);
        }

        private void OnPuzzleReseted()
        {
            UnhighlightImmedately();
        }

        public void SetValue(int value)
        {
            if (currentValue != null)
                //currentValue.Play(OUT);
                currentValue.gameObject.SetActive(false);

            if (value == LogicalBoard.VALUE_ZERO)
            {
                currentValue = zeroValueAnim;
            }
            else if (value == LogicalBoard.VALUE_ONE)
            {
                currentValue = oneValueAnim;
            }
            else if (value == LogicalBoard.VALUE_EMPTY)
            {
                currentValue = null;
            }
            if (currentValue != null)
                //currentValue.Play(IN);
                currentValue.gameObject.SetActive(true);
        }

        public void Show()
        {
            //backgroundAnim.Play(IN);
            container.gameObject.SetActive(true);
        }

        public void Hide()
        {
            //backgroundAnim.Play(OUT);
            container.gameObject.SetActive(false);
        }

        public void Highlight()
        {
            if (isActive)
            {
                isHighlighting = true;
                if (errorAnim.gameObject.activeInHierarchy)
                    errorAnim.Play(IN);
                else
                    HighlightImmedately();
            }
        }

        public void Unhighlight()
        {
            if (isActive)
            {
                isHighlighting = false;
                if (errorAnim.gameObject.activeInHierarchy)
                    errorAnim.Play(OUT);
                else
                    UnhighlightImmedately();
            }
        }

        public void HighlightImmedately()
        {
            isHighlighting = true;
            errorSpriteRenderer.color = errorAnim.gradients[IN].Evaluate(1);
        }

        public void UnhighlightImmedately()
        {
            isHighlighting = false;
            errorSpriteRenderer.color = errorAnim.gradients[OUT].Evaluate(1);
        }

        public void Solve()
        {
            lockAnim.Play(IN);
            isSolved = true;
            //if (isFlag)
            //    HideFlag();
        }

        public void ShowFlag()
        {
            //isFlag = true;
            //flagAnim.Play(IN);
        }

        public void HideFlag()
        {
            //isFlag = false;
            //flagAnim.Play(OUT);
        }

        public void SetColorBlindMode(bool enable)
        {
            enable = supportNumber;
            colorBlindZeroValueSpriteRenderer.gameObject.SetActive(enable);
            colorBlindOneValueSpriteRenderer.gameObject.SetActive(enable);
        }

        private void OnColorBlindModeChanged(bool enable)
        {
            enable = supportNumber;
            SetColorBlindMode(enable);
        }

        public void SetBackgroundColorController(ColorController controller)
        {
            backgroundColorController = controller;
            UpdateBGColor(backgroundColorController.color);
            controller.onColorChanged += OnColorChanged;
        }

        private void OnColorChanged(ColorController colorController,Color color)
        {
            UpdateBGColor(backgroundColorController.color);
        }

        private void UpdateBGColor(Color color)
        {
            if (gameObject)
            {
                backgroundSpriteRenderer.color = backgroundColorController.color;
                activeBGColor = backgroundSpriteRenderer.color;
            }
        }

        private void Update()
        {
            errorSpriteRenderer.enabled = errorSpriteRenderer.color.a > 0;
            shinySpriteRenderer.enabled = shinySpriteRenderer.color.a > 0;

            if (!isActiveFlipAni)
            {
                lockSpriteRenderer.enabled = lockSpriteRenderer.color.a > 0;
                backgroundSpriteRenderer.enabled = !zeroValueSpriteRenderer.gameObject.activeInHierarchy && !oneValueSpriteRenderer.gameObject.activeInHierarchy;
            }
        }

        public void PlayShinyAnim()
        {
            if (shinyAnim.gameObject.activeInHierarchy)
                shinyAnim.Play(0);
        }
            
        public void SetInActiveColor(float duration = -1){
            StartCoroutine(CR_SetInactiveColors(duration));
        }

        private IEnumerator CR_SetInactiveColors(float duration){
            if (duration == -1)
                duration = setInactiveColorDuration;

            isActive = false;
            Color inactiveColorZero = new Color(activeZeroColor.grayscale,activeZeroColor.grayscale,activeZeroColor.grayscale, activeZeroColor.a);
            Color inactiveColorOne = new Color(activeOneColor.grayscale,activeOneColor.grayscale,activeOneColor.grayscale,activeOneColor.a);
            Color inactiveColorBG = new Color(activeBGColor.grayscale*5/6, activeBGColor.grayscale * 5 / 6, activeBGColor.grayscale * 5 / 6);
            float t = 0;
            while (t <= duration)
            {
                t += Time.deltaTime;
                zeroValueSpriteRenderer.color = Color.Lerp(zeroValueSpriteRenderer.color, inactiveColorZero, t / duration);
                oneValueSpriteRenderer.color = Color.Lerp(oneValueSpriteRenderer.color, inactiveColorOne, t / duration);
                backgroundSpriteRenderer.color = Color.Lerp(backgroundSpriteRenderer.color, inactiveColorBG, t / duration);
                yield return null;
            }
            lockSpriteRenderer.gameObject.SetActive(false);
            colorBlindOneValueSpriteRenderer.enabled = false;
            colorBlindZeroValueSpriteRenderer.enabled = false;
            zeroValueSpriteRenderer.color = inactiveColorZero;
            oneValueSpriteRenderer.color = inactiveColorOne;
            backgroundSpriteRenderer.color = inactiveColorBG;
        }

        private IEnumerator CR_SetActiveColors(float duration){
            if (duration == -1)
                duration = setInactiveColorDuration;
            isActive = true;
            float t = 0;
            while (t <= duration)
            {
                t += Time.deltaTime;
                zeroValueSpriteRenderer.color = Color.Lerp(zeroValueSpriteRenderer.color, activeZeroColor, t / duration);
                oneValueSpriteRenderer.color = Color.Lerp(oneValueSpriteRenderer.color, activeOneColor, t / duration);
                backgroundSpriteRenderer.color = Color.Lerp(backgroundSpriteRenderer.color, activeBGColor, t / duration);
                yield return null;
            }
            lockSpriteRenderer.gameObject.SetActive(true);
            colorBlindOneValueSpriteRenderer.enabled = true;
            colorBlindZeroValueSpriteRenderer.enabled = true;
            zeroValueSpriteRenderer.color = activeZeroColor;
            oneValueSpriteRenderer.color = activeOneColor;
            backgroundSpriteRenderer.color = activeBGColor;
        }

        public void SetActiveColor(float duration = -1){
            StartCoroutine (CR_SetActiveColors (duration));
        }

        public void HighLightCell()
        {
            if (!highLighterRenderer.gameObject.activeSelf)
                highLighterRenderer.gameObject.SetActive(true);
        }

        public void StopHighLight()
        {
            if (highLighterRenderer.gameObject.activeSelf)
                highLighterRenderer.gameObject.SetActive(false);
        }

        public void SetFlipSprite(Sprite sprite)
        {
            flipedSprite = sprite;
        }

        public void FlipOver(float delayStart,float delayScale)
        {
            if (flipCoroutine != null)
                StopCoroutine(flipCoroutine);
            flipCoroutine = StartCoroutine(CR_FlipOver(delayStart, delayScale));
        }


        public void ChangeSprite(Sprite sprite)
        {
            if (sprite != null)
            {
                if (oneValueSpriteRenderer.enabled)
                {
                    oneValueSpriteRenderer.sprite = sprite;
                }
                if (zeroValueSpriteRenderer.enabled)
                {
                    zeroValueSpriteRenderer.sprite = sprite;
                }
            }
        }

        IEnumerator CR_FlipOver(float delayStart,float delayScale)
        {
            isActiveFlipAni = true;
            yield return new WaitForSeconds(delayStart);
            backgroundSpriteRenderer.enabled = true;
            float value = 0;
            float speed = 1 / VisualBoard.Instance.timeFlip;

            int numberFlip = UnityEngine.Random.Range(VisualBoard.Instance.MinMaxFlipNumber.x, VisualBoard.Instance.MinMaxFlipNumber.y);
            bool isSwitchedColor = false;
            while (numberFlip > 0)
            {
                numberFlip--;
                value = 0;
                int dir = UnityEngine.Random.Range(0, 2);

                Quaternion startRotation = Quaternion.Euler(0, 0, 0);
                Quaternion endRotation = dir == 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(90, 0, 0);
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    if (oneValueSpriteRenderer.enabled)
                        oneValueSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    if (zeroValueSpriteRenderer.enabled)
                        zeroValueSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    lockSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    yield return null;
                }

                spriteMask.enabled = true;
                oneValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                zeroValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                value = 0;
                startRotation = dir == 0 ? Quaternion.Euler(-180, 90, -180) : Quaternion.Euler(90, -180, -180);
                endRotation = dir == 0 ? Quaternion.Euler(-180, 180, -180) : Quaternion.Euler(180, -180, -180);
                oneValueSpriteRenderer.transform.GetChild(0).gameObject.SetActive(false);
                zeroValueSpriteRenderer.transform.GetChild(0).gameObject.SetActive(false);
                float startRatio = 1;
                if (flipedSprite != null && numberFlip == 0)
                {
                    oneValueSpriteRenderer.color = Color.white;
                    zeroValueSpriteRenderer.color = Color.white;
                    startRatio = (oneValueSpriteRenderer.sprite.bounds.size.x / flipedSprite.bounds.size.x);
                    if (oneValueSpriteRenderer.enabled)
                    {
                        oneValueSpriteRenderer.sprite = flipedSprite;
                        oneValueSpriteRenderer.transform.localScale = Vector3.one * startRatio;
                    }
                    if (zeroValueSpriteRenderer.enabled)
                    {
                        zeroValueSpriteRenderer.sprite = flipedSprite;
                        zeroValueSpriteRenderer.transform.localScale = Vector3.one * startRatio;
                    }
                }
                else
                {
                    if (isSwitchedColor)
                    {
                        zeroValueSpriteRenderer.color = activeZeroColor;
                        oneValueSpriteRenderer.color = activeOneColor;
                    }
                    else
                    {
                        zeroValueSpriteRenderer.color = activeOneColor;
                        oneValueSpriteRenderer.color = activeZeroColor;
                    }
                    isSwitchedColor = !isSwitchedColor;
                }

                lockSpriteRenderer.enabled = false;
                while (value < 1)
                {
                    value += Time.deltaTime * speed;
                    if (oneValueSpriteRenderer.enabled)
                        oneValueSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    if (zeroValueSpriteRenderer.enabled)
                        zeroValueSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    lockSpriteRenderer.transform.rotation = Quaternion.Lerp(startRotation, endRotation, value);
                    yield return null;
                }

                yield return new WaitForSeconds(VisualBoard.Instance.timeHoldFlip);
            }


            //yield return new WaitForSeconds(delayScale);
            //value = 0;
            //speed = 1 / 0.3f;
            //backgroundSpriteRenderer.enabled = false;
            //oneValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            //zeroValueSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            //while (value < 1)
            //{
            //    value += Time.deltaTime * speed;
            //    if (oneValueSpriteRenderer.enabled)
            //        oneValueSpriteRenderer.transform.localScale = Mathf.Lerp(startRatio, endRatio, value) * Vector3.one;
            //    if (zeroValueSpriteRenderer.enabled)
            //        zeroValueSpriteRenderer.transform.localScale = Mathf.Lerp(startRatio, endRatio, value) * Vector3.one;
            //    yield return null;
            //}
            //spriteMask.enabled = false;
        }

        internal void SetSkin(SkinScriptableObject skinSO)
        {
            zeroValueSpriteRenderer.sprite = skinSO.zeroSprite;
            zeroValueSpriteRenderer.color = skinSO.zeroTintColor;
            activeZeroColor = skinSO.zeroTintColor;

            oneValueSpriteRenderer.sprite = skinSO.oneSprite;
            oneValueSpriteRenderer.color = skinSO.oneTintColor;
            activeOneColor = skinSO.oneTintColor;

            supportNumber = skinSO.supportNumber;
            SetColorBlindMode(supportNumber);
        }

        internal void SetSkin()
        {
            if (listenToSkinChangedEvent == false)
                return;
            SetSkin(SkinManager.GetActivatedSkin());
        }

        internal void SetSkin(int index)
        {
            if (listenToSkinChangedEvent == false)
                return;
            SetSkin(SkinManager.GetSkinFromIndex(index));
        }
    }
}