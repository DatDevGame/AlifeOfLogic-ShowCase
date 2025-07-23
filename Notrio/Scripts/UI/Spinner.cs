using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinwheel;

namespace Takuzu
{
    public class Spinner : MonoBehaviour
    {
        public AnimController anim;

        private void OnEnable()
        {
            anim.loopCount = -1;
            anim.Play();
        }
    }
}