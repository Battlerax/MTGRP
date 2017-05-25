var report_menu = null;
var menu_pool = null;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "show_report_menu":
            {
                menu_pool = API.getMenuPool();
                report_menu = API.createMenu("Report Menu", "Request assistance or report a player.", 0, 0, 3);
                report_menu.AddItem(API.createMenuItem("Request Assistance", "Request administrator assistance."));
                report_menu.AddItem(API.createMenuItem("Report a Player", "Report a player for breaking the server rules."));
                report_menu.AddItem(API.createMenuItem("Close", "Close"));
                report_menu.Visible = true;
                menu_pool.Add(report_menu);
                report_menu.OnItemSelect.connect(function (sender, selectedItem, index) {
                    if (selectedItem.Text == "Close") {
						report_menu.Visible = false;
                        report_menu = null;
                        menu_pool = null;
                    }
                    else if (selectedItem.Text == "Request Assistance"){
					  report_menu.Visible = false;
                      report_menu = null;
                      menu_pool = null;
                      var message = API.getUserInput("What do you need help with?", 200);
                      API.triggerServerEvent("OnRequestSubmitted", message);
                    }

                    else if (selectedItem.Text == "Report a Player"){
					  report_menu.Visible = false;
                      report_menu = null;
                      menu_pool = null;
                      var targetPlayer = API.getUserInput("Enter the name or ID of a player breaking the rules..", 65);
                      var message = API.getUserInput("What is your report reason?", 200);
                      API.triggerServerEvent("OnReportMade", message, targetPlayer);
                    }

                });
    }
  }
});

API.onUpdate.connect(function () {
    if (menu_pool != null) {
        menu_pool.ProcessMenus();
    }
});
