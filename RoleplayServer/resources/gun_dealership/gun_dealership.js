let menuPool = null;

menuPool = API.getMenuPool();

let pistolsmenu = API.createMenu("Ammu-Nation", 0, 0, 6);
let meleemenu = API.createMenu("Ammu-Nation", 0, 0, 6);

//Pistols
let pistols1 = API.createMenuItem("Pistol", "");pistols1.SetRightLabel("4000$");
let pistols2 = API.createMenuItem("Combat Pistol", "");pistols2.SetRightLabel("4700$");
let pistols3 = API.createMenuItem(".50 Pistol", ""); pistols3.SetRightLabel("6500$");
let pistols4 = API.createMenuItem("Revolver", ""); pistols4.SetRightLabel("8000$");
pistolsmenu.AddItem(pistols1);
pistolsmenu.AddItem(pistols2);
pistolsmenu.AddItem(pistols3);
pistolsmenu.AddItem(pistols4);

//Melee
let melee1 = API.createMenuItem("Bat", "");melee1.SetRightLabel("40$");
meleemenu.AddItem(melee1);
// main menu
let mainmenu = API.createMenu("Ammu-Nation", 0, 0, 6);

//main menu items
let melee = API.createMenuItem("Melee", "");
let pistols = API.createMenuItem("Pistols", "");

mainmenu.AddItem(pistols);
mainmenu.AddItem(melee)
menuPool.Add(mainmenu);
menuPool.Add(pistolsmenu);
menuPool.Add(meleemenu);

API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "openmenu": {
			mainmenu.Visible = true;
			API.showCursor(false);
            break;
        }

		case "closemenu": {
			mainmenu.Visible = false;
			pistolsmenu.Visible = false;
			meleemenu.Visible = false;
			break;
		}
    }
});

API.onUpdate.connect(function () {
    if (menuPool != null) {
        menuPool.ProcessMenus();
    }
});

pistols.Activated.connect(function(menu,item) {


    mainmenu.Visible = false;
    pistolsmenu.Visible = true;
    API.showCursor(false);

});

melee.Activated.connect(function(menu,item) {


    mainmenu.Visible = false;
    meleemenu.Visible = true;
    API.showCursor(false);

});
//Melee

melee1.Activated.connect(function (menu, item) {

    API.triggerServerEvent("clickeditem", "Bat", 40);
    API.sendNotification("You swipe your credit card to buy a ~g~Pistol");
});

// pistols event
pistols1.Activated.connect(function (menu, item) {

    API.triggerServerEvent("clickeditem", "Pistol", 4000);
    API.sendNotification("You swipe your credit card to buy a ~g~Pistol");
});

pistols2.Activated.connect(function (menu, item) {

    API.triggerServerEvent("clickeditem", "CombatPistol", 4700);
    API.sendNotification("You swipe your credit card to buy a ~g~Combat Pistol");
});

pistols3.Activated.connect(function (menu, item) {

    API.triggerServerEvent("clickeditem", "HeavyPistol", 6500);
    API.sendNotification("You swipe your credit card to buy a ~g~.50 Pistol");
});

pistols4.Activated.connect(function (menu, item) {

    API.triggerServerEvent("clickeditem", "Revolver", 8000);
    API.sendNotification("You swipe your credit card to buy a ~g~Revolver");
});