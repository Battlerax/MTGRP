using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.IO;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using GTANetworkServer;
using mtgvrp.player_manager;

namespace mtgvrp.core
{
    public static class DiscordManager
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextModule Commands { get; set; }

        public static readonly string AdminChannel = "lobby";

        public static void StartBot()
        {
            RunBotAsync().GetAwaiter().GetResult();
        }

        public static async Task RunBotAsync()
        {
            // Config
            var cfg = new DiscordConfig
            {
                Token = "MzMyMTI5ODk3MTEzODQ1NzYw.DD5qYQ.xEfmKaGANPkmF8tc7Q3m9n1DVgs",
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };

            // then we want to instantiate our client
            Client = new DiscordClient(cfg);

            Client.SetWebSocketClient<WebSocketSharpClient>();

            // next, let's hook some events, so we know
            // what's going on
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientError += Client_ClientError;

            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefix = "/",

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            Commands = Client.UseCommandsNext(ccfg);

            // let's hook some command events, so we know what's 
            // going on
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            // up next, let's register our commands
            Commands.RegisterCommands<Commands>();

            // finnaly, let's connect and log in
            await Client.ConnectAsync();

            // when the bot is running, try doing <prefix>help
            // to see the list of registered commands, and 
            // <prefix>help <command> to see help about specific
            // command.

            // and this is to prevent premature quitting
            await Task.Delay(-1);
        }

        private static Task Client_Ready(ReadyEventArgs e)
        {
            // let's log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"Guild available: {e.Guild.Name}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Commands_CommandExecuted(CommandExecutedEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                var embed = new DiscordEmbed
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = 0xFF0000 // red
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }

        public static void SendAdminMessage(string msg)
        {
            Client.SendMessageAsync(Client.GetChannelAsync(331922156126732290).GetAwaiter().GetResult(), msg).GetAwaiter().GetResult();
        }
    }


    public class Commands
    {
        [DSharpPlus.CommandsNext.Attributes.Command("a")] // let's define this method as a command
        [Description("Sends a message in admin channel.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task AdminChat(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != DiscordManager.AdminChannel)
                return;

            if (ctx.Member.Roles.Any(x => x.Name == "V-RP Developer"))
            {
                foreach (var c in API.shared.getAllPlayers())
                {
                    Account receiverAccount = c.GetAccount();

                    if (receiverAccount.AdminLevel > 0)
                    {
                        API.shared.sendChatMessageToPlayer(c, Color.AdminChat, "[Discord A] " + ctx.Member.DisplayName + ": " + ctx.RawArgumentString);
                    }
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("admins")] // let's define this method as a command
        [Description("Views admins online.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task AdminsList(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != DiscordManager.AdminChannel)
                return;

            if (ctx.Member.Roles.Any(x => x.Name == "V-RP Developer"))
            {
                await ctx.TriggerTypingAsync();

                var msg = "=====ADMINS ONLINE NOW=====\n";
                foreach (var c in API.shared.getAllPlayers())
                {
                    Account receiverAccount = c.GetAccount();

                    if (receiverAccount.AdminLevel <= 1) continue;

                    msg += "* " + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel + " | " + (receiverAccount.AdminDuty ? "**On Duty**" : "Off Duty") + "\n";
                }
                await ctx.RespondAsync(msg + "===========================");
            }
        }
    }
}
