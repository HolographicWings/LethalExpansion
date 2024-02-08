using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LethalExpansion.Extenders
{
    public static class TerminalKeyword_Extender
    {
        private static readonly ConditionalWeakTable<TerminalKeyword, TerminalKeywordExtention> extention = new ConditionalWeakTable<TerminalKeyword, TerminalKeywordExtention>();

        public static void SetIsFromLE(this TerminalKeyword level, bool value)
        {
            if (!extention.TryGetValue(level, out var data))
            {
                data = new TerminalKeywordExtention();
                extention.Add(level, data);
            }

            data.isFromLE = value;
        }

        public static bool GetIsFromLE(this TerminalKeyword level)
        {
            if (extention.TryGetValue(level, out var data))
            {
                return data.isFromLE;
            }

            return false;
        }

        private class TerminalKeywordExtention
        {
            public bool isFromLE { get; set; }
        }
    }
}
