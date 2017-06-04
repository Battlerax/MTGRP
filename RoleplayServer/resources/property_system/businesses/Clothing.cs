using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.component_manager;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.property_system;

namespace RoleplayServer.resources.property_system.businesses
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

            API.freezePlayer(player, true);
            API.setEntityPosition(player, new Vector3(403, -997, -99));
            API.setEntityRotation(player, new Vector3(0, 0, 177.2663));

            var character = player.GetCharacter();

            var oldClothes = new int[][]
            {
                new int[] {character.Model.PantsStyle, character.Model.PantsVar},
                new int[] {character.Model.ShoeStyle, character.Model.ShoeVar},
                new int[] {character.Model.AccessoryStyle, character.Model.AccessoryVar},
                new int[] {character.Model.UndershirtStyle, character.Model.UndershirtVar},
                new int[] {character.Model.TopStyle, character.Model.TopVar},
                new int[] {character.Model.HatStyle, character.Model.HatVar},
                new int[] {character.Model.GlassesStyle, character.Model.GlassesVar},
                new int[] {character.Model.EarStyle, character.Model.EarVar},
            };
            var prices = biz.ItemPrices.Select(x => x.Value).ToArray();
            API.triggerClientEvent(player, "properties_buyclothes", (character.Model.Gender == Character.GenderMale ? _maleComponents : _femaleComponents), API.toJson(oldClothes), API.toJson(prices));
        }
    }
}
