using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.inventory
{
    public interface IStorage
    {
        List<IInventoryItem> Inventory { get; set; }
        [BsonIgnore]
        int MaxInvStorage { get; }
    }
}
