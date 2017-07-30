var text = null;

API.onServerEventTrigger.connect((event, args) => {
    if (event === "UPDATE_SCUBA_PERCENTAGE") {
        if (args[0] === "none")
            text = null;
        else
            text = args[0];
    }
    else if (event === "REQUEST_SCUBA_UNDERWATER") {
        var value = API.returnNative("GET_ENTITY_SUBMERGED_LEVEL", 7, API.getLocalPlayer());
        if (value === 1.0) {
            API.triggerServerEvent("SCUBA_ISUNDERWATER");
        }
    }
});

API.onUpdate.connect(() => {
    if (text !== null) {
        var res = API.getScreenResolutionMaintainRatio();
        var pos = resource.JsFunctions.scaleCoordsToReal({ X: res.Width - 400, Y: res.Height - 60 });

        API.drawText(text, pos.X, pos.Y, 0.75, 0, 255, 0, 255, 1, 0, true, true, 0);
    }
});