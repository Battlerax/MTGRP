using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class Component
    {
        public const int COMPONENT_TYPE_FACE = 0;
        public const int COMPONENT_TYPE_MASK = 1;
        public const int COMPONENT_TYPE_HAIR = 2;
        public const int COMPONENT_TYPE_TORSO = 3;
        public const int COMPONENT_TYPE_LEGS = 4;
        public const int COMPONENT_TYPE_BAGS = 5;
        public const int COMPONENT_TYPE_SHOES = 6;
        public const int COMPONENT_TYPE_ACCESSORIES = 7;
        public const int COMPONENT_TYPE_UNDERSHIRT = 8;
        public const int COMPONENT_TYPE_BODYARMOR = 9;
        public const int COMPONENT_TYPE_DECALS = 10;
        public const int COMPONENT_TYPE_TOPS = 11;

        //Use (20 - COMPONENT_TYPE) when setting using API functions
        public const int COMPONENT_TYPE_HATS = 20;
        public const int COMPONENT_TYPE_GLASSES = 21;
        public const int COMPONENT_TYPE_EARS = 22;

        public ObjectId _id;
        public string name;
        public int type;
        public int gender;
        public int component_id;
        public List<int> variations = new List<int>();
        
        public Component(int type, int gender, int component_id, List<int> variations)
        {
            this.type = type;
            this.gender = gender;
            this.component_id = component_id;
            this.variations = variations;
            
        }

        public Component(int type, int gender, int component_id, List<int> variations, string name)
        {
            this.type = type;
            this.gender = gender;
            this.component_id = component_id;
            this.variations = variations;
            this.name = name;
        }
        
    }
}
