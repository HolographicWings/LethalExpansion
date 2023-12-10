using HarmonyLib;
using LethalSDK.Component;
using LethalSDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    public class EntranceTeleport_Patch
    {
        [HarmonyPatch("SetAudioPreset")]
        [HarmonyPrefix]
        public static bool SetAudioPreset_Prefix(EntranceTeleport __instance, int playerObj)
        {
            if(GameObject.FindObjectOfType<AudioReverbPresets>() == null)
            {
                __instance.PlayAudioAtTeleportPositions();
                return false;
            }
            if(GameObject.FindObjectOfType<AudioReverbPresets>().audioPresets.Length <= __instance.audioReverbPreset)
            {
                __instance.PlayAudioAtTeleportPositions();
                return false;
            }
            return true;
        }
        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        [HarmonyPostfix]
        public static void TeleportPlayer_Postfix(EntranceTeleport __instance)
        {
            var water = GameObject.FindObjectOfType<SI_WaterSurface>();
            if(water != null)
            {
                if (__instance.isEntranceToBuilding)
                {
                    water.GetComponentInChildren<AudioSource>().enabled = false;
                }
                else
                {
                    water.GetComponentInChildren<AudioSource>().enabled = true;
                }
            }
        }
    }
}
