using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    internal class Program
    {
        public static DiscordClient _discord;

        public static string LogoUrl = "https://forum.sol-rp.com/styles/default/xenforo/southlandrp.png";

        #region DiscordGuilds

        private static readonly ulong _mainGuildId = 748851074135490560;
        public static DiscordGuild MainGuild;

        private static readonly ulong _emergencyGuildId = 797216050429034516;
        private static DiscordGuild _emergencyGuild;

        /*
        private static readonly ulong _govGuildId = 692782773903294566;
        private static DiscordGuild _govGuild;
        */

        #endregion DiscordGuilds

        #region MainGuildChannels

        private static readonly ulong _mainGuildLogChannelId = 795062350398881832;
        public static DiscordChannel MainGuildLogChannel;

        private static readonly ulong _mainGuildJoinChannelId = 748851942075269201;
        private static DiscordChannel _mainGuildJoinChannel;

        #endregion MainGuildChannels

        #region EmergencyGuildChannels

        private static readonly ulong _emergencyGuildLogChannelId = 798259039062196275;
        private static DiscordChannel _emergencyGuildLogChannel;

        #endregion EmergencyGuildChannels

        /*

        #region Gov Guild Channels

        private static readonly ulong _govLogChannelId = 695134603228217344;
        private static DiscordChannel _govLogChannel;

        #endregion Gov Guild Channels

        */

        private static CommandsNextExtension _commandsNext;

        private static List<string> bannedWords = new List<string>
        {
            "gta.world",
        };

        private static async Task Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif

                Console.WriteLine("Starting Discord Bot");

                Console.Title = "Southland Roleplay Discord Bot";

                WeatherUpdate.InitWeatherUpdate();
                SignalR.StartConnection();

                _discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = Settings.Default.Token,
                    TokenType = TokenType.Bot,
                    MinimumLogLevel = LogLevel.Error,
                    AutoReconnect = true,
                });

                MainGuild = await _discord.GetGuildAsync(_mainGuildId);
                _emergencyGuild = await _discord.GetGuildAsync(_emergencyGuildId);
                //_govGuild = await _discord.GetGuildAsync(_govGuildId);

                if (MainGuild != null)
                {
                    Console.WriteLine($"Connected to {MainGuild.Name} Discord Guild.");
                }

                if (_emergencyGuild != null)
                {
                    Console.WriteLine($"Connected to {_emergencyGuild.Name} Discord Guild");
                }
                /*
                if (_govGuild != null)
                {
                    Console.WriteLine($"Connected to {_govGuild.Name} Discord Guild");
                }
                */

                #region Main Guild Channels

                MainGuildLogChannel = await _discord.GetChannelAsync(_mainGuildLogChannelId);
                _mainGuildJoinChannel = await _discord.GetChannelAsync(_mainGuildJoinChannelId);

                #endregion Main Guild Channels

                #region Emergency Guild Channels

                if (_emergencyGuild != null)
                {
                    _emergencyGuildLogChannel = await _discord.GetChannelAsync(_emergencyGuildLogChannelId);
                }

                #endregion Emergency Guild Channels

                /*

                #region Gov Guild Channels

                if (_govGuild != null)
                {
                    _govLogChannel = await _discord.GetChannelAsync(_govLogChannelId);
                }

                #endregion Gov Guild Channels

                */

                #region Events

                #region Member Added

                _discord.GuildMemberAdded += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        if (e.Guild == MainGuild)
                        {
                            // If entered main guild

                            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                            {
                                Title = "New User",
                                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = LogoUrl },
                                Timestamp = DateTimeOffset.Now,
                                Color = DiscordColor.SpringGreen,
                                Description = $"Look out! {e.Member.Username} has joined {e.Guild.Name}!"
                            };

                            await _mainGuildJoinChannel.SendMessageAsync(null, false, embedBuilder);

                            await e.Member.SendMessageAsync(
                                $"Welcome to Southland Roleplay! For any information on using the bot please use ?help in one of the server channels!\n" +
                                $"Don't forget to read and react to the rules channel to get full access to the discord!");

                            return;
                        }
                    });
                };

                #endregion Member Added

                #region Member Removed

                _discord.GuildMemberRemoved += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        if (e.Guild == MainGuild)
                        {
                            await MainGuildLogChannel.SendMessageAsync(
                                $"{e.Member.Nickname} ({e.Member.Username}) has left the guild");

                            await e.Member.SendMessageAsync(
                                $"Sorry to see you go! Please tell us how we can improve on the forums. https://forum.sol-rp.com");
                        }
                    });
                };

                #endregion Member Removed

                #region Member Updated

                _discord.GuildMemberUpdated += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        if (e.Guild == MainGuild)
                        {
                            if (e.NicknameAfter != e.NicknameBefore)
                            {
                                if (!string.IsNullOrEmpty(e.NicknameBefore) && !string.IsNullOrEmpty(e.NicknameAfter))
                                {
                                    await MainGuildLogChannel.SendMessageAsync(
                                        $"User {e.NicknameBefore} has updated their nickname to {e.NicknameAfter}");
                                    return;
                                }

                                if (string.IsNullOrEmpty(e.NicknameBefore))
                                {
                                    await MainGuildLogChannel.SendMessageAsync(
                                        $"User {e.Member.Username} has updated their nickname to {e.NicknameAfter}");
                                    return;
                                }

                                if (string.IsNullOrEmpty(e.NicknameAfter))
                                {
                                    await MainGuildLogChannel.SendMessageAsync(
                                        $"User {e.NicknameBefore} has updated their nickname to {e.Member.Username}");
                                    return;
                                }
                            }

                            if (!Equals(e.RolesBefore, e.RolesAfter))
                            {
                                if (e.RolesAfter.Count > e.RolesBefore.Count)
                                {
                                    // Have more roles, so they've gained.

                                    foreach (DiscordRole discordRole in e.RolesAfter)
                                    {
                                        // Loop through roles since change

                                        // Previous role exists so skip
                                        if (e.RolesBefore.Contains(discordRole)) continue;

                                        string nickName = e.Member.Nickname;

                                        if (string.IsNullOrEmpty(nickName))
                                        {
                                            nickName = e.Member.Username;
                                        }

                                        await MainGuildLogChannel.SendMessageAsync(
                                            $"{nickName} has been added to the {discordRole.Name} role.");
                                    }

                                    return;
                                }

                                if (e.RolesAfter.Count < e.RolesBefore.Count)
                                {
                                    // Have less roles than before, so they've lost
                                    foreach (DiscordRole discordRole in e.RolesBefore)
                                    {
                                        // Loop through roles since change

                                        // Previous role exists so skip
                                        if (e.RolesAfter.Contains(discordRole)) continue;

                                        string nickName = e.Member.Nickname;

                                        if (string.IsNullOrEmpty(nickName))
                                        {
                                            nickName = e.Member.Username;
                                        }

                                        await MainGuildLogChannel.SendMessageAsync(
                                            $"{nickName} has been removed from the {discordRole.Name} role.");

                                        if (discordRole.Id == 704015916047794298 || discordRole.Id == 704015916047794298)
                                        {
                                            // Removed from Donators

                                            // Remove Red Emoji
                                            /*await MainGuild.RevokeRoleAsync(e.Member, MainGuild.GetRole(697477618022219778), "No longer wanted the color!");
                                            await MainGuild.RevokeRoleAsync(e.Member, MainGuild.GetRole(697477763791192094),
                                                "No longer wanted the color!");
                                            await MainGuild.RevokeRoleAsync(e.Member, MainGuild.GetRole(697478876137521282),
                                                "No longer wanted the color!");
                                            await MainGuild.RevokeRoleAsync(e.Member, MainGuild.GetRole(697479141594759188),"Change of color");
                                            */
                                        }
                                    }

                                    return;
                                }
                            }
                        }
                    });
                };

                #endregion Member Updated

                #region Message Created

                _discord.MessageCreated += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        bool contains = false;

                        foreach (string bannedWord in bannedWords)
                        {
                            if (e.Message.Content.ToLower().Contains(bannedWord.ToLower()))
                            {
                                contains = true;
                            }
                        }

                        if (contains && !e.Author.IsBot)
                        {
                            await e.Message.DeleteAsync();

                            if (e.Guild == MainGuild)
                            {
                                await MainGuildLogChannel.SendMessageAsync(
                                    $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                            }
                            if (e.Guild == _emergencyGuild)
                            {
                                await _emergencyGuildLogChannel.SendMessageAsync(
                                    $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                            }/*
                    if (e.Guild == _govGuild)
                    {
                        await _govLogChannel.SendMessageAsync(
                            $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                    }*/
                        }

                        if (e.Channel.Id == 787791455950471218)
                        {
                            if (e.Author.IsBot) return;

                            if (e.Message.Content.StartsWith('?')) return;

                            if (e.Message.Content.StartsWith('<')) return;

                            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);

                            string nickName = string.IsNullOrEmpty(member.DisplayName) ? member.Username : member
                                .DisplayName;

                            SignalR.SendMessageFromAdminChat(nickName, e.Message.Content);
                        }
                    });
                };

                #endregion Message Created

                #region Message Deleted

                _discord.MessageDeleted += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        if (s == _discord) return;

                        if (e.Guild == MainGuild)
                        {
                            await MainGuildLogChannel.SendMessageAsync(
                                $"A message was deleted from  {e.Channel.Mention}. Contents: {e.Message.Content}. The message was created by: {e.Message.Author.Username} at {e.Message.CreationTimestamp}.");
                        }
                    });
                };

                #endregion Message Deleted

                #region Reaction Added

                _discord.MessageReactionAdded += (s, e) =>
                {
                    return Task.Factory.StartNew(async () =>
                    {
                        #region Rules React

                        if (e.Channel.Id == 788176889806061598)
                        {
                            // Discord Rules
                            if (e.Emoji == DiscordEmoji.FromName(_discord, ":jobdone:"))
                            {
                                DiscordMember discordMember = MainGuild.GetMemberAsync(e.User.Id).Result;

                                await discordMember.GrantRoleAsync(MainGuild.GetRole(795064838635913236));
                            }
                        }

                        #endregion Rules React
                    });
                };

                #endregion Reaction Added

                #region Reaction Removed

                _discord.MessageReactionRemoved += (s, e) =>
                {
                    return Task.CompletedTask;
                };

                #endregion Reaction Removed

                #endregion Events

                _commandsNext = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = new[] { "?" },
                    EnableDms = false,
                    EnableDefaultHelp = false
                });

                _commandsNext.RegisterCommands<DiscordCommands>();

#if DEBUG
                sw.Stop();
                Debug.WriteLine($"Loading Bot took {sw.Elapsed}.");
#endif

                await _discord.ConnectAsync();

                await OnDiscordBotLoaded();

                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static void CurrentDomainOnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        {
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/firstChance.txt", e.Exception.Message);
            Console.WriteLine(e.Exception.Message);
            return;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText($"{Directory.GetCurrentDirectory()}/unhandled.txt", (e.ExceptionObject as Exception)?.Message);
            Console.WriteLine((e.ExceptionObject as Exception)?.Message);
            return;
        }

        private static async Task OnDiscordBotLoaded()
        {
            try
            {
#if RELEASE
                /*
                Console.WriteLine($"Loading Donator Color System!");

                ulong colorChannelId = 704016300170805268;
                DiscordChannel colorChannel = await _discord.GetChannelAsync(colorChannelId);

                if (colorChannel != null)
                {
                    IReadOnlyList<DiscordMessage> colorMessages = await colorChannel.GetPinnedMessagesAsync();

                    DiscordEmoji redEmoji = DiscordEmoji.FromName(_discord, ":red_circle:");

                    DiscordEmoji orangeEmoji = await MainGuild.GetEmojiAsync(697489723828076624);

                    DiscordEmoji pinkRedEmoji = DiscordEmoji.FromName(_discord, ":small_red_triangle_down:");

                    DiscordEmoji pinkEmoji = await MainGuild.GetEmojiAsync(697518319304966254);

                    foreach (DiscordMessage discordMessage in await colorChannel.GetMessagesAsync())
                    {
                        if (discordMessage.Content.Contains("Hit one of the reactions below to select a color!")) continue;

                        await discordMessage.DeleteAsync();
                    }

                    DiscordMessage currentMessage = colorMessages.FirstOrDefault();

                    if (colorMessages.Any() && currentMessage != null)
                    {
                        Console.WriteLine("Found Existing Message.");

                        if (currentMessage.Reactions.FirstOrDefault(x => x.Emoji == redEmoji) == null)
                        {
                            Console.WriteLine($"Red Emoji not found!");
                            await currentMessage.CreateReactionAsync(redEmoji);
                            await currentMessage.ModifyAsync($"{currentMessage.Content}\n{redEmoji} - Red!");
                        }

                        if (currentMessage.Reactions.FirstOrDefault(x => x.Emoji == orangeEmoji) == null)
                        {
                            Console.WriteLine($"Dark Orange Emoji not found!");
                            await currentMessage.CreateReactionAsync(redEmoji);
                            await currentMessage.ModifyAsync($"{currentMessage.Content}\n{orangeEmoji} - Dark Orange!");
                        }

                        if (currentMessage.Reactions.FirstOrDefault(x => x.Emoji == pinkRedEmoji) == null)
                        {
                            Console.WriteLine($"Pink Red Emoji not found!");
                            await currentMessage.CreateReactionAsync(pinkRedEmoji);
                            await currentMessage.ModifyAsync($"{currentMessage.Content}\n{pinkRedEmoji} - Pinky Red!");
                        }

                        if (currentMessage.Reactions.FirstOrDefault(x => x.Emoji == pinkEmoji) == null)
                        {
                            Console.WriteLine($"Pink Emoji not found!");
                            await currentMessage.CreateReactionAsync(pinkEmoji);
                            await currentMessage.ModifyAsync($"{currentMessage.Content}\n{pinkEmoji} - Hot Pink!");
                        }

                        return;
                    }

                    Console.WriteLine($"No message found, generating messages and reactions.");

                    DiscordMessage colorMessage =
                        await colorChannel.SendMessageAsync($"Hit one of the reactions below to select a color!\n{redEmoji} - Red!\n{orangeEmoji} - Dark Orange!\n{pinkRedEmoji} - Pinky Red!\n{pinkEmoji} - Hot Pink!");

                    await colorMessage.PinAsync();

                    await colorMessage.CreateReactionAsync(redEmoji);
                    await colorMessage.CreateReactionAsync(orangeEmoji);
                    await colorMessage.CreateReactionAsync(pinkRedEmoji);
                    await colorMessage.CreateReactionAsync(pinkEmoji);

                    Console.WriteLine($"New Message created and pinned.");
                }

               */
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static Task DiscordOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return Task.CompletedTask;

            #region Color Reaction

            /*
            if (e.Channel.Id == 704016300170805268)
            {
                Console.WriteLine($"{e.User.Username} has reacted with {e.Emoji.Name} in the {e.Channel.Name} channel.");

                DiscordMember discordMember = await MainGuild.GetMemberAsync(e.User.Id);

                if (e.Emoji == DiscordEmoji.FromName(_discord, ":red_circle:"))
                {
                    await MainGuild.GrantRoleAsync(discordMember, MainGuild.GetRole(697477618022219778));
                }
                else if (e.Emoji == await MainGuild.GetEmojiAsync(697489723828076624))
                {
                    await MainGuild.GrantRoleAsync(discordMember, MainGuild.GetRole(697477763791192094));
                }
                else if (e.Emoji == DiscordEmoji.FromName(_discord, ":small_red_triangle_down:"))
                {
                    await MainGuild.GrantRoleAsync(discordMember, MainGuild.GetRole(697478876137521282));
                }
                else if (e.Emoji == await MainGuild.GetEmojiAsync(697518319304966254))
                {
                    await MainGuild.GrantRoleAsync(discordMember, MainGuild.GetRole(697479141594759188));
                }
            }
            */

            #endregion Color Reaction

            #region Gov Discord

            /*
            if (e.Message.Id == 695137895446741022)
            {
                // Gov Roles
                if (e.Emoji == DiscordEmoji.FromName(_discord, ":regional_indicator_c:"))
                {
                    // C Emoji
                    DiscordMember discordMember = await _govGuild.GetMemberAsync(e.User.Id);
                    await _govGuild.GrantRoleAsync(discordMember, _govGuild.GetRole(695134562396930089));
                }
            }
            */

            #endregion Gov Discord

            return Task.CompletedTask;
        }

        private static Task DiscordOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs e)
        {
            #region Color Reaction

            /*
            if (e.Channel.Id == 697251555111469136)
            {
                DiscordMember discordMember = await MainGuild.GetMemberAsync(e.User.Id);

                Console.WriteLine($"{discordMember.Nickname} has removed their reaction {e.Emoji} from the {e.Channel.Name} channel!");

                if (e.Emoji == DiscordEmoji.FromName(_discord, ":red_circle:"))
                {
                    await MainGuild.RevokeRoleAsync(discordMember, MainGuild.GetRole(697477618022219778), "Change of color");
                }
                else if (e.Emoji == await MainGuild.GetEmojiAsync(697489723828076624))
                {
                    await MainGuild.RevokeRoleAsync(discordMember, MainGuild.GetRole(697477763791192094), "Change of color");
                }

                if (e.Emoji == DiscordEmoji.FromName(_discord, ":small_red_triangle_down:"))
                {
                    await MainGuild.RevokeRoleAsync(discordMember, MainGuild.GetRole(697478876137521282), "Change of color");
                }
                else if (e.Emoji == await MainGuild.GetEmojiAsync(697518319304966254))
                {
                    await MainGuild.RevokeRoleAsync(discordMember, MainGuild.GetRole(697479141594759188), "Change of color");
                }
            }

            return;
            */

            #endregion Color Reaction

            return Task.CompletedTask;
        }

        private static async Task DiscordOnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            if (sender == _discord) return;

            if (e.Guild == MainGuild)
            {
                await MainGuildLogChannel.SendMessageAsync(
                    $"A message was deleted from  {e.Channel.Mention}. Contents: {e.Message.Content}. The message was created by: {e.Message.Author.Username} at {e.Message.CreationTimestamp}.");
            }
            /*
            if (e.Guild == _emergencyGuild)
            {
                await _emergencyGuildLogChannel.SendMessageAsync(
                    $"Dispatch here, A message was deleted from {e.Channel.Mention}. Contents: {e.Message.Content}. The message was created by: {e.Message.Author.Username} at {e.Message.CreationTimestamp}.");
            }*/
        }

        private static async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            try
            {
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return;
            }
        }

        private static async Task DiscordOnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            if (e.Guild == _emergencyGuild)
            {
                if (e.NicknameAfter != e.NicknameBefore)
                {
                    if (!string.IsNullOrEmpty(e.NicknameBefore) && !string.IsNullOrEmpty(e.NicknameAfter))
                    {
                        await _emergencyGuildLogChannel.SendMessageAsync(
                            $"Dispatch here, {e.NicknameBefore} has updated their nickname to {e.NicknameAfter}");
                        return;
                    }

                    if (string.IsNullOrEmpty(e.NicknameBefore))
                    {
                        await _emergencyGuildLogChannel.SendMessageAsync(
                            $"Dispatch here, {e.Member.Username} has updated their nickname to {e.NicknameAfter}");
                        return;
                    }

                    if (string.IsNullOrEmpty(e.NicknameAfter))
                    {
                        await _emergencyGuildLogChannel.SendMessageAsync(
                            $"Dispatch here, {e.NicknameBefore} has updated their nickname to {e.Member.Username}");
                        return;
                    }
                }

                if (!Equals(e.RolesBefore, e.RolesAfter))
                {
                    if (e.RolesAfter.Count > e.RolesBefore.Count)
                    {
                        // Have more roles, so they've gained.

                        foreach (DiscordRole discordRole in e.RolesAfter)
                        {
                            // Loop through roles since change

                            // Previous role exists so skip
                            if (e.RolesBefore.Contains(discordRole)) continue;

                            string nickName = e.Member.Nickname;

                            if (string.IsNullOrEmpty(nickName))
                            {
                                nickName = e.Member.Username;
                            }

                            await _emergencyGuildLogChannel.SendMessageAsync(
                                $"Dispatch here, {nickName} has been added to the {discordRole.Name} role.");
                        }

                        return;
                    }

                    if (e.RolesAfter.Count < e.RolesBefore.Count)
                    {
                        // Have less roles than before, so they've lost
                        foreach (DiscordRole discordRole in e.RolesBefore)
                        {
                            // Loop through roles since change

                            // Previous role exists so skip
                            if (e.RolesAfter.Contains(discordRole)) continue;

                            string nickName = e.Member.Nickname;

                            if (string.IsNullOrEmpty(nickName))
                            {
                                nickName = e.Member.Username;
                            }

                            await _emergencyGuildLogChannel.SendMessageAsync(
                                $"Dispatch here, {nickName} has been removed from the {discordRole.Name} role.");
                        }

                        return;
                    }
                }
            }

            /*
            if (e.Guild == _govGuild)
            {
                if (e.NicknameAfter != e.NicknameBefore)
                {
                    if (!string.IsNullOrEmpty(e.NicknameBefore) && !string.IsNullOrEmpty(e.NicknameAfter))
                    {
                        await _govLogChannel.SendMessageAsync(
                            $"Bot here, {e.NicknameBefore} has updated their nickname to {e.NicknameAfter}");
                        return;
                    }

                    if (string.IsNullOrEmpty(e.NicknameBefore))
                    {
                        await _govLogChannel.SendMessageAsync(
                            $"Bot here, {e.Member.Username} has updated their nickname to {e.NicknameAfter}");
                        return;
                    }

                    if (string.IsNullOrEmpty(e.NicknameAfter))
                    {
                        await _govLogChannel.SendMessageAsync(
                            $"Bot here, {e.NicknameBefore} has updated their nickname to {e.Member.Username}");
                        return;
                    }
                }

                if (!Equals(e.RolesBefore, e.RolesAfter))
                {
                    if (e.RolesAfter.Count > e.RolesBefore.Count)
                    {
                        // Have more roles, so they've gained.

                        foreach (DiscordRole discordRole in e.RolesAfter)
                        {
                            // Loop through roles since change

                            // Previous role exists so skip
                            if (e.RolesBefore.Contains(discordRole)) continue;

                            string nickName = e.Member.Nickname;

                            if (string.IsNullOrEmpty(nickName))
                            {
                                nickName = e.Member.Username;
                            }

                            await _govLogChannel.SendMessageAsync(
                                $"Bot here, {nickName} has been added to the {discordRole.Name} role.");
                        }

                        return;
                    }

                    if (e.RolesAfter.Count < e.RolesBefore.Count)
                    {
                        // Have less roles than before, so they've lost
                        foreach (DiscordRole discordRole in e.RolesBefore)
                        {
                            // Loop through roles since change

                            // Previous role exists so skip
                            if (e.RolesAfter.Contains(discordRole)) continue;

                            string nickName = e.Member.Nickname;

                            if (string.IsNullOrEmpty(nickName))
                            {
                                nickName = e.Member.Username;
                            }

                            await _govLogChannel.SendMessageAsync(
                                $"Bot here, {nickName} has been removed from the {discordRole.Name} role.");
                        }

                        return;
                    }
                }
            }*/
        }

        private static async Task DiscordOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
        }
    }
}