using HarmonyLib;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LethalExpansion.Utils;
using LethalSDK.ScriptableObjects;
using BepInEx.Bootstrap;
using LethalSDK.Component;
using Unity.AI.Navigation;
using Unity.Netcode.Components;
using UnityEngine.AI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Video;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    public class GameNetworkManager_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void Start_Prefix(GameNetworkManager __instance)
        {
            if (!LethalExpansion.CompatibleGameVersions.Contains(__instance.gameVersionNum))
            {
                bool showWarning = true;
                if (Chainloader.PluginInfos.ContainsKey("me.swipez.melonloader.morecompany") && __instance.gameVersionNum == 9999)
                {
                    showWarning = false;
                }
                if(showWarning)
                {
                    LethalExpansion.Log.LogWarning("Warning, this mod is not made for this Game Version, this could cause unexpected behaviors.");
                    LethalExpansion.Log.LogWarning(string.Format("Game version: {0}", __instance.gameVersionNum));
                    LethalExpansion.Log.LogWarning(string.Format("Compatible mod versions: {0}", string.Join(",", LethalExpansion.CompatibleGameVersions)));
                }
            }
            AssetBank mainBank = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<ModManifest>("Assets/Mods/LethalExpansion/modmanifest.asset").assetBank;
            if (mainBank != null)
            {
                foreach (var networkprefab in mainBank.NetworkPrefabs())
                {
                    if (networkprefab.PrefabPath != null && networkprefab.PrefabPath.Length > 0)
                    {
                        GameObject prefab = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>(networkprefab.PrefabPath);
                        __instance.GetComponent<NetworkManager>().PrefabHandler.AddNetworkPrefab(prefab);
                        LethalExpansion.Log.LogInfo($"{networkprefab.PrefabName} Prefab registered.");
                    }
                }
            }
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules"))
            {
                Sprite scrapSprite = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<Sprite>("Assets/Mods/LethalExpansion/Sprites/ScrapItemIcon2.png");
                try
                {
                    foreach (KeyValuePair<String,(AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                    {
                        (AssetBundle, ModManifest) bundle2 = AssetBundlesManager.Instance.Load(bundle.Key);

                        if (bundle2.Item1 != null && bundle2.Item2 != null)
                        {
                            if(bundle2.Item2.scraps != null && bundle2.Item2.scraps.Length > 0)
                            {
                                foreach (var newScrap in bundle2.Item2.scraps)
                                {
                                    if (newScrap != null && newScrap.prefab != null && (newScrap.RequiredBundles == null || AssetBundlesManager.Instance.BundlesLoaded(newScrap.RequiredBundles)) && (newScrap.IncompatibleBundles == null || !AssetBundlesManager.Instance.IncompatibleBundlesLoaded(newScrap.IncompatibleBundles)))
                                    {
                                        Item tmpItem = ScriptableObject.CreateInstance<Item>();

                                        tmpItem.name = newScrap.name;
                                        tmpItem.itemName = newScrap.itemName;
                                        tmpItem.canBeGrabbedBeforeGameStart = true;
                                        tmpItem.isScrap = true;
                                        tmpItem.minValue = newScrap.minValue;
                                        tmpItem.maxValue = newScrap.maxValue;
                                        tmpItem.weight = (float)newScrap.weight / 100 + 1;

                                        CheckAndRemoveIllegalComponents(newScrap.prefab.transform, ComponentWhitelists.scrapWhitelist);
                                        tmpItem.spawnPrefab = newScrap.prefab;

                                        tmpItem.twoHanded = newScrap.twoHanded;
                                        tmpItem.twoHandedAnimation = newScrap.twoHandedAnimation;
                                        tmpItem.requiresBattery = newScrap.requiresBattery;
                                        tmpItem.isConductiveMetal = newScrap.isConductiveMetal;

                                        tmpItem.itemIcon = scrapSprite;
                                        tmpItem.syncGrabFunction = false;
                                        tmpItem.syncUseFunction = false;
                                        tmpItem.syncDiscardFunction = false;
                                        tmpItem.syncInteractLRFunction = false;
                                        tmpItem.verticalOffset = newScrap.verticalOffset;
                                        tmpItem.restingRotation = newScrap.restingRotation;
                                        tmpItem.positionOffset = newScrap.positionOffset;
                                        tmpItem.rotationOffset = newScrap.rotationOffset;
                                        tmpItem.meshOffset = false;
                                        tmpItem.meshVariants = newScrap.meshVariants;
                                        tmpItem.materialVariants = newScrap.materialVariants;
                                        tmpItem.canBeInspected = false;

                                        PhysicsProp physicsProp = newScrap.prefab.AddComponent<PhysicsProp>();
                                        physicsProp.grabbable = true;
                                        physicsProp.itemProperties = tmpItem;
                                        physicsProp.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();

                                        AudioSource audioSource = newScrap.prefab.AddComponent<AudioSource>();
                                        audioSource.playOnAwake = false;
                                        audioSource.spatialBlend = 1f;

                                        Transform scanNodeObject = newScrap.prefab.transform.Find("ScanNode");
                                        if (scanNodeObject != null)
                                        {
                                            ScanNodeProperties scanNode = scanNodeObject.gameObject.AddComponent<ScanNodeProperties>();
                                            scanNode.maxRange = 13;
                                            scanNode.minRange = 1;
                                            scanNode.headerText = newScrap.itemName;
                                            scanNode.subText = "Value: ";
                                            scanNode.nodeType = 2;
                                        }
                                        try
                                        {
                                            __instance.GetComponent<NetworkManager>().PrefabHandler.AddNetworkPrefab(newScrap.prefab);
                                            LethalExpansion.Log.LogInfo(newScrap.itemName + " Scrap registered.");
                                        }
                                        catch (Exception ex)
                                        {
                                            LethalExpansion.Log.LogError(ex.Message);
                                        }
                                    }
                                }
                                /*foreach (var newMoon in bundle2.Item2.moons)
                                {
                                    if (newMoon != null && newMoon.MainPrefab != null)
                                    {
                                        CheckAndRemoveIllegalComponents(newMoon.MainPrefab.transform, moonPrefabComponentWhitelist);
                                    }
                                }*/
                                if (bundle2.Item2.assetBank != null && bundle2.Item2.assetBank.NetworkPrefabs() != null && bundle2.Item2.assetBank.NetworkPrefabs().Length > 0)
                                {
                                    foreach (var networkprefab in bundle2.Item2.assetBank.NetworkPrefabs())
                                    {
                                        if (networkprefab.PrefabPath != null && networkprefab.PrefabPath.Length > 0)
                                        {
                                            GameObject prefab = bundle.Value.Item1.LoadAsset<GameObject>(networkprefab.PrefabPath);
                                            CheckAndRemoveIllegalComponents(bundle.Value.Item1.LoadAsset<GameObject>(networkprefab.PrefabPath).transform, ComponentWhitelists.scrapWhitelist);
                                            __instance.GetComponent<NetworkManager>().PrefabHandler.AddNetworkPrefab(prefab);
                                            LethalExpansion.Log.LogInfo($"{networkprefab.PrefabName} Prefab registered.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                }
            }
            /*LethalExpansion.Log.LogInfo("1");
            var objtest = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<GameObject>("Assets/Mods/LethalExpansion/Prefabs/itemshipanimcontainer.prefab");
            GameObject.DontDestroyOnLoad(objtest);
            __instance.GetComponent<NetworkManager>().PrefabHandler.AddNetworkPrefab(objtest);
            LethalExpansion.Log.LogInfo("2");*/
        }
        static void CheckAndRemoveIllegalComponents(Transform prefab, List<Type> whitelist)
        {
            try
            {
                var components = prefab.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (!whitelist.Any(whitelistType => component.GetType() == whitelistType))
                    {
                        LethalExpansion.Log.LogWarning($"Removed illegal {component.GetType().Name} component.");
                        GameObject.Destroy(component);
                    }
                }

                foreach (Transform child in prefab)
                {
                    CheckAndRemoveIllegalComponents(child, whitelist);
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
    }
}
