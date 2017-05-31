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
            editMenu.AddItem(API.createMenuItem("Goto Property", "Teleport to enterance."));
            editMenu.AddItem(API.createMenuItem("Set main door.", "Set this with the doorid that needs to be locked/unlocked via /lockproperty."));
            editMenu.AddItem(API.createMenuItem("Toggle Teleportable", "Set if person can /enter or not."));
            editMenu.AddItem(API.createMenuItem("Change Teleport Position", "Change the teleport position on /enter."));
            editMenu.AddItem(API.createMenuItem("Toggle Enteractable", "Set can interact."));
            editMenu.AddItem(API.createMenuItem("Move Interaction Point", "Moves the interaction point."));
            editMenu.AddItem(API.createMenuItem("Toggle Property Locked", "Set property as locked or not."));
            editMenu.AddItem(API.createMenuItem("Set Price", "Set property's price."));
            editMenu.AddItem(API.createMenuItem("Set Owner", "Set property's owner."));
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
                        editMenu.Visible = false;
                        changingPropertyPos = true;
                        sendMessage("Goto the position you would like and hit LMB to save there.");
                        break;

                    case 4:
                        API.triggerServerEvent("editproperty_gotoenterance", selID);
                        break;

                    case 5:
                        var doorid = "";
                        while (doorid === "") {
                            doorid = API.getUserInput("", 100);
                            if (doorid === "") {
                                sendMessage("DoorID can't be empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_setmaindoor", selID, doorid);
                        break;

                    case 6:
                        API.triggerServerEvent("editproperty_toggleteleportable", selID);
                        break;

                    case 7:
                        editMenu.Visible = false;
                        changingTeleportPos = true;
                        sendMessage("Goto the position you would like and hit LMB to save there.");
                        break;

                    case 8:
                        API.triggerServerEvent("editproperty_toggleinteractable", selID);
                        break;

                    case 9:
                        editMenu.Visible = false;
                        changingInteractionPos = true;
                        sendMessage("Goto the position you would like and hit LMB to save there.");
                        break;

                    case 10:
                        API.triggerServerEvent("editproperty_togglelock", selID);
                        break;

                    case 11:
                        var price = "";
                        while (price === "") {
                            price = API.getUserInput("", 100);
                            if (price === "") {
                                sendMessage("Price can't be empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_setprice", selID, price);
                        break;

                    case 12:
                        var owner = "";
                        while (owner === "") {
                            owner = API.getUserInput("", 100);
                            if (owner === "") {
                                sendMessage("Owner can't be empty.");
                            }
                        }
                        API.triggerServerEvent("editproperty_setowner", selID, owner);
                        break;

                    case 13:
                        editMenu.Visible = false;
                        API.triggerServerEvent("editproperty_deleteproperty", selID);
                        break;
                }
            });
            break;
    }
});

var changingPropertyPos = false;
var changingTeleportPos = false;
var changingInteractionPos = false;

API.onUpdate.connect(function() {
    if (editMenu != null)
        API.drawMenu(editMenu);

    if (API.isControlJustPressed(24)) {
        if (changingPropertyPos === true) {
            changingPropertyPos = false;
            API.triggerServerEvent("editproperty_setenterancepos", selID);
        } 
        else if (changingTeleportPos === true) {
            changingTeleportPos = false;
            API.triggerServerEvent("editproperty_setteleportpos", selID);
        }
        else if (changingInteractionPos === true) {
            changingInteractionPos = false;
            API.triggerServerEvent("editproperty_setinteractpos", selID);
        }
    }
});