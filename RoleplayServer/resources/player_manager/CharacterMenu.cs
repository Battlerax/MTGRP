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
                    character.last_pos = new Vector3(-1037.253, -2736.865, 13.76621);
                    character.last_rot = new Vector3(0, 0, -21.91818);

                    character.insert();

                    API.setEntityData(player, "Character", character);
                    PlayerManager.players.Add(character);

                    API.setEntityPosition(player.handle, character.last_pos);
                    API.setEntityRotation(player.handle, character.last_rot);
                    API.setEntityDimension(player.handle, 0);
                    API.sendChatMessageToPlayer(player, "You have successfully created your character: " + char_name);
                    API.triggerClientEvent(player, "login_finished");
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
                        API.setEntityData(player, "Character", c);
                        PlayerManager.players.Add(c);
                        break;
                    }

                    Character character = API.getEntityData(player.handle, "Character");

                    if(character.account_id != account._id.ToString())
                    {
                        API.sendChatMessageToPlayer(player, "~r~ ERROR: This character does not belong to this account!");
                        API.kickPlayer(player);
                        return;
                    }

                    API.setEntityPosition(player.handle, character.last_pos);
                    API.setEntityRotation(player.handle, character.last_rot);
                    API.setEntityDimension(player.handle, 0);
                    API.sendChatMessageToPlayer(player, "You have successfully loaded your character: " + char_name);
                    API.triggerClientEvent(player, "login_finished");
                }

            }
        }
    }
}
