using System;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot
{
    public class ChannelHandler
    {
        public static async void SendMessageToMainLogChannel(string message)
        {
            try
            {
                await Program.MainGuildLogChannel.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async void OnReceiveDiscordMessage(string channelIdString, string message)
        {
            try
            {
                Console.WriteLine($"[SignalR] Received Message: {message} to channel {channelIdString}");

                bool tryParse = ulong.TryParse(channelIdString, out ulong channelId);

                if (!tryParse) return;

                DiscordChannel channel = await Program._discord.GetChannelAsync(channelId);

                if (channel == null) return;

                await channel.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async void OnReceiveDiscordEmbed(string channelIdString, string embedJson)
        {
            try
            {
                Console.WriteLine($"[SignalR] Received Embed to channel {channelIdString}");

                bool tryParse = ulong.TryParse(channelIdString, out ulong channelId);

                if (!tryParse) return;

                DiscordChannel channel = await Program._discord.GetChannelAsync(channelId);

                if (channel == null) return;

                DiscordEmbed embed = JsonConvert.DeserializeObject<DiscordEmbed>(embedJson);

                await channel.SendMessageAsync(embed: embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}