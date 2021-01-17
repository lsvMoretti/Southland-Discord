using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace DiscordBot
{
    public class GameReportHandler
    {
        public static List<AdminReportObject> AdminReports = new List<AdminReportObject>();
        public static Dictionary<int, RestTextChannel> ReportChannels = new Dictionary<int, RestTextChannel>();

        public static async void AddAdminReport(string reportJson)
        {
            AdminReportObject reportObject = JsonConvert.DeserializeObject<AdminReportObject>(reportJson);

            Console.WriteLine($"New Report ID: {reportObject.Id}.");

            AdminReports.Add(reportObject);

            EmbedBuilder discordEmbed = new EmbedBuilder
            {
                Color = Color.Blue,
                Description = reportObject.Message,
                ThumbnailUrl = Program.LogoUri,
                Timestamp = reportObject.Time,
                Title = "New Report"
            };

            discordEmbed.AddField("Report ID", $"{reportObject.Id}");
            discordEmbed.AddField("Player ID", $"{reportObject.PlayerId}");
            discordEmbed.AddField("Character ID", $"{reportObject.CharacterId}");
            discordEmbed.AddField("Character Name", $"{reportObject.CharacterName}");

            var reportCategory = Program.MainGuild.CategoryChannels.FirstOrDefault(x => x.Id == 795085207672848396);

            RestTextChannel reportChannel = await Program.MainGuild.CreateTextChannelAsync($"Report-{reportObject.Id}",
                properties =>
                {
                    properties.CategoryId = reportCategory.Id;
                    properties.Topic = $"Report Channel for {reportObject.CharacterName}";
                });

            if (!ReportChannels.ContainsKey(reportObject.Id))
            {
                ReportChannels.Add(reportObject.Id, reportChannel);
            }

            await reportChannel.SendMessageAsync(embed: discordEmbed.Build());
            await reportChannel.SendMessageAsync($"Commands: ?message to reply, ?cr to close the report");
        }

        public static async void RemoveAdminReport(string reportJson)
        {
            try
            {
                AdminReportObject reportObject = JsonConvert.DeserializeObject<AdminReportObject>(reportJson);

                AdminReportObject listObject = AdminReports.FirstOrDefault(x => x.Id == reportObject.Id);

                AdminReports.Remove(listObject);

                bool tryGetChannel = ReportChannels.TryGetValue(reportObject.Id, out RestTextChannel reportChannel);

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

                    ITextChannel discordReportChannel = Program.MainGuild.GetTextChannel(reportChannel.Id);

                    discordReportChannel?.DeleteAsync();
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
            var reportCategory = Program.MainGuild.CategoryChannels.FirstOrDefault(x => x.Id == 795085207672848396);

            var discordChannels = reportCategory.Channels.ToList();

            foreach (var discordChannel in discordChannels)
            {
                await discordChannel.DeleteAsync();
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
            var reportsCategory = Program.MainGuild.CategoryChannels.FirstOrDefault(x => x.Id == 795085207672848396);

            if (reportsCategory == null)
            {
                Console.WriteLine("Reports Category null");
                return;
            }

            var reportChannel = reportsCategory.Channels.FirstOrDefault(x => x.Name == $"report-{reportId}");

            var socketTextChannel = reportChannel as SocketTextChannel;

            if (reportChannel == null)
            {
                Console.WriteLine($"Report Reply Channel is null for report ID: {reportId}.");
                return;
            }

            if (socketTextChannel != null) await socketTextChannel.SendMessageAsync($"{message}");
        }
    }
}