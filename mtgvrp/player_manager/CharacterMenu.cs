using System;
using System.Timers;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
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

namespace mtgvrp.player_manager
{
    class CharacterMenu : Script
    {
        public CharacterMenu()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
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

        public List<mtgvrp.vehicle_manager.Vehicle> SpawnedVehicles = new List<mtgvrp.vehicle_manager.Vehicle>();

        public void SpawnCharacter(Client player)
        {
            Account acc = API.getEntityData(player, "Account");
            Character character = API.getEntityData(player, "Character");
            character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
            character.LastRot = new Vector3(0, 0, 90);
            character.update_ped();
            character.update_nametag();
            API.setEntityPosition(player.handle, character.LastPos);
            API.setEntityRotation(player.handle, character.LastRot);
            API.setEntityDimension(player.handle, 0);
            API.freezePlayer(player, false);
            API.sendChatMessageToPlayer(player,
                "~g~You have successfully created your character: " + character.CharacterName + "!");
            API.sendChatMessageToPlayer(player,
                "~g~If you have any questions please use /n(ewbie) chat or /ask for moderator assitance.");

            //Startup money.
            character.BankBalance = 20000;
            InventoryManager.GiveInventoryItem(character, new Money(), 5000);

            acc.IsLoggedIn = true;
            character.IsCreated = true;
            character.StartTrackingTimePlayed();
            character.PaycheckTimer = new Timer { Interval = 1000 };
            character.PaycheckTimer.Elapsed += delegate { PlayerManager.SendPaycheckToPlayer(player); };
            character.PaycheckTimer.Start();
            character.Save();

            API.triggerClientEvent(player, "login_finished");
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "OnCharacterMenuSelect":
                    Account account = player.GetAccount();
                    var charName = (string)arguments[0];

                    account.LastIp = player.address;

                    if(charName == "Create new character")
                    {

                        var filter = Builders<Character>.Filter.Eq("AccountId", account.Id.ToString());
                        var characters = DatabaseManager.CharacterTable.Find(filter).ToList();

                        if (characters.Count >= account.CharacterSlots)
                        {
                            player.sendChatMessage($"You cannot own more than {account.CharacterSlots} characters.");
                            return;
                        }

                        charName = (string)arguments[1];

                        if(charName.Length < 1)
                        {
                            API.sendChatMessageToPlayer(player, "~r~ERROR: The character name entered is too short.");
                            return;
                        }

                        if (Character.IsCharacterRegistered(charName))
                        {
                            API.sendChatMessageToPlayer(player, "~r~ ERROR: That character name is already registered.");
                            return;
                        }

                        var character = new Character
                        {
                            CharacterName = charName,
                            AccountId = account.Id.ToString(),
                            Client = player,
                        };

                        character.Insert();

                        API.setEntityData(player.handle, "Character", character);
                        PlayerManager.AddPlayer(character);

                        API.sendChatMessageToPlayer(player, "Welcome to Los Santos, " + charName + "! Let's get started with what you look like!");
                        API.freezePlayer(player, true);
                        API.setEntityDimension(player, player.GetCharacter().Id + 1000);
                        API.setEntitySyncedData(player, "REG_DIMENSION", player.GetCharacter().Id + 1000);
                        character.Model.SetDefault();
                        API.triggerClientEvent(player, "show_character_creation_menu");
                    }
                    else
                    {
                        if (API.hasEntityData(player.handle, "Character") == true)
                        {
                            API.sendChatMessageToPlayer(player, Color.Yellow,
                                "Your character is already loaded, please be patient.");
                            return;
                        }

                        var filter = Builders<Character>.Filter.Eq("CharacterName", charName);
                        var foundCharacters = DatabaseManager.CharacterTable.Find(filter).ToList();

                        if(foundCharacters.Count > 1)
                        {
                            API.sendChatMessageToPlayer(player, "~r~ERROR: More than one character found with that name.");
                            return;
                        }

                        if(foundCharacters.Count == 0)
                        {
                            API.sendChatMessageToPlayer(player, "~r~ ERROR: No characters found with that name.");
                            return;
                        }

                        foreach(var c in foundCharacters)
                        {
                            API.setEntityData(player.handle, "Character", c);
                            PlayerManager.AddPlayer(c);
                            break;
                        }

                        Character character = API.getEntityData(player.handle, "Character");
                        character.Client = player;

                        if (character.AccountId != account.Id.ToString())
                        {
                            API.sendChatMessageToPlayer(player, "~r~ ERROR: This character does not belong to this account!");
                            API.kickPlayer(player);
                            return;
                        }

                        if(character.IsCreated == false)
                        {
                            API.sendChatMessageToPlayer(player, "Welcome back, " + character.CharacterName + "! Let's finish figuring out what you look like!");
                            character.update_ped();
                            API.freezePlayer(player, true);
                            API.setEntityDimension(player, player.GetCharacter().Id + 1000);
                            API.setEntitySyncedData(player, "REG_DIMENSION", player.GetCharacter().Id + 1000);
                            character.Model.SetDefault();
                            API.triggerClientEvent(player, "show_character_creation_menu");
                            return;
                        }

                        API.setPlayerSkin(player,
                            character.Model.Gender == Character.GenderMale
                                ? PedHash.FreemodeMale01
                                : PedHash.FreemodeFemale01);

                        character.update_ped();
                        character.update_nametag();
                        character.StartTrackingTimePlayed();
                        character.PaycheckTimer = new Timer { Interval = 1000 };
                        character.PaycheckTimer.Elapsed += delegate { PlayerManager.SendPaycheckToPlayer(player); };
                        character.PaycheckTimer.Start();
                        API.shared.triggerClientEvent(player, "update_money_display", Money.GetCharacterMoney(character));

                        character.JobOne = JobManager.GetJobById(character.JobOneId);
                        character.Group = GroupManager.GetGroupById(character.GroupId);

                        var lmcitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                        if (lmcitems.Length != 0)
                        {
                            var lmcphone = (Phone)lmcitems[0];
                            lmcphone.LoadContacts();
                        }

                        API.setEntityPosition(player.handle, character.LastPos);
                        API.setEntityRotation(player.handle, character.LastRot);
                        API.setEntityDimension(player.handle, character.LastDimension);
                        API.setPlayerHealth(player, character.Health);

                        if (character.Group != Group.None)
                        {
                            GroupManager.SendGroupMessage(player,
                                character.CharacterName + " from your group has logged in.");

                            if (character.Group.CommandType == Group.CommandTypeLspd)
                            {
                                API.setEntitySyncedData(character.Client.handle, "IsCop", true);
                            }
                        }

                        if (character.IsJailed)
                        {
                            Lspd.JailControl(player, character.JailTimeLeft);
                        }

                        API.sendChatMessageToPlayer(player, "You have successfully loaded your character: " + charName);
                        LogManager.Log(LogManager.LogTypes.Connection, player.socialClubName + $" has loaded the character {character.CharacterName}. (IP: " + player.address + ")");

                        API.triggerClientEvent(player, "login_finished");
                        OnCharacterLogin(this, new CharacterLoginEventArgs(character));
                    }
                    break;
                case "change_parent_info":
                {
                    var fatherPed = (NetHandle)arguments[0];
                    var motherPed = (NetHandle)arguments[1];
                    var fatherIntId = (int)arguments[2];
                    var motherIntId = (int)arguments[3];
                    var parentLean = (float)arguments[4];
                    var gender = (int)arguments[5];

                    API.sendNativeToPlayer(player, Hash.SET_PED_HEAD_BLEND_DATA, fatherPed, fatherIntId, fatherIntId, 0, fatherIntId, fatherIntId, 0, 1.0, 1.0, 0, false);
                    API.sendNativeToPlayer(player, Hash.SET_PED_HEAD_BLEND_DATA, motherPed, motherIntId, motherIntId, 0, motherIntId, motherIntId, 0, 1.0, 1.0, 0, false);

                    Character character = API.getEntityData(player.handle, "Character");

                    character.Model.FatherId = fatherIntId;
                    character.Model.MotherId = motherIntId;
                    character.Model.ParentLean = parentLean;

                    if(character.Model.Gender != gender)
                    {
                        character.Model.Gender = gender;
                        character.Model.SetDefault();
                    }

                    character.update_ped();
                    character.Save();
                }
                    break;
                case "change_facial_features":
                {
                    Character character = API.getEntityData(player.handle, "Character");
                
                    character.Model.HairStyle = character.Model.Gender == Character.GenderMale ? ComponentManager.ValidMaleHair[(int)arguments[0]].ComponentId : ComponentManager.ValidFemaleHair[(int)arguments[0]].ComponentId;

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
                    break;
                case "change_clothes":
                    {

                        Character character = API.getEntityData(player.handle, "Character");

                        if (character.Model.Gender == Character.GenderMale)
                        {
                            switch ((int)arguments[0])
                            {
                                case Component.ComponentTypeLegs:
                                    character.Model.PantsStyle = ComponentManager.ValidMaleLegs[(int)arguments[1]].ComponentId;
                                    character.Model.PantsVar = (int)ComponentManager.ValidMaleLegs[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeShoes:
                                    character.Model.ShoeStyle = ComponentManager.ValidMaleShoes[(int)arguments[1]].ComponentId;
                                    character.Model.ShoeVar = (int)ComponentManager.ValidMaleShoes[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeAccessories:
                                    character.Model.AccessoryStyle = ComponentManager.ValidMaleAccessories[(int)arguments[1]].ComponentId;
                                    character.Model.AccessoryVar = (int)ComponentManager.ValidMaleAccessories[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeUndershirt:
                                    character.Model.UndershirtStyle = ComponentManager.ValidMaleUndershirt[(int)arguments[1]].ComponentId;
                                    character.Model.UndershirtVar = (int)ComponentManager.ValidMaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeTops:
                                    character.Model.TopStyle = ComponentManager.ValidMaleTops[(int)arguments[1]].ComponentId;
                                    character.Model.TopVar = (int)ComponentManager.ValidMaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeHats:
                                    character.Model.HatStyle = ComponentManager.ValidMaleHats[(int)arguments[1]].ComponentId;
                                    character.Model.HatVar = (int)ComponentManager.ValidMaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeGlasses:
                                    character.Model.GlassesStyle = ComponentManager.ValidMaleGlasses[(int)arguments[1]].ComponentId;
                                    character.Model.GlassesVar = (int)ComponentManager.ValidMaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeEars:
                                    character.Model.EarStyle = ComponentManager.ValidMaleEars[(int)arguments[1]].ComponentId;
                                    character.Model.EarVar = (int)ComponentManager.ValidMaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeTorso:
                                    character.Model.TorsoStyle = (int)arguments[1];
                                    character.Model.TorsoVar = (int)arguments[2];
                                    break;
                            }
                        }
                        else
                        {
                            switch ((int)arguments[0])
                            {
                                case Component.ComponentTypeLegs:
                                    character.Model.PantsStyle = ComponentManager.ValidFemaleLegs[(int)arguments[1]].ComponentId;
                                    character.Model.PantsVar = (int)ComponentManager.ValidFemaleLegs[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeShoes:
                                    character.Model.ShoeStyle = ComponentManager.ValidFemaleShoes[(int)arguments[1]].ComponentId;
                                    character.Model.ShoeVar = (int)ComponentManager.ValidFemaleShoes[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeAccessories:
                                    character.Model.AccessoryStyle = ComponentManager.ValidFemaleAccessories[(int)arguments[1]].ComponentId;
                                    character.Model.AccessoryVar = (int)ComponentManager.ValidFemaleAccessories[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeUndershirt:
                                    character.Model.UndershirtStyle = ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].ComponentId;
                                    character.Model.UndershirtVar = (int)ComponentManager.ValidFemaleUndershirt[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeTops:
                                    character.Model.TopStyle = ComponentManager.ValidFemaleTops[(int)arguments[1]].ComponentId;
                                    character.Model.TopVar = (int)ComponentManager.ValidFemaleTops[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeHats:
                                    character.Model.HatStyle = ComponentManager.ValidFemaleHats[(int)arguments[1]].ComponentId;
                                    character.Model.HatVar = (int)ComponentManager.ValidFemaleHats[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeGlasses:
                                    character.Model.GlassesStyle = ComponentManager.ValidFemaleGlasses[(int)arguments[1]].ComponentId;
                                    character.Model.GlassesVar = (int)ComponentManager.ValidFemaleGlasses[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeEars:
                                    character.Model.EarStyle = ComponentManager.ValidFemaleEars[(int)arguments[1]].ComponentId;
                                    character.Model.EarVar = (int)ComponentManager.ValidFemaleEars[(int)arguments[1]].Variations.ToArray().GetValue((int)arguments[2]);
                                    break;
                                case Component.ComponentTypeTorso:
                                    character.Model.TorsoStyle = (int)arguments[1];
                                    character.Model.TorsoVar = (int)arguments[2];
                                    break;
                            }
                        }

                        character.update_ped();
                        character.Save();
                    }
                    break;
                case "finish_character_creation":
                {
                    Character character = API.getEntityData(player.handle, "Character");
                    Account acc = player.GetAccount();
                    character.Age = (int) arguments[0];
                    character.Birthday = (string) arguments[1];
                    character.Birthplace = (string) arguments[2];

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

                    character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
                    character.LastRot = new Vector3(0, 0, 90);
                    character.update_ped();
                    character.update_nametag();

                    API.triggerClientEvent(player, "start_introduction");
                }
                    break;
                case "initialize_hair":
                    var maxHairStyles = (int)arguments[0] == Character.GenderMale ? ComponentManager.ValidMaleHair.Count : ComponentManager.ValidFemaleHair.Count;

                    API.triggerClientEvent(player, "initialize_hair", maxHairStyles);
                    break;
                case "initiate_style_limits":
                    Character cha = API.getEntityData(player.handle, "Character");
                    API.triggerClientEvent(player, "initialize_components", (cha.Model.Gender == Character.GenderMale ? Clothing.MaleComponents : Clothing.FemaleComponents));
                    break;

                case "bus_driving_bridge":
                    var vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(-276.1117, -2411.626, 59.68943), new Vector3(0, 0, 53.19402), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypeTemp, 0, 0, API.getEntityDimension(player));
                    vehicle.Insert();
                    VehicleManager.spawn_vehicle(vehicle);
                    API.setPlayerIntoVehicle(player, vehicle.NetHandle, -1);
                    API.setVehicleEngineStatus(vehicle.NetHandle, true);
                    SpawnedVehicles.Add(vehicle);

                    API.sendNativeToAllPlayers(Hash.TASK_VEHICLE_DRIVE_TO_COORD, player.handle, vehicle.NetHandle, -582.3301, -2201.367, 56.25008, 120f, 1f, vehicle.GetHashCode(), 16777216, 1f, true);
                    break;

                case "bus_driving_station":
                    vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(513.3119, -676.2706, 25.19653), new Vector3(0, 0, 85.25442), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypeTemp, 0, 0, API.getEntityDimension(player));
                    vehicle.Insert();
                    VehicleManager.spawn_vehicle(vehicle);
                    API.setPlayerIntoVehicle(player, vehicle.NetHandle, -1);
                    API.setVehicleEngineStatus(vehicle.NetHandle, true);
                    SpawnedVehicles.Add(vehicle);

                    API.sendNativeToAllPlayers(Hash.TASK_VEHICLE_DRIVE_TO_COORD, player.handle, vehicle.NetHandle, 464.645, -673.3629, 27.20791, 10f, 1f, vehicle.GetHashCode(), 16777216, 1f, true);
                    break;

                case "player_exiting_bus":
                    vehicle = VehicleManager.CreateVehicle(VehicleHash.Bus, new Vector3(429.8345, -672.5932, 29.05217), new Vector3(0.9295838, 3.945374, 90.3828), "Unregistered", player.GetCharacter().Id, vehicle_manager.Vehicle.VehTypePerm, 0, 0, API.getEntityDimension(player));
                    vehicle.Insert();
                    VehicleManager.spawn_vehicle(vehicle);
                    API.setPlayerIntoVehicle(player, vehicle.NetHandle, -1);
                    API.setVehicleEngineStatus(vehicle.NetHandle, true);
                    SpawnedVehicles.Add(vehicle);

                    API.sendNativeToAllPlayers(Hash.TASK_LEAVE_VEHICLE, player.handle, vehicle.NetHandle, 0);
                    break;

                case "finish_intro":
                    foreach (var veh in SpawnedVehicles) { veh.Despawn(); veh.Delete(); }
                    SpawnCharacter(player);
                    break;

            }
        }
    }
}
