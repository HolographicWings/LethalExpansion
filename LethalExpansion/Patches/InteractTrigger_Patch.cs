using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(InteractTrigger))]
    internal class InteractTrigger_Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start_Postfix(AudioReverbTrigger __instance)
        {
            __instance.gameObject.AddComponent<InteractTrigger_Extension>();
        }
    }
    public class InteractTrigger_Extension : MonoBehaviour
    {
        private InteractTrigger trigger;
        private void Awake()
        {
            trigger = this.GetComponent<InteractTrigger>();
        }
        private void OnTriggerExit(Collider other)
        {
            if (trigger != null)
            {
                if (!trigger.touchTrigger)
                {
                    return;
                }
                PlayerControllerB player = other.gameObject.GetComponent<PlayerControllerB>();
                if (other.gameObject.CompareTag("Player") && player != null && player.IsOwner)
                {
                    trigger.onStopInteract.Invoke(player);
                }
            }
        }
    }
}
