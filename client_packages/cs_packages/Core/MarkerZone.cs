using System;
using System.Collections.Generic;
using System.Text;
using RAGE;

namespace cs_packages.Items
{
    public class MarkerZone : Events.Script
    {
        public MarkerZone()
        {
            Events.Add("setMarkerZoneRouteVisible", setMarkerZoneRouteVisible);
        }

        private void setMarkerZoneRouteVisible(object[] args)
        {
            RAGE.Game.Ui.SetBlipRouteColour((int)args[0], (int)args[2]);
            RAGE.Game.Ui.SetBlipRoute((int)args[0], (bool)args[1]);
        }
    }
}
