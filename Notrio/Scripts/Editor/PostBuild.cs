using System.Collections;
using System.Collections.Generic;
using EasyMobile;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PostBuild
{
    [PostProcessBuildAttribute(999)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if UNITY_IOS
        Debug.Log("alol custom post build process");

        var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        var infoPlist = new PlistDocument();
        var infoPlistPath = pathToBuiltProject + "/Info.plist";
        infoPlist.ReadFromFile(infoPlistPath);

        string dummyGUID = project.AddFile(Application.dataPath + "/dummy.png", "/dummy.png");
        project.AddFileToBuild(GetDefaultTarget(project), dummyGUID);
        project.AddAssetTagForFile(GetDefaultTarget(project), dummyGUID, "dummy");
        project.AddAssetTagToDefaultInstall(GetDefaultTarget(project), "dummy");

        infoPlist.root.SetString("GADApplicationIdentifier",  EM_Settings.Advertising.AdMob.AppId.IosId);
        project.AddBuildProperty(GetDefaultTarget(project), "PRODUCT_BUNDLE_IDENTIFIER", infoPlist.root["CFBundleIdentifier"].AsString());

        project.WriteToFile(projectPath);
        infoPlist.WriteToFile(infoPlistPath);
#endif
    }

    private static string GetDefaultTarget(PBXProject project) {
        return project.GetUnityMainTargetGuid();
    }
}
