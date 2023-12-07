using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRound_Patch
    {
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        [HarmonyPrefix]
        public static bool Awake_Prefix(StartOfRound __instance)
        {
            LethalExpansion.Log.LogInfo("Game starting.");
            return true;
        }

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
        static void OnPlayerConnectedClientRpc_Postfix(StartOfRound __instance)
        {
            if (!LethalExpansion.ishost)
            {
                LethalExpansion.ishost = false;
                LethalExpansion.sessionWaiting = false;
                LethalExpansion.Log.LogInfo("LethalExpansion Client Started.");
            }
        }
    }
}
