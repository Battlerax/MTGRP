using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.door_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.property_system
{
    public class PropertyManager : Script
    {
        public static List<Property> Properties;

        public PropertyManager()
        {
            API.onResourceStart += API_onResourceStart;
            API.onEntityEnterColShape += API_onEntityEnterColShape;
            API.onEntityExitColShape += API_onEntityExitColShape;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onResourceStart()
        {
            Properties = DatabaseManager.PropertyTable.Find(FilterDefinition<Property>.Empty).ToList();
            foreach (var prop in Properties)
            {
                prop.CreateProperty();
            }
            API.consoleOutput("Created Properties.");
        }

        public enum PropertyTypes
        {
            Clothing,
            TwentyFourSeven,
            Hardware,
            Bank,
            Restaurant
        }

        #region ColShapeKnowing

        private void API_onEntityExitColShape(ColShape colshape, GTANetworkShared.NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_entrance"))
            {
                if (API.getEntityData(entity, "at_interance_property_id") == colshape.getData("property_entrance"))
                {
                    API.resetEntityData(entity, "at_interance_property_id");
                }
            }

            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_interaction"))
            {
                if (API.getEntityData(entity, "at_interaction_property_id") == colshape.getData("property_interaction"))
                {
                    API.resetEntityData(entity, "at_interaction_property_id");
                }
            }

            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_exit"))
            {
                if (API.getEntityData(entity, "at_exit_property_id") == colshape.getData("property_exit"))
                {
                    API.resetEntityData(entity, "at_exit_property_id");
                }
            }
        }

        private void API_onEntityEnterColShape(ColShape colshape, GTANetworkShared.NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_entrance"))
            {
                API.setEntityData(entity, "at_interance_property_id", colshape.getData("property_entrance"));
            }

            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_interaction"))
            {
                API.setEntityData(entity, "at_interaction_property_id", colshape.getData("property_interaction"));
            }

            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_exit"))
            {
                API.setEntityData(entity, "at_exit_property_id", colshape.getData("property_exit"));
            }
        }

        public static Property IsAtPropertyEntrance(Client player)
        {
            if (API.shared.hasEntityData(player, "at_interance_property_id"))
            {
                int id = API.shared.getEntityData(player, "at_interance_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        public static Property IsAtPropertyExit(Client player)
        {
            if (API.shared.hasEntityData(player, "at_exit_property_id"))
            {
                int id = API.shared.getEntityData(player, "at_exit_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        public static Property IsAtPropertyInteraction(Client player)
        {
            if (API.shared.hasEntityData(player, "at_interaction_property_id"))
            {
                int id = API.shared.getEntityData(player, "at_interaction_property_id");
                var property = Properties.SingleOrDefault(x => x.Id == id);
                return property;
            }
            return null;
        }

        #endregion

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "editproperty_setname":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.PropertyName = (string) arguments[1];
                        prop.Save();
                        prop.UpdateMarkers();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Name of Property #{id} was changed to: '{arguments[1]}'");
                    }
                    break;

                case "editproperty_settype":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        PropertyTypes type;
                        if (Enum.TryParse((string) arguments[1], out type))
                        {
                            prop.Type = type;
                            ItemManager.SetDefaultPrices(prop);
                            prop.Save();
                            prop.UpdateMarkers();
                            API.sendChatMessageToPlayer(sender,
                                $"[Property Manager] Type of Property #{id} was changed to: '{prop.Type}'");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Type Entered.");
                        }
                    }
                    break;

                case "editproperty_setsupplies":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        int sup;
                        if (int.TryParse((string) arguments[1], out sup))
                        {
                            prop.Supplies = sup;
                            prop.Save();
                            API.sendChatMessageToPlayer(sender,
                                $"[Property Manager] Supplies of Property #{id} was changed to: '{sup}'");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Supplies Entered.");
                        }
                    }
                    break;

                case "editproperty_setentrancepos":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.EntrancePos = sender.position;
                        prop.EntranceRot = sender.rotation;
                        prop.Save();
                        prop.UpdateMarkers();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Entrance position of property #{id} was changed.");
                    }
                    break;

                case "editproperty_gotoentrance":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        sender.position = prop.EntrancePos;
                        sender.rotation = prop.EntranceRot;
                        sender.dimension = 0;
                    }
                    break;

                case "editproperty_setmaindoor":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        int doorid;
                        if (int.TryParse((string) arguments[1], out doorid))
                        {
                            if (Door.Doors.Exists(x => x.Id == doorid))
                            {
                                prop.MainDoorId = doorid;
                                prop.Save();
                                API.sendChatMessageToPlayer(sender,
                                    $"[Property Manager] Main Door of Property #{id} was changed to: '{doorid}'");
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid DoorId Entered.");
                            }
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid DoorId Entered.");
                        }
                    }
                    break;

                case "editproperty_toggleteleportable":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.IsTeleportable = !prop.IsTeleportable;
                        prop.Save();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Property #{id} was made to be '" +
                            (prop.IsTeleportable ? "Teleportable" : "UnTeleportable") + "'");
                    }
                    break;

                case "editproperty_setteleportpos":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        if (!prop.IsTeleportable)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Property isn't teleportable.");
                            return;
                        }
                        prop.TargetPos = sender.position;
                        prop.TargetRot = sender.rotation;
                        prop.TargetDimension = sender.dimension;
                        prop.Save();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Interior TP position of property #{id} was changed.");
                    }
                    break;

                case "editproperty_toggleinteractable":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.IsInteractable = !prop.IsInteractable;
                        prop.Save();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Property #{id} was made to be '" +
                            (prop.IsInteractable ? "Interactable" : "UnInteractable") + "'");
                    }
                    break;

                case "editproperty_setinteractpos":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        if (!prop.IsInteractable)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Property isn't interactable.");
                            return;
                        }
                        prop.InteractionPos = sender.position;
                        prop.InteractionRot = sender.rotation;
                        prop.InteractionDimension = sender.dimension;
                        prop.UpdateMarkers();
                        prop.Save();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Interaction position of property #{id} was changed.");
                    }
                    break;

                case "editproperty_togglelock":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.IsLocked = !prop.IsLocked;
                        prop.UpdateLockStatus();
                        prop.Save();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Property #{id} was made to be '" +
                            (prop.IsLocked ? "Locked" : "UnLocked") + "'");
                    }
                    break;

                case "editproperty_deleteproperty":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        prop.Delete();
                        API.sendChatMessageToPlayer(sender, $"[Property Manager] Property #{id} was deleted.");
                    }
                    break;

                case "editproperty_setprice":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        int price;
                        if (int.TryParse((string) arguments[1], out price))
                        {
                            prop.PropertyPrice = price;
                            prop.Save();
                            prop.UpdateMarkers();
                            API.sendChatMessageToPlayer(sender,
                                $"[Property Manager] Price of Property #{id} was changed to: '{price}'");
                        }
                        else
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Price Entered.");
                        }
                    }
                    break;

                case "editproperty_setowner":
                    if (sender.GetAccount().AdminLevel >= 5)
                    {
                        var id = Convert.ToInt32(arguments[0]);
                        var prop = Properties.SingleOrDefault(x => x.Id == id);
                        if (prop == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Property Id.");
                            return;
                        }
                        var player = PlayerManager.ParseClient((string) arguments[1]);
                        if (player == null)
                        {
                            API.sendChatMessageToPlayer(sender, "[Property Manager] Invalid Player Entered.");
                            return;
                        }
                        prop.OwnerId = player.GetCharacter().Id;
                        prop.Save();
                        prop.UpdateMarkers();
                        API.sendChatMessageToPlayer(sender,
                            $"[Property Manager] Owner of Property #{id} was changed to: '{player.GetCharacter().CharacterName}'");
                    }
                    break;
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
            }
            return "";
        }

        [Command("enter")]
        public void Enterproperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player);
            if (prop != null)
            {
                if (prop.IsTeleportable && (!prop.IsLocked || prop.OwnerId == player.GetCharacter().Id))
                {
                    player.position = prop.TargetPos;
                    player.rotation = prop.TargetRot;
                    player.dimension = prop.TargetDimension;
                    ChatManager.RoleplayMessage(player, $"has entered {prop.PropertyName}.", ChatManager.RoleplayMe);
                }
                else
                {
                    API.sendNotificationToPlayer(player,
                        prop.IsLocked ? "Property is locked." : "Property is not teleportable.");
                }
            }
        }

        [Command("exit")]
        public void Exitproperty(Client player)
        {
            var prop = IsAtPropertyExit(player);
            if (prop != null)
            {
                if (prop.IsTeleportable && (!prop.IsLocked || prop.OwnerId == player.GetCharacter().Id))
                {
                    player.position = prop.EntrancePos;
                    player.rotation = prop.EntranceRot;
                    player.dimension = 0;
                    ChatManager.RoleplayMessage(player, $"has exited the building.", ChatManager.RoleplayMe);
                }
                else
                {
                    API.sendNotificationToPlayer(player,
                        prop.IsLocked ? "Property is locked." : "Property is not teleportable.");
                }
            }
        }

        [Command("changefoodname", GreedyArg = true)]
        public void Changefoodname_cmd(Client player, string item = "", string name = "")
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId != player.GetCharacter().Id || prop.Type != PropertyTypes.Restaurant)
            {
                API.sendChatMessageToPlayer(player, "You aren't the owner or the business isn't a Restaurant.");
                return;
            }

            if (item == "")
            {
                API.sendChatMessageToPlayer(player, "[ERROR] Choose one: [custom1,custom2,custom3,custom4]");
                return;
            }
            if (name == "")
            {
                API.sendChatMessageToPlayer(player, "[ERROR] Name can't be nothing.");
                return;
            }

            if(prop.RestaurantItems == null) prop.RestaurantItems = new string[4];
            switch (item)
            {
                case "custom1":
                    prop.RestaurantItems[0] = name;
                    API.sendChatMessageToPlayer(player, $"Changed custom1 name to '{name}'.");
                    break;
                case "custom2":
                    prop.RestaurantItems[1] = name;
                    API.sendChatMessageToPlayer(player, $"Changed custom2 name to '{name}'.");
                    break;
                case "custom3":
                    prop.RestaurantItems[2] = name;
                    API.sendChatMessageToPlayer(player, $"Changed custom3 name to '{name}'.");
                    break;
                case "custom4":
                    prop.RestaurantItems[3] = name;
                    API.sendChatMessageToPlayer(player, $"Changed custom4 name to '{name}'.");
                    break;
                default:
                    API.sendChatMessageToPlayer(player, $"Invalid type.");
                    break;
            }
            prop.Save();
        }

        [Command("manageprices")]
        public void Manageprices(Client player, string item = "", int price = 0)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                switch (prop.Type)
                {
                    case PropertyTypes.Clothing:
                        if (item == "")
                        {                                                              //0    ,1    ,2          ,3          ,4   ,5   ,6      ,7
                            API.sendChatMessageToPlayer(player, "[ERROR] Choose a type: [Pants,Shoes,Accessories,Undershirts,Tops,Hats,Glasses,Earrings,Bags]");
                            return;
                        }
                        if (price == 0)
                        {
                            API.sendChatMessageToPlayer(player, "[ERROR] Price can't be zero.");
                            return;
                        }

                        switch (item.ToLower())
                        {
                            case "pants":
                                prop.ItemPrices["0"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Pants~w~ price to {price}");
                                break;
                            case "shoes":
                                prop.ItemPrices["1"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Shoes~w~ price to {price}");
                                break;
                            case "accessories":
                                prop.ItemPrices["2"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Accessories~w~ price to {price}");
                                break;
                            case "undershirts":
                                prop.ItemPrices["3"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Undershirts~w~ price to {price}");
                                break;
                            case "tops":
                                prop.ItemPrices["4"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Tops~w~ price to {price}");
                                break;
                            case "hats":
                                prop.ItemPrices["5"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Hats~w~ price to {price}");
                                break;
                            case "glasses":
                                prop.ItemPrices["6"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Glasses~w~ price to {price}");
                                break;
                            case "earrings":
                                prop.ItemPrices["7"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Earrings~w~ price to {price}");
                                break;
                            case "bags":
                                prop.ItemPrices["8"] = price;
                                API.sendChatMessageToPlayer(player, $"Changed ~g~Earrings~w~ price to {price}");
                                break;
                        }
                        break;

                        case PropertyTypes.Restaurant:
                            if (item == "")
                            {
                                API.sendChatMessageToPlayer(player, "[ERROR] Choose a type: [sprunk,custom1,custom2,custom3,custom4]");
                                return;
                            }
                            if (price == 0)
                            {
                                API.sendChatMessageToPlayer(player, "[ERROR] Price can't be zero.");
                                return;
                            }

                            switch (item.ToLower())
                            {
                                case "sprunk":
                                    prop.ItemPrices["sprunk"] = price;
                                    API.sendChatMessageToPlayer(player, $"Changed ~g~Sprunk~w~ price to {price}");
                                    break;
                                case "custom1":
                                    prop.ItemPrices["custom1"] = price;
                                    API.sendChatMessageToPlayer(player, $"Changed ~g~Custom 1~w~ price to {price}");
                                    break;
                                case "custom2":
                                    prop.ItemPrices["custom2"] = price;
                                    API.sendChatMessageToPlayer(player, $"Changed ~g~Custom 2~w~ price to {price}");
                                    break;
                                case "custom3":
                                    prop.ItemPrices["custom3"] = price;
                                    API.sendChatMessageToPlayer(player, $"Changed ~g~Custom 3~w~ price to {price}");
                                    break;
                                case "custom4":
                                    prop.ItemPrices["custom4"] = price;
                                    API.sendChatMessageToPlayer(player, $"Changed ~g~Custom 4~w~ price to {price}");
                                    break;
                        }
                        break;

                    case PropertyTypes.Hardware: case PropertyTypes.TwentyFourSeven:
                        if (item == "")
                        {
                            API.sendChatMessageToPlayer(player, "Choose a type: ");
                            string msg = "";
                            foreach (var key in prop.ItemPrices.Keys)
                            {
                                msg += key + ",";
                            }
                            msg = msg.Remove(msg.Length - 1, 1);
                            API.sendChatMessageToPlayer(player, msg);
                            return;
                        }
                        if (price == 0)
                        {
                            API.sendChatMessageToPlayer(player, "[ERROR] Price can't be zero.");
                            return;
                        }

                        if (!prop.ItemPrices.ContainsKey(item))
                        {
                            API.sendChatMessageToPlayer(player, "[ERROR] That type doesn't exist.");
                            return;
                        }

                        prop.ItemPrices[item] = price;
                        API.sendChatMessageToPlayer(player, $"Changed ~g~{item}~w~ price to {price}");
                        break;
                }
                prop.Save();
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("buyproperty")]
        public void Buyproperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a property entrance.");
                return;
            }

            if (prop.OwnerId != 0)
            {
                API.sendChatMessageToPlayer(player, "That property isn't for sale.");
                return;
            }

            if (Money.GetCharacterMoney(player.GetCharacter()) < prop.PropertyPrice)
            {
                API.sendChatMessageToPlayer(player, "You don't have enough money to buy this property.");
                return;
            }

            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), prop.PropertyPrice);
            prop.OwnerId = player.GetCharacter().Id;
            prop.Save();
            prop.UpdateMarkers();

            API.sendChatMessageToPlayer(player, $"You have sucessfully bought a ~r~{prop.Type}~w~ for ~g~{prop.PropertyPrice}~w~.");
        }

        [Command("lockproperty")]
        public void LockProperty(Client player)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                prop.IsLocked = !prop.IsLocked;
                prop.UpdateLockStatus();
                API.sendNotificationToPlayer(player,
                    prop.IsLocked ? "Property has been ~g~locked." : "Property has been ~r~unlocked.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("propertyname", GreedyArg = true)]
        public void PropertyName(Client player, string name)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.OwnerId == player.GetCharacter().Id)
            {
                prop.PropertyName = name;
                prop.UpdateMarkers();
                API.sendNotificationToPlayer(player, "Property name has been changed.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You don't own that property.");
            }
        }

        [Command("propertystorage")]
        public void PropertyStorage(Client player)
        {
            var prop = IsAtPropertyEntrance(player) ?? IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if(prop.Inventory == null) prop.Inventory = new List<IInventoryItem>();
            InventoryManager.ShowInventoryManager(player, player.GetCharacter(), prop, "Inventory: ", "Property: ");
        }

        [Command("createproperty")]
        public void create_property(Client player, PropertyTypes type)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                var property = new Property(type, player.position, player.rotation, type.ToString());
                ItemManager.SetDefaultPrices(property);
                property.Insert();
                property.CreateProperty();
                Properties.Add(property);
                API.sendChatMessageToPlayer(player, "You have sucessfully create a property of type " + type.ToString());
            } 
        }

        [Command("editproperty")]
        public void edit_property(Client player, int id)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                var prop = Properties.SingleOrDefault(x => x.Id == id);
                if (prop == null)
                {
                    API.sendChatMessageToPlayer(player, "Invalid Property Id.");
                    return;
                }
                API.triggerClientEvent(player, "editproperty_showmenu", prop.Id);
            }
        }
    }
}
