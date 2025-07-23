using UnityEngine;
using System.Collections;
using Pinwheel;

namespace Pinwheel
{
    public abstract class ProceduralAnimation : MonoBehaviour
    {
        public float duration;
        public abstract void Play(int curveIndex);
    }
}