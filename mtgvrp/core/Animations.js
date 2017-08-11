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