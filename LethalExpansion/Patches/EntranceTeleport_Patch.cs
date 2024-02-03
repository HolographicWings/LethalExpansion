using HarmonyLib;
using LethalSDK.Component;
using LethalSDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LethalExpansion.Extenders;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    public class EntranceTeleport_Patch
    {
        [HarmonyPatch("SetAudioPreset")]
        [HarmonyPrefix]
        public static bool SetAudioPreset_Prefix(EntranceTeleport __instance, int playerObj)
        {
            //avoid error when the moon don't contain any Audio Reverb Presets and the player try to enter or leave the dungeon
            if (GameObject.FindObjectOfType<AudioReverbPresets>() == null)
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
            //disable the water surfaces when entering in the dungeon and enable them back when leaving the dungeon
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
        /*[HarmonyPatch(nameof(EntranceTeleport.Awake))]
        [HarmonyPrefix]
        public static bool Awake_Prefix(EntranceTeleport __instance)
        {
            if (__instance.NetworkManager.IsHost)
            {
                __instance.SetNetEntranceID(__instance.entranceId);
                __instance.SetNetAudioReverbPreset(__instance.audioReverbPreset);
            }
            else
            {
                __instance.entranceId = __instance.GetNetEntranceID();
                __instance.audioReverbPreset = __instance.GetNetAudioReverbPreset();
            }
            return true;
        }*/
    }
}
