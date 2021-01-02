using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Server.Extensions.Weather;
using TwitchLib.Api.V5.Models.Users;

namespace DiscordBot.Commands
{
    public class DiscordCommands
    {
        [DSharpPlus.CommandsNext.Attributes.Command("ping")]
        public async Task DiscordCommandPing(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Pong! {ctx.User.Mention}");

            await Task.CompletedTask;
        }

        [Command("random")]
        public async Task DiscordCommandRandom(CommandContext ctx, int min, int max)
        {
            Random rnd = new Random();
            await ctx.RespondAsync($"{ctx.User.Mention} Your number is: {rnd.Next(min, max)}");

            await Task.CompletedTask;
        }

        [Command("newplayer"), Aliases("np")]
        public async Task DiscordCommandNewPlayer(CommandContext ctx, DiscordUser mentionedUser = null)
        {
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.Title = "Welcome";
            embedBuilder.ThumbnailUrl = "http://paradigmroleplay.com/img/logo.png";
            embedBuilder.Timestamp = DateTime.Now;
            embedBuilder.Color = DiscordColor.DarkBlue;

            DateTime dateTime = DateTime.Now;

            embedBuilder.Description = mentionedUser != null
                ? $"Welcome to Paradigm Roleplay {mentionedUser.Mention}, We are proud to have you with us! Our server is under a restructure!"
                : $"Welcome to Paradigm Roleplay, We are proud to have you with us! Our server is under a restructure!";

            embedBuilder.AddField("Forums", "https://forum.paradigmroleplay.com");
            embedBuilder.AddField("Website", "https://paradigmroleplay.com");

            if (mentionedUser != null)
            {
                await ctx.RespondAsync(mentionedUser.Mention);
            }

            await ctx.RespondAsync(null, false, embedBuilder);

            await Task.CompletedTask;
        }

        [Command("myid")]
        public async Task DiscordCommandMyId(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();

            await ctx.Member.SendMessageAsync($"Your Discord ID is {ctx.Member.Id}");

            await Task.CompletedTask;
        }

        [Command("player"), Aliases("players")]
        public async Task DiscordCommandOnlinePlayers(CommandContext ctx)
        {
            if (ctx.Channel.Id != 704015413729689700) return;
            
            await ctx.TriggerTypingAsync();



            int playerCount = await SignalR.FetchOnlinePlayerCount();

            await ctx.RespondAsync($"Current Player Count: {playerCount}");
        }

        [Command("bug")]
        public async Task DiscordCommandBug(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Bugs can be reported on our forums over at https://forum.paradigmroleplay.com/index.php?forums/bug-reports.34/");

            await Task.CompletedTask;
        }

        [Command("activity")]
        public async Task DiscordActivityCommand(CommandContext ctx)
        {
            if (ctx.Channel.Id != 704015413729689700)
            {
                await ctx.Message.DeleteAsync("Incorrect Channel");
                return;
            }

            await ctx.TriggerTypingAsync();

            if (!ServerHandler.GameActivityList.Any())
            {
                await ctx.RespondAsync("There has been no activity!");
                return;
            }

            await ctx.RespondAsync($"Fetching the last {ServerHandler.GameActivityList.Count} Game Server Activities!");

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

            await ctx.RespondAsync(message);
        }

        [Command("weather"), Aliases("time")]
        public async Task DiscordCommandShowWeather(CommandContext ctx)
        {
            try
            {
                await ctx.TriggerTypingAsync();

                OpenWeather currentWeather = WeatherUpdate.CurrentWeather;

                if (currentWeather == null)
                {
                    await ctx.RespondAsync($"Unable to fetch latest weather! Sorry {ctx.User.Mention}");
                    return;
                }

                int[] serverTime = await SignalR.FetchTime();

                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById("UTC");

                DateTimeOffset localServerTime = DateTimeOffset.Now;

                DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

                CultureInfo cultureInfo   = Thread.CurrentThread.CurrentCulture;
                TextInfo textInfo = cultureInfo.TextInfo;

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Author = null,
                    Color = DiscordColor.Cyan,
                    Description = $"Current Weather: {textInfo.ToTitleCase(currentWeather.weather.FirstOrDefault().description)}",
                    ThumbnailUrl =  $"https://openweathermap.org/img/wn/{currentWeather.weather.FirstOrDefault()?.icon}.png",
                    Timestamp = DateTimeOffset.Now,
                    Title = "Current Weather for Los Santos",
                };

                embedBuilder.AddField("Temperature",
                    $"{Math.Round(currentWeather.main.temp)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp))} °F", true);
            
                embedBuilder.AddField("High Temperature",
                    $"{Math.Round(currentWeather.main.temp_max)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp_max))} °F", true);
            
                embedBuilder.AddField("Low Temperature",
                    $"{Math.Round(currentWeather.main.temp_min)} °C - {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(currentWeather.main.temp_min))} °F", true);

                embedBuilder.AddField("Humidity", $"{currentWeather.main.humidity} %RH", true);

                embedBuilder.AddField("Visibility", $"{currentWeather.visibility} meters", true);

                embedBuilder.AddField("Time", $"{serverTime[0]:D2}:{serverTime[1]:D2} (({localTime.Hour:D2}:{localTime.Minute:D2}))", true);

                await ctx.RespondAsync(embed: embedBuilder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
/*
        [Command("removeplayerrole")]
        public async Task DiscordCommandRemovePlayerRole(CommandContext ctx)
        {
            if (ctx.Channel.Id == 703688837641273485)
            {
                int count = 0;

                List<DiscordMember> members = ctx.Guild.Members.ToList();

                foreach (DiscordMember discordMember in members)
                {
                    if (discordMember == null) continue;

                    await discordMember.RevokeRoleAsync(ctx.Guild.GetRole(665692452086218754));
                    count++;
                }

                await ctx.RespondAsync($"Removed {count} members from the players role.");


            }
        }
*/
        [Command("uptime")]
        public async Task DiscordCommandUptime(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            DateTime startTime = await SignalR.FetchServerStartTime();

            DateTime now = DateTime.Now;

            TimeSpan upTime = now - startTime;

            Console.WriteLine($"Uptime: {upTime}");

            await ctx.RespondAsync(
                $"Server Up Time: {upTime.Days:D1} Days, {upTime.Hours:D1} Hours, {upTime.Minutes:D1} Minutes!");
        }

        [Command("wipe"), Description("Wipes all messages"), Hidden]
        [RequirePermissions(Permissions.Administrator)]
        public async Task WipeCommand(CommandContext ctx, int wipeCount = 0)
        {
            if (wipeCount == 0)
            {
                int count = 0;

                foreach (DiscordMessage discordMessage in await ctx.Channel.GetMessagesAsync())
                {
                    await ctx.Channel.DeleteMessageAsync(discordMessage);
                    count++;
                }
                await Program.MainGuildLogChannel.SendMessageAsync(
                    $"{ctx.User.Mention} has deleted {count} messages from {ctx.Channel.Mention}.");

                return;
            }

            IReadOnlyList<DiscordMessage> messages = await ctx.Channel.GetMessagesAsync();

            List<DiscordMessage> messageList = messages.Skip(Math.Max(0, messages.Count - wipeCount)).ToList();

            int totalWiped = 0;
            
            foreach (DiscordMessage discordMessage in messageList)
            {
                await ctx.Channel.DeleteMessageAsync(discordMessage);
                totalWiped++;
            }

            await Program.MainGuildLogChannel.SendMessageAsync(
                $"{ctx.Member.Username} has wiped the last {totalWiped / wipeCount} messages from {ctx.Channel.Mention}.");
            
        }

        [Command("message"), Description("Messages a player from the report"), Hidden]
        public async Task ReportMessage(CommandContext ctx)
        {
            if (ctx.Channel.Parent != await Program._discord.GetChannelAsync(704011246071971972))
            {
                await ctx.Message.DeleteAsync();
                return;
            }

            string[] channelSplit = ctx.Channel.Name.Split("-");

            bool tryParse = int.TryParse(channelSplit[1], out int channelId);

            if (!tryParse)
            {
                Console.WriteLine($"An error occurred parsing the report channel name: {ctx.Channel.Name}");
                return;
            }

            string[] messageSplit = ctx.Message.Content.Split("?message");
            
            SignalR.SendMessageToReportPlayer(channelId, messageSplit[1]);
        }

        [Command("cr"), Description("Used to close a report"), Hidden]
        public async Task CloseReport(CommandContext ctx)
        {
            
            if (ctx.Channel.Parent != await Program._discord.GetChannelAsync(704011246071971972))
            {
                await ctx.Message.DeleteAsync();
                return;
            }

            string[] channelSplit = ctx.Channel.Name.Split("-");

            bool tryParse = int.TryParse(channelSplit[1], out int channelId);

            if (!tryParse)
            {
                Console.WriteLine($"An error occurred parsing the report channel name: {ctx.Channel.Name}");
                return;
            }

            SignalR.SendMessageToReportPlayer(channelId, "Your report has been closed.");

            GameReportHandler.ClearReport(channelId);
            
            await ctx.Channel.DeleteAsync($"Report Closed by {ctx.User.Username}");
        }

        [Command("post"), RequirePermissions(Permissions.Administrator), Hidden]
        public async Task PostMessageCommand(CommandContext ctx, DiscordChannel channel, [RemainingText] string message)
        {
            if (ctx.Guild != Program.MainGuild) return;
            
            DiscordMessage discordMessage = await channel.SendMessageAsync(message);

            await Program.MainGuildLogChannel.SendMessageAsync(
                $"{ctx.User.Username} has posted a message as the bot in {channel.Mention}.");
        }

        [Command("masspm"), RequirePermissions(Permissions.Administrator), Hidden]
        public async Task MassPmCommand(CommandContext ctx, [RemainingText] string message)
        {
            if (ctx.Guild != Program.MainGuild)
            {
                await ctx.Message.DeleteAsync();
                return;
            }

            await ctx.TriggerTypingAsync();

            await ctx.RespondAsync($"Sending the mass PM out!");

            await ctx.TriggerTypingAsync();

            int count = 0;
            
            foreach (DiscordMember discordMember in ctx.Guild.Members)
            {
                if(discordMember.IsBot) continue;
                
                Console.WriteLine($"Sending Message to {discordMember.Username}");

                DiscordDmChannel dmChannel = await Program._discord.CreateDmAsync(discordMember);

                if (dmChannel == null) continue;

                DiscordMessage discordMessage =  await dmChannel.SendMessageAsync(message);

                if (discordMessage == null) continue;
                count++;
                
                Console.WriteLine($"Message sent to {discordMember.Username}");

            }

            await ctx.RespondAsync($"Send the PM to {count}/{ctx.Guild.MemberCount} Members.");
        }
    }
}