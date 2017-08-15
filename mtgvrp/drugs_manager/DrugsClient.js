"use strict";
API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName === "getClientGround") {
        var clientLoc = API.getEntityPosition(API.getLocalPlayer());
        var ground = API.getGroundHeight(clientLoc);
        API.triggerServerEvent("findGround", ground, args[0]);
    }
});
