using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LatteGames.Log
{
    [Flags]
    public enum StackTraceLogType
    {
        //
        // Summary:
        //     LogType used for Errors.
        Error = 1 << 0,
        //
        // Summary:
        //     LogType used for Asserts. (These could also indicate an error inside Unity itself.)
        Assert = 1 << 1,
        //
        // Summary:
        //     LogType used for Warnings.
        Warning = 1 << 2,
        //
        // Summary:
        //     LogType used for regular log messages.
        Log = 1 << 3,
        //
        // Summary:
        //     LogType used for Exceptions.
        Exception = 1 << 4
    }
    [Serializable]
    public struct LogConfig
    {
        [SerializeField]
        private bool m_IsEnableLogOnBuild;
        [SerializeField]
        private bool m_IsSaveLogFile;
        [SerializeField]
        private StackTraceLogType m_SaveStackTraceLogType;

        public bool isEnableLogOnBuild => m_IsEnableLogOnBuild;
        public bool isSaveLogFile => m_IsSaveLogFile;
        public StackTraceLogType saveStackTraceLogType => m_SaveStackTraceLogType;
    }
}