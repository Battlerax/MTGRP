using System;
using System.Collections.Generic;
using System.Threading;


using GTANetworkAPI;


using mtgvrp.AdminSystem;
using mtgvrp.component_manager;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lspd;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.phone_manager;
using mtgvrp.property_system.businesses;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;
using Color = mtgvrp.core.Color;
using Timer = System.Timers.Timer;

namespace mtgvrp.player_manager
{
    class CharacterMenu : Script
    {
        public CharacterMenu()
        {
        }

        //On Character Enter Event.
        public class CharacterLoginEventArgs : EventArgs
        {
            public readonly Character Character;

            public CharacterLoginEventArgs(Character chr)
            {
                Character = chr;
            }
        }
        public static event EventHandler<CharacterLoginEventArgs> OnCharacterLogin;

        public List<GameVehicle> SpawnedVehicles = new List<GameVehicle>();

        public void SpawnCharacter(Player player)
        {
            Account acc = player.GetAccount();
            Character character = player.GetCharacter();
            character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
            character.LastRot = new Vector3(0, 0, 90);
            character.update_ped();
            character.update_nametag();
            NAPI.Entity.SetEntityPosition(player, character.LastPos);
            NAPI.Entity.SetEntityRotation(player, character.LastRot);
            NAPI.Entity.SetEntityDimension(player, 0);
            player.TriggerEvent("freezePlayer", false);
            NAPI.Chat.SendChatMessageToPlayer(player,
                "~g~You have successfully created your character: " + character.CharacterName + "!");
            NAPI.Chat.SendChatMessageToPlayer(player,
                "~g~If you have any questions please use /n(ewbie) chat or /ask for moderator assitance.");

            //Startup money.
            character.BankBalance = 20000;
            InventoryManager.GiveInventoryItem(character, new Money(), 5000);

            acc.IsLoggedIn = true;
            character.IsCreated = true;
            character.StartTrackingTimePlayed();
            character.Save();

            NAPI.ClientEvent.TriggerClientEvent(player, "login_finished");
        }

        [RemoteEvent("OnCharacterMenuSelect")]
        public void OnCharacterMenuSelect(Player player, params object[] arguments)
        {
            Account account = player.GetAccount();
            var charName = (string)arguments[0];

            account.LastIp = player.Address;

            if (charName == "Create new character")
            {

                var filter = Builders<Character>.Filter.Eq("AccountId", account.Id.ToString());
                var characters = DatabaseManager.CharacterTable.Find(filter).ToList();

                if (characters.Count >= account.CharacterSlots)
                {
                    player.SendChatMessage($"You cannot own more than {account.CharacterSlots} characters.");
                    return;
                }

                charName = (string)arguments[1];

                if (charName.Length < 1)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~ERROR: The character name entered is too short.");
                    return;
                }

                if (Character.IsCharacterRegistered(charName))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~ ERROR: That character name is already registered.");
                    return;
                }

                var character = new Character
                {
                    CharacterName = charName,
                    AccountId = account.Id.ToString(),
                    Player = player,
                };

                character.Insert();

                NAPI.Data.SetEntityData(player, "Character", character);
                PlayerManager.AddPlayer(character);

                NAPI.Chat.SendChatMessageToPlayer(player, "Welcome to Los Santos, " + charName + "! Let's get started with what you look like!");
                player.TriggerEvent("freezePlayer", true);
                NAPI.Entity.SetEntityDimension(player, (uint)player.GetCharacter().Id + 1000);
                NAPI.Data.SetEntitySharedData(player, "REG_DIMENSION", player.GetCharacter().Id + 1000);
                character.Model.SetDefault();
                NAPI.ClientEvent.TriggerClientEvent(player, "show_character_creation_menu");
            }
            else
            {
                if (NAPI.Data.HasEntityData(player, "Character") == true)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.Yellow,
                        "Your character is already loaded, please be patient.");
                    return;
                }

                var filter = Builders<Character>.Filter.Eq("CharacterName", charName);
                var foundCharacters = DatabaseManager.CharacterTable.Find(filter).ToList();

                if (foundCharacters.Count > 1)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~ERROR: More than one character found with that name.");
                    return;
                }

                if (foundCharacters.Count == 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~ ERROR: No characters found with that name.");
                    return;
                }

                foreach (var c in foundCharacters)
                {
                    NAPI.Data.SetEntityData(player, "Character", c);
                    PlayerManager.AddPlayer(c);
                    break;
                }

                Character character = player.GetCharacter();
                character.Player = player;

                if (character.AccountId != account.Id.ToString())
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "~r~ ERROR: This character does not belong to this account!");
                    NAPI.Player.KickPlayer(player);
                    return;
                }

                if (character.IsCreated == false)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Welcome back, " + character.CharacterName + "! Let's finish figuring out what you look like!");
                    character.update_ped();
                    player.TriggerEvent("freezePlayer", true);
                    NAPI.Entity.SetEntityDimension(player, (uint)player.GetCharacter().Id + 1000);
                    NAPI.Data.SetEntitySharedData(player, "REG_DIMENSION", player.GetCharacter().Id + 1000);
                    character.Model.SetDefault();
                    NAPI.ClientEvent.TriggerClientEvent(player, "show_character_creation_menu");
                    return;
                }

                API.SetPlayerSkin(player,
                    character.Model.Gender == Character.GenderMale
                        ? PedHash.FreemodeMale01
                        : PedHash.FreemodeFemale01);

                character.update_ped();
                character.update_nametag();
                character.StartTrackingTimePlayed();
                API.Shared.TriggerClientEvent(player, "update_money_display", Money.GetCharacterMoney(character));

                character.JobOne = JobManager.GetJobById(character.JobOneId);
                character.Group = GroupManager.GetGroupById(character.GroupId);

                var lmcitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                if (lmcitems.Length != 0)
                {
                    var lmcphone = (Phone)lmcitems[0];
                    lmcphone.LoadContacts();
                }

                NAPI.Entity.SetEntityPosition(player, character.LastPos);
                NAPI.Entity.SetEntityRotation(player, character.LastRot);
                NAPI.Entity.SetEntityDimension(player, (uint)character.LastDimension);
                NAPI.Player.SetPlayerHealth(player, character.Health);
                NAPI.Player.SetPlayerArmor(player, character.Armor);

                if (account.AdminLevel > 0)
                {
                    foreach (var p in PlayerManager.Players)
                    {
                        if (p.Player.GetAccount().AdminLevel > 0)
                        {
                            p.Player.SendChatMessage($"Admin {account.AdminName} has signed in.");
                        }
                    }
                }

                if (character.Group != Group.None)
                {
                    GroupManager.SendGroupMessage(player,
                        character.rp_name() + " from your group has logged in.");

                    if (character.Group.CommandType == Group.CommandTypeLspd)
                    {
                        NAPI.Data.SetEntitySharedData(character.Player, "IsCop", true);
                    }
                }

                if (character.IsJailed)
                {
                    Lspd.JailControl(player, character.JailTimeLeft);
                }

                if (character.isAJailed)
                {
                    AdminCommands.aJailControl(player, character.aJailTimeLeft);
                }

                NAPI.Player.SetPlayerHealth(player, character.Health);
                NAPI.Chat.SendChatMessageToPlayer(player, "You have successfully loaded your character: " + charName);
                LogManager.Log(LogManager.LogTypes.Connection, player.SocialClubName + $" has loaded the character {character.CharacterName}. (IP: " + player.Address + ")");

                NAPI.ClientEvent.TriggerClientEvent(player, "login_finished");
                OnCharacterLogin(this, new CharacterLoginEventArgs(character));
            }
        }

        [RemoteEvent("change_parent_info")]
        public void ChangeParentInfo(Player player, params object[] arguments)
        {
            var fatherPed = (Entity)arguments[0];
            var motherPed = (Entity)arguments[1];
            var fatherIntId = (int)arguments[2];
            var motherIntId = (int)arguments[3];
            var parentLean = (float)arguments[4];
            var gender = (int)arguments[5];

            player.TriggerEvent("CallNative", Hash.SET_PED_HEAD_BLEND_DATA, fatherPed, fatherIntId, fatherIntId, 0, fatherIntId, fatherIntId, 0, 1.0, 1.0, 0, false);
            player.TriggerEvent("CallNative", Hash.SET_PED_HEAD_BLEND_DATA, motherPed, motherIntId, motherIntId, 0, motherIntId, motherIntId, 0, 1.0, 1.0, 0, false);

            Character character = player.GetCharacter();

            character.Model.FatherId = fatherIntId;
            character.Model.MotherId = motherIntId;
            character.Model.ParentLean = parentLean;

            if (character.Model.Gender != gender)
            {
                character.Model.Gender = gender;
                character.Model.SetDefault();
            }

            character.update_ped();
            character.Save();
        }

        [RemoteEvent("change_facial_features")]
        public void ChangeFacialFeatures(Player player, params object[] arguments)
        {
            Character character = player.GetCharacter();

            //character.Model.HairStyle = character.Model.Gender == Character.GenderMale ? ComponentManager.ValidMaleHair[(int)arguments[0]].ComponentId : ComponentManager.ValidFemaleHair[(int)arguments[0]].ComponentId;
            character.Model.HairStyle = (int)arguments[0];

            character.Model.HairColor = (int)arguments[1];
            character.Model.Blemishes = (int)arguments[2];
            character.Model.FacialHair = (int)arguments[3];
            character.Model.Eyebrows = (int)arguments[4];
            character.Model.Ageing = (int)arguments[5];
            character.Model.Makeup = (int)arguments[6];
            character.Model.MakeupColor = (int)arguments[7];
            character.Model.Blush = (int)arguments[8];
            character.Model.BlushColor = (int)arguments[9];
            character.Model.Complexion = (int)arguments[10];
            character.Model.SunDamage = (int)arguments[11];
            character.Model.Lipstick = (int)arguments[12];
            character.Model.LipstickColor = (int)arguments[13];
            character.Model.MolesFreckles = (int)arguments[14];
            character.update_ped();
            character.Save();
        }

        [RemoteEvent("change_clothes")]
        public void ChangeClothes(Player player, params object[] arguments)
        {
            Character character = player.GetCharacter();

            if (character.Model.Gender == Character.GenderMale)
            {
                character.Model.PantsStyle = ComponentManager.ValidMaleLegs[(int)arguments[0]].ComponentId;
                character.Model.PantsVar = (int)ComponentManager.ValidMaleLegs[(int)arguments[0]]
                    .Variations[(int)arguments[1]];

                character.Model.ShoeStyle = ComponentManager.ValidMaleShoes[(int)arguments[2]].ComponentId;
                character.Model.ShoeVar = (int)ComponentManager.ValidMaleShoes[(int)arguments[2]]
                    .Variations.ToArray().GetValue((int)arguments[3]);

                character.Model.AccessoryStyle = ComponentManager.ValidMaleAccessories[(int)arguments[4]]
                    .ComponentId;
                character.Model.AccessoryVar = (int)ComponentManager
                    .ValidMaleAccessories[(int)arguments[4]].Variations.ToArray()
                    .GetValue((int)arguments[5]);

                character.Model.UndershirtStyle = ComponentManager.ValidMaleUndershirt[(int)arguments[6]]
                    .ComponentId;
                character.Model.UndershirtVar = (int)ComponentManager
                    .ValidMaleUndershirt[(int)arguments[6]].Variations.ToArray()
                    .GetValue((int)arguments[7]);

                character.Model.TopStyle = ComponentManager.ValidMaleTops[(int)arguments[8]].ComponentId;
                character.Model.TopVar = (int)ComponentManager.ValidMaleTops[(int)arguments[8]].Variations
                    .ToArray().GetValue((int)arguments[9]);

                character.Model.HatStyle = ComponentManager.ValidMaleHats[(int)arguments[10]].ComponentId;
                character.Model.HatVar = (int)ComponentManager.ValidMaleHats[(int)arguments[10]]
                    .Variations.ToArray().GetValue((int)arguments[11]);

                character.Model.GlassesStyle = ComponentManager.ValidMaleGlasses[(int)arguments[12]]
                    .ComponentId;
                character.Model.GlassesVar = (int)ComponentManager.ValidMaleGlasses[(int)arguments[12]]
                    .Variations.ToArray().GetValue((int)arguments[13]);

                character.Model.EarStyle = ComponentManager.ValidMaleEars[(int)arguments[14]].ComponentId;
                character.Model.EarVar = (int)ComponentManager.ValidMaleEars[(int)arguments[14]]
                    .Variations.ToArray().GetValue((int)arguments[15]);

                character.Model.TorsoStyle = (int)arguments[16];
                character.Model.TorsoVar = (int)arguments[17];
            }
            else
            {
                character.Model.PantsStyle = ComponentManager.ValidFemaleLegs[(int)arguments[0]].ComponentId;
                character.Model.PantsVar = (int)ComponentManager.ValidFemaleLegs[(int)arguments[0]]
                    .Variations[(int)arguments[1]];

                character.Model.ShoeStyle = ComponentManager.ValidFemaleShoes[(int)arguments[2]].ComponentId;
                character.Model.ShoeVar = (int)ComponentManager.ValidFemaleShoes[(int)arguments[2]]
                    .Variations.ToArray().GetValue((int)arguments[3]);

                character.Model.AccessoryStyle = ComponentManager.ValidFemaleAccessories[(int)arguments[4]]
                    .ComponentId;
                character.Model.AccessoryVar = (int)ComponentManager
                    .ValidFemaleAccessories[(int)arguments[4]].Variations.ToArray()
                    .GetValue((int)arguments[5]);

                character.Model.UndershirtStyle = ComponentManager.ValidFemaleUndershirt[(int)arguments[6]]
                    .ComponentId;
                character.Model.UndershirtVar = (int)ComponentManager
                    .ValidFemaleUndershirt[(int)arguments[6]].Variations.ToArray()
                    .GetValue((int)arguments[7]);

                character.Model.TopStyle = ComponentManager.ValidFemaleTops[(int)arguments[8]].ComponentId;
                character.Model.TopVar = (int)ComponentManager.ValidFemaleTops[(int)arguments[8]].Variations
                    .ToArray().GetValue((int)arguments[9]);

                character.Model.HatStyle = ComponentManager.ValidFemaleHats[(int)arguments[10]].ComponentId;
                character.Model.HatVar = (int)ComponentManager.ValidFemaleHats[(int)arguments[10]]
                    .Variations.ToArray().GetValue((int)arguments[11]);

                character.Model.GlassesStyle = ComponentManager.ValidFemaleGlasses[(int)arguments[12]]
                    .ComponentId;
                character.Model.GlassesVar = (int)ComponentManager.ValidFemaleGlasses[(int)arguments[12]]
                    .Variations.ToArray().GetValue((int)arguments[13]);

                character.Model.EarStyle = ComponentManager.ValidFemaleEars[(int)arguments[14]].ComponentId;
                character.Model.EarVar = (int)ComponentManager.ValidFemaleEars[(int)arguments[14]]
                    .Variations.ToArray().GetValue((int)arguments[15]);

                character.Model.TorsoStyle = (int)arguments[16];
                character.Model.TorsoVar = (int)arguments[17];
            }

            character.update_ped();
            character.Save();
        }

        [RemoteEvent("finish_character_creation")]
        public void FinishCharacterCreation(Player player, params object[] arguments)
        {
            Character character = player.GetCharacter();
            Account acc = player.GetAccount();
            character.Age = (int)arguments[0];
            character.Birthday = (string)arguments[1];
            character.Birthplace = (string)arguments[2];

            /*if ((int) arguments[3] == 0) //Airport spawn
            {
                character.LastPos = new Vector3(-1037.253, -2736.865, 13.76621);
                character.LastRot = new Vector3(0, 0, -37);
            }
            else //Train Station spawn
            {
                character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
                character.LastRot = new Vector3(0, 0, 90);
            }*/

            if (!player.HasData("REDOING_CHAR"))
            {
                character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
                character.LastRot = new Vector3(0, 0, 90);
                character.update_ped();
                character.update_nametag();
                NAPI.ClientEvent.TriggerClientEvent(player, "start_introduction");
            }

            else
            {
                character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
                character.LastRot = new Vector3(0, 0, 90);
                character.update_ped();
                character.update_nametag();
                NAPI.Entity.SetEntityPosition(player, character.LastPos);
                NAPI.Entity.SetEntityRotation(player, character.LastRot);
                NAPI.Entity.SetEntityDimension(player, 0);
                player.TriggerEvent("freezePlayer", false);
                player.ResetData("REDOING_CHAR");
            }
        }

        [RemoteEvent("initialize_hair")]
        public void InitializeHair(Player player, params object[] arguments)
        {
            List<string> hairstyleNames = new List<string>();
            List<int> hairstyleIds = new List<int>();
            if ((int)arguments[0] == Character.GenderMale)
            {
                foreach (Component c in ComponentManager.ValidMaleHair)
                {
                    hairstyleNames.Add(c.Name);
                    hairstyleIds.Add(c.ComponentId);
                }
            }
            else
            {
                foreach (Component c in ComponentManager.ValidFemaleHair)
                {
                    hairstyleNames.Add(c.Name);
                    hairstyleIds.Add(c.ComponentId);
                }
            }
            var hairstyleNamesString = String.Join(",", hairstyleNames);
            var hairstyleIdsString = String.Join(",", hairstyleIds);
            var maxHairStyles = (int)arguments[0] == Character.GenderMale ? ComponentManager.ValidMaleHair.Count : ComponentManager.ValidFemaleHair.Count;

            NAPI.ClientEvent.TriggerClientEvent(player, "initialize_hair", maxHairStyles, hairstyleNamesString, hairstyleIdsString);
        }

        [RemoteEvent("initiate_style_limits")]
        public void InitializeStyleLimits(Player player, params object[] arguments)
        {
            Character cha = player.GetCharacter();
            NAPI.ClientEvent.TriggerClientEvent(player, "initialize_components", (cha.Model.Gender == Character.GenderMale ? Clothing.MaleComponents : Clothing.FemaleComponents));
        }

        // This stuff was commented out in the event handler, i just transferred them into events for now. idk. - austin
        /*[RemoteEvent("bus_driving_bridge")]
        public void BusDrivingBridge(Player player, params object[] arguments)
        {
            var vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(-276.1117, -2411.626, 59.68943), new Vector3(0, 0, 53.19402), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypeTemp, 0, 0, NAPI.Entity.GetEntityDimension(player));
            vehicle.Insert();
            VehicleManager.spawn_vehicle(vehicle);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle.Entity, -1);
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle.Entity, true);
            SpawnedVehicles.Add(vehicle);

            API.SendNativeToAllPlayers(Hash.TASK_VEHICLE_DRIVE_TO_COORD, player, vehicle.Entity, -582.3301, -2201.367, 56.25008, 120f, 1f, vehicle.GetHashCode(), 16777216, 1f, true);
        }

        [RemoteEvent("bus_driving_station")]
        public void BusDrivingStation(Player player, params object[] arguments)
        {
            var vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(513.3119, -676.2706, 25.19653), new Vector3(0, 0, 85.25442), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypeTemp, 0, 0, NAPI.Entity.GetEntityDimension(player));
            vehicle.Insert();
            VehicleManager.spawn_vehicle(vehicle);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle.Entity, -1);
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle.Entity, true);
            SpawnedVehicles.Add(vehicle);

            API.SendNativeToAllPlayers(Hash.TASK_VEHICLE_DRIVE_TO_COORD, player, vehicle.Entity, 464.645, -673.3629, 27.20791, 10f, 1f, vehicle.GetHashCode(), 16777216, 1f, true);
        }

        [RemoteEvent("player_exiting_bus")]
        public void PlayerExitingBus(Player player, params object[] arguments)
        {
            var vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(429.8345, -672.5932, 29.05217), new Vector3(0.9295838, 3.945374, 90.3828), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypePerm, 0, 0, NAPI.Entity.GetEntityDimension(player));
            vehicle.Insert();
            VehicleManager.spawn_vehicle(vehicle);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle.Entity, -1);
            NAPI.Vehicle.SetVehicleEngineStatus(vehicle.Entity, true);
            SpawnedVehicles.Add(vehicle);

            API.SendNativeToAllPlayers(Hash.TASK_LEAVE_VEHICLE, player, vehicle.Entity, 0);
        }*/

        [RemoteEvent("finish_intro")]
        public void FinishIntro(Player player, params object[] arguments)
        {
            //foreach (var veh in SpawnedVehicles) { veh.Despawn(); veh.Delete(); }
            SpawnCharacter(player);
        }
    }
}
