using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace mtgvrp.core.Discord
{
    class RandomFunCommands
    {
        [Command("poll"), Description("Run a poll with reactions.")]
        public async Task Poll(CommandContext ctx, [Description("How long should the poll last.")] int duration, [Description("What options should people have.")] params DiscordEmoji[] options)
        {
            if (!ctx.Member.Roles.Any(x => x.Permissions.HasFlag(Permissions.ChangeNickname)))
            {
                return;
            }

            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivityModule();
            var poll_options = options;

            // then let's present the poll
            var embed = new DiscordEmbed
            {
                Title = "Poll time!",
                Description = string.Join(" ", poll_options.Select(x => x.ToString()))
            };
            var msg = await ctx.RespondAsync("", embed: embed);

            // add the options as reactions
            for (var i = 0; i < options.Length; i++)
            {
                await msg.CreateReactionAsync(options[i]);
                await Task.Delay(250);
            }

            // collect and filter responses
            var poll_result = await interactivity.CollectReactionsAsync(msg, TimeSpan.FromSeconds(duration));
            var results = poll_result.Where(xkvp => poll_options.Contains(xkvp.Key))
                .Select(xkvp => $"{xkvp.Key}: {xkvp.Value}");

            // and finally post the results
            await ctx.RespondAsync("And the poll results are: ");
            await ctx.RespondAsync(string.Join("\n", results));
        }

        [Command("waitforcode"), Description("Waits for a response containing a generated code.")]
        public async Task WaitForCode(CommandContext ctx)
        {
            if (!ctx.Member.Roles.Any(x => x.Permissions.HasFlag(Permissions.ChangeNickname)))
            {
                return;
            }

            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivityModule();

            // generate a code
            var codebytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(codebytes);

            var code = BitConverter.ToString(codebytes).ToLower().Replace("-", "");

            // announce the code
            await ctx.RespondAsync($"The first one to type the following code gets a reward: `{code}`");

            // wait for anyone who types it
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.Contains(code), TimeSpan.FromSeconds(60));

            if (msg != null)
            {
                // announce the winner
                await ctx.RespondAsync($"And the winner is: {msg.Author.Mention}");
            }
            else
            {
                await ctx.RespondAsync("Nobody? Really?");
            }
        }

        [Command("waitforreact"), Description("Waits for a reaction.")]
        public async Task WaitForReaction(CommandContext ctx)
        {
            if (!ctx.Member.Roles.Any(x => x.Permissions.HasFlag(Permissions.ChangeNickname)))
            {
                return;
            }

            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivityModule();

            // specify the emoji
            var emoji = DiscordEmoji.FromName(ctx.Client, ":point_up:");

            // announce
            await ctx.RespondAsync($"React with {emoji} to quote a message!");

            // wait for a reaction
            var em = await interactivity.WaitForReactionAsync(xe => xe.Id == 0 && xe.Name == emoji.Name, ctx.User, TimeSpan.FromSeconds(60));

            if (em != null)
            {
                // quote
                var embed = new DiscordEmbed
                {
                    Color = em.Author is DiscordMember m ? m.Color : 0xFF00FF,
                    Description = em.Content,
                    Author = new DiscordEmbedAuthor
                    {
                        Name = em.Author is DiscordMember mx ? mx.DisplayName : em.Author.Username,
                        Url = em.Author.AvatarUrl
                    }
                };
                await ctx.RespondAsync("", embed: embed);
            }
            else
            {
                await ctx.RespondAsync("Seriously?");
            }
        }

        [Command("waitfortyping"), Description("Waits for a typing indicator.")]
        public async Task WaitForTyping(CommandContext ctx)
        {
            if (!ctx.Member.Roles.Any(x => x.Permissions.HasFlag(Permissions.ChangeNickname)))
            {
                return;
            }

            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivityModule();

            // then wait for author's typing
            var chn = await interactivity.WaitForTypingChannelAsync(ctx.User, TimeSpan.FromSeconds(60));

            if (chn != null)
            {
                // got 'em
                await ctx.RespondAsync($"{ctx.User.Mention}, you typed in {chn.Mention}!");
            }
            else
            {
                await ctx.RespondAsync("*yawn*");
            }
        }
    }
}
