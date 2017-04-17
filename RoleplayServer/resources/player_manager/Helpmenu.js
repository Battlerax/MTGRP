var menu = new Menu();
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
API.onUpdate.connect(function () {
    if (menu.menuPool !== null) {
        menu.menuPool.ProcessMenus();
    }
});

function Menu() {
    this.mainMenu = null;
    this.menuPool = API.getMenuPool();

    this.createMenu = function (title, subTitle, isResetKey, posX, posY, anchor) {

        this.destroyMenu();

        title = typeof title !== "undefined" ? title : "Menu";
        subTitle = typeof subTitle !== "undefined" ? subTitle : "";
        isResetKey = typeof isResetKey !== "undefined" ? isResetKey : false;
        anchor = typeof anchor !== "undefined" ? anchor : 6;
        posX = typeof posX !== "undefined" ? posX : 0;
        posY = typeof posY !== "undefined" ? posY : 0;

        this.mainMenu = API.createMenu(title, subTitle, posX, posY, anchor);
        this.menuPool.Add(this.mainMenu);
        this.mainMenu.Visible = true;

        if (isResetKey === true) {
            this.mainMenu.ResetKey(menuControl.Back);
        }

        return this;
    };

    this.addMenuItem = function (title, subTitle, isActivateInvisibleMenu, isActivated, callFunction) {

        title = typeof title !== "undefined" ? title : "MenuItem";
        subTitle = typeof subTitle !== "undefined" ? subTitle : "";
        isActivateInvisibleMenu = typeof isActivateInvisibleMenu !== "undefined" ? isActivateInvisibleMenu : true;
        isActivated = typeof isActivated !== "undefined" ? isActivated : false;
        callFunction = typeof callFunction !== "undefined" ? callFunction : "";

        var args = [].slice.call(arguments).splice(5);
        var uiMenu = this.mainMenu;
        var menuItem = API.createMenuItem(title, subTitle);

        if (isActivated === true) {
            menuItem.Activated.connect(function (menu, item) {

                if (isActivateInvisibleMenu === true)
                    uiMenu.Visible = false;

                eval(callFunction)(args);
            });
        }

        this.mainMenu.AddItem(menuItem);

        return this;
    };

    this.addCheckBoxItem = function (title, subTitle, defaultValue, isActivateInvisibleMenu, isActivated, callFunction) {

        title = typeof title !== "undefined" ? title : "CheckBoxItem";
        subTitle = typeof subTitle !== "undefined" ? subTitle : "";
        defaultValue = typeof defaultValue !== "undefined" ? defaultValue : true;
        isActivateInvisibleMenu = typeof isActivateInvisibleMenu !== "undefined" ? isActivateInvisibleMenu : true;
        isActivated = typeof isActivated !== "undefined" ? isActivated : false;
        callFunction = typeof callFunction !== "undefined" ? callFunction : "";

        var args = [].slice.call(arguments).splice(6);
        var checkBoxItem = API.createCheckboxItem(title, subTitle, defaultValue);
        var uiMenu = this.mainMenu;

        if (isActivated === true) {
            checkBoxItem.CheckboxEvent.connect(function (item, callBack) {
                if (isActivateInvisibleMenu === true)
                    uiMenu.Visible = false;

                eval(callFunction)(callBack, args);
            });
        }

        this.mainMenu.AddItem(checkBoxItem);

        return this;
    };

    this.addCloseButton = function () {

        var uiMenu = this.mainMenu;
        var destroyMenu = this.destroyMenu();
        var menuItem = API.createMenuItem("~r~Close", "");

        menuItem.Activated.connect(function (menu, item) {
            uiMenu.Visible = false;
            destroyMenu();
        });
        this.mainMenu.AddItem(menuItem);

        return true;
    };

    this.callClientFunction = function (functionName /*, args */) {
        var args = [].slice.call(arguments).splice(1);
        eval(functionName)(args);
    };

    this.callServerFunction = function (functionName /*, args */) {
        var args = [].slice.call(arguments).splice(1);
        API.triggerServerEvent(functionName, args);
    };

    this.destroyMenu = function () {
        if (this.mainMenu !== null)
            this.mainMenu.Visible = false;

        this.mainMenu = null;
        this.menuPool = null;
        this.menuPool = API.getMenuPool();

        return true;
    };
}
