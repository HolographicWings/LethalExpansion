using HarmonyLib;
using LethalExpansion.Utils;
using UnityEngine;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManager_Patch
    {
        [HarmonyPatch("AddChatMessage")]
        [HarmonyPrefix]
        static bool ChatInterpreter(HUDManager __instance, string chatMessage)
        {
            if (ChatMessageProcessor.ProcessMessage(chatMessage))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update_Postfix_Patch(HUDManager __instance)
        {
            if(GameNetworkManager.Instance.localPlayerController != null)
            {
                float num2 = (float)Mathf.RoundToInt(Mathf.Clamp(GameNetworkManager.Instance.localPlayerController.carryWeight - 1f, 0f, 100f) * 105f);
                switch (ConfigManager.Instance.FindItemValue<int>("WeightUnit"))
                {
                    case 1:
                        __instance.weightCounter.text = string.Format("{0} kg", ConfigManager.Instance.FindItemValue<bool>("ConvertPoundsToKilograms") ? Mathf.RoundToInt(num2 * 0.4536f) : num2);
                        break;
                    case 2:
                        __instance.weightCounter.text = string.Format("{0} kg\n{1} lb", Mathf.RoundToInt(num2 * 0.4536f), num2);
                        break;
                    default:
                        break;
                }
            }
        }
        [HarmonyPatch(nameof(HUDManager.SetClock))]
        [HarmonyPrefix]
        public static bool SetClock_Prefix_Patch(HUDManager __instance, ref string __result, float timeNormalized, float numberOfHours, bool createNewLine)
        {
            string newLine;
            if (ConfigManager.Instance.FindItemValue<bool>("24HoursClock"))
            {
                int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
                int num2 = (int)Mathf.Floor((float)(num / 60));
                num2 %= 24;
                if (!createNewLine)
                {
                    newLine = " ";
                }
                else
                {
                    newLine = "\n";
                }
                int num3 = num % 60;
                string text = string.Format("{0:0}:{1:00}", num2, num3);
                __instance.clockNumber.text = text;
                __result = text;
            }
            else
            {
                string amPM;
                int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
                int num2 = (int)Mathf.Floor((float)(num / 60));
                num2 %= 24;
                if (!createNewLine)
                {
                    newLine = " ";
                }
                else
                {
                    newLine = "\n";
                }
                amPM = newLine + "AM";
                if (num2 < 12)
                {
                    amPM = newLine + "AM";
                }
                else
                {
                    amPM = newLine + "PM";
                }
                if (num2 > 12)
                {
                    num2 %= 12;
                }
                int num3 = num % 60;
                string text = string.Format("{0:0}:{1:00}", num2, num3) + amPM;
                __instance.clockNumber.text = text;
                __result = text;
            }
            return false;
        }
        [HarmonyPatch(nameof(HUDManager.SetClockVisible))]
        [HarmonyPostfix]
        public static void SetClockVisible_Postfix_Patch(HUDManager __instance)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("ClockAlwaysVisible"))
            {
                __instance.Clock.targetAlpha = 1f;
            }
        }
    }
}
