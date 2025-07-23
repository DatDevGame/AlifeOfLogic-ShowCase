using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Takuzu
{
    public class GCManager : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Time.frameCount % 180 == 0)
                GC.Collect();
        }
    }
}