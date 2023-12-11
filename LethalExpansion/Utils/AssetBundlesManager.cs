using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using LethalSDK.ScriptableObjects;
using System.Linq;

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
        public AssetBundle mainAssetBundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("LethalExpansion.dll", "lethalexpansion.lem"));
        public Dictionary<String, (AssetBundle, ModManifest)> assetBundles = new Dictionary<String, (AssetBundle, ModManifest)>();
        public (AssetBundle, ModManifest) Load(string name)
        {
            return assetBundles[name.ToLower()];
        }
        public void LoadAllAssetBundles()
        {
            string localPath;
            if (Assembly.GetExecutingAssembly().Location.Contains("HolographicWings-LethalExpansion"))
            {
                localPath = Assembly.GetExecutingAssembly().Location.Replace(@"HolographicWings-LethalExpansion\LethalExpansion.dll", string.Empty);
                if(Directory.Exists(Application.dataPath.Replace("Lethal Company_Data", @"BepInEx\plugins")))
                {
                    foreach (string file in Directory.GetFiles(Application.dataPath.Replace("Lethal Company_Data", @"BepInEx\plugins"), "*.lem", SearchOption.AllDirectories))
                    {
                        LoadBundle(file);
                    }
                }
            }
            else
            {
                localPath = Assembly.GetExecutingAssembly().Location.Replace(@"LethalExpansion.dll", string.Empty);
            }
            foreach (string file in Directory.GetFiles(localPath, "*.lem", SearchOption.AllDirectories))
            {
                LoadBundle(file);
            }
        }
        public void LoadBundle(string file)
        {
            if (Path.GetFileName(file) != "lethalexpansion.lem")
            {
                if (!assetBundles.ContainsKey(Path.GetFileNameWithoutExtension(file)))
                {
                    AssetBundle loadedBundle = null;
                    try
                    {
                        loadedBundle = AssetBundle.LoadFromFile(file);
                    }
                    catch (Exception e)
                    {
                        LethalExpansion.Log.LogError(e);
                    }
                    if (loadedBundle != null)
                    {
                        LethalExpansion.Log.LogInfo("AssetBundle loaded: " + Path.GetFileName(file));

                        string manifestPath = $"Assets/Mods/{Path.GetFileNameWithoutExtension(file)}/ModManifest.asset";

                        ModManifest modManifest = loadedBundle.LoadAsset<ModManifest>(manifestPath);
                        if (modManifest != null)
                        {
                            if(!assetBundles.Any(b => b.Value.Item2.modName == modManifest.modName))
                            {
                                LethalExpansion.Log.LogInfo("ModManifest found: " + modManifest.modName);

                                assetBundles.Add(Path.GetFileNameWithoutExtension(file).ToLower(), (loadedBundle, modManifest));
                            }
                            else
                            {
                                LethalExpansion.Log.LogWarning("Another mod with same name is already loaded: " + modManifest.modName);
                                loadedBundle.Unload(true);
                                LethalExpansion.Log.LogInfo("AssetBundle unloaded: " + Path.GetFileName(file));
                            }
                        }
                        else
                        {
                            LethalExpansion.Log.LogWarning("AssetBundle have no ModManifest: " + Path.GetFileName(file));
                            loadedBundle.Unload(true);
                            LethalExpansion.Log.LogInfo("AssetBundle unloaded: " + Path.GetFileName(file));
                        }
                    }
                    else
                    {
                        LethalExpansion.Log.LogWarning("File is not an AssetBundle: " + Path.GetFileName(file));
                    }
                }
                else
                {
                    LethalExpansion.Log.LogWarning("AssetBundle with same name already loaded: " + Path.GetFileName(file));
                }
            }
        }
        public bool BundleLoaded(string bundleName)
        {
            return assetBundles.ContainsKey(bundleName.ToLower());
        }
        public bool BundlesLoaded(string[] bundleNames)
        {
            foreach(string bundleName in bundleNames)
            {
                if (!assetBundles.ContainsKey(bundleName.ToLower()))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
