using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.component_manager;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.player_manager;
using mtgvrp.core.Help;

namespace mtgvrp.property_system.businesses
{
    class Clothing : Script
    {
        Dictionary<string, string> _maleComponents = new Dictionary<string, string>();
        Dictionary<string, string> _femaleComponents = new Dictionary<string, string>();

        public static string MaleComponents;
        public static string FemaleComponents;

        public Clothing()
        {

            API.onResourceStart += API_onResourceStart;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "closeclothingmenu")
            {
                API.freezePlayer(sender, false);
                sender.position = API.getEntityData(sender, "clothing_lastpos");
                sender.rotation = API.getEntityData(sender, "clothing_lastrot");
                API.sendChatMessageToPlayer(sender, "You have exiting the clothing menu.");
            }
            else if (eventName == "clothing_buyclothe")
            {
                Character character = sender.GetCharacter();
                int price = 0;
                switch ((int) arguments[0])
                {
                    case Component.ComponentTypeLegs:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["pants"];
                        break;
                    case Component.ComponentTypeShoes:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["shoes"];
                        break;
                    case Component.ComponentTypeAccessories:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["accessories"];
                        break;
                    case Component.ComponentTypeUndershirt:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["undershirts"];
                        break;
                    case Component.ComponentTypeTops:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["tops"];
                        break;
                    case Component.ComponentTypeHats:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["hats"];
                        break;
                    case Component.ComponentTypeGlasses:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["glasses"];
                        break;
                    case Component.ComponentTypeEars:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["earrings"];
                        break;
                    case Component.ComponentTypeTorso:
                        price = 0;
                        break;
                }
                var prop = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"));

                if (prop.Supplies <= 0)
                {
                    API.sendChatMessageToPlayer(sender, "The business is out of supplies.");
                    return;
                }

                if (Money.GetCharacterMoney(character) < price)
                {
                    API.sendChatMessageToPlayer(sender, "You don't have enough money.");
                    return;
                }

                if (character.Model.Gender == Character.GenderMale)
                {
                    switch ((int)arguments[0])
                    {
                        case Component.ComponentTypeLegs:
                            character.Model.PantsStyle = ComponentManager.ValidMaleLegs[(int)arguments[1]].ComponentId;
                            character.Model.PantsVar = (int)ComponentManager.ValidMaleLegs[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeShoes:
                            character.Model.ShoeStyle = ComponentManager.ValidMaleShoes[(int)arguments[1]].ComponentId;
                            character.Model.ShoeVar = (int)ComponentManager.ValidMaleShoes[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeAccessories:
                            character.Model.AccessoryStyle = ComponentManager.ValidMaleAccessories[(int)arguments[1]].ComponentId;
                            character.Model.AccessoryVar = (int)ComponentManager.ValidMaleAccessories[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidMaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidMaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidMaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidMaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidMaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidMaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidMaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidMaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidMaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidMaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int) arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeTorso:
                            character.Model.TorsoStyle = (int)arguments[1];
                            character.Model.TorsoVar = (int)arguments[2];
                            break;
                    }
                }
                else
                {
                    switch ((int)arguments[0])
                    {
                        case Component.ComponentTypeLegs:
                            character.Model.PantsStyle = ComponentManager.ValidFemaleLegs[(int)arguments[1]].ComponentId;
                            character.Model.PantsVar = (int)ComponentManager.ValidFemaleLegs[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeShoes:
                            character.Model.ShoeStyle = ComponentManager.ValidFemaleShoes[(int)arguments[1]].ComponentId;
                            character.Model.ShoeVar = (int)ComponentManager.ValidFemaleShoes[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeAccessories:
                            character.Model.AccessoryStyle = ComponentManager.ValidFemaleAccessories[(int)arguments[1]].ComponentId;
                            character.Model.AccessoryVar = (int)ComponentManager.ValidFemaleAccessories[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidFemaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidFemaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidFemaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidFemaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidFemaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidFemaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidFemaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidFemaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);

                            if ((int)arguments[1] == 0)
                                price = 0;
                            break;
                        case Component.ComponentTypeTorso:
                            character.Model.TorsoStyle = (int)arguments[1];
                            character.Model.TorsoVar = (int)arguments[2];
                            break;
                    }
                }

                InventoryManager.DeleteInventoryItem(character, typeof(Money), price);
                InventoryManager.GiveInventoryItem(prop, new Money(), price);
                prop.Supplies -= 1;

                character.update_ped();
                character.Save();

                API.sendChatMessageToPlayer(sender, "You've successfully bought this.");
                API.triggerClientEvent(sender, "clothing_boughtsucess", arguments[0], arguments[1], arguments[2]);
                LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought some clothing for {price} from property ID {prop.Id}.");
            }
            else if (eventName == "clothing_bag_preview")
            {
                var bagstyle = ComponentManager.ValidBags[(int)arguments[0]].ComponentId;
                var bagvar = (int)ComponentManager.ValidBags[(int)arguments[0]].Variations.ToArray().GetValue((int)arguments[1]);
                API.setPlayerClothes(sender, 5, bagstyle, bagvar - 1);
            }
            else if(eventName == "clothing_bag_closed")
            {
                API.freezePlayer(sender, false);
                sender.position = API.getEntityData(sender, "clothing_lastpos");
                sender.rotation = API.getEntityData(sender, "clothing_lastrot");

                API.setPlayerClothes(sender, 5, 0, 0);

                var bag = InventoryManager.DoesInventoryHaveItem<BagItem>(sender.GetCharacter());
                if (bag.Length > 0)
                {
                    var bagg = (BagItem) bag[0];
                    API.setPlayerClothes(sender, 5, bagg.BagType, bagg.BagDesign);
                }
            }
            else if (eventName == "clothing_buybag")
            {
                var price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                    .ItemPrices["8"];

                if (Money.GetCharacterMoney(sender.GetCharacter()) < price)
                {
                    API.sendChatMessageToPlayer(sender, "You don't have enough money.");
                    return;
                }

                var bagstyle = ComponentManager.ValidBags[(int)arguments[0]].ComponentId;
                var bagvar = (int)ComponentManager.ValidBags[(int)arguments[0]].Variations.ToArray().GetValue((int)arguments[1]) - 1;

                var bag = new BagItem()
                {
                    BagType = bagstyle,
                    BagDesign = bagvar,
                    BagName = "default"
                };
                switch (InventoryManager.GiveInventoryItem(sender.GetCharacter(), bag))
                {
                    case InventoryManager.GiveItemErrors.Success:
                        InventoryManager.DeleteInventoryItem(sender.GetCharacter(), typeof(Money), price);
                        var prop = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"));
                        InventoryManager.GiveInventoryItem(prop, new Money(), price);
                        API.sendChatMessageToPlayer(sender, "You've successfully bought this.");
                        break;
                    case InventoryManager.GiveItemErrors.HasBlockingItem:
                        API.sendChatMessageToPlayer(sender, "You have a blocking item.");
                        break;
                    case InventoryManager.GiveItemErrors.MaxAmountReached:
                        API.sendChatMessageToPlayer(sender, "You have reached the maximum amount.");
                        break;
                    case InventoryManager.GiveItemErrors.NotEnoughSpace:
                        API.sendChatMessageToPlayer(sender, "You don't have enough space for that item.");
                        break;
                }

            }
        }

        private void API_onResourceStart()
        {
            API.consoleOutput("Loading componentes into array for clothes.");

            _maleComponents.Add("Legs", API.toJson(ComponentManager.ValidMaleLegs.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Shoes", API.toJson(ComponentManager.ValidMaleShoes.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Accessories", API.toJson(ComponentManager.ValidMaleAccessories.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Undershirts", API.toJson(ComponentManager.ValidMaleUndershirt.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Tops", API.toJson(ComponentManager.ValidMaleTops.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Hats", API.toJson(ComponentManager.ValidMaleHats.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Glasses", API.toJson(ComponentManager.ValidMaleGlasses.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Ears", API.toJson(ComponentManager.ValidMaleEars.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            MaleComponents = API.toJson(_maleComponents);

            _femaleComponents.Add("Legs", API.toJson(ComponentManager.ValidFemaleLegs.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Shoes", API.toJson(ComponentManager.ValidFemaleShoes.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Accessories", API.toJson(ComponentManager.ValidFemaleAccessories.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Undershirts", API.toJson(ComponentManager.ValidFemaleUndershirt.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Tops", API.toJson(ComponentManager.ValidFemaleTops.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Hats", API.toJson(ComponentManager.ValidFemaleHats.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Glasses", API.toJson(ComponentManager.ValidFemaleGlasses.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Ears", API.toJson(ComponentManager.ValidFemaleEars.Select(x => new string[] { x.Name, x.ComponentId.ToString(), API.toJson(x.Variations) }).ToArray()));
            FemaleComponents = API.toJson(_femaleComponents);

            API.consoleOutput("Finished loading componentes into array for clothes.");
        }

        [Command("buyclothes"), Help(HelpManager.CommandGroups.Bussiness, "Used inside a clothing store to buy clothes.", null)]
        public void BuyClothes(Client player)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }

            API.setEntityData(player, "clothing_lastpos", player.position);
            API.setEntityData(player, "clothing_lastrot", player.rotation);
            API.setEntityData(player, "clothing_id", biz.Id);

            API.freezePlayer(player, true);

            var character = player.GetCharacter();

            var oldClothes = new List<int[]>();

            if (character.Model.Gender == Character.GenderMale)
            {
                var pantsStyle = ComponentManager.ValidMaleLegs.SingleOrDefault(x => x.ComponentId == character.Model.PantsStyle);
                var pantid = ComponentManager.ValidMaleLegs.IndexOf(pantsStyle);
                var pantsVar = pantsStyle.Variations.IndexOf(character.Model.PantsVar);

                var shoesStyle = ComponentManager.ValidMaleShoes.SingleOrDefault(x => x.ComponentId == character.Model.ShoeStyle);
                var shoeid = ComponentManager.ValidMaleShoes.IndexOf(shoesStyle);
                var shoesVar = shoesStyle.Variations.IndexOf(character.Model.ShoeVar);

                var accessoriesStyle = ComponentManager.ValidMaleAccessories.SingleOrDefault(x => x.ComponentId == character.Model.AccessoryStyle);
                var accessoryid = ComponentManager.ValidMaleAccessories.IndexOf(accessoriesStyle);
                var accessoryVar = accessoriesStyle.Variations.IndexOf(character.Model.AccessoryVar);

                var undershirtStyle = ComponentManager.ValidMaleUndershirt.SingleOrDefault(x => x.ComponentId == character.Model.UndershirtStyle);
                var undershirtid = ComponentManager.ValidMaleUndershirt.IndexOf(undershirtStyle);
                var undershirtVar = undershirtStyle.Variations.IndexOf(character.Model.UndershirtVar);

                var topStyle = ComponentManager.ValidMaleTops.SingleOrDefault(x => x.ComponentId == character.Model.TopStyle);
                var topid = ComponentManager.ValidMaleTops.IndexOf(topStyle);
                var topVar = topStyle.Variations.IndexOf(character.Model.TopVar);

                var hatsStyle = ComponentManager.ValidMaleHats.SingleOrDefault(x => x.ComponentId == character.Model.HatStyle);
                var hatsid = ComponentManager.ValidMaleHats.IndexOf(hatsStyle);
                var hatsVar = hatsStyle?.Variations.IndexOf(character.Model.HatVar) ?? -1;

                var glassesStyle = ComponentManager.ValidMaleGlasses.SingleOrDefault(x => x.ComponentId == character.Model.GlassesStyle);
                var glassesid = ComponentManager.ValidMaleGlasses.IndexOf(glassesStyle);
                var glassesVar = glassesStyle?.Variations.IndexOf(character.Model.GlassesVar) ?? -1;

                var earStyle = ComponentManager.ValidMaleEars.SingleOrDefault(x => x.ComponentId == character.Model.EarStyle);
                var earid = ComponentManager.ValidMaleEars.IndexOf(earStyle);
                var earsVar = earStyle?.Variations.IndexOf(character.Model.EarVar) ?? -1;

                oldClothes.Add(new [] { pantid, pantsVar });
                oldClothes.Add(new[] { shoeid, shoesVar });
                oldClothes.Add(new[] { accessoryid, accessoryVar });
                oldClothes.Add(new[] { undershirtid, undershirtVar });
                oldClothes.Add(new[] { topid, topVar });
                oldClothes.Add(new[] { hatsid, hatsVar });
                oldClothes.Add(new[] { glassesid, glassesVar });
                oldClothes.Add(new[] { earid, earsVar });
            }
            else
            {
                var pantsStyle = ComponentManager.ValidFemaleLegs.SingleOrDefault(x => x.ComponentId == character.Model.PantsStyle);
                var pantid = ComponentManager.ValidFemaleLegs.IndexOf(pantsStyle);
                var pantsVar = pantsStyle?.Variations.IndexOf(character.Model.PantsVar) ?? 0;

                var shoesStyle = ComponentManager.ValidFemaleShoes.SingleOrDefault(x => x.ComponentId == character.Model.ShoeStyle);
                var shoeid = ComponentManager.ValidFemaleShoes.IndexOf(shoesStyle);
                var shoesVar = shoesStyle?.Variations.IndexOf(character.Model.ShoeVar) ?? 0;

                var accessoriesStyle = ComponentManager.ValidFemaleAccessories.SingleOrDefault(x => x.ComponentId == character.Model.AccessoryStyle);
                var accessoryid = ComponentManager.ValidFemaleAccessories.IndexOf(accessoriesStyle);
                var accessoryVar = accessoriesStyle?.Variations.IndexOf(character.Model.AccessoryVar) ?? 0;

                var undershirtStyle = ComponentManager.ValidFemaleUndershirt.SingleOrDefault(x => x.ComponentId == character.Model.UndershirtStyle);
                var undershirtid = ComponentManager.ValidFemaleUndershirt.IndexOf(undershirtStyle);
                var undershirtVar = undershirtStyle?.Variations.IndexOf(character.Model.UndershirtVar) ?? 0;

                var topStyle = ComponentManager.ValidFemaleTops.SingleOrDefault(x => x.ComponentId == character.Model.TopStyle);
                var topid = ComponentManager.ValidFemaleTops.IndexOf(topStyle);
                var topVar = topStyle?.Variations.IndexOf(character.Model.TopVar) ?? 0;

                var hatsStyle = ComponentManager.ValidFemaleHats.SingleOrDefault(x => x.ComponentId == character.Model.HatStyle);
                var hatsid = ComponentManager.ValidFemaleHats.IndexOf(hatsStyle);
                var hatsVar = hatsStyle?.Variations.IndexOf(character.Model.HatVar) ?? 0;

                var glassesStyle = ComponentManager.ValidFemaleGlasses.SingleOrDefault(x => x.ComponentId == character.Model.GlassesStyle);
                var glassesid = ComponentManager.ValidFemaleGlasses.IndexOf(glassesStyle);
                var glassesVar = glassesStyle?.Variations.IndexOf(character.Model.GlassesVar) ?? 0;

                var earStyle = ComponentManager.ValidFemaleEars.SingleOrDefault(x => x.ComponentId == character.Model.EarStyle);
                var earid = ComponentManager.ValidFemaleEars.IndexOf(earStyle);
                var earsVar = earStyle?.Variations.IndexOf(character.Model.EarVar) ?? 0;

                oldClothes.Add(new[] { pantid, pantsVar });
                oldClothes.Add(new[] { shoeid, shoesVar });
                oldClothes.Add(new[] { accessoryid, accessoryVar });
                oldClothes.Add(new[] { undershirtid, undershirtVar });
                oldClothes.Add(new[] { topid, topVar });
                oldClothes.Add(new[] { hatsid, hatsVar });
                oldClothes.Add(new[] { glassesid, glassesVar });
                oldClothes.Add(new[] { earid, earsVar });
            }

            var prices = biz.ItemPrices.Select(x => x.Value).ToArray();
            API.triggerClientEvent(player, "properties_buyclothes", (character.Model.Gender == Character.GenderMale ? MaleComponents : FemaleComponents), API.toJson(oldClothes), API.toJson(prices));
        }

        [Command("buybag"), Help(HelpManager.CommandGroups.General, "Used inside a clothing store to buy a bag.", null)]
        public void Buybag(Client player)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }

            API.setEntityData(player, "clothing_lastpos", player.position);
            API.setEntityData(player, "clothing_lastrot", player.rotation);
            API.setEntityData(player, "clothing_id", biz.Id);

            //Setup bag list.
            var bagsList = ComponentManager.ValidBags.Select(x => new[] {x.Name, x.Variations.Count.ToString()}).ToArray();

            API.freezePlayer(player, true);
            API.triggerClientEvent(player, "properties_buybag", API.toJson(bagsList), biz.ItemPrices["8"]);
        }
    }
}
