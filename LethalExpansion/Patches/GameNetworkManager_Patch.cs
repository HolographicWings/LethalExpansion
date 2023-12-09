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

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManager_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void Start_Prefix(GameNetworkManager __instance)
        {
            if (!LethalExpansion.CompatibleGameVersions.Contains(__instance.gameVersionNum))
            {
                LethalExpansion.Log.LogWarning("Warning, this mod is not made for this Game Version, this could cause unexpected behaviors.");
                LethalExpansion.Log.LogWarning(string.Format("Game version: {0}", __instance.gameVersionNum));
                LethalExpansion.Log.LogWarning(string.Format("Compatible mod versions: {0}", string.Join(",", LethalExpansion.CompatibleGameVersions)));
            }
            if (ConfigManager.Instance.FindItemValue<bool>("LoadModules"))
            {
                Sprite scrapSprite = AssetBundlesManager.Instance.mainAssetBundle.LoadAsset<Sprite>("Assets/Mods/LethalExpansion/Sprites/ScrapItemIcon2.png");
                try
                {
                    foreach (KeyValuePair<String,(AssetBundle, ModManifest)> bundle in AssetBundlesManager.Instance.assetBundles)
                    {
                        (AssetBundle, ModManifest) bundle2 = AssetBundlesManager.Instance.Load(bundle.Key);

                        foreach (var newScrap in bundle2.Item2.scraps)
                        {
                            if(newScrap == null)
                            {
                                return;
                            }
                            //GameObject newItem = bundle2.Item1.LoadAsset<GameObject>($"Assets/Mods/{bundle.Key}/{newScrap.ScrapPath}");

                            Item tmpItem = (Item)ScriptableObject.CreateInstance(typeof(Item));

                            tmpItem.name = newScrap.name;
                            tmpItem.itemName = newScrap.itemName;
                            tmpItem.canBeGrabbedBeforeGameStart = true;
                            tmpItem.isScrap = true;
                            tmpItem.minValue = newScrap.minValue;
                            tmpItem.maxValue = newScrap.maxValue;
                            tmpItem.weight = (float)newScrap.weight/100 + 1;
                            tmpItem.spawnPrefab = newScrap.prefab;
                            tmpItem.requiresBattery = newScrap.requiresBattery;
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

                            ScanNodeProperties scanNode = newScrap.prefab.transform.Find("ScanNode").gameObject.AddComponent<ScanNodeProperties>();
                            scanNode.maxRange = 13;
                            scanNode.minRange = 1;
                            scanNode.headerText = newScrap.itemName;
                            scanNode.subText = "Value: ";
                            scanNode.nodeType = 2;

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
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex.Message);
                }
            }
        }
    }
}
