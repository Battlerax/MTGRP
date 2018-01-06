using System;
using System.Collections.Generic;
using System.Diagnostics;

using GTANetworkAPI;



using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;

namespace mtgvrp.drugs_manager
{
    public class Airdrop : IStorage
    {

        public int Amount { get; private set; }
        public Vector3 Loc { get; private set; }
        public bool IsOpen { get; set; }
        public DrugTypes DrugName { get; private set; }
        public Guid id { get;}
        public NetHandle prop { get; set; }
        public MarkerZone marker { get; set; }

        // Inv for airdrops.
        public List<IInventoryItem> Inventory { get; set; }
        public int MaxInvStorage => DrugsManager.MaxAirDropSize;

        public void Save()
        {
            //Ignored.
        }

        public Airdrop(IInventoryItem drug, Vector3 loc)
        {
            this.Loc = loc;
            IsOpen = false;
            id = Guid.NewGuid();
            Inventory = new List<IInventoryItem> {drug};
            marker = new MarkerZone(new Vector3(),new Vector3());
        }


        public void Delete()
        {
            this.Loc = null;
            Inventory = null;
            marker.Destroy();
            marker = null;
            API.Shared.DeleteEntity(prop);
        }

        public void UpdateMarkerOpen()
        {
            IsOpen = true;
            marker.Destroy();
            marker.TextLabelText = "Drugs Crate - Unlocked";
            marker.Create();
        }

        public void UpdateMarkerClose()
        {
            IsOpen = false;
            marker.Destroy();
            marker.TextLabelText = "Drugs Crate - Locked";
            marker.Create();
        }

        public void SetCorrectCrateLocation(Vector3 newLocation)
        {
            Loc = newLocation;
        }
    }
}
