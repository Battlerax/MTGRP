var weedblip = null;
const localPlayer = API.getLocalPlayer();
var cam = API.getActiveCamera();
var timer;

API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName === "getClientGround") {
        
        var clientLoc = API.getEntityPosition(API.getLocalPlayer());
        var ground = API.getGroundHeight(clientLoc);
        API.triggerServerEvent("findGround",ground,args[0]);
    }

    else if (eventName === "weedVisual") {
        timer = args[0];
        API.playScreenEffect("DrugsMichaelAliensFight", timer, false);
        weedblip = API.createBlip(new Vector3(0, 0, 0));
        API.setBlipSprite(weedblip, 496);
        API.setBlipScale(weedblip, 20);
        API.setBlipColor(weedblip, 2);


    }

    else if (eventName === "speedVisual") {
        timer = args[0];
        API.playScreenEffect("RaceTurbo", timer, false);


    }

    else if (eventName === "heroinVisual") {
        timer = args[0];
        API.playScreenEffect("DrugsTrevorClownsFight", timer, false);
    }


    else if (eventName === "cokeVisual") {
        timer = args[0];
        API.playScreenEffect("DMT_flight_intro", timer, false);
    }

     else if (eventName === "methVisual") {
        
    }

    else if (eventName === "clearWeed") {
        if (weedblip != null) {
            API.deleteEntity(weedblip);
            weedblip = null;
            API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
        }
    }


    else if (eventName === "clearHeroin") {
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    }

    else if (eventName === "clearSpeed") {
        API.setHudVisible(true);
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    }

    else if (eventName === "clearCoke") {
        API.playScreenEffect("RampageOut", 1000, false);
    }
    // Really don't want permanent effects. 
    else if (eventName === "clearAllEffects") {
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);

    }
});
