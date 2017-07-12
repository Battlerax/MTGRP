using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using MongoDB.Driver;

namespace mtgvrp.core.Discord
{
    class PlayerDiscordCommands
    {
        /*[DSharpPlus.CommandsNext.Attributes.Command("login")]
        [Description("Used to link an account with your discord.")]
        public async Task ChangeGame(CommandContext ctx, [Description("Your social club name.")] string name, [Description("The code your got ingame from /getcode")] string code)
        {
            if (!ctx.Channel.IsPrivate)
                return;

            await ctx.TriggerTypingAsync();

            //Check if already linked.
            var filter = Builders<Account>.Filter.Where(x => x.DiscordUser == ctx.Member.Username);
            if (await DatabaseManager.AccountTable.CountAsync(filter) > 0)
            {
                await ctx.RespondAsync("Your discord account is already linked.");
                return;
            }

            //Check if already in-game.
            if (PlayerManager.Players.Any(x => x.Client.socialClubName == name))
            {
                await ctx.RespondAsync("Please log out from Ingame first.");
                return;
            }

            //Check if code exists in there name.
            filter = Builders<Account>.Filter.Where(x => x.AccountName == name && x.DiscordCode == code);
            if (await DatabaseManager.AccountTable.CountAsync(filter) == 0)
            {
                await ctx.RespondAsync("Invalid name or code.");
                return;
            }

            //Set the discord as owned.
            var update = Builders<Account>.Update.Set(x => x.DiscordUser, ctx.Member.Username);
            if ((await DatabaseManager.AccountTable.UpdateOneAsync(x => x.AccountName == name, update)).IsAcknowledged)
            {
                await ctx.RespondAsync("Discord account was sucessfully linked with social account: " + name);
                return;
            }
            else
            {
                await ctx.RespondAsync("Something wrong has happend, please conatat a developer to aid you.");
                return;
            }
        }*/
    }
}
