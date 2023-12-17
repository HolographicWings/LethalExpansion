using HarmonyLib;
using LethalExpansion.Utils;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static LethalExpansion.Utils.NetworkPacketManager;
using static UnityEngine.Rendering.HighDefinition.ScalableSettingLevelParameter;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRound_Patch
    {
        public static int[] currentWeathers = new int[0];
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake_Postfix(StartOfRound __instance)
        {
            LethalExpansion.Log.LogInfo(__instance.randomMapSeed);
            __instance.randomMapSeed = 464657;
            LethalExpansion.Log.LogInfo(__instance.randomMapSeed);
        }
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
                NetworkPacketManager.Instance.sendPacket(packetType.data, "hostweathers", weathers, -1, false);
                LethalExpansion.weathersReadyToShare = true;
            }
        }
        [HarmonyPatch(nameof(StartOfRound.ChangeLevel))]
        [HarmonyPrefix]
        static bool ChangeLevel_Prefix(StartOfRound __instance, ref int levelID)
        {
            if (levelID >= __instance.levels.Length)
            {
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
        /*[HarmonyPatch(nameof(StartOfRound.KickPlayer))]
        [HarmonyPrefix]
        public static bool KickPlayer_Prefix(StartOfRound __instance, int playerObjToKick)
        {
            LethalExpansion.Log.LogError(__instance.allPlayerScripts.Length);
            foreach (var playerObj in __instance.allPlayerScripts)
            {
                LethalExpansion.Log.LogError(playerObj.name);
            }
            LethalExpansion.Log.LogError(playerObjToKick);
            LethalExpansion.Log.LogError(__instance.allPlayerScripts[playerObjToKick].actualClientId);
            LethalExpansion.Log.LogError(__instance.allPlayerScripts[playerObjToKick].NetworkObjectId);
            LethalExpansion.Log.LogError(__instance.allPlayerScripts[playerObjToKick].OwnerClientId);
            LethalExpansion.Log.LogError("1");
            if (!__instance.allPlayerScripts[playerObjToKick].isPlayerControlled && !__instance.allPlayerScripts[playerObjToKick].isPlayerDead)
            {
                LethalExpansion.Log.LogError("2");
                return false;
            }
            if (!__instance.IsServer)
            {
                LethalExpansion.Log.LogError("3");
                return false;
            }
            LethalExpansion.Log.LogError("4");
            if (!GameNetworkManager.Instance.disableSteam)
            {
                LethalExpansion.Log.LogError("5");
                ulong playerSteamId = StartOfRound.Instance.allPlayerScripts[playerObjToKick].playerSteamId;
                LethalExpansion.Log.LogError("6");
                if (!__instance.KickedClientIds.Contains(playerSteamId))
                {
                    LethalExpansion.Log.LogError("7");
                    __instance.KickedClientIds.Add(playerSteamId);
                }
            }
            LethalExpansion.Log.LogError("8");
            try
            {
                NetworkManager.Singleton.DisconnectClient(__instance.allPlayerScripts[playerObjToKick].actualClientId);
            }
            catch(Exception ex)
            {
                LethalExpansion.Log.LogError(ex);
            }
            LethalExpansion.Log.LogError("9");
            HUDManager.Instance.AddTextToChatOnServer(string.Format("[playerNum{0}] was kicked.", playerObjToKick), -1);
            LethalExpansion.Log.LogError("10");
            return false;
        }*/
    }
}
