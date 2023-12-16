using HarmonyLib;
using LethalExpansion.Utils;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRound_Patch
    {
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPostfix]
        public static void Awake_Postfix(StartOfRound __instance)
        {
            if (__instance.currentLevel.name.StartsWith("Assets/Mods/"))
            {
                SceneManager.LoadScene(__instance.currentLevel.name, LoadSceneMode.Additive);
            }
            LethalExpansion.Log.LogInfo("Game started.");
        }
        [HarmonyPatch("OnPlayerConnectedClientRpc")]
        [HarmonyPostfix]
        static void OnPlayerConnectedClientRpc_Postfix(StartOfRound __instance, ulong clientId)
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
