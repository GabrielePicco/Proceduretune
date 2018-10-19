using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : MonoBehaviour
{
    private static AssetBundleManager _instance;
    private AssetBundle downloadBundle;
    private Dictionary<string, AssetBundle> cachedBundle = new Dictionary<string, AssetBundle>();

    public static AssetBundleManager Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public IEnumerator DownloadAssetBundle(string bundle)
    {
        //WWW www = WWW.LoadFromCacheOrDownload("file:///" + Application.dataPath + "/AssetBundles/octave1.unity3d", 0);
        //WWW www = WWW.LoadFromCacheOrDownload("https://www.dropbox.com/s/9kkx8utmt8qj3f5/octave1.unity3d?dl=1", 0);
        if (cachedBundle.ContainsKey(bundle))
        {
            downloadBundle = cachedBundle[bundle];
        }
        else
        {
            WWW www = WWW.LoadFromCacheOrDownload(StreamingAssetPath() + bundle, 0);
            yield return www;
            downloadBundle = www.assetBundle;
            cachedBundle.Add(bundle, downloadBundle);
        }
    }

    public AssetBundle getBundle()
    {
        return downloadBundle;
    }


    public static String StreamingAssetPath()
    {
        String path = "";
#if UNITY_ANDROID
        path = "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IOS
        path Application.dataPath + "/Raw/";
#else
        path = Application.dataPath + "/StreamingAssets/";
#endif
        return path;
    }
}
