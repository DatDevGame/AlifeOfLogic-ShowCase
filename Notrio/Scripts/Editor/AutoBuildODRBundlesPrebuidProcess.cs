using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

class MyCustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    static string iosODRPath = Path.Combine(Application.dataPath, "ODR");
    static string androidODRPath = Path.Combine(Application.streamingAssetsPath, "ODR");
    static string iosODRPrebuildPath = PathCombine(Application.dataPath, "PrebuildAssets", "ODR", "Ios");
    static string androidODRPrebuildPath = PathCombine(Application.dataPath, "PrebuildAssets", "ODR", "Android");

    public void OnPreprocessBuild(BuildReport report)
    {
        //clean odr destination folder
        CleanODRFolders();
        //copy bundles from prebuild folder
        #if UNITY_IOS
        Copy(iosODRPrebuildPath, iosODRPath);
        #else 
        Copy(androidODRPrebuildPath, androidODRPath);
        #endif
    }

    [MenuItem("Bundle/Build ODR AssetBundles")]
    public static void BuildODRAssetsBundles()
    {   
        //clean all odr output folder
        //base on build platform create odr bundles
        #if UNITY_IOS
            CleanIOSPrebuildAssetsBundles();
            BuildIOSAssetBundles();
        #else
            CleanAndroidPrebuildAssetsBundles();
            BuildAndroidAssetBundles();
        #endif
    }
    private static void CleanIOSPrebuildAssetsBundles()
    {
        if(Directory.Exists(iosODRPrebuildPath))
            Directory.Delete(iosODRPrebuildPath, true);
    }
    private static void CleanAndroidPrebuildAssetsBundles()
    {
        if(Directory.Exists(androidODRPrebuildPath))
            Directory.Delete(androidODRPrebuildPath, true);
    }
    private static void CleanODRFolders()
    {
        if(Directory.Exists(iosODRPath))
            Directory.Delete(iosODRPath, true);
        if(Directory.Exists(androidODRPath))
            Directory.Delete(androidODRPath, true);
    }
    //Ios bundles build
    [InitializeOnLoadMethod]
    static void SetupResourcesBuild( )
    {
        UnityEditor.iOS.BuildPipeline.collectResources += CollectResources;
    }

    static UnityEditor.iOS.Resource[] CollectResources( )
    {
        return new UnityEditor.iOS.Resource[]
        {
            new UnityEditor.iOS.Resource( "textures", "Assets/ODR/textures" ).AddOnDemandResourceTags( "textures" ),
            new UnityEditor.iOS.Resource( "video", "Assets/ODR/video").AddOnDemandResourceTags("video")
        };
    }

    static void BuildIOSAssetBundles( )
    {
        var options = BuildAssetBundleOptions.None;

        bool shouldCheckODR = EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;

#if UNITY_TVOS
            shouldCheckODR |= EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS;
#endif

        if( shouldCheckODR )
        {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
            if( PlayerSettings.iOS.useOnDemandResources )
                options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif

#if ENABLE_IOS_APP_SLICING
            options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
        }
        CreatePrebuildODRBundles(iosODRPrebuildPath, options);
    }
    //Android bundles build
    static void BuildAndroidAssetBundles()
    {
        var options = BuildAssetBundleOptions.None;
        CreatePrebuildODRBundles(androidODRPrebuildPath, options);
    }

    static void CreatePrebuildODRBundles(string path, BuildAssetBundleOptions options)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path);
        }
        Directory.CreateDirectory(path);
        BuildPipeline.BuildAssetBundles(path, options, EditorUserBuildSettings.activeBuildTarget);
    }

    private static string PathCombine(string basePath, params string[] paths)
    {
        string generatedPath = basePath;
        foreach (var path in paths)
        {
            generatedPath = Path.Combine(generatedPath, path);
        }
        return generatedPath;
    }

    static void Copy(string sourceDir, string targetDir, bool recursive = false)
    {
        if(Directory.Exists(targetDir) == false)
            Directory.CreateDirectory(targetDir);

        foreach(var file in Directory.GetFiles(sourceDir))
        {
            if(Path.GetExtension(file) == ".meta")
                continue;
            File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
        }

        if(recursive == false)
            return;
        foreach(var directory in Directory.GetDirectories(sourceDir))
            Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
    }
}