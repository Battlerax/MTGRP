var selID;
var editMenu = null;

function sendMessage(msg) {
    API.SendChatMessage("[Property Manager] " + msg);
}

var IPLlist;
var selIPL = 0;
Event.OnServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "editproperty_showmenu":
            if (editMenu !== null) {
                editMenu.Visible = false;
            }

            selID = args[0];
            editMenu = API.createMenu("Edit Property", "Select an action.", 0, 0, 6);
	        API.setMenuBannerRectangle(editMenu, 255, 60, 60, 255);
            editMenu.AddItem(API.createMenuItem("Property Name", "Set the name of property that shows in marker."));
            editMenu.AddItem(API.createMenuItem("Change Type", "Change the type of property."));
            editMenu.AddItem(API.createMenuItem("Supplies", "Set the amount of supplied in business."));
            editMenu.AddItem(API.createMenuItem("Move Property", "Move entrance."));
            editMenu.AddItem(API.createMenuItem("Goto Property", "Teleport to entrance."));
            editMenu.AddItem(API.createMenuItem("Set main door.", "Set this with the doorid that needs to be locked/unlocked via /lockproperty."));
            editMenu.AddItem(API.createMenuItem("Toggle Teleportable", "Set if person can /enter or not."));
            editMenu.AddItem(API.createMenuItem("Change Teleport Position", "Change the teleport position on /enter."));
            editMenu.AddItem(API.createMenuItem("Toggle Interactable", "Set can interact."));
            editMenu.AddItem(API.createMenuItem("Move Interaction Point", "Moves the interaction point."));
            editMenu.AddItem(API.createMenuItem("Toggle Property Locked", "Set property as locked or not."));
            editMenu.AddItem(API.createMenuItem("Set Price", "Set property's price."));
            editMenu.AddItem(API.createMenuItem("Set Owner", "Set property's owner."));
			editMenu.AddItem(API.createMenuItem("Set Garbage Point", "Set property garbage point."));
            editMenu.AddItem(API.createMenuItem("Toggle Has Garbage Point", "Toggle whether the property has a garbage point or not."));

            IPLlist = JSON.parse(args[1]);
            var items = new List(String);
            for (var i = 0; i < IPLlist.length; i++) {
                items.Add(IPLlist[i]);
            }
            items.Add("Add New");

            var listitem = API.createListItem("Change IPLs",
                "Toggle whether the property has a garbage point or not.",
                items,
                0);

            editMenu.AddItem(listitem);

            listitem.OnListChanged.connect(function (sender, index) {
                selIPL = index;
            });

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
                        API.triggerServerEvent("editproperty_gotoentrance", selID);
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
                        changingGarbagePointPos = true;
                        sendMessage("Goto the position you would like and hit LMB to save there.");
                        break;

					case 14:
                        API.triggerServerEvent("editproperty_togglehasgarbage", selID);
                        break;

                    case 15:
                        API.SendChatMessage("Selected ID: " + selIPL + " Length: " + IPLlist.length);
                        if (selIPL === IPLlist.length) {
                            var ipl = "";
                            while (ipl === "") {
                                ipl = API.getUserInput("", 100);
                                if (ipl === "") {
                                    sendMessage("IPL can't be empty.");
                                }
                            }
                            API.triggerServerEvent("editproperty_addipl", selID, ipl);
                        } else {
                            API.triggerServerEvent("editproperty_deleteipl", selID, IPLlist[selIPL]);
                        }
                        break;
                    
                    case 16:
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
var changingGarbagePointPos = false;

Event.OnUpdate.connect(function() {
    if (editMenu != null)
        API.drawMenu(editMenu);

    if (API.isControlJustPressed(24)) {
        if (changingPropertyPos === true) {
            changingPropertyPos = false;
            API.triggerServerEvent("editproperty_setentrancepos", selID);
        } 
        else if (changingTeleportPos === true) {
            changingTeleportPos = false;
            API.triggerServerEvent("editproperty_setteleportpos", selID);
        }
        else if (changingInteractionPos === true) {
            changingInteractionPos = false;
            API.triggerServerEvent("editproperty_setinteractpos", selID);
        }
		else if (changingGarbagePointPos === true) {
            changingGarbagePointPos = false;
            API.triggerServerEvent("editproperty_setgarbagepoint", selID);
        }
    }
});

Event.OnKeyUp.connect(function (sender, e)
{
	if (e.KeyCode === Keys.C && API.isChatOpen() === false) {
		API.triggerServerEvent("attempt_enter_prop");
	}
});