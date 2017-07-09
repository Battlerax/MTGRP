using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTANetworkServer;
using GTANetworkShared;
using System.Threading;


public class FreeCam : Script
{
    string barry;
    public FreeCam()
	{
        API.onChatMessage += OnPlayerChat;
        API.onResourceStart += OnResourceStartHandler;
        API.onResourceStop += OnResourceStopHandler;
        API.onPlayerConnected += OnPlayerConnectedHandler;

    }

    public void OnResourceStartHandler()
    {     

        API.consoleOutput("Main cs started..");
        API.requestIpl("apa_v_inttest");
        API.requestIpl("apa_v_mp_h_01_a");
        API.requestIpl("apa_v_mp_h_01_b");
        API.requestIpl("apa_v_mp_h_01_c");

        API.requestIpl("apa_v_mp_h_02_a");
        API.requestIpl("apa_v_mp_h_02_b");
        API.requestIpl("apa_v_mp_h_02_c");

        API.requestIpl("apa_v_mp_h_03_a");
        API.requestIpl("apa_v_mp_h_03_b");
        API.requestIpl("apa_v_mp_h_03_c");

        API.requestIpl("apa_v_mp_h_04_a");
        API.requestIpl("apa_v_mp_h_04_b");
        API.requestIpl("apa_v_mp_h_04_c");

        API.requestIpl("apa_v_mp_h_05_a");
        API.requestIpl("apa_v_mp_h_05_b");
        API.requestIpl("apa_v_mp_h_05_c");

        API.requestIpl("apa_v_mp_h_06_a");
        API.requestIpl("apa_v_mp_h_06_b");
        API.requestIpl("apa_v_mp_h_06_c");

        API.requestIpl("apa_v_mp_h_07_a");
        API.requestIpl("apa_v_mp_h_07_b");
        API.requestIpl("apa_v_mp_h_07_c");

        API.requestIpl("apa_v_mp_h_08_a");
        API.requestIpl("apa_v_mp_h_08_b");
        API.requestIpl("apa_v_mp_h_08_c");

        API.requestIpl("bkr_biker_interior_placement");
        API.requestIpl("bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo_");
        API.requestIpl("bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo_");

        API.requestIpl("bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo_");
        API.requestIpl("bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo_");

        API.requestIpl("bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo_");
        API.requestIpl("bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo_");
        API.requestIpl("bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo_");
    }
    public void OnResourceStopHandler()
    {
        API.consoleOutput("Main cs shuttedown"); 
    }
    string GetLine(string fileName, int line)
    {
        using (var sr = new StreamReader(fileName))
        {
            for (int i = 1; i < line; i++)
                sr.ReadLine();
            return sr.ReadLine();
        }
    }
    static void WriteLine(string newText, string fileName, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        arrLine[line_to_edit] = newText;
        File.WriteAllLines(fileName, arrLine);
    }
    private void OnPlayerConnectedHandler(Client player)
    {

    }

    #region CloseMSGFunction
    public void sendCloseMessage(Client player, float radius, string sender, string msg)
    {
        List<Client> nearPlayers = API.getPlayersInRadiusOfPlayer(radius, player);
        foreach (Client target in nearPlayers)
        {
            API.sendChatMessageToPlayer(player, sender, msg);
        }
    }
    public void OnPlayerChat(Client player, string message, CancelEventArgs e)
    {
        sendCloseMessage(player, 15.0f, "~#ffffff~", API.getPlayerName(player) + " says: " + message);
        e.Cancel = true;
        return;
    }
    #endregion
    #region commands
    [Command("fc", GreedyArg = false)] // enable free cam
    public void Command_fc(Client sender)
    {
        API.sendChatMessageToPlayer(sender, "~g~Message: ~w~Free cam enabled..");
        API.triggerClientEvent(sender, "FreeCamEvent", API.getEntityPosition(sender));
    }
    [Command("fcoff", GreedyArg = false)] // disable free cam
    public void Command_fcoff(Client sender)
    {
        API.sendChatMessageToPlayer(sender, "~g~Message: ~w~Free cam disabled.");
        API.triggerClientEvent(sender, "FreeCamEventStop");
    }
    [Command("save", GreedyArg = true)]
    public void Command_Save(Client sender, string name = "Unknown")
    {
        var pos = API.getEntityPosition(sender);
        File.AppendAllText("savepos.txt", string.Format("{0}:new Vector3({1}, {2}, {3})", name, pos.X, pos.Y, pos.Z));
        API.sendNotificationToPlayer(sender, string.Format("Position saved as: {0}", name), true);
    }
    #endregion

}
