"use strict";
var isFishing = false;
var res = null;
var perfectCatchStrength = 0;
var currentCatchStrength = 50;
var nextStrengthTick = 0;
var nextCatchTick = 0;
var catchTime = 0;
API.onResourceStart.connect(function () {
    res = API.getScreenResolutionMaintainRatio();
});
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "start_fishing":
            isFishing = true;
            perfectCatchStrength = args[0];
            currentCatchStrength = perfectCatchStrength;
            break;
    }
});
API.onUpdate.connect(function () {
    if (isFishing) {
        if (nextCatchTick == 0 || (nextCatchTick < API.getGlobalTime())) {
            nextCatchTick = API.getGlobalTime() + 1000;
            catchTime++;
        }
        if (nextStrengthTick == 0 || (nextStrengthTick < API.getGlobalTime())) {
            nextStrengthTick = API.getGlobalTime() + 250;
            if (currentCatchStrength - 1 > 0) {
                currentCatchStrength--;
            }
            else {
                currentCatchStrength = 0;
            }
        }
        var difference = currentCatchStrength - perfectCatchStrength;
        if (difference >= -2 && difference <= 2) {
            API.drawText("* Reeling Strength: " + currentCatchStrength + "% *", res.Width - 1220, 800, 0.75, 0, 255, 0, 255, 1, 0, true, true, 0);
        }
        else if (difference >= -10 && difference <= 10) {
            API.drawText("Reeling Strength: " + currentCatchStrength + "%", res.Width - 1220, 800, 0.75, 0, 255, 0, 255, 1, 0, true, true, 0);
        }
        else if (difference >= -15 && difference <= 15) {
            API.drawText("Reeling Strength: " + currentCatchStrength + "%", res.Width - 1220, 800, 0.75, 255, 255, 0, 255, 1, 0, true, true, 0);
        }
        else if (difference >= -20 && difference <= 20) {
            API.drawText("Reeling Strength: " + currentCatchStrength + "%", res.Width - 1220, 800, 0.75, 255, 130, 0, 255, 1, 0, true, true, 0);
        }
        else {
            API.drawText("Reeling Strength: " + currentCatchStrength + "%", res.Width - 1220, 800, 0.75, 255, 0, 0, 255, 1, 0, true, true, 0);
        }
        if (catchTime == 15) {
            API.triggerServerEvent("caught_fish", currentCatchStrength);
            isFishing = false;
            perfectCatchStrength = 0;
            currentCatchStrength = 50;
            nextCatchTick = 0;
            nextStrengthTick = 0;
            catchTime = 0;
        }
    }
});
API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode == Keys.Space && isFishing) {
        currentCatchStrength += 5;
        if (currentCatchStrength >= 125) {
            API.triggerServerEvent("snapped_rod");
            isFishing = false;
            perfectCatchStrength = 0;
            currentCatchStrength = 50;
            nextCatchTick = 0;
            nextStrengthTick = 0;
            catchTime = 0;
        }
    }
});
