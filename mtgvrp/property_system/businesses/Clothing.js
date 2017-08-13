var menu_pool = null;

var pant_menu;
var shoe_menu;
var accessory_menu;
var undershirt_menu;
var top_menu;
var hat_menu;
var glasses_menu;
var ear_menu;
var torso_menu;

var pantsIndex = 0;
var pantsVariation = 0;
var shoeIndex = 0;
var shoeVariation = 0;
var accessoryIndex = 0;
var accessoryVariation = 0;
var undershirtIndex = 0;
var undershirtVariation = 0;
var topIndex = 0;
var topVariation = 0;
var hatIndex = 0;
var hatVariation = 0;
var glassesIndex = 0;
var glassesVariation = 0;
var earIndex = 0;
var earVariation = 0;

var torsoIndex = 0;
var torsoVariation = 0;

var oldpantsIndex = 0;
var oldpantsVariation = 0;
var oldshoeIndex = 0;
var oldshoeVariation = 0;
var oldaccessoryIndex = 0;
var oldaccessoryVariation = 0;
var oldundershirtIndex = 0;
var oldundershirtVariation = 0;
var oldtopIndex = 0;
var oldtopVariation = 0;
var oldhatIndex = 0;
var oldhatVariation = 0;
var oldglassesIndex = 0;
var oldglassesVariation = 0;
var oldearIndex = 0;
var oldearVariation = 0;

var oldtorsoIndex = 0;
var oldtorsoVariation = 0;

var pant_price;
var shoe_price;
var accessory_price;
var undershirt_price;
var top_price;
var hat_price;
var glasses_price;
var ear_price;

//Component (clothing) 
var Component = (function () {
// ReSharper disable once InconsistentNaming
    function Component() {
        this.type = 0;
        this.name = "";
        this.id = 0;
        this.variations = 0;
    }
    return Component;
}());

var legs_list = [];
var shoes_list = [];
var access_list = [];
var undershits_list = [];
var tops_list = [];
var hats_list = [];
var glasses_list = [];
var ears_list = [];

var creation_view = API.createCamera(new Vector3(403, -999.5, -98), new Vector3(0, 0, -15));
API.pointCameraAtPosition(creation_view, new Vector3(403, -998, -98.5));

var facial_view = API.createCamera(new Vector3(403, -998, -98.2), new Vector3(0, 0, -15));

API.onServerEventTrigger.connect((eventName, args) => {
	switch (eventName) {
	case "properties_buyclothes":
		//Setup items first.
	    var list = JSON.parse(args[0]);

	    var newList = JSON.parse(list["Legs"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        legs_list.push(component);
	    }

	    newList = JSON.parse(list["Shoes"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        shoes_list.push(component);
	    }

	    newList = JSON.parse(list["Accessories"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        access_list.push(component);
	    }

	    newList = JSON.parse(list["Undershirts"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        undershits_list.push(component);
	    }

	    newList = JSON.parse(list["Tops"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        tops_list.push(component);
	    }

	    newList = JSON.parse(list["Hats"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        hats_list.push(component);
	    }

	    newList = JSON.parse(list["Glasses"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        glasses_list.push(component);
	    }

	    newList = JSON.parse(list["Ears"]);
	    for (var a = 0; a < newList.length; a++) {

	        var component = new Component();
	        component.name = newList[a][0];
	        component.id = newList[a][1];
	        component.variations = JSON.parse(newList[a][2]);

	        ears_list.push(component);
	    }

		//Set old clothes
		var oldClothes = JSON.parse(args[1]);
		oldpantsIndex = oldClothes[0][0];
		oldpantsVariation = oldClothes[0][1];
		oldshoeIndex = oldClothes[1][0];
		oldshoeVariation = oldClothes[1][1];
		oldaccessoryIndex = oldClothes[2][0];
		oldaccessoryVariation = oldClothes[2][1];
		oldundershirtIndex = oldClothes[3][0];
		oldundershirtVariation = oldClothes[3][1];
		oldtopIndex = oldClothes[4][0];
		oldtopVariation = oldClothes[4][1];
		oldhatIndex = oldClothes[5][0];
		oldhatVariation = oldClothes[5][1];
		oldglassesIndex = oldClothes[6][0];
		oldglassesVariation = oldClothes[6][1];
		oldearIndex = oldClothes[7][0];
		oldearVariation = oldClothes[7][1];

		//Set prices.
		var prices = JSON.parse(args[2]);
		pant_price = prices[0];
		shoe_price = prices[1];
		accessory_price = prices[2];
		undershirt_price = prices[3];
		top_price = prices[4];
		hat_price = prices[5];
		glasses_price = prices[6];
		ear_price = prices[7];

		var player = API.getLocalPlayer();
		menu_pool = API.getMenuPool();

		API.setEntityPosition(player, new Vector3(403, -997, -100));
		API.setEntityRotation(player, new Vector3(0, 0, 177.2663));

		API.sendChatMessage("~g~Select the clothes you would like to buy.");
		API.sendChatMessage("~y~NOTE: You can use the PLUS and MINUS keys to rotate your character!");

		API.setActiveCamera(creation_view);

		pant_menu = API.createMenu("Clothes Shop", "Pants Selection" + "         ~g~($" + pant_price + ")", 0, 0, 6);
		shoe_menu = API.createMenu("Clothes Shop", "Shoe Selection" + "          ~g~($" + shoe_price + ")", 0, 0, 6);
		accessory_menu =
			API.createMenu("Clothes Shop", "Accessory Selection" + "     ~g~($" + accessory_price + ")", 0, 0, 6);
		undershirt_menu = API.createMenu("Clothes Shop",
			"Undershirt Selection" + "    ~g~($" + undershirt_price + ")",
			0,
			0,
			6);
		top_menu = API.createMenu("Clothes Shop", "Top Selection" + "           ~g~($" + top_price + ")", 0, 0, 6);
		hat_menu = API.createMenu("Clothes Shop", "Hat Selection" + "           ~g~($" + hat_price + ")", 0, 0, 6);
		glasses_menu = API.createMenu("Clothes Shop", "Glasses Selection" + "       ~g~($" + glasses_price + ")", 0, 0, 6);
		ear_menu = API.createMenu("Clothes Shop", "Ear Accessory Selection" + " ~g~($" + ear_price + ")", 0, 0, 6);
		torso_menu = API.createMenu("Clothes Shop", "Torso Selection ~g~(Free)", 0, 0, 6);

		menu_pool.Add(pant_menu);
		menu_pool.Add(shoe_menu);
		menu_pool.Add(accessory_menu);
		menu_pool.Add(undershirt_menu);
		menu_pool.Add(top_menu);
		menu_pool.Add(hat_menu);
		menu_pool.Add(glasses_menu);
		menu_pool.Add(ear_menu);
		menu_pool.Add(torso_menu);

	    for (var i = 0; i < legs_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < legs_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        pant_menu.AddItem(API.createListItem(legs_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < shoes_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < shoes_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        shoe_menu.AddItem(API.createListItem(shoes_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < access_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < access_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        accessory_menu.AddItem(API.createListItem(access_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < undershits_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < undershits_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        undershirt_menu.AddItem(API.createListItem(undershits_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < tops_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < tops_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        top_menu.AddItem(API.createListItem(tops_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < hats_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < hats_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        hat_menu.AddItem(API.createListItem(hats_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < glasses_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < glasses_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        glasses_menu.AddItem(API.createListItem(glasses_list[i].name, "Press enter to select and go back.", list, 0));
	    }

	    for (var i = 0; i < ears_list.length; i++) {
	        var list = new List(String);
	        for (var j = 0; j < ears_list[i].variations.length; j++) {
	            list.Add((j + 1).toString());
	        }
	        ear_menu.AddItem(API.createListItem(ears_list[i].name, "Press enter to select and go back.", list, 0));
	    }

		var variations = API.returnNative("2834476523764480066", 0, player, 3);
		for (var i = 0; i < variations; i++) {
			var list = new List(String);
			var types = API.returnNative("10336137878209981357", 0, player, 3, i);
			for (var j = 0; j < types; j++) {
				list.Add((j + 1).toString());
			}
			torso_menu.AddItem(API.createListItem("Style " + i, "Press enter to select and go back.", list, 0));
        }

		var characterCreationMenu = API.createMenu("Clothes Shop", "Outfit Selection", 0, 0, 6);
        var item = API.createMenuItem("Pants", "View the available pants");
        item.SetRightLabel("~g~$" + pant_price);
        characterCreationMenu.AddItem(item);

        item = API.createMenuItem("Shoes", "View the available shoes");
	    item.SetRightLabel("~g~$" + shoe_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Accessories",
	        "View the available accesories. These may require not having an undershirt or top to see.");
	    item.SetRightLabel("~g~$" + accessory_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Tops", "View the available tops. Some of these require an undershirt.");
	    item.SetRightLabel("~g~$" + top_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Undershirts",
	        "View the available undershirts. These may require a certain top to look correct.");
	    item.SetRightLabel("~g~$" + undershirt_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Hats", "View the available hats");
	    item.SetRightLabel("~g~$" + hat_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Glasses", "View the available glasses");
	    item.SetRightLabel("~g~$" + glasses_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Ear Accessories", "View the available ear accessories");
	    item.SetRightLabel("~g~$" + ear_price);
        characterCreationMenu.AddItem(item);

	    item = API.createMenuItem("Torsos", "View the available torsos.");
	    item.SetRightLabel("~g~Free");
        characterCreationMenu.AddItem(item);

		menu_pool.Add(characterCreationMenu);
		characterCreationMenu.Visible = true;

		characterCreationMenu.OnItemSelect.connect(function(sender, item, index) {
			characterCreationMenu.Visible = false;
			switch (index) {
			case 0:
				API.setActiveCamera(creation_view);
				pant_menu.Visible = true;
				pant_menu.CurrentSelection = 0;
				break;
			case 1:
				API.setActiveCamera(creation_view);
				shoe_menu.Visible = true;
				shoe_menu.CurrentSelection = 0;
				break;
			case 2:
				API.setActiveCamera(creation_view);
				accessory_menu.Visible = true;
				accessory_menu.CurrentSelection = 0;
				break;
			case 3:
				API.setActiveCamera(creation_view);
				top_menu.Visible = true;
				top_menu.CurrentSelection = 0;
				break;
			case 4:
				API.setActiveCamera(creation_view);
				undershirt_menu.Visible = true;
				undershirt_menu.CurrentSelection = 0;
				break;
			case 5:
				API.setActiveCamera(facial_view);
				API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
				hat_menu.Visible = true;
				hat_menu.CurrentSelection = 0;
				break;
			case 6:
				API.setActiveCamera(facial_view);
				API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
				glasses_menu.Visible = true;
				glasses_menu.CurrentSelection = 0;
				break;
			case 7:
				API.setActiveCamera(facial_view);
				API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
				ear_menu.Visible = true;
				ear_menu.CurrentSelection = 0;
				break;
			case 8:
				API.setActiveCamera(creation_view);
				torso_menu.Visible = true;
				torso_menu.CurrentSelection = 0;
				break;
			}
		});

		characterCreationMenu.OnMenuClose.connect(function(menu) {
            API.setActiveCamera(null);
		    menu_pool = null;
			API.triggerServerEvent("closeclothingmenu");
		});


		pant_menu.OnIndexChange.connect(function(sender, index) {
			pantsIndex = index;
			pantsVariation = 0;
			updateClothes(4, pantsIndex, pantsVariation);
		});

		pant_menu.OnListChange.connect(function(sender, list, index) {
			pantsVariation = index;
			updateClothes(4, pantsIndex, pantsVariation);
		});

		pant_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(4, oldpantsIndex, oldpantsVariation);
		});

		pant_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(`Would you like to buy this ~r~Pants~w~ for ~g~\$${pant_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 4, pantsIndex, pantsVariation);
			}
		});

		shoe_menu.OnIndexChange.connect(function(sender, index) {
			shoeIndex = index;
			shoeVariation = 0;
			updateClothes(6, shoeIndex, shoeVariation);
		});

		shoe_menu.OnListChange.connect(function(sender, list, index) {
			shoeVariation = index;
			updateClothes(6, shoeIndex, shoeVariation);
		});

		shoe_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(6, oldshoeIndex, oldshoeVariation);
		});

		shoe_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(`Would you like to buy this ~r~Shoes~w~ for ~g~\$${shoe_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 6, shoeIndex, shoeVariation);
			}
		});

		accessory_menu.OnIndexChange.connect(function(sender, index) {
			accessoryIndex = index;
			accessoryVariation = 0;
			updateClothes(7, accessoryIndex, accessoryVariation);
		});

		accessory_menu.OnListChange.connect(function(sender, list, index) {
			accessoryVariation = index;
			updateClothes(7, accessoryIndex, accessoryVariation);
		});

		accessory_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(7, oldaccessoryIndex, oldaccessoryVariation);
		});

		accessory_menu.OnItemSelect.connect(function(sender, item, index) {
			if (accessoryIndex === 0) {
				API.triggerServerEvent("clothing_buyclothe", 7, accessoryIndex, accessoryVariation);
				return;
			}
			
			API.sendChatMessage(
				`Would you like to buy this ~r~Accessory~w~ for ~g~\$${accessory_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 7, accessoryIndex, accessoryVariation);
			}
		});

		undershirt_menu.OnIndexChange.connect(function(sender, index) {
			undershirtIndex = index;
			undershirtVariation = 0;
			updateClothes(8, undershirtIndex, undershirtVariation);
		});

		undershirt_menu.OnListChange.connect(function(sender, list, index) {
			undershirtVariation = index;
			updateClothes(8, undershirtIndex, undershirtVariation);
		});

		undershirt_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(8, oldundershirtIndex, oldundershirtVariation);
		});

		undershirt_menu.OnItemSelect.connect(function(sender, item, index) {
			if (undershirtIndex === 0) {
				API.triggerServerEvent("clothing_buyclothe", 8, undershirtIndex, undershirtVariation);
				return;
			}

			API.sendChatMessage(
				`Would you like to buy this ~r~Undershirt~w~ for ~g~\$${undershirt_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 8, undershirtIndex, undershirtVariation);
			}
		});

		top_menu.OnIndexChange.connect(function(sender, index) {
			topIndex = index;
			topVariation = 0;
			updateClothes(11, topIndex, topVariation);
		});

		top_menu.OnListChange.connect(function(sender, list, index) {
			topVariation = index;
			updateClothes(11, topIndex, topVariation);
		});

		top_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(11, oldtopIndex, oldtopVariation);
		});

		top_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(`Would you like to buy this ~r~Top~w~ for ~g~\$${top_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 11, topIndex, topVariation);
			}
		});

		hat_menu.OnIndexChange.connect(function(sender, index) {
			hatIndex = index;
			hatVariation = 0;
			updateClothes(20, hatIndex, hatVariation);

		});

		hat_menu.OnListChange.connect(function(sender, list, index) {
			hatVariation = index;
			updateClothes(20, hatIndex, hatVariation);
		});

		hat_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(20, oldhatIndex, oldhatVariation);
		});

		hat_menu.OnItemSelect.connect(function(sender, item, index) {
			if (hatIndex === 0) {
				API.triggerServerEvent("clothing_buyclothe", 20, hatIndex, hatVariation);
				return;
			}

			API.sendChatMessage(`Would you like to buy this ~r~Hat~w~ for ~g~\$${hat_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 20, hatIndex, hatVariation);
			}
		});

		glasses_menu.OnIndexChange.connect(function(sender, index) {
			glassesIndex = index;
			glassesVariation = 0;
			updateClothes(21, glassesIndex, glassesVariation);
		});

		glasses_menu.OnListChange.connect(function(sender, list, index) {
			glassesVariation = index;
			updateClothes(21, glassesIndex, glassesVariation);
		});

		glasses_menu.OnMenuClose.connect(function(menu) {
			updateClothes(21, oldglassesIndex, oldglassesVariation);
			characterCreationMenu.Visible = true;
		});

		glasses_menu.OnItemSelect.connect(function(sender, item, index) {
			if (glassesIndex === 0) {
				API.triggerServerEvent("clothing_buyclothe", 21, glassesIndex, glassesVariation);
				return;
			}

			API.sendChatMessage(
				`Would you like to buy this ~r~Glasses~w~ for ~g~\$${glasses_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 21, glassesIndex, glassesVariation);
			}
		});

		ear_menu.OnIndexChange.connect(function(sender, index) {
			earIndex = index;
			earVariation = 0;
			updateClothes(22, earIndex, earVariation);
		});

		ear_menu.OnListChange.connect(function(sender, list, index) {
			earVariation = index;
			updateClothes(22, earIndex, earVariation);
		});

		ear_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(22, oldearIndex, oldearVariation);
		});

		ear_menu.OnItemSelect.connect(function(sender, item, index) {
			if (earIndex === 0) {
				API.triggerServerEvent("clothing_buyclothe", 22, earIndex, earVariation);
				return;
			}

			API.sendChatMessage(`Would you like to buy this ~r~Earrings~w~ for ~g~\$${ear_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 22, earIndex, earVariation);
			}
		});

		torso_menu.OnIndexChange.connect(function(sender, index) {
			torsoIndex = index;
			torsoVariation = 0;
			updateClothes(3, torsoIndex, torsoVariation);
		});

		torso_menu.OnListChange.connect(function(sender, list, index) {
			earVariation = index;
			updateClothes(3, torsoIndex, torsoVariation);
		});

		torso_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			updateClothes(3, oldtorsoIndex, oldtorsoVariation);
		});

		torso_menu.OnItemSelect.connect(function(sender, item, index) {
			API.triggerServerEvent("clothing_buyclothe", 3, torsoIndex, torsoVariation);
		});
		break;

		case "clothing_boughtsucess":
			switch(args[0]) {
				case 4:
					oldpantsIndex = args[1];
					oldpantsVariation = args[2];
					break;
				case 6:
					oldshoeIndex = args[1];
					oldshoeVariation = args[2];
					break;
				case 7:
					oldaccessoryIndex = args[1];
					oldaccessoryVariation = args[2];
					break;
				case 8:
					oldundershirtIndex = args[1];
					oldundershirtVariation= args[2];
					break;
				case 11:
					oldtopIndex = args[1];
					oldtopVariation = args[2];
					break;
				case 20:
					oldhatIndex = args[1];
					oldhatVariation = args[2];
					break;
				case 21:
					oldglassesIndex = args[1];
					oldglassesVariation = args[2];
					break;
				case 22:
					oldearIndex = args[1];
					oldearVariation = args[2];
					break;
				case 3:
					oldtorsoIndex = args[1];
					oldtorsoVariation = args[2];
					break;
			}
		break;

		case "properties_buybag":
			var pl = API.getLocalPlayer();

			//Set pos.
			API.setEntityPosition(pl, new Vector3(403, -997, -100));
			API.setEntityRotation(pl, new Vector3(0, 0, 357.2663));
			API.setActiveCamera(creation_view);

			//remove curr bag
			API.setPlayerClothes(pl, 5, 0, 0);

			//menu
			bags_menu = API.createMenu("Bags", "Select a bag that fits you.", 0, 0, 6);
			API.setMenuBannerRectangle(bags_menu, 255, 60, 60, 255);

			var bag_index;
			var bag_variation;

			var bagsList = JSON.parse(args[0]);
			for (var a = 0; a < bagsList.length; a++) {
				var list = new List(String);
				for (var j = 0; j < parseInt(bagsList[a][1]); j++) {
					list.Add((j + 1).toString());
				}
				bags_menu.AddItem(API.createListItem(bagsList[a][0], "Press enter to select and go back.", list, 0));
			}

			bags_menu.Visible = true;

			bags_menu.OnIndexChange.connect(function(sender, index) {
				bag_index = index;
				bag_variation = 0;
				API.triggerServerEvent("clothing_bag_preview", bag_index, bag_variation);
			});

			bags_menu.OnListChange.connect(function(sender, list, index) {
				bag_variation = index;
				API.triggerServerEvent("clothing_bag_preview", bag_index, bag_variation);
			});

			bags_menu.OnMenuClose.connect(function(menu) {
                API.setActiveCamera(null);
			    bags_menu = null;
				API.triggerServerEvent("clothing_bag_closed");
			});

			bags_menu.OnItemSelect.connect(function(sender, item, index) {
				API.sendChatMessage(`Would you like to buy this ~r~Bag~w~ for ~g~\$${args[1]}~w~ ? Enter 'yes' to confirm.`);
				if (API.getUserInput("no", 4) === "yes") {
					API.triggerServerEvent("clothing_buybag", bag_index, bag_variation);
				}
			});
            break;

        case "checkPedGender":
            var hash = JSON.parse(args[0]);
            var ped = API.createPed(hash, new Vector3(0, 0, 0),5);
            var pedNo = API.returnNative("GET_PED_TYPE", 0, ped);
            API.deleteEntity(ped);
            API.triggerServerEvent("returnPedGender",hash,pedNo);
	}
});

function updateClothes(type, index, variation) {
    if (type === 4) {
        API.setPlayerClothes(API.getLocalPlayer(), type, parseInt(legs_list[index].id), parseInt(legs_list[index].variations[variation]) - 1);
    }
    else if (type === 6) {
        API.setPlayerClothes(API.getLocalPlayer(), type, parseInt(shoes_list[index].id), parseInt(shoes_list[index].variations[variation]) - 1);
    }
    else if (type === 7) {
        API.setPlayerClothes(API.getLocalPlayer(), type, parseInt(access_list[index].id), parseInt(access_list[index].variations[variation]) - 1);
    }
    else if (type === 8) {
        API.setPlayerClothes(API.getLocalPlayer(), type, parseInt(undershits_list[index].id), parseInt(undershits_list[index].variations[variation]) - 1);
    }
    else if (type === 11) {
        API.setPlayerClothes(API.getLocalPlayer(), type, parseInt(tops_list[index].id), parseInt(tops_list[index].variations[variation]) - 1);
    }
    else if (type === 20) {
        API.setPlayerAccessory(API.getLocalPlayer(), 0, parseInt(hats_list[index].id), parseInt(hats_list[index].variations[variation]) - 1);
    }
    else if (type === 21) {
        API.setPlayerAccessory(API.getLocalPlayer(), 1, parseInt(glasses_list[index].id), parseInt(glasses_list[index].variations[variation]) - 1);
    }
    else if (type === 22) {
        API.setPlayerAccessory(API.getLocalPlayer(), 2, parseInt(ears_list[index].id), parseInt(ears_list[index].variations[variation]) - 1);
    }
    else if (type === 3) {
        API.setPlayerClothes(API.getLocalPlayer(), type, index, variation);
    }
}

var bags_menu = null;

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }

	if (bags_menu !== null)
		API.drawMenu(bags_menu);
});

var rotating = 0;
API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode == Keys.Oemplus) {
        rotating = 4;

    } else if (e.KeyCode == Keys.OemMinus) {
        rotating = -4;
    }
});

API.onKeyUp.connect(function (sender, e) {
    if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.OemMinus) {
        rotating = 0;
    }
});

API.onUpdate.connect(function () {
    if (rotating != 0 && (menu_pool !== null || bags_menu !== null)) {
        var player = API.getLocalPlayer();
        var new_rot = API.getEntityRotation(player).Add(new Vector3(0, 0, rotating));
        API.setEntityRotation(player, new_rot);
    }
});