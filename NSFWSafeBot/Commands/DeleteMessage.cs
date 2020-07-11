using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NSFWSafeBot.Commands
{
	class DeleteMessage : ICommand
	{

		public string CommandName => "delete";

		public async Task Execute(Telegram.Bot.Types.Message message)
		{
			if (message.ReplyToMessage != null)
				await DBController.DeleteMessageByIdAsync(message.Chat.Id, message.ReplyToMessage.MessageId);
			else
				await BotClient.botClient.SendTextMessageAsync(message.Chat.Id, "Send delete as reply to message you want to delete");

		}
	}
}
