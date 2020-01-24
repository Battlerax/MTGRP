var lastChop = Date.now();
var res = API.getScreenResolutionMaintainRatio();

function getPositionInfrontOfPlayer(distance) {
    var pos = API.getEntityPosition(API.getLocalPlayer());
    var a = API.getEntityRotation(API.getLocalPlayer()).Z;

    var rad = a * Math.PI / 180;

    var newpos = new Vector3(pos.X + (distance * Math.sin(-rad)),
        pos.Y + (distance * Math.cos(-rad)),
        pos.Z);
    return newpos;
}

Event.OnUpdate.connect(() => {
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

        var r = API.createRaycast(API.getEntityPosition(API.getLocalPlayer()).Subtract(new Vector3(0, 0, 0.8)), getPositionInfrontOfPlayer(5), 16, null);
        if (r.didHitEntity) {
            if (API.hasEntitySyncedData(r.hitEntity, "IS_TREE")) {
                API.triggerServerEvent("lumberjack_hittree");
                lastChop = Date.now() + 1000;
            }
        }
    }
});