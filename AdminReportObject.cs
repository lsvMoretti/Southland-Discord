using System;

namespace DiscordBot
{
    public class AdminReportObject
    {
        public int Id { get; set; }
        
        public int CharacterId { get; set; }
        public int PlayerId { get; set; }
        public string CharacterName { get; set; }
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public AdminReportObject(int id, int characterId, int playerId, string characterName, string message)
        {
            Id = id;
            CharacterId = characterId;
            PlayerId = playerId;
            CharacterName = characterName;
            Message = message;
            Time = DateTime.Now;
        }

    }
}