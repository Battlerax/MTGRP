using GTANetworkServer;

namespace RoleplayServer.resources.core
{
    public static class DebugManager
    {
        private const int DebugLevel = 10;

        public const int StacktracePrintLevel = 2;

        public static void DebugManagerInit()
        {
            DebugMessage("[DebugM] Debug manager loaded... (Current Level: " + DebugLevel + ")");
        }


        public static void DebugMessage(string msg, int level = 0)
        {
            if(level <= DebugLevel)
            {
                API.shared.consoleOutput(msg);
            }
        }
    }
}
