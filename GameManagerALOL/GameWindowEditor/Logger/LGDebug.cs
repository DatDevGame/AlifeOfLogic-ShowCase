using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class LGDebug
{
    public const string k_LatteDebugDefineSymbol = "LATTE_DEBUG";
    public const string k_UnityEditorDefineSymbol = "UNITY_EDITOR";

    public static string FormatLogMessage(object message, string tag)
    {
        return $"LATTEDEBUG_{tag}:: {message}";
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void Log(object message, string tag = "Default", UnityEngine.Object context = null)
    {
        Debug.Log(FormatLogMessage(message, tag), context);
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void LogWarning(object message, string tag = "Default", UnityEngine.Object context = null)
    {
        Debug.LogWarning(FormatLogMessage(message, tag), context);
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void LogWarningFormat(string format, string[] args)
    {
        Debug.LogWarningFormat(format, args);
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void LogError(object message, string tag = "Default", UnityEngine.Object context = null)
    {
        Debug.LogError(FormatLogMessage(message, tag), context);
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void LogErrorFormat(string format, string[] args)
    {
        Debug.LogErrorFormat(format, args);
    }

    [Conditional(k_UnityEditorDefineSymbol), Conditional(k_LatteDebugDefineSymbol)]
    public static void LogException(Exception exception, UnityEngine.Object context = null)
    {
        Debug.LogException(exception, context);
    }
}