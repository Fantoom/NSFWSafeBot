using LiteDB;
using NSFWSafeBot.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NSFWSafeBot
{
    internal static class DBController
    {
        private const string DatabaseFileName = "messages.db";

        public static LiteDatabase Database { get; private set; }
        public static ILiteCollection<ChatMessage> Messages { get; private set; }

        static DBController()
        {
            string databasePath = Path.Combine(Directory.GetCurrentDirectory(), DatabaseFileName);
            Database = new LiteDatabase(databasePath);
            Messages = Database.GetCollection<ChatMessage>("messages");
        }

        public static BsonValue AddMessage(ChatMessage message)
        {
            return Messages.Insert(message);
        }

        public static bool DeleteMessageById(long chatId, int messageId)
        {
            var message = Messages.FindOne(msg => msg.ChatId == chatId && msg.MessageId == messageId);
            return Messages.Delete(message.Id);
        }

        public static IEnumerable<ChatMessage> GetUserMessages(int userId)
        {
            return Messages.Find(msg => msg.UserId == userId);
        }

        public static Task<IEnumerable<ChatMessage>> GetUserMessagesAsync(int userId)
        {
            return Task.Run(() => GetUserMessages(userId));
        }

        public static Task<BsonValue> AddMessageAsync(ChatMessage message)
        {
            return Task.Run(() => AddMessage(message));
        }

        public static Task<bool> DeleteMessageByIdAsync(long chatId, int messageId)
        {
            return Task.Run(() => DeleteMessageById(chatId, messageId));
        }
    }
}