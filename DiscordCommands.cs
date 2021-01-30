using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Server.Extensions.Weather;
using TwitchLib.Api.V5.Models.Users;

namespace DiscordBot.Commands
{
    public class DiscordCommands : ModuleBase<SocketCommandContext>
    {
        public PictureService PictureService { get; set; }

        [Command("ping")]
        [Alias("pong", "hello")]
        [RequireContext(ContextType.Guild)]
        public Task PingAsync() => ReplyAsync("pong!");

        [Command("random")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandRandom(int min, int max)
        {
            Random rnd = new Random();

            await ReplyAsync($"{Context.User.Mention} Your number is: {rnd.Next(min, max)}");
        }

        [Command("newplayer"), Alias("np")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandNewPlayer(IGuildUser mentionedUser = null)
        {
            await Context.Message.DeleteAsync();
            await Context.Channel.TriggerTypingAsync();

            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "Welcome",
                ThumbnailUrl = Program.LogoUri,
                Timestamp = DateTime.Now,
                Color = Color.DarkBlue
            };

            embedBuilder.Description = mentionedUser != null
                ? $"Welcome to Southland Roleplay {mentionedUser.Mention}, We are proud to have you with us! Our server is online and accepting applications!"
                : $"Welcome to Southland Roleplay, We are proud to have you with us! Our server is online and accepting applications!";

            embedBuilder.AddField("Forums", "https://forum.sol-rp.com");
            embedBuilder.AddField("Website", "https://sol-rp.com");

            if (mentionedUser != null)
            {
                await ReplyAsync(mentionedUser.Mention);
            }

            await ReplyAsync(null, false, embedBuilder.Build());
        }

        [Command("myid")]
        [RequireContext(ContextType.Guild)]
        public Task DiscordCommandMyId() => ReplyAsync($"{Context.User.Mention} Your ID is {Context.User.Id}");

        [Command("player"), Alias("players")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandOnlinePlayers()
        {
            if (Context.Channel.Id != 802384633378635788) return;

            await Context.Channel.TriggerTypingAsync();

            int playerCount = await SignalR.FetchOnlinePlayerCount();

            await ReplyAsync($"Current Player Count: {playerCount}");
        }

        [Command("bug")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandBug()
        {
            await Context.Channel.TriggerTypingAsync();

            await ReplyAsync(
                $"Bugs can be reported on our forums over at https://forum.sol-rp.com/threads/bug-report-format.11/unread");
        }

        [Command("activity")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordActivityCommand()
        {
            if (Context.Channel.Id != 802384633378635788)
            {
                await Context.Message.DeleteAsync();
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            if (!ServerHandler.GameActivityList.Any())
            {
                await ReplyAsync("There has been no activity!");
                return;
            }

            await ReplyAsync($"Fetching the last {ServerHandler.GameActivityList.Count} Game Server Activities!");

            string message = "";

            foreach (GameActivity gameActivity in ServerHandler.GameActivityList)
            {
                if (gameActivity.ActivityType == GameActivityType.Login)
                {
                    message = $"{message}\n{gameActivity.Username} has logged in at {gameActivity.DateTime}.";
                }

                if (gameActivity.ActivityType == GameActivityType.Logout)
                {
                    message = $"{message}\n{gameActivity.Username} has logged out at {gameActivity.DateTime}.";
                }
            }

            await ReplyAsync(message);
        }

        [Command("weather"), Alias("time")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandShowWeather()
        {
            try
            {
                await Context.Channel.TriggerTypingAsync();

                OpenWeather currentWeather = WeatherUpdate.CurrentWeather;

                if (currentWeather == null)
                {
                    await ReplyAsync($"Unable to fetch latest weather! Sorry {Context.User.Mention}");
                    return;
                }

                int[] serverTime = await SignalR.FetchTime();

                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("UTC");

                DateTimeOffset localServerTime = DateTimeOffset.Now;

                DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;

                EmbedBuilder embedBuilder = new EmbedBuilder
                {
                    Author = null,
                    Color = Color.Blue,
                    Description =
                        $"Current Weather: {textInfo.ToTitleCase(currentWeather.weather.FirstOrDefault().description)}",
                    ThumbnailUrl =
                        $"https://openweathermap.org/img/wn/{currentWeather.weather.FirstOrDefault()?.icon}.png",
                    Timestamp = DateTimeOffset.Now,
                    Title = "Current Weather for Los Santos",
                };

                embedBuilder.AddField("Temperature",
                    $"{Math.Round(currentWeather.main.temp)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp))} °F",
                    true);

                embedBuilder.AddField("High Temperature",
                    $"{Math.Round(currentWeather.main.temp_max)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp_max))} °F",
                    true);

                embedBuilder.AddField("Low Temperature",
                    $"{Math.Round(currentWeather.main.temp_min)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp_min))} °F",
                    true);

                embedBuilder.AddField("Humidity", $"{currentWeather.main.humidity} %RH", true);

                embedBuilder.AddField("Visibility", $"{currentWeather.visibility} meters", true);

                embedBuilder.AddField("Time",
                    $"{serverTime[0]:D2}:{serverTime[1]:D2} (({localTime.Hour:D2}:{localTime.Minute:D2}))", true);

                await ReplyAsync(embed: embedBuilder.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /*
                [Command("removeplayerrole")]
                public async Task DiscordCommandRemovePlayerRole(CommandContext Context)
                {
                    if (Context.Channel.Id == 703688837641273485)
                    {
                        int count = 0;

                        List<DiscordMember> members = Context.Guild.Members.ToList();

                        foreach (DiscordMember discordMember in members)
                        {
                            if (discordMember == null) continue;

                            await discordMember.RevokeRoleAsync(Context.Guild.GetRole(665692452086218754));
                            count++;
                        }

                        await ReplyAsync($"Removed {count} members from the players role.");
                    }
                }
        */

        [Command("uptime")]
        [RequireContext(ContextType.Guild)]
        public async Task DiscordCommandUptime()
        {
            await Context.Channel.TriggerTypingAsync();

            DateTime startTime = await SignalR.FetchServerStartTime();

            DateTime now = DateTime.Now;

            TimeSpan upTime = now - startTime;

            Console.WriteLine($"Uptime: {upTime}");

            await ReplyAsync(
                $"Server Up Time: {upTime.Days:D1} Days, {upTime.Hours:D1} Hours, {upTime.Minutes:D1} Minutes!");
        }

        [Command("wipe"), Description("Wipes all messages")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task WipeCommand(int wipeCount = 0)
        {
            if (wipeCount == 0)
            {
                int count = 0;

                foreach (SocketMessage cachedMessage in Context.Channel.GetCachedMessages())
                {
                    await cachedMessage.DeleteAsync();
                    count++;
                }

                await Program.MainGuildLogChannel.SendMessageAsync(
                    $"{Context.User.Mention} has deleted {count} messages from {Context.Channel.Name}.");

                return;
            }

            var messages = Context.Channel.GetCachedMessages().ToList();

            var messageList = messages.SkipLast(wipeCount);

            int totalWiped = 0;

            foreach (var discordMessage in messageList)
            {
                await Context.Channel.DeleteMessageAsync(discordMessage);
                totalWiped++;
            }

            await Program.MainGuildLogChannel.SendMessageAsync(
                $"{Context.User.Username} has wiped the last {totalWiped / wipeCount} messages from {Context.Channel.Name}.");
        }

        [Command("message"), Description("Messages a player from the report")]
        [RequireContext(ContextType.Guild)]
        public async Task ReportMessage([Remainder] string messageText)
        {
            try
            {
                SocketCategoryChannel categoryChannel = Context.Guild.GetCategoryChannel(795085207672848396);

                if (categoryChannel.Channels.All(x => x.Id != Context.Channel.Id))
                {
                    await Context.Message.DeleteAsync();
                    return;
                }

                string[] channelSplit = Context.Channel.Name.Split("-");

                bool tryParse = int.TryParse(channelSplit[1], out int channelId);

                if (!tryParse)
                {
                    Console.WriteLine($"An error occurred parsing the report channel name: {Context.Channel.Name}");
                    return;
                }

                SignalR.SendMessageToReportPlayer(channelId, messageText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("cr"), Description("Used to close a report")]
        [RequireContext(ContextType.Guild)]
        public async Task CloseReport()
        {
            SocketCategoryChannel categoryChannel = Context.Guild.GetCategoryChannel(795085207672848396);

            if (categoryChannel.Channels.All(x => x.Id != Context.Channel.Id))
            {
                await Context.Message.DeleteAsync();
                return;
            }

            string[] channelSplit = Context.Channel.Name.Split("-");

            bool tryParse = int.TryParse(channelSplit[1], out int channelId);

            if (!tryParse)
            {
                Console.WriteLine($"An error occurred parsing the report channel name: {Context.Channel.Name}");
                return;
            }

            SignalR.SendMessageToReportPlayer(channelId, "Your report has been closed.");

            GameReportHandler.ClearReport(channelId);

            Context.Guild.GetTextChannel(Context.Channel.Id)?.DeleteAsync();
        }

        [Command("post")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PostMessageCommand(ITextChannel channel, [Remainder] string message)
        {
            if (Context.Guild != Program.MainGuild) return;

            await channel.SendMessageAsync(message);

            await Program.MainGuildLogChannel.SendMessageAsync(
                $"{Context.User.Username} has posted a message as the bot in {channel.Name}.");
        }

        [Command("mute")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task MuteMemberCommand(IGuildUser member, [Remainder] string reason = "")
        {
            await Context.Message.DeleteAsync();

            SocketRole mutedRole = Context.Guild.GetRole(796163891783663617);

            SocketGuildUser adminUser = (SocketGuildUser)Context.User;

            string adminNick = !string.IsNullOrEmpty(adminUser.Nickname) ? adminUser.Nickname : adminUser.Username;

            string nick = !string.IsNullOrEmpty(member.Nickname) ? member.Nickname : member.Username;

            bool containsMutedRole = member.RoleIds.Any(x => x == 796163891783663617);

            if (containsMutedRole)
            {
                await member.RemoveRoleAsync(mutedRole);
                await Program.MainGuildLogChannel.SendMessageAsync($"{adminNick} has un-muted {nick}.");
                return;
            }

            await member.AddRoleAsync(mutedRole);
            await Program.MainGuildLogChannel.SendMessageAsync($"{adminNick} has muted {nick}. Reason: {reason}");
            await member.SendMessageAsync(
                $"You've been muted in the {Context.Guild.Name} Discord. Reasoning behind this is: {reason}");
        }

        [Command("clearreports")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task ClearReports()
        {
            if (Context.Guild != Program.MainGuild) return;

            GameReportHandler.ClearReportChannels();
        }
    }
}