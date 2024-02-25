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
using UnityEngine.SceneManagement;
using LethalExpansion.Extenders;
using Unity.Mathematics;

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
                                        NetworkObject no = newScrap.prefab.GetComponent<NetworkObject>();

                                        if (no == null)
                                        {
                                            LethalExpansion.Log.LogWarning(newScrap.itemName + " have no NetworkObject component, skipping...");
                                            continue;
                                        }

                                        if (!checkScrapNetworkObjecValues(new bool[] {
                                                no.AlwaysReplicateAsRoot,
                                                no.SynchronizeTransform,
                                                no.ActiveSceneSynchronization,
                                                no.SceneMigrationSynchronization,
                                                no.SpawnWithObservers,
                                                no.DontDestroyWithOwner,
                                                no.AutoObjectParentSync
                                            }))
                                        {
                                            LethalExpansion.Log.LogWarning(newScrap.itemName + " NetworkObject component is missconfigured, skipping...");
                                            continue;
                                        }

                                        if (newScrap.meshVariants != null && newScrap.meshVariants.Length > 0 && newScrap.prefab.GetComponent<MeshFilter>() == null)
                                        {
                                            LethalExpansion.Log.LogWarning(newScrap.itemName + " have MeshVariants but no MeshFilter in it's parent Object, skipping...");
                                            continue;
                                        }

                                        AudioSource audioSource = newScrap.prefab.AddComponent<AudioSource>();
                                        audioSource.playOnAwake = false;
                                        audioSource.spatialBlend = 1f;

                                        Item tmpItem = ScriptableObject.CreateInstance<Item>();

                                        tmpItem.SetIsFromLE(true);

                                        tmpItem.name = newScrap.name;
                                        tmpItem.itemName = newScrap.itemName;
                                        tmpItem.canBeGrabbedBeforeGameStart = true;
                                        tmpItem.isScrap = true;
                                        tmpItem.minValue = newScrap.minValue;
                                        tmpItem.maxValue = newScrap.maxValue;
                                        tmpItem.weight = (float)newScrap.weight / 100 + 1;

                                        CheckRiskyComponents(newScrap.prefab.transform, ComponentWhitelists.mainWhitelist, tmpItem.itemName, bundle2.Item2.modName);
                                        tmpItem.spawnPrefab = newScrap.prefab;

                                        tmpItem.twoHanded = newScrap.twoHanded;
                                        switch (newScrap.HandedAnimation)
                                        {
                                            case GrabAnim.OneHanded:
                                                tmpItem.twoHandedAnimation = false;
                                                tmpItem.grabAnim = string.Empty;
                                                break;
                                            case GrabAnim.TwoHanded:
                                                tmpItem.twoHandedAnimation = true;
                                                tmpItem.grabAnim = "HoldLung";
                                                break;
                                            case GrabAnim.Shotgun:
                                                tmpItem.twoHandedAnimation = true;
                                                tmpItem.grabAnim = "HoldShotgun";
                                                break;
                                            case GrabAnim.Jetpack:
                                                tmpItem.twoHandedAnimation = true;
                                                tmpItem.grabAnim = "HoldJetpack";
                                                break;
                                            case GrabAnim.Clipboard:
                                                tmpItem.twoHandedAnimation = false;
                                                tmpItem.grabAnim = "GrabClipboard";
                                                break;
                                            default:
                                                tmpItem.twoHandedAnimation = false;
                                                tmpItem.grabAnim = string.Empty;
                                                break;
                                        }
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

                                        switch (newScrap.scrapType)
                                        {
                                            case ScrapType.Normal:
                                                PhysicsProp pp = newScrap.prefab.AddComponent<PhysicsProp>();

                                                pp.grabbable = true;
                                                pp.itemProperties = tmpItem;
                                                pp.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();
                                                pp.useCooldown = newScrap.useCooldown;
                                                break;
                                            case ScrapType.Shovel:
                                                Shovel s = newScrap.prefab.AddComponent<Shovel>();

                                                tmpItem.holdButtonUse = true;

                                                s.grabbable = true;
                                                s.itemProperties = tmpItem;
                                                s.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();

                                                s.useCooldown = newScrap.useCooldown;
                                                s.shovelHitForce = newScrap.shovelHitForce;
                                                s.shovelAudio = newScrap.shovelAudio != null ? newScrap.shovelAudio : newScrap.prefab.GetComponent<AudioSource>();
                                                if (s.shovelAudio == null)
                                                {
                                                    s.shovelAudio = newScrap.prefab.AddComponent<AudioSource>();
                                                }
                                                if (newScrap.prefab.GetComponent<OccludeAudio>() == null)
                                                {
                                                    newScrap.prefab.AddComponent<OccludeAudio>();
                                                }
                                                break;
                                            case ScrapType.Flashlight:
                                                FlashlightItem fi = newScrap.prefab.AddComponent<FlashlightItem>();

                                                fi.grabbable = true;
                                                fi.itemProperties = tmpItem;
                                                fi.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();

                                                fi.useCooldown = newScrap.useCooldown;
                                                fi.usingPlayerHelmetLight = newScrap.usingPlayerHelmetLight;
                                                fi.flashlightInterferenceLevel = newScrap.flashlightInterferenceLevel;
                                                fi.flashlightBulb = newScrap.flashlightBulb;
                                                if (fi.flashlightBulb == null)
                                                {
                                                    fi.flashlightBulb = new Light();
                                                    fi.flashlightBulb.intensity = 0;
                                                }
                                                fi.flashlightBulbGlow = newScrap.flashlightBulbGlow;
                                                if (fi.flashlightBulbGlow == null)
                                                {
                                                    fi.flashlightBulbGlow = new Light();
                                                    fi.flashlightBulbGlow.intensity = 0;
                                                }
                                                fi.flashlightAudio = newScrap.flashlightAudio != null ? newScrap.flashlightAudio : newScrap.prefab.GetComponent<AudioSource>();
                                                if (fi.flashlightAudio == null)
                                                {
                                                    fi.flashlightAudio = newScrap.prefab.AddComponent<AudioSource>();
                                                }
                                                if (newScrap.prefab.GetComponent<OccludeAudio>() == null)
                                                {
                                                    newScrap.prefab.AddComponent<OccludeAudio>();
                                                }
                                                fi.bulbLight = newScrap.bulbLight;
                                                if (fi.bulbLight == null)
                                                {
                                                    fi.bulbLight = new Material(Shader.Find("HDRP/Lit"));
                                                }
                                                fi.bulbDark = newScrap.bulbDark;
                                                if (fi.bulbDark == null)
                                                {
                                                    fi.bulbDark = new Material(Shader.Find("HDRP/Lit"));
                                                }
                                                fi.flashlightMesh = newScrap.flashlightMesh != null ? newScrap.flashlightMesh : fi.mainObjectRenderer;
                                                fi.flashlightTypeID = newScrap.flashlightTypeID;
                                                fi.changeMaterial = newScrap.changeMaterial;
                                                break;
                                            case ScrapType.Noisemaker:
                                                NoisemakerProp np = newScrap.prefab.AddComponent<NoisemakerProp>();

                                                np.grabbable = true;
                                                np.itemProperties = tmpItem;
                                                np.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();

                                                np.useCooldown = newScrap.useCooldown;
                                                np.noiseAudio = newScrap.noiseAudio;
                                                if (np.noiseAudio == null)
                                                {
                                                    np.noiseAudio = newScrap.prefab.AddComponent<AudioSource>();

                                                    // Configure AudioSource
                                                    np.noiseAudio.playOnAwake = false;
                                                    np.noiseAudio.priority = 128;
                                                    np.noiseAudio.pitch = 1f;
                                                    np.noiseAudio.panStereo = 0f;
                                                    np.noiseAudio.spatialBlend = 1f;
                                                    np.noiseAudio.reverbZoneMix = 1f;

                                                    // Configure 3D Sound Settings
                                                    np.noiseAudio.dopplerLevel = 4;
                                                    np.noiseAudio.spread = 26;
                                                    np.noiseAudio.minDistance = 4;
                                                    np.noiseAudio.maxDistance = 21;
                                                    np.noiseAudio.rolloffMode = AudioRolloffMode.Linear;
                                                }
                                                np.noiseAudioFar = newScrap.noiseAudioFar;
                                                if (np.noiseAudioFar == null)
                                                {
                                                    np.noiseAudioFar = newScrap.prefab.AddComponent<AudioSource>();

                                                    // Configure AudioSource
                                                    np.noiseAudioFar.playOnAwake = true;
                                                    np.noiseAudioFar.priority = 128;
                                                    np.noiseAudioFar.pitch = 1f;
                                                    np.noiseAudioFar.panStereo = 0f;
                                                    np.noiseAudioFar.spatialBlend = 1f;
                                                    np.noiseAudioFar.reverbZoneMix = 1f;

                                                    // Configure 3D Sound Settings
                                                    np.noiseAudioFar.dopplerLevel = 1.4f;
                                                    np.noiseAudioFar.spread = 87;
                                                    np.noiseAudioFar.rolloffMode = AudioRolloffMode.Custom;
                                                    np.noiseAudioFar.maxDistance = 75;
                                                    np.noiseAudioFar.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new Keyframe[] {
                                                        new Keyframe(18f, 0f, 0f, 0.065f),
                                                        new Keyframe(25.59f, 0.866f, -0.01f, -0.01f),
                                                        new Keyframe(87f, 0f, -0.018f, 0f)
                                                    }));
                                                }
                                                np.noiseRange = newScrap.noiseRange;
                                                np.maxLoudness = newScrap.maxLoudness;
                                                np.minLoudness = newScrap.minLoudness;
                                                np.minPitch = newScrap.minPitch;
                                                np.maxPitch = newScrap.maxPitch;
                                                np.triggerAnimator = newScrap.triggerAnimator;
                                                np.itemProperties.syncUseFunction = true;
                                                break;
                                            case ScrapType.WhoopieCushion:
                                                WhoopieCushionItem wci = newScrap.prefab.AddComponent<WhoopieCushionItem>();

                                                wci.grabbable = true;
                                                wci.itemProperties = tmpItem;
                                                wci.mainObjectRenderer = newScrap.prefab.GetComponent<MeshRenderer>();

                                                wci.useCooldown = newScrap.useCooldown;
                                                wci.whoopieCushionAudio = newScrap.whoopieCushionAudio != null ? newScrap.whoopieCushionAudio : newScrap.prefab.GetComponent<AudioSource>();
                                                if (wci.whoopieCushionAudio == null)
                                                {
                                                    wci.whoopieCushionAudio = newScrap.prefab.AddComponent<AudioSource>();
                                                }
                                                Transform triggerObject = newScrap.prefab.transform.Find("Trigger");
                                                if (triggerObject == null)
                                                {
                                                    LethalExpansion.Log.LogWarning($"{wci.itemProperties.name} Whoopie Cushion Trigger not found, please add one");
                                                }
                                                else if (triggerObject.gameObject.GetComponent<WhoopieCushionTrigger>() == null)
                                                {
                                                    WhoopieCushionTrigger trigger = triggerObject.gameObject.AddComponent<WhoopieCushionTrigger>();
                                                    trigger.itemScript = wci;
                                                }
                                                break;
                                        }

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
                                            CheckRiskyComponents(bundle.Value.Item1.LoadAsset<GameObject>(networkprefab.PrefabPath).transform, ComponentWhitelists.mainWhitelist, networkprefab.PrefabName, bundle2.Item2.modName);
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
        }
        static readonly bool[] requiredScrapNetworkObjectValues = { false, true, false, true, true, true, false };
        static bool checkScrapNetworkObjecValues(bool[] current)
        {
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != requiredScrapNetworkObjectValues[i])
                {
                    return false;
                }
            }

            return true;
        }
        static void CheckRiskyComponents(Transform prefab, List<Type> whitelist, string objname, string modulename)
        {
            try
            {
                var components = prefab.GetComponents<Component>();
                foreach (var component in components)
                {
                    if (component == null)
                    {
                        LethalExpansion.Log.LogWarning($"{objname} from the {modulename} module contains a component that is not native to Unity or LethalSDK. No associated script could be found from any dll, it will not be loaded.");
                    }
                    else if (!whitelist.Any(whitelistType => component.GetType() == whitelistType))
                    {
                        LethalExpansion.Log.LogWarning($"{component.GetType().Name} component is not native of Unity or LethalSDK. It can contains malwares. From {objname}, {modulename} module.");
                    }
                }

                foreach (Transform child in prefab)
                {
                    CheckRiskyComponents(child, whitelist, objname, modulename);
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex.Message);
            }
        }
    }
}
