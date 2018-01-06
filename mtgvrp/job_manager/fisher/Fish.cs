using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.fisher
{
    public class Fish : IInventoryItem
    {
        public static Fish None = new Fish("", 0, 0, 0, false, 0);

        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 20;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public string CommandFriendlyName => Name?.Replace(" ", "") + "_" + ActualWeight;
        public string LongName => Name;
        public int Object => 0;

        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxWeight { get; set; }
        public double MinWeight { get; set; }
        public bool RequiresBoat { get; set; }
        public int Rarity { get; set; }
        public double ActualWeight { get; set; }

        public Fish(string name, int minValue, int minWeight, int maxWeight, bool requiresBoat, int rarity)
        {
            Name = name;
            MinValue = minValue;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
            RequiresBoat = requiresBoat;
            Rarity = rarity;
        }

        public Fish()
        {
            
        }

        public double calculate_value()
        {
            return (MinValue * 2) -
                   Math.Round((double) MinValue * ((double) MaxWeight - (double) ActualWeight) /
                              ((double) MaxWeight - (double) MinWeight));
        }

        public double calculate_value(double weight)
        {
            return (MinValue * 2) -
                   Math.Round((double)MinValue * ((double)MaxWeight - (double)weight) /
                              ((double)MaxWeight - (double)MinWeight));
        }
    }
}
