using System;

namespace DiscordBot
{
    public class UserHandler
    {
        public static async void SendMessageToUser(string userId, string message)
        {
            /*
            bool tryParse = ulong.TryParse(userId, out ulong uid);

            if (!tryParse)
            {
                Console.WriteLine($"An error occurred parsing the Discord User Id!");
                return;
            }

            DiscordUser user = await Program._discord.GetUserAsync(uid);

            if (user == null)
            {
                Console.WriteLine($"An error occurred fetching the discord user.");
                return;
            }

            DiscordDmChannel dmChannel = await Program._discord.CreateDmAsync(user);

            if (dmChannel == null)
            {
                Console.WriteLine($"An error occurred creating the DM Channel.");
                return;
            }

            await dmChannel.SendMessageAsync(message);*/
        }
    }
}