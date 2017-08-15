using System;
using System.Collections.Generic;
using System.Diagnostics;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;

namespace mtgvrp.drugs_manager
{
    public class Airdrop : IStorage
    {

        public int Amount { get; private set; }
        public Vector3 Loc { get; private set; }
        public bool IsOpen { get; private set; }
        public DrugTypes DrugName { get; private set; }
        public TimeSpan TimeRem { get; private set; }
        public Guid id { get;}
        public NetHandle prop { get; set; }
        public Stopwatch timeSpent { get; set; }

        // Inv for airdrops.
        public List<IInventoryItem> Inventory { get; set; }
        public int MaxInvStorage => DrugsManager.MaxAirDropSize;

        public Airdrop(IInventoryItem drug, Vector3 loc)
        {
            this.Loc = loc;
            IsOpen = false;
            id = Guid.NewGuid();
            TimeRem = TimeSpan.FromMinutes(1.5);
            timeSpent = new Stopwatch();
            Inventory = new List<IInventoryItem>();
            Inventory.Add(drug);
        }

    }
}