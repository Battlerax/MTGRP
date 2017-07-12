/// <reference path="../../../types-gtanetwork/index.d.ts" />
var selectingPlayer = false;
var playerSelected = null;
var interactBrowser = null;
var res = null;
API.onResourceStart.connect(function () {
    res = API.getScreenResolutionMaintainRatio();
});
API.onServerEventTrigger.connect(function (eventname, args) {
    switch (eventname) {
        case "player_interact_subtitle":
            API.displaySubtitle(args[0], 1000);
            break;
    }
});
API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode == Keys.X && e.Control) {
        API.showCursor(true);
        selectingPlayer = true;
    }
    else if (e.KeyCode == Keys.Space) {
        API.triggerServerEvent("cancel_following");
    }
});
API.onKeyUp.connect(function (sender, e) {
    if (e.KeyCode == Keys.X && selectingPlayer == true) {
        API.showCursor(false);
        selectingPlayer = false;
        playerSelected = null;
        reset_browser();
    }
});
API.onUpdate.connect(function () {
    if (selectingPlayer) {
        API.disableControlThisFrame(25);
        var player = API.getLocalPlayer();
        var cursOp = API.getCursorPositionMaintainRatio();
        var s2w = API.screenToWorldMaintainRatio(cursOp);
        var rayCast = API.createRaycast(API.getGameplayCamPos(), s2w, 12, player);
        var player_hit = null;
        if (rayCast.didHitEntity) {
            player_hit = rayCast.hitEntity;
        }
        if (player_hit != null) {
            if (API.getEntityType(player_hit) == 6) {
                if (API.getEntityPosition(player).DistanceTo(API.getEntityPosition(player_hit)) < 10) {
                    API.displaySubtitle("Selecting player: " + player_hit.name, 500);
                    playerSelected = player_hit;
                }
            }
            if (API.isDisabledControlPressed(25) && playerSelected != null) {
                if (interactBrowser == null) {
                    loadBrowser(cursOp.X, cursOp.Y);
                    API.showCursor(false);
                    selectingPlayer = false;
                    API.displaySubtitle("");
                    if (API.getEntitySyncedData(player, "IsCop") == true) {
                        API.sleep(500);
                        interactBrowser.call("display_lspd_options");
                    }
                }
                else {
                    API.setCefBrowserPosition(interactBrowser, cursOp.X, cursOp.Y);
                }
            }
        }
    }
});
function loadBrowser(x, y) {
    interactBrowser = API.createCefBrowser(500, 500);
    API.waitUntilCefBrowserInit(interactBrowser);
    API.setCefBrowserPosition(interactBrowser, x, y);
    API.loadPageCefBrowser(interactBrowser, "player_manager/player_interaction/PlayerInteraction.html");
    API.waitUntilCefBrowserLoaded(interactBrowser);
    API.showCursor(true);
}
function reset_browser() {
    API.destroyCefBrowser(interactBrowser);
    API.showCursor(false);
    interactBrowser = null;
}
function interaction_menu_click(option) {
    switch (option) {
        case "exit":
            reset_browser();
            break;
        default:
            API.triggerServerEvent("player_interaction_menu", option, playerSelected);
            break;
    }
}
