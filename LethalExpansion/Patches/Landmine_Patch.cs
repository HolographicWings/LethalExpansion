using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using LethalExpansion.Utils;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class Landmine_Patch
    {
        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        static bool LandmineOnTriggerEnter_Prefix(Landmine __instance, Collider other)
        {
            if (ConfigManager.Instance.FindItemValue<bool>("PreventMineToExplodeWithItems"))
            {
                if (other.CompareTag("PlayerRagdoll") || other.CompareTag("PhysicsProp"))
                {
                    if (other.GetComponent<GrabbableObject>())
                    {
                        if (other.GetComponent<GrabbableObject>().itemProperties.weight - 1 < ConfigManager.Instance.FindItemValue<float>("MineActivationWeight"))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPostfix]
        static void LandmineOnTriggerEnter_Postfix(Landmine __instance, Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerRagdoll") || other.CompareTag("PhysicsProp"))
            {
                if (other.GetComponent<PlayerControllerB>() || other.GetComponent<DeadBodyInfo>())
                {
                    __instance.AddWeight(2f);
                }
                else if (other.GetComponent<GrabbableObject>())
                {
                    __instance.AddWeight(other.GetComponent<GrabbableObject>().itemProperties.weight - 1);
                    other.GetComponent<GrabbableObject>().SetLandmine(__instance);
                }
            }
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        static bool LandmineOnTriggerExit_Prefix(Landmine __instance, Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerRagdoll") || other.CompareTag("PhysicsProp"))
            {
                if (other.GetComponent<PlayerControllerB>() || other.GetComponent<DeadBodyInfo>())
                {
                    __instance.RemWeight(2f);
                }
                else if (other.GetComponent<PhysicsProp>())
                {
                    __instance.RemWeight(other.GetComponent<PhysicsProp>().itemProperties.weight - 1);
                }
            }
            if (ConfigManager.Instance.FindItemValue<bool>("PreventMineToExplodeWithItems"))
            {
                if (__instance.GetWeight() >= ConfigManager.Instance.FindItemValue<float>("MineActivationWeight"))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public static class Landmine_Extender
    {
        private static readonly ConditionalWeakTable<Landmine, LandmineExtention> extention = new ConditionalWeakTable<Landmine, LandmineExtention>();
        
        public static void SetWeight(this Landmine landmine, float value)
        {
            if (!extention.TryGetValue(landmine, out var data))
            {
                data = new LandmineExtention();
                extention.Add(landmine, data);
            }

            data.Weight = value;
        }
        public static void AddWeight(this Landmine landmine, float value)
        {
            if (!extention.TryGetValue(landmine, out var data))
            {
                data = new LandmineExtention();
                extention.Add(landmine, data);
            }

            data.Weight += value;
            data.Weight = (float)Math.Round(data.Weight, 2);
        }
        public static void RemWeight(this Landmine landmine, float value)
        {
            if (!extention.TryGetValue(landmine, out var data))
            {
                data = new LandmineExtention();
                extention.Add(landmine, data);
            }

            data.Weight -= value;
            data.Weight = (float)Math.Round(data.Weight, 2);
        }

        public static float GetWeight(this Landmine landmine)
        {
            if (extention.TryGetValue(landmine, out var data))
            {
                data.Weight = (float)Math.Round(data.Weight, 2);
                return data.Weight;
            }

            return 0;
        }
        public static void ItemHeld(this Landmine __instance, GrabbableObject other)
        {
            LethalExpansion.Log.LogInfo("Item removed from mine.");
            if (other.CompareTag("PhysicsProp"))
            {
                if (other.GetComponent<GrabbableObject>())
                {
                    __instance.RemWeight(other.itemProperties.weight - 1);
                }
            }
            if (ConfigManager.Instance.FindItemValue<bool>("PreventMineToExplodeWithItems"))
            {
                if (__instance.GetWeight() < ConfigManager.Instance.FindItemValue<float>("MineActivationWeight"))
                {
                    __instance.SetOffMineAnimation();
                    var field = AccessTools.Field(typeof(Landmine), "sendingExplosionRPC");
                    Console.WriteLine(field.GetValue(__instance));
                    field.SetValue(__instance, (bool)true);
                    Console.WriteLine(field.GetValue(__instance));
                    __instance.ExplodeMineServerRpc();
                }
            }
        }

        private class LandmineExtention
        {
            public float Weight { get; set; }
        }
    }
}