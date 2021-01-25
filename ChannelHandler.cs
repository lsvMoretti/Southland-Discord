using System;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
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

                SocketTextChannel channel = Program.MainGuild.GetTextChannel(channelId);

                if (channel == null)
                {
                    channel = Program.EmergencyGuild.GetTextChannel(channelId);
                    if (channel == null)
                    {
                        return;
                    }
                }

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

                SocketTextChannel channel = Program.MainGuild.GetTextChannel(channelId);

                if (channel == null)
                {
                    channel = Program.EmergencyGuild.GetTextChannel(channelId);
                    if (channel == null) return;
                }

                DiscordEmbed discordEmbed = JsonConvert.DeserializeObject<DiscordEmbed>(embedJson);

                List<EmbedFieldBuilder> fieldBuilder = new List<EmbedFieldBuilder>();

                foreach (DiscordEmbedField discordEmbedField in discordEmbed.Fields)
                {
                    fieldBuilder.Add(new EmbedFieldBuilder
                    {
                        Name = discordEmbedField.Name,
                        Value = discordEmbedField.Value,
                        IsInline = discordEmbedField.Inline
                    });
                }

                EmbedBuilder embed = new EmbedBuilder
                {
                    Title = discordEmbed.Title,
                    Timestamp = discordEmbed.Timestamp,
                    Description = discordEmbed.Description,
                    Color = Color.DarkOrange,
                    Fields = fieldBuilder
                };

                if (discordEmbed.Thumbnail != null)
                {
                    if (!string.IsNullOrEmpty(discordEmbed.Thumbnail.Url.ToString()))
                    {
                        embed.ThumbnailUrl = discordEmbed.Thumbnail.Url.ToString();
                    }
                }

                if (discordEmbed.Image != null)
                {
                    if (!string.IsNullOrEmpty(discordEmbed.Image.Url.ToString()))
                    {
                        embed.ImageUrl = discordEmbed.Image.Url.ToString();
                    }
                }

                await channel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}