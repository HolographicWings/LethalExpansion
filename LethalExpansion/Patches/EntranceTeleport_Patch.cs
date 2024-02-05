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
using GameNetcodeStuff;
using DunGen;
using AsmResolver.PE.DotNet.Metadata;
using LethalExpansion.Utils;
using static Netcode.Transports.Facepunch.FacepunchTransport;

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
        [HarmonyPatch(nameof(EntranceTeleport.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(EntranceTeleport __instance)
        {
            if(__instance.NetworkObject != null && __instance.NetworkObject.GetComponent<SI_NetworkData>() != null)
            {
                __instance.gameObject.AddComponent<EntranceTeleport_Extension>();
            }
        }
    }
    public class EntranceTeleport_Extension : MonoBehaviour
    {
        private EntranceTeleport entrance;
        private SI_NetworkData netdata;
        private void Awake()
        {
            entrance = this.GetComponent<EntranceTeleport>();
            if(entrance != null && entrance.NetworkObject != null)
            {
                netdata = entrance.NetworkObject.GetComponent<SI_NetworkData>();
            }
        }
        private void Start()
        {
            if (netdata != null)
            {
                Debug.Log(netdata.IsServer);
                netdata.dataChangeEvent.AddListener(UpdateEntrance);
                if (netdata.IsServer)
                {
                    UpdateEntrance();
                    netdata.datacache = netdata.serializedData;
                }
                else
                {
                    NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "networkobjectdata", string.Join(",", NetworkDataManager.NetworkData.Select(x => x.Key.ToString()).ToArray()), (long)0);
                }
            }
        }
        private void UpdateEntrance()
        {
            if (netdata != null)
            {
                if (netdata.serializedData != null && netdata.serializedData.Length > 0 && netdata.serializedData.Contains(','))
                {
                    StringStringPair[] data = netdata.getData();
                    if (data.Any(e => e._string1.ToLower() == "entranceid"))
                    {
                        int.TryParse(data.First(e => e._string1.ToLower() == "entranceid")._string2, out entrance.entranceId);
                    }
                    if (data.Any(e => e._string1.ToLower() == "audioreverbpreset"))
                    {
                        int.TryParse(data.First(e => e._string1.ToLower() == "audioreverbpreset")._string2, out entrance.audioReverbPreset);
                    }
                }
            }
        }
    }
}
