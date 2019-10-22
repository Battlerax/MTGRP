using System.Collections.Generic;

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
                    prop.ItemPrices.Add("axe", 100);
                    prop.ItemPrices.Add("scuba", 2000);
                    prop.ItemPrices.Add("engineparts", 200);
                    prop.ItemPrices.Add("spraypaint", 250);
                    prop.ItemPrices.Add("crowbar", 1000);
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

                case PropertyManager.PropertyTypes.LSNN:
                    prop.ItemPrices.Add("lotto_ticket", 200);
                    break;

                case PropertyManager.PropertyTypes.HuntingStation:
                    prop.ItemPrices.Add("deer_tag", 250);
                    prop.ItemPrices.Add("boar_tag", 500);
                    prop.ItemPrices.Add("ammo", 150);
                    break;

                case PropertyManager.PropertyTypes.VIPLounge:
                    prop.ItemPrices.Add("pink_tint", 500);
                    prop.ItemPrices.Add("gold_tint", 500);
                    prop.ItemPrices.Add("green_tint", 300);
                    prop.ItemPrices.Add("orange_tint", 300);
                    prop.ItemPrices.Add("platinum_tint", 400);
                    break;

                case PropertyManager.PropertyTypes.Government:
                    prop.ItemPrices.Add("id", 150);
                    break;

                case PropertyManager.PropertyTypes.DMV:
                    prop.ItemPrices.Add("drivingtest", 150);
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
            new [] {"axe", "Axe", "Used to cut wood."},
            new [] {"scuba", "Scuba Set", "Used to dive."},
            new [] {"engineparts", "EngineParts", "Used to fix vehicles as a mechanic."},
            new [] {"spraypaint", "SprayPaint", "Used to change vehicle colors as a mechanic."},
            new [] {"crowbar","Crowbar","Used to pry open crates."}
        };

        public static string[][] AmmunationItems =
        {
            //ID, NAME, PRICE, DESCRIPTION
            new[] {"bat", "Bat", "A bat, used to play baseball probably."},
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

        public static string[][] VIPItems =
{
            //ID, NAME, PRICE, DESCRIPTION
            new[] {"pink_tint", "Pink", "Change your weapon tint."},
            new[] {"gold_tint", "Gold", "Change your weapon tint."},
            new[] {"green_tint", "Green", "Change your weapon tint."},
            new[] {"orange_tint", "Orange", "Change your weapon tint."},
            new[] {"platinum_tint", "Platinum", "Change your weapon tint."},
        };

        public static string[][] HuntingItems =
        {
            new [] {"deer_tag", "Deer Tag", "Used to turn in a killed deer for cash. Can only purchase one per day."},
            new [] {"boar_tag", "Boar Tag", "Used to turn in a killed boar for cash. Can only purchase one per day."},
            new [] {"ammo", "5.56 Ammo", "Extra ammo in case you miss. Each tag comes with one free bullet."},
        };

        public static string[][] GovItems =
        {
            new [] {"id", "Identification", "Used to identify yourself to anyone."},
        };
    }
}
