using HarmonyLib;
using LethalExpansion.Utils;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManager_Patch
    {
        [HarmonyPatch(nameof(MenuManager.StartHosting))]
        [HarmonyPrefix]
        public static bool StartHosting_Prefix(MenuManager __instance)
        {
            LethalExpansion.Log.LogInfo("LethalExpansion Starting Host.");
            return true;
        }

        [HarmonyPatch(nameof(MenuManager.StartHosting))]
        [HarmonyPostfix]
        public static void StartHosting_Postfix(MenuManager __instance)
        {
            LethalExpansion.ishost = true;
            LethalExpansion.sessionWaiting = false;
            LethalExpansion.Log.LogInfo("LethalExpansion Host Started.");
        }
        [HarmonyPatch(nameof(MenuManager.StartAClient))]
        [HarmonyPrefix]
        public static bool StartAClient_Prefix(MenuManager __instance)
        {
            LethalExpansion.Log.LogInfo("LethalExpansion Starting LAN Client.");
            return true;
        }

        [HarmonyPatch(nameof(MenuManager.StartAClient))]
        [HarmonyPostfix]
        public static void StartAClient_Postfix(MenuManager __instance)
        {
            LethalExpansion.ishost = false;
            LethalExpansion.sessionWaiting = false;
            LethalExpansion.Log.LogInfo("LethalExpansion LAN Client Started.");
        }
    }
}
