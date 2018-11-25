using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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

    private void Start()
    {
        string[] initBundleCache = new string[] { "octave1", "octave2", "octave3", "octave4", "octave5", "octave6", "octave7"};
        foreach(string bundle in initBundleCache){
            StartCoroutine(DownloadAssetBundle(bundle));
        }
    }

    public bool IsInCache(string bundle)
    {
        return cachedBundle.ContainsKey(bundle);
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
            Caching.ClearCache();
            //WWW www = WWW.LoadFromCacheOrDownload("https://www.dropbox.com/s/ygnfqwjjealjjlj/octave5?dl=1", 0);
            //yield return www;
            //downloadBundle = www.assetBundle;
            if(bundle.Contains("http")){
                using (var uwr = new UnityWebRequest(bundle, UnityWebRequest.kHttpVerbGET))
                {
                    uwr.downloadHandler = new DownloadHandlerAssetBundle(bundle, 0);
                    yield return uwr.SendWebRequest();
                    downloadBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
            }
            else{
                downloadBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundle));
            }
            cachedBundle.Add(bundle, downloadBundle);
        }
        yield return downloadBundle;
    }

    public AssetBundle GetBundle(string bundle)
    {
        return cachedBundle.ContainsKey(bundle) ? cachedBundle[bundle] : null;
    }

    public AssetBundle GetBundle()
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
