using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleplayServer.resources.job_manager.fisher
{
    public class Fish
    {
        public static Fish None = new Fish("", 0, 0, 0, false, 0);

        public string Name { get; set; }
        public int MinValue { get; set; }
        public int MaxWeight { get; set; }
        public int MinWeight { get; set; }
        public bool RequiresBoat { get; set; }
        public int Rarity { get; set; }

        public Fish(string name, int minValue, int minWeight, int maxWeight, bool requiresBoat, int rarity)
        {
            Name = name;
            MinValue = minValue;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
            RequiresBoat = requiresBoat;
            Rarity = rarity;
        }

        public int calculate_value(int weight)
        {
            return (MinValue * 2) - (int)Math.Round((double)MinValue * ((double)MaxWeight - (double)weight) / ((double)MaxWeight - (double)MinWeight));
        }
    }
}
