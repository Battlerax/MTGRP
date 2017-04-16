/*let menuPool = null;

API.onUpdate.connect(function () {
    if (menuPool != null) {
        menuPool.ProcessMenus();
    }
});

API.onChatCommand.connect(function (msg) {
    if (msg == "/help") {
        if (menupool == null) {
            menu_pool = API.getMenuPool();
            let menu = API.createMenu("help", " ", 0, 0, 6);
            let item1 = API.createMenuItem("Commands", "");
            let item2 = API.createMenuItem("Police", "");
            let item3 = API.createMenuItem("Admin", "");
            let item4 = API.createMenuItem("Rules", "");
            let item5 = API.createMenuItem("FAQ", "");
            menu.AddItem(item1);
            menu.AddItem(item2);
            menu.AddItem(item3);
            menu.AddItem(item4);
            menu.AddItem(item5);
            menuPool.Add(menu);
            menu.Visible = true;
        }
    }
});
*/
//**************************************
//*************[Example]****************
//**************************************

var menu = new Menu();
define
function openHelpMenu() {
    menu
        .createMenu("Help Menu")
        .addMenuItem("Commands", "", true, true, "callServerTrigger", "Commands1")
        .addMenuItem("Animations", "", true, true, "callServerTrigger", "Animations1")
        .addMenuItem("Police", "", true, true, "callServerTrigger", "Police1")
        .addMenuItem("Admin", "", true, true, "callServerTrigger", "Admin1")
        .addMenuItem("Rules", "", true, true, "callServerTrigger", "Rules1")
        .addMenuItem("FAQ", "", true, true, "callServerTrigger", "FAQ1")
        .addCloseButton()
    ;
}

function callServerTrigger(eventname) {
    var args = [].slice.call(arguments).splice(1);
    menu.callServerFunction(eventname, args);
}