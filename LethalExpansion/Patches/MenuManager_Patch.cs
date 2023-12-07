using HarmonyLib;
using LethalExpansion.Utils;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManager_Patch
    {
        [HarmonyPatch(nameof(MenuManager.StartHosting))]
        [HarmonyPostfix]
        public static void StartHosting_Postfix(MenuManager __instance)
        {
            LethalExpansion.ishost = true;
            LethalExpansion.sessionWaiting = false;
            LethalExpansion.Log.LogInfo("LethalExpansion Host Started.");
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
