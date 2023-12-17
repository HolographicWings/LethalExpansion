using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalExpansion.Utils
{
    internal class NetworkPacketManager
    {
        private static NetworkPacketManager _instance;
        private NetworkPacketManager() { }
        public static NetworkPacketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetworkPacketManager();
                }
                return _instance;
            }
        }
        public void sendPacket(packetType type, string header, string packet, long destination = -1, bool waitForAnswer = true)
        {
            HUDManager.Instance.AddTextToChatOnServer($"[sync]{(int)type}|{RoundManager.Instance.NetworkManager.LocalClientId}>{destination}|{header}={packet}[sync]");
            if (waitForAnswer && RoundManager.Instance.NetworkManager.IsHost && ConfigManager.Instance.FindItemValue<bool>("LoadModules") || ConfigManager.Instance.FindItemValue<bool>("KickPlayerWithoutMod"))
            {
                StartTimeout(destination);
            }
        }
        public void sendPacket(packetType type, string header, string packet, long[] destinations, bool waitForAnswer = true)
        {
            foreach (int destination in destinations)
            {
                if(destination != -1)
                {
                    HUDManager.Instance.AddTextToChatOnServer($"[sync]{(int)type}|{RoundManager.Instance.NetworkManager.LocalClientId}>{destination}|{header}={packet}[sync]");
                    if (waitForAnswer && RoundManager.Instance.NetworkManager.IsHost && ConfigManager.Instance.FindItemValue<bool>("LoadModules") || ConfigManager.Instance.FindItemValue<bool>("KickPlayerWithoutMod"))
                    {
                        StartTimeout(destination);
                    }
                }
            }
        }
        private ConcurrentDictionary<long, CancellationTokenSource> timeoutDictionary = new ConcurrentDictionary<long, CancellationTokenSource>();

        public void StartTimeout(long id)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            if (timeoutDictionary.TryAdd(id, cancellationTokenSource))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await PacketTimeout(id, cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {

                    }
                    finally
                    {
                        timeoutDictionary.TryRemove(id, out _);
                    }
                });
            }
        }

        public void CancelTimeout(long id)
        {
            if (timeoutDictionary.TryRemove(id, out CancellationTokenSource cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }
        }

        private async Task PacketTimeout(long id, CancellationToken token)
        {
            await Task.Delay(5000, token);
            if (!token.IsCancellationRequested)
            {
                /*LethalExpansion.Log.LogWarning($"Kicking {id} for timeout.");
                NetworkPacketManager.Instance.sendPacket(packetType.data, "kickreason", "Packet timeout.", id);
                StartOfRound.Instance.KickPlayer(StartOfRound.Instance.ClientPlayerList[(ulong)id]);*/
            }
        }
        public enum packetType
        {
            request = 0,
            data = 1,
            other = -1
        }
    }
}
