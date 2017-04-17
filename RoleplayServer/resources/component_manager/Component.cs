using System.Collections.Generic;
using MongoDB.Bson;

namespace RoleplayServer.resources.component_manager
{
    public class Component
    {
        public const int ComponentTypeFace = 0;
        public const int ComponentTypeMask = 1;
        public const int ComponentTypeHair = 2;
        public const int ComponentTypeTorso = 3;
        public const int ComponentTypeLegs = 4;
        public const int ComponentTypeBags = 5;
        public const int ComponentTypeShoes = 6;
        public const int ComponentTypeAccessories = 7;
        public const int ComponentTypeUndershirt = 8;
        public const int ComponentTypeBodyarmor = 9;
        public const int ComponentTypeDecals = 10;
        public const int ComponentTypeTops = 11;

        //Use (20 - COMPONENT_TYPE) when setting using API functions
        public const int ComponentTypeHats = 20;
        public const int ComponentTypeGlasses = 21;
        public const int ComponentTypeEars = 22;

        public ObjectId Id;
        public string Name;
        public int Type;
        public int Gender;
        public int ComponentId;
        public List<int> Variations = new List<int>();
        
        public Component(int type, int gender, int componentId, List<int> variations)
        {
            Type = type;
            Gender = gender;
            ComponentId = componentId;
            Variations = variations;
            
        }

        public Component(int type, int gender, int componentId, List<int> variations, string name)
        {
            Type = type;
            Gender = gender;
            ComponentId = componentId;
            Variations = variations;
            Name = name;
        }
        
    }
}
