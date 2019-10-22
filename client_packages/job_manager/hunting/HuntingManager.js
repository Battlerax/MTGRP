Event.OnServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "update_animal_position":
            API.triggerServerEvent("update_animal_position", args[0], API.getEntityPosition(args[0]));
            break;
        case "toggle_animal_invincible":
            API.setEntityInvincible(args[0], args[1]);
            break;
    }
});