var isInAnimation = false;
var resW = API.getScreenResolutionMaintainRatio().Width;
var resH = API.getScreenResolutionMaintainRatio().Height;

Event.OnKeyDown.connect(function (sender, e) {
    if (e.KeyCode === Keys.Space && !API.isChatOpen()) {
        if (isInAnimation) {
            isInAnimation = false;
            API.triggerServerEvent("stopPlayerAnims");
        }
    }
});

Event.OnServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case 'setPlayerIntoAnim':
            isInAnimation = true;
            break;
    }
});

Event.OnUpdate.connect(function () {
    if (isInAnimation) {
        API.drawText("~o~Press SPACE to stop the animation", resW / 2, resH - 100, 0.75, 255, 255, 255, 255, 4, 1, false, true, 0);
        if(API.returnNative('IS_ENTITY_IN_WATER', 8, API.getLocalPlayer()) === true) { /* checking if player is in water */
            isInAnimation = false;
            API.triggerServerEvent("stopPlayerAnims");
        }
    }
});