using System;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer
{
    public static class DebugManager
    {
        public const int DEBUG_LEVEL = 10;

        public const int STACKTRACE_PRINT_LEVEL = 2;

        public static void DebugManagerInit()
        {
            debugMessage("[DebugM] Debug manager loaded... (Current Level: " + DEBUG_LEVEL + ")");
        }


        public static void debugMessage(string msg, int level = 0)
        {
            if(level <= DEBUG_LEVEL)
            {
                API.shared.consoleOutput(msg);
            }
        }
    }
}
