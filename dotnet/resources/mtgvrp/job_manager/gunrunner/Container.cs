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


public class Container
{

    public ObjectId Id { get; set; }
    public int OwnerId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    [BsonIgnore]
    public NetHandle ContainerObject { get; set; }
    [BsonIgnore]
    public Property property { get; set; }
    public bool CanBeMoved { get; set; }
    public bool CanTrackLocation { get; set; }
    public bool CanIntervene { get; set; }
    public int RenownToNextUpgrade { get; set; }


    public Container(Character owner, Vector3 position, Vector3 rotation)
    {
        OwnerId = owner.Id;
        Position = position;
        Rotation = rotation;
        RenownToNextUpgrade = 1000;
    }

    public void Deploy()
    {
        ContainerObject = API.Shared.CreateObject(API.Shared.GetHashKey("prop_container_03b"), Position, Rotation);
        property = new Property(PropertyManager.PropertyTypes.Container, Position, Rotation, PropertyManager.PropertyTypes.Container.ToString());
        ItemManager.SetDefaultPrices(property);
        property.OwnerId = OwnerId;
        property.IsInteractable = true;
        property.IsTeleportable = true;
        property.EntranceDimension = 0;
        property.EntrancePos = Position;
        property.EntranceRot = Rotation;
        property.TargetPos = new Vector3(-194.0557, -744.3947, 15.81151); /*container coords*/
        property.TargetRot = new Vector3(0, 0, 0); /*container coords*/
        property.InteractionPos = new Vector3(-193.9447, -735.7366, 15.81151); /*container coords*/
        property.InteractionRot = new Vector3(0, 0, 0); /*container coords*/
        property.InteractionDimension = property.TargetDimension;
        property.PropertyName = "Gunrunning Headquarters";
        property.CreateProperty();
        PropertyManager.Properties.Add(property);
    }

    public void Remove()
    {
        API.Shared.DeleteEntity(ContainerObject);
        property.DestroyMarkers();
        var filter = Builders<Property>.Filter.Eq("_id", Id);
        DatabaseManager.PropertyTable.DeleteOne(filter);
        PropertyManager.Properties.Remove(property);
    }

    public void Respawn()
    {
        Remove();
        Deploy();
    }

    public void Insert()
    {
        DatabaseManager.ContainersTable.InsertOne(this);
    }

    public void Save()
    {
        var filter = Builders<Container>.Filter.Eq("_id", Id);
        DatabaseManager.ContainersTable.ReplaceOne(filter, this);
    }

}
