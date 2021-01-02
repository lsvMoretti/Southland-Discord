using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot
{
    public class ServerHandler
    {
        public static List<GameActivity> GameActivityList = new List<GameActivity>();

        public static void OnUserGameLogin(int accountId, string userName)
        {
#if DEBUG
            Console.WriteLine($"[SignalR] User has logged in IG: {userName}.");
#endif

            if (GameActivityList.Count >= 10)
            {
                GameActivityList.Remove(GameActivityList.First());
            }

            GameActivityList.Add(new GameActivity(userName, accountId, GameActivityType.Login));
        }

        public static void OnUserGameLogout(int accountId, string userName)
        {
#if DEBUG
            Console.WriteLine($"[SignalR] User has logged out IG: {userName}.");
#endif
            if (GameActivityList.Count >= 10)
            {
                GameActivityList.Remove(GameActivityList.First());
            }

            GameActivityList.Add(new GameActivity(userName, accountId, GameActivityType.Logout));
        }
    }

    public class GameActivity
    {
        public string Username { get; }
        public int AccountId { get; }

        public DateTime DateTime { get; }

        public GameActivityType ActivityType { get; }

        public GameActivity(string userName, int accountId, GameActivityType activityType)
        {
            Username = userName;
            AccountId = accountId;
            DateTime = DateTime.Now;
            ActivityType = activityType;
        }
    }

    public enum GameActivityType
    {
        Login,
        Logout
    }
}