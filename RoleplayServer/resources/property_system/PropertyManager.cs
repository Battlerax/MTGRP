using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;

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
        }

        private void API_onResourceStart()
        {
            Properties = DatabaseManager.PropertyTable.Find(x => x.Id != -1).ToList();
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
            Restaurent,
            Bank
        }

        #region ColShapeKnowing
        private void API_onEntityExitColShape(ColShape colshape, GTANetworkShared.NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_enterance"))
            {
                if (API.getEntityData(entity, "at_interance_property_id") == colshape.getData("property_enterance"))
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
        }
        private void API_onEntityEnterColShape(ColShape colshape, GTANetworkShared.NetHandle entity)
        {
            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_enterance"))
            {
                API.setEntityData(entity, "at_interance_property_id", colshape.getData("property_enterance"));
            }

            if (API.getEntityType(entity) == EntityType.Player && colshape.hasData("property_interaction"))
            {
                API.setEntityData(entity, "at_interaction_property_id", colshape.getData("property_interaction"));
            }
        }
#endregion

        private Property IsAtPropertyEnterance(Client player)
        {
            if (API.hasEntityData(player, "at_property_id"))
            {
                int id = API.getEntityData(player, "at_property_id");
                var property = Properties.Single(x => x.Id == id);
                return property;
            }
            return null;
        }

        private Property IsAtPropertyInteraction(Client player)
        {
            if (API.hasEntityData(player, "at_interaction_property_id"))
            {
                int id = API.getEntityData(player, "at_interaction_property_id");
                var property = Properties.Single(x => x.Id == id);
                return property;
            }
            return null;
        }

        [Command("createproperty")]
        public void create_property(Client player, PropertyTypes type)
        {
            var account = player.GetAccount();
            if (account.AdminLevel >= 5)
            {
                var property = new Property(type, player.position, player.rotation, type.ToString());
                property.Insert();
                property.CreateProperty();
                Properties.Add(property);
                API.sendChatMessageToPlayer(player, "You have sucessfully create a property of type " + type.ToString());
            } 
        }
    }
}
