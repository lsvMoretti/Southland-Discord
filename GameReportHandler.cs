using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace DiscordBot
{
    public class GameReportHandler
    {
        public static List<AdminReportObject> AdminReports = new List<AdminReportObject>();
        public static Dictionary<int, DiscordChannel> ReportChannels = new Dictionary<int, DiscordChannel>();

        public static async void AddAdminReport(string reportJson)
        {
            AdminReportObject reportObject = JsonConvert.DeserializeObject<AdminReportObject>(reportJson);

            Console.WriteLine($"New Report ID: {reportObject.Id}.");

            AdminReports.Add(reportObject);

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Description = reportObject.Message,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.LogoUrl },
                Timestamp = reportObject.Time,
                Title = "New Report"
            };

            discordEmbed.AddField("Report ID", $"{reportObject.Id}");
            discordEmbed.AddField("Player ID", $"{reportObject.PlayerId}");
            discordEmbed.AddField("Character ID", $"{reportObject.CharacterId}");
            discordEmbed.AddField("Character Name", $"{reportObject.CharacterName}");

            //await Program.MainGuildReportChannel.SendMessageAsync(embed: discordEmbed);

            DiscordChannel reportCategory = await Program._discord.GetChannelAsync(704011246071971972);

            DiscordChannel reportChannel = await Program.MainGuild.CreateChannelAsync($"Report-{reportObject.Id}", ChannelType.Text,
                reportCategory);

            if (!ReportChannels.ContainsKey(reportObject.Id))
            {
                ReportChannels.Add(reportObject.Id, reportChannel);
            }

            await reportChannel.SendMessageAsync(embed: discordEmbed);
            await reportChannel.SendMessageAsync($"Commands: ?message to reply, ?cr to close the report");
        }

        public static async void RemoveAdminReport(string reportJson)
        {
            try
            {
                AdminReportObject reportObject = JsonConvert.DeserializeObject<AdminReportObject>(reportJson);

                AdminReportObject listObject = AdminReports.FirstOrDefault(x => x.Id == reportObject.Id);

                AdminReports.Remove(listObject);

                bool tryGetChannel = ReportChannels.TryGetValue(reportObject.Id, out DiscordChannel reportChannel);

                if (!tryGetChannel) return;

                await reportChannel.SendMessageAsync($"The report has been closed or handled in-game.\nThis channel is being removed in five seconds");

                Timer timer = new Timer(5000)
                {
                    AutoReset = false
                };

                timer.Start();

                timer.Elapsed += (sender, args) =>
                {
                    timer.Stop();

                    DiscordChannel discordReportChannel = Program._discord.GetChannelAsync(reportChannel.Id).Result;

                    if (discordReportChannel != null)
                    {
                        discordReportChannel.DeleteAsync();
                    }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async void ClearReportChannels()
        {
            DiscordChannel reportCategory = await Program._discord.GetChannelAsync(704011246071971972);

            List<DiscordChannel> discordChannels = reportCategory.Children.ToList();

            foreach (DiscordChannel discordChannel in discordChannels)
            {
                await discordChannel.DeleteAsync("Server Restart");
            }

            AdminReports = new List<AdminReportObject>();
        }

        public static void ClearReport(int reportId)
        {
            AdminReportObject adminReport = AdminReports.FirstOrDefault(x => x.Id == reportId);

            AdminReports.Remove(adminReport);

            SignalR.CloseReport(reportId);
        }

        public static async void SendReportReply(int reportId, string message)
        {
            KeyValuePair<ulong, DiscordChannel>? reportChannel =
                Program.MainGuild.Channels.FirstOrDefault(x => x.Value.Name == $"report-{reportId}" && x.Value.ParentId == 704011246071971972);

            if (reportChannel == null)
            {
                Console.WriteLine($"Report Reply Channel is null for report ID: {reportId}.");
                return;
            }

            await reportChannel.Value.Value.SendMessageAsync($"{message}");
        }
    }
}