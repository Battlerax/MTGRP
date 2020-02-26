using System;
using System.Collections.Generic;
using System.Text;
using RAGE;
using RAGE.Elements;

namespace cs_packages.Core
{
    public class Random : Events.Script
    {
        int _lastObj;
        string _lastEvent;

        public Random()
        {
            Events.Add("PLACE_OBJECT_ON_GROUND_PROPERLY", PlaceOnGroundProperly);
            Events.Add("COMPLETE_FREEZE", CompleteFreeze);
            Events.OnEntityStreamIn += OnEntityStreamIn;
        }

        private void OnEntityStreamIn(Entity entity)
        {
            if ((int)entity.GetSharedData("TargetObj") == _lastObj) { 
                RAGE.Game.Invoker.Invoke(0x58A850EAEE20FAA3, entity);
                var pos = entity.Position;
                var rot = RAGE.Game.Entity.GetEntityRotation(entity.Id, 2);
                RAGE.Elements.Player.LocalPlayer.Call(_lastEvent, _lastObj, pos, rot);
            }
        }

        private void CompleteFreeze(object[] args)
        {
            var state = (bool)args[0];
            var p = RAGE.Elements.Player.LocalPlayer;

            p.FreezePosition(state);
            if (p.IsInAnyVehicle(false)) {
                p.Vehicle.FreezePosition(state);
                if (state)
                    p.Vehicle.SetDoorsLocked(4);
                else
                    p.Vehicle.SetDoorsLocked(0);
            }
        }

        private void PlaceOnGroundProperly(object[] args)
        {
            _lastObj = (int)args[0];
            RAGE.Game.Object.PlaceObjectOnGroundProperly(_lastObj);
            var pos = RAGE.Game.Entity.GetEntityCoords(_lastObj, false);
            var rot = RAGE.Game.Entity.GetEntityRotation(_lastObj, 2); 
            RAGE.Elements.Player.LocalPlayer.Call("OBJECT_PLACED_PROPERLY", args[0], pos, rot);
            if ((string)args[1] != "")
                RAGE.Elements.Player.LocalPlayer.Call((string)args[1], _lastObj);
        }
    }
}
