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
    }
}
