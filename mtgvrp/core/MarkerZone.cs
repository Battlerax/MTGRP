using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.core
{
    public class MarkerZone
    {
        public static readonly MarkerZone None = new MarkerZone(new Vector3(), new Vector3());

        public string LabelText { get; set; }

        public Vector3 Location { get; set; }
        public Vector3 Rotation { get; set; }
        public int Dimension { get; set; }

        public float ColZoneSize { get; set; }

        public int MarkerType { get; set; }
        public Vector3 Scale { get; set; }
        public int Alpha { get; set; }
        public int Red { get; set; }
        public int Blue { get; set; }
        public int Green { get; set; }

        public int BlipSprite { get; set; }

        [BsonIgnore]
        public NetHandle Marker { get; set; }
        [BsonIgnore]
        public NetHandle Label { get; set; }
        [BsonIgnore]
        public NetHandle Blip { get; set; }
        [BsonIgnore]
        public SphereColShape ColZone { get; set; }

        public MarkerZone(Vector3 loc, Vector3 rot, int dimension = 0, float zoneSize = 2.0f)
        {
            Location = loc;
            Rotation = rot;
            Dimension = dimension;
            ColZoneSize = zoneSize;


            MarkerType = 2;
            Alpha = 255;
            Red = 255;
            Green = 255;
            Blue = 0;

            BlipSprite = -1;

            Scale = new Vector3(0.5, 0.5, 0.5);
        }

        public void Create()
        {
            if (this == None)
                return;

            Marker = API.shared.createMarker(2, Location, Location, Rotation, Scale, Alpha, Red, Green, Blue, Dimension);
            Label = API.shared.createTextLabel("~g~" + LabelText, Location.Add(new Vector3(0.0, 0.0, 0.5)), 25f, 0.5f, true, Dimension);
            ColZone = API.shared.createSphereColShape(Location, ColZoneSize);

            if (BlipSprite != -1)
            {
                Blip = API.shared.createBlip(Marker);
                API.shared.setBlipSprite(Blip, BlipSprite);
            }
        }

        public void Destroy()
        {
            if (API.shared.doesEntityExist(Marker)) { API.shared.deleteEntity(Marker);}
            if (API.shared.doesEntityExist(Label)) { API.shared.deleteEntity(Label);}
            API.shared.deleteColShape(ColZone);
            if (API.shared.doesEntityExist(Blip)) { API.shared.deleteEntity(Blip);}
        }

        public void Refresh()
        {
            Destroy();
            Create();
        }
    }
}
