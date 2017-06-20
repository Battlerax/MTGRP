var menu_pool = null;
var bank_menu = null;

var withdraw_index = 0;
var deposit_index = 0;

API.onServerEventTrigger.connect(function(eventName, args){
    if(eventName === "openATM") {
        if (bank_menu == null || bank_menu.Visible == false) {
            var player = API.getLocalPlayer();

            if (API.isPlayerInAnyVehicle(player) == true)
                return;

            menu_pool = API.getMenuPool();
            withdraw_index = 0;
            deposit_index = 0;
            var withdraw_item = null;
            var deposit_item = null;
            bank_menu = API.createMenu("ATM machine", "Bank of Los Santos", 0, 0, 3);

            var withdraw_list = new List(String);
            withdraw_list.Add("$100");
            withdraw_list.Add("$200");
            withdraw_list.Add("$300");
            withdraw_list.Add("$400");
            withdraw_list.Add("$500");
            withdraw_list.Add("$600");
            withdraw_list.Add("$700");
            withdraw_list.Add("$800");
            withdraw_list.Add("$900");
            withdraw_list.Add("$1000");
            withdraw_list.Add("$2000");
            withdraw_list.Add("$3000");
            withdraw_list.Add("$4000");
            withdraw_list.Add("$5000");
            withdraw_list.Add("$6000");
            withdraw_list.Add("$7000");
            withdraw_list.Add("$8000");
            withdraw_list.Add("$9000");
            withdraw_list.Add("$10000");
            withdraw_list.Add("$20000");
            withdraw_list.Add("$30000");
            withdraw_list.Add("$40000");
            withdraw_list.Add("$50000");
            withdraw_list.Add("$60000");
            withdraw_list.Add("$70000");
            withdraw_list.Add("$80000");
            withdraw_list.Add("$90000");
            withdraw_list.Add("$100000");
            withdraw_list.Add("All");
            withdraw_item = API.createListItem("Withdraw cash", "Withdraw cash from your bank account.", withdraw_list, 0);

            var deposit_list = new List(String);
            deposit_list.Add("$100");
            deposit_list.Add("$200");
            deposit_list.Add("$300");
            deposit_list.Add("$400");
            deposit_list.Add("$500");
            deposit_list.Add("$600");
            deposit_list.Add("$700");
            deposit_list.Add("$800");
            deposit_list.Add("$900");
            deposit_list.Add("$1000");
            deposit_list.Add("$2000");
            deposit_list.Add("$3000");
            deposit_list.Add("$4000");
            deposit_list.Add("$5000");
            deposit_list.Add("$6000");
            deposit_list.Add("$7000");
            deposit_list.Add("$8000");
            deposit_list.Add("$9000");
            deposit_list.Add("$10000");
            deposit_list.Add("$20000");
            deposit_list.Add("$30000");
            deposit_list.Add("$40000");
            deposit_list.Add("$50000");
            deposit_list.Add("$60000");
            deposit_list.Add("$70000");
            deposit_list.Add("$80000");
            deposit_list.Add("$90000");
            deposit_list.Add("$100000");
            deposit_list.Add("All");
            deposit_item = API.createListItem("Deposit cash", "Deposit cash into your bank account.", deposit_list, 0);

            bank_menu.AddItem(withdraw_item);
            bank_menu.AddItem(deposit_item);

            menu_pool.Add(bank_menu);
            bank_menu.Visible = true;

            withdraw_item.Activated.connect(function (menu, item) {
                API.triggerServerEvent("OnBankMenuTrigger", "Withdraw cash", withdraw_index);
            });

            deposit_item.Activated.connect(function (menu, item) {
                API.triggerServerEvent("OnBankMenuTrigger", "Deposit cash", deposit_index);
            });

            withdraw_item.OnListChanged.connect(function (sender, new_index) {
                withdraw_index = new_index;
            });

            deposit_item.OnListChanged.connect(function (sender, new_index) {
                deposit_index = new_index;
            });

        }
        else {
            bank_menu.Visible = false;
        }

    }

});

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});