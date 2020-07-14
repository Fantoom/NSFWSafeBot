using LiteDB;
using NSFWSafeBot.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static List<ChatMessage> GetUserMessages(int userId, string query = null)
        {
            var messages = Messages.Find(msg => msg.UserId == userId);
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                messages = messages.Where(msg => msg.Message.Caption.ToLower().Contains(query));
            }

            return messages.ToList();
        }

        public static Task<List<ChatMessage>> GetUserMessagesAsync(int userId, string query = null)
        {
            return Task.Run(() => GetUserMessages(userId, query));
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