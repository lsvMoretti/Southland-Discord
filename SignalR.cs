using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.SignalR.Client;

namespace DiscordBot
{
    public class SignalR
    {
        private static HubConnection hubConnection;

        public static async Task StartConnection()
        {
            Console.WriteLine($"Starting SignalR Hub Connection");

            string url = @"http://149.202.88.222:2000/signalr";

            hubConnection = new HubConnectionBuilder().WithUrl(url).Build();
            hubConnection.Reconnecting += HubConnection_Reconnecting;
            hubConnection.Reconnected += HubConnection_Reconnected;
            await Connect(hubConnection);

            hubConnection.Closed += HubConnection_Closed;
            hubConnection.Reconnecting += HubConnectionOnReconnecting;

            await hubConnection.InvokeAsync("AddToUsergroup", "Discord", null);

            #region Usergroup Callback

            hubConnection.On<string>("AddedToUsergroup",
                (usergroup) => { Console.WriteLine($"[SignalR] Added to Usergroup: {usergroup}."); });

            #endregion Usergroup Callback

            #region Message Callbacks

            hubConnection.On<string>("SendDiscordMessageToLogChannel", ChannelHandler.SendMessageToMainLogChannel);

            hubConnection.On<string, string>("ReceiveDiscordMessage", ChannelHandler.OnReceiveDiscordMessage);

            hubConnection.On<string, string>("ReceiveDiscordEmbed", ChannelHandler.OnReceiveDiscordEmbed);

            hubConnection.On<string, string>("SendMessageToUser", UserHandler.SendMessageToUser);

            #endregion Message Callbacks

            #region Game Activity

            hubConnection.On<int, string>("OnUserLogin", ServerHandler.OnUserGameLogin);

            hubConnection.On<int, string>("OnUserLogout", ServerHandler.OnUserGameLogout);

            #endregion Game Activity

            #region Admin Reports

            hubConnection.On<string>("NewReport", GameReportHandler.AddAdminReport);
            hubConnection.On<string>("RemoveReport", GameReportHandler.RemoveAdminReport);
            hubConnection.On("ServerRestart", GameReportHandler.ClearReportChannels);
            hubConnection.On<int, string>("SendReportReply", GameReportHandler.SendReportReply);

            #endregion Admin Reports

            hubConnection.On<ulong>("SendLinkedDiscordMessage", SendLinkedMessageToDiscordUser);

            hubConnection.On<string, string>("SendScreenshotToDiscordUser", Program.SendScreenShotToUser);
        }

        private static Task HubConnectionOnReconnecting(Exception arg)
        {
            Console.WriteLine("Reconnecting....");
            return Task.CompletedTask;
        }

        private static async Task SendLinkedMessageToDiscordUser(ulong discordId)
        {
            Console.WriteLine($"Send Linked Message To Discord User -- ID: {discordId}");

            SocketUser user = Program.Discord.GetUser(discordId);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            await user.SendMessageAsync(
                "Your UCP account has been successfully linked to this account. If this is incorrect, please contact a staff member.");
        }

        private static Task HubConnection_Reconnected(string arg)
        {
            Console.WriteLine("[SignalR] Hub Reconnected");

            hubConnection.InvokeAsync("AddToUsergroup", "Discord", null);
            return Task.CompletedTask;
        }

        private static Task HubConnection_Reconnecting(Exception arg)
        {
            Console.WriteLine($"[SignalR] Hub Reconnecting");
            return Task.CompletedTask;
        }

        private static async Task HubConnection_Closed(Exception arg)
        {
            Console.WriteLine($"[SignalR] Hub Connection Lost");
            bool connected = await Connect(hubConnection);

            while (!connected)
            {
                connected = await Connect(hubConnection);
            }

            Console.WriteLine($"[SignalR] Hub Reconnected");
        }

        private static async Task<bool> Connect(HubConnection connection)
        {
            try
            {
                bool connected = false;
                hubConnection.StartAsync().Wait();
                while (!connected)
                {
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        connected = true;
                    }
                }

                return connected;
            }
            catch (Exception e)
            {
                Console.WriteLine("[SignalR] Unable to connect to hub. Retrying..");
                return false;
            }
        }

        public static async Task<int> FetchOnlinePlayerCount()
        {
            return await hubConnection.InvokeAsync<int>("FetchOnlinePlayerCount");
        }

        public static async Task<int[]> FetchTime()
        {
            return await hubConnection.InvokeAsync<int[]>("FetchGameTime");
        }

        public static async Task<DateTime> FetchServerStartTime()
        {
            Console.WriteLine($"Fetching Server Time");
            DateTime startTime = await hubConnection.InvokeAsync<DateTime>("FetchGameServerStartTime");
            Console.WriteLine($"Start Time: {startTime}");
            return startTime;
        }

        public static async void SendMessageFromAdminChat(string username, string message)
        {
            await hubConnection.InvokeAsync("SendMessageFromAdminChat", username, message);
        }

        public static async void SendMessageToReportPlayer(int reportId, string message)
        {
            await hubConnection.InvokeAsync("SendMessageToReport", reportId, message);
        }

        public static async void CloseReport(int reportId)
        {
            await hubConnection.InvokeAsync("CloseReport", reportId);
        }
    }
}