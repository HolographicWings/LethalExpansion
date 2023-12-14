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
        public void sendPacket(packetType type, string header, string packet, long destination = -1)
        {
            HUDManager.Instance.AddTextToChatOnServer($"[sync]{(int)type}|{RoundManager.Instance.NetworkManager.LocalClientId}>{destination}|{header}={packet}[sync]");
        }
        public void sendPacket(packetType type, string header, string packet, long[] destinations)
        {
            foreach (int destination in destinations)
            {
                HUDManager.Instance.AddTextToChatOnServer($"[sync]{(int)type}|{RoundManager.Instance.NetworkManager.LocalClientId}>{destination}|{header}={packet}[sync]");
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
