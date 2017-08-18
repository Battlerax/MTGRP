API.onEntityStreamIn.connect((ent, entType) => {
    if (entType === 1) { // Vehicle
        API.callNative('SET_DISABLE_VEHICLE_PETROL_TANK_FIRES', ent, true);
        API.callNative('SET_DISABLE_VEHICLE_PETROL_TANK_DAMAGE', ent, true);

        //if vehicle is empty set undamageable.
        if (API.returnNative("GET_VEHICLE_NUMBER_OF_PASSENGERS", 0, ent) === 0 ||
            API.returnNative("IS_VEHICLE_SEAT_FREE", 8, ent, -1) === true) {

            API.callNative("SET_ENTITY_INVINCIBLE", ent, true);
            API.callNative("SET_ENTITY_PROOFS", ent, 1, 1, 1, 1, 1, 1, 1, 1);
            API.callNative("SET_VEHICLE_TYRES_CAN_BURST", ent, 0);
            API.callNative("SET_VEHICLE_WHEELS_CAN_BREAK", ent, 0);
            API.callNative("SET_VEHICLE_CAN_BE_VISIBLY_DAMAGED", ent, 0);

        } else {
            API.callNative("SET_ENTITY_INVINCIBLE", ent, false);
            API.callNative("SET_ENTITY_PROOFS", ent, 0, 0, 0, 0, 0, 0, 0, 0);
            API.callNative("SET_VEHICLE_TYRES_CAN_BURST", ent, 1);
            API.callNative("SET_VEHICLE_WHEELS_CAN_BREAK", ent, 1);
            API.callNative("SET_VEHICLE_CAN_BE_VISIBLY_DAMAGED", ent, 1);
        }

        API.triggerServerEvent("VehicleStreamedForPlayer", ent);
    }
});