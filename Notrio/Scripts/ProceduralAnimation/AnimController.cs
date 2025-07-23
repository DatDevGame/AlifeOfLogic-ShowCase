using UnityEngine;
using System.Collections;

namespace Pinwheel
{
    [AddComponentMenu("Pinwheel/Animate/Procedural Animation Controller")]
    public class AnimController : MonoBehaviour
    {
        public ProceduralAnimation anim;
        public bool playOnAwake;
        public float delay;
        [Tooltip("-1 for infinite looping, 0 for stop, n for n-times looping")]
        public int loopCount;
        public float loopOffset;
        public int curveIndex;

        private float loopDelay;
        private int remainingLoop;

        [HideInInspector]
        public bool isPlaying;

        public void Awake()
        {
            if (playOnAwake)
                Play();
        }

        public void Play()
        {
            isPlaying = true;
            loopDelay = anim.duration + loopOffset;
            remainingLoop = loopCount;
            StartCoroutine(PlayRepeating());
        }

        private IEnumerator PlayRepeating()
        {
            yield return new WaitForSeconds(delay);
            while (remainingLoop != 0)
            {
                --remainingLoop;
                anim.Play(curveIndex);
                yield return new WaitForSeconds(loopDelay);
            }
            isPlaying = false;
        }

        public void Stop()
        {
            remainingLoop = 0;
        }

        public void StopImmediately()
        {
            remainingLoop = 0;
            isPlaying = false;
            StopAllCoroutines();
        }

        public void OnDisable()
        {
            isPlaying = false;
            StopAllCoroutines();
        }


    }
}