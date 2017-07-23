using GTANetworkServer;
using MongoDB.Driver;
using mtgvrp.database_manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtgvrp.mapping_manager
{
    public class Mapping
    {
        public int Id;
        public string Description;

        public int PropertyLinkId;
        public bool IsActive;
        public bool IsSpawned;

        public string CreatedBy;
        public DateTime CreatedDate;

        public List<MappingObject> Objects = new List<MappingObject>();
        public int Dimension;

        public List<MappingObject> DeleteObjects = new List<MappingObject>();

        public string PastebinLink;

        public Mapping(string createdBy, string pastebinLink, string description, int propertyLink, int dimension)
        {
            CreatedBy = createdBy;
            PastebinLink = pastebinLink;
            Description = description;
            PropertyLinkId = propertyLink;
            Dimension = dimension;

            IsActive = true;
            IsSpawned = true;
            CreatedDate = DateTime.Now;
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("mapping");
            DatabaseManager.MappingTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Mapping>.Filter.Eq("_id", Id);
            DatabaseManager.MappingTable.ReplaceOne(filter, this);
        }

        public void Load()
        {
            if(IsSpawned == false)
            {
                foreach (var o in Objects)
                {
                    o.Spawn(Dimension);
                }
                IsSpawned = true;
            }
        }
    }
}
