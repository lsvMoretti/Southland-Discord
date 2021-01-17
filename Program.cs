using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot;
using DiscordBot.Commands;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static DiscordSocketClient Discord;
    public static readonly string LogoUri = "https://sol-rp.com/southland-logo.png";
    public static readonly CommandService CommandService;

    #region Guilds

    public static SocketGuild MainGuild;
    private static readonly ulong MainGuildId = 748851074135490560;

    public static SocketGuild EmergencyGuild;
    private static readonly ulong EmergencyGuildId = 797216050429034516;

    #endregion Guilds

    #region Channels

    public static ITextChannel MainGuildLogChannel;
    private static readonly ulong MainGuildLogChannelId = 795062350398881832;

    public static ITextChannel MainGuildJoinChannel;
    private static readonly ulong MainGuildJoinChannelId = 748851942075269201;

    public static ITextChannel EmergencyGuildLogChannel;
    private static readonly ulong EmergencyGuildLogChannelId = 798259039062196275;

    #endregion Channels

    public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {
        await using (var services = ConfigureServices())
        {
            Discord = services.GetRequiredService<DiscordSocketClient>();

            Discord.Log += Log;

            services.GetRequiredService<CommandService>().Log += Log;

            await Discord.LoginAsync(TokenType.Bot, Settings.Default.Token);
            await Discord.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
        }

        MainGuild = Discord.GetGuild(MainGuildId);
        EmergencyGuild = Discord.GetGuild(EmergencyGuildId);

        if (MainGuild == null)
        {
            Console.WriteLine($"Unable to connect to the Main Guild!");
            return;
        }

        if (MainGuild != null)
        {
            Console.WriteLine($"Connected to {MainGuild.Name}");
        }

        if (EmergencyGuild == null)
        {
            Console.WriteLine($"Unable to connect to the Emergency Guild");
            return;
        }

        if (EmergencyGuild != null)
        {
            Console.WriteLine($"Connected to {EmergencyGuild.Name}");
        }

        MainGuildLogChannel = MainGuild.GetTextChannel(MainGuildLogChannelId);

        MainGuildJoinChannel = MainGuild.GetTextChannel(MainGuildJoinChannelId);

        EmergencyGuildLogChannel = EmergencyGuild.GetTextChannel(EmergencyGuildLogChannelId);

        #region Events

        Discord.UserJoined += DiscordOnUserJoined;
        Discord.UserLeft += DiscordOnUserLeft;
        Discord.GuildMemberUpdated += DiscordOnGuildMemberUpdated;

        Discord.MessageReceived += DiscordOnMessageReceived;
        Discord.MessageDeleted += DiscordOnMessageDeleted;

        Discord.ReactionAdded += DiscordOnReactionAdded;

        #endregion Events

        await Task.Delay(-1);
    }

    private async Task DiscordOnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
    {
        if (!cachedMessage.HasValue) return;
        var message = cachedMessage.Value;

        #region Rules React

        if (channel.Id == 788176889806061598)
        {
            if (reaction.Emote.Name == ":jobdone:")
            {
                SocketGuildUser user = MainGuild.GetUser(reaction.UserId);

                IRole role = MainGuild.GetRole(795064838635913236);

                await user.AddRoleAsync(role);
            }
        }

        #endregion Rules React
    }

    private async Task DiscordOnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, ISocketMessageChannel channel)
    {
        if (!cachedMessage.HasValue) return;

        var message = cachedMessage.Value;

        await MainGuildLogChannel.SendMessageAsync(
            $"A message ({message.Id}) from {message.Author} was removed from the channel {channel.Name}. Contents: {message.Content}");
    }

    private async Task DiscordOnMessageReceived(SocketMessage arg)
    {
        if (arg.Channel.Id == 787791455950471218)
        {
            if (arg.Author.IsBot) return;

            if (arg.Content.StartsWith('?')) return;

            if (arg.Content.StartsWith('<')) return;

            if (string.IsNullOrEmpty(arg.Content)) return;

            SocketGuildUser user = (SocketGuildUser)arg.Author;

            string nick = !string.IsNullOrEmpty(user.Nickname) ? user.Nickname : user.Username;

            SignalR.SendMessageFromAdminChat(nick, arg.Content);
            return;
        }
    }

    private async Task DiscordOnGuildMemberUpdated(SocketGuildUser before, SocketGuildUser after)
    {
        string nick = !string.IsNullOrEmpty(after.Nickname) ? after.Nickname : after.Username;

        if (after.Guild == MainGuild)
        {
            if (before.Nickname != after.Nickname)
            {
                await MainGuildLogChannel.SendMessageAsync(
                    $"User {before.Nickname} has changed their nickname to {after.Nickname}");
                return;
            }

            if (!Equals(before.Roles, after.Roles))
            {
                if (after.Roles.Count > before.Roles.Count)
                {
                    foreach (SocketRole role in after.Roles)
                    {
                        if (before.Roles.Contains(role)) continue;

                        await MainGuildLogChannel.SendMessageAsync($"{nick} has been added to the {role.Name} role.");
                    }

                    return;
                }

                if (after.Roles.Count < before.Roles.Count)
                {
                    foreach (SocketRole role in before.Roles)
                    {
                        if (after.Roles.Contains(role)) continue;

                        await MainGuildLogChannel.SendMessageAsync(
                            $"{nick} has been removed from the role {role.Name}.");
                    }

                    return;
                }
            }
        }
    }

    private async Task DiscordOnUserLeft(SocketGuildUser arg)
    {
        string userNick = !string.IsNullOrEmpty(arg.Nickname) ? arg.Nickname : arg.Username;

        if (arg.Guild == MainGuild)
        {
            await MainGuildLogChannel.SendMessageAsync($"{userNick} (Username: {arg.Username}) has left the guild!");

            await arg.SendMessageAsync($"Sorry to see you go! Please tell us how we can improve on the forums. https://forum.sol-rp.com");
        }
    }

    private async Task DiscordOnUserJoined(SocketGuildUser arg)
    {
        string userNick = !string.IsNullOrEmpty(arg.Nickname) ? arg.Nickname : arg.Username;

        if (arg.Guild == MainGuild)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder
            {
                Title = "New User",
                ThumbnailUrl = LogoUri,
                Timestamp = DateTimeOffset.Now,
                Color = Color.DarkRed,
                Description = $"Look out! {userNick} has joined us!"
            };

            Embed embed = embedBuilder.Build();

            await MainGuildJoinChannel.SendMessageAsync(embed: embed);

            await arg.SendMessageAsync($"Welcome to Southland Roleplay! For any information on using the bot please use ?help in one of the server channels!\n" +
                                       $"Don't forget to read and react to the rules channel to get full access to the discord!");
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<PictureService>()
            .BuildServiceProvider();
    }
}