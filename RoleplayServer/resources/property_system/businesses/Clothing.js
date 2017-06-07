var menu_pool;

var pant_menu;
var shoe_menu;
var accessory_menu;
var undershirt_menu;
var top_menu;
var hat_menu;
var glasses_menu;
var ear_menu;


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

var component_list = [];

var creation_view = API.createCamera(new Vector3(403, -999.5, -98), new Vector3(0, 0, -15));
API.pointCameraAtPosition(creation_view, new Vector3(403, -998, -98.5));

var facial_view = API.createCamera(new Vector3(403, -998, -98.2), new Vector3(0, 0, -15));

API.onServerEventTrigger.connect((eventName, args) => {
	switch (eventName) {
	case "properties_buyclothes":
		//Setup items first.
		var lost = args[0];
		for (var a = 0; a < lost.Count; a++) {
			var obj = JSON.parse(lost[a]);

			var component = new Component();
			component.type = obj.type;
			component.name = obj.name;
			component.id = obj.id;
			component.variations = obj.variations;

			component_list.push(component);
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

		menu_pool.Add(pant_menu);
		menu_pool.Add(shoe_menu);
		menu_pool.Add(accessory_menu);
		menu_pool.Add(undershirt_menu);
		menu_pool.Add(top_menu);
		menu_pool.Add(hat_menu);
		menu_pool.Add(glasses_menu);
		menu_pool.Add(ear_menu);

		for (var i = 0; i < component_list.length; i++) {

			if (component_list[i].type == 4) { //Pants
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				pant_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
			} else if (component_list[i].type == 6) { //Shoes
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				shoe_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
			} else if (component_list[i].type == 7) { //Accessory
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				accessory_menu.AddItem(API.createListItem(component_list[i].name,
					"Press enter to select and go back.",
					list,
					0));
			} else if (component_list[i].type == 8) { //Undershirt
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				undershirt_menu.AddItem(API.createListItem(component_list[i].name,
					"Press enter to select and go back.",
					list,
					0));
			} else if (component_list[i].type == 11) { //Tops
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				top_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
			} else if (component_list[i].type == 20) { // hat
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				hat_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
			} else if (component_list[i].type == 21) { //glasses
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				glasses_menu.AddItem(API.createListItem(component_list[i].name,
					"Press enter to select and go back.",
					list,
					0));
			} else if (component_list[i].type == 22) { //ear
				var list = new List(String);
				for (var j = 0; j < component_list[i].variations; j++) {
					list.Add((j + 1).toString());
				}
				ear_menu.AddItem(
					API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
			}
		}

		var characterCreationMenu = API.createMenu("Clothes Shop", "Outfit Selection", 0, 0, 6);
		characterCreationMenu.AddItem(API.createMenuItem("Pants" + "          ~g~($" + pant_price + ")",
			"View the available pants"));
		characterCreationMenu.AddItem(API.createMenuItem("Shoes" + "           ~g~($" + shoe_price + ")",
			"View the available shoes"));
		characterCreationMenu.AddItem(API.createMenuItem("Accessories" + "     ~g~($" + accessory_price + ")",
			"View the available accesories. These may require not having an undershirt or top to see."));
		characterCreationMenu.AddItem(API.createMenuItem("Tops" + "            ~g~($" + top_price + ")",
			"View the available tops. Some of these require an undershirt."));
		characterCreationMenu.AddItem(API.createMenuItem("Undershirts" + "     ~g~($" + undershirt_price + ")",
			"View the available undershirts. These may require a certain top to look correct."));
		characterCreationMenu.AddItem(API.createMenuItem("Hats" + "            ~g~($" + hat_price + ")",
			"View the available hats"));
		characterCreationMenu.AddItem(API.createMenuItem("Glasses" + "         ~g~($" + glasses_price + ")",
			"View the available glasses"));
		characterCreationMenu.AddItem(API.createMenuItem("Ear Accessories" + " ~g~($" + ear_price + ")",
			"View the available ear accessories"));
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
			}
		});

		characterCreationMenu.OnMenuClose.connect(function(menu) {
			API.setActiveCamera(null);
			API.triggerServerEvent("closeclothingmenu");
		});


		pant_menu.OnIndexChange.connect(function(sender, index) {
			pantsIndex = index;
			pantsVariation = 0;
			API.triggerServerEvent("clothing_preview", 4, pantsIndex, pantsVariation);
		});

		pant_menu.OnListChange.connect(function(sender, list, index) {
			pantsVariation = index;
			API.triggerServerEvent("clothing_preview", 4, pantsIndex, pantsVariation);
		});

		pant_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 4, oldpantsIndex, oldpantsVariation);
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
			API.triggerServerEvent("clothing_preview", 6, shoeIndex, shoeVariation);
		});

		shoe_menu.OnListChange.connect(function(sender, list, index) {
			shoeVariation = index;
			API.triggerServerEvent("clothing_preview", 6, shoeIndex, shoeVariation);
		});

		shoe_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 6, oldshoeIndex, oldshoeVariation);
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
			API.triggerServerEvent("clothing_preview", 7, accessoryIndex, accessoryVariation);
		});

		accessory_menu.OnListChange.connect(function(sender, list, index) {
			accessoryVariation = index;
			API.triggerServerEvent("clothing_preview", 7, accessoryIndex, accessoryVariation);
		});

		accessory_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 7, oldaccessoryIndex, oldaccessoryVariation);
		});

		accessory_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(
				`Would you like to buy this ~r~Accessory~w~ for ~g~\$${accessory_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 7, accessoryIndex, accessoryVariation);
			}
		});

		undershirt_menu.OnIndexChange.connect(function(sender, index) {
			undershirtIndex = index;
			undershirtVariation = 0;
			API.triggerServerEvent("clothing_preview", 8, undershirtIndex, undershirtVariation);
		});

		undershirt_menu.OnListChange.connect(function(sender, list, index) {
			undershirtVariation = index;
			API.triggerServerEvent("clothing_preview", 8, undershirtIndex, undershirtVariation);
		});

		undershirt_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 8, oldundershirtIndex, oldundershirtVariation);
		});

		undershirt_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(
				`Would you like to buy this ~r~Undershirt~w~ for ~g~\$${undershirt_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 8, undershirtIndex, undershirtVariation);
			}
		});

		top_menu.OnIndexChange.connect(function(sender, index) {
			topIndex = index;
			topVariation = 0;
			API.triggerServerEvent("clothing_preview", 11, topIndex, topVariation);
		});

		top_menu.OnListChange.connect(function(sender, list, index) {
			topVariation = index;
			API.triggerServerEvent("clothing_preview", 11, topIndex, topVariation);
		});

		top_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 11, oldtopIndex, oldtopVariation);
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
			API.triggerServerEvent("clothing_preview", 20, hatIndex, hatVariation);

		});

		hat_menu.OnListChange.connect(function(sender, list, index) {
			hatVariation = index;
			API.triggerServerEvent("clothing_preview", 20, hatIndex, hatVariation);
		});

		hat_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 20, oldhatIndex, oldhatVariation);
		});

		hat_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(`Would you like to buy this ~r~Hat~w~ for ~g~\$${hat_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 20, hatIndex, hatVariation);
			}
		});

		glasses_menu.OnIndexChange.connect(function(sender, index) {
			glassesIndex = index;
			glassesVariation = 0;
			API.triggerServerEvent("clothing_preview", 21, glassesIndex, glassesVariation);
		});

		glasses_menu.OnListChange.connect(function(sender, list, index) {
			glassesVariation = index;
			API.triggerServerEvent("clothing_preview", 21, glassesIndex, glassesVariation);
		});

		glasses_menu.OnMenuClose.connect(function(menu) {
			API.triggerServerEvent("clothing_preview", 21, oldglassesIndex, oldglassesVariation);
			characterCreationMenu.Visible = true;
		});

		glasses_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(
				`Would you like to buy this ~r~Glasses~w~ for ~g~\$${glasses_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 21, glassesIndex, glassesVariation);
			}
		});

		ear_menu.OnIndexChange.connect(function(sender, index) {
			earIndex = index;
			earVariation = 0;
			API.triggerServerEvent("clothing_preview", 22, earIndex, earVariation);
		});

		ear_menu.OnListChange.connect(function(sender, list, index) {
			earVariation = index;
			API.triggerServerEvent("clothing_preview", 22, earIndex, earVariation);
		});

		ear_menu.OnMenuClose.connect(function(menu) {
			characterCreationMenu.Visible = true;
			API.triggerServerEvent("clothing_preview", 22, oldearIndex, oldearVariation);
		});

		ear_menu.OnItemSelect.connect(function(sender, item, index) {
			API.sendChatMessage(`Would you like to buy this ~r~Earrings~w~ for ~g~\$${ear_price}~w~ ? Enter 'yes' to confirm.`);
			if (API.getUserInput("no", 4) === "yes") {
				API.triggerServerEvent("clothing_buyclothe", 22, earIndex, earVariation);
			}
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
				API.triggerServerEvent("clothing_bag_closed");
			});

			bags_menu.OnItemSelect.connect(function(sender, item, index) {
				API.sendChatMessage(`Would you like to buy this ~r~Bag~w~ for ~g~\$${args[1]}~w~ ? Enter 'yes' to confirm.`);
				if (API.getUserInput("no", 4) === "yes") {
					API.triggerServerEvent("clothing_buybag", bag_index, bag_variation);
				}
			});
			break;
	}
});

var bags_menu = null;

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }

	if (bags_menu !== null)
		API.drawMenu(bags_menu);
});