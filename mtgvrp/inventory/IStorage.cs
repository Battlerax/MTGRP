using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.inventory
{
    public interface IStorage
    {
        List<IInventoryItem> Inventory { get; set; }
        [BsonIgnore]
        int MaxInvStorage { get; }
    }
}
