
using GTANetworkAPI;





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
        public Entity Marker { get; set; }
        [BsonIgnore]
        public Entity Label { get; set; }
        [BsonIgnore]
        public Entity Blip { get; set; }
        [BsonIgnore]
        public ColShape ColZone { get; set; }

        public int BlipColor { get; set; } = 0;
        public string BlipName { get; set; }
        public float BlipScale { get; set; } = 1;
        public bool BlipShortRange { get; set; } = true;
        public int BlipSprite { get; set; } = 2;
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
                // TODO: SCALE IS NO LONGER VECTOR
                Marker = API.Shared.CreateMarker(MarkerType, Location, MarkerDirection, Rotation, MarkerScale.X,
                    new GTANetworkAPI.Color(MarkerColor[0], MarkerColor[1], MarkerColor[2], MarkerColor[3]), false, (uint)Dimension);
            }

            if (UseText)
            {
                // TODO: did some shit to this
                Label = API.Shared.CreateTextLabel(TextLabelText, Location, TextLabelRange, TextLabelSize, 1, new GTANetworkAPI.Color(1, 1, 1),
                    entitySeethrough: TextLabelSeeThrough, dimension: (uint)Dimension);
                API.Shared.SetTextLabelColor(Label, TextLabelColor[1], TextLabelColor[2], TextLabelColor[3], TextLabelColor[0]);
            }

            //Blip = API.Shared.CreateBlip(Location, BlipRange, Dimension);
            Blip = API.Shared.CreateBlip(Location, Dimension);
            API.Shared.SetBlipColor(Blip, BlipColor);
            API.Shared.SetBlipName(Blip, BlipName);
            API.Shared.SetBlipScale(Blip, BlipScale);
            API.Shared.SetBlipShortRange(Blip, BlipShortRange);
            API.Shared.SetBlipSprite(Blip, (UseBlip && BlipSprite != -1) ? (BlipSprite) : (2));
            API.Shared.SetBlipTransparency(Blip, BlipTransparency);

            if (UseColZone)
            {
                ColZone = API.Shared.CreateCylinderColShape(Location, ColZoneSize, ColZoneHeight);
            }
        }

        public void Destroy()
        {
            if (API.Shared.DoesEntityExist(Marker)) { API.Shared.DeleteEntity(Marker);}
            if (API.Shared.DoesEntityExist(Label)) { API.Shared.DeleteEntity(Label);}
            if (API.Shared.DoesEntityExist(Blip)) { API.Shared.DeleteEntity(Blip);}

            if(ColZone != null && API.Shared.DoesEntityExist(ColZone))
                API.Shared.DeleteColShape(ColZone);
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

        public void Refresh(Entity type)
        {
            if (_fullRefreshRequired)
            {
                TotalRefresh();
                _fullRefreshRequired = false;
                return;
            }

            switch (API.Shared.GetEntityType(type))
            {
                case EntityType.Marker:
                    API.Shared.SetMarkerColor(Marker, MarkerColor[0], MarkerColor[1], MarkerColor[2], MarkerColor[3]);
                    API.Shared.SetMarkerDirection(Marker, MarkerDirection);
                    API.Shared.SetMarkerScale(Marker, MarkerScale.X);
                    API.Shared.SetMarkerType(Marker, MarkerType);
                    break;
                case EntityType.TextLabel:
                    API.Shared.SetTextLabelSeethrough(Label, TextLabelSeeThrough);
                    API.Shared.SetTextLabelColor(Label, TextLabelColor[1], TextLabelColor[2], TextLabelColor[3], TextLabelColor[0]);
                    API.Shared.SetTextLabelRange(Label, TextLabelRange);
                    API.Shared.SetTextLabelText(Label, TextLabelText);
                    break;
                case EntityType.Blip:
                    API.Shared.SetBlipColor(Blip, BlipColor);
                    API.Shared.SetBlipName(Blip, BlipName);
                    API.Shared.SetBlipScale(Blip, BlipScale);
                    API.Shared.SetBlipShortRange(Blip, BlipShortRange);
                    API.Shared.SetBlipSprite(Blip, (UseBlip && BlipSprite != -1) ? (BlipSprite) : (2));
                    API.Shared.SetBlipTransparency(Blip, BlipTransparency);
                    break;
            }
        }

        public void Refresh(ColShape colshape)
        {
            if(ColZone != null && API.Shared.DoesEntityExist(ColZone))
                API.Shared.DeleteColShape(ColZone);
            if (UseColZone)
            {
                ColZone = API.Shared.CreateCylinderColShape(Location, ColZoneSize, ColZoneHeight);
            }
        }

        public void SetMarkerZoneRouteVisible(Player player, bool visible, int color)
        {
            API.Shared.TriggerClientEvent(player, "setMarkerZoneRouteVisible", Blip, visible, color);
        }
    }
}
