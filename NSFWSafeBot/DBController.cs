using LiteDB;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace NSFWSafeBot
{
	static class DBController
	{
		public static LiteDatabase db { get; private set; }
		public static ILiteCollection<Message> Messages { get; private set; }

		public static void Init()
		{
			db = new LiteDatabase(Path.Combine(Directory.GetCurrentDirectory(), "messages.db"));
			Messages = db.GetCollection<Message>("messages");
		}

		public static BsonValue AddMessage(Message message)
		{
			return Messages.Insert(message);
		}
		public static bool DeleteMessageById(long chatId, int messageId)
		{
			var message = Messages.Query().ToList().Where(x => x.ChatId == chatId && x.MessageId == messageId).FirstOrDefault();
			return Messages.Delete(message.Id);
		}

		public static List<Message> GetMessagesList(int userId)
		{
			return Messages.Query().ToList().Where(x => x.UserId == userId).ToList();
		}

		public static async Task<List<Message>> GetMessagesListAsync(int userId)
		{
			return await Task.Run(() => GetMessagesList(userId));
		}

		public static async Task<BsonValue> AddMessageAsync(Message message)
		{
			return await Task.Run(() => AddMessage(message));
		}
		public static async Task<bool> DeleteMessageByIdAsync(long chatId, int messageId)
		{
			return await Task.Run(() => DeleteMessageById(chatId, messageId));
		}


	}
}
