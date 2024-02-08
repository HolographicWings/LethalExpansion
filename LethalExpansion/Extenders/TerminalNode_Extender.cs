using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LethalExpansion.Extenders
{
    public static class TerminalNode_Extender
    {
        private static readonly ConditionalWeakTable<TerminalNode, TerminalNodeExtention> extention = new ConditionalWeakTable<TerminalNode, TerminalNodeExtention>();

        public static void SetIsFromLE(this TerminalNode level, bool value)
        {
            if (!extention.TryGetValue(level, out var data))
            {
                data = new TerminalNodeExtention();
                extention.Add(level, data);
            }

            data.isFromLE = value;
        }

        public static bool GetIsFromLE(this TerminalNode level)
        {
            if (extention.TryGetValue(level, out var data))
            {
                return data.isFromLE;
            }

            return false;
        }

        private class TerminalNodeExtention
        {
            public bool isFromLE { get; set; } = false;
        }
    }
}
