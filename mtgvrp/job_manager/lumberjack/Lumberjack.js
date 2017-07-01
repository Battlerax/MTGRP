var lastChop = Date.now();
var res = API.getScreenResolutionMantainRatio();

API.onUpdate.connect(() => {
    if (API.getPlayerCurrentWeapon() == -102973651) {
        API.disableControlThisFrame(24);

        if (lastChop < Date.now()) {
            API.stopPlayerAnimation();
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