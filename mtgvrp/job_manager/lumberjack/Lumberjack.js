var lastChop = Date.now();
var res = API.getScreenResolutionMantainRatio();

API.onUpdate.connect(() => {
    if (API.getPlayerCurrentWeapon() == -102973651) {
        API.disableControlThisFrame(24);

        if (lastChop < Date.now() && lastChop != -1) {
            API.stopPlayerAnimation();
            lastChop = -1;
        }
    }

    if (API.isControlJustPressed(24) && API.getPlayerCurrentWeapon() === -102973651) {

        if (lastChop > Date.now()) {
            return;
        }

        API.triggerServerEvent("lumberjack_hittree");
        lastChop = Date.now() + 1000;
    }
});