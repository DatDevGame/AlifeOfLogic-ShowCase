using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LatteGames.Log;

[WindowMenuItem("General", "Game Data", "Assets/GameManagerALOL/GameWindowEditor/Resources", order: -100)]
[CreateAssetMenu(menuName = "LatteGames/GameDataSO", fileName = "GameDataSO")]
public class GameDataSO : SingletonSO<GameDataSO>
{
    [SerializeField, OnValueChanged("OnValidateIsEnableLogOnBuild")]
    private bool m_IsDevMode;
    [SerializeField, ShowIf("m_IsDevMode"), OnValueChanged("OnValidateIsEnableLogOnBuild")]
    private LogConfig m_LogConfig;

    public bool isDevMode => m_IsDevMode;
    public bool isEnableLogOnBuild => isDevMode && m_LogConfig.isEnableLogOnBuild;
    public bool isSaveLogFile => isDevMode && m_LogConfig.isSaveLogFile;
    public LogConfig logConfig => m_LogConfig;

#if UNITY_EDITOR
    private void OnValidateIsEnableLogOnBuild()
    {
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        if (buildTargetGroup == BuildTargetGroup.Unknown)
        {
            var propertyInfo = typeof(EditorUserBuildSettings).GetProperty("activeBuildTargetGroup", BindingFlags.Static | BindingFlags.NonPublic);
            if (propertyInfo != null)
                buildTargetGroup = (BuildTargetGroup)propertyInfo.GetValue(null, null);
        }

        var previousProjectDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        var projectDefines = previousProjectDefines.Split(';').ToList();

        if (isEnableLogOnBuild)
        {
            if (!projectDefines.Contains(LGDebug.k_LatteDebugDefineSymbol))
            {
                // Prevent reload assemblies
                EditorApplication.LockReloadAssemblies();
                projectDefines.Add(LGDebug.k_LatteDebugDefineSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", projectDefines.ToArray()));
                EditorApplication.UnlockReloadAssemblies();
            }
        }
        else
        {
            if (projectDefines.Contains(LGDebug.k_LatteDebugDefineSymbol))
            {
                // Prevent reload assemblies
                EditorApplication.LockReloadAssemblies();
                projectDefines.Remove(LGDebug.k_LatteDebugDefineSymbol);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", projectDefines.ToArray()));
                EditorApplication.UnlockReloadAssemblies();
            }
        }
    }
#endif
}
