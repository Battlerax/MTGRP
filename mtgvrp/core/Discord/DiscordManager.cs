using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using GTANetworkAPI;
using mtgvrp.player_manager;

namespace mtgvrp.core.Discord
{
    public static class DiscordManager
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextModule Commands { get; set; }

        public static readonly string AdminChannel = "vrp-admins";
        public static readonly string VIPChannel = "vip-general";
        public static readonly string AdminRole = "V-RP Admin";
        public const ulong AdminChannelId = 331924706191998987;
        public const ulong VIPChannelId = 331965611573903363;

        public static void StartBot()
        {
            RunBotAsync().GetAwaiter().GetResult();
        }

        public static async Task RunBotAsync()
        {
            // Config
            var cfg = new DiscordConfiguration()
            {
                Token = "MzMyMTI5ODk3MTEzODQ1NzYw.DD5qYQ.xEfmKaGANPkmF8tc7Q3m9n1DVgs",
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            };

            // then we want to instantiate our client
            Client = new DiscordClient(cfg);

            // next, let's hook some events, so we know
            // what's going on
            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientError;

            // up next, let's set up our commands
            var ccfg = new CommandsNextConfiguration
            {
                // let's use the string prefix defined in config.json
                StringPrefix = "/",

                EnableDefaultHelp = false,

                // enable mentioning the bot as a command prefix
                EnableMentionPrefix = true
            };

            // and hook them up
            Commands = Client.UseCommandsNext(ccfg);

            Client.UseInteractivity(new InteractivityConfiguration());

            // let's hook some command events, so we know what's 
            // going on
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            // up next, let's register our commands
            Commands.RegisterCommands<Commands>();
            Commands.RegisterCommands<PlayerDiscordCommands>();
            Commands.RegisterCommands<RandomFunCommands>();

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
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "MTG-Bot", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "MTG-Bot", $"Guild available: {e.Guild.Name}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "MTG-Bot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "MTG-Bot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            // let's log the name of the guild that was just
            // sent to our client
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "MTG-Bot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);

            // let's check if the error is a result of lack
            // of required permissions
            if (e.Exception is ChecksFailedException)
            {
                // yes, the user lacks required permissions, 
                // let them know

                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                // let's wrap the response into an embed
                /*var embed = new DiscordEmbed()
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = 0xFF0000 // red
                };*/
                await e.Context.RespondAsync("", embed: null);
            }
        }

        public static void SendAdminMessage(string msg)
        {
            Client.SendMessageAsync(Client.GetChannelAsync(AdminChannelId).GetAwaiter().GetResult(), msg).GetAwaiter().GetResult();
        }

        public static void SendVIPMessage(string msg)
        {
            Client.SendMessageAsync(Client.GetChannelAsync(VIPChannelId).GetAwaiter().GetResult(), msg).GetAwaiter().GetResult();
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

            if (ctx.Member.Roles.Any(x => x.Name == DiscordManager.AdminRole || x.Name == "V-RP Developer"))
            {
                foreach (var c in API.Shared.GetAllPlayers())
                {
                    if (c == null)
                        continue;

                    Account receiverAccount = c.GetAccount();

                    if (receiverAccount?.AdminLevel > 0)
                    {
                        API.Shared.SendChatMessageToPlayer(c, Color.AdminChat, "[Discord A] " + ctx.Member.DisplayName + ": " + ctx.RawArgumentString);
                    }
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("igplayers")] // let's define this method as a command
        [Description("Shgows list of IG players.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task GetPlayers(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != DiscordManager.AdminChannel)
                return;

            var players = "";
            if (ctx.Member.Roles.Any(x => x.Name == DiscordManager.AdminRole || x.Name == "V-RP Developer"))
            {
                foreach (var c in PlayerManager.Players)
                {
                    if (c == null)
                        continue;

                    Account receiverAccount = c.Client.GetAccount();

                    players += $"[{PlayerManager.GetPlayerId(c)}] {c.rp_name()} - {receiverAccount.AccountName}" + "\n";
                }
                var interactivity = ctx.Client.GetInteractivityModule();
                var playersPages = interactivity.GeneratePagesInEmbeds(players);
                await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, playersPages, TimeSpan.FromMinutes(5), TimeoutBehaviour.Delete);
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("v")] // let's define this method as a command
        [Description(
            "Sends a message in VIP channel.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task VIPChannel(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != DiscordManager.VIPChannel)
                return;

            var players = API.Shared.GetAllPlayers();
            foreach (var p in players)
            {
                if(p == null)
                    continue;

                Account pAccount = p.GetAccount();
                if (pAccount?.VipLevel > 0)
                {
                    API.Shared.SendChatMessageToPlayer(p, Color.VipChat, "[Discord V] " + ctx.Member.DisplayName + ": " + ctx.RawArgumentString);
                }
            }
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
        }

        [DSharpPlus.CommandsNext.Attributes.Command("players")] // let's define this method as a command
        [Description(
            "Views players online.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task PlayersCount(CommandContext ctx) // this command takes no arguments
        {
            if(!ctx.Channel.IsPrivate)
                return;

            await ctx.TriggerTypingAsync();
            int count = PlayerManager.Players.SkipWhile(x => x == null).Count();
            var msg = "Players Online: " + count;
            await ctx.RespondAsync(msg);
        }

        [DSharpPlus.CommandsNext.Attributes.Command("admins")] // let's define this method as a command
        [Description(
            "Views admins online.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task AdminsList(CommandContext ctx) // this command takes no arguments
        {
            if(!ctx.Channel.IsPrivate)
                return;

            await ctx.TriggerTypingAsync();

            var msg = "";
            foreach (var c in API.Shared.GetAllPlayers())
            {
                Account receiverAccount = c.GetAccount();
                if(receiverAccount == null)
                    continue;

                if (receiverAccount.AdminLevel < 1) continue;

                msg += receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel + " | " +
                       (receiverAccount.AdminDuty ? "**On Duty**" : "Off Duty") + "\n";
            }
            /*var embed = new DiscordEmbed
            {
                Title = "Admins Online",
                Description = (msg == "" ? "None" : msg),
                Color = 0x00FF00 // green
            };*/
            await ctx.RespondAsync("", embed: null);
        }


        [DSharpPlus.CommandsNext.Attributes.Command("setgame")] // let's define this method as a command
        [Description("Set the game status..")] // this will be displayed to tell users what this command does when they invoke help
        public async Task ChangeGame(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != DiscordManager.AdminChannel)
                return;

            if (ctx.Member.Roles.Any(x => x.Name == DiscordManager.AdminRole))
            {
                await DiscordManager.Client.UpdateStatusAsync(null/*new Game(ctx.RawArgumentString)*/);
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("sendig")] // let's define this method as a command
        [Description("Send message IG.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task SendIG(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != "vrp-development")
                return;

            if (ctx.Member.Roles.Any(x => x.Name == "V-RP Developer"))
            {
                foreach (var player in API.Shared.GetAllPlayers())
                {
                    if(player == null)
                        continue;
                    
                    API.Shared.SendChatMessageToPlayer(player, "~r~" + ctx.RawArgumentString);
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
            }
        }

        [DSharpPlus.CommandsNext.Attributes.Command("saveandclose")] // let's define this method as a command
        [Description("Save all characters, kick em and close.")] // this will be displayed to tell users what this command does when they invoke help
        public async Task SaveAll(CommandContext ctx) // this command takes no arguments
        {
            if (ctx.Channel.Name != "vrp-development")
                return;

            if (ctx.Member.Roles.Any(x => x.Name == "V-RP Developer"))
            {
                foreach (var player in API.Shared.GetAllPlayers())
                {
                    if (player == null)
                        continue;

                    var character = player.GetCharacter();
                    if (character == null)
                        continue;

                    character.Save();
                    character.Client?.Kick(ctx.RawArgumentString);
                }
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(DiscordManager.Client, ":white_check_mark:"));
            }
        }
    }
}
