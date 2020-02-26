using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using RAGE;
using RAGE.Game;
using RAGE.NUI;

namespace cs_packages
{
    public class Main : Events.Script
    {
        public Main()
        {
            Events.Add("freezePlayer", FreezePlayer);
            Events.Add("CallNative", CallNative);
        }

        private void CallNative(object[] args)
        {
            ulong hash = (ulong) args[0];
            RAGE.Game.Invoker.Invoke(hash, args.ToList().GetRange(1, args.Length-1).ToArray());
        }

        private void FreezePlayer(object[] args)
        {
            RAGE.Elements.Player.LocalPlayer.FreezePosition((bool)args[0]);
        }

        public static void Notify(string text)
        {
            RAGE.Game.Ui.SetNotificationTextEntry("STRING");
            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(text);
            RAGE.Game.Ui.DrawNotification(false, false);
        }

        public static string GetUserInput(string defaultText, int maxLength) //TODO: review this shit.
        {
            RAGE.Game.Misc.DisplayOnscreenKeyboard(1, "", "", defaultText, "", "", "", maxLength);
            while (RAGE.Game.Misc.UpdateOnscreenKeyboard() != 1 && RAGE.Game.Misc.UpdateOnscreenKeyboard() != 2)
            {
                RAGE.Game.Invoker.Wait(0);
            }
            string ans = RAGE.Game.Misc.GetOnscreenKeyboardResult();
            return ans;
        }
    }
}
