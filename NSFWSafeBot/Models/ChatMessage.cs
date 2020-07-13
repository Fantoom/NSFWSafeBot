using Telegram.Bot.Types;

namespace NSFWSafeBot.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public string Caption { get; set; }
        public PhotoSize Photo { get; set; }

        // ReSharper disable once UnusedMember.Global
        // Uses by LiteDbCollection
        public ChatMessage()
        {
        }

        public ChatMessage(Message message)
        {
            ChatId = message.Chat.Id;
            UserId = message.From.Id;
            MessageId = message.MessageId;
            Caption = message.Caption?.ToLower() ?? string.Empty;
            Photo = message.Photo?[0];
        }
    }
}