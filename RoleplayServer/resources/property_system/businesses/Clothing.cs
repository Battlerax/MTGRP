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
            API.freezePlayer(player, true);
            API.setEntityPosition(player, new Vector3(403, -997, -100));
            API.setEntityRotation(player, new Vector3(0, 0, 177.2663));
           

            API.triggerClientEvent(player, "properties_buyclothes", (player.GetCharacter().Model.Gender == Character.GenderMale ? _maleComponents : _femaleComponents));
        }
    }
}
