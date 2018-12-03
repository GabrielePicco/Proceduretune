#if UNITY_EDITOR
using UnityEditor;

public class CreateAssetBundles : EditorWindow
{
    [MenuItem("Assets/Save Asset Bundle")]
    static void ExportBundle()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.iOS);
        AssetDatabase.Refresh();
    }
}
#endif