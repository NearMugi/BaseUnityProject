using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AssetBundleManager_Base : MonoBehaviour
{
    #region Singleton

    private static AssetBundleManager_Base instance;

    public static AssetBundleManager_Base Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (AssetBundleManager_Base)FindObjectOfType(typeof(AssetBundleManager_Base));

                if (instance == null)
                {
                    Debug.LogError(typeof(AssetBundleManager_Base) + "is nothing");
                }
            }

            return instance;
        }
    }

    #endregion Singleton


    private void Awake()
    {
        Caching.ClearCache();
    }

    // A dictionary to hold the AssetBundle references
    static private Dictionary<string, AssetBundleRef> dictAssetBundleRefs;
    static AssetBundleManager_Base()
    {
        dictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
    }
    // Class with the AssetBundle reference, url and version
    private class AssetBundleRef
    {
        public AssetBundle assetBundle = null;
        public int version;
        public string url;
        public AssetBundleRef(string strUrlIn, int intVersionIn)
        {
            url = strUrlIn;
            version = intVersionIn;
        }
    };
    // Get an AssetBundle
    public AssetBundle getAssetBundle(string url, int version)
    {
        string keyName = ReplaceSharp(url) + version.ToString();

        //Debug.LogWarning("[getAssetBundle]  keyName:" + keyName);

        AssetBundleRef abRef;

        if (dictAssetBundleRefs.TryGetValue(keyName, out abRef))
        {
            //Debug.LogWarning("[getAssetBundle]  abRef.assetBundle.name:" + abRef.assetBundle.name);
            
            return abRef.assetBundle;
        }
        else
        {
            return null;
        }

    }
    // Download an AssetBundle
    public IEnumerator downloadAssetBundle(string url, int version)
    {
        string keyName = ReplaceSharp(url) + version.ToString();
        if (dictAssetBundleRefs.ContainsKey(keyName))
        {
            yield return null;
        }
        else
        {
            while (!Caching.ready) yield return null;

            using (WWW www = WWW.LoadFromCacheOrDownload(ReplaceSharp(url), version))
            {
                yield return www;
                
                if (!string.IsNullOrEmpty(www.error))
                {
                    throw new Exception("[downloadAssetBundle] \r\n URL:" + url + "\r\n Error:" + www.error + "\r\n--------------------------------");
                }

                //ディレクトリーに追加
                AssetBundleRef abRef = new AssetBundleRef(url, version);
                abRef.assetBundle = www.assetBundle;
                dictAssetBundleRefs.Add(keyName, abRef);
                //Debug.LogWarning("[downloadAssetBundle]  keyName:" + keyName + "  assetBundle:" + abRef.assetBundle.name);

            }
        }
    }
    // Unload an AssetBundle
    public void Unload(string url, int version, bool allObjects)
    {
        string keyName = ReplaceSharp(url) + version.ToString();
        AssetBundleRef abRef;
        if (dictAssetBundleRefs.TryGetValue(keyName, out abRef))
        {
            abRef.assetBundle.Unload(allObjects);
            abRef.assetBundle = null;
            dictAssetBundleRefs.Remove(keyName);
        }
    }

    string ReplaceSharp(string url)
    {
        string _s = url;
        if (_s.Contains("#"))
        {
            _s = _s.Replace("#", "%23");
        }

        return _s;
    }


}

