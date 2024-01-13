using HarmonyLib;
using LethalExpansion.Utils;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRound_Patch
    {
        public static int[] currentWeathers = new int[0];

        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void StartGame_Postfix(StartOfRound __instance)
        {
            if (__instance.currentLevel.name.StartsWith("Assets/Mods/"))
            {
                SceneManager.LoadScene(__instance.currentLevel.name, LoadSceneMode.Additive);
            }
            LethalExpansion.Log.LogInfo("Game started.");
        }
        [HarmonyPatch("OnPlayerConnectedClientRpc")]
        [HarmonyPostfix]
        static void OnPlayerConnectedClientRpc_Postfix(StartOfRound __instance, ulong clientId, int connectedPlayers, ulong[] connectedPlayerIdsOrdered, int assignedPlayerObjectId, int serverMoneyAmount, int levelID, int profitQuota, int timeUntilDeadline, int quotaFulfilled, int randomSeed)
        {
            if (!LethalExpansion.ishost)
            {
                LethalExpansion.ishost = false;
                LethalExpansion.sessionWaiting = false;
                LethalExpansion.Log.LogInfo("LethalExpansion Client Started." + __instance.NetworkManager.LocalClientId);
            }
            else
            {
                NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "clientinfo", string.Empty, (long)clientId);
            }
        }
        [HarmonyPatch(nameof(StartOfRound.SetMapScreenInfoToCurrentLevel))]
        [HarmonyPostfix]
        static void SetMapScreenInfoToCurrentLevel_Postfix(StartOfRound __instance)
        {
            AutoScrollText obj = __instance.screenLevelDescription.GetComponent<AutoScrollText>();
            if (obj != null)
            {
                obj.ResetScrolling();
            }
        }
        [HarmonyPatch(nameof(StartOfRound.SetPlanetsWeather))]
        [HarmonyPrefix]
        static bool SetPlanetsWeather_Prefix(StartOfRound __instance)
        {
            if(__instance.IsHost)
            {
                LethalExpansion.weathersReadyToShare = false;
                return true;
            }
            else
            {
                if (LethalExpansion.alreadypatched)
                {
                    NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "hostweathers", string.Empty, 0);
                }
                return false;
            }
        }
        [HarmonyPatch(nameof(StartOfRound.SetPlanetsWeather))]
        [HarmonyPostfix]
        static void SetPlanetsWeather_Postfix(StartOfRound __instance)
        {
            //if host, share the weathers to clients
            if (__instance.IsHost)
            {
                currentWeathers = new int[__instance.levels.Length];
                string weathers = string.Empty;
                for (int i = 0; i < __instance.levels.Length; i++)
                {
                    currentWeathers[i] = (int)__instance.levels[i].currentWeather;
                    weathers += (int)__instance.levels[i].currentWeather + "&";
                }
                weathers = weathers.Remove(weathers.Length - 1);
                NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.data, "hostweathers", weathers, -1, false);
                LethalExpansion.weathersReadyToShare = true;
            }
        }
        [HarmonyPatch(nameof(StartOfRound.ChangeLevel))]
        [HarmonyPrefix]
        static bool ChangeLevel_Prefix(StartOfRound __instance, ref int levelID)
        {
            //if joining a lobby actually orbiting a modded moon
            if (levelID >= __instance.levels.Length)
            {
                //if is not actually delaying, will temporary show the first moon while it is adding the new ones
                if (LethalExpansion.delayedLevelChange == -1)
                {
                    LethalExpansion.delayedLevelChange = levelID;
                    levelID = 0;
                }
                else
                {
                    LethalExpansion.Log.LogError($"Error loading moon ID {levelID}.");
                    levelID = 0;
                }
            }
            return true;
        }
    }
}
