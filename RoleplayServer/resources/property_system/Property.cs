using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.property_system
{
    class Property
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int OwnerId { get; set; }
        public string PropertyName { get; set; }
        public PropertyManager.PropertyTypes Type { get; set; }
    }
}
