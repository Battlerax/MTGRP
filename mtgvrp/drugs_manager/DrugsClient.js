"use strict";
var weedblip = null;
var localPlayer = mp.players.local;
// var cam = API.getActiveCamera();
var timer;

mp.events.add({
    "getClientGround": (arg1) => {
        var clientLoc = localPlayer.getCoords(alive);
        var ground = localPlayer.getHeight(clientLoc, true, true);
        mp.events.callRemote('findGround', ground, arg1)
    },
    
    'weedVisual': (time) => {
        timer = time
        mp.game.graphics.startScreenEffect("DrugsMichaelAliensFight", timer, false);
        weedblip = mp.blips.new(496, new Vector3(-4583, 5638, -0.1), {
        scale: 20,
        color: 2,
        shortRange: false,
    });
    },
    
    'speedVisual': (time) => {
      mp.game.graphics.startScreenEffect("RaceTurbo", time, false);
    },
    
    'heroinVisual': (time) => {
      mp.game.graphics.startScreenEffect("DrugsTrevorClownsFight", time, false);
    },
    
     'cokeVisual': (time) => {
      mp.game.graphics.startScreenEffect("DMT_flight_intro", time, false);
    },
    
    'clearWeed': () => {
        if (weedblip !== null) {
            weedblip.destroy();
            weedblip = null;
            mp.game.graphics.startScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
        }
    },
    
    'clearCoke': () => {
      mp.game.graphics.startScreenEffect("RampageOut", 1000, false);
    },
    
    'clearHeroin': () => {
      mp.game.graphics.startScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    },
    
    'clearSpeed': () => {
      mp.game.ui.displayHud(true);    
      mp.game.graphics.startScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    },
    
    'clearAllEffects': (time) => {
      mp.game.graphics.startScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    },
    
    
})


/*
Event.OnServerEventTrigger.connect(function (eventName, args) {
    if (eventName === "getClientGround") {
        var clientLoc = API.getEntityPosition(API.getLocalPlayer());
        var ground = API.getGroundHeight(clientLoc);
        API.triggerServerEvent("findGround", ground, args[0]);
    }
    else if (eventName === "weedVisual") {
        timer = args[0];
        API.playScreenEffect("DrugsMichaelAliensFight", timer, false);
        weedblip = API.createBlip();
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
    else if (eventName === "clearAllEffects") {
        API.playScreenEffect("DrugsTrevorClownsFightOut", 1000, false);
    }
});
*/
