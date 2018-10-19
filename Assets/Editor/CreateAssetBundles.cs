#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CreateAssetBundles : Editor
{
    [MenuItem("Assets/Create Asset Bundle")]
    static void ExportBundle()
    {
        //string bundlePath = "Assets/AssetBundles/newBundle.unity3d";
        string bundlePath = AssetBundleManager.StreamingAssetPath() + "newBundle.unity3d";
        Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        BuildPipeline.BuildAssetBundle(Selection.activeObject, selectedAssets, bundlePath, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, BuildTarget.Android);
        AssetDatabase.Refresh();
    }
}
#endif