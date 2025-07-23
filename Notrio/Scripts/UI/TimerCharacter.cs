using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pinwheel;

namespace Takuzu
{
    public class TimerCharacter : MonoBehaviour
    {
        public RotateAnimation anim;
        public RectTransform selfRt;
        public RectTransform block1;
        public RectTransform block2;
        public Text text1;
        public Text text2;

        public Vector3 preparePosition1;
        public Vector3 prepareRotation1;
        public Vector3 preparePosition2;
        public Vector3 prepareRotation2;


        public void SetNewText(string t)
        {
            if (t.Equals(text2.text))
                return;
            selfRt.rotation = Quaternion.identity;
            text1.text = text2.text;
            text2.text = t;

            block1.anchoredPosition = preparePosition1;
            block1.rotation = Quaternion.Euler(prepareRotation1);
            block2.anchoredPosition = preparePosition2;
            block2.rotation = Quaternion.Euler(prepareRotation2);

            if(gameObject.activeInHierarchy)
                anim.Play(0);
        }
    }
}