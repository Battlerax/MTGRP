using System.Collections.Generic;
using GTANetworkShared;

namespace mtgvrp.property_system
{
    public static class ItemManager
    {
        public static void SetDefaultPrices(Property prop)
        {
            prop.ItemPrices = new Dictionary<string, int>();
            switch (prop.Type)
            {
                case PropertyManager.PropertyTypes.Clothing:
                    prop.ItemPrices.Add("pants", 30); //Pants
                    prop.ItemPrices.Add("shoes", 40); //Shoes
                    prop.ItemPrices.Add("accessories", 10); //Accessories
                    prop.ItemPrices.Add("undershirts", 20); //Undershirts
                    prop.ItemPrices.Add("tops", 20); //Tops
                    prop.ItemPrices.Add("hats", 10); //Hats
                    prop.ItemPrices.Add("glasses", 10); //Glasses
                    prop.ItemPrices.Add("earrings", 5); //Earrings


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

                case PropertyManager.PropertyTypes.GasStation:
                    prop.ItemPrices.Add("gas", 3);
                    break;

                case PropertyManager.PropertyTypes.Ammunation:
                    prop.ItemPrices.Add("bat", 40);
                    prop.ItemPrices.Add("pistol", 4000);
                    prop.ItemPrices.Add("combat_pistol", 4700);
                    prop.ItemPrices.Add("heavy_pistol", 6500);
                    prop.ItemPrices.Add("revolver", 8000);
                    break;

                case PropertyManager.PropertyTypes.LSNN:
                    prop.ItemPrices.Add("lotto_ticket", 200);
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

        public static string[][] AmmunationItems =
        {
            //ID, NAME, PRICE, DESCRIPTION
            new[] {"bat", "Bat", "Used to tie people."},
            new[] {"pistol", "Pistol", "A standard issue 9mm pistol. Great for self defense."},
            new[] {"combat_pistol", "Combat Pistol", "A combat pistol specialized for tight spaces and accuracy."},
            new[] {"heavy_pistol", "Heavy Pistol", "A pistol specialized to leave a mark."},
            new[] { "revolver", "Revolver", "For when you want to feel like a cowboy"},
        };

        public static string[][] LSNNItems =
        {
            //ID, NAME, PRICE, DESCRIPTION
            new[] {"lotto_ticket", "Lotto Ticket", "Purchase a lotto ticket and enter the lotto!"},

        };
    }
}
