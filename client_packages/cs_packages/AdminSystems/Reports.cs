using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using RAGE;
using RAGE.NUI;

namespace cs_packages.AdminSystems
{
    public class Reports : Events.Script
    {
        private UIMenu report_menu;
        private MenuPool _menuPool;

        public Reports()
        {
            _menuPool = new MenuPool();
            report_menu = new UIMenu("Report Menu", "Request assistance or report a player.", new Point(50, 50));
            report_menu.AddItem(new UIMenuItem("Request Assistance", "Request administrator assistance."));
            report_menu.AddItem(new UIMenuItem("Report a Player", "Report a player for breaking the server rules."));
            report_menu.AddItem(new UIMenuItem("Close", "Close"));
            report_menu.FreezeAllInput = true;
            _menuPool.Add(report_menu);
            report_menu.OnItemSelect += Report_menu_OnItemSelect;

            Events.Add("show_report_menu", ShowReportMenuEvent);
            Events.Add("getwaypoint", GetWaypointEvent);
            Events.Add("GET_CP_TO_SEND", GetCPToSend);

            Events.Tick+=Tick;
        }

        private void Tick(List<Events.TickNametagData> nametags)
        {
            _menuPool.ProcessMenus();
        }

        private void GetCPToSend(object[] args)
        {
            // TODO: there's no getwaypoint.
        }

        private void GetWaypointEvent(object[] args)
        {
            // TODO: there's no getwaypoint.
        }

        private void ShowReportMenuEvent(object[] args)
        {
            report_menu.Visible = true;
        }

        private void Report_menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem.Text == "Close")
            {
                report_menu.Visible = false;
            }
            else if (selectedItem.Text == "Request Assistance")
            {
                report_menu.Visible = false;
                Main.Notify("~r~Enter your reason for request.");
                var message = Main.GetUserInput("", 200);
                if (message.Length == 0)
                {
                    Main.Notify("~r~Please enter a valid reason for your request.");
                    return;
                }
                Events.CallRemote("OnRequestSubmitted", message);
            }

            else if (selectedItem.Text == "Report a Player")
            {
                report_menu.Visible = false;
                Main.Notify("~r~Enter the name or ID of the player you want to report.");
                var targetPlayer = Main.GetUserInput("", 65);
                if (targetPlayer.Length == 0)
                {
                    Main.Notify("~r~Please enter a valid name or ID of the player you want to report.");
                    return;
                }

                Main.Notify("~r~Enter your report reason.");
                var nmessage = Main.GetUserInput("", 200);
                if (nmessage.Length == 0)
                {
                    Main.Notify("~r~Please enter a valid report reason.");
                    return;
                }
                Events.CallRemote("OnReportMade", nmessage, targetPlayer);
            }
        }
    }
}
