using Telegram.Bot.Types;

namespace NSFWSafeBot.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
}