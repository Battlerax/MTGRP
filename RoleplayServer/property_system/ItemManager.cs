using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoleplayServer.resources.phone_manager;

namespace RoleplayServer.resources.property_system
{
    public static class ItemManager
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


                    prop.ItemPrices.Add("8", 200); //Bags
                    break;

                case PropertyManager.PropertyTypes.TwentyFourSeven:
                    prop.ItemPrices.Add("rope", 20);
                    prop.ItemPrices.Add("rags", 10);
                    prop.ItemPrices.Add("sprunk", 5);
                    break;

                case PropertyManager.PropertyTypes.Hardware:
                    prop.ItemPrices.Add("rope", 20);
                    prop.ItemPrices.Add("rags", 10);
                    prop.ItemPrices.Add("phone", 500);
                    break;

                //This is kind of a unique business.. item names are the id.. it doesn't have a general sell list like 24/7 and Hardware.
                case PropertyManager.PropertyTypes.Restaurant:
                    prop.ItemPrices.Add("sprunk", 5);
                    prop.ItemPrices.Add("custom1", 50);
                    prop.ItemPrices.Add("custom2", 100);
                    prop.ItemPrices.Add("custom3", 100);
                    prop.ItemPrices.Add("custom4", 100);
                    prop.RestaurantItems = new string[]
                    {
                        "Food 1",
                        "Food 2",
                        "Food 3",
                        "Food 4"
                    };
                    break;
            }
        }

        public static string[][] TwentyFourSevenItems =
        {
            //ID, NAME, PRICE, DESCRIPTION
            new [] {"rope", "Rope", "Used to tie people."},
            new [] {"rags", "Rags", "Used to mute or blindfold people."},
            new [] {"sprunk", "Sprunk", "Used to get some health."},
        };

        public static string[][] HardwareItems =
        {
            //ID, NAME, PRICE, DESCRIPTION
            new [] {"rope", "Rope", "Used to tie people."},
            new [] {"rags", "Rags", "Used to mute or blindfold people."},
            new [] {"phone", "Phone", "Used to contact other people."},
        };
    }
}
