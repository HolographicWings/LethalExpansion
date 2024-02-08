using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LethalExpansion.Extenders
{
    public static class Item_Extender
    {
        private static readonly ConditionalWeakTable<Item, ItemExtention> extention = new ConditionalWeakTable<Item, ItemExtention>();

        public static void SetIsFromLE(this Item level, bool value)
        {
            if (!extention.TryGetValue(level, out var data))
            {
                data = new ItemExtention();
                extention.Add(level, data);
            }

            data.isFromLE = value;
        }

        public static bool GetIsFromLE(this Item level)
        {
            if (extention.TryGetValue(level, out var data))
            {
                return data.isFromLE;
            }

            return false;
        }

        private class ItemExtention
        {
            public bool isFromLE { get; set; } = false;
        }
    }
}
