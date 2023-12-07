using HarmonyLib;
using LethalExpansion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDay_Patch
    {
        [HarmonyPatch(nameof(TimeOfDay.SyncNewProfitQuotaClientRpc))]
        [HarmonyPostfix]
        public static void SyncNewProfitQuotaClientRpc_Postfix(TimeOfDay __instance)
        {
            LethalExpansion.Log.LogInfo("New deadline.");
            if (ConfigManager.Instance.FindItemValue<bool>("AutomaticDeadline"))
            {
                __instance.quotaVariables.deadlineDaysAmount = ConfigManager.Instance.FindItemValue<int>("DeadlineDaysAmount") + __instance.profitQuota / ConfigManager.Instance.FindItemValue<int>("AutomaticDeadlineStage");
                __instance.timeUntilDeadline = __instance.totalTime * __instance.quotaVariables.deadlineDaysAmount;
            }
        }
    }
}
