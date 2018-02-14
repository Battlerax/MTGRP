using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace mtgvrp.Components.Groups
{
    public class GroupManager : Script
    {
        public GroupManager()
        {
            Event.OnResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            // Load All Groups @Dylan
        }
    }
}
