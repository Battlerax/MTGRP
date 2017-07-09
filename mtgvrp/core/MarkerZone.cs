using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.core
{
    public class MarkerZone
    {

        public static readonly MarkerZone None = new MarkerZone(new Vector3(), new Vector3());

        private Vector3 _location;
        public Vector3 Location
        {
            get { return _location; }
            set
            {
                _location = value;
                _fullRefreshRequired = true;
            }
        }

        private Vector3 _rotation;
        public Vector3 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _fullRefreshRequired = true;
            }
        }

        private int _dimension;
        public int Dimension
        {
            get { return _dimension; }
            set
            {
                _dimension = value;
                _fullRefreshRequired = true;
            }
        }

        public float ColZoneHeight { get; set; } = 15f;
        public float ColZoneSize { get; set; } = 5f;

        [BsonIgnore]
        public NetHandle Marker { get; set; }
        [BsonIgnore]
        public NetHandle Label { get; set; }
        [BsonIgnore]
        public NetHandle Blip { get; set; }
        [BsonIgnore]
        public CylinderColShape ColZone { get; set; }

        public int BlipColor { get; set; } = 0;
        public string BlipName { get; set; }
        public float BlipScale { get; set; } = 1;
        public bool BlipShortRange { get; set; } = true;
        public int BlipSprite { get; set; } = -1;
        public int BlipTransparency { get; set; } = 255;

        private float _blipRange = 100f;
        public float BlipRange
        {
            get { return _blipRange; }
            set
            {
                _blipRange = value;
                _fullRefreshRequired = true;
            }
        } 

        public int[] MarkerColor { get; set; } = {255, 255, 255, 0}; //Alpha Red Green Blue
        public Vector3 MarkerDirection { get; set; } = new Vector3(0, 0, 0);
        public int MarkerType { get; set; } = 2; //Default arrow
        public Vector3 MarkerScale { get; set; } = new Vector3(0.5, 0.5, 0.5);

        public int[] TextLabelColor { get; set; } = {255, 0, 255, 0}; //Alpha Red Green Blue
        public bool TextLabelSeeThrough { get; set; } = false;
        public string TextLabelText { get; set; } = "";
        public float TextLabelRange { get; set; } = 10;
        public float TextLabelSize { get; set; } = 1f;


        public bool UseBlip { get; set; } = true;
        public bool UseMarker { get; set; } = true;
        public bool UseText { get; set; } = true;
        public bool UseColZone { get; set; } = true;

        private bool _fullRefreshRequired = false;

        public MarkerZone(Vector3 loc, Vector3 rot, int dimension = 0)
        {
            Location = loc;
            Rotation = rot;
            Dimension = dimension;
        }

        public void Create()
        {
            if (this == None)
                return;
          
            if (UseMarker)
            {
                Marker = API.shared.createMarker(MarkerType, Location, MarkerDirection, Rotation, MarkerScale,
                    MarkerColor[0], MarkerColor[1], MarkerColor[2], MarkerColor[3], Dimension);
            }

            if (UseText)
            {
                Label = API.shared.createTextLabel(TextLabelText, Location, TextLabelRange, TextLabelSize,
                    TextLabelSeeThrough, Dimension);
                API.shared.setTextLabelColor(Label, TextLabelColor[1], TextLabelColor[2], TextLabelColor[3], TextLabelColor[0]);
            }
           
            Blip = API.shared.createBlip(Location, BlipRange, Dimension);
            API.shared.setBlipColor(Blip, BlipColor);
            API.shared.setBlipName(Blip, BlipName);
            API.shared.setBlipScale(Blip, BlipScale);
            API.shared.setBlipShortRange(Blip, BlipShortRange);
            API.shared.setBlipSprite(Blip, (UseBlip) ? (BlipSprite) : (2));
            API.shared.setBlipTransparency(Blip, BlipTransparency);

            if (UseColZone)
            {
                ColZone = API.shared.createCylinderColShape(Location, ColZoneSize, ColZoneHeight);
            }
        }

        public void Destroy()
        {
            if (API.shared.doesEntityExist(Marker)) { API.shared.deleteEntity(Marker);}
            if (API.shared.doesEntityExist(Label)) { API.shared.deleteEntity(Label);}
            if (API.shared.doesEntityExist(Blip)) { API.shared.deleteEntity(Blip);}

            API.shared.deleteColShape(ColZone);
        }

        public void TotalRefresh()
        {
            Destroy();
            Create();
        }

        public void Refresh()
        {
            Refresh(Marker);
            Refresh(Label);
            Refresh(Blip);
            Refresh(ColZone);
        }

        public void Refresh(NetHandle type)
        {
            if (_fullRefreshRequired)
            {
                TotalRefresh();
                _fullRefreshRequired = false;
                return;
            }

            switch (API.shared.getEntityType(type))
            {
                case EntityType.Marker:
                    API.shared.setMarkerColor(Marker, MarkerColor[0], MarkerColor[1], MarkerColor[2], MarkerColor[3]);
                    API.shared.setMarkerDirection(Marker, MarkerDirection);
                    API.shared.setMarkerScale(Marker, MarkerScale);
                    API.shared.setMarkerType(Marker, MarkerType);
                    break;
                case EntityType.TextLabel:
                    API.shared.setTextLabelSeethrough(Label, TextLabelSeeThrough);
                    API.shared.setTextLabelColor(Label, TextLabelColor[1], TextLabelColor[2], TextLabelColor[3], TextLabelColor[0]);
                    API.shared.setTextLabelRange(Label, TextLabelRange);
                    API.shared.setTextLabelText(Label, TextLabelText);
                    break;
                case EntityType.Blip:
                    API.shared.setBlipColor(Blip, BlipColor);
                    API.shared.setBlipName(Blip, BlipName);
                    API.shared.setBlipScale(Blip, BlipScale);
                    API.shared.setBlipShortRange(Blip, BlipShortRange);
                    API.shared.setBlipSprite(Blip, BlipSprite);
                    API.shared.setBlipTransparency(Blip, BlipTransparency);
                    break;
            }
        }

        public void Refresh(CylinderColShape colshape)
        {
            API.shared.deleteColShape(ColZone);
            ColZone = API.shared.createCylinderColShape(Location, ColZoneSize, ColZoneHeight);
        }

        public void SetMarkerZoneRouteVisible(Client player, bool visible, int color)
        {
            API.shared.triggerClientEvent(player, "setMarkerZoneRouteVisible", this, visible, color);
        }
    }
}
