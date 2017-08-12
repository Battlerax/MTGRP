var isInAnimation = false;

API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode === Keys.Space) {
        if (isInAnimation) {
            isInAnimation = false;
            API.triggerServerEvent("stopPlayerAnims");
        }
    }
});

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case 'setPlayerIntoAnim':
            isInAnimation = true;
            break;
    }
});

API.onUpdate.connect(function () {
    if(isInAnimation) {
        if(API.callNative("0xCFB0A0D8EDD145A3", API.getLocalPlayer()) == true) { /* checking if player is in water */
            isInAnimation = false;
            API.triggerServerEvent("stopPlayerAnims");
        }
    }
});