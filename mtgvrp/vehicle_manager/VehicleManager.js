API.onEntityStreamIn.connect((ent, entType) => {
    if (entType === 1) { // Vehicle
        API.callNative('SET_DISABLE_VEHICLE_PETROL_TANK_FIRES', ent, true);
        API.callNative('SET_DISABLE_VEHICLE_PETROL_TANK_DAMAGE', ent, true);
    }
});