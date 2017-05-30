var selID;
var editMenu = null;

function sendMessage(msg) {
    API.sendChatMessage("[Property Manager] " + msg);
}

API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "editproperty_showmenu":
            selID = args[0];
            editMenu = API.createMenu("Edit Property", "Select an action.", 0, 0, 6);
            editMenu.AddItem(API.createMenuItem("Property Name", "Set the name of property that shows in marker."));
            editMenu.AddItem(API.createMenuItem("Change Type", "Change the type of property."));
            editMenu.AddItem(API.createMenuItem("Supplies", "Set the amount of supplied in business."));
            editMenu.AddItem(API.createMenuItem("Move Property", "Move enterance."));
            editMenu.AddItem(API.createMenuItem("Set main door.", "Set this with the doorid that needs to be locked/unlocked via /lockproperty."));
            editMenu.AddItem(API.createMenuItem("Toggle Teleportable", "Set if person can /enter or not."));
            editMenu.AddItem(API.createMenuItem("Change Teleport Position", "Change the teleport position on /enter."));
            editMenu.AddItem(API.createMenuItem("Toggle Enteractable", "Set can interact."));
            editMenu.AddItem(API.createMenuItem("Move Interaction Point", "Moves the interaction point."));
            editMenu.AddItem(API.createMenuItem("Toggle Business Locked", "Set business as locked or not."));
            editMenu.AddItem(API.createMenuItem("~r~Delete Property", "Delete the property."));

            editMenu.Visible = true;

            editMenu.OnItemSelect.connect(function (sender, item, index) {
                switch(index) {
                    case 0:
                        var propname = "";
                        while (propname === "") {
                            propname = API.getUserInput("", 100);
                            if (propname === "") {
                                sendMessage("Property Name Can't Be Empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_setname", selID, propname);
                        break;

                    case 1:
                        var typename = "";
                        while (typename === "") {
                            typename = API.getUserInput("", 100);
                            if (typename === "") {
                                sendMessage("Type Can't Be Empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_settype", selID, typename);
                        break;

                    case 2:
                        var supplies = "";
                        while (supplies === "") {
                            supplies = API.getUserInput("", 100);
                            if (supplies === "") {
                                sendMessage("Supplies Amount Can't Be Empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_setsupplies", selID, supplies);
                        break;

                    case 3:

                        break;
                }
            });
            break;
    }
});

API.onUpdate.connect(function() {
    if (editMenu != null)
        API.drawMenu(editMenu);
});