using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper instance;
        public static CoroutineHelper Instance
        {
            get
            {
                if (instance==null)
                {
                    GameObject g = new GameObject("CoroutineHelperInstance");
                    instance = g.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(g);
                }
                return instance;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void DoActionDelay(Action action, float delay)
        {
            StartCoroutine(CrDoActionDelay(action, delay));
        }

        private IEnumerator CrDoActionDelay(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Error on coroutine: " + e.ToString());
            }
        }

        public void PostponeActionUntil(Action action, Func<bool> predicate)
        {
            StartCoroutine(CrPostponeActionUntil(action, predicate));
        }

        private IEnumerator CrPostponeActionUntil(Action action, Func<bool> predicate)
        {
            yield return new WaitUntil(predicate);
            action();
        }

        public void ForeachPerFrame<T>(Action<T> action, ICollection<T> collection)
        {
            StartCoroutine(CrForeachPerFrame(action, collection));
        }

        private IEnumerator CrForeachPerFrame<T>(Action<T> action, ICollection<T> collection)
        {
            IEnumerator i = collection.GetEnumerator();
            while(i.MoveNext())
            {
                action.Invoke((T)i.Current);
                yield return null;
            }
        }

        public void RepeatUntil(Action action, float interval, Func<bool> predicate)
        {
            StartCoroutine(CrRepeatUntil(action, interval, predicate));
        }

        private IEnumerator CrRepeatUntil(Action action, float interval, Func<bool> predicate)
        {
            while(!predicate())
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }

                yield return new WaitForSeconds(interval);
            }
        }
    }
}