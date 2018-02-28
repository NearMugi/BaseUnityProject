using UnityEditor;
using UnityEngine;

using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        var platform = "Standalone";
#if UNITY_ANDROID
            platform="Android";
#endif
#if UNITY_IOS
            platform="iOS";
#endif
        if (!Directory.Exists(Application.streamingAssetsPath + "/AssetBundles"))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/AssetBundles");
        }

        if (!Directory.Exists(Application.streamingAssetsPath + "/AssetBundles/" + platform))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/AssetBundles/" + platform);
        }

        Debug.Log("Build asset bundles for " + EditorUserBuildSettings.activeBuildTarget);
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/AssetBundles/" + platform, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }
}
