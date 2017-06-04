using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoleplayServer.resources.phone_manager;

namespace RoleplayServer.resources.property_system
{
    public class ItemManager
    {
        public static void SetDefaultPrices(Property prop)
        {
            prop.ItemPrices = new Dictionary<string, int>();
            switch (prop.Type)
            {
                case PropertyManager.PropertyTypes.Clothing:
                    prop.ItemPrices.Add("0", 30); //Pants
                    prop.ItemPrices.Add("1", 40); //Shoes
                    prop.ItemPrices.Add("2", 10); //Accessories
                    prop.ItemPrices.Add("3", 20); //Undershirts
                    prop.ItemPrices.Add("4", 20); //Tops
                    prop.ItemPrices.Add("5", 10); //Hats
                    prop.ItemPrices.Add("6", 10); //Glasses
                    prop.ItemPrices.Add("7", 5); //Earrings
                    break;
            }
        }

        public string[][] HardwareItems =
        {
            new [] {"1", "Phone", "100", nameof(Phone)},
            new [] {""},
        };
    }
}
