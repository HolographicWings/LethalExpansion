using HarmonyLib;
using LethalExpansion.Patches;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Rendering.HighDefinition;
using static LethalExpansion.Utils.NetworkPacketManager;

namespace LethalExpansion.Utils
{
    public class ChatMessageProcessor
    {
        public static bool ProcessMessage(string message)
        {
            //packets begins and ends by [sync], in that case the packet will not be shown in the chat
            if (Regex.IsMatch(message, @"^\[sync\].*\[sync\]$"))
            {
                try
                {
                    //cut the [sync] to get the content of the packet
                    string content = Regex.Match(message, @"^\[sync\](.*)\[sync\]$").Groups[1].Value;

                    string[] parts = content.Split('|');
                    if (parts.Length == 3)
                    {
                        packetType type = (packetType)int.Parse(parts[0]);
                        string[] mid = parts[1].Split('>');
                        ulong sender = ulong.Parse(mid[0]);
                        long destination = long.Parse(mid[1]);
                        string[] last = parts[2].Split('=');
                        string header = last[0];
                        string packet = last[1];

                        if (destination == -1 || (ulong)destination == RoundManager.Instance.NetworkManager.LocalClientId)
                        {
                            if (sender != 0)
                            {
                                NetworkPacketManager.Instance.CancelTimeout((long)sender);
                            }
                            LethalExpansion.Log.LogInfo(message);
                            switch (type)
                            {
                                case packetType.request:
                                    ProcessRequest(sender, header, packet);
                                    break;
                                case packetType.data:
                                    ProcessData(sender, header, packet);
                                    break;
                                case packetType.other:
                                    LethalExpansion.Log.LogInfo("Unsupported type.");
                                    break;
                                default:
                                    LethalExpansion.Log.LogInfo("Unrecognized type.");
                                    break;
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    LethalExpansion.Log.LogError(ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private static void ProcessRequest(ulong sender, string header, string packet)
        {
            try
            {
                switch (header)
                {
                    case "clientinfo": //client receive info request from host
                        if (!LethalExpansion.ishost && sender == 0)
                        {
                            string configPacket = $"{LethalExpansion.ModVersion.ToString()}$";
                            foreach (var bundle in AssetBundlesManager.Instance.assetBundles)
                            {
                                configPacket += $"{bundle.Key}v{bundle.Value.Item2.GetVersion().ToString()}&";
                            }
                            if (configPacket.EndsWith('&'))
                            {
                                configPacket = configPacket.Remove(configPacket.Length - 1);
                            }
                            NetworkPacketManager.Instance.sendPacket(packetType.data, "clientinfo", configPacket, 0);
                        }
                        break;
                    case "hostconfig": //host receive config request from client
                        if (LethalExpansion.ishost && sender != 0)
                        {
                            NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "clientinfo", string.Empty, (long)sender);
                        }
                        break;
                    case "hostweathers": //host receive weather request from client
                        if (LethalExpansion.ishost && sender != 0 && LethalExpansion.weathersReadyToShare)
                        {
                            string weathers = string.Empty;
                            foreach (var weather in StartOfRound_Patch.currentWeathers)
                            {
                                weathers += weather + "&";
                            }
                            if (weathers.EndsWith('&'))
                            {
                                weathers = weathers.Remove(weathers.Length - 1);
                            }
                            NetworkPacketManager.Instance.sendPacket(packetType.data, "hostweathers", weathers, (long)sender, false);
                        }
                        break;
                    case "networkobjectdata": //host receive network object data request from client
                        if (LethalExpansion.ishost && sender != 0)
                        {
                            string data = string.Empty;
                            if(packet.Length > 0 && packet.Contains(','))
                            {
                                try
                                {
                                    ulong[] objectids = Array.ConvertAll(packet.Split(','), ulong.Parse);
                                    foreach (ulong id in objectids)
                                    {
                                        data += $"{id}${LethalSDK.Utils.NetworkDataManager.NetworkData[id].serializedData}&";
                                    }
                                    if (data.EndsWith('&'))
                                    {
                                        data = data.Remove(data.Length - 1);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LethalExpansion.Log.LogError(ex);
                                    data = "error";
                                }
                                NetworkPacketManager.Instance.sendPacket(packetType.data, "networkobjectdata", data, (long)sender, false);
                            }
                            else
                            {
                                LethalExpansion.Log.LogError("networkobjectdata packet error");
                                data = "packet error";
                            }
                        }
                        break;
                    default:
                        LethalExpansion.Log.LogInfo("Unrecognized command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex);
            }
        }
        private static void ProcessData(ulong sender, string header, string packet)
        {
            try
            {
                switch (header)
                {
                    case "clientinfo": //host receive info from client
                        if (LethalExpansion.ishost && sender != 0)
                        {
                            string[] values;
                            if (packet.Contains('$'))
                            {
                                values = packet.Split('$');
                            }
                            else
                            {
                                values = new string[1] { packet };
                            }

                            string bundles = string.Empty;
                            foreach (var bundle in AssetBundlesManager.Instance.assetBundles)
                            {
                                bundles += $"{bundle.Key}v{bundle.Value.Item2.GetVersion().ToString()}&";
                            }
                            if (bundles.Length > 0 && bundles.EndsWith('&'))
                            {
                                bundles = bundles.Remove(bundles.Length - 1);
                            }

                            if (values[0] != LethalExpansion.ModVersion.ToString())
                            {
                                if (StartOfRound.Instance.ClientPlayerList.ContainsKey(sender))
                                {
                                    LethalExpansion.Log.LogError($"Kicking {sender} for wrong version.");
                                    NetworkPacketManager.Instance.sendPacket(packetType.data, "kickreason", "Wrong version.", (long)sender);
                                    StartOfRound.Instance.KickPlayer(StartOfRound.Instance.ClientPlayerList[sender]);
                                }
                                break;
                            }
                            else if (values.Length > 1 && values[1] != bundles)
                            {
                                if (StartOfRound.Instance.ClientPlayerList.ContainsKey(sender))
                                {
                                    LethalExpansion.Log.LogError($"Kicking {sender} for wrong bundles.");
                                    NetworkPacketManager.Instance.sendPacket(packetType.data, "kickreason", "Wrong bundles.", (long)sender);
                                    StartOfRound.Instance.KickPlayer(StartOfRound.Instance.ClientPlayerList[sender]);
                                }
                                break;
                            }

                            string config = string.Empty;
                            foreach (ConfigItem item in ConfigManager.Instance.GetAll())
                            {
                                switch (item.type.Name)
                                {
                                    case "Int32":
                                        config += "i" + ((int)item.Value).ToString(CultureInfo.InvariantCulture);
                                        break;
                                    case "Single":
                                        config += "f" + ((float)item.Value).ToString(CultureInfo.InvariantCulture);
                                        break;
                                    case "Boolean":
                                        config += "b" + ((bool)item.Value);
                                        break;
                                    case "String":
                                        config += "s" + item;
                                        break;
                                    default:
                                        break;
                                }
                                config += "&";
                            }
                            if (config.EndsWith('&'))
                            {
                                config = config.Remove(config.Length - 1);
                            }
                            NetworkPacketManager.Instance.sendPacket(packetType.data, "hostconfig", config, (long)sender);
                        }
                        break;
                    case "hostconfig": //client receive config from host
                        if (!LethalExpansion.ishost && sender == 0)
                        {
                            string[] values = packet.Split('&');

                            LethalExpansion.Log.LogInfo("Received host config: " + packet);

                            for (int i = 0; i < values.Length; i++)
                            {
                                if (i < ConfigManager.Instance.GetCount())
                                {
                                    if (ConfigManager.Instance.MustBeSync(i))
                                    {
                                        ConfigManager.Instance.SetItemValue(i, values[i].Substring(1), values[i][0]);
                                    }
                                }
                            }

                            LethalExpansion.hostDataWaiting = false;
                            LethalExpansion.Log.LogInfo("Updated config");
                        }
                        break;
                    case "hostweathers": //client receive weathers from host
                        if (!LethalExpansion.ishost && sender == 0)
                        {
                            string[] values = packet.Split('&');

                            LethalExpansion.Log.LogInfo("Received host weathers: " + packet);

                            StartOfRound_Patch.currentWeathers = new int[values.Length];
                            for (int i = 0; i < values.Length; i++)
                            {
                                int tmp = 0;
                                if (int.TryParse(values[i], out tmp))
                                {
                                    StartOfRound_Patch.currentWeathers[i] = tmp;
                                    StartOfRound.Instance.levels[i].currentWeather = (LevelWeatherType)tmp;
                                }
                            }
                        }
                        break;
                    case "networkobjectdata": //client receive network object data from host
                        if (!LethalExpansion.ishost && sender == 0)
                        {
                            string[] datas;
                            if (packet.Contains('&'))
                            {
                                datas = packet.Split('&');
                            }
                            else
                            {
                                datas = new string[1] { packet };
                            }
                            try
                            {
                                foreach (string data in datas)
                                {
                                    if (data.Contains('$'))
                                    {
                                        string[] tmp = data.Split('$');
                                        ulong id = 0;
                                        if (tmp.Length >= 2 && ulong.TryParse(tmp[0], out id) && tmp[1].Contains(','))
                                        {
                                            LethalSDK.Utils.NetworkDataManager.NetworkData[id].serializedData = tmp[1];
                                            LethalExpansion.Log.LogDebug($"networkobjectdata: {tmp[0]} data received {tmp[1]}");
                                        }
                                        else
                                        {
                                            LethalExpansion.Log.LogError("networkobjectdata error " + packet);
                                        }
                                    }
                                    else
                                    {
                                        LethalExpansion.Log.LogError("networkobjectdata error " + packet);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LethalExpansion.Log.LogError(ex);
                            }
                        }
                        break;
                    case "kickreason":
                        if (!LethalExpansion.ishost && sender == 0)
                        {
                            LethalExpansion.lastKickReason = packet;
                        }
                        break;
                    default:
                        LethalExpansion.Log.LogInfo("Unrecognized property.");
                        break;
                }
            }
            catch (Exception ex)
            {
                LethalExpansion.Log.LogError(ex);
            }
        }
    }
}
