using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.component_manager;
using RoleplayServer.core;
using RoleplayServer.database_manager;
using RoleplayServer.group_manager;
using RoleplayServer.job_manager;
using RoleplayServer.phone_manager;
using RoleplayServer.group_manager.lspd;
using RoleplayServer.inventory;

namespace RoleplayServer.player_manager
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
            public readonly Character character;

            public CharacterLoginEventArgs(Character chr)
            {
                character = chr;
            }
        }
        public static event EventHandler<CharacterLoginEventArgs> OnCharacterLogin;

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
                            Client = player
                        };

                        character.Insert();

                        API.setEntityData(player.handle, "Character", character);
                        PlayerManager.Players.Add(character);

                        API.sendChatMessageToPlayer(player, "Welcome to Los Santos, " + charName + "! Let's get started with what you look like!");
                        API.freezePlayer(player, true);
                        API.triggerClientEvent(player, "show_character_creation_menu");
                    }
                    else
                    {
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
                            PlayerManager.Players.Add(c);
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

                        if (character.Group != Group.None)
                        {
                            GroupManager.SendGroupMessage(player,
                                character.CharacterName + " from your group has logged in.");

                            if (character.Group.CommandType == Group.CommandTypeLspd)
                            {
                                API.setEntitySyncedData(character.Client.handle, "IsCop", true);
                            }
                        }

                        if (character.isJailed)
                        {
                            Lspd.jailControl(player, character.jailTimeLeft);
                        }

                        API.sendChatMessageToPlayer(player, "You have successfully loaded your character: " + charName);
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

                    if(character.Model.Gender == Character.GenderMale)
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
                        }
                    }

                    character.update_ped();
                    character.Save();
                }
                    break;
                case "finish_character_creation":
                {
                    Character character = API.getEntityData(player.handle, "Character");
                    character.Age = (int)arguments[0];
                    character.Birthday = (string)arguments[1];
                    character.Birthplace = (string)arguments[2];

                    if((int)arguments[3] == 0) //Airport spawn
                    {
                        character.LastPos = new Vector3(-1037.253, -2736.865, 13.76621);
                        character.LastRot = new Vector3(0, 0, -37);
                    }
                    else //Train Station spawn
                    {
                        character.LastPos = new Vector3(433.2354, -645.8408, 28.72639);
                        character.LastRot = new Vector3(0, 0, 90);
                    }

                    character.update_ped();
                    character.update_nametag();

                    API.setEntityPosition(player.handle, character.LastPos);
                    API.setEntityRotation(player.handle, character.LastRot);
                    API.setEntityDimension(player.handle, 0);
                    API.freezePlayer(player, false);
                    API.sendChatMessageToPlayer(player, "~g~You have successfully created your character: " + character.CharacterName + "!");
                    API.sendChatMessageToPlayer(player, "~g~If you have any questions please use /n(ewbie) chat or /ask for moderator assitance.");

                    character.IsCreated = true;
                    character.StartTrackingTimePlayed();
                    character.Save();

                    API.triggerClientEvent(player, "login_finished");
                }
                    break;
                case "initialize_hair":
                    var maxHairStyles = (int)arguments[0] == Character.GenderMale ? ComponentManager.ValidMaleHair.Count : ComponentManager.ValidFemaleHair.Count;

                    API.triggerClientEvent(player, "initialize_hair", maxHairStyles);
                    break;
                case "initiate_style_limits":
                    var components = new List<string>();
             
                    if((int)arguments[0] == Character.GenderMale)
                    {
                   
                        foreach(var c in ComponentManager.ValidMaleLegs)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeLegs,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleShoes)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeShoes,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleAccessories)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeAccessories,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleUndershirt)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeUndershirt,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleTops)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeTops,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleHats)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeHats,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleGlasses)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeGlasses,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidMaleEars)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeEars,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }
                    }
                    else
                    {
                  
                        foreach (var c in ComponentManager.ValidFemaleLegs)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeLegs,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleShoes)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeShoes,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleAccessories)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeAccessories,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleUndershirt)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeUndershirt,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleTops)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeTops,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleHats)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeHats,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleGlasses)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeGlasses,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }

                        foreach (var c in ComponentManager.ValidFemaleEars)
                        {
                            var dic = new Dictionary<string, object>
                            {
                                ["type"] = Component.ComponentTypeEars,
                                ["name"] = c.Name,
                                ["id"] = c.ComponentId,
                                ["variations"] = c.Variations.Count
                            };
                            components.Add(API.toJson(dic));
                        }
                    }
                    API.triggerClientEvent(player, "initialize_components", components);
                    break;
            }
        }
    }
}
