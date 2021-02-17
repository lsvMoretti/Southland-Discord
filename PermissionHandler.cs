using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordBot
{
    public class PermissionHandler
    {
        public static List<ulong> ConnectedAccounts = new List<ulong>();
        public static List<DonatorInfo> DonatorInfo = new List<DonatorInfo>();
    }

    public class DonatorInfo
    {
        public ulong Id { get; set; }
        public DonationLevel DonatorLevel { get; set; }
    }

    public enum DonationLevel
    {
        [Description("None")]
        None,

        [Description("Bronze")]
        Bronze,

        [Description("Silver")]
        Silver,

        [Description("Gold")]
        Gold
    }
}