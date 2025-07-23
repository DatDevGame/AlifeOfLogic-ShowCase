using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SupportService
{
    public class CustomLogger : MonoBehaviour
    {
        public static Action<string> UnexpectedError = delegate { };
        private static readonly ILogger appsflyerLogger = new AppsflyerLogger();
        public static ILogger GetLogger()
        {
            return appsflyerLogger;
        }
    }
}