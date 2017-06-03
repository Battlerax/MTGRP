var menu_pool;
var pant_menu;
var shoe_menu;
var accessory_menu;
var undershirt_menu;
var top_menu;
var hat_menu;
var glasses_menu;
var ear_menu;

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
        case "clothing_initialize_components":
            var list = args[0];
            for (var i = 0; i < list.Count; i++) {
                var obj = JSON.parse(list[i]);

                var component = new Component();
                component.type = obj.type;
                component.name = obj.name;
                component.id = obj.id;
                component.variations = obj.variations;

                component_list.push(component);
            }
            break;

        case "properties_buyclothes":
            var player = API.getLocalPlayer();
            menu_pool = API.getMenuPool();

            API.sendChatMessage("~g~Select the clothes you would like to buy.");
            API.sendChatMessage("~y~NOTE: You can use the PLUS and MINUS keys to rotate your character!");

            API.setActiveCamera(creation_view);

            pant_menu = API.createMenu("Character Creation", "Pants Selection", 0, 0, 6);
            shoe_menu = API.createMenu("Character Creation", "Shoe Selection", 0, 0, 6);
            accessory_menu = API.createMenu("Character Creation", "Accessory Selection", 0, 0, 6);
            undershirt_menu = API.createMenu("Character Creation", "Undershirt Selection", 0, 0, 6);
            top_menu = API.createMenu("Character Creation", "Top Selection", 0, 0, 6);
            hat_menu = API.createMenu("Character Creation", "Hat Selection", 0, 0, 6);
            glasses_menu = API.createMenu("Character Creation", "Glasses Selection", 0, 0, 6);
            ear_menu = API.createMenu("Character Creation", "Ear Accessory Selection", 0, 0, 6);

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
                    pant_menu.AddItem(API.createListItem(component_list[i].name,
                        "Press enter to select and go back.",
                        list,
                        0));
                }
                else if (component_list[i].type == 6) { //Shoes
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    shoe_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 7) { //Accessory
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    accessory_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 8) { //Undershirt
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    undershirt_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 11) { //Tops
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    top_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 20) { // hat
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    hat_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 21) { //glasses
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    glasses_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }
                else if (component_list[i].type == 22) { //ear
                    var list = new List(String);
                    for (var j = 0; j < component_list[i].variations; j++) {
                        list.Add((j + 1).toString());
                    }
                    ear_menu.AddItem(API.createListItem(component_list[i].name, "Press enter to select and go back.", list, 0));
                }


            }

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
            var hatIndex = -1;
            var hatVariation = 0;
            var glassesIndex = 0;
            var glassesVariation = 0;
            var earIndex = 0;
            var earVariation = 0;

            var characterCreationMenu = API.createMenu("Character Creation", "Outfit Selection", 0, 0, 6);
            characterCreationMenu.AddItem(API.createMenuItem("Pants", "View the available pants"));
            characterCreationMenu.AddItem(API.createMenuItem("Shoes", "View the available shoes"));
            characterCreationMenu.AddItem(API.createMenuItem("Accessories", "View the available accesories. These may require not having an undershirt or top to see."));
            characterCreationMenu.AddItem(API.createMenuItem("Tops", "View the available tops. Some of these require an undershirt."));
            characterCreationMenu.AddItem(API.createMenuItem("Undershirts", "View the available undershirts. These may require a certain top to look correct."));
            characterCreationMenu.AddItem(API.createMenuItem("Hats", "View the available hats"));
            characterCreationMenu.AddItem(API.createMenuItem("Glasses", "View the available glasses"));
            characterCreationMenu.AddItem(API.createMenuItem("Ear Accessories", "View the available ear accessories"));
            menu_pool.Add(characterCreationMenu);
            characterCreationMenu.Visible = true;

            pant_menu.OnIndexChange.connect(function (sender, index) {
                pantsIndex = index;
                pantsVariation = 0;
                API.setPlayerClothes(player, 4, pantsIndex, pantsVariation);
            });

            pant_menu.OnListChange.connect(function (sender, list, index) {
                pantsVariation = index;
                API.setPlayerClothes(player, 4, pantsIndex, pantsVariation);
            });

            shoe_menu.OnIndexChange.connect(function (sender, index) {
                shoeIndex = index;
                shoeVariation = 0;
                API.setPlayerClothes(player, 6, shoeIndex, shoeVariation);
            });

            shoe_menu.OnListChange.connect(function (sender, list, index) {
                shoeVariation = index;
                API.setPlayerClothes(player, 6, shoeIndex, shoeVariation);
            });

            accessory_menu.OnIndexChange.connect(function (sender, index) {
                accessoryIndex = index;
                accessoryVariation = 0;
                API.setPlayerClothes(player, 7, accessoryIndex, accessoryVariation);
            });

            accessory_menu.OnListChange.connect(function (sender, list, index) {
                accessoryVariation = index;
                API.setPlayerClothes(player, 7, accessoryIndex, accessoryVariation);
            });

            undershirt_menu.OnIndexChange.connect(function (sender, index) {
                undershirtIndex = index;
                undershirtVariation = 0;
                API.setPlayerClothes(player, 8, undershirtIndex, undershirtVariation);
            });

            undershirt_menu.OnListChange.connect(function (sender, list, index) {
                undershirtVariation = index;
                API.setPlayerClothes(player, 8, undershirtIndex, undershirtVariation);
            });

            top_menu.OnIndexChange.connect(function (sender, index) {
                topIndex = index;
                topVariation = 0;
                API.setPlayerClothes(player, 11, topIndex, topVariation);
            });

            top_menu.OnListChange.connect(function (sender, list, index) {
                topVariation = index;
                API.setPlayerClothes(player, 11, topIndex, topVariation);
            });

            hat_menu.OnIndexChange.connect(function (sender, index) {
                hatIndex = index;
                hatVariation = 0;
                API.setPlayerClothes(player, 20, hatIndex, hatVariation);
            });

            hat_menu.OnListChange.connect(function (sender, list, index) {
                hatVariation = index;
                API.setPlayerClothes(player, 20, hatIndex, hatVariation);
            });

            glasses_menu.OnIndexChange.connect(function (sender, index) {
                glassesIndex = index;
                glassesVariation = 0;
                API.setPlayerClothes(player, 21, glassesIndex, glassesVariation);
            });

            glasses_menu.OnListChange.connect(function (sender, list, index) {
                glassesVariation = index;
                API.setPlayerClothes(player, 21, glassesIndex, glassesVariation);
            });

            ear_menu.OnIndexChange.connect(function (sender, index) {
                earIndex = index;
                earVariation = 0;
                API.setPlayerClothes(player, 22, earIndex, earVariation);
            });

            ear_menu.OnListChange.connect(function (sender, list, index) {
                earVariation = index;
                API.setPlayerClothes(player, 22, earIndex, earVariation);
            });

            characterCreationMenu.OnItemSelect.connect(function (sender, item, index) {
                characterCreationMenu.Visible = false;
                switch (item.Text) {
                    case "Pants":
                        API.setActiveCamera(creation_view);
                        pant_menu.Visible = true;
                        pant_menu.CurrentSelection = 0;
                        break;
                    case "Shoes":
                        API.setActiveCamera(creation_view);
                        shoe_menu.Visible = true;
                        shoe_menu.CurrentSelection = 0;
                        break;
                    case "Accessories":
                        API.setActiveCamera(creation_view);
                        accessory_menu.Visible = true;
                        accessory_menu.CurrentSelection = 0;
                        break;
                    case "Undershirts":
                        API.setActiveCamera(creation_view);
                        undershirt_menu.Visible = true;
                        undershirt_menu.CurrentSelection = 0;
                        break;
                    case "Tops":
                        API.setActiveCamera(creation_view);
                        top_menu.Visible = true;
                        top_menu.CurrentSelection = 0;
                        break;
                    case "Hats":
                        API.setActiveCamera(facial_view);
                        API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
                        hat_menu.Visible = true;
                        hat_menu.CurrentSelection = 0;
                        break;
                    case "Glasses":
                        API.setActiveCamera(facial_view);
                        API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
                        glasses_menu.Visible = true;
                        glasses_menu.CurrentSelection = 0;
                        break;
                    case "Ear Accessories":
                        API.setActiveCamera(facial_view);
                        API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));
                        ear_menu.Visible = true;
                        ear_menu.CurrentSelection = 0;
                        break;
                }
            });
            break;
    }
});