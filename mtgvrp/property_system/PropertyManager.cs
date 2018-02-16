using System;
using System.Collections.Generic;
using System.Linq;

using GTANetworkAPI;





using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.door_manager;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.job_manager.delivery;
using mtgvrp.player_manager;
using MongoDB.Driver;
using mtgvrp.core.Help;

namespace mtgvrp.property_system
{
    public class PropertyManager : Script
    {
        public static List<Property> Properties;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            Properties = DatabaseManager.PropertyTable.Find(FilterDefinition<Property>.Empty).ToList();
            foreach (var prop in Properties)
            {
                prop.CreateProperty();
            }
            NAPI.Util.ConsoleOutput("Created Properties.");
        }

        //NEVER DELETE ANY PROPERTY TYPE FROM HERE, OR THE ONES UNDER WILL FUCK UP!!!!!!
        //IF YOU DELETE ONE, REPLACE IT
        public enum PropertyTypes
        {
            Clothing,
            TwentyFourSeven,
            Hardware,
            Bank,
            Restaurant,
            Advertising,
            GasStation,
            ModdingShop,
            LSNN,
            HuntingStation,
            Housing,
            VIPLounge,
            Government,
            DMV,
            Container,
        }

        #region ColShapeKnowing

        [ServerEvent(Event.PlayerExitColshape)]
        public void OnPlayerExitColShape(ColShape colshape, Client entity)
        {
            if (colshape.HasData("property_entrance"))
            {
                if (NAPI.Data.GetEntityData(entity, "at_interance_property_id") == colshape.GetData("property_entrance"))
                {
                    NAPI.Data.ResetEntityData(entity, "at_interance_property_id");
                }
            }

            if (colshape.HasData("property_interaction"))
            {
                if (NAPI.Data.GetEntityData(entity, "at_interaction_property_id") == colshape.GetData("property_interaction"))
                {
                    NAPI.Data.ResetEntityData(entity, "at_interaction_property_id");
                }
            }

            if (colshape.HasData("property_garbage"))
            {
                if (NAPI.Data.GetEntityData(entity, "at_garbage_property_id") == colshape.GetData("property_garbage"))
                {
                    NAPI.Data.ResetEntityData(entity, "at_garbage_property_id");
                }
            }

            if (colshape.HasData("property_exit"))
            {
                if (NAPI.Data.GetEntityData(entity, "at_exit_property_id") == colshape.GetData("property_exit"))
                {
                    NAPI.Data.ResetEntityData(entity, "at_exit_property_id");
                }
            }
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void OnPlayterEnterColShape(ColShape colshape, Client entity)
        {
            if (colshape.HasData("property_entrance"))
            {
                int id = colshape.GetData("property_entrance");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                if(property.EntranceDimension != API.GetEntityDimension(entity))
                    return;

                NAPI.Data.SetEntityData(entity, "at_interance_property_id", colshape.GetData("property_entrance"));
            }

            if (colshape.HasData("property_interaction"))
            {
                int id = colshape.GetData("property_interaction");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                if(property.InteractionDimension != API.GetEntityDimension(entity))
                    return;

                NAPI.Data.SetEntityData(entity, "at_interaction_property_id", colshape.GetData("property_interaction"));
            }

            if (colshape.HasData("property_garbage"))
            {
                int id = colshape.GetData("property_garbage");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                if(property.GarbageDimension != API.GetEntityDimension(entity))
                    return;
                
                NAPI.Data.SetEntityData(entity, "at_garbage_property_id", colshape.GetData("property_garbage"));
            }

            if (colshape.HasData("property_exit"))
            {
                int id = colshape.GetData("property_exit");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                if(property.TargetDimension != API.GetEntityDimension(entity))
                    return;

                NAPI.Data.SetEntityData(entity, "at_exit_property_id", colshape.GetData("property_exit"));
            }
        }

        public static Property IsAtPropertyEntrance(Client player)
        {
            if (API.Shared.HasEntityData(player, "at_interance_property_id"))
            {
                int id = API.Shared.GetEntityData(player, "at_interance_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        public static Property IsAtPropertyExit(Client player)
        {
            if (API.Shared.HasEntityData(player, "at_exit_property_id"))
            {
                int id = API.Shared.GetEntityData(player, "at_exit_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        public static Property IsAtPropertyInteraction(Client player)
        {
            if (API.Shared.HasEntityData(player, "at_interaction_property_id"))
            {
                int id = API.Shared.GetEntityData(player, "at_interaction_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        public static Property IsAtPropertyGarbagePoint(Client player)
        {
            if (API.Shared.HasEntityData(player, "at_garbage_property_id"))
            {
                int id = API.Shared.GetEntityData(player, "at_garbage_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        #endregion

        [RemoteEvent("editproperty_setname")]
        public void EditPropertySetName(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.PropertyName = (string)arguments[1];
                prop.Save();
                prop.UpdateMarkers();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Name of Property #{id} was changed to: '{arguments[1]}'");
            }
        }

        [RemoteEvent("editproperty_settype")]
        public void EditPropertySetType(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                PropertyTypes type;
                if (Enum.TryParse((string)arguments[1], out type))
                {
                    prop.Type = type;
                    ItemManager.SetDefaultPrices(prop);
                    prop.Save();
                    prop.UpdateMarkers();
                    NAPI.Chat.SendChatMessageToPlayer(sender,
                        $"[Property Manager] Type of Property #{id} was changed to: '{prop.Type}'");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Type Entered.");
                }
            }
        }

        [RemoteEvent("editproperty_setsupplies")]
        public void EditPropertySetSupplies(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                int sup;
                if (int.TryParse((string)arguments[1], out sup))
                {
                    prop.Supplies = sup;
                    prop.Save();
                    NAPI.Chat.SendChatMessageToPlayer(sender,
                        $"[Property Manager] Supplies of Property #{id} was changed to: '{sup}'");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Supplies Entered.");
                }
            }
        }

        [RemoteEvent("editproperty_setentrancepos")]
        public void EditPropertySetEntrancePos(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.EntrancePos = sender.Position;
                prop.EntranceRot = sender.Rotation;
                prop.EntranceDimension = (int)sender.Dimension;
                prop.Save();
                prop.UpdateMarkers();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Entrance Position of property #{id} was changed.");
            }
        }

        [RemoteEvent("editproperty_gotoentrance")]
        public void EditPropertyGotoEntrance(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                sender.Position = prop.EntrancePos;
                sender.Rotation = prop.EntranceRot;
                sender.Dimension = 0;
            }
        }

        [RemoteEvent("editproperty_setmaindoor")]
        public void EditPropertySetMainDoor(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                int doorid;
                if (int.TryParse((string)arguments[1], out doorid))
                {
                    if (Door.Doors.Exists(x => x.Id == doorid))
                    {
                        prop.MainDoorId = doorid;
                        prop.Save();
                        NAPI.Chat.SendChatMessageToPlayer(sender,
                            $"[Property Manager] Main Door of Property #{id} was changed to: '{doorid}'");
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid DoorId Entered.");
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid DoorId Entered.");
                }
            }
        }

        [RemoteEvent("editproperty_toggleteleportable")]
        public void EditPropertyToggleTeleportable(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.IsTeleportable = !prop.IsTeleportable;
                prop.Save();
                prop.UpdateMarkers();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Property #{id} was made to be '" +
                    (prop.IsTeleportable ? "Teleportable" : "UnTeleportable") + "'");
            }
        }

        [RemoteEvent("editproperty_setteleportpos")]
        public void EditPropertySetTeleportPos(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                if (!prop.IsTeleportable)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Property isn't teleportable.");
                    return;
                }
                prop.TargetPos = sender.Position;
                prop.TargetRot = sender.Rotation;
                prop.Save();
                prop.UpdateMarkers();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Interior TP Position of property #{id} was changed.");
            }
        }

        [RemoteEvent("editproperty_toggleinteractable")]
        public void EditPropertyToggleInteractable(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.IsInteractable = !prop.IsInteractable;
                if (!prop.IsInteractable) { prop.UpdateMarkers(); }
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Property #{id} was made to be '" +
                    (prop.IsInteractable ? "Interactable" : "UnInteractable") + "'");
            }
        }

        [RemoteEvent("editproperty_setinteractpos")]
        public void EditPropertySetInteractPos(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                if (!prop.IsInteractable)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Property isn't interactable.");
                    return;
                }
                prop.InteractionPos = sender.Position;
                prop.InteractionRot = sender.Rotation;
                prop.InteractionDimension = (int)sender.Dimension;
                prop.UpdateMarkers();
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Interaction Position of property #{id} was changed.");
            }
        }

        [RemoteEvent("editproperty_togglelock")]
        public void EditPropertyToggleLock(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.IsLocked = !prop.IsLocked;
                prop.UpdateLockStatus();
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Property #{id} was made to be '" +
                    (prop.IsLocked ? "Locked" : "UnLocked") + "'");
            }
        }

        [RemoteEvent("editproperty_deleteproperty")]
        public void EditPropertyDeleteProperty(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.Delete();
                NAPI.Chat.SendChatMessageToPlayer(sender, $"[Property Manager] Property #{id} was deleted.");
            }
        }

        [RemoteEvent("editproperty_setprice")]
        public void EditPropertySetPrice(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                int price;
                if (int.TryParse((string)arguments[1], out price))
                {
                    prop.PropertyPrice = price;
                    prop.Save();
                    prop.UpdateMarkers();
                    NAPI.Chat.SendChatMessageToPlayer(sender,
                        $"[Property Manager] Price of Property #{id} was changed to: '{price}'");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Price Entered.");
                }
            }
        }

        [RemoteEvent("editproperty_setowner")]
        public void EditPropertySetOwner(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                var player = PlayerManager.ParseClient((string)arguments[1]);
                if (player == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Player Entered.");
                    return;
                }
                prop.OwnerId = player.GetCharacter().Id;
                prop.Save();
                prop.UpdateMarkers();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Owner of Property #{id} was changed to: '{player.GetCharacter().CharacterName}'");
            }
        }

        [RemoteEvent("editproperty_togglehasgarbage")]
        public void EditPropertyToggleHasGarbage(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.HasGarbagePoint = !prop.HasGarbagePoint;
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Property #{id} was made to '" +
                    (prop.HasGarbagePoint ? "have garbage" : "have no garbage") + "'");
            }
        }

        [RemoteEvent("editproperty_setgarbagepoint")]
        public void EditPropertySetGarbagePoint(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                if (!prop.HasGarbagePoint)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Property cannot have a garbage point.");
                    return;
                }
                prop.GarbagePoint = sender.Position;
                prop.GarbageRotation = sender.Rotation;
                prop.GarbageDimension = (int)sender.Dimension;
                prop.UpdateMarkers();
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Garbage point of property #{id} was changed.");
            }
        }

        [RemoteEvent("attempt_enter_prop")]
        public void AttemptEnterProp(Client sender, params object[] arguments)
        {
            if (Exitproperty(sender) == false)
                Enterproperty(sender);
        }

        [RemoteEvent("editproperty_addipl")]
        public void EditPropertyAddIpl(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }
                prop.IPLs.Add(arguments[1].ToString());
                prop.Save();
                NAPI.Chat.SendChatMessageToPlayer(sender,
                    $"[Property Manager] Added IPL {arguments[1]} to property #{id}.");
                NAPI.ClientEvent.TriggerClientEvent(sender, "editproperty_showmenu", prop.Id, NAPI.Util.ToJson(prop.IPLs.ToArray()));
            }
        }

        [RemoteEvent("editproperty_deleteipl")]
        public void EditPropertyDeleteIpl(Client sender, params object[] arguments)
        {
            if (sender.GetAccount().AdminLevel >= 5)
            {
                var id = Convert.ToInt32(arguments[0]);
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                    return;
                }

                var ipl = arguments[1].ToString();
                if (prop.IPLs.RemoveAll(x => x == ipl) > 0)
                {
                    prop.Save();
                    NAPI.Chat.SendChatMessageToPlayer(sender,
                        $"[Property Manager] Removed IPL {ipl} from property #{id}.");
                    NAPI.ClientEvent.TriggerClientEvent(sender, "editproperty_showmenu", prop.Id, NAPI.Util.ToJson(prop.IPLs.ToArray()));
                }
            }
        }

        public static string GetInteractText(PropertyTypes type)
        {
            switch (type)
            {
                case PropertyTypes.Clothing:
                    return "/buyclothes /buybag";
                case PropertyTypes.TwentyFourSeven:
                    return "/buy";
                case PropertyTypes.Hardware:
                    return "/buy";
                case PropertyTypes.Bank:
                    return "/balance /deposit /withdraw\n/wiretransfer /redeemcheck";
                case PropertyTypes.Restaurant:
                    return "/buy";
                case PropertyTypes.Advertising:
                    return "/advertise";
                case PropertyTypes.GasStation:
                    return "/refuel /refillgascan";
                case PropertyTypes.LSNN:
                    return "/buy";
                case PropertyTypes.HuntingStation:
                    return "/buy\n/redeemdeertag\n/redeemboartag";
                case PropertyTypes.Government:
                    return "/buy";
                case PropertyTypes.DMV:
                    return "/starttest /registervehicle";
                case PropertyTypes.VIPLounge:
                    return "/buyweapontint";
                case PropertyTypes.Container:
                    return "/upgradehq /trackdealer\n/propertystorage";
            }
            return "";
        }

        [Command("togacceptsupplies", Alias = "togas"), Help(HelpManager.CommandGroups.Bussiness, "Used to toggle accepting supplies for your business.", null)]
        public void TogSupplies(Client player)
        {
            var prop = IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point.");
                return;
            }

            if (prop.OwnerId != player.GetCharacter().Id || 
                prop.Type == PropertyTypes.Bank ||
                prop.Type == PropertyTypes.Advertising ||
                prop.Type == PropertyTypes.Housing ||
                prop.Type == PropertyTypes.LSNN ||
                prop.Type == PropertyTypes.VIPLounge ||
                prop.Type == PropertyTypes.Container
                )
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't the owner or the business doesnt support supplies.");
                return;
            }

            prop.DoesAcceptSupplies = !prop.DoesAcceptSupplies;

            NAPI.Chat.SendChatMessageToPlayer(player,
                prop.DoesAcceptSupplies
                    ? "You are now ~g~accepting~w~ supplies."
                    : "You are now ~r~not accepting~w~ supplies.");
            prop.Save();
        }

        [Command("setsupplyprice", Alias = "setsp"), Help(HelpManager.CommandGroups.Bussiness, "Setting the price you pay per delivery of supplies.", new[] { "Price per supply." })]
        public void SetSupplyPrice(Client player, int amount)
        {
            var prop = IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point.");
                return;
            }

            if (prop.OwnerId != player.GetCharacter().Id ||
                prop.Type == PropertyTypes.Bank ||
                prop.Type == PropertyTypes.Advertising ||
                prop.Type == PropertyTypes.Housing ||
                prop.Type == PropertyTypes.LSNN ||
                prop.Type == PropertyTypes.VIPLounge ||
                prop.Type == PropertyTypes.Container
            )
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't the owner or the business doesnt support supplies.");
                return;
            }

            if (amount <= 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Price can't be below 0");
                return;
            }

            prop.SupplyPrice = amount;

            NAPI.Chat.SendChatMessageToPlayer(player, "You've set the supply price to: $" + amount);
            NAPI.Chat.SendChatMessageToPlayer(player, "Make sure you do have enough money in the business storage.");
            prop.Save();
        }

        [Command("enter"), Help(HelpManager.CommandGroups.General, "How to enter buildings, there is marker on the door for ones that work.", null)]
        public bool Enterproperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player);
            if (prop != null)
            {
                if (prop.IsVIP && player.GetAccount().VipLevel < 1)
                {
                    player.SendChatMessage("You cannot enter a VIP building. Visit www.mt-gaming.com to check out the available upgrades!");
                    return false;
                }

                if (prop.IsTeleportable && (!prop.IsLocked || prop.OwnerId == player.GetCharacter().Id))
                {
                    foreach (var ipl in prop.IPLs ?? new List<string>())
                    {
                        //TODO: request ipl for player.
                    }

                    player.Position = prop.TargetPos;
                    player.Rotation = prop.TargetRot;
                    player.Dimension = (uint)prop.TargetDimension;
                    ChatManager.RoleplayMessage(player, $"has entered {prop.PropertyName}.", ChatManager.RoleplayMe);

                    //Supplies.
                    if (prop.DoesAcceptSupplies &&
                        player.GetCharacter().JobOne?.Type == JobManager.JobTypes.DeliveryMan && InventoryManager
                            .DoesInventoryHaveItem<SupplyItem>(player.GetCharacter()).Length > 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "This business is selling supplies for $" + prop.SupplyPrice);
                    }
                    return true;
                }
                else
                {
                    NAPI.Notification.SendNotificationToPlayer(player,
                        prop.IsLocked ? "Property is locked." : "Property is not teleportable.");
                }
            }
            return false;
        }

        [Command("exit"), Help(HelpManager.CommandGroups.General, "How to exit buildings, there is marker on the door for ones that work.", null)]
        public bool Exitproperty(Client player)
        {
            var prop = IsAtPropertyExit(player);
            if (prop != null)
            {
                if (prop.IsTeleportable && (!prop.IsLocked || prop.OwnerId == player.GetCharacter().Id))
                {
                    foreach (var ipl in prop.IPLs ?? new List<string>())
                    {
                       //TODO: remove ipl for player.
                    }

                    if (prop.Type == PropertyTypes.Container)
                    {
                        player.Position = prop.EntrancePos + new Vector3(0, 0, 5f);
                    }
                    else
                    {
                        player.Position = prop.EntrancePos;
                    }
                        player.Rotation = prop.EntranceRot;
                        player.Dimension = (uint)prop.EntranceDimension;
                    
                    ChatManager.RoleplayMessage(player, $"has exited the building.", ChatManager.RoleplayMe);
                    return true;
                }
                else
                {
                    NAPI.Notification.SendNotificationToPlayer(player,
                        prop.IsLocked ? "Property is locked." : "Property is not teleportable.");
                }
            }
            return false;
        }

        [Command("changefoodname", GreedyArg = true), Help(HelpManager.CommandGroups.Bussiness, "Changing the name of items in your restaurant.", new[] { "Item", "New name"})]
        public void Changefoodname_cmd(Client player, string item = "", string name = "")
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId != player.GetCharacter().Id || prop.Type != PropertyTypes.Restaurant)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't the owner or the business isn't a Restaurant.");
                return;
            }

            if (item == "")
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "[ERROR] Choose one: [custom1,custom2,custom3,custom4]");
                return;
            }
            if (name == "")
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "[ERROR] Name can't be nothing.");
                return;
            }

            if (prop.RestaurantItems == null) prop.RestaurantItems = new string[4];
            switch (item)
            {
                case "custom1":
                    prop.RestaurantItems[0] = name;
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Changed custom1 name to '{name}'.");
                    break;
                case "custom2":
                    prop.RestaurantItems[1] = name;
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Changed custom2 name to '{name}'.");
                    break;
                case "custom3":
                    prop.RestaurantItems[2] = name;
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Changed custom3 name to '{name}'.");
                    break;
                case "custom4":
                    prop.RestaurantItems[3] = name;
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Changed custom4 name to '{name}'.");
                    break;
                default:
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Invalid type.");
                    break;
            }
            prop.Save();
        }

        [Command("manageprices"), Help(HelpManager.CommandGroups.Bussiness, "Setting prices of items inside your business.", new[] { "Item", "Price" })]
        public void Manageprices(Client player, string item = "", int price = 0)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                if (price <= 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "[ERROR] Price can't be zero.");
                    return;
                }

                if (item == "")
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Choose a type: ");
                    string msg = prop.ItemPrices.Keys.Aggregate("", (current, key) => current + (key + ","));
                    msg = msg.Remove(msg.Length - 1, 1);
                    NAPI.Chat.SendChatMessageToPlayer(player, msg);
                    return;
                }

                if (!prop.ItemPrices.ContainsKey(item))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "[ERROR] That type doesn't exist.");
                    return;
                }

                prop.ItemPrices[item] = price;
                NAPI.Chat.SendChatMessageToPlayer(player, $"Changed ~g~{item}~w~ price to {price}");
                prop.Save();
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("buyproperty"), Help(HelpManager.CommandGroups.PropertyGeneral, "Command to purchause property when near it.", null)]
        public void Buyproperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a property entrance.");
                return;
            }

            if (prop.OwnerId != 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That property isn't for sale.");
                return;
            }

            if (Money.GetCharacterMoney(player.GetCharacter()) < prop.PropertyPrice)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have enough money to buy this property.");
                return;
            }

            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), prop.PropertyPrice);
            prop.OwnerId = player.GetCharacter().Id;
            prop.Save();
            prop.UpdateMarkers();

            NAPI.Chat.SendChatMessageToPlayer(player,
                $"You have sucessfully bought a ~r~{prop.Type}~w~ for ~g~{prop.PropertyPrice}~w~.");
        }

        [Command("lockproperty"), Help(HelpManager.CommandGroups.PropertyGeneral, "Locking your business/house.", null)]
        public void LockProperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                prop.IsLocked = !prop.IsLocked;
                prop.UpdateLockStatus();
                prop.Save();
                NAPI.Notification.SendNotificationToPlayer(player,
                    prop.IsLocked ? "Property has been ~g~locked." : "Property has been ~r~unlocked.");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("propertyname", GreedyArg = true), Help(HelpManager.CommandGroups.PropertyGeneral, "Changing the name of your property.", new[] { "Name" })]
        public void PropertyName(Client player, string name)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                prop.PropertyName = name;
                prop.UpdateMarkers();
                prop.Save();
                NAPI.Notification.SendNotificationToPlayer(player, "Property name has been changed.");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("propertystorage"), Help(HelpManager.CommandGroups.PropertyGeneral, "Command to access the storage inside your property.", null)]
        public void PropertyStorage(Client player)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId != player.GetCharacter().Id || player.GetAccount().AdminLevel < 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't own this property.");
                return;
            }

            if (prop.Inventory == null) prop.Inventory = new List<IInventoryItem>();
            InventoryManager.ShowInventoryManager(player, player.GetCharacter(), prop, "Inventory: ", "Property: ");
        }

        [Command("createproperty"), Help(HelpManager.CommandGroups.AdminLevel5, "To create a new business/house.", new[] { "Property type." })]
        public void create_property(Client player, PropertyTypes type)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                var property = new Property(type, player.Position, player.Rotation, type.ToString());
                ItemManager.SetDefaultPrices(property);
                property.Insert();
                property.CreateProperty();
                Properties.Add(property);
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have sucessfully create a property of type " + type.ToString());
            }
        }

        [Command("propertytypes"), Help(HelpManager.CommandGroups.AdminLevel5, "Lists all property types.", null)]
        public void Propertytypes(Client player)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "______ Listing Property Types ______");
                foreach (var type in Enum.GetNames(typeof(PropertyTypes)))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "* " + type);
                }
                NAPI.Chat.SendChatMessageToPlayer(player, "____________________________________");
            }
        }

        [Command("editproperty"), Help(HelpManager.CommandGroups.AdminLevel5, "Edit any information about a property", new[] { "ID of property." })]

        public void edit_property(Client player, int id)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Invalid Property Id.");
                    return;
                }
                
                if(prop.IPLs == null)
                    prop.IPLs = new List<string>();

                NAPI.ClientEvent.TriggerClientEvent(player, "editproperty_showmenu", prop.Id, NAPI.Util.ToJson(prop.IPLs.ToArray()));
            }
        }

        [Command("listproperties"), Help(HelpManager.CommandGroups.AdminLevel5, "Lists all properties.", new[] { "The type." })]

        public void listprops_cmd(Client player, PropertyTypes type)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "______ Listing Property Types ______");
                foreach (var prop in Properties.Where(x => x.Type == type))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, $"* Property Id: ~g~{prop.Id}~w~ | Name: ~g~{prop.PropertyName}");
                }
                NAPI.Chat.SendChatMessageToPlayer(player, "____________________________________");
            }
        }
    }
}
