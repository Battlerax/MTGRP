using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer
{
    public class MarkerZone
    {
        public string label_text { get; set; }

        public Vector3 location { get; set; }
        public Vector3 rotation { get; set; }
        public int dimension { get; set; }

        public float col_zone_size { get; set; }

        public int marker_type { get; set; }
        public Vector3 scale { get; set; }
        public int alpha { get; set; }
        public int red { get; set; }
        public int blue { get; set; }
        public int green { get; set; }

        public int blip_sprite { get; set; }

        [BsonIgnore]
        public NetHandle marker { get; set; }
        [BsonIgnore]
        public NetHandle label { get; set; }
        [BsonIgnore]
        public NetHandle blip { get; set; }
        [BsonIgnore]
        public Rectangle2DColShape col_zone { get; set; }

        public MarkerZone(Vector3 loc, Vector3 rot, int dimension = 0, float zone_size = 10.0f)
        {
            this.location = loc;
            this.rotation = rot;
            this.dimension = dimension;
            this.col_zone_size = zone_size;


            marker_type = 2;
            alpha = 255;
            red = 255;
            green = 255;
            blue = 0;

            scale = new Vector3(0.5, 0.5, 0.5);
        }

        public void create()
        {
            this.marker = API.shared.createMarker(2, location, location, rotation, scale, alpha, red, green, blue, dimension);
            this.label = API.shared.createTextLabel("~g~" + label_text, location.Add(new Vector3(0.0, 0.0, 0.5)), 25f, 0.5f, true, dimension);
            this.col_zone = API.shared.create2DColShape(location.X, location.Y, col_zone_size, 5.0f);

            this.blip = API.shared.createBlip(this.marker);
            API.shared.setBlipSprite(this.blip, blip_sprite);
        }
    }
}
