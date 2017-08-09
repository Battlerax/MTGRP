using System;
using System.Collections.Generic;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using mtgvrp.core;
using mtgvrp.core.Items;
using mtgvrp.inventory;
using mtgvrp.phone_manager;
using mtgvrp.weapon_manager;
using mtgvrp.player_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lsgov;
using mtgvrp.job_manager.hunting;
using mtgvrp.job_manager.scuba;
using mtgvrp.core.Help;
using Color = mtgvrp.core.Color;

namespace mtgvrp.property_system.businesses
{
    class GeneralBuying : Script
    {
        public GeneralBuying()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {

            Character character = sender.GetCharacter();
            if (eventName == "property_exitbuy")
            {
                API.freezePlayer(sender, false);
            }
            else if (eventName == "property_buyitem")
            {
                var prop = PropertyManager.IsAtPropertyInteraction(sender);
                if (prop == null)
                {
                    API.sendChatMessageToPlayer(sender, "You aren't at a property or you moved away.");
                    return;
                }

                var itemName = (string) arguments[0];

                string name = "";
                int price = prop.ItemPrices.SingleOrDefault(x => x.Key == itemName).Value;

                //Make sure has enough money.
                if (Money.GetCharacterMoney(sender.GetCharacter()) < price)
                {
                    API.sendChatMessageToPlayer(sender, "Not Enough Money");
                    return;
                }

                if (prop.HasGarbagePoint)
                {
                    prop.GarbageBags += 1;
                    prop.UpdateMarkers();
                    if (prop.GarbageBags >= 10)
                    {
                        job_manager.garbageman.Garbageman.SendNotificationToGarbagemen("A business is overflowing with garbage. We need garbagemen on the streets right now!");
                    }
                }

                if (prop.Supplies <= 0)
                {
                    API.sendChatMessageToPlayer(sender, "The business is out of supplies.");
                    return;
                }

                IInventoryItem item = null;
                if (prop.Type == PropertyManager.PropertyTypes.TwentyFourSeven)
                {
                    name = ItemManager.TwentyFourSevenItems.Single(x => x[0] == itemName)[1];
                    switch (itemName)
                    {
                        case "sprunk":
                            item = new SprunkItem();
                            break;

                        case "rope":
                            item = new RopeItem();
                            break;

                        case "rags":
                            item = new RagsItem();
                            break;

                    }
                }
                else if (prop.Type == PropertyManager.PropertyTypes.Hardware)
                {
                    name = ItemManager.HardwareItems.Single(x => x[0] == itemName)[1];
                    switch (itemName)
                    {
                        case "phone":
                            var number = PhoneManager.GetNewNumber();
                            var phone = new Phone()
                            {
                                PhoneNumber = number,
                                PhoneName = "default"
                            };
                            item = phone;
                            break;

                        case "rope":
                            item = new RopeItem();
                            break;

                        case "rags":
                            item = new RagsItem();
                            break;
                        case "axe":
                            if (InventoryManager
                                    .DoesInventoryHaveItem<Weapon>(character, x => x.WeaponHash == WeaponHash.Hatchet)
                                    .Length > 0)
                            {
                                API.sendChatMessageToPlayer(sender, "You already have that weapon.");
                                return;
                            }

                            WeaponManager.CreateWeapon(sender, WeaponHash.Hatchet, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            prop.Supplies--;
                            API.sendChatMessageToPlayer(sender,
                                $"[BUSINESS] You have sucessfully bought an ~g~Axe~w~ for ~g~${price}.");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought an Axe for {price} from property ID {prop.Id}.");
                            return;
                        case "scuba":
                            item = new ScubaItem();
                            break;
                        case "engineparts":
                            item = new EngineParts();
                            break;
                        case "spraypaint":
                            item = new SprayPaint();
                            break;
                    }
                }
                else if (prop.Type == PropertyManager.PropertyTypes.Restaurant)
                {
                    prop.Supplies--;
                    switch (itemName)
                    {
                        case "sprunk":
                            name = "Sprunk";
                            item = new SprunkItem();
                            break;

                        case "custom1":
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            sender.health += 15;
                            if (sender.health > 100) sender.health = 100;
                            API.sendChatMessageToPlayer(sender,
                                $"[BUSINESS] You have sucessfully bought a ~g~{prop.RestaurantItems[0]}~w~ for ~g~${price}.");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {prop.RestaurantItems[0]} for {price} from property ID {prop.Id}.");
                            return;

                        case "custom2":
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            sender.health += 25;
                            if (sender.health > 100) sender.health = 100;
                            API.sendChatMessageToPlayer(sender,
                                $"[BUSINESS] You have sucessfully bought a ~g~{prop.RestaurantItems[1]}~w~ for ~g~${price}.");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {prop.RestaurantItems[1]} for {price} from property ID {prop.Id}.");
                            return;

                        case "custom3":
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            sender.health += 25;
                            if (sender.health > 100) sender.health = 100;
                            API.sendChatMessageToPlayer(sender,
                                $"[BUSINESS] You have sucessfully bought a ~g~{prop.RestaurantItems[2]}~w~ for ~g~${price}.");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {prop.RestaurantItems[2]} for {price} from property ID {prop.Id}.");
                            return;

                        case "custom4":
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            sender.health += 25;
                            if (sender.health > 100) sender.health = 100;
                            API.sendChatMessageToPlayer(sender,
                                $"[BUSINESS] You have sucessfully bought a ~g~{prop.RestaurantItems[3]}~w~ for ~g~${price}.");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {prop.RestaurantItems[3]} for {price} from property ID {prop.Id}.");
                            return;
                    }
                }
                else if (prop.Type == PropertyManager.PropertyTypes.Ammunation)
                {
                    if (character.GetPlayingHours() < 2)
                    {
                        sender.sendChatMessage("You must have more than 2 playing hours to buy a weapon.");
                        return;
                    }
                    prop.Supplies--;
                    switch (itemName)
                    {
                        case "bat":
                            WeaponManager.CreateWeapon(sender, WeaponHash.Bat, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            break;
                        case "pistol":
                            WeaponManager.CreateWeapon(sender, WeaponHash.Pistol, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            break;
                        case "combat_pistol":
                            WeaponManager.CreateWeapon(sender, WeaponHash.CombatPistol, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            break;
                        case "heavy_pistol":
                            WeaponManager.CreateWeapon(sender, WeaponHash.HeavyPistol, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            break;
                        case "revolver":
                            WeaponManager.CreateWeapon(sender, WeaponHash.Revolver, WeaponTint.Normal, true);
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            break;
                    }
                    name = ItemManager.AmmunationItems.Single(x => x[0] == itemName)[1];

                    API.sendChatMessageToPlayer(sender, "[BUSINESSES] You have successfully bought a ~g~" + name + "~w~ for ~g~" + price + "~w~.");
                    LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {name} for {price} from property ID {prop.Id}.");
                    return;

                }
                else if (prop.Type == PropertyManager.PropertyTypes.VIPLounge)
                {
                    prop.Supplies--;

                    if (API.getPlayerCurrentWeapon(sender) == WeaponHash.Unarmed)
                    {
                        sender.sendChatMessage("You must be holding the weapon you want to tint.");
                        return;
                    }

                    switch (itemName)
                    {
                        case "pink_tint":
                            WeaponManager.SetWeaponTint(sender, API.getPlayerCurrentWeapon(sender), WeaponTint.Pink);
                            break;
                        case "gold_tint":
                            if (sender.GetAccount().VipLevel < 3)
                            {
                                sender.sendChatMessage("You must be a gold VIP to buy a gold tint.");
                                return;
                            }
                            WeaponManager.SetWeaponTint(sender, API.getPlayerCurrentWeapon(sender), WeaponTint.Gold);
                            break;
                        case "green_tint":
                            WeaponManager.SetWeaponTint(sender, API.getPlayerCurrentWeapon(sender), WeaponTint.Green);
                            break;
                        case "orange_tint":
                            WeaponManager.SetWeaponTint(sender, API.getPlayerCurrentWeapon(sender), WeaponTint.Orange);
                            break;
                        case "platinum_tint":
                            WeaponManager.SetWeaponTint(sender, API.getPlayerCurrentWeapon(sender), WeaponTint.Platinum);
                            break;

                    }
                    name = ItemManager.VIPItems.Single(x => x[0] == itemName)[1];

                    API.sendChatMessageToPlayer(sender, "[BUSINESSES] You have successfully bought a ~g~" + name + "~w~ weapon tint for ~g~" + price + "~w~.");
                    LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {name} for {price} from property ID {prop.Id}.");
                    return;

                }
                else if (prop.Type == PropertyManager.PropertyTypes.LSNN)
                {
                    switch (itemName)
                    {
                        case "lotto_ticket":
                            foreach (var i in GroupManager.Groups)
                            {
                                if (i.CommandType == Group.CommandTypeLsnn) { i.LottoSafe += price; }
                            }
                            InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                            character.HasLottoTicket = true;
                            API.sendChatMessageToPlayer(sender, "You purchased a lottery ticket. Good luck!");
                            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a lottery ticket for {price} from property ID {prop.Id}.");
                            return;
                    }
                    return;
                }
                else if (prop.Type == PropertyManager.PropertyTypes.HuntingStation)
                {
                    HuntingTag boughtTag = null;

                    switch (itemName)
                    {
                        case "deer_tag":
                        {
                            if (sender.GetCharacter().LastRedeemedDeerTag == DateTime.Today.Date)
                            {
                                API.sendChatMessageToPlayer(sender, Color.White,
                                    "~r~[ERROR]~w~ You have already redeemed a deer tag today.");
                                return;
                            }

                            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
                            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Deer))
                            {
                                API.sendChatMessageToPlayer(sender, Color.White,
                                    "~r~[ERROR]~w~ You have already purchased a deer tag. Please drop any old ones before buying a new one.");
                                return;
                            }

                            boughtTag = new HuntingTag
                            {
                                Type = HuntingManager.AnimalTypes.Deer,
                                ValidDate = DateTime.Today
                            };
                            name = "Deer Tag";
                            WeaponManager.CreateWeapon(sender, WeaponHash.SniperRifle, WeaponTint.Normal, true);
                            break;
                        }
                        case "boar_tag":
                        {
                            if (sender.GetCharacter().LastRedeemedBoarTag == DateTime.Today.Date)
                            {
                                API.sendChatMessageToPlayer(sender, Color.White,
                                    "~r~[ERROR]~w~ You have already redeemed a boar tag today.");
                                return;
                            }

                            var tags = InventoryManager.DoesInventoryHaveItem(character, typeof(HuntingTag));
                            if (tags.Cast<HuntingTag>().Any(i => i.Type == HuntingManager.AnimalTypes.Boar))
                            {
                                API.sendChatMessageToPlayer(sender, Color.White,
                                    "~r~[ERROR]~w~ You have already purchased a boar tag. Please drop any old ones before buying a new one.");
                                return;
                            }

                            boughtTag = new HuntingTag
                            {
                                Type = HuntingManager.AnimalTypes.Boar,
                                ValidDate = DateTime.Today
                            };
                            name = "Boar Tag";
                            WeaponManager.CreateWeapon(sender, WeaponHash.SniperRifle, WeaponTint.Normal, true);
                            break;
                        }
                        case "ammo":
                        {
                            switch (InventoryManager.GiveInventoryItem(sender.GetCharacter(), new AmmoItem()))
                            {
                                case InventoryManager.GiveItemErrors.Success:
                                    InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                                    prop.Supplies--;
                                    WeaponManager.CreateWeapon(sender, WeaponHash.SniperRifle, WeaponTint.Normal, true);
                                    API.sendChatMessageToPlayer(sender,
                                        $"[BUSINESS] You have sucessfully bought a ~g~ 5.56 Bullet ~w~ for ~g~${price}.");
                                    LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a 5.56 Bullet for {price} from property ID {prop.Id}.");
                                        break;

                                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                                    API.sendChatMessageToPlayer(sender,
                                        $"[BUSINESS] You dont have enough space for that item. Need {new AmmoItem().AmountOfSlots} Slots.");
                                    break;

                                case InventoryManager.GiveItemErrors.MaxAmountReached:
                                    API.sendChatMessageToPlayer(sender,
                                        $"[BUSINESS] You have reached the maximum allowed ammount of that item.");
                                    break;
                            }
                            break;
                        }

                       
                    }
                    if (boughtTag != null)
                    {
                        switch (InventoryManager.GiveInventoryItem(sender.GetCharacter(), boughtTag))
                        {
                            case InventoryManager.GiveItemErrors.Success:
                                InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                                InventoryManager.GiveInventoryItem(sender.GetCharacter(), new AmmoItem());
                                prop.Supplies--;
                                API.sendChatMessageToPlayer(sender,
                                    $"[BUSINESS] You have sucessfully bought a ~g~{name}~w~ for ~g~${price}.");
                                LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {name} for {price} from property ID {prop.Id}.");
                                break;

                            case InventoryManager.GiveItemErrors.NotEnoughSpace:
                                API.sendChatMessageToPlayer(sender,
                                    $"[BUSINESS] You dont have enough space for that item. Need {boughtTag.AmountOfSlots} Slots.");
                                break;

                            case InventoryManager.GiveItemErrors.MaxAmountReached:
                                API.sendChatMessageToPlayer(sender,
                                    $"[BUSINESS] You have reached the maximum allowed amount of that item.");
                                break;
                        }
                        return;
                    }
                    else return;
                }
                else if (prop.Type == PropertyManager.PropertyTypes.Government)
                {
                    name = ItemManager.GovItems.Single(x => x[0] == itemName)[1];
                    switch (itemName)
                    {
                        case "id":
                            item = new IdentificationItem();
                            break;
                    }
                }

                if (item == null)
                {
                    API.sendChatMessageToPlayer(sender,
                        "Error finding the item you bought, report this as a bug report.");
                    return;
                }

                //Send message.
                switch (InventoryManager.GiveInventoryItem(sender.GetCharacter(), item))
                {
                    case InventoryManager.GiveItemErrors.Success:
                        InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                        InventoryManager.GiveInventoryItem(prop, new Money(), price);

                        if (item.GetType() == typeof(Phone))
                        {
                            ((Phone)item).InsertNumber();
                            API.sendChatMessageToPlayer(sender, "Your phone number is: ~g~" + ((Phone)item).PhoneNumber);
                        }
                        prop.Supplies--;
                        API.sendChatMessageToPlayer(sender,
                            $"[BUSINESS] You have sucessfully bought a ~g~{name}~w~ for ~g~${price}.");

                        LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought a {name} for {price} from property ID {prop.Id}.");
                        break;

                    case InventoryManager.GiveItemErrors.NotEnoughSpace:
                        API.sendChatMessageToPlayer(sender,
                            $"[BUSINESS] You dont have enough space for that item. Need {item.AmountOfSlots} Slots.");
                        break;

                    case InventoryManager.GiveItemErrors.MaxAmountReached:
                        API.sendChatMessageToPlayer(sender,
                            $"[BUSINESS] You have reached the maximum allowed amount of that item.");
                        break;

                   case InventoryManager.GiveItemErrors.HasSimilarItem:
                       API.sendChatMessageToPlayer(sender,
                           $"[BUSINESS] You already have a similar item.");
                        break;

                    case InventoryManager.GiveItemErrors.HasBlockingItem:
                        API.sendChatMessageToPlayer(sender,
                            $"[BUSINESS] You have a blocking item, please store it somewhere first.");
                        break;
                }
            }
        }

        [Command("buy"), Help(HelpManager.CommandGroups.Bussiness, "Used inside businesses to buy items.", null)]
        public void Buy(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a property.");
                return;
            }

            if (player.isInVehicle)
            {
                API.sendChatMessageToPlayer(player, "You cannot /buy while in a vehicle.");
                return;
            }

            switch (prop.Type)
            {
                case PropertyManager.PropertyTypes.Hardware:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.HardwareItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "Hardware",
                        prop.PropertyName);
                }
                    break;
                case PropertyManager.PropertyTypes.TwentyFourSeven:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.TwentyFourSevenItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "24/7",
                        prop.PropertyName);
                }
                    break;
                case PropertyManager.PropertyTypes.Restaurant:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == 0)
                        {
                            itemsWithPrices.Add(new[]
                            {
                                "sprunk", "Sprunk", "", prop.ItemPrices["sprunk"].ToString()
                            });
                            continue;
                        }

                        itemsWithPrices.Add(new[]
                        {
                            "custom" + i, prop.RestaurantItems[i - 1], "", prop.ItemPrices["custom" + i].ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "Restaurant",
                        prop.PropertyName);
                }
                    break;
                case PropertyManager.PropertyTypes.Ammunation:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.AmmunationItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "Ammunation",
                        prop.PropertyName);
                }
                    break;

                case PropertyManager.PropertyTypes.LSNN:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.LSNNItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()),
                        "Los Santos News Network",
                        prop.PropertyName);
                }
                    break;
                case PropertyManager.PropertyTypes.HuntingStation:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.HuntingItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()),
                        "Hunting Shop",
                        prop.PropertyName);
                }
                    break;

                case PropertyManager.PropertyTypes.Government:
                {
                    API.freezePlayer(player, true);
                    List<string[]> itemsWithPrices = new List<string[]>();
                    foreach (var itm in ItemManager.GovItems)
                    {
                        itemsWithPrices.Add(new[]
                        {
                            itm[0], itm[1], itm[2],
                            prop.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                        });
                    }
                    API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "Government",
                        prop.PropertyName);
                        break;
                }

                default:
                    API.sendChatMessageToPlayer(player, "This property doesn't sell anything.");
                    break;
            }
        }
    }
}
