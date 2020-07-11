using System;
using System.Collections.Generic;
using System.Text;

namespace NSFWSafeBot
{
	class Message
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public long ChatId { get; set; }
		public int MessageId { get; set; }
		public Telegram.Bot.Types.Message message { get; set; }
	}
}
