using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Server.API;

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
    public NetHandle Blip { get; set; }
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
        Blip = API.shared.createBlip(Position, 0);
        API.shared.setBlipName(Blip, $"Container Zone {GetContainerId(this)}");
        API.shared.setBlipSprite(Blip, 357);
        ColShape = API.shared.create2DColShape(Position.X, Position.Y, Radius, Radius);
    }

    public void Remove()
    {
        var filter = Builders<ContainerZone>.Filter.Eq("_id", Id);
        DatabaseManager.ContainerZonesTable.DeleteOne(filter);
        API.shared.deleteEntity(Blip);
        API.shared.deleteColShape(ColShape);
        ResetContainerZones();
        Save();
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
