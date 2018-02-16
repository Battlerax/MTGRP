using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using GTANetworkAPI;



using mtgvrp.component_manager;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.player_manager;
using mtgvrp.core.Help;

namespace mtgvrp.property_system.businesses
{
    public class Clothing : Script
    {
        Dictionary<string, string> _maleComponents = new Dictionary<string, string>();
        Dictionary<string, string> _femaleComponents = new Dictionary<string, string>();

        PedHash[] illegalpeds = new PedHash[] {
            PedHash.ChickenHawk,
            PedHash.Boar,
            PedHash.BradCadaverCutscene,
            PedHash.Cat,
            PedHash.Chimp,
            PedHash.Chop,
            PedHash.Cormorant,
            PedHash.Cow,
            PedHash.Coyote,
            PedHash.Crow,
            PedHash.Deer,
            PedHash.Dolphin,
            PedHash.Fish,
            PedHash.HammerShark,
            PedHash.Hen,
            PedHash.Humpback,
            PedHash.Husky,
            PedHash.KillerWhale,
            PedHash.MountainLion,
            PedHash.MovAlien01,
            PedHash.Niko01,
            PedHash.Pig,
            PedHash.Pigeon,
            PedHash.Poodle,
            PedHash.Pug,
            PedHash.Rabbit,
            PedHash.Rat,
            PedHash.Retriever,
            PedHash.Rhesus,
            PedHash.Rottweiler,
            PedHash.Seagull,
            PedHash.Shepherd,
            PedHash.Stingray,
            PedHash.TigerShark,
            PedHash.Westy,
            PedHash.FreemodeMale01,
            PedHash.FreemodeFemale01,
            PedHash.Acult01AMM,
            PedHash.Acult01AMO,
            PedHash.Acult01AMY,
            PedHash.Acult02AMY,
            PedHash.Armoured01,
            PedHash.Armoured01SMM,
            PedHash.Armoured02SMM,
            PedHash.Armymech01SMY,
            PedHash.Babyd,
            PedHash.Blackops01SMY,
            PedHash.Blackops02SMY,
            PedHash.Blackops03SMY,
            PedHash.Casey,
            PedHash.CaseyCutscene,
            PedHash.CopCutscene,
            PedHash.FatCult01AFM,
            PedHash.FibArchitect,
            PedHash.FibOffice01SMM,
            PedHash.GarbageSMY,
            PedHash.JohnnyKlebitz,
            PedHash.Justin,
            PedHash.Marston01,
            PedHash.MerryWeatherCutscene,
            PedHash.Movspace01SMM,
            PedHash.Musclbeac01AMY,
            PedHash.Orleans,
            PedHash.OrleansCutscene,
            PedHash.PestContGunman,
            PedHash.Pogo01,
            PedHash.Prisguard01SMM,
            PedHash.PrisMuscl01SMY,
            PedHash.Prisoner01,
            PedHash.Prisoner01SMY,
            PedHash.PrologueSec01,
            PedHash.PrologueSec01Cutscene,
            PedHash.PrologueSec02,
            PedHash.PrologueSec02Cutscene,
            PedHash.RampMarineCutscene,
            PedHash.RashkovskyCutscene,
            PedHash.RsRanger01AMO,
            PedHash.Security01SMM,
            PedHash.Staggrm01AMO,
            PedHash.Strperf01SMM,
            PedHash.TrafficWarden,
            PedHash.TrafficWardenCutscene,
            PedHash.Tranvest01AMM,
            PedHash.Tranvest02AMM,
            PedHash.Zombie01,
        };

        public static string MaleComponents;
        public static string FemaleComponents;

        [RemoteEvent("closeclothingmenu")]
        public void CloseClothingMenu(Client sender, params object[] arguments)
        {
            NAPI.Player.FreezePlayer(sender, false);
            sender.Position = NAPI.Data.GetEntityData(sender, "clothing_lastpos");
            sender.Rotation = NAPI.Data.GetEntityData(sender, "clothing_lastrot");
            NAPI.Entity.SetEntityDimension(sender, 0);
            NAPI.Chat.SendChatMessageToPlayer(sender, "You have exited the clothing menu.");
        }

        [RemoteEvent("returnPedGender")]
        public void ReturnPedGender(Client sender, params object[] arguments)
        {
            setPlayerPedSkin(sender, (PedHash)arguments[0], (int)arguments[1]);
        }

        [RemoteEvent("clothing_buyclothe")]
        public void ClothingBuyClothes(Client sender, params object[] arguments)
        {
            Character character = sender.GetCharacter();
            int price = 0;
            switch ((int)arguments[0])
            {
                case Component.ComponentTypeLegs:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["pants"];
                    break;
                case Component.ComponentTypeShoes:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["shoes"];
                    break;
                case Component.ComponentTypeAccessories:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["accessories"];
                    break;
                case Component.ComponentTypeUndershirt:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["undershirts"];
                    break;
                case Component.ComponentTypeTops:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["tops"];
                    break;
                case Component.ComponentTypeHats:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["hats"];
                    break;
                case Component.ComponentTypeGlasses:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["glasses"];
                    break;
                case Component.ComponentTypeEars:
                    price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                        .ItemPrices["earrings"];
                    break;
                case Component.ComponentTypeTorso:
                    price = 0;
                    break;
            }
            var prop = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"));

            if (prop.Supplies <= 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "The business is out of supplies.");
                return;
            }

            if (Money.GetCharacterMoney(character) < price)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough money.");
                return;
            }

            NAPI.Util.ConsoleOutput($"CHARACTER BUY CLOTHES: {arguments[0]} | {arguments[1]} | {arguments[2]}");
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

                        if ((int)arguments[1] == 0)
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

            NAPI.Chat.SendChatMessageToPlayer(sender, "You've successfully bought this.");
            NAPI.ClientEvent.TriggerClientEvent(sender, "clothing_boughtsucess", arguments[0], arguments[1], arguments[2]);
            LogManager.Log(LogManager.LogTypes.Stats, $"[Business] {sender.GetCharacter().CharacterName}[{sender.GetAccount().AccountName}] has bought some clothing for {price} from property ID {prop.Id}.");
        }

        [RemoteEvent("clothing_bag_preview")]
        public void ClothingBagPreview(Client sender, params object[] arguments)
        {
            var bagstyle = ComponentManager.ValidBags[(int)arguments[0]].ComponentId;
            var bagvar = (int)ComponentManager.ValidBags[(int)arguments[0]].Variations.ToArray().GetValue((int)arguments[1]);
            NAPI.Player.SetPlayerClothes(sender, 5, bagstyle, bagvar - 1);
        }

        [RemoteEvent("clothing_bag_closed")]
        public void ClothingBagClosed(Client sender, params object[] arguments)
        {
            NAPI.Player.FreezePlayer(sender, false);
            sender.Position = NAPI.Data.GetEntityData(sender, "clothing_lastpos");
            sender.Rotation = NAPI.Data.GetEntityData(sender, "clothing_lastrot");
            NAPI.Entity.SetEntityDimension(sender, 0);

            NAPI.Player.SetPlayerClothes(sender, 5, 0, 0);

            var bag = InventoryManager.DoesInventoryHaveItem<BagItem>(sender.GetCharacter());
            if (bag.Length > 0)
            {
                var bagg = (BagItem)bag[0];
                NAPI.Player.SetPlayerClothes(sender, 5, bagg.BagType, bagg.BagDesign);
            }
        }

        [RemoteEvent("clothing_buybag")]
        public void ClothingBuyBag(Client sender, params object[] arguments)
        {
            var price = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"))
                    .ItemPrices["8"];

            if (Money.GetCharacterMoney(sender.GetCharacter()) < price)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough money.");
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
                    var prop = PropertyManager.Properties.Single(x => x.Id == sender.GetData("clothing_id"));
                    InventoryManager.GiveInventoryItem(prop, new Money(), price);
                    NAPI.Chat.SendChatMessageToPlayer(sender, "You've successfully bought this.");
                    break;
                case InventoryManager.GiveItemErrors.MaxAmountReached:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "You have reached the maximum amount.");
                    break;
                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                    NAPI.Chat.SendChatMessageToPlayer(sender, "You don't have enough space for that item.");
                    break;
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Util.ConsoleOutput("Loading componentes into array for clothes.");

            _maleComponents.Add("Legs", NAPI.Util.ToJson(ComponentManager.ValidMaleLegs.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Shoes", NAPI.Util.ToJson(ComponentManager.ValidMaleShoes.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Accessories", NAPI.Util.ToJson(ComponentManager.ValidMaleAccessories.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Undershirts", NAPI.Util.ToJson(ComponentManager.ValidMaleUndershirt.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Tops", NAPI.Util.ToJson(ComponentManager.ValidMaleTops.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Hats", NAPI.Util.ToJson(ComponentManager.ValidMaleHats.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Glasses", NAPI.Util.ToJson(ComponentManager.ValidMaleGlasses.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _maleComponents.Add("Ears", NAPI.Util.ToJson(ComponentManager.ValidMaleEars.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            MaleComponents = NAPI.Util.ToJson(_maleComponents);

            _femaleComponents.Add("Legs", NAPI.Util.ToJson(ComponentManager.ValidFemaleLegs.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Shoes", NAPI.Util.ToJson(ComponentManager.ValidFemaleShoes.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Accessories", NAPI.Util.ToJson(ComponentManager.ValidFemaleAccessories.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Undershirts", NAPI.Util.ToJson(ComponentManager.ValidFemaleUndershirt.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Tops", NAPI.Util.ToJson(ComponentManager.ValidFemaleTops.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Hats", NAPI.Util.ToJson(ComponentManager.ValidFemaleHats.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Glasses", NAPI.Util.ToJson(ComponentManager.ValidFemaleGlasses.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            _femaleComponents.Add("Ears", NAPI.Util.ToJson(ComponentManager.ValidFemaleEars.Select(x => new string[] { x.Name, x.ComponentId.ToString(), NAPI.Util.ToJson(x.Variations) }).ToArray()));
            FemaleComponents = NAPI.Util.ToJson(_femaleComponents);

            NAPI.Util.ConsoleOutput("Finished loading componentes into array for clothes.");
        }

        public void ResetSkin(Client player)
        {
            Character c = player.GetCharacter();
            c.HasSkin = false;

            API.SetPlayerSkin(player, c.Model.Gender == Character.GenderFemale ? PedHash.FreemodeFemale01 : PedHash.FreemodeMale01);

            player.GetCharacter().update_ped();
        }

        [Command("buyclothes"), Help(HelpManager.CommandGroups.Bussiness, "Used inside a clothing store to buy clothes.", null)]
        public void BuyClothes(Client player)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }

            if (player.IsInVehicle)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You cannot buy new clothes while in a vehicle.");
                return;
            }

            NAPI.Data.SetEntityData(player, "clothing_lastpos", player.Position);
            NAPI.Data.SetEntityData(player, "clothing_lastrot", player.Rotation);
            NAPI.Data.SetEntityData(player, "clothing_id", biz.Id);

            NAPI.Player.FreezePlayer(player, true);
            NAPI.Entity.SetEntityDimension(player, (uint)player.GetCharacter().Id + 1000);

            var character = player.GetCharacter();

            var oldClothes = new List<int[]>();

            if (character.HasSkin)
            {
                ResetSkin(player);
            }

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
            NAPI.ClientEvent.TriggerClientEvent(player, "properties_buyclothes", (character.Model.Gender == Character.GenderMale ? MaleComponents : FemaleComponents), NAPI.Util.ToJson(oldClothes), NAPI.Util.ToJson(prices));
        }

        [Command("buybag"), Help(HelpManager.CommandGroups.General, "Used inside a clothing store to buy a bag.", null)]
        public void Buybag(Client player)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }

            if (player.IsInVehicle)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You cannot buy new clothes while in a vehicle.");
                return;
            }

            NAPI.Data.SetEntityData(player, "clothing_lastpos", player.Position);
            NAPI.Data.SetEntityData(player, "clothing_lastrot", player.Rotation);
            NAPI.Data.SetEntityData(player, "clothing_id", biz.Id);

            //Setup bag list.
            var bagsList = ComponentManager.ValidBags.Select(x => new[] {x.Name, x.Variations.Count.ToString()}).ToArray();

            NAPI.Player.FreezePlayer(player, true);
            NAPI.ClientEvent.TriggerClientEvent(player, "properties_buybag", NAPI.Util.ToJson(bagsList), biz.ItemPrices["8"]);
            NAPI.Entity.SetEntityDimension(player, (uint)player.GetCharacter().Id + 1000);
        }

        [Command("buyskin"), Help(HelpManager.CommandGroups.General, "Buy a pedestrian skin as a VIP.", new[] { "Item", "New name" })]
        public void buyskin_cmd(Client player, PedHash hash)
        {
            Account account = player.GetAccount();
            var biz = PropertyManager.IsAtPropertyInteraction(player);

            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }

            if (account.VipLevel < 1)
            {
                player.SendChatMessage("You must be a VIP to use this command.");
                return;
            }

            if (Money.GetCharacterMoney(player.GetCharacter()) < 250)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Not Enough Money");
                return;
            }

            if (illegalpeds.Contains(hash))
            {
                player.SendChatMessage("This skin is not allowed.");
                return;
            }

            NAPI.ClientEvent.TriggerClientEvent(player,"checkPedGender",NAPI.Util.ToJson(hash));

        }

        public void setPlayerPedSkin(Client player, PedHash hash, int gender)
        {
            Character c = player.GetCharacter();
            if (c.Model.Gender == Character.GenderMale)
            {
                if (gender != 4)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player,"You're unable to use this skin.");
                    return;
                }
            }

            else if (c.Model.Gender == Character.GenderFemale)
            {
                if (gender != 5)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player,"You're unable to use this skin.");
                    return;
                }
            }

            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), 250);
            API.SetPlayerSkin(player, hash);
            player.GetCharacter().Skin = hash;
            player.GetCharacter().HasSkin = true;
            player.GetCharacter().Save();
            player.SendChatMessage("Skin changed! You were charged $250.");
        }
    }
}
