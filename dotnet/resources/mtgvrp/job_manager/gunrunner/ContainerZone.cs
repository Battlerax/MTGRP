using System;
using System.Collections.Generic;
using GTANetworkAPI;
using mtgvrp.database_manager;
using MongoDB.Bson;
using MongoDB.Driver;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using MongoDB.Bson.Serialization.Attributes;

public class ContainerZone
{

    public ObjectId Id { get; set; }
    public float Radius { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    [BsonIgnore] //This stuff cant be saved to DB
    public Entity Blip { get; set; }
    [BsonIgnore]
    public ColShape ColShape { get; set; }

    public ContainerZone(Vector3 position, Vector3 rotation, float radius = 30f)
    {
        Position = position;
        Rotation = rotation;
        Radius = radius;
    }

    public void Create()
    {
        Insert();
        Deploy();
    }

    public void Deploy()
    {
        Blip = API.Shared.CreateBlip(Position, 0);
        API.Shared.SetBlipName(Blip, $"Container Zone {GetContainerId(this)}");
        API.Shared.SetBlipSprite(Blip, 357);
        API.Shared.SetBlipShortRange(Blip, true);
        ColShape = API.Shared.Create2DColShape(Position.X, Position.Y, Radius, Radius);
    }

    public void Remove()
    {
        API.Shared.DeleteEntity(Blip);
        API.Shared.DeleteColShape(ColShape);
        var filter = Builders<ContainerZone>.Filter.Eq("_id", Id);
        DatabaseManager.ContainerZonesTable.DeleteOne(filter);
        ResetContainerZones();
    }

    public void ResetContainerZones()
    {
        foreach(var c in GetAllContainerZones())
        {
            c.Remove();
            c.Create();
        }
    }

    public static int GetContainerId(ContainerZone containerZone)
    {
        int i = 0;
        foreach (var c in GetAllContainerZones())
        {
            if (c.Id == containerZone.Id)
            {
                return i;
            }
            i++;
        }
        return 0;
    }

    public static List<ContainerZone> GetAllContainerZones()
    {
        return DatabaseManager.ContainerZonesTable.Find(FilterDefinition<ContainerZone>.Empty).ToList();
    }

    public void Insert()
    {
        DatabaseManager.ContainerZonesTable.InsertOne(this);
    }

    public void Save()
    {
        var filter = Builders<ContainerZone>.Filter.Eq("_id", Id);
        DatabaseManager.ContainerZonesTable.ReplaceOne(filter, this);
    }
}
