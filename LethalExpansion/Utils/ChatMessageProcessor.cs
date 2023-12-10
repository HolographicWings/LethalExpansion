using System.Globalization;
using System.Text.RegularExpressions;

namespace LethalExpansion.Utils
{
    internal class ChatMessageProcessor
    {
        public static bool ProcessMessage(string message)
        {
            if (Regex.IsMatch(message, @"^\[sync\].*\[sync\]$"))
            {
                LethalExpansion.Log.LogInfo(message);
                string content = Regex.Match(message, @"^\[sync\](.*)\[sync\]$").Groups[1].Value;

                string[] parts = content.Split(':');
                if (parts.Length == 2)
                {
                    string key = parts[0];
                    string value = parts[1];

                    switch (key)
                    {
                        case "config":
                            ProcessConfig(value);
                            break;
                        case "command":
                            ProcessCommand(value);
                            break;
                        default:
                            LethalExpansion.Log.LogInfo("Unrecognized key.");
                            break;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private static void ProcessConfig(string configValue)
        {
            string[] configParts = configValue.Split('=');
            if (configParts.Length == 2)
            {
                string property = configParts[0];
                string propertyValue = configParts[1];

                switch (property)
                {
                    case "generalConfig":
                        if (!LethalExpansion.ishost)
                        {
                            string[] values = propertyValue.Split('|');

                            LethalExpansion.Log.LogInfo("Received host settings: " + propertyValue);

                            for(int i = 0; i< values.Length; i++)
                            {
                                if(i < ConfigManager.Instance.GetCount())
                                {
                                    if (ConfigManager.Instance.MustBeSync(i))
                                    {
                                        ConfigManager.Instance.SetItemValue(i, values[i].Substring(1), values[i][0]);
                                    }
                                }
                            }

                            LethalExpansion.hostDataWaiting = false;
                            LethalExpansion.Log.LogInfo("Updated settings");
                        }
                        break;
                    default:
                        LethalExpansion.Log.LogInfo("Unrecognized property.");
                        break;
                }
            }
        }
        private static void ProcessCommand(string commandValue)
        {
            switch (commandValue)
            {
                case "requestConfig":
                    if (LethalExpansion.ishost)
                    {
                        string configPacket = "config:generalConfig=";
                        foreach (var item in ConfigManager.Instance.GetAll())
                        {
                            switch (item.type.Name)
                            {
                                case "Int32":
                                    configPacket += 'i';
                                    break;
                                case "Single":
                                    configPacket += 'f';
                                    break;
                                case "Boolean":
                                    configPacket += 'b';
                                    break;
                                case "String":
                                    configPacket += 's';
                                    break;
                                default:
                                    break;
                            }
                            configPacket += item.Value + "|";
                        }
                        configPacket = configPacket.Remove(configPacket.Length - 1);
                        NetworkPacketManager.Instance.sendPacket(configPacket);
                    }
                    break;
                default:
                    LethalExpansion.Log.LogInfo("Unrecognized command.");
                    break;
            }
        }
    }
}
