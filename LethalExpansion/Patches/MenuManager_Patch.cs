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

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake_Postfix(MenuManager __instance)
        {
            if(__instance.versionNumberText != null)
            {
                __instance.versionNumberText.enableWordWrapping = false;
                __instance.versionNumberText.text += $"     LEv{LethalExpansion.ModVersion.ToString()}";
            }
        }
    }
}
