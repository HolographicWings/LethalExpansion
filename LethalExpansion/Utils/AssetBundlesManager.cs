using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalSDK.ScriptableObjects;

namespace LethalExpansion.Utils
{
    public class AssetBundlesManager
    {
        private static AssetBundlesManager _instance;
        public static AssetBundlesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetBundlesManager();
                }
                return _instance;
            }
        }
        public AssetBundle mainAssetBundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("LethalExpansion.dll", "lethalexpansion"));
        public Dictionary<String, (AssetBundle, ModManifest)> assetBundles = new Dictionary<String, (AssetBundle, ModManifest)>();
        public (AssetBundle, ModManifest) Load(string name)
        {
            return assetBundles[name.ToLower()];
        }
        public void LoadAllAssetBundles()
        {
            foreach (string file in Directory.GetFiles(Assembly.GetExecutingAssembly().Location.Replace("LethalExpansion.dll", @"Modules\")))
            {
                AssetBundle loadedBundle = AssetBundle.LoadFromFile(file);
                if (loadedBundle != null)
                {
                    Debug.Log("AssetBundle loaded: " + Path.GetFileName(file));

                    string manifestPath = $"Assets/Mods/{Path.GetFileName(file)}/ModManifest.asset";

                    ModManifest modManifest = loadedBundle.LoadAsset<ModManifest>(manifestPath);
                    if (modManifest != null)
                    {
                        Debug.Log("ModManifest found: " + modManifest.modName);

                        assetBundles.Add(Path.GetFileName(file).ToLower(), (loadedBundle, modManifest));
                    }
                    else
                    {
                        Debug.Log("AssetBundle have no ModManifest: " + Path.GetFileName(file));
                    }
                }
                else
                {
                    Debug.Log("File is not an AssetBundle: " + Path.GetFileName(file));
                }
            }
        }
    }
}
