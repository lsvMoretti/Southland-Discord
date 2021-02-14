using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using DiscordBot.Models;
using TwitchLib.Api.Core.Models.Undocumented.ChannelPanels;

namespace DiscordBot
{
    public class PermissionHandler
    {
        public static List<ulong> ConnectedAccounts = new List<ulong>();

        public static void StartPermissionUpdate()
        {
            Timer timer = new Timer(60000)
            {
                AutoReset = true
            };

            timer.Start();

            timer.Elapsed += (sender, args) =>
            {
                ConnectedAccounts = new List<ulong>();

                using Database database = new Database();

                List<Account> accounts = database.Account.ToList();

                foreach (Account account in accounts)
                {
                    lock (account)
                    {
                        if (string.IsNullOrEmpty(account.DiscordId)) continue;

                        bool tryParse = ulong.TryParse(account.DiscordId, out ulong discordId);

                        if (!tryParse) continue;

                        ConnectedAccounts.Add(discordId);
                    }
                }

                Program.UpdateDiscordLinkPermissions();
            };
        }
    }
}