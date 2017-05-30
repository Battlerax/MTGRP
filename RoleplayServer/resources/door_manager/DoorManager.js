var gMenu = null;
var actionMenu = null;

var chosenDoor;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "doormanager_managedoors":
            var doors = JSON.parse(args[0]);
            gMenu = API.createMenu("Door Manager", "", 0, 0, 6);
            var arrayLength = doors.length;
            for (var i = 0; i < arrayLength; i++) {
                gMenu.AddItem(API.createMenuItem(doors[i][0], doors[i][1]));
            }
            gMenu.AddItem(API.createMenuItem("Create New Door", "CreateDoor"));

            actionMenu = API.createMenu("Door Actions", "", 0, 0, 6);
            actionMenu.AddItem(API.createMenuItem("Toggle Lock", "Locks or unlocks door."));
            actionMenu.AddItem(API.createMenuItem("Change Description", "Chnage door description."));
            actionMenu.AddItem(API.createMenuItem("Goto", "Goto door."));
            actionMenu.AddItem(API.createMenuItem("~r~Delete", "Deletes door."));

            gMenu.Visible = true;

            gMenu.OnItemSelect.connect(function (sender, item, index) {
                API.sendChatMessage("You selected: ~g~" + item.Text);
                if (item.Description === "CreateDoor") {
                    gMenu.Visible = false;
                    API.showCursor(true);
                    selectingDoor = true;
                    API.sendChatMessage("[Door Manager] Select a door using your mouse.");
                } else {
                    chosenDoor = item.Description;
                    gMenu.Visible = false;
                    actionMenu.Visible = true;
                }
            });

            actionMenu.OnItemSelect.connect(function (sender, item, index) {
                switch(index) {
                    case 0:
                        API.triggerServerEvent("doormanager_togglelock", chosenDoor);
                        break;
                    case 1:
                        var desc = "";
                        while (desc === "") {
                            desc = API.getUserInput("", 100);
                            if (desc === "") {
                                API.sendChatMessage("[Door Manager] Description can't be empty.");
                            }
                        }
                        API.triggerServerEvent("doormanager_changedesc", chosenDoor, desc);
                        break;
                    case 2:
                        API.triggerServerEvent("doormanager_goto", chosenDoor);
                        break;
                    case 3:
                        API.triggerServerEvent("doormanager_delete", chosenDoor);
                        actionMenu.Visible = false;
                        break;
                }
            });
            break;
    }
});

var selectingDoor = false;
var lastDoor = null;
var lastDoorV = 0;

API.onUpdate.connect(function () {
    if (gMenu != null)
        API.drawMenu(gMenu);
    if (actionMenu != null)
        API.drawMenu(actionMenu);

    if (selectingDoor) {
        var cursOp = API.getCursorPositionMantainRatio();
        var s2w = API.screenToWorldMantainRatio(cursOp);
        var rayCast = API.createRaycast(API.getGameplayCamPos(), s2w, -1, null);
        var localH = null;
        var localV = 0;
        if (rayCast.didHitEntity) {
            localH = rayCast.hitEntity;
            localV = localH.Value;
        }

        API.displaySubtitle("Object Handle: " + localV);

        if (localV != lastDoorV) {
            if (localH != null) API.setEntityTransparency(localH, 50);
            if (lastDoor != null) API.setEntityTransparency(lastDoor, 255);
            lastDoor = localH;
            lastDoorV = localV;
        }

        if (API.isDisabledControlJustPressed(24)) {
            API.showCursor(false);
            selectingDoor = false;

            if (localH != null) {
                API.sendChatMessage("Object model is " + API.getEntityModel(localH));
                var model = API.getEntityModel(localH);
                var pos = API.getEntityPosition(localH);
                API.sendChatMessage("[Door Manager] Enter a description.");
                var desc = "";
                while (desc === "") {
                    desc = API.getUserInput("", 100);
                    if (desc === "") {
                        API.sendChatMessage("[Door Manager] Description can't be empty.");
                    }
                }
                API.triggerServerEvent("doormanager_createdoor", model, pos, desc);
            }
        }
    }
    else if (lastDoor != null) {
        API.setEntityTransparency(lastDoor, 255);
        lastDoor = null;
    }
});