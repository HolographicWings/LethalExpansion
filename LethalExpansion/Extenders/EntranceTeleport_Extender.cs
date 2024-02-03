using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LethalExpansion.Extenders
{
    public static class EntranceTeleport_Extender
    {
        private static readonly ConditionalWeakTable<EntranceTeleport, EntranceTeleportExtention> extention = new ConditionalWeakTable<EntranceTeleport, EntranceTeleportExtention>();

        public static void SetNetEntranceID(this EntranceTeleport flow, int value)
        {
            if (!extention.TryGetValue(flow, out var data))
            {
                data = new EntranceTeleportExtention();
                extention.Add(flow, data);
            }

            data.Net_EntranceID.Set(value);
        }

        public static int GetNetEntranceID(this EntranceTeleport flow)
        {
            if (extention.TryGetValue(flow, out var data))
            {
                return data.Net_EntranceID.Value;
            }

            return 0;
        }
        public static void SetNetAudioReverbPreset(this EntranceTeleport flow, int value)
        {
            if (!extention.TryGetValue(flow, out var data))
            {
                data = new EntranceTeleportExtention();
                extention.Add(flow, data);
            }

            data.Net_AudioReverbPreset.Set(value);
        }

        public static int GetNetAudioReverbPreset(this EntranceTeleport flow)
        {
            if (extention.TryGetValue(flow, out var data))
            {
                return data.Net_AudioReverbPreset.Value;
            }

            return 0;
        }

        private class EntranceTeleportExtention
        {
            public NetworkVariable<int> Net_EntranceID = new NetworkVariable<int>();
            public NetworkVariable<int> Net_AudioReverbPreset = new NetworkVariable<int>();
        }
    }
}
