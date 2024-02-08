using DunGen;
using HarmonyLib;
using LethalExpansion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalExpansion.Patches
{
    public static class SelectableLevel_Extender
    {
        private static readonly ConditionalWeakTable<SelectableLevel, SelectableLevelExtention> extention = new ConditionalWeakTable<SelectableLevel, SelectableLevelExtention>();

        public static void SetFireExitAmountOverwrite(this SelectableLevel level, int value)
        {
            if (!extention.TryGetValue(level, out var data))
            {
                data = new SelectableLevelExtention();
                extention.Add(level, data);
            }

            data.FireExitAmountOverwrite = value;
        }

        public static int GetFireExitAmountOverwrite(this SelectableLevel level)
        {
            if (extention.TryGetValue(level, out var data))
            {
                return data.FireExitAmountOverwrite;
            }

            return 0;
        }

        public static void SetIsFromLE(this SelectableLevel level, bool value)
        {
            if (!extention.TryGetValue(level, out var data))
            {
                data = new SelectableLevelExtention();
                extention.Add(level, data);
            }

            data.isFromLE = value;
        }

        public static bool GetIsFromLE(this SelectableLevel level)
        {
            if (extention.TryGetValue(level, out var data))
            {
                return data.isFromLE;
            }

            return false;
        }

        private class SelectableLevelExtention
        {
            public int FireExitAmountOverwrite { get; set; }
            public bool isFromLE { get; set; } = false;
        }
    }
}
