using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LethalExpansion.Extenders
{
    public static class DungeonFlow_Extender
    {
        private static readonly ConditionalWeakTable<DungeonFlow, DungeonFlowExtention> extention = new ConditionalWeakTable<DungeonFlow, DungeonFlowExtention>();

        public static void SetDefaultFireExitAmount(this DungeonFlow flow, int value)
        {
            if (!extention.TryGetValue(flow, out var data))
            {
                data = new DungeonFlowExtention();
                extention.Add(flow, data);
            }

            data.DefaultFireExitAmount = value;
        }

        public static int GetDefaultFireExitAmount(this DungeonFlow flow)
        {
            if (extention.TryGetValue(flow, out var data))
            {
                return data.DefaultFireExitAmount;
            }

            return 0;
        }

        private class DungeonFlowExtention
        {
            public int DefaultFireExitAmount { get; set; }
        }
    }
}
