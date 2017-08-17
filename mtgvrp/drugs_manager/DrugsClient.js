"use strict";
var weedblip = null;
var localPlayer = API.getLocalPlayer();
var cam = API.getActiveCamera();
var timer;
API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName === "getClientGround") {
        var clientLoc = API.getEntityPosition(API.getLocalPlayer());
        var ground = API.getGroundHeight(clientLoc);
        API.triggerServerEvent("findGround", ground, args[0]);
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
        API.playScreenEffect("DrugsTrevorClownsFight", timer, false);
        API.setHudVisible(false);
    }
    else if (eventName === "heroinVisual") {
        API.setCameraShake(cam, "DRUNK_SHAKE", 5);
    }
    else if (eventName === "clearWeed") {
        if (weedblip != null) {
            API.deleteEntity(weedblip);
            weedblip = null;
            API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
        }
    }
    else if (eventName === "clearHeroin") {
        API.stopCameraShake(cam);
    }
    else if (eventName === "clearSpeed") {
        API.setHudVisible(true);
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    }
    else if (eventName === "clearAllEffects") {
        API.callNative("0x4E6D875B");
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
        API.stopCameraShake(cam);
    }
});
