var weedblip = null;
const localPlayer = API.getLocalPlayer();

API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName === "getClientGround") {
        
        var clientLoc = API.getEntityPosition(API.getLocalPlayer());
        var ground = API.getGroundHeight(clientLoc);
        API.triggerServerEvent("findGround",ground,args[0]);
    }

    else if (eventName === "weedVisual") {
        var timer = args[0];
        API.playScreenEffect("DrugsMichaelAliensFight", timer, false);
        weedblip = API.createBlip(new Vector3(0, 0, 0));
        API.setBlipSprite(weedblip, 496);
        API.setBlipScale(weedblip, 20);
        API.setBlipColor(weedblip, 2);


    }

    else if (eventName === "clearWeed") {
        if (weedblip != null) {
            API.deleteEntity(weedblip);
            weedblip = null;
            API.playScreenEffect("DrugsMichaelAliensFightOut", 1000, false);
        }
    }

    // Really don't want permanent effects. 
    else if (eventName === "clearAllEffects") {
        API.callNative("0x4E6D875B");
    }
});
