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
        public readonly string[] forcedNative = new string[]
        {
            "templatemod",
            "oldseaport"
        };
        public (AssetBundle, ModManifest) Load(string name)
        {
            return assetBundles[name.ToLower()];
        }
        public DirectoryInfo modPath = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
        public DirectoryInfo modDirectory;
        public DirectoryInfo pluginsDirectory;
        public void LoadAllAssetBundles()
        {
            modDirectory = modPath.Parent;
            pluginsDirectory = modDirectory;

            while (pluginsDirectory != null && pluginsDirectory.Name != "plugins")
            {
                pluginsDirectory = pluginsDirectory.Parent;
            }

            if (pluginsDirectory != null)
            {
                LethalExpansion.Log.LogInfo("Plugins folder found: " + pluginsDirectory.FullName);
            }
            else
            {
                LethalExpansion.Log.LogWarning("Mod is not in a plugins folder.");
                return;
            }

            foreach (string file in Directory.GetFiles(pluginsDirectory.FullName, "*.lem", SearchOption.AllDirectories))
            {
                LoadBundle(file);
            }
        }
        public void LoadBundle(string file)
        {
            if (forcedNative.Contains(Path.GetFileNameWithoutExtension(file)) && !file.Contains(modDirectory.FullName))
            {
                LethalExpansion.Log.LogWarning($"Illegal use of reserved Asset Bundle name: {Path.GetFileNameWithoutExtension(file)} at: {file}.");
                return;
            }
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
                        string manifestPath = $"Assets/Mods/{Path.GetFileNameWithoutExtension(file)}/ModManifest.asset";

                        ModManifest modManifest = loadedBundle.LoadAsset<ModManifest>(manifestPath);
                        if (modManifest != null)
                        {
                            if(!assetBundles.Any(b => b.Value.Item2.modName == modManifest.modName))
                            {
                                LethalExpansion.Log.LogInfo($"Module found: {modManifest.modName} v{(modManifest.GetVersion() != null ? modManifest.GetVersion().ToString() : "0.0.0.0" )}");

                                assetBundles.Add(Path.GetFileNameWithoutExtension(file).ToLower(), (loadedBundle, modManifest));
                            }
                            else
                            {
                                LethalExpansion.Log.LogWarning($"Another mod with same name is already loaded: {modManifest.modName}");
                                loadedBundle.Unload(true);
                                LethalExpansion.Log.LogInfo($"AssetBundle unloaded: {Path.GetFileName(file)}");
                            }
                        }
                        else
                        {
                            LethalExpansion.Log.LogWarning($"AssetBundle have no ModManifest: {Path.GetFileName(file)}");
                            loadedBundle.Unload(true);
                            LethalExpansion.Log.LogInfo($"AssetBundle unloaded: {Path.GetFileName(file)}");
                        }
                    }
                    else
                    {
                        LethalExpansion.Log.LogWarning($"File is not an AssetBundle: {Path.GetFileName(file)}");
                    }
                }
                else
                {
                    LethalExpansion.Log.LogWarning($"AssetBundle with same name already loaded: {Path.GetFileName(file)}");
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
        public bool IncompatibleBundlesLoaded(string[] bundleNames)
        {
            foreach(string bundleName in bundleNames)
            {
                if (assetBundles.ContainsKey(bundleName.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
