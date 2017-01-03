var menu_pool = null;
var character_menu = null;

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

        character_menu.OnItemSelect.connect(function (sender, item, index) {
            if (item.Text == "Create new character") {
                var desired_name = API.getUserInput("Enter desired username here", 64);
                API.triggerServerEvent("OnCharacterMenuSelect", item.Text, desired_name);
            }
            else {
                API.triggerServerEvent("OnCharacterMenuSelect", item.Text);
            }
           
        });
    }
    else if (eventName == "login_finished") {
        character_menu.Visible = false;
    }
});


API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});