using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using RAGE;

namespace cs_packages
{
    public class Main : Events.Script
    {
        public Main()
        {
            Events.Add("freezePlayer", FreezePlayer);
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
            while (RAGE.Game.Misc.UpdateOnscreenKeyboard() != 1 && RAGE.Game.Misc.UpdateOnscreenKeyboard() != 2) { }
            string ans = RAGE.Game.Misc.GetOnscreenKeyboardResult();
            return ans;
        }
    }
}
