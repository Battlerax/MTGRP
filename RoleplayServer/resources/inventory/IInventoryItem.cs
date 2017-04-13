using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.inventory
{
    public interface IInventoryItem
    {
        //Just an id.
        [BsonId]
        ObjectId Id { get; set; }

        //Perms.
        bool CanBeGiven { get; }
        bool CanBeDropped { get; }
        bool CanBeStashed { get; }
        bool CanBeStacked { get; } //NO for stuff like IDs and YES for stuff like engine parts.
        bool IsBlocking { get; } //if true, users inv cannot be updated anyway while its in their inv.

        //Amount of slots it takes.
        int AmountOfSlots { get; }

        //For use in commands.. like '/give [CommandFriendlyName] [amount]'
        string CommandFriendlyName { get; }

        //Long name.
        string LongName { get; }

        //Object hash.
        int Object { get; }

        //Amount of items.
        int Amount { get; set; }
    }
}
