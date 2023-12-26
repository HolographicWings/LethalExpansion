using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(AudioReverbTrigger))]
    public class AudioReverbTrigger_Patch
    {
        [HarmonyPatch(nameof(AudioReverbTrigger.ChangeAudioReverbForPlayer))]
        [HarmonyPostfix]
        public static void SChangeAudioReverbForPlayer_Postfix(AudioReverbTrigger __instance)
        {
            if(LethalExpansion.currentWaterSurface != null)
            {
                LethalExpansion.currentWaterSurface.gameObject.SetActive(!__instance.disableAllWeather);
            }
        }
    }
}
