var lastChop = Date.now();

API.onUpdate.connect(() => {
    if (API.isControlJustPressed(24) && API.getPlayerCurrentWeapon() === -102973651) {

        if (lastChop > Date.now()) {
            return;
        }

        API.triggerServerEvent("lumberjack_hittree");
        lastChop = Date.now() + 1000;
    }
});