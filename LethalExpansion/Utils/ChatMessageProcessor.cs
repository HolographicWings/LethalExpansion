using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static LethalExpansion.Utils.NetworkPacketManager;

namespace LethalExpansion.Utils
{
    internal class ChatMessageProcessor
    {
        public static bool ProcessMessage(string message)
        {
            if (Regex.IsMatch(message, @"^\[sync\].*\[sync\]$"))
            {
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

                    if(destination == -1 || (ulong)destination == RoundManager.Instance.NetworkManager.LocalClientId)
                    {
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
            else
            {
                return false;
            }
        }
        private static void ProcessRequest(ulong sender, string header, string packet)
        {
            switch (header)
            {
                case "clientinfo":
                    if (!LethalExpansion.ishost && sender == 0)
                    {
                        string configPacket = $"{LethalExpansion.ModVersion.ToString()}-";
                        foreach (var bundle in AssetBundlesManager.Instance.assetBundles)
                        {
                            configPacket += $"{bundle.Key}v{bundle.Value.Item2.GetVersion().ToString()}&";
                        }
                        configPacket = configPacket.Remove(configPacket.Length - 1);
                        NetworkPacketManager.Instance.sendPacket(packetType.data, "clientinfo", configPacket, 0);
                    }
                    break;
                case "hostconfig":
                    if (LethalExpansion.ishost && sender != 0)
                    {
                        NetworkPacketManager.Instance.sendPacket(NetworkPacketManager.packetType.request, "clientinfo", string.Empty, (long)sender);
                    }
                    break;
                default:
                    LethalExpansion.Log.LogInfo("Unrecognized command.");
                    break;
            }
        }
        private static void ProcessData(ulong sender, string header, string packet)
        {
            switch (header)
            {
                case "clientinfo":
                    if (LethalExpansion.ishost && sender != 0)
                    {
                        string[] values = packet.Split('-');

                        string bundles = string.Empty;
                        foreach (var bundle in AssetBundlesManager.Instance.assetBundles)
                        {
                            bundles += $"{bundle.Key}v{bundle.Value.Item2.GetVersion().ToString()}&";
                        }
                        bundles = bundles.Remove(bundles.Length - 1);
                        if (values[0] != LethalExpansion.ModVersion.ToString())
                        {
                            if (StartOfRound.Instance.ClientPlayerList.ContainsKey(sender))
                            {
                                LethalExpansion.Log.LogError($"Kicking {sender} for wrong version.");
                                NetworkPacketManager.Instance.sendPacket(packetType.data, "kickreason", "Wrong version.", (long)sender);
                                StartOfRound.Instance.KickPlayer(StartOfRound.Instance.ClientPlayerList[sender]);
                            }
                        }
                        else if (values[1] != bundles)
                        {
                            if (StartOfRound.Instance.ClientPlayerList.ContainsKey(sender))
                            {
                                LethalExpansion.Log.LogError($"Kicking {sender} for wrong bundles.");
                                NetworkPacketManager.Instance.sendPacket(packetType.data, "kickreason", "Wrong bundles.", (long)sender);
                                StartOfRound.Instance.KickPlayer(StartOfRound.Instance.ClientPlayerList[sender]);
                            }
                        }
                        else
                        {
                            string config = string.Empty;
                            foreach (var item in ConfigManager.Instance.GetAll())
                            {
                                switch (item.type.Name)
                                {
                                    case "Int32":
                                        config += 'i';
                                        break;
                                    case "Single":
                                        config += 'f';
                                        break;
                                    case "Boolean":
                                        config += 'b';
                                        break;
                                    case "String":
                                        config += 's';
                                        break;
                                    default:
                                        break;
                                }
                                config += item.Value + "&";
                            }
                            config = config.Remove(config.Length - 1);
                            NetworkPacketManager.Instance.sendPacket(packetType.data, "hostconfig", config, (long)sender);
                        }
                    }
                    break;
                case "hostconfig":
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
    }
}
