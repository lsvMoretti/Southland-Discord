using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordBot
{
    internal class Program
    {
        public static DiscordClient _discord;

        public const string LogoUrl = "https://forum.sol-rp.com/styles/default/xenforo/southlandrp.png";

        #region DiscordGuilds

        private static readonly ulong _mainGuildId = 748851074135490560;
        public static DiscordGuild MainGuild;
        /*
        private static readonly ulong _emergencyGuildId = 633766938396590080;
        private static DiscordGuild _emergencyGuild;

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

        /*

        #region EmergencyGuildChannels

        private static readonly ulong _emergencyGuildLogChannelId = 668503333878890509;
        private static DiscordChannel _emergencyGuildLogChannel;

        #endregion EmergencyGuildChannels

        #region Gov Guild Channels

        private static readonly ulong _govLogChannelId = 695134603228217344;
        private static DiscordChannel _govLogChannel;

        #endregion Gov Guild Channels

        */

        private static CommandsNextModule _commandsNext;

        private static List<string> bannedWords = new List<string>
        {
            "gta.world",
        };

        private static async Task Main(string[] args)
        {
            try
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif

                Console.WriteLine("Starting Discord Bot");

                Console.Title = "Southland Roleplay Discord Bot";

#if RELEASE
                WeatherUpdate.InitWeatherUpdate();
                SignalR.StartConnection();
#endif

                _discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = Settings.Default.Token,
                    TokenType = TokenType.Bot,
                    LogLevel = LogLevel.Debug,
                    AutoReconnect = true,
                });

                MainGuild = await _discord.GetGuildAsync(_mainGuildId);
                //_emergencyGuild = await _discord.GetGuildAsync(_emergencyGuildId);
                //_govGuild = await _discord.GetGuildAsync(_govGuildId);

                if (MainGuild != null)
                {
                    Console.WriteLine($"Connected to {MainGuild.Name} Discord Guild.");
                }
                /*
                if (_emergencyGuild != null)
                {
                    Console.WriteLine($"Connected to {_emergencyGuild.Name} Discord Guild");
                }

                if (_govGuild != null)
                {
                    Console.WriteLine($"Connected to {_govGuild.Name} Discord Guild");
                }
                */

                #region Main Guild Channels

                MainGuildLogChannel = await _discord.GetChannelAsync(_mainGuildLogChannelId);
                _mainGuildJoinChannel = await _discord.GetChannelAsync(_mainGuildJoinChannelId);

                #endregion Main Guild Channels

                /*

                #region Emergency Guild Channels

                if (_emergencyGuild != null)
                {
                    _emergencyGuildLogChannel = await _discord.GetChannelAsync(_emergencyGuildLogChannelId);
                }

                #endregion Emergency Guild Channels

                #region Gov Guild Channels

                if (_govGuild != null)
                {
                    _govLogChannel = await _discord.GetChannelAsync(_govLogChannelId);
                }

                #endregion Gov Guild Channels

                */

                #region Events

                _discord.GuildMemberAdded += DiscordOnGuildMemberAdded;
                _discord.GuildMemberRemoved += DiscordOnGuildMemberRemoved;
                _discord.GuildMemberUpdated += DiscordOnGuildMemberUpdated;
                _discord.MessageCreated += DiscordOnMessageCreated;
                _discord.MessageDeleted += DiscordOnMessageDeleted;
                _discord.MessageReactionAdded += DiscordOnMessageReactionAdded;
                _discord.MessageReactionRemoved += DiscordOnMessageReactionRemoved;

                #endregion Events

                _commandsNext = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = "?",
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

        private static async Task DiscordOnMessageReactionAdded(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot) return;

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

            #region Rules React

            if (e.Channel.Id == 788207707177484349)
            {
                // Discord Rules
                if (e.Emoji == DiscordEmoji.FromName(_discord, ":jobdone:"))
                {
                    DiscordMember discordMember = await MainGuild.GetMemberAsync(e.User.Id);

                    await MainGuild.GrantRoleAsync(discordMember, MainGuild.GetRole(704002720692174908));
                }
            }

            #endregion Rules React

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
        }

        private static async Task DiscordOnMessageReactionRemoved(MessageReactionRemoveEventArgs e)
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
        }

        private static async Task DiscordOnMessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Client == _discord) return;

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

        private static async Task DiscordOnMessageCreated(MessageCreateEventArgs e)
        {
            try
            {
                if (e.Channel.Id == 704002734411743363)
                {
                    if (!e.Message.Attachments.Any() && !e.Message.Embeds.Any() && !e.Message.Content.StartsWith("http"))
                    {
                        await e.Message.DeleteAsync("Non-image content in gallery.");
                        DiscordDmChannel dmChannel = await _discord.CreateDmAsync(e.Author);
                        await dmChannel.SendMessageAsync($"{e.Channel.Mention} is a media only channel!");
                        await dmChannel.DeleteAsync();
                    }
                }

                DiscordUser morettiuser = await _discord.GetUserAsync(132968074709041153);

                bool morettiMention = e.Message.MentionedUsers.Any(x => x.Id == 132968074709041153);

                if (morettiMention && !e.Author.IsBot)
                {
                    if (morettiuser.Presence.Status != UserStatus.Online || morettiuser.Presence.Status == UserStatus.Invisible || morettiuser.Presence.Status == UserStatus.Offline)
                    {
                        await e.Message.DeleteAsync();
                        await e.Channel.SendMessageAsync(
                            $"{e.Author.Mention} - Moretti is currently busy. If it's a bug, please use the ?bug command to report it. Anything else can be put onto the forums!");

                        DiscordMember moretti = null;

                        if (e.Guild == MainGuild)
                        {
                            moretti = MainGuild.Members.FirstOrDefault(x => x.Id == 132968074709041153);
                        }
                        /*
                        if (e.Guild == _emergencyGuild)
                        {
                            moretti = _emergencyGuild.Members.FirstOrDefault(x => x.Id == 132968074709041153);
                        }

                        if (e.Guild == _govGuild)
                        {
                            moretti = _govGuild.Members.FirstOrDefault(x => x.Id == 132968074709041153);
                        }
                        */
                        if (moretti == null) return;

                        await moretti.SendMessageAsync(
                            $"{e.Author.Username} has tried to send you a message in {e.Channel.Mention}. Contents: \n {e.Message.Content}");
                    }
                }

                bool contains = false;

                foreach (string bannedWord in bannedWords)
                {
                    if (e.Message.Content.ToLower().Contains(bannedWord.ToLower()))
                    {
                        contains = true;
                    }
                }

                if (contains && e.Client != _discord)
                {
                    await e.Message.DeleteAsync();

                    if (e.Guild == MainGuild)
                    {
                        await MainGuildLogChannel.SendMessageAsync(
                            $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                    }/*
                    if (e.Guild == _emergencyGuild)
                    {
                        await _emergencyGuildLogChannel.SendMessageAsync(
                            $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                    }
                    if (e.Guild == _govGuild)
                    {
                        await _govLogChannel.SendMessageAsync(
                            $"{e.Author.Username} has messaged a banned word in {e.Channel.Mention}. Message: {e.Message.Content}");
                    }*/
                }

                if (e.Channel.Id == 704002753185447946)
                {
                    if (e.Author.IsBot) return;

                    string nickName = e.Author.Username;

                    SignalR.SendMessageFromAdminChat(nickName, e.Message.Content);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return;
            }
        }

        private static async Task DiscordOnGuildMemberUpdated(GuildMemberUpdateEventArgs e)
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
            /*
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
            */
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

        private static async Task DiscordOnGuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            if (e.Guild == MainGuild)
            {
                await MainGuildLogChannel.SendMessageAsync(
                    $"{e.Member.Nickname} ({e.Member.Username}) has left the guild");

                await e.Member.SendMessageAsync(
                    $"Sorry to see you go! Please tell us how we can improve on the forums. https://forum.sol-rp.com");
            }
        }

        private static async Task DiscordOnGuildMemberAdded(GuildMemberAddEventArgs e)
        {
            if (e.Guild == MainGuild)
            {
                // If entered main guild

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "New User",
                    ThumbnailUrl = LogoUrl,
                    Timestamp = DateTimeOffset.Now,
                    Color = DiscordColor.SpringGreen,
                    Description = $"Look out! {e.Member.Username} has joined {e.Guild.Name}!"
                };

                await _mainGuildJoinChannel.SendMessageAsync(null, false, embedBuilder);

                await e.Member.SendMessageAsync(
                    $"Welcome to Southland Roleplay! For any information on using the bot please use ?help in one of the server channels!");
            }
            /*
                        if (e.Guild == _emergencyGuild)
                        {
                            // If entered emergency guild

                            await e.Member.SendMessageAsync(
                                $"Dispatch here, Welcome to Los Santos V Emergency Services Discord! For any information on using the bot please use ?help in one of the server channels!");
                        }*/
        }
    }
}