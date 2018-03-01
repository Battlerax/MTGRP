var isBlindfolded, camera;

mp.events.add({
    'blindfold_intiate': () => {
        camera = mp.cameras.new('default', mp.players.local.position, new mp.Vector3(90,0,0), 40);
        camera.setActive(true);
        mp.game.cam.renderScriptCams(true, false, 0, true, false);
        isBlindfolded = true;
    },
    
    'blindfold_cancel': () => {
        if (camera !== null) {
            camera.setActive(false);
            camera = null;
            isBlindfolded = false;
        }
    },
    
    'render': () => {
        if (isBlindfolded) {
            mp.game.ui.displayHud(false);
            mp.game.ui.displayRadar(false);
        }
    }
});