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
    internal class GameNetworkManager_Patch
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
                            if (newScrap == null)
                            {
                                break;
                            }

                            Item tmpItem = ScriptableObject.CreateInstance<Item>();

                            tmpItem.name = newScrap.name;
                            tmpItem.itemName = newScrap.itemName;
                            tmpItem.canBeGrabbedBeforeGameStart = true;
                            tmpItem.isScrap = true;
                            tmpItem.minValue = newScrap.minValue;
                            tmpItem.maxValue = newScrap.maxValue;
                            tmpItem.weight = (float)newScrap.weight/100 + 1;

                            CheckAndRemoveIllegalComponents(newScrap.prefab.transform);
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

                            if (newScrap.prefab == null)
                            {
                                break;
                            }
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
        private static List<Type> whitelist = new List<Type> {
            //Base
            typeof(Transform),
            //Mesh
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer),
            //Physics
            typeof(MeshCollider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(SphereCollider),
            typeof(TerrainCollider),
            typeof(WheelCollider),
            typeof(ArticulationBody),
            typeof(ConstantForce),
            typeof(ConfigurableJoint),
            typeof(FixedJoint),
            typeof(HingeJoint),
            typeof(Cloth),
            typeof(Rigidbody),
            //Netcode
            typeof(NetworkObject),
            typeof(NetworkRigidbody),
            typeof(NetworkTransform),
            typeof(NetworkAnimator),
            //Animation
            typeof(Animator),
            typeof(Animation),
            //Rendering
            typeof(DecalProjector),
            typeof(LODGroup),
            typeof(Light),
            typeof(HDAdditionalLightData),
            typeof(LightProbeGroup),
            typeof(LightProbeProxyVolume),
            typeof(LocalVolumetricFog),
            typeof(OcclusionArea),
            typeof(OcclusionPortal),
            typeof(ReflectionProbe),
            typeof(PlanarReflectionProbe),
            typeof(HDAdditionalReflectionData),
            typeof(SortingGroup),
            typeof(SpriteRenderer),
            //Audio
            typeof(AudioSource),
            typeof(AudioReverbZone),
            typeof(AudioReverbFilter),
            typeof(AudioChorusFilter),
            typeof(AudioDistortionFilter),
            typeof(AudioEchoFilter),
            typeof(AudioHighPassFilter),
            typeof(AudioLowPassFilter),
            typeof(AudioListener),
            //Effect
            typeof(LensFlare),
            typeof(TrailRenderer),
            typeof(LineRenderer),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(ParticleSystemForceField),
            //Video
            typeof(VideoPlayer)
        };
        static void CheckAndRemoveIllegalComponents(Transform prefab)
        {
            var components = prefab.GetComponents<Component>();
            foreach (var component in components)
            {
                if (!whitelist.Any(whitelistType => component.GetType() == whitelistType))
                {
                    LethalExpansion.Log.LogWarning($"Removed illegal {component.GetType().Name} component.");
                    GameObject.DestroyImmediate(component);
                }
            }

            foreach (Transform child in prefab)
            {
                CheckAndRemoveIllegalComponents(child);
            }
        }
    }
}
