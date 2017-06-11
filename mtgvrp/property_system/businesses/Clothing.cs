using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.component_manager;
using RoleplayServer.core;
using RoleplayServer.inventory;
using RoleplayServer.inventory.bags;
using RoleplayServer.player_manager;

namespace RoleplayServer.property_system.businesses
{
    class Clothing : Script
    {
        List<string> _maleComponents = new List<string>();
        List<string> _femaleComponents = new List<string>();

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
            else if (eventName == "clothing_preview")
            {
                Character character = sender.GetCharacter();

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
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidMaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidMaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidMaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidMaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidMaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidMaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidMaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidMaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidMaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidMaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
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
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidFemaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidFemaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidFemaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidFemaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidFemaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidFemaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidFemaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidFemaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                    }
                }
                character.update_ped();
            }
            else if (eventName == "clothing_buyclothe")
            {
                Character character = sender.GetCharacter();
                int price = 0;
                switch ((int) arguments[0])
                {
                    case Component.ComponentTypeLegs:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["0"];
                        break;
                    case Component.ComponentTypeShoes:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["1"];
                        break;
                    case Component.ComponentTypeAccessories:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["2"];
                        break;
                    case Component.ComponentTypeUndershirt:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["3"];
                        break;
                    case Component.ComponentTypeTops:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["4"];
                        break;
                    case Component.ComponentTypeHats:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["5"];
                        break;
                    case Component.ComponentTypeGlasses:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["6"];
                        break;
                    case Component.ComponentTypeEars:
                        price = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"))
                            .ItemPrices["7"];
                        break;
                }

                if (Money.GetCharacterMoney(character) < price)
                {
                    API.sendChatMessageToPlayer(sender, "You don't have enough money.");
                    return;
                }

                InventoryManager.DeleteInventoryItem(character, typeof(Money), price);
                var prop = PropertyManager.Properties.Single(x => x.Id == sender.getData("clothing_id"));
                InventoryManager.GiveInventoryItem(prop, new Money(), price);

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
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidMaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidMaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidMaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidMaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidMaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidMaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidMaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidMaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidMaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidMaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
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
                            break;
                        case Component.ComponentTypeUndershirt:
                            character.Model.UndershirtStyle = ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].ComponentId;
                            character.Model.UndershirtVar = (int)ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeTops:
                            character.Model.TopStyle = ComponentManager.ValidFemaleTops[(int)arguments[1]].ComponentId;
                            character.Model.TopVar = (int)ComponentManager.ValidFemaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeHats:
                            character.Model.HatStyle = ComponentManager.ValidFemaleHats[(int)arguments[1]].ComponentId;
                            character.Model.HatVar = (int)ComponentManager.ValidFemaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeGlasses:
                            character.Model.GlassesStyle = ComponentManager.ValidFemaleGlasses[(int)arguments[1]].ComponentId;
                            character.Model.GlassesVar = (int)ComponentManager.ValidFemaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.ComponentTypeEars:
                            character.Model.EarStyle = ComponentManager.ValidFemaleEars[(int)arguments[1]].ComponentId;
                            character.Model.EarVar = (int)ComponentManager.ValidFemaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                            break;
                    }
                }

                character.update_ped();
                character.Save();

                API.sendChatMessageToPlayer(sender, "You've successfully bought this.");
                API.triggerClientEvent(sender, "clothing_boughtsucess", arguments[0], arguments[1], arguments[2]);
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
                    BagDesign = bagvar
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
            foreach (var c in ComponentManager.ValidMaleLegs)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeLegs,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleShoes)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeShoes,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleAccessories)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeAccessories,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleUndershirt)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeUndershirt,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleTops)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeTops,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleHats)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeHats,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleGlasses)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeGlasses,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidMaleEars)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeEars,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _maleComponents.Add(API.toJson(dic));
            }


            foreach (var c in ComponentManager.ValidFemaleLegs)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeLegs,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleShoes)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeShoes,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleAccessories)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeAccessories,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleUndershirt)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeUndershirt,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleTops)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeTops,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleHats)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeHats,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleGlasses)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeGlasses,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }

            foreach (var c in ComponentManager.ValidFemaleEars)
            {
                var dic = new Dictionary<string, object>
                {
                    ["type"] = Component.ComponentTypeEars,
                    ["name"] = c.Name,
                    ["id"] = c.ComponentId,
                    ["variations"] = c.Variations.Count
                };
                _femaleComponents.Add(API.toJson(dic));
            }
            API.consoleOutput("Finished loading componentes into array for clothes.");
        }

        [Command("buyclothes")]
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
                var pantsVar = pantsStyle.Variations.IndexOf(character.Model.PantsVar);

                var shoesStyle = ComponentManager.ValidFemaleShoes.SingleOrDefault(x => x.ComponentId == character.Model.ShoeStyle);
                var shoeid = ComponentManager.ValidFemaleShoes.IndexOf(shoesStyle);
                var shoesVar = shoesStyle.Variations.IndexOf(character.Model.ShoeVar);

                var accessoriesStyle = ComponentManager.ValidFemaleAccessories.SingleOrDefault(x => x.ComponentId == character.Model.AccessoryStyle);
                var accessoryid = ComponentManager.ValidFemaleAccessories.IndexOf(accessoriesStyle);
                var accessoryVar = accessoriesStyle.Variations.IndexOf(character.Model.AccessoryVar);

                var undershirtStyle = ComponentManager.ValidFemaleUndershirt.SingleOrDefault(x => x.ComponentId == character.Model.UndershirtStyle);
                var undershirtid = ComponentManager.ValidFemaleUndershirt.IndexOf(undershirtStyle);
                var undershirtVar = undershirtStyle.Variations.IndexOf(character.Model.UndershirtVar);

                var topStyle = ComponentManager.ValidFemaleTops.SingleOrDefault(x => x.ComponentId == character.Model.TopStyle);
                var topid = ComponentManager.ValidFemaleTops.IndexOf(topStyle);
                var topVar = topStyle.Variations.IndexOf(character.Model.TopVar);

                var hatsStyle = ComponentManager.ValidFemaleHats.SingleOrDefault(x => x.ComponentId == character.Model.HatStyle);
                var hatsid = ComponentManager.ValidFemaleHats.IndexOf(hatsStyle);
                var hatsVar = hatsStyle.Variations.IndexOf(character.Model.HatVar);

                var glassesStyle = ComponentManager.ValidFemaleGlasses.SingleOrDefault(x => x.ComponentId == character.Model.GlassesStyle);
                var glassesid = ComponentManager.ValidFemaleGlasses.IndexOf(glassesStyle);
                var glassesVar = glassesStyle.Variations.IndexOf(character.Model.GlassesVar);

                var earStyle = ComponentManager.ValidFemaleEars.SingleOrDefault(x => x.ComponentId == character.Model.EarStyle);
                var earid = ComponentManager.ValidFemaleEars.IndexOf(earStyle);
                var earsVar = earStyle.Variations.IndexOf(character.Model.EarVar);

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
            API.triggerClientEvent(player, "properties_buyclothes", (character.Model.Gender == Character.GenderMale ? _maleComponents : _femaleComponents), API.toJson(oldClothes), API.toJson(prices));
        }

        [Command("buybag")]
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
