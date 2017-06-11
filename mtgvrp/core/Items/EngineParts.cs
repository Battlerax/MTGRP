using RoleplayServer.resources.inventory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

//Change class name
public class EngineParts : IInventoryItem
{

    [BsonId]
    public ObjectId Id { get; set; }
    public int Amount { get; set; }


    public int AmountOfSlots => 2;


    public bool CanBeDropped => true;
    public bool CanBeGiven => true;
    public bool CanBeStacked => false;
    public bool CanBeStashed => true;
    public bool IsBlocking => false;
    public int MaxAmount => 10; 

    public string CommandFriendlyName => $"engineparts";

    public string LongName => $"Engine Parts";

    public int Object => 0;
}