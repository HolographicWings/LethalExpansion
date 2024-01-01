using HarmonyLib;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using LethalExpansion.Utils;
using DunGen;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManager_Patch
    {
        [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
        [HarmonyPrefix]
        public static bool DespawnPropsAtEndOfRound_Prefix(RoundManager __instance, bool despawnAllItems)
        {
            LethalExpansion.Log.LogInfo("Despawn all items.");
            if (ConfigManager.Instance.FindItemValue<bool>("PreventScrapWipeWhenAllPlayersDie"))
            {
                if (!__instance.IsServer)
                {
                    return false;
                }
                GrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();

                for (int i = 0; i < array.Length; i++)
                {
                    if (despawnAllItems || (!array[i].isHeld && !array[i].isInShipRoom))
                    {
                        if (array[i].isHeld && array[i].playerHeldBy != null)
                        {
                            array[i].playerHeldBy.DropAllHeldItems(true, false);
                        }
                        array[i].gameObject.GetComponent<NetworkObject>().Despawn(true);
                    }
                    else
                    {
                        array[i].scrapPersistedThroughRounds = true;
                    }
                    if (__instance.spawnedSyncedObjects.Contains(array[i].gameObject))
                    {
                        __instance.spawnedSyncedObjects.Remove(array[i].gameObject);
                    }
                }
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(RoundManager.SpawnMapObjects))]
        [HarmonyPrefix]
        public static bool SpawnMapObjects_Prefix(RoundManager __instance)
        {
            if (__instance.currentLevel.spawnableMapObjects == null)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(RoundManager.PlotOutEnemiesForNextHour))]
        [HarmonyPrefix]
        public static bool PlotOutEnemiesForNextHour_Prefix(RoundManager __instance)
        {
            if (__instance.currentLevel.enemySpawnChanceThroughoutDay == null)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(RoundManager.SpawnEnemiesOutside))]
        [HarmonyPrefix]
        public static bool SpawnEnemiesOutside_Prefix(RoundManager __instance)
        {
            if (__instance.currentLevel.outsideEnemySpawnChanceThroughDay == null)
            {
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(RoundManager.SpawnDaytimeEnemiesOutside))]
        [HarmonyPrefix]
        public static bool SpawnDaytimeEnemiesOutside_Prefix(RoundManager __instance)
        {
            if (__instance.currentLevel.daytimeEnemySpawnChanceThroughDay == null)
            {
                return false;
            }
            return true;
        }
        private static bool zeroQuotaWorkaround = false;
        [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        public static bool AdvanceHourAndSpawnNewBatchOfEnemies_Prefix(RoundManager __instance)
        {
            if (TimeOfDay.Instance.profitQuota == 0)
            {
                TimeOfDay.Instance.profitQuota = 1;
                zeroQuotaWorkaround = true;
            }
            else
            {
                zeroQuotaWorkaround = false;
            }
            return true;
        }
        [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPostfix]
        public static void AdvanceHourAndSpawnNewBatchOfEnemies_Postfix(RoundManager __instance)
        {
            if (zeroQuotaWorkaround)
            {
                TimeOfDay.Instance.profitQuota = 0;
                zeroQuotaWorkaround = false;
            }
        }
        [HarmonyPatch(nameof(RoundManager.GenerateNewFloor))]
        [HarmonyPrefix]
        public static bool GenerateNewFloor_Prefix(RoundManager __instance)
        {
            if (!__instance.hasInitializedLevelRandomSeed)
            {
                __instance.hasInitializedLevelRandomSeed = true;
                __instance.InitializeRandomNumberGenerators();
            }
            if (__instance.currentLevel.dungeonFlowTypes != null && __instance.currentLevel.dungeonFlowTypes.Length != 0)
            {
                List<int> list = new List<int>();
                for (int i = 0; i < __instance.currentLevel.dungeonFlowTypes.Length; i++)
                {
                    list.Add(__instance.currentLevel.dungeonFlowTypes[i].rarity);
                }
                int id = __instance.currentLevel.dungeonFlowTypes[__instance.GetRandomWeightedIndex(list.ToArray(), __instance.LevelRandom)].id;
                __instance.dungeonGenerator.Generator.DungeonFlow = __instance.dungeonFlowTypes[id];

                if (RoundManager.Instance.currentLevel.GetFireExitAmountOverwrite() != 0)
                {
                    RoundManager.Instance.dungeonGenerator.Generator.DungeonFlow.GlobalProps.First(p => p.ID == 1231).Count = new IntRange(RoundManager.Instance.currentLevel.GetFireExitAmountOverwrite(), RoundManager.Instance.currentLevel.GetFireExitAmountOverwrite());
                }

                if (id < __instance.firstTimeDungeonAudios.Length && __instance.firstTimeDungeonAudios[id] != null)
                {
                    EntranceTeleport[] array = GameObject.FindObjectsOfType<EntranceTeleport>();
                    if (array != null && array.Length != 0)
                    {
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j].isEntranceToBuilding)
                            {
                                array[j].firstTimeAudio = __instance.firstTimeDungeonAudios[id];
                                array[j].dungeonFlowId = id;
                            }
                        }
                    }
                }
            }
            __instance.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
            __instance.dungeonGenerator.Generator.Seed = __instance.LevelRandom.Next();
            Debug.Log(string.Format("GenerateNewFloor(). Map generator's random seed: {0}", __instance.dungeonGenerator.Generator.Seed));
            __instance.dungeonGenerator.Generator.LengthMultiplier = __instance.currentLevel.factorySizeMultiplier * __instance.mapSizeMultiplier;
            __instance.dungeonGenerator.Generate();

            return false;
        }
    }
}
