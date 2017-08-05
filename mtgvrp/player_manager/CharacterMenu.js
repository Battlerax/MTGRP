var menu_pool = null;
var character_menu = null;
var character_creation_menu = null;

var gender = 0;

var MAX_HAIR_STYLE = 0;
var MAX_HAIR_COLORS = API.returnNative("_GET_NUM_HAIR_COLORS", 0);

var MAX_MAKEUP_COLORS = API.returnNative("_GET_NUM_MAKEUP_COLORS", 0);
var MAX_BLUSH_COLORS = 33;
var MAX_LIPSTICK_COLORS = 27;

function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

//Component (clothing) 
var Component = (function () {
    function Component() {
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

//Create the camera view to watch character creation
var creation_view = API.createCamera(new Vector3(403, -999.5, -98), new Vector3(0, 0, -15));
API.pointCameraAtPosition(creation_view, new Vector3(403, -998, -98.5));

var facial_view = API.createCamera(new Vector3(403, -998, -98.2), new Vector3(0, 0, -15));

API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "showCharacterSelection") {
        var player = API.getLocalPlayer();
        menu_pool = API.getMenuPool();

        character_menu = API.createMenu("Character Selection", "Select a character below", -200, 75, 4);

        var char_count = API.getEntitySyncedData(player, "char_count");

        for (var i = 0; i < char_count; i++) {
            var menu_item = API.createMenuItem(API.getEntitySyncedData(player, "char_name_" + i), API.getEntitySyncedData(player, "char_info_" + i));
            character_menu.AddItem(menu_item);
        }

        menu_pool.Add(character_menu);
        character_menu.Visible = true;
        character_menu.CurrentSelection = 0;

        character_menu.OnItemSelect.connect(function (sender, item, index) {
            if (item.Text == "Create new character") {
	            API.sendChatMessage("* Enter your desired character name: ");
                
				var res = false;
				while (res === false) {
					API.sendChatMessage("Character name must be similar to: ~g~John_Doe~w~.");
                    var desiredName = API.getUserInput("", 64);
				    if (desiredName === "")
				        return;
					var patt = new RegExp("^[A-Z][a-zA-Z]+_[A-Z][a-zA-Z]+$");
					res = patt.test(desiredName);
				}
				API.triggerServerEvent("OnCharacterMenuSelect", item.Text, desiredName);
            }
            else {
                API.triggerServerEvent("OnCharacterMenuSelect", item.Text);
            }

        });
    } else if (eventName == "show_character_creation_menu") {
        var player = API.getLocalPlayer();
        API.setEntityDimension(player, getRandomInt(1, 5000));
        next_character_creation_step(player, 0);
    }
    else if (eventName == "login_finished") {
        character_menu.Visible = false;
		menu_pool = null;
    }
    else if (eventName == "initialize_hair") {
        MAX_HAIR_STYLE = args[0];
    }
    else if (eventName == "initialize_components") {
       
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
    }
});


API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
		API.disableAllControlsThisFrame();
    }
});

var pant_menu = null;
var shoe_menu = null;
var accessory_menu = null;
var undershirt_menu = null;
var top_menu = null;
var hat_menu = null;
var glasses_menu = null;
var ear_menu = null;
var torso_menu = null;

var father_ped = null;
var mother_ped = null;

function next_character_creation_step(player, step) {

    switch (step) {
        case 0: {
            menu_pool = API.getMenuPool();

	        API.sendChatMessage("~g~Welcome to character creation! Let's begin by choosing your gender and your parents!");

            API.triggerServerEvent("initialize_hair", gender);

            var gender_menu_item = null;
            var father_menu_item = null;
            var mother_menu_item = null;
            var parent_lean_menu_item = null;
            var next_menu_item = null;

            var gender_list = new List(String);
            var father_list = new List(String);
            var mother_list = new List(String);
            var parent_lean_list = new List(String);

            //Father IDs: 0-20, 42,43,44
            //Mother IDs: 21-41, 45
            var father_int_id = 0;
            var mother_int_id = 21;
            var parent_lean = 0.5;

            //Set Camera to CharacterCreation
            API.setActiveCamera(creation_view);
            API.setEntityPosition(player, new Vector3(403, -997, -100));
            API.setEntityRotation(player, new Vector3(0, 0, 177.2663));
			API.setEntityDimension(player, API.getEntitySyncedData(player, "REG_DIMENSION"));
	        API.setPlayerSkin(1885233650);

            //Initiate the lists
            gender_list.Add("Male");
            gender_list.Add("Female");
            for (var i = 0; i < 11; i++) {
                parent_lean_list.Add(i.toString());
            }
            for (var i = 0; i < 24; i++) {
                father_list.Add((i + 1).toString());
            }
            for (var i = 0; i < 22; i++) {
                mother_list.Add((i + 1).toString());
            }

            //Initalize the menu options
            gender_menu_item = API.createListItem("Gender", "Select your characters gender.", gender_list, 0);
            father_menu_item = API.createListItem("Father", "Select your father.", father_list, 0);
            mother_menu_item = API.createListItem("Mother", "Select your mother.", mother_list, 0);
            parent_lean_menu_item = API.createListItem("Parent Lean", "Select towards which parent your traits lean. (0 - 10, Father to Mother)", parent_lean_list, 0);
            next_menu_item = API.createMenuItem("Next", "Continue onto the next portion of character creation.");

            //Create the display peds and set their info
			if(father_ped !== null)
				API.deleteEntity(father_ped);
	        if(mother_ped !== null)
		        API.deleteEntity(mother_ped);

            father_ped = API.createPed(1885233650, new Vector3(402.5, -996.5, -99), 172);
            mother_ped = API.createPed(-1667301416, new Vector3(403.38, -996.5, -99), 172);
            API.triggerServerEvent("change_parent_info", father_ped, mother_ped, father_int_id, mother_int_id, parent_lean, gender);

			//Set default clothes.
	        /*updateClothes(4, 0, 0);
	        updateClothes(6, 0, 0);
	        updateClothes(11, 0, 0);*/

            //Handle the lists
            gender_menu_item.OnListChanged.connect(function (sender, new_index) {
                if (new_index == 0) {
                    makeup = 255;
                    blush = 255;
                    lipstick = 255;
                    API.setPlayerSkin(1885233650);
                }
                else {
                    facial_hair = 255;
                    API.setPlayerSkin(-1667301416);
                }
               
                gender = new_index;
                API.triggerServerEvent("initialize_hair", gender);
                API.triggerServerEvent("change_parent_info", father_ped, mother_ped, father_int_id, mother_int_id, parent_lean, gender);
            });

            father_menu_item.OnListChanged.connect(function (sender, new_index) {
                if (new_index < 21) {
                    father_int_id = new_index;
                }
                else {
                    switch (new_index) {
                        case 21:
                            father_int_id = 42;
                            break;
                        case 22:
                            father_int_id = 43;
                            break;
                        case 23:
                            father_int_id = 44;
                            break;
                    }
                }

                API.callNative("SET_PED_HEAD_BLEND_DATA", father_ped, father_int_id, father_int_id, 0, father_int_id, father_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", mother_ped, mother_int_id, mother_int_id, 0, mother_int_id, mother_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", API.getLocalPlayer(), father_int_id,
                    mother_int_id, 0, father_int_id, mother_int_id, 0, parent_lean, parent_lean, 0, false);
            });

            mother_menu_item.OnListChanged.connect(function (sender, new_index) {
                if (new_index < 21) {
                    mother_int_id = new_index + 21;
                }
                else {
                    mother_int_id = 45;
                }

                API.callNative("SET_PED_HEAD_BLEND_DATA", father_ped, father_int_id, father_int_id, 0, father_int_id, father_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", mother_ped, mother_int_id, mother_int_id, 0, mother_int_id, mother_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", API.getLocalPlayer(), father_int_id,
                    mother_int_id, 0, father_int_id, mother_int_id, 0, parent_lean, parent_lean, 0, false);
            });

            parent_lean_menu_item.OnListChanged.connect(function (sender, new_index) {
                switch (new_index) {
                    case 0:
                        parent_lean = 0.01;
                        break;
                    case 1:
                        parent_lean = 0.1;
                        break;
                    case 2:
                        parent_lean = 0.2;
                        break;
                    case 3:
                        parent_lean = 0.3;
                        break;
                    case 4:
                        parent_lean = 0.4;
                        break;
                    case 5:
                        parent_lean = 0.5;
                        break;
                    case 6:
                        parent_lean = 0.6;
                        break;
                    case 7:
                        parent_lean = 0.7;
                        break;
                    case 8:
                        parent_lean = 0.8;
                        break;
                    case 9:
                        parent_lean = 0.9;
                        break;
                    case 10:
                        parent_lean = 1.01;
                        break;
                }

                API.callNative("SET_PED_HEAD_BLEND_DATA", father_ped, father_int_id, father_int_id, 0, father_int_id, father_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", mother_ped, mother_int_id, mother_int_id, 0, mother_int_id, mother_int_id, 0, API.f(1.0), API.f(1.0), 0, false);
                API.callNative("SET_PED_HEAD_BLEND_DATA", API.getLocalPlayer(), father_int_id,
                    mother_int_id, 0, father_int_id, mother_int_id, 0, parent_lean, parent_lean, 0, false);
            });


            //Finish creating the menu
            character_creation_menu = API.createMenu("Character Creation", "Parent Selection", 0, 0, 6);
            character_creation_menu.AddItem(gender_menu_item);
            character_creation_menu.AddItem(father_menu_item);
            character_creation_menu.AddItem(mother_menu_item);
            character_creation_menu.AddItem(parent_lean_menu_item);
            character_creation_menu.AddItem(next_menu_item);
            menu_pool.Add(character_creation_menu);

            character_creation_menu.Visible = true;

            character_creation_menu.OnItemSelect.connect(function (sender, item, index) {
                if (item.Text == "Next") {
                    API.triggerServerEvent("change_parent_info", father_ped, mother_ped, father_int_id, mother_int_id, parent_lean, gender);
                    next_character_creation_step(player, 1);
                }
            });

	        character_creation_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });
            break;
        }
        case 1: {
            API.triggerServerEvent("initiate_style_limits", gender);

            API.sendChatMessage("~g~Alright, now that we have figured out your parents let us see what your face looks like!");
            API.sendChatMessage("~g~NOTE: You can use the PLUS and MINUS keys to rotate your character!");

            player = API.getLocalPlayer();
            character_creation_menu.Visible = false;
            menu_pool = API.getMenuPool();

            API.setActiveCamera(facial_view);
            API.pointCameraAtEntityBone(facial_view, player, 65068, new Vector3(0, 0, 0));

            var hair_style = 0;
            var hair_color = 0;
            var facial_hair = 255; 
            var blemishes = 255;
            var eyebrows = 0;
            var ageing = 255;
            var makeup = 255;
            var makeup_color = 0;
            var blush = 255;
            var blush_color = 0;
            var complexion = 255;
            var sun_damage = 255;
            var lipstick = 255;
            var lipstick_color = 0;
            var moles_freckles = 255;

            var hair_style_list = new List(String);
            var hair_color_list = new List(String);
            var facial_hair_list = new List(String);
            var blemishes_list = new List(String);
            var eyebrow_list = new List(String);
            var ageing_list = new List(String);
            var makeup_list = new List(String);
            var makeup_color_list = new List(String);
            var blush_list = new List(String);
            var blush_color_list = new List(String);
            var lipstick_list = new List(String);
            var lipstick_color_list = new List(String);
            var complexion_list = new List(String);
            var sundamage_list = new List(String);
            var moles_freckles_list = new List(String);

            for (var i = 0; i < MAX_HAIR_STYLE; i++) {
                hair_style_list.Add((i + 1).toString());
            }
            for (var i = 0; i < MAX_HAIR_COLORS; i++) {
                hair_color_list.Add((i + 1).toString());
            }
            facial_hair_list.Add("None");
            for (var i = 0; i < 29; i++) {
                facial_hair_list.Add((i + 1).toString());
            }
            blemishes_list.Add("None");
            for (var i = 0; i < 24; i++) {
                blemishes_list.Add((i + 1).toString());
            }
            for (var i = 0; i < 34; i++) {
                eyebrow_list.Add((i + 1).toString());
            }
            ageing_list.Add("None");
            for (var i = 0; i < 15; i++) {
                ageing_list.Add((i + 1).toString());
            }
            makeup_list.Add("None");
            for (var i = 0; i < 75; i++) {
                makeup_list.Add((i + 1).toString());
            }
            for (var i = 0; i < MAX_MAKEUP_COLORS; i++) {
                makeup_color_list.Add((i + 1).toString());
            }
            blush_list.Add("None");
            for (var i = 0; i < 7; i++) {
                blush_list.Add((i + 1).toString());
            }
            for (var i = 0; i < MAX_BLUSH_COLORS; i++) {
                blush_color_list.Add((i + 1).toString());
            }
            blush_list.Add("None");
            for (var i = 0; i < 10; i++) {
                lipstick_list.Add((i + 1).toString());
            }
            for (var i = 0; i < MAX_LIPSTICK_COLORS; i++) {
                lipstick_color_list.Add((i + 1).toString());
            }
            complexion_list.Add("None");
            for (var i = 0; i < 12; i++) {
                complexion_list.Add((i + 1).toString());
            }
            sundamage_list.Add("None");
            for (var i = 0; i < 11; i++) {
                sundamage_list.Add((i + 1).toString());
            }
            moles_freckles_list.Add("None");
            for (var i = 0; i < 18; i++) {
                moles_freckles_list.Add((i + 1).toString());
            }

            var hair_style_menu_item = API.createListItem("Hair Style", "Select a hair style.", hair_style_list, 0);
            var hair_color_menu_item = API.createListItem("Hair Color", "Select a hair color.", hair_color_list, 0);
            var blemishes_menu_item = API.createListItem("Blemishes", "Select any blemishes style.", blemishes_list, 0);
            var eyebrow_menu_item = API.createListItem("Eyebrows", "Select an eyebrow style", eyebrow_list, 0);
            var ageing_menu_item = API.createListItem("Ageing", "Select any features of old age.", ageing_list, 0);
            var complexion_menu_item = API.createListItem("Complexion", "Select a style of complexion.", complexion_list, 0);
            var sundamage_menu_item = API.createListItem("Sundamage", "Select the type of sundamage you want.", sundamage_list, 0);
            var moles_freckles_menu_item = API.createListItem("Moles & Freckles", "Select a style of moles or freckles.", moles_freckles_list, 0);

            //Male only
            var facial_hair_menu_item = API.createListItem("Facial Hair", "Select a facial hair style.", facial_hair_list, 0);

            //Female only
            var makeup_menu_item = API.createListItem("Makeup", "Select a style of makeup.", makeup_list, 0);
            var makeup_color_menu_item = API.createListItem("Makeup Color", "Select a makeup color.", makeup_color_list, 0);
            var blush_menu_item = API.createListItem("Blush", "Select a style of blush.", blush_list, 0);
            var blush_color_menu_item = API.createListItem("Blush Color", "Select a blush color.", blush_list, 0);
            var lipstick_menu_item = API.createListItem("Lipstick", "Select a lipstick style.", lipstick_list, 0);
            var lipstick_color_menu_item = API.createListItem("Lipstick Color", "Select a lipstick color.", lipstick_color_list, 0);

            var next_menu_item = API.createMenuItem("Next", "Continue onto the next portion of character creation.");

            character_creation_menu = API.createMenu("Character Creation", "Facial Features", 0, 0, 6);
            character_creation_menu.AddItem(hair_style_menu_item);
            character_creation_menu.AddItem(hair_color_menu_item);

            if (gender == 0) {
                character_creation_menu.AddItem(facial_hair_menu_item);
            }
            else if(gender == 1) {
                character_creation_menu.AddItem(makeup_menu_item);
                character_creation_menu.AddItem(makeup_color_menu_item);
                character_creation_menu.AddItem(blush_menu_item);
                character_creation_menu.AddItem(blush_color_menu_item);
                character_creation_menu.AddItem(lipstick_menu_item);
                character_creation_menu.AddItem(lipstick_color_menu_item);
            }

            character_creation_menu.AddItem(blemishes_menu_item);
            character_creation_menu.AddItem(eyebrow_menu_item);
            character_creation_menu.AddItem(ageing_menu_item);
            character_creation_menu.AddItem(complexion_menu_item);
            character_creation_menu.AddItem(sundamage_menu_item);
            character_creation_menu.AddItem(moles_freckles_menu_item);
            character_creation_menu.AddItem(next_menu_item);
            menu_pool.Add(character_creation_menu);
            character_creation_menu.CurrentSelection = 0;
            character_creation_menu.Visible = true;

            character_creation_menu.OnListChange.connect(function (sender, list, new_index) {
                var playerHandle = API.getLocalPlayer();
                switch (list.Text) {
                    case "Hair Style":
                        hair_style = new_index;
                        API.setPlayerClothes(playerHandle, 2, hair_style, 0);
                        break;
                    case "Hair Color":
                        hair_color = new_index;
                        API.callNative("_SET_PED_HAIR_COLOR", playerHandle, hair_color);
                        break;
                    case "Blemishes":
                        if (new_index == 0)
                            blemishes = 255;
                        else blemishes = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 0, blemishes, API.f(1.0));
                        break;
                    case "Eyebrows":
                        eyebrows = new_index;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 2, eyebrows, API.f(1.0));
                        API.callNative("_SET_PED_HEAD_OVERLAY_COLOR", playerHandle, 2, 1, hair_color,
                            hair_color);
                        break;
                    case "Ageing":
                        if (new_index == 0)
                            ageing = 255;
                        else ageing = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 3, ageing, API.f(1.0));
                        break;
                    case "Complexion":
                        if (new_index == 0)
                            complexion = 255;
                        else complexion = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 6, complexion, API.f(1.0));
                        break;
                    case "Sundamage":
                        if (new_index == 0)
                            sun_damage = 255;
                        else sun_damage = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 7, sun_damage, API.f(1.0));
                        break;
                    case "Moles & Freckles":
                        if (new_index == 0)
                            moles_freckles = 255;
                        else moles_freckles = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 9, moles_freckles, API.f(1.0));
                        break;
                    case "Facial Hair":
                        if (new_index == 0)
                            facial_hair = 255;
                        else facial_hair = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 1, facial_hair, API.f(1.0));
                        API.callNative("_SET_PED_HEAD_OVERLAY_COLOR", playerHandle, 1, 1, hair_color,
                            hair_color);
                        break;
                    case "Makeup":
                        if (new_index == 0)
                            makeup = 255;
                        else makeup = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 4, makeup, API.f(1.0));
                        break;
                    case "Makeup Color":
                        if (new_index == 0)
                            makeup_color = 255;
                        else makeup_color = new_index - 1;
                        API.callNative("_SET_PED_HEAD_OVERLAY_COLOR", playerHandle, 4, 0, makeup_color,
                            makeup_color);
                        break;
                    case "Blush":
                        if (new_index == 0)
                            blush = 255;
                        else blush = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 5, blush, API.f(1.0));
                        break;
                    case "Blush Color":
                        if (new_index == 0)
                            blush_color = 255;
                        else blush_color = new_index - 1;
                        API.callNative("_SET_PED_HEAD_OVERLAY_COLOR", playerHandle, 5, 2, blush_color,
                            blush_color);
                        break;
                    case "Lipstick":
                        if (new_index == 0)
                            lipstick = 255;
                        else lipstick = new_index - 1;
                        API.callNative("SET_PED_HEAD_OVERLAY", playerHandle, 8, lipstick, API.f(1.0));
                        break;
                    case "Lipstick Color":
                        if (new_index == 0)
                            lipstick_color = 255;
                        else lipstick_color = new_index - 1;
                        API.callNative("_SET_PED_HEAD_OVERLAY_COLOR", playerHandle, 8, 2, lipstick_color,
                            lipstick_color);
                        break;
                }
            });

            character_creation_menu.OnItemSelect.connect(function (sender, item, index) {
                if (item.Text == "Next") {
                    //Save em.
                    API.triggerServerEvent("change_facial_features", hair_style, hair_color, blemishes, facial_hair, eyebrows, ageing, makeup, makeup_color, blush, blush_color, complexion, sun_damage, lipstick, lipstick_color, moles_freckles);

                    API.sendChatMessage("~o~Creating menus... Please wait, this may take a second!");
                    next_character_creation_step(player, 2);
                }
            });

	        character_creation_menu.OnMenuClose.connect((menu) => {
		        next_character_creation_step(player, 0);
	        });
            break;
        }
        case 2: {
            player = API.getLocalPlayer();
            character_creation_menu.Visible = false;
            menu_pool = API.getMenuPool();

            API.sendChatMessage("~o~Menus created!");

            API.sendChatMessage("~g~Now you can choose what clothes you wear! You can always buy more outfits at a clothing store.");
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
			torso_menu = API.createMenu("Character Creation", "Torso Selection", 0, 0, 6);

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

            var pants_index = 0;
            var pants_variation = 0;
            var shoe_index = 0;
            var shoe_variation = 0;
            var accessory_index = 0;
            var accessory_variation = 0;
            var undershirt_index = 0;
            var undershirt_variation = 0;
            var top_index = 0;
            var top_variation = 0;
            var hat_index = 0;
            var hat_variation = 0;
            var glasses_index = 0;
            var glasses_variation = 0;
            var ear_index = 0;
            var ear_variation = 0;

			var torso_index = 0;
			var torso_variation = 0;

            character_creation_menu = API.createMenu("Character Creation", "Outfit Selection", 0, 0, 6);
            character_creation_menu.AddItem(API.createMenuItem("Pants", "View the available pants"));
            character_creation_menu.AddItem(API.createMenuItem("Shoes", "View the available shoes"));
            character_creation_menu.AddItem(API.createMenuItem("Accessories", "View the available accesories. These may require not having an undershirt or top to see."));
            character_creation_menu.AddItem(API.createMenuItem("Tops", "View the available tops. Some of these require an undershirt."));
            character_creation_menu.AddItem(API.createMenuItem("Undershirts", "View the available undershirts. These may require a certain top to look correct."));
            character_creation_menu.AddItem(API.createMenuItem("Hats", "View the available hats"));
            character_creation_menu.AddItem(API.createMenuItem("Glasses", "View the available glasses"));
            character_creation_menu.AddItem(API.createMenuItem("Ear Accessories", "View the available ear accessories"));
	        character_creation_menu.AddItem(API.createMenuItem("Torsos", "View the available torsos."));
            character_creation_menu.AddItem(API.createMenuItem("Next", "Continue to the final step of character creation."));
            menu_pool.Add(character_creation_menu);
            character_creation_menu.Visible = true;

            pant_menu.OnIndexChange.connect(function (sender, index) {
                pants_index = index;
                pants_variation = 0;
                updateClothes(4, pants_index, pants_variation);
            });

            pant_menu.OnListChange.connect(function (sender, list, index) {
                pants_variation = index;
                updateClothes(4, pants_index, pants_variation);
            });

	        pant_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            shoe_menu.OnIndexChange.connect(function (sender, index) {
                shoe_index = index;
                shoe_variation = 0;
                updateClothes(6, shoe_index, shoe_variation);
            });

            shoe_menu.OnListChange.connect(function (sender, list, index) {
                shoe_variation = index;
                updateClothes(6, shoe_index, shoe_variation);
            });

	        shoe_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            accessory_menu.OnIndexChange.connect(function (sender, index) {
                accessory_index = index;
                accessory_variation = 0;
                updateClothes(7, accessory_index, accessory_variation);
            });

            accessory_menu.OnListChange.connect(function (sender, list, index) {
                accessory_variation = index;
                updateClothes(7, accessory_index, accessory_variation);
            });

	        accessory_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            undershirt_menu.OnIndexChange.connect(function (sender, index) {
                undershirt_index = index;
                undershirt_variation = 0;
                updateClothes(8, undershirt_index, undershirt_variation);
            });

            undershirt_menu.OnListChange.connect(function (sender, list, index) {
                undershirt_variation = index;
                updateClothes(8, undershirt_index, undershirt_variation);
            });

	        undershirt_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            top_menu.OnIndexChange.connect(function (sender, index) {
                top_index = index;
                top_variation = 0;
                updateClothes(11, top_index, top_variation);
            });

            top_menu.OnListChange.connect(function (sender, list, index) {
                top_variation = index;
                updateClothes(11, top_index, top_variation);
            });

	        top_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            hat_menu.OnIndexChange.connect(function (sender, index) {
                hat_index = index;
                hat_variation = 0;
                updateClothes(20, hat_index, hat_variation);
            });

            hat_menu.OnListChange.connect(function (sender, list, index) {
                hat_variation = index;
                updateClothes(20, hat_index, hat_variation);
            });

	        hat_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            glasses_menu.OnIndexChange.connect(function (sender, index) {
                glasses_index = index;
                glasses_variation = 0;
                updateClothes(21, glasses_index, glasses_variation);
            });

            glasses_menu.OnListChange.connect(function (sender, list, index) {
                glasses_variation = index;
                updateClothes(21, glasses_index, glasses_variation);
            });

	        glasses_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            ear_menu.OnIndexChange.connect(function (sender, index) {
                ear_index = index;
                ear_variation = 0;
                updateClothes(22, ear_index, ear_variation);
            });

            ear_menu.OnListChange.connect(function (sender, list, index) {
                ear_variation = index;
                updateClothes(22, ear_index, ear_variation);
            });

	        ear_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

	        torso_menu.OnIndexChange.connect(function (sender, index) {
		        torso_index = index;
		        torso_variation = 0;
		        updateClothes(3, torso_index, torso_variation);
	        });

	        torso_menu.OnListChange.connect(function (sender, list, index) {
		        torso_variation = index;
		        updateClothes(3, torso_index, torso_variation);
	        });

	        torso_menu.OnMenuClose.connect((menu) => {
		        character_creation_menu.Visible = true;
	        });

            character_creation_menu.OnItemSelect.connect(function (sender, item, index) {
                character_creation_menu.Visible = false;
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
					case "Torsos":
						API.setActiveCamera(creation_view);
						torso_menu.Visible = true;
						torso_menu.CurrentSelection = 0;
						break;
                    case "Next":
                        //Save clothes.
                        API.triggerServerEvent("change_clothes", pants_index, pants_variation, shoe_index, shoe_variation, accessory_index, accessory_variation, undershirt_index, undershirt_variation, top_index, top_variation, hat_index, hat_variation, glasses_index, glasses_variation, ear_index, ear_variation, torso_index, top_variation);

                        next_character_creation_step(player, 3);
                        break;
                }
            });

	        character_creation_menu.OnMenuClose.connect((menu) => {
		        next_character_creation_step(player, 1);
	        });
            break;
        }
        case 3: {
            player = API.getLocalPlayer();
            character_creation_menu.Visible = false;
            menu_pool = API.getMenuPool();

            API.sendChatMessage("~y~Lastly, we need some basic information about your character!");

            var age_list = new List(String);

            for(var i = 18; i <= 80; i++){
                age_list.Add((i).toString());
            }

            var spawn_list = new List(String);
            //spawn_list.Add("Los Santos Airport");
            spawn_list.Add("Dashound Bus Station");

            var age_menu = API.createListItem("Age", "Select a reasonable age for your character. This should match their appreance.", age_list, 0);
            var birthday_menu = API.createMenuItem("Birthday", "Input a birthday for your character. This should follow the format: DD/MM");
            var birthplace_menu = API.createMenuItem("Birthplace", "Input your characters birthplace. This should follow the format: City, Country");
            var spawn_menu = API.createListItem("Spawn Location", "Select where you would like to start your journey.", spawn_list, 0);

            character_creation_menu = API.createMenu("Character Creation", "Basic Character Information", 0, 0, 6);

            character_creation_menu.AddItem(age_menu);
            character_creation_menu.AddItem(birthday_menu);
            character_creation_menu.AddItem(birthplace_menu);
            character_creation_menu.AddItem(spawn_menu);
            character_creation_menu.AddItem(API.createMenuItem("Finish Character Creation", "Finish character creation and spawn. This cannot be undone."));
            menu_pool.Add(character_creation_menu);
            character_creation_menu.Visible = true;

            var spawn_point = 0;
            var age = 18;
            var birthday = "01/01";
            var birthplace = "Los Santos, USA";

            birthday_menu.SetRightLabel(birthday);
            birthplace_menu.SetRightLabel(birthplace);

            character_creation_menu.OnItemSelect.connect(function (sender, item, index) {
                switch(item.Text){
                    case "Birthday":
                        birthday = API.getUserInput("DD/MM", 5);

                        var patt = new RegExp("([0-9]+)\/([0-9]+)");
                        var res = patt.exec(birthday);
                        if (res == null) {
                            birthday = "01/01";
                            API.sendChatMessage("Wrong age format.");
                        } else {
                            if (parseInt(res[1]) > 31 || parseInt(res[2]) > 12) {
                                birthday = "01/01";
                                API.sendChatMessage("Days must be less than 31 and month must be less than 12.");
                            }
                        }

                        birthday_menu.SetRightLabel(birthday);
                        break;
                    case "Birthplace":
                        birthplace = API.getUserInput("Los Santos, USA", 32);
                        birthplace_menu.SetRightLabel(birthplace);
                        break;
                    case "Finish Character Creation":
                        character_creation_menu.Visible = false;
                        API.triggerServerEvent("finish_character_creation", age, birthday, birthplace, spawn_point);
                        menu_pool = null;
                        API.setActiveCamera(null);
                        break;
                }
            });

            age_menu.OnListChanged.connect(function (sender, new_index) {
                age = new_index + 19;
            });

            spawn_menu.OnListChanged.connect(function (sender, new_index) {
                spawn_point = new_index;
            });

	        character_creation_menu.OnMenuClose.connect((menu) => {
		        next_character_creation_step(player, 2);
	        });
        }
    }
}

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

var rotating = 0;
API.onKeyDown.connect(function(sender, e) {
    if (e.KeyCode === Keys.Enter) {
        if (pant_menu != null) {
            if (pant_menu.Visible == true) {
                pant_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (shoe_menu.Visible == true) {
                shoe_menu.Visible = false;
                character_creation_menu.Visible = true;
            }

            if (accessory_menu.Visible == true) {
                accessory_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (undershirt_menu.Visible == true) {
                undershirt_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (top_menu.Visible == true) {
                top_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (hat_menu.Visible == true) {
                hat_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (glasses_menu.Visible == true) {
                glasses_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
            if (ear_menu.Visible == true) {
                ear_menu.Visible = false;
                character_creation_menu.Visible = true;
            }
	        if (torso_menu.Visible == true) {
		        torso_menu.Visible = false;
		        character_creation_menu.Visible = true;
	        }
        }
    } else if (e.KeyCode == Keys.Oemplus) {
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
    if (rotating != 0 && menu_pool !== null) {
        var player = API.getLocalPlayer();
        var new_rot = API.getEntityRotation(player).Add(new Vector3(0, 0, rotating));
        API.setEntityRotation(player, new_rot);
    }
});