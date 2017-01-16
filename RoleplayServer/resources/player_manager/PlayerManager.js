API.onEntityStreamIn.connect(function (ent, entType) {
    if (entType == 6 || entType == 8) {
        API.triggerServerEvent("update_ped_for_client", ent);
    }
});