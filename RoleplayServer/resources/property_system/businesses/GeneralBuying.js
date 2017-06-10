var menu = null;

API.onServerEventTrigger.connect((eventName, args) => {
	switch (eventName) {
		case "property_buy":
			menu = API.createMenu(args[1], args[2], 0, 0, 6);
			API.setMenuBannerRectangle(menu, 255, 60, 60, 255);

			var items = JSON.parse(args[0]);
			for (var i = 0; i < items.length; i++) {
				menu.AddItem(API.createMenuItem(items[i][1] + `	~g~(\$${items[i][3]})`, items[i][2]));
			}

			menu.Visible = true;

			menu.OnItemSelect.connect((sender, item, index) => {
				API.triggerServerEvent("property_buyitem", items[index][0]);
			});

			menu.OnMenuClose.connect((menu) => {
				API.triggerServerEvent("property_exitbuy");
				menu = null;
			});
			break;
	}
});

API.onUpdate.connect(function() {
	if(menu !== null)
		API.drawMenu(menu);
});