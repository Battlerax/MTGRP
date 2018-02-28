var isInAnimation = false;
var resW = API.getScreenResolutionMaintainRatio().Width;
var resH = API.getScreenResolutionMaintainRatio().Height;

mp.keys.bind(0x20, false, function() {
    if (isInAnimation) {
        isInAnimation = false;
        mp.events.callRemote("stopPlayerAnims");
    }
})

mp.events.add
({    
    "setPlayerIntoAnim": () => {
    isInAnimation = true;
    },
    
    "render": () => {
        if (isInAnimation) {
            mp.game.graphics.drawText("~o~Press SPACE to stop the animation", [0.5, 0.8], { 
      font: 7, 
      color: [255, 255, 255, 185], 
      scale: [1.2, 1.2], 
      outline: true
    });
    
    if (mp.players.local.isInWater()) {
        isInAnimation = false;
            mp.events.callRemote("stopPlayerAnims");
    }
    
        }
    }
});