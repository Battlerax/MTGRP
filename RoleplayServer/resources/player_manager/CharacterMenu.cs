using System;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using System.Collections.Generic;

namespace RoleplayServer
{
    class CharacterMenu : Script
    {
        public CharacterMenu()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        [Command("test")]
        public void test(Client sender)
        {
            API.setEntityPosition(sender.handle, new Vector3(-30.680381774902344,  -728.7491455078125,  44.271934509277344));
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if(eventName == "OnCharacterMenuSelect")
            {
                Account account = API.getEntityData(player, "Account");
                string char_name = (string)arguments[0];

                if(char_name == "Create new character")
                {
                    char_name = (string)arguments[1];

                    if(char_name.Length < 1)
                    {
                        API.sendChatMessageToPlayer(player, "~r~ERROR: The character name entered is too short.");
                        return;
                    }

                    if (Character.IsCharacterRegistered(char_name))
                    {
                        API.sendChatMessageToPlayer(player, "~r~ ERROR: That character name is already registered.");
                        return;
                    }

                    Character character = new Character();
                    character.character_name = char_name;
                    character.account_id = account._id.ToString();
                    character.client = player;

                    character.insert();

                    API.setEntityData(player.handle, "Character", character);
                    PlayerManager.players.Add(character);

                    API.sendChatMessageToPlayer(player, "Welcome to Los Santos, " + char_name + "! Let's get started with what you look like!");
                    API.freezePlayer(player, true);
                    API.triggerClientEvent(player, "show_character_creation_menu");
                }
                else
                {
                    FilterDefinition<Character> filter = Builders<Character>.Filter.Eq("character_name", char_name);
                    List<Character> found_characters = DatabaseManager.character_table.Find(filter).ToList<Character>();

                    if(found_characters.Count > 1)
                    {
                        API.sendChatMessageToPlayer(player, "~r~ERROR: More than one character found with that name.");
                        return;
                    }

                    if(found_characters.Count == 0)
                    {
                        API.sendChatMessageToPlayer(player, "~r~ ERROR: No characters found with that name.");
                        return;
                    }

                    foreach(Character c in found_characters)
                    {
                        API.setEntityData(player.handle, "Character", c);
                        PlayerManager.players.Add(c);
                        break;
                    }

                    Character character = API.getEntityData(player.handle, "Character");
                    character.client = player;

                    if (character.account_id != account._id.ToString())
                    {
                        API.sendChatMessageToPlayer(player, "~r~ ERROR: This character does not belong to this account!");
                        API.kickPlayer(player);
                        return;
                    }

                    if(character.is_created == false)
                    {
                        API.sendChatMessageToPlayer(player, "Welcome back, " + character.character_name + "! Let's finish figuring out what you look like!");
                        character.update_ped(player.handle);
                        API.freezePlayer(player, true);
                        API.triggerClientEvent(player, "show_character_creation_menu");
                        return;
                    }

                    if(character.model.gender == Character.GENDER_MALE)
                    {
                        API.setPlayerSkin(player, PedHash.FreemodeMale01);
                    }
                    else
                    {
                        API.setPlayerSkin(player, PedHash.FreemodeFemale01);
                    }

                    character.update_ped(player.handle);
                    character.update_nametag();

                    API.setEntityPosition(player.handle, character.last_pos);
                    API.setEntityRotation(player.handle, character.last_rot);
                    API.setEntityDimension(player.handle, character.last_dimension);
                    API.sendChatMessageToPlayer(player, "You have successfully loaded your character: " + char_name);
                    
                    API.triggerClientEvent(player, "login_finished");
                }

            }
            else if(eventName == "change_parent_info")
            {
                NetHandle father_ped = (NetHandle)arguments[0];
                NetHandle mother_ped = (NetHandle)arguments[1];
                int father_int_id = (int)arguments[2];
                int mother_int_id = (int)arguments[3];
                float parent_lean = (float)arguments[4];
                int gender = (int)arguments[5];

                API.sendNativeToPlayer(player, Hash.SET_PED_HEAD_BLEND_DATA, father_ped, father_int_id, father_int_id, 0, father_int_id, father_int_id, 0, 1.0, 1.0, 0, false);
                API.sendNativeToPlayer(player, Hash.SET_PED_HEAD_BLEND_DATA, mother_ped, mother_int_id, mother_int_id, 0, mother_int_id, mother_int_id, 0, 1.0, 1.0, 0, false);

                Character character = API.getEntityData(player.handle, "Character");

                character.model.father_id = father_int_id;
                character.model.mother_id = mother_int_id;
                character.model.parent_lean = parent_lean;

                if(character.model.gender != gender)
                {
                    character.model.gender = gender;
                    character.model.setDefault();
                }

                character.update_ped(player.handle);
                character.save();
            }
            else if(eventName == "change_facial_features")
            {
                Character character = API.getEntityData(player.handle, "Character");
                
                if(character.model.gender == Character.GENDER_MALE)
                {
                    character.model.hair_style = ComponentManager.valid_male_hair[(int)arguments[0]].component_id;
                }
                else
                {
                    character.model.hair_style = ComponentManager.valid_female_hair[(int)arguments[0]].component_id;
                }

                character.model.hair_color = (int)arguments[1];
                character.model.blemishes = (int)arguments[2];
                character.model.facial_hair = (int)arguments[3];
                character.model.eyebrows = (int)arguments[4];
                character.model.ageing = (int)arguments[5];
                character.model.makeup = (int)arguments[6];
                character.model.makeup_color = (int)arguments[7];
                character.model.blush = (int)arguments[8];
                character.model.blush_color = (int)arguments[9];
                character.model.complexion = (int)arguments[10];
                character.model.sun_damage = (int)arguments[11];
                character.model.lipstick = (int)arguments[12];
                character.model.lipstick_color = (int)arguments[13];
                character.model.moles_freckles = (int)arguments[14];
                character.update_ped(player.handle);
                character.save();
            }
            else if(eventName == "change_clothes")
            {

                Character character = API.getEntityData(player.handle, "Character");

                if(character.model.gender == Character.GENDER_MALE)
                {
                    switch ((int)arguments[0])
                    {
                        case Component.COMPONENT_TYPE_LEGS:
                            character.model.pants_style = ComponentManager.valid_male_legs[(int)arguments[1]].component_id;
                            character.model.pants_var = (int)ComponentManager.valid_male_legs[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_SHOES:
                            character.model.shoe_style = ComponentManager.valid_male_shoes[(int)arguments[1]].component_id;
                            character.model.shoe_var = (int)ComponentManager.valid_male_shoes[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_ACCESSORIES:
                            character.model.accessory_style = ComponentManager.valid_male_accessories[(int)arguments[1]].component_id;
                            character.model.accessory_var = (int)ComponentManager.valid_male_accessories[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_UNDERSHIRT:
                            character.model.undershirt_style = ComponentManager.valid_male_undershirt[(int)arguments[1]].component_id;
                            character.model.undershirt_var = (int)ComponentManager.valid_male_undershirt[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_TOPS:
                            character.model.top_style = ComponentManager.valid_male_tops[(int)arguments[1]].component_id;
                            character.model.top_var = (int)ComponentManager.valid_male_tops[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_HATS:
                            character.model.hat_style = ComponentManager.valid_male_hats[(int)arguments[1]].component_id;
                            character.model.hat_var = (int)ComponentManager.valid_male_hats[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_GLASSES:
                            character.model.glasses_style = ComponentManager.valid_male_glasses[(int)arguments[1]].component_id;
                            character.model.glasses_var = (int)ComponentManager.valid_male_glasses[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_EARS:
                            character.model.ear_style = ComponentManager.valid_male_ears[(int)arguments[1]].component_id;
                            character.model.ear_var = (int)ComponentManager.valid_male_ears[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                    }
                }
                else
                {
                    switch ((int)arguments[0])
                    {
                        case Component.COMPONENT_TYPE_LEGS:
                            character.model.pants_style = ComponentManager.valid_female_legs[(int)arguments[1]].component_id;
                            character.model.pants_var = (int)ComponentManager.valid_female_legs[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_SHOES:
                            character.model.shoe_style = ComponentManager.valid_female_shoes[(int)arguments[1]].component_id;
                            character.model.shoe_var = (int)ComponentManager.valid_female_shoes[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_ACCESSORIES:
                            character.model.accessory_style = ComponentManager.valid_female_accessories[(int)arguments[1]].component_id;
                            character.model.accessory_var = (int)ComponentManager.valid_female_accessories[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_UNDERSHIRT:
                            character.model.undershirt_style = ComponentManager.valid_female_undershirt[(int)arguments[1]].component_id;
                            character.model.undershirt_var = (int)ComponentManager.valid_female_undershirt[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_TOPS:
                            character.model.top_style = ComponentManager.valid_female_tops[(int)arguments[1]].component_id;
                            character.model.top_var = (int)ComponentManager.valid_female_tops[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_HATS:
                            character.model.hat_style = ComponentManager.valid_female_hats[(int)arguments[1]].component_id;
                            character.model.hat_var = (int)ComponentManager.valid_female_hats[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_GLASSES:
                            character.model.glasses_style = ComponentManager.valid_female_glasses[(int)arguments[1]].component_id;
                            character.model.glasses_var = (int)ComponentManager.valid_female_glasses[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                        case Component.COMPONENT_TYPE_EARS:
                            character.model.ear_style = ComponentManager.valid_female_ears[(int)arguments[1]].component_id;
                            character.model.ear_var = (int)ComponentManager.valid_female_ears[(int)arguments[1]].variations.ToArray().GetValue((int)arguments[2]);
                            break;
                    }
                }

                character.update_ped(player.handle);
                character.save();
            }
            else if(eventName == "finish_character_creation")
            {
                Character character = API.getEntityData(player.handle, "Character");
                character.age = (int)arguments[0];
                character.birthday = (string)arguments[1];
                character.birthplace = (string)arguments[2];

                if((int)arguments[3] == 0) //Airport spawn
                {
                    character.last_pos = new Vector3(-1037.253, -2736.865, 13.76621);
                    character.last_rot = new Vector3(0, 0, -37);
                }
                else //Train Station spawn
                {
                    character.last_pos = new Vector3(433.2354, -645.8408, 28.72639);
                    character.last_rot = new Vector3(0, 0, 90);
                }

                character.update_ped(player.handle);
                character.update_nametag();

                API.setEntityPosition(player.handle, character.last_pos);
                API.setEntityRotation(player.handle, character.last_rot);
                API.setEntityDimension(player.handle, 0);
                API.freezePlayer(player, false);
                API.sendChatMessageToPlayer(player, "~g~You have successfully created your character: " + character.character_name + "!");
                API.sendChatMessageToPlayer(player, "~g~If you have any questions please use /n(ewbie) chat or /ask for moderator assitance.");

                character.is_created = true;
                character.save();

                API.triggerClientEvent(player, "login_finished");
            }
            else if(eventName == "initialize_hair")
            {
                int max_hair_styles = 0;
                if((int)arguments[0] == Character.GENDER_MALE)
                {
                    max_hair_styles = ComponentManager.valid_male_hair.Count;
                }
                else
                {
                    max_hair_styles = ComponentManager.valid_female_hair.Count;
                }

                API.triggerClientEvent(player, "initialize_hair", max_hair_styles);
            }
            else if(eventName == "initiate_style_limits")
            {
                List<string> components = new List<string>();
             
                if((int)arguments[0] == Character.GENDER_MALE)
                {
                   
                    foreach(Component c in ComponentManager.valid_male_legs)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_LEGS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_shoes)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_SHOES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_accessories)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_ACCESSORIES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_undershirt)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_UNDERSHIRT;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_tops)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_TOPS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_hats)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_HATS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_glasses)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_GLASSES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_male_ears)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_EARS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }
                }
                else
                {
                  
                    foreach (Component c in ComponentManager.valid_female_legs)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_LEGS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_shoes)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_SHOES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_accessories)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_ACCESSORIES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_undershirt)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_UNDERSHIRT;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_tops)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_TOPS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_hats)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_HATS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_glasses)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_GLASSES;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }

                    foreach (Component c in ComponentManager.valid_female_ears)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["type"] = Component.COMPONENT_TYPE_EARS;
                        dic["name"] = c.name;
                        dic["id"] = c.component_id;
                        dic["variations"] = c.variations.Count;
                        components.Add(API.toJson(dic));
                    }
                }
                API.triggerClientEvent(player, "initialize_components", components);
            }
        }
    }
}
