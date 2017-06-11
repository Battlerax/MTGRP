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
					  API.sendNotification("~r~Enter your reason for request.");
					  API.sendChatMessage("~r~Enter your reason for request.");
                      var message = API.getUserInput("", 200);
					  if(message.length == 0){
						API.sendNotification("~r~Please enter a valid reason for your request.");
						API.sendChatMessage("~r~Please enter a valid reason for your request.");
						return;
					  }
                      API.triggerServerEvent("OnRequestSubmitted", message);
                    }

                    else if (selectedItem.Text == "Report a Player"){
					  report_menu.Visible = false;
                      report_menu = null;
                      menu_pool = null;
					  API.sendNotification("~r~Enter the name or ID of the player you want to report.");
					  API.sendChatMessage("~r~Enter the name or ID of the player you want to report.");
                      var targetPlayer = API.getUserInput("",65);
					  if(targetPlayer.length == 0){
						API.sendNotification("~r~Please enter a valid name or ID of the player you want to report.");
						API.sendChatMessage("~r~Please enter a valid name or ID of the player you want to report.");
						return;
					  }

					  API.sendNotification("~r~Enter your report reason.");
					  API.sendChatMessage("~r~Enter your report reason.");
                      var message = API.getUserInput("", 200);
					  if(message.length == 0){
						API.sendNotification("~r~Please enter a valid report reason.");
						API.sendChatMessage("~r~Please enter a valid report reason.");
						return;
					  }
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
