"use strict";
var lspd_menu = null;
var menu_pool = null;
Event.OnServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "show_lspd_locker":
            {
                menu_pool = API.getMenuPool();
                lspd_menu = API.createMenu("LSPD Locker", "Locker Options", 0, 0, 3);
                lspd_menu.AddItem(API.createMenuItem("Change Clothes", "Change into and out of your LSPD uniform."));
                lspd_menu.AddItem(API.createMenuItem("Toggle Duty", "Go on and off duty."));
                lspd_menu.AddItem(API.createMenuItem("Equip Standard Equipment", "Withdraw standard weapons and armour from the LSPD locker."));
                lspd_menu.AddItem(API.createMenuItem("Equip SWAT Equipment", "Equip standard SWAT equipment and armour."));
                lspd_menu.AddItem(API.createMenuItem("Close Menu", "Close the locker room menu."));
                lspd_menu.Visible = true;
                menu_pool.Add(lspd_menu);
                lspd_menu.OnItemSelect.connect(function (sender, selectedItem, index) {
                    if (selectedItem.Text != "Close Menu") {
                        API.triggerServerEvent("LSPD_Menu_" + selectedItem.Text.replace(new RegExp(" ", "g"), "_"));
                    }
                    else {
                        lspd_menu.Visible = false;
                        lspd_menu = null;
                        menu_pool = null;
                    }
                });
                lspd_menu.OnMenuClose.connect(function () {
                    lspd_menu = null;
                    menu_pool = null;
                });
                break;
            }
    }
});
Event.OnUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});
