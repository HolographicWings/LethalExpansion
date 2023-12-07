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
        public void sendPacket(string packet)
        {
            HUDManager.Instance.AddTextToChatOnServer("[sync]" + packet + "[sync]");
        }
    }
}
