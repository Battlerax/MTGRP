using System.Collections.Generic;
using GTANetworkServer;

namespace mtgvrp.core
{
    public class Whitelist : Script
    {

       

        private static bool _useWhitelist = true;

        private static readonly List<string> WhitelistedNames = new List<string>
        {
             "ChenkoRules",
             "nickson1993",
             "NortonPlays",
             "Millingtonlol",
             "Ahmad45123",
             "Colm692",
             "XeroTheGreat",
             "Im_Toro",
             "Nickson1993",
             "Sandman4321",

             //Kevin & Friends
             "maniac1994",
             "DontCallMeKevin",
             "Wilko0103",
             "Maxispio",

             //Admins
             "Eagle2404",
             "Risenful",
             "ClarkAU",
             "Altezza.",
             "1Bxnnj",
             "Clarke446",

             //Closed Beta
             "MTGFord",
             "CnrGTA",
             "Mooneyyy26",
             "22403681",
             "TheSmoothie",
             "boomitswill",
             "YoBoyDallas",
             "JackoDon",
             "Hambooger69",
             "DualSkulz",
             "iMerle",
             "Charpur",
             "YepityS",
             "Boca11",
             "ltzJordan",
             "ToastyTheZombie",
             "unlivedpoem",
             "Shaneboy7780",
             "Dreamvenoms",
             "SwirlyD",
             "SirRichTea",
             "IICreedx",
             "SovietLeninFluxx",
             "XcRaZyToMX",
             "SlipkyMTG",
             "NotoriousVIII",
             "cEmmet",
             "BigTucks",
             "Westingham",
             "Nowin.V1",
             "Connor123k",
             "Juuuusoo",
             "XeroTheGreat",
             "SlavishClown0",
             "BasedSosaa",
             "YoMomK",
             "justcallmeGorM",
             "Weavetron",
             "Guvnors",
             "MTGMoleman",
             "Dylan_Parker",
             "iEinarS",
             "TheSunglass",
             "stevanowskis007",
             "GreenIsGo",
             "BCBud420",
             "IntergratedGamer",
             "Jaguarr234",
             "Craignum",
             "cartoonmagic",
             "laitilee",
             "shaneboy7780",
             "ChipotleAwayJay",
             "Lambeeee",
             "Battlerax",
             "RedDeadVet360",
             "ChipotleAwayJay",
        };

        public Whitelist()
        {
            API.onPlayerConnected += WhiteList_OnPlayerConnect;
            API.consoleOutput("[WHITELIST] Whitelist is " + ((_useWhitelist == true) ? ("Active") : ("Inactive")));
        }

        public void WhiteList_OnPlayerConnect(Client player)
        {
            if (_useWhitelist == true)
            {
                if (!WhitelistedNames.Contains(player.socialClubName))
                {
                    API.kickPlayer(player, "You are not whitelisted.");
                }
            }
        }

    }
}
