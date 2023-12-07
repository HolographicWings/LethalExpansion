using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LethalExpansion.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class GrabbableObject_Patch
    {
        [HarmonyPatch(nameof(GrabbableObject.GrabItem))]
        [HarmonyPrefix]
        public static void GrabItem_Postfix(GrabbableObject __instance)
        {
            if (__instance.GetIsOnLandmine())
            {
                __instance.GetLandmine().ItemHeld(__instance);
            }
            LethalExpansion.Log.LogInfo("Item Grabbed.");
        }
    }
    public static class GrabbableObject_Extender
    {
        private static readonly ConditionalWeakTable<GrabbableObject, GrabbableObjectExtention> extention = new ConditionalWeakTable<GrabbableObject, GrabbableObjectExtention>();

        public static void SetLandmine(this GrabbableObject grabbableobject, Landmine value)
        {
            if (!extention.TryGetValue(grabbableobject, out var data))
            {
                data = new GrabbableObjectExtention();
                extention.Add(grabbableobject, data);
            }
            if(value == null)
            {
                data.onLandmine = false;
            }
            else
            {
                data.onLandmine = true;
            }
            data.landmine = value;
        }

        public static Landmine GetLandmine(this GrabbableObject grabbableobject)
        {
            if (extention.TryGetValue(grabbableobject, out var data))
            {
                return data.landmine;
            }

            return null;
        }
        public static bool GetIsOnLandmine(this GrabbableObject grabbableobject)
        {
            if (extention.TryGetValue(grabbableobject, out var data))
            {
                return data.onLandmine;
            }

            return false;
        }

        private class GrabbableObjectExtention
        {
            public bool onLandmine { get; set; } = false;
            public Landmine landmine { get; set; } = null;
        }
    }
}
