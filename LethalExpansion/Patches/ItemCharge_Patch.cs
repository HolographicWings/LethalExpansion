using HarmonyLib;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(ItemCharger))]
    internal class ItemCharge_Patch
    {
        [HarmonyPatch(nameof(ItemCharger.ChargeItem))]
        [HarmonyPrefix]
        public static bool Awake_Prefix(ItemCharger __instance)
        {
            LethalExpansion.Log.LogInfo("Item Charging.");
            return true;
        }
        [HarmonyPatch(nameof(ItemCharger.ChargeItem))]
        [HarmonyPrefix]
        public static void Awake_Postfix(ItemCharger __instance)
        {
            LethalExpansion.Log.LogInfo("Item Charged.");
        }
    }
}